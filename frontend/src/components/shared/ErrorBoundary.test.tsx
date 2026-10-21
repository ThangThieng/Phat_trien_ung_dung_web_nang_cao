import { fireEvent, render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import ErrorBoundary from './ErrorBoundary';

let shouldThrow = true;
function Flaky() {
  if (shouldThrow) throw new Error('boom: stack trace không được lộ ra');
  return <p>Nội dung đã tải</p>;
}

const renderWithQuery = (ui: React.ReactElement) =>
  render(<QueryClientProvider client={new QueryClient()}>{ui}</QueryClientProvider>);

describe('<ErrorBoundary />', () => {
  let consoleError: jest.SpyInstance;
  beforeEach(() => {
    shouldThrow = true;
    // React ghi lỗi render ra console — chặn để log kiểm thử gọn.
    consoleError = jest.spyOn(console, 'error').mockImplementation(() => {});
  });
  afterEach(() => consoleError.mockRestore());

  it('component con ném lỗi → hiện fallback thân thiện, không lộ thông điệp kỹ thuật', () => {
    renderWithQuery(
      <ErrorBoundary>
        <Flaky />
      </ErrorBoundary>,
    );
    expect(screen.getByRole('alert')).toBeInTheDocument();
    expect(screen.queryByText(/stack trace/)).not.toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Thử lại' })).toBeInTheDocument();
  });

  it('bấm "Thử lại" → render lại phần con và gọi onReset', () => {
    const onReset = jest.fn();
    renderWithQuery(
      <ErrorBoundary onReset={onReset}>
        <Flaky />
      </ErrorBoundary>,
    );
    shouldThrow = false;
    fireEvent.click(screen.getByRole('button', { name: 'Thử lại' }));
    expect(screen.getByText('Nội dung đã tải')).toBeInTheDocument();
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
    expect(onReset).toHaveBeenCalledTimes(1);
  });

  it('nhận fallback tùy biến', () => {
    renderWithQuery(
      <ErrorBoundary
        fallback={({ reset }) => (
          <button type="button" onClick={reset}>
            Tải lại khối
          </button>
        )}
      >
        <Flaky />
      </ErrorBoundary>,
    );
    expect(screen.getByRole('button', { name: 'Tải lại khối' })).toBeInTheDocument();
  });

  it('lỗi không làm hỏng phần nằm ngoài boundary', () => {
    renderWithQuery(
      <>
        <h1>Trang chủ</h1>
        <ErrorBoundary>
          <Flaky />
        </ErrorBoundary>
      </>,
    );
    expect(screen.getByRole('heading', { name: 'Trang chủ' })).toBeInTheDocument();
  });
});
