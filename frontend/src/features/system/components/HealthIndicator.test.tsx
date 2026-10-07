import type { ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { cleanup, render, screen, waitFor } from '@testing-library/react';
import HealthIndicator from './HealthIndicator';
import { HEALTH_POLL_INTERVAL_MS } from '../health';

function renderWithQuery(ui: ReactNode) {
  const client = new QueryClient();
  return render(<QueryClientProvider client={client}>{ui}</QueryClientProvider>);
}

function healthResponse(status: number, body: unknown): Response {
  return { ok: status < 400, status, json: async () => body } as Response;
}

describe('HealthIndicator', () => {
  const originalFetch = global.fetch;

  afterEach(() => {
    cleanup();
    global.fetch = originalFetch;
    jest.useRealTimers();
  });

  it.each([
    ['Healthy', 200, 'bg-green-500', 'Hoạt động bình thường'],
    ['Degraded', 200, 'bg-amber-400', 'Suy giảm'],
    ['Unhealthy', 503, 'bg-red-600', 'Gián đoạn'],
  ])('trạng thái %s → chấm %s', async (status, httpStatus, dotClass, label) => {
    global.fetch = jest.fn().mockResolvedValue(healthResponse(httpStatus, { status, entries: [] }));

    renderWithQuery(<HealthIndicator />);

    await waitFor(() =>
      expect(screen.getByTestId('health-dot')).toHaveAttribute('data-status', status),
    );
    expect(screen.getByTestId('health-dot')).toHaveClass(dotClass);
    expect(screen.getByRole('status')).toHaveTextContent(label);
    expect(global.fetch).toHaveBeenCalledWith(
      '/api/health',
      expect.objectContaining({ cache: 'no-store' }),
    );
  });

  it('máy chủ giao diện không phản hồi (lỗi mạng) → đỏ', async () => {
    global.fetch = jest.fn().mockRejectedValue(new TypeError('Failed to fetch'));

    renderWithQuery(<HealthIndicator />);

    await waitFor(() =>
      expect(screen.getByTestId('health-dot')).toHaveAttribute('data-status', 'Unhealthy'),
    );
  });

  it('poll lại sau mỗi chu kỳ và chuyển đỏ khi Redis chết', async () => {
    jest.useFakeTimers();
    global.fetch = jest
      .fn()
      .mockResolvedValueOnce(healthResponse(200, { status: 'Healthy', entries: [] }))
      .mockResolvedValueOnce(healthResponse(503, { status: 'Unhealthy', entries: [] }));

    renderWithQuery(<HealthIndicator />);
    await waitFor(() =>
      expect(screen.getByTestId('health-dot')).toHaveAttribute('data-status', 'Healthy'),
    );

    jest.advanceTimersByTime(HEALTH_POLL_INTERVAL_MS);

    await waitFor(() =>
      expect(screen.getByTestId('health-dot')).toHaveAttribute('data-status', 'Unhealthy'),
    );
    expect(global.fetch).toHaveBeenCalledTimes(2);
  });
});
