// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RollingInterval.cs" company="Hämmer Electronics">
// The project is licensed under the GNU GENERAL PUBLIC LICENSE, Version 3, 29 June 2007
// </copyright>
// <summary>
//   Defines the RollingInterval type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    /// <summary>
    /// Specifies the frequency at which the log file should roll.
    /// </summary>
    public enum RollingInterval
    {
        /// <summary>
        /// Roll every year. File names will have a four-digit year appended in the pattern <code>yyyy</code>.
        /// </summary>
        Year,

        /// <summary>
        /// Roll every calendar month. File names will have <code>yyyyMM</code> appended.
        /// </summary>
        Month,

        /// <summary>
        /// Roll every day. File names will have <code>yyyyMMdd</code> appended.
        /// </summary>
        Day,

        /// <summary>
        /// Roll every hour. File names will have <code>yyyyMMddHH</code> appended.
        /// </summary>
        Hour,

        /// <summary>
        /// Roll every minute. File names will have <code>yyyyMMddHHmm</code> appended.
        /// </summary>
        Minute
    }
}
