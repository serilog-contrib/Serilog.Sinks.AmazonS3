// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RollingLogFile.cs" company="Hämmer Electronics">
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
//   The rolling log file class to handle rolling log files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    using System;

    /// <summary>
    /// The rolling log file class to handle rolling log files.
    /// </summary>
    public class RollingLogFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RollingLogFile"/> class.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="sequenceNumber">The sequence number.</param>
        public RollingLogFile(string fileName, DateTime? dateTime, int? sequenceNumber)
        {
            this.FileName = fileName;
            this.DateTime = dateTime;
            this.SequenceNumber = sequenceNumber;
        }

        /// <summary>
        /// Gets the file name.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the date time.
        /// </summary>
        public DateTime? DateTime { get; }

        /// <summary>
        /// Gets the sequence number.
        /// </summary>
        public int? SequenceNumber { get; }
    }
}
