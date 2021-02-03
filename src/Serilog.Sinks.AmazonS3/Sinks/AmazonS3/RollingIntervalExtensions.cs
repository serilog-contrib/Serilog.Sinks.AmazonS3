// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RollingIntervalExtensions.cs" company="Hämmer Electronics">
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
//   This class contains extension methods for the <see cref="RollingInterval"/> enumeration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// This class contains extension methods for the <see cref="RollingInterval"/> enumeration.
    /// </summary>
    public static class RollingIntervalExtensions
    {
        /// <summary>
        /// Gets the format <see cref="string"/>.
        /// </summary>
        /// <param name="interval">The interval.</param>
        /// <returns>The format <see cref="string"/>.</returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1122:UseStringEmptyForEmptyStrings", Justification = "Reviewed. Suppression is OK here.")]
        public static string GetFormat(this RollingInterval interval)
        {
            switch (interval)
            {
                case RollingInterval.Infinite:
                    return "";
                case RollingInterval.Year:
                    return "yyyy";
                case RollingInterval.Month:
                    return "yyyyMM";
                case RollingInterval.Day:
                    return "yyyyMMdd";
                case RollingInterval.Hour:
                    return "yyyyMMddHH";
                case RollingInterval.Minute:
                    return "yyyyMMddHHmm";
                default:
                    throw new ArgumentException("Invalid rolling interval");
            }
        }

        /// <summary>
        /// Gets the current check point as <see cref="DateTime"/>.
        /// </summary>
        /// <param name="interval">The interval.</param>
        /// <param name="dateTime">The date time.</param>
        /// <returns>The current check point as <see cref="DateTime"/>.</returns>
        public static DateTime? GetCurrentCheckpoint(this RollingInterval interval, DateTime dateTime)
        {
            switch (interval)
            {
                case RollingInterval.Infinite:
                    return null;
                case RollingInterval.Year:
                    return new DateTime(dateTime.Year, 1, 1, 0, 0, 0, dateTime.Kind);
                case RollingInterval.Month:
                    return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);
                case RollingInterval.Day:
                    return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Kind);
                case RollingInterval.Hour:
                    return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, dateTime.Kind);
                case RollingInterval.Minute:
                    return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, dateTime.Kind);
                default:
                    throw new ArgumentException("Invalid rolling interval");
            }
        }

        /// <summary>
        /// Gets the next check point as <see cref="DateTime"/>.
        /// </summary>
        /// <param name="interval">The interval.</param>
        /// <param name="dateTime">The date time.</param>
        /// <returns>The next check point as <see cref="DateTime"/>.</returns>
        public static DateTime? GetNextCheckpoint(this RollingInterval interval, DateTime dateTime)
        {
            var current = GetCurrentCheckpoint(interval, dateTime);

            if (current is null)
            {
                return null;
            }

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (interval)
            {
                case RollingInterval.Year:
                    return current.Value.AddYears(1);
                case RollingInterval.Month:
                    return current.Value.AddMonths(1);
                case RollingInterval.Day:
                    return current.Value.AddDays(1);
                case RollingInterval.Hour:
                    return current.Value.AddHours(1);
                case RollingInterval.Minute:
                    return current.Value.AddMinutes(1);
                default:
                    throw new ArgumentException("Invalid rolling interval");
            }
        }
    }
}
