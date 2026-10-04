using System.Collections.Concurrent;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;

namespace CulinaryBlog.API.IntegrationTests.Infrastructure.Fakes;

/// <summary>
/// Thay <see cref="IBackgroundJobClient"/> thật trong integration test: ghi lại lời gọi thay vì đẩy job
/// vào storage. Lý do: BackgroundJobServer chạy song song khiến kết quả test không tất định (job có thể
/// chạy trước hoặc sau assertion), trong khi điều cần kiểm chứng ở tầng API chỉ là
/// "endpoint CÓ enqueue đúng job với đúng tham số" — bản thân WelcomeEmailJob đã chạy thật từ Buổi 2.
/// </summary>
public sealed class FakeBackgroundJobClient : IBackgroundJobClient
{
    private readonly ConcurrentQueue<EnqueuedJob> _jobs = new();

    public IReadOnlyCollection<EnqueuedJob> EnqueuedJobs => [.. _jobs];

    public string Create(Job job, IState state)
    {
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(state);

        var id = Guid.NewGuid().ToString("N");
        _jobs.Enqueue(new EnqueuedJob(id, job.Type, job.Method.Name, [.. job.Args], state.Name));
        return id;
    }

    public bool ChangeState(string jobId, IState state, string? expectedState) => true;

    public void Clear()
    {
        while (_jobs.TryDequeue(out _))
        {
            // Xả hàng đợi giữa các test.
        }
    }

    /// <summary>Có job nào gọi <typeparamref name="TJob"/>.<paramref name="methodName"/> với tham số đầu là <paramref name="firstArgument"/> hay không.</summary>
    public bool WasEnqueued<TJob>(string methodName, object? firstArgument = null) =>
        EnqueuedJobs.Any(j =>
            j.JobType == typeof(TJob)
            && string.Equals(j.MethodName, methodName, StringComparison.Ordinal)
            && (firstArgument is null || (j.Arguments.Count > 0 && Equals(j.Arguments[0], firstArgument))));

    public sealed record EnqueuedJob(string Id, Type JobType, string MethodName, IReadOnlyList<object?> Arguments, string StateName);
}
