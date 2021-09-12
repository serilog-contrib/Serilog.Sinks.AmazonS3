// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorMessageConstants.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   Constants to use in user-facing error messages.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Serilog.Sinks.AmazonS3
{
    /// <summary>
    /// Constants to use in user-facing error messages.
    /// </summary>
    internal static class ErrorMessageConstants
    {
        /// <summary>
        /// The <see cref="AmazonS3Options.ServiceUrl"/> provided is in invalid format.
        /// </summary>
        internal const string ServiceUrlInvalidFormat = "URL must be a valid absolute URL including the protocol.";
    }
}
