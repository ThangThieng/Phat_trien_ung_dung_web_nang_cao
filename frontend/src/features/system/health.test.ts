import { normalizeHealthReport, parseTimeSpanMs, unreachableHealth } from './health';

const CHECKED_AT = new Date('2026-10-07T03:00:00.000Z');

describe('parseTimeSpanMs', () => {
  it.each([
    ['00:00:00.0123456', 12.35],
    ['00:00:01', 1000],
    ['00:00:03.5', 3500],
    ['1.00:00:00', 86_400_000],
  ])('đổi TimeSpan %s của .NET sang %s ms', (value, expected) => {
    expect(parseTimeSpanMs(value)).toBe(expected);
  });

  it.each([undefined, null, 12, '', 'abc', '00:00'])(
    'trả null cho giá trị không phải TimeSpan: %p',
    (value) => {
      expect(parseTimeSpanMs(value)).toBeNull();
    },
  );
});

describe('normalizeHealthReport', () => {
  it('chuẩn hóa báo cáo HealthChecks.UI: thời gian theo ms, tag, mô tả và thứ tự cố định', () => {
    const report = normalizeHealthReport(
      {
        status: 'Degraded',
        totalDuration: '00:00:00.0300000',
        entries: {
          minio: {
            status: 'Degraded',
            duration: '00:00:03',
            tags: [],
            description: 'MinIO không phản hồi',
          },
          redis: { status: 'Healthy', duration: '00:00:00.0020000', tags: ['ready'] },
          database: { status: 'Healthy', duration: '00:00:00.0100000', tags: ['ready'], data: {} },
        },
      },
      CHECKED_AT,
    );

    expect(report).toEqual({
      status: 'Degraded',
      totalDurationMs: 30,
      checkedAt: '2026-10-07T03:00:00.000Z',
      apiReachable: true,
      entries: [
        { name: 'database', status: 'Healthy', durationMs: 10, description: null, tags: ['ready'] },
        { name: 'redis', status: 'Healthy', durationMs: 2, description: null, tags: ['ready'] },
        {
          name: 'minio',
          status: 'Degraded',
          durationMs: 3000,
          description: 'MinIO không phản hồi',
          tags: [],
        },
      ],
    });
  });

  it('coi trạng thái lạ là Unhealthy thay vì bỏ qua', () => {
    const report = normalizeHealthReport(
      { status: 'Unknown', entries: { redis: { status: 'Weird' } } },
      CHECKED_AT,
    );

    expect(report.status).toBe('Unhealthy');
    expect(report.entries[0]).toMatchObject({
      name: 'redis',
      status: 'Unhealthy',
      durationMs: null,
      tags: [],
    });
  });

  it('dữ liệu không phải object → báo API không đọc được', () => {
    const report = normalizeHealthReport('<html>502</html>', CHECKED_AT);

    expect(report.apiReachable).toBe(false);
    expect(report.status).toBe('Unhealthy');
  });
});

describe('unreachableHealth', () => {
  it('báo chính API là phụ thuộc đang hỏng, kèm lý do', () => {
    expect(unreachableHealth('Không kết nối được tới API.', CHECKED_AT)).toEqual({
      status: 'Unhealthy',
      totalDurationMs: null,
      checkedAt: '2026-10-07T03:00:00.000Z',
      apiReachable: false,
      entries: [
        {
          name: 'api',
          status: 'Unhealthy',
          durationMs: null,
          description: 'Không kết nối được tới API.',
          tags: ['ready'],
        },
      ],
    });
  });
});
