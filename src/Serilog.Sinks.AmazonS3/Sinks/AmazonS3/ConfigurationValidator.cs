// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationValidator.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
// Validators for configuration parameters.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    using System;

    /// <summary>
    /// Validators for configuration parameters.
    /// </summary>
    internal static class ConfigurationValidator
    {
        /// <summary>
        /// Validates the value provided for <see cref="AmazonS3Options.ServiceUrl"/> property.
        /// </summary>
        /// <param name="serviceUrl">The provided service url.</param>
        /// <returns>A value indicating whether the service url is valid or not.</returns>
        internal static bool ValidateServiceUrl(string serviceUrl) => Uri.TryCreate(serviceUrl, UriKind.Absolute, out _);
    }
}
