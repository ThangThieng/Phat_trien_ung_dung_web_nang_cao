/**
 * @jest-environment node
 */
import { GET } from './route';

jest.mock('@/lib/config', () => ({ getApiBaseUrl: () => 'http://api:8080/api/v1' }));

function jsonResponse(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  });
}

describe('GET /api/health', () => {
  const originalFetch = global.fetch;

  afterEach(() => {
    global.fetch = originalFetch;
  });

  it('gọi /health ở gốc API (ngoài /api/v1) và trả 200 + no-store khi Healthy', async () => {
    const fetchMock = jest.fn().mockResolvedValue(
      jsonResponse(200, {
        status: 'Healthy',
        totalDuration: '00:00:00.0100000',
        entries: { database: { status: 'Healthy', duration: '00:00:00.0050000', tags: ['ready'] } },
      }),
    );
    global.fetch = fetchMock;

    const response = await GET();

    expect(fetchMock).toHaveBeenCalledWith(
      'http://api:8080/health',
      expect.objectContaining({ cache: 'no-store' }),
    );
    expect(response.status).toBe(200);
    expect(response.headers.get('Cache-Control')).toBe('no-store');
    await expect(response.json()).resolves.toMatchObject({
      status: 'Healthy',
      apiReachable: true,
      entries: [{ name: 'database', durationMs: 5 }],
    });
  });

  it('Degraded (MinIO lỗi) vẫn là 200 — không phải sự cố khiến ngừng phục vụ', async () => {
    global.fetch = jest
      .fn()
      .mockResolvedValue(
        jsonResponse(200, {
          status: 'Degraded',
          entries: { minio: { status: 'Degraded', tags: [] } },
        }),
      );

    const response = await GET();

    expect(response.status).toBe(200);
    await expect(response.json()).resolves.toMatchObject({ status: 'Degraded' });
  });

  it('API trả 503 (Redis chết) → chuyển tiếp 503 kèm báo cáo chi tiết từng phụ thuộc', async () => {
    global.fetch = jest.fn().mockResolvedValue(
      jsonResponse(503, {
        status: 'Unhealthy',
        entries: {
          database: { status: 'Healthy', tags: ['ready'] },
          redis: {
            status: 'Unhealthy',
            tags: ['ready'],
            description: 'It was not possible to connect to the redis server(s).',
          },
        },
      }),
    );

    const response = await GET();

    expect(response.status).toBe(503);
    const body = await response.json();
    expect(body.apiReachable).toBe(true);
    expect(body.entries).toEqual([
      expect.objectContaining({ name: 'database', status: 'Healthy' }),
      expect.objectContaining({ name: 'redis', status: 'Unhealthy' }),
    ]);
  });

  it('không gọi được API → 503 với mục "api" Unhealthy thay vì ném lỗi', async () => {
    global.fetch = jest.fn().mockRejectedValue(new TypeError('fetch failed'));

    const response = await GET();

    expect(response.status).toBe(503);
    await expect(response.json()).resolves.toMatchObject({
      status: 'Unhealthy',
      apiReachable: false,
      entries: [{ name: 'api', description: 'Không kết nối được tới API.' }],
    });
  });

  it('API treo quá hạn → báo timeout', async () => {
    global.fetch = jest
      .fn()
      .mockRejectedValue(new DOMException('The operation timed out.', 'TimeoutError'));

    const body = await (await GET()).json();

    expect(body.entries[0].description).toBe('API không phản hồi trong 5 giây.');
  });
});
