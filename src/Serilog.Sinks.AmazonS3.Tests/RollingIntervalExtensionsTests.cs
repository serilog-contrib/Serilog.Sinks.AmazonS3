// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RollingIntervalExtensionsTests.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class contains the tests for the <see cref="RollingIntervalExtensions"/> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3.Tests;

/// <summary>
/// This class contains the tests for the <see cref="RollingIntervalExtensions"/> class.
/// </summary>
[TestClass]
public class RollingIntervalExtensionsTests
{
    /// <summary>
    /// The date all tests calculate their checkpoints from.
    /// </summary>
    private static readonly DateTime TestDate = new(2026, 8, 17, 13, 45, 30);

    /// <summary>
    /// This method is used to test the format string of each rolling interval.
    /// </summary>
    /// <param name="interval">The rolling interval.</param>
    /// <param name="expectedFormat">The expected format string.</param>
    [TestMethod]
    [DataRow(RollingInterval.Infinite, "")]
    [DataRow(RollingInterval.Year, "yyyy")]
    [DataRow(RollingInterval.Month, "yyyyMM")]
    [DataRow(RollingInterval.Day, "yyyyMMdd")]
    [DataRow(RollingInterval.Hour, "yyyyMMddHH")]
    [DataRow(RollingInterval.Minute, "yyyyMMddHHmm")]
    public void GetFormatReturnsTheFormatOfTheInterval(RollingInterval interval, string expectedFormat)
    {
        Assert.AreEqual(expectedFormat, interval.GetFormat());
    }

    /// <summary>
    /// This method is used to test the current checkpoint of each rolling interval.
    /// </summary>
    [TestMethod]
    public void GetCurrentCheckpointTruncatesTheDate()
    {
        Assert.IsNull(RollingInterval.Infinite.GetCurrentCheckpoint(TestDate));
        Assert.AreEqual(new DateTime(2026, 1, 1), RollingInterval.Year.GetCurrentCheckpoint(TestDate));
        Assert.AreEqual(new DateTime(2026, 8, 1), RollingInterval.Month.GetCurrentCheckpoint(TestDate));
        Assert.AreEqual(new DateTime(2026, 8, 17), RollingInterval.Day.GetCurrentCheckpoint(TestDate));
        Assert.AreEqual(new DateTime(2026, 8, 17, 13, 0, 0), RollingInterval.Hour.GetCurrentCheckpoint(TestDate));
        Assert.AreEqual(new DateTime(2026, 8, 17, 13, 45, 0), RollingInterval.Minute.GetCurrentCheckpoint(TestDate));
    }

    /// <summary>
    /// This method is used to test the next checkpoint of each rolling interval.
    /// </summary>
    [TestMethod]
    public void GetNextCheckpointAddsOneInterval()
    {
        Assert.IsNull(RollingInterval.Infinite.GetNextCheckpoint(TestDate));
        Assert.AreEqual(new DateTime(2027, 1, 1), RollingInterval.Year.GetNextCheckpoint(TestDate));
        Assert.AreEqual(new DateTime(2026, 9, 1), RollingInterval.Month.GetNextCheckpoint(TestDate));
        Assert.AreEqual(new DateTime(2026, 8, 18), RollingInterval.Day.GetNextCheckpoint(TestDate));
        Assert.AreEqual(new DateTime(2026, 8, 17, 14, 0, 0), RollingInterval.Hour.GetNextCheckpoint(TestDate));
        Assert.AreEqual(new DateTime(2026, 8, 17, 13, 46, 0), RollingInterval.Minute.GetNextCheckpoint(TestDate));
    }

    /// <summary>
    /// This method is used to test that an undefined rolling interval is refused.
    /// </summary>
    [TestMethod]
    public void AnUndefinedIntervalIsRefused()
    {
        var interval = (RollingInterval)99;
        Assert.ThrowsExactly<ArgumentException>(() => { _ = interval.GetFormat(); });
        Assert.ThrowsExactly<ArgumentException>(() => { _ = interval.GetCurrentCheckpoint(TestDate); });
        Assert.ThrowsExactly<ArgumentException>(() => { _ = interval.GetNextCheckpoint(TestDate); });
    }
}
