using System.IO;

namespace RestVsGraphQL.Middleware;

/// <summary>
/// A stream wrapper that tracks bytes written and optionally captures the response body.
/// When capture is enabled, it buffers the response for later retrieval.
/// When capture is disabled, it acts as a pass-through stream without overhead.
/// </summary>
public class ResponseCapturingStream : Stream
{
    private readonly Stream _innerStream;
    private readonly MemoryStream? _captureBuffer;
    private long _bytesWritten;

    public ResponseCapturingStream(Stream innerStream, bool captureResponse = false)
    {
        _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        _captureBuffer = captureResponse ? new MemoryStream() : null;
    }

    public long BytesWritten => _bytesWritten;

    public string? GetCapturedResponse()
    {
        if (_captureBuffer == null) return null;
        _captureBuffer.Position = 0;
        using var reader = new StreamReader(_captureBuffer, leaveOpen: true);
        return reader.ReadToEnd();
    }

    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => _innerStream.CanSeek;
    public override bool CanWrite => _innerStream.CanWrite;
    public override long Length => _innerStream.Length;

    public override long Position
    {
        get => _innerStream.Position;
        set => _innerStream.Position = value;
    }

    public override void Flush()
    {
        _innerStream.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return _innerStream.FlushAsync(cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _innerStream.Read(buffer, offset, count);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return _innerStream.ReadAsync(buffer, cancellationToken);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _innerStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _innerStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _innerStream.Write(buffer, offset, count);
        _captureBuffer?.Write(buffer, offset, count);
        _bytesWritten += count;
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        if (_captureBuffer != null)
            await _captureBuffer.WriteAsync(buffer, offset, count, cancellationToken);
        _bytesWritten += count;
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await _innerStream.WriteAsync(buffer, cancellationToken);
        if (_captureBuffer != null)
            await _captureBuffer.WriteAsync(buffer, cancellationToken);
        _bytesWritten += buffer.Length;
    }

    public override void WriteByte(byte value)
    {
        _innerStream.WriteByte(value);
        _captureBuffer?.WriteByte(value);
        _bytesWritten += 1;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _captureBuffer?.Dispose();
            // Don't dispose the inner stream - let the pipeline handle it
        }
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_captureBuffer != null)
            await _captureBuffer.DisposeAsync();
        // Don't dispose the inner stream - let the pipeline handle it
        await base.DisposeAsync();
    }
}
