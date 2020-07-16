// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFlushableFileSink.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   Defines the IFlushableFileSink type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    /// <summary>
    ///     This class is supported by (file-based) sinks that can be explicitly flushed.
    /// </summary>
    public interface IFlushableFileSink
    {
        /// <summary>   Flush buffered contents to the disk. </summary>
        void FlushToDisk();
    }
}