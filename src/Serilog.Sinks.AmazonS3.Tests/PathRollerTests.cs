// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PathRollerTests.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class contains the tests for the <see cref="PathRoller"/> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3.Tests;

/// <summary>
/// This class contains the tests for the <see cref="PathRoller"/> class.
/// </summary>
[TestClass]
public class PathRollerTests
{
    /// <summary>
    /// The date all tests roll their file names for.
    /// </summary>
    private static readonly DateTime TestDate = new(2026, 8, 17, 13, 45, 30);

    /// <summary>
    /// This method is used to test that the period is put between the file name and the extension.
    /// </summary>
    [TestMethod]
    public void GetLogFilePathAddsThePeriodToTheFileName()
    {
        var pathRoller = new PathRoller("log.txt", RollingInterval.Day);
        pathRoller.GetLogFilePath(TestDate, null, out var path);
        Assert.AreEqual("log20260817.txt", Path.GetFileName(path));
    }

    /// <summary>
    /// This method is used to test that the sequence number is appended with three digits.
    /// </summary>
    [TestMethod]
    public void GetLogFilePathAddsTheSequenceNumber()
    {
        var pathRoller = new PathRoller("log.txt", RollingInterval.Day);
        pathRoller.GetLogFilePath(TestDate, 3, out var path);
        Assert.AreEqual("log20260817_003.txt", Path.GetFileName(path));
    }

    /// <summary>
    /// This method is used to test that a rolling interval of <see cref="RollingInterval.Infinite"/> keeps the file name.
    /// </summary>
    [TestMethod]
    public void GetLogFilePathWithoutIntervalKeepsTheFileName()
    {
        var pathRoller = new PathRoller("log.txt", RollingInterval.Infinite);
        pathRoller.GetLogFilePath(TestDate, null, out var path);
        Assert.AreEqual("log.txt", Path.GetFileName(path));
    }

    /// <summary>
    /// This method is used to test that the log file directory is absolute.
    /// </summary>
    [TestMethod]
    public void LogFileDirectoryIsAbsolute()
    {
        var filePath = Path.Combine(Path.GetTempPath(), "log.txt");
        var pathRoller = new PathRoller(filePath, RollingInterval.Day);
        Assert.AreEqual(Path.GetFullPath(Path.GetDirectoryName(filePath)!), pathRoller.LogFileDirectory);
    }

    /// <summary>
    /// This method is used to test the directory search pattern.
    /// </summary>
    [TestMethod]
    public void DirectorySearchPatternIsBuiltFromTheFileName()
    {
        var pathRoller = new PathRoller("log.txt", RollingInterval.Day);
        Assert.AreEqual("log*.txt", pathRoller.DirectorySearchPattern);
    }

    /// <summary>
    /// This method is used to test that only the files belonging to the roller are selected.
    /// </summary>
    [TestMethod]
    public void SelectMatchesIgnoresForeignFiles()
    {
        var pathRoller = new PathRoller("log.txt", RollingInterval.Day);

        var fileNames = new List<string>
        {
            "log20260817.txt",
            "log20260817_003.txt",
            "other20260817.txt",
            "log20260817.log",
            "log.txt"
        };

        var matches = pathRoller.SelectMatches(fileNames).ToList();

        Assert.AreEqual(2, matches.Count);
        Assert.AreEqual(new DateTime(2026, 8, 17), matches[0].DateTime);
        Assert.IsNull(matches[0].SequenceNumber);
        Assert.AreEqual(new DateTime(2026, 8, 17), matches[1].DateTime);
        Assert.AreEqual(3, matches[1].SequenceNumber);
    }

    /// <summary>
    /// This method is used to test that a null path is refused.
    /// </summary>
    [TestMethod]
    public void PathRollerRefusesANullPath()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => { _ = new PathRoller(null!, RollingInterval.Day); });
    }
}
