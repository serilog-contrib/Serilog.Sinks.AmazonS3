// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PathRoller.cs" company="Hämmer Electronics">
// The project is double licensed under the MIT license and the Apache License, Version 2.0.
// This code is a partly modified source code of the original Serilog code.
// The original license is:
//
// --------------------------------------------------------------------------------------------------------------------
// Copyright 2013-2016 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// --------------------------------------------------------------------------------------------------------------------
//
// </copyright>
// <summary>
//   The path roller to get the correct file sequences.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The path roller to get the correct file sequences.
    /// </summary>
    public class PathRoller
    {
        /// <summary>
        /// The period match group.
        /// </summary>
        private const string PeriodMatchGroup = "period";

        /// <summary>
        /// The sequence number match group.
        /// </summary>
        private const string SequenceNumberMatchGroup = "sequence";

        /// <summary>
        /// The file name prefix.
        /// </summary>
        private readonly string fileNamePrefix;

        /// <summary>
        /// The file name suffix.
        /// </summary>
        private readonly string fileNameSuffix;

        /// <summary>
        /// The file name matcher.
        /// </summary>
        private readonly Regex fileNameMatcher;

        /// <summary>
        /// The interval.
        /// </summary>
        private readonly RollingInterval interval;

        /// <summary>
        /// The period format.
        /// </summary>
        private readonly string periodFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathRoller"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="interval">The interval.</param>
        public PathRoller(string path, RollingInterval interval)
        {
            if (path is null)
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
            this.fileNamePrefix = Path.GetFileNameWithoutExtension(path);
            this.fileNameSuffix = Path.GetExtension(path);
            this.fileNameMatcher = new Regex(
                "^" +
                Regex.Escape(this.fileNamePrefix) +
                "(?<" + PeriodMatchGroup + ">\\d{" + this.periodFormat.Length + "})" +
                "(?<" + SequenceNumberMatchGroup + ">_[0-9]{3,}){0,1}" +
                Regex.Escape(this.fileNameSuffix) +
                "$",
                RegexOptions.Compiled);

            this.DirectorySearchPattern = $"{this.fileNamePrefix}*{this.fileNameSuffix}";
        }

        /// <summary>
        /// Gets the log file directory.
        /// </summary>
        public string LogFileDirectory { get; }

        /// <summary>
        /// Gets the directory search pattern.
        /// </summary>
        public string DirectorySearchPattern { get; }

        /// <summary>
        /// Gets the log file path.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="sequenceNumber">The sequence number.</param>
        /// <param name="path">The path.</param>
        public void GetLogFilePath(DateTime date, int? sequenceNumber, out string path)
        {
            var currentCheckpoint = this.GetCurrentCheckpoint(date);

            var tok = currentCheckpoint?.ToString(this.periodFormat, CultureInfo.InvariantCulture) ?? string.Empty;

            if (sequenceNumber != null)
            {
                tok += "_" + sequenceNumber.Value.ToString("000", CultureInfo.InvariantCulture);
            }

            path = Path.Combine(this.LogFileDirectory, this.fileNamePrefix + tok + this.fileNameSuffix);
        }

        /// <summary>
        /// Selects the matches.
        /// </summary>
        /// <param name="fileNames">The file names.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="RollingLogFile"/>s.</returns>
        public IEnumerable<RollingLogFile> SelectMatches(IEnumerable<string> fileNames)
        {
            foreach (var fileName in fileNames)
            {
                var match = this.fileNameMatcher.Match(fileName);

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

                yield return new RollingLogFile(fileName, period, inc);
            }
        }

        /// <summary>
        /// Gets the current check point as <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>The current check point as <see cref="DateTime"/>.</returns>
        public DateTime? GetCurrentCheckpoint(DateTime dateTime) => this.interval.GetCurrentCheckpoint(dateTime);

        /// <summary>
        /// Gets the next check point as <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>The next check point as <see cref="DateTime"/>.</returns>
        public DateTime? GetNextCheckpoint(DateTime dateTime) => this.interval.GetNextCheckpoint(dateTime);
    }
}
