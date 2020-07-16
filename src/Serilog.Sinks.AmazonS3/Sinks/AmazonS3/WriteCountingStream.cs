// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WriteCountingStream.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   Defines the WriteCountingStream type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    using System;
    using System.IO;

    /// <summary>   This class is used to provide a write counting stream. </summary>
    /// <seealso cref="T:System.IO.Stream" />
    public class WriteCountingStream : Stream
    {
        /// <summary>   The stream. </summary>
        private readonly Stream stream;

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:Serilog.Sinks.AmazonS3.WriteCountingStream" /> class.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException">   The stream is null. </exception>
        /// <param name="stream">   The stream. </param>
        /// <inheritdoc cref="Stream" />
        public WriteCountingStream(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.CountedLength = stream.Length;
        }

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether the current stream
        ///     supports reading.
        /// </summary>
        /// <inheritdoc cref="Stream" />
        public override bool CanRead => false;

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether the current stream
        ///     supports seeking.
        /// </summary>
        /// <inheritdoc cref="Stream" />
        public override bool CanSeek => this.stream.CanSeek;

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether the current stream
        ///     supports writing.
        /// </summary>
        /// <inheritdoc cref="Stream" />
        public override bool CanWrite => true;

        /// <summary>Gets the length of the counted value. </summary>
        public long CountedLength { get; private set; }

        /// <summary>
        ///     When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <inheritdoc cref="Stream" />
        public override long Length => this.stream.Length;

        /// <summary>
        ///     When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <exception cref="NotSupportedException">    . </exception>
        /// <inheritdoc cref="Stream" />
        public override long Position
        {
            get => this.stream.Position;
            set => throw new NotSupportedException();
        }

        /// <summary>
        ///     When overridden in a derived class, clears all buffers for this stream and causes any
        ///     buffered data to be written to the underlying device.
        /// </summary>
        /// <inheritdoc cref="Stream" />
        public override void Flush()
        {
            this.stream.Flush();
        }

        /// <summary>
        ///     When overridden in a derived class, reads a sequence of bytes from the current stream and
        ///     advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <exception cref="NotSupportedException">    . </exception>
        /// <param name="buffer">
        ///     An array of bytes. When this method returns, the buffer contains the
        ///     specified byte array with the values between
        ///     <paramref name="offset" /> and (<paramref name="offset" /> +
        ///     <paramref name="count" /> - 1) replaced by the bytes read from the
        ///     current source.
        /// </param>
        /// <param name="offset">
        ///     The zero-based byte offset in <paramref name="buffer" /> at which to
        ///     begin storing the data read from the current stream.
        /// </param>
        /// <param name="count">    The maximum number of bytes to be read from the current stream. </param>
        /// <returns>
        ///     The total number of bytes read into the buffer. This can be less than the number of bytes
        ///     requested if that many bytes are not currently available, or zero (0) if the end of the
        ///     stream has been reached.
        /// </returns>
        /// <inheritdoc cref="Stream" />
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Seek operations are not available through
        ///     `{nameof(WriteCountingStream)}
        /// </exception>
        /// <param name="offset">   A byte offset relative to the <paramref name="origin" /> parameter. </param>
        /// <param name="origin">
        ///     A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the
        ///     reference point used to obtain the new position.
        /// </param>
        /// <returns>   The new position within the current stream. </returns>
        /// <inheritdoc cref="Stream" />
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException(
                $"Seek operations are not available through `{nameof(WriteCountingStream)}`.");
        }

        /// <summary>   When overridden in a derived class, sets the length of the current stream. </summary>
        /// <exception cref="NotSupportedException">    . </exception>
        /// <param name="value">    The desired length of the current stream in bytes. </param>
        /// <inheritdoc cref="Stream" />
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     When overridden in a derived class, writes a sequence of bytes to the current stream and
        ///     advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">
        ///     An array of bytes. This method copies <paramref name="count" /> bytes
        ///     from
        ///     <paramref name="buffer" /> to the current stream.
        /// </param>
        /// <param name="offset">
        ///     The zero-based byte offset in <paramref name="buffer" /> at which to
        ///     begin copying bytes to the current stream.
        /// </param>
        /// <param name="count">    The number of bytes to be written to the current stream. </param>
        /// <inheritdoc cref="Stream" />
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.stream.Write(buffer, offset, count);
            this.CountedLength += count;
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the <see cref="T:Stream" /> and optionally
        ///     releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to
        ///     release only unmanaged resources.
        /// </param>
        /// <inheritdoc cref="Stream" />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.stream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}