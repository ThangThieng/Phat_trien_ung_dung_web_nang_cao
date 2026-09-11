namespace CulinaryBlog.Application.Common.Files;

/// <summary>
/// Stream chỉ-đọc ghép phần header đã đọc (để kiểm tra magic bytes) với phần còn lại của stream gốc,
/// giúp upload không phải buffer toàn bộ file vào memory.
/// </summary>
public sealed class PrefixedReadStream(ReadOnlyMemory<byte> prefix, Stream inner, long totalLength) : Stream
{
    private int _prefixPosition;
    private long _position;

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => totalLength;

    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

    public override int Read(Span<byte> buffer)
    {
        int read;
        if (_prefixPosition < prefix.Length)
        {
            read = Math.Min(buffer.Length, prefix.Length - _prefixPosition);
            prefix.Span.Slice(_prefixPosition, read).CopyTo(buffer);
            _prefixPosition += read;
        }
        else
        {
            read = inner.Read(buffer);
        }

        _position += read;
        return read;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        int read;
        if (_prefixPosition < prefix.Length)
        {
            read = Math.Min(buffer.Length, prefix.Length - _prefixPosition);
            prefix.Slice(_prefixPosition, read).CopyTo(buffer);
            _prefixPosition += read;
        }
        else
        {
            read = await inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        _position += read;
        return read;
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
