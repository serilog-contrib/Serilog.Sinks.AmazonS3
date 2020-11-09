// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileInformation.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   The file information.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    using System.IO;

    /// <summary>
    /// The file information.
    /// </summary>
    public class FileInformation
    {
        /// <summary>
        /// Gets or sets the output stream writer.
        /// </summary>
        public StreamWriter OutputWriter { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; }
    }
}
