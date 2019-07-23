// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFlushableFileSink.cs" company="Hämmer Electronics">
// The project is licensed under the GNU GENERAL PUBLIC LICENSE, Version 3, 29 June 2007
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
