// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AmazonS3Options.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
// This class is the main options class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    using System;
    using System.Text;

    using Amazon;
    using Amazon.S3;

    using Serilog.Formatting;

    /// <summary>
    /// This class is the main options class.
    /// </summary>
    public class AmazonS3Options
    {
        /// <summary>
        /// Gets or sets the Amazon S3 client.
        /// </summary>
        public AmazonS3Client AmazonS3Client { get; set; }

        /// <summary>
        /// Gets or sets the AWS access key identifier.
        /// </summary>
        public string AwsAccessKeyId { get; set; }

        /// <summary>
        /// Gets or sets the AWS secret access key.
        /// </summary>
        public string AwsSecretAccessKey { get; set; }

        /// <summary>
        /// Gets or sets the Amazon S3 bucket name.
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// Gets or sets the path where local files are stored.
        /// </summary>
        public string BucketPath { get; set; }

        /// <summary>
        /// Gets or sets the encoding.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets the Amazon S3 key endpoint.
        /// </summary>
        public RegionEndpoint Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the local path where the files are stored.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the text formatter.
        /// </summary>
        public ITextFormatter Formatter { get; set; }

        /// <summary>
        /// Gets or sets the failure callback.
        /// </summary>
        public Action<Exception> FailureCallback { get; set; }

        /// <summary>
        /// Gets or sets the Amazon S3 service url.
        /// </summary>
        public string ServiceUrl { get; set; } = "s3.amazonaws.com";

        /// <summary>
        /// Gets or sets the path roller.
        /// </summary>
        public PathRoller PathRoller { get; set; }

        /// <summary>
        /// Gets or sets the next checkpoint.
        /// </summary>
        public DateTime? NextCheckpoint { get; set; }

        /// <summary>
        /// Gets or sets the current file sequence number.
        /// </summary>
        public int? CurrentFileSequence { get; set; }

        /// <summary>
        /// Gets or sets the rolling interval.
        /// </summary>
        public RollingInterval RollingInterval { get; set; }

        /// <summary>
        /// Gets or sets the output template.
        /// </summary>
        public string OutputTemplate { get; set; }

        /// <summary>
        /// Gets or sets the format provider.
        /// </summary>
        public IFormatProvider FormatProvider { get; set; }
    }
}