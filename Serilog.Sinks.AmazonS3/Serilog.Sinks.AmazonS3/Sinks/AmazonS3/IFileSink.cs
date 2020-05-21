// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileSink.cs" company="Haemmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   Defines the IFileSink type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    using Serilog.Core;
    using Serilog.Events;

    /// <summary>
    ///     Exists only for the convenience of <see cref="RollingFileSink" />, which switches
    ///     implementations based on sharing. Would refactor, but preserving backwards compatibility.
    /// </summary>
    /// ###
    /// <inheritdoc cref="IFlushableFileSink" />
    public interface IFileSink : ILogEventSink, IFlushableFileSink
    {
        /// <summary>   Emits the <see cref="LogEvent" /> or overflows. </summary>
        /// <param name="logEvent"> The log event. </param>
        /// <returns>   A <see cref="bool" /> indicating whether the emitting was a success or not. </returns>
        bool EmitOrOverflow(LogEvent logEvent);
    }
}