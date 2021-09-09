using Serilog.Sinks.AmazonS3;
using System;

namespace Serilog
{
    /// <summary>
    /// Validators for configuration parameters. 
    /// </summary>
    internal static class S3ConfigurationValidator
    {
        /// <summary>
        /// Validate the value provided for <see cref="AmazonS3Options.ServiceUrl"/> property.
        /// </summary>
        /// <param name="serviceUrl"></param>
        /// <returns></returns>
        internal static bool ValidateServiceUrl(string serviceUrl)
            => Uri.TryCreate(serviceUrl, UriKind.Absolute, out _);
    }
}
