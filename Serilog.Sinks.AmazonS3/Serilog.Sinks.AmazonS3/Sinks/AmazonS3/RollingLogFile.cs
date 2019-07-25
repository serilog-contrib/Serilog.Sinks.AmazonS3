// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RollingLogFile.cs" company="Hämmer Electronics">
// The project is licensed under the GNU GENERAL PUBLIC LICENSE, Version 3, 29 June 2007
// </copyright>
// <summary>
//   Defines the RollingLogFile type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Serilog.Sinks.AmazonS3
{
    /// <summary>   A class that represents a rolling log file internally. </summary>
    public class RollingLogFile
    {
        /// <summary>   Initializes a new instance of the <see cref="RollingLogFile" /> class. </summary>
        /// <param name="filename">         The filename. </param>
        /// <param name="dateTime">         The date time. </param>
        /// <param name="sequenceNumber">   The sequence number. </param>
        public RollingLogFile(string filename, DateTime? dateTime, int? sequenceNumber)
        {
            Filename = filename;
            DateTime = dateTime;
            SequenceNumber = sequenceNumber;
        }

        /// <summary>   Gets the filename. </summary>
        /// <value> The filename. </value>

        public string Filename { get; }

        /// <summary>   Gets the date time. </summary>
        /// <value> The date time. </value>

        public DateTime? DateTime { get; }

        /// <summary>   Gets the sequence number. </summary>
        /// <value> The sequence number. </value>

        public int? SequenceNumber { get; }
    }
}