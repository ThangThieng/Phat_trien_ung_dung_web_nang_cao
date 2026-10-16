using System.Text;

namespace CulinaryBlog.API.IntegrationTests.Infrastructure;

/// <summary>
/// Byte ảnh dựng tay cho test magic bytes (NFR-SEC-004 / CONS-007). Không dùng file ảnh thật trong repo:
/// thứ cần kiểm chứng là 12 byte đầu, nên nội dung phía sau header không quan trọng — mà một file nhị phân
/// nằm trong git thì không ai đọc được diff khi nó thay đổi.
/// </summary>
public static class TestImages
{
    public static byte[] Png(int payloadBytes = 128) =>
        Build([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], payloadBytes);

    public static byte[] Jpeg(int payloadBytes = 128) =>
        Build([0xFF, 0xD8, 0xFF, 0xE0], payloadBytes);

    public static byte[] Webp(int payloadBytes = 128) =>
        Build([.. "RIFF"u8, 0x00, 0x00, 0x00, 0x00, .. "WEBP"u8], payloadBytes);

    /// <summary>
    /// File mã nguồn đội lốt ảnh: Content-Type khai báo image/png nhưng magic bytes là văn bản.
    /// Nội dung để vô hại có chủ đích — chuỗi webshell thật nằm trong repo sẽ bị antivirus của máy dev
    /// cách ly và làm gãy build, trong khi phép thử vẫn y hệt: header không phải ảnh thì phải bị từ chối.
    /// </summary>
    public static byte[] NotAnImage() =>
        Encoding.UTF8.GetBytes("#!/bin/sh\necho khong-phai-anh\n");

    private static byte[] Build(ReadOnlySpan<byte> header, int payloadBytes)
    {
        var buffer = new byte[header.Length + payloadBytes];
        header.CopyTo(buffer);

        for (var i = header.Length; i < buffer.Length; i++)
        {
            buffer[i] = (byte)(i % 251);
        }

        return buffer;
    }
}
