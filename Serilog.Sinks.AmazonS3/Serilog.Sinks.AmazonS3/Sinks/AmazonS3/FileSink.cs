// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSink.cs" company="Hämmer Electronics">
// The project is licensed under the GNU GENERAL PUBLIC LICENSE, Version 3, 29 June 2007
// </copyright>
// <summary>
//   Defines the FileSink type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    using System;
    using System.IO;
    using System.Text;

    using Serilog.Events;
    using Serilog.Formatting;

    /// <inheritdoc cref="IFileSink" />
    /// <inheritdoc cref="IDisposable" />
    /// <summary>
    ///     This class enables writing log events to a disk file.
    /// </summary>
    public class FileSink : IFileSink, IDisposable
    {
        /// <summary>
        ///     The buffered content.
        /// </summary>
        private readonly bool buffered;

        /// <summary>
        ///     The counting stream wrapper.
        /// </summary>
        private readonly WriteCountingStream countingStreamWrapper;

        /// <summary>
        ///     The file size limit bytes.
        /// </summary>
        private readonly long? fileSizeLimitBytes;

        /// <summary>
        ///     The output <see cref="TextWriter" />.
        /// </summary>
        private readonly TextWriter output;

        /// <summary>
        ///     The synchronize root.
        /// </summary>
        private readonly object syncRoot = new object();

        /// <summary>
        ///     The text formatter.
        /// </summary>
        private readonly ITextFormatter textFormatter;

        /// <summary>
        ///     The underlying <see cref="FileStream" />.
        /// </summary>
        private readonly FileStream underlyingStream;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileSink" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="textFormatter">The text formatter.</param>
        /// <param name="fileSizeLimitBytes">The file size limit bytes.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="buffered">if set to <c>true</c> [buffered].</param>
        /// <param name="hooks">The hooks.</param>
        /// <exception cref="ArgumentNullException">
        ///     path
        ///     or
        ///     textFormatter
        /// </exception>
        /// <exception cref="ArgumentException">Negative value provided; file size limit must be non-negative.</exception>
        /// <exception cref="InvalidOperationException">The file lifecycle hook FileLifecycleHooks.OnFileOpened.</exception>
        public FileSink(
            string path,
            ITextFormatter textFormatter,
            long? fileSizeLimitBytes,
            Encoding encoding,
            bool buffered,
            FileLifecycleHooks hooks)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (fileSizeLimitBytes.HasValue && fileSizeLimitBytes < 0)
            {
                throw new ArgumentException("Negative value provided; file size limit must be non-negative.");
            }

            this.textFormatter = textFormatter ?? throw new ArgumentNullException(nameof(textFormatter));
            this.fileSizeLimitBytes = fileSizeLimitBytes;
            this.buffered = buffered;

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Stream outputStream = this.underlyingStream = File.Open(
                                      path,
                                      FileMode.Append,
                                      FileAccess.Write,
                                      FileShare.Read);
            if (this.fileSizeLimitBytes != null)
            {
                outputStream = this.countingStreamWrapper = new WriteCountingStream(this.underlyingStream);
            }

            // Parameter reassignment.
            encoding = encoding ?? new UTF8Encoding(false);

            if (hooks != null)
            {
                outputStream = hooks.OnFileOpened(outputStream, encoding) ?? throw new InvalidOperationException(
                                   $"The file lifecycle hook `{nameof(FileLifecycleHooks.OnFileOpened)}(...)` returned `null`.");
            }

            this.output = new StreamWriter(outputStream, encoding);
        }

        /// <inheritdoc cref="IFileSink" />
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            lock (this.syncRoot)
            {
                this.output.Dispose();
            }
        }

        /// <inheritdoc cref="IFileSink" />
        /// <summary>
        ///     Emit the provided log event to the sink.
        /// </summary>
        /// <param name="logEvent">The log event to write.</param>
        public void Emit(LogEvent logEvent)
        {
            ((IFileSink)this).EmitOrOverflow(logEvent);
        }

        /// <inheritdoc cref="IFileSink" />
        /// <summary>
        ///     Flush buffered contents to the disk.
        /// </summary>
        public void FlushToDisk()
        {
            lock (this.syncRoot)
            {
                this.output.Flush();
                this.underlyingStream.Flush(true);
            }
        }

        /// <inheritdoc cref="IFileSink" />
        /// <summary>
        ///     Emits the <see cref="LogEvent" /> or overflows.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <returns>
        ///     A <see cref="bool" /> indicating whether the emitting was a success or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">logEvent</exception>
        public bool EmitOrOverflow(LogEvent logEvent)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            lock (this.syncRoot)
            {
                if (this.fileSizeLimitBytes != null)
                {
                    if (this.countingStreamWrapper.CountedLength >= this.fileSizeLimitBytes.Value)
                    {
                        return false;
                    }
                }

                this.textFormatter.Format(logEvent, this.output);
                if (!this.buffered)
                {
                    this.output.Flush();
                }

                return true;
            }
        }
    }
}