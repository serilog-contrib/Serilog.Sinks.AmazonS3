// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PathRoller.cs" company="Hämmer Electronics">
// The project is licensed under the GNU GENERAL PUBLIC LICENSE, Version 3, 29 June 2007
// </copyright>
// <summary>
//   Defines the PathRoller type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>   The class to apply the rolling path scenarios. </summary>
    public class PathRoller
    {
        /// <summary>   The period match group. </summary>
        private const string PeriodMatchGroup = "period";

        /// <summary>   The sequence number match group. </summary>
        private const string SequenceNumberMatchGroup = "sequence";

        /// <summary>   The filename matcher. </summary>
        private readonly Regex filenameMatcher;

        /// <summary>   The filename prefix. </summary>
        private readonly string filenamePrefix;

        /// <summary>   The filename suffix. </summary>
        private readonly string filenameSuffix;

        /// <summary>   The rolling interval. </summary>
        private readonly RollingInterval interval;

        /// <summary>   The period format. </summary>
        private readonly string periodFormat;

        /// <summary>   Initializes a new instance of the <see cref="PathRoller" /> class. </summary>
        /// <exception cref="ArgumentNullException">
        ///     An <see cref="ArgumentNullException" /> thrown
        ///     when the path is null.
        /// </exception>
        /// <param name="path">         The path. </param>
        /// <param name="interval">     The interval. </param>
        public PathRoller(string path, RollingInterval interval)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.interval = interval;
            this.periodFormat = interval.GetFormat();

            var pathDirectory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(pathDirectory))
            {
                pathDirectory = Directory.GetCurrentDirectory();
            }

            this.LogFileDirectory = Path.GetFullPath(pathDirectory);
            this.filenamePrefix = Path.GetFileNameWithoutExtension(path);
            this.filenameSuffix = Path.GetExtension(path);
            this.filenameMatcher = new Regex(
                "^" + Regex.Escape(this.filenamePrefix) + "(?<" + PeriodMatchGroup + ">\\d{" + this.periodFormat.Length
                + "})" + "(?<" + SequenceNumberMatchGroup + ">_[0-9]{3,}){0,1}" + Regex.Escape(this.filenameSuffix)
                + "$");

            this.DirectorySearchPattern = $"{this.filenamePrefix}*{this.filenameSuffix}";
        }

        /// <summary>   Gets the directory search pattern. </summary>
        /// <value> The directory search pattern. </value>

        public string DirectorySearchPattern { get; }

        /// <summary>   Gets the log file directory. </summary>
        /// <value> The log file directory. </value>

        public string LogFileDirectory { get; }

        /// <summary>   Gets the current checkpoint. </summary>
        /// <param name="instant">  The instant. </param>
        /// <returns>   A <see cref="DateTime" /> value that gives the current checkpoint. </returns>
        public DateTime? GetCurrentCheckpoint(DateTime instant)
        {
            return this.interval.GetCurrentCheckpoint(instant);
        }

        /// <summary>   Gets the log file path. </summary>
        /// <param name="date">             The date. </param>
        /// <param name="sequenceNumber">   The sequence number. </param>
        /// <param name="path">             [out] The path. </param>
        public void GetLogFilePath(DateTime date, int? sequenceNumber, out string path)
        {
            var currentCheckpoint = this.GetCurrentCheckpoint(date);

            var tok = currentCheckpoint?.ToString(this.periodFormat, CultureInfo.InvariantCulture) ?? string.Empty;

            if (sequenceNumber != null)
            {
                tok += "_" + sequenceNumber.Value.ToString("000", CultureInfo.InvariantCulture);
            }

            path = Path.Combine(this.LogFileDirectory, this.filenamePrefix + tok + this.filenameSuffix);
        }

        /// <summary>   Gets the next checkpoint. </summary>
        /// <param name="instant">  The instant. </param>
        /// <returns>   A <see cref="DateTime" /> value that gives the next checkpoint. </returns>
        public DateTime? GetNextCheckpoint(DateTime instant)
        {
            return this.interval.GetNextCheckpoint(instant);
        }

        /// <summary>   Selects the matches. </summary>
        /// <param name="fileNames">    The file names. </param>
        /// <returns>   An <see cref="IEnumerable{T}" /> of <see cref="RollingLogFile" />s. </returns>
        public IEnumerable<RollingLogFile> SelectMatches(IEnumerable<string> fileNames)
        {
            foreach (var filename in fileNames)
            {
                var match = this.filenameMatcher.Match(filename);
                if (!match.Success)
                {
                    continue;
                }

                int? inc = null;
                var incGroup = match.Groups[SequenceNumberMatchGroup];
                if (incGroup.Captures.Count != 0)
                {
                    var incPart = incGroup.Captures[0].Value.Substring(1);
                    inc = int.Parse(incPart, CultureInfo.InvariantCulture);
                }

                DateTime? period = null;
                var periodGroup = match.Groups[PeriodMatchGroup];
                if (periodGroup.Captures.Count != 0)
                {
                    var dateTimePart = periodGroup.Captures[0].Value;
                    if (DateTime.TryParseExact(
                        dateTimePart,
                        this.periodFormat,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var dateTime))
                    {
                        period = dateTime;
                    }
                }

                yield return new RollingLogFile(filename, period, inc);
            }
        }
    }
}