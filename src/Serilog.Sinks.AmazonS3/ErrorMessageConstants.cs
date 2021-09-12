using Serilog.Sinks.AmazonS3;

namespace Serilog
{
    /// <summary>
    /// Constants to use in user-facing error messages.
    /// </summary>
    internal static class ErrorMessageConstants
    {
        /// <summary>
        /// <see cref="AmazonS3Options.ServiceUrl"/> provided is in invalid format.
        /// </summary>
        internal const string ServiceUrlInvalidFormat = "URL must be valid absolute URL including protocol.";
    }
}
