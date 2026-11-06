using System;
using Xunit;

namespace Bergdahl.NewsbinPro.Client.Tests;

// Lines marked "captured" come from Newsbin 6.91RC4 (subjects, posters and paths anonymised). The others follow the
// formats the 2014 library parsed and haven't been checked against a live server yet.
public class ModelTests
{
    private static readonly DateTime Posted = new DateTime(2014, 11, 1, 12, 0, 0, DateTimeKind.Utc);
    private const string PostedUnix = "1414843200";

    [Fact]
    public void GroupFileSet_Parses()
    {
        var set = new GroupFileSet($"7\tSome.Subject\t123456\t{PostedUnix}\tposter@example.com\t12\t3\t0");
        Assert.Equal(7, set.Id);
        Assert.Equal("Some.Subject", set.Subject);
        Assert.Equal(123456, set.FileSize);
        Assert.Equal(Posted, set.TimeStamp);
        Assert.Equal(DateTimeKind.Utc, set.TimeStamp.Kind);
        Assert.Equal("poster@example.com", set.Poster);
        Assert.Equal(12, set.FileCount);
        Assert.Equal(3, set.ParCount);
        Assert.True(set.IsOld);
    }

    [Fact]
    public void GroupFileSet_Parses_Captured()
    {
        // LOADED LIST after SEARCH INTERNET; the first field is the 1-based position.
        var set = new GroupFileSet(
            "1\tSome Course [092/104] - \"73. Guest Os Recovery - Linux.mp4\" yEnc  21385772 (1/1)\t22212510\t1790085552\tSender <sender@example.com>\t0\t0\t1");
        Assert.Equal(1, set.Id);
        Assert.Equal(22212510, set.FileSize);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1790085552).UtcDateTime, set.TimeStamp);
        Assert.Equal("Sender <sender@example.com>", set.Poster);
        Assert.Equal(0, set.FileCount);
        Assert.Equal(0, set.ParCount);
        Assert.False(set.IsOld); // Meaning of the last field ("1" here) is still unverified.
    }

    [Fact]
    public void GroupFileSet_TooFewFields_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => new GroupFileSet("7\tSubject"));
    }

    [Fact]
    public void FilesFileSet_Parses_Captured()
    {
        var set = new FilesFileSet(
            "14815\t<DL> 63f51b12eb72f0cc3514dcaa5f74f233.txt\t73\t1789324240\tposter <abc@example.com>\tC:\\NBPro\\Download\\Some.Release\\63f51b12eb72f0cc3514dcaa5f74f233.txt");
        Assert.Equal(14815, set.Id);
        Assert.Equal("<DL> 63f51b12eb72f0cc3514dcaa5f74f233.txt", set.Subject);
        Assert.Equal(73, set.FileSize);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1789324240).UtcDateTime, set.TimeStamp);
        Assert.Equal("poster <abc@example.com>", set.Poster);
        Assert.Equal(@"C:\NBPro\Download\Some.Release\63f51b12eb72f0cc3514dcaa5f74f233.txt", set.FileName);
    }

    [Fact]
    public void FilesFileSet_UncPath_Captured()
    {
        var set = new FilesFileSet(
            "16969\t<Decode> Some.Release.nzb [2 of 2]\t380270\t1793917732\tposter@example.com\t\\\\media.example.local\\upload\\Some.Release\\thumbnail.jpg");
        Assert.Equal(380270, set.FileSize);
        Assert.Equal(@"\\media.example.local\upload\Some.Release\thumbnail.jpg", set.FileName);
    }

    [Fact]
    public void WishFileSet_Parses()
    {
        var set = new WishFileSet($"5\tSubject\t1000\t{PostedUnix}\tposter\tD:\\Downloads\t\t\t[12 files, 2 pars] waiting");
        Assert.Equal(5, set.Id);
        Assert.Equal(@"D:\Downloads", set.DownloadPath);
        Assert.Equal(12, set.FileCount);
        Assert.Equal(2, set.ParCount);
        Assert.Equal("[12 files, 2 pars] waiting", set.StatusString);
    }

    [Fact]
    public void DownloadPartStatus_Parses()
    {
        var status = new DownloadPartStatus("[102 files, 16 pars] D:36 DL:4 In:1 P:15 R:2");
        Assert.Equal(102, status.FileCount);
        Assert.Equal(16, status.ParCount);
        Assert.Equal(36, status.Downloaded);
        Assert.Equal(4, status.Downloading);
        Assert.Equal(1, status.Incomplete);
        Assert.Equal(15, status.Paused);
        Assert.Equal(2, status.Retries);
    }

    [Fact]
    public void DownloadPartStatus_WithoutBracket_IsEmpty()
    {
        var status = new DownloadPartStatus("Queued");
        Assert.Equal(0, status.FileCount);
    }

    [Fact]
    public void DownloadFileSet_Download_ParsesDecimalSizes()
    {
        var set = new DownloadFileSet(
            $"1\tMy.Download\t6356551598\t{PostedUnix}\tposter\tD:\\Downloads\t\t383.3 KB/5.92 GB\t[102 files, 16 pars] D:36 DL:4 In:0 P:15 R:0\t1");
        Assert.Equal(FileSetType.Download, set.FileSetType);
        Assert.Equal("My.Download", set.Subject);
        Assert.Equal(6356551598, set.FileSize);
        Assert.Equal(Posted, set.TimeStamp);
        Assert.Equal(383_300L, set.DownloadedCurrentSize);
        Assert.Equal(5_920_000_000L, set.DownloadedSize);
        Assert.True(set.IsPaused);
        Assert.Equal(102, set.PartStatus.FileCount);
        Assert.Equal(36, set.PartStatus.Downloaded);
    }

    [Fact]
    public void DownloadFileSet_Download_Captured()
    {
        // DOWNLOADS LIST 1 1 while a 3.49 GB download was running. Path fields were empty.
        var set = new DownloadFileSet(
            "17051\tSome.Release - [77/77] - \"some-release.rar\" (1/1)\t3486062667\t1793759183\tPoster@example.com\t\t\t1.04 GB/3.49 GB\t[68 files, 0 pars] D:19 DL:49 Inc:54 P:0 R:0\t0");
        Assert.Equal(17051, set.Id);
        Assert.Equal(FileSetType.Download, set.FileSetType);
        Assert.Equal(3486062667, set.FileSize);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1793759183).UtcDateTime, set.TimeStamp);
        Assert.Equal("Poster@example.com", set.Poster);
        Assert.Equal("", set.DownloadPath);
        Assert.Equal(1_040_000_000L, set.DownloadedCurrentSize);
        Assert.Equal(3_490_000_000L, set.DownloadedSize);
        // In this capture the progress total matched the listed size, within Newsbin's rounding.
        // That isn't guaranteed: when the group holds more of the set (other parts, par files) the total can be larger.
        Assert.InRange(set.DownloadedSize, set.FileSize * 0.995, set.FileSize * 1.005);
        Assert.False(set.IsPaused);
        Assert.Equal(68, set.PartStatus.FileCount);
        Assert.Equal(0, set.PartStatus.ParCount);
        Assert.Equal(19, set.PartStatus.Downloaded);
        Assert.Equal(49, set.PartStatus.Downloading);
        Assert.Equal(54, set.PartStatus.Incomplete);
        Assert.Equal(0, set.PartStatus.Paused);
        Assert.Equal(0, set.PartStatus.Retries);
    }

    [Fact]
    public void DownloadFileSet_GroupWithParFiles_Captured()
    {
        // One .7z.001 plus 9 par2 files. FileSize is the main post; the progress total is the whole group.
        var set = new DownloadFileSet(
            "17181\t<Name.7z> [01/10] - \"Name.7z.001\" yEnc  961287538 (1/1)\t994107323\t1790029622\tPoster <poster@example.com>\t\t\t994.8 MB/1.10 GB\t[1 files, 9 pars] D:1 DL:8 Inc:10 P:0 R:0\t0");
        Assert.Equal(FileSetType.Download, set.FileSetType);
        Assert.Equal(994107323, set.FileSize);
        Assert.Equal(994_800_000L, set.DownloadedCurrentSize);
        Assert.Equal(1_100_000_000L, set.DownloadedSize);
        Assert.True(set.DownloadedSize > set.FileSize);
        Assert.Equal(1, set.PartStatus.FileCount);
        Assert.Equal(9, set.PartStatus.ParCount);
        Assert.Equal(1, set.PartStatus.Downloaded);
        Assert.Equal(8, set.PartStatus.Downloading);
        Assert.Equal(10, set.PartStatus.Incomplete);
    }

    [Fact]
    public void GroupFileSet_SetWithParFiles_Captured()
    {
        // The same set in the search results: size is the whole group (1.10 GB), counts are "0 | 9".
        var set = new GroupFileSet(
            "1\t<Name.7z> [01/10] - \"Name.7z.001\" yEnc  961287538 (1/1)\t1100015812\t1790029622\tPoster <poster@example.com>\t0\t9\t1");
        Assert.Equal(1100015812, set.FileSize);
        Assert.Equal(0, set.FileCount);
        Assert.Equal(9, set.ParCount);
    }

    [Fact]
    public void DownloadFileSet_Header()
    {
        var set = new DownloadFileSet("2\t\t0\t0\talt.binaries.test\t\t\t10 MB/20 MB\tHeaders\t0");
        Assert.Equal(FileSetType.Header, set.FileSetType);
        Assert.Equal("alt.binaries.test", set.Subject);
        Assert.Equal(10_000_000L, set.DownloadedCurrentSize);
        Assert.Equal(20_000_000L, set.DownloadedSize);
        Assert.False(set.IsPaused);
    }

    [Theory]
    [InlineData("PAR Repair: 45%", FileSetType.Repair, 45)]
    [InlineData("UnRAR Done: 80%", FileSetType.Unpack, 80)]
    [InlineData("PAR Verifying", FileSetType.Repair, 0)]
    public void DownloadFileSet_RepairAndUnpack_ParsePercent(string status, FileSetType type, long percent)
    {
        var set = new DownloadFileSet($"3\tSubject\t100\t{PostedUnix}\tposter\tpath\t\t\t{status}\t0");
        Assert.Equal(type, set.FileSetType);
        Assert.Equal(100, set.DownloadedSize);
        Assert.Equal(percent, set.DownloadedCurrentSize);
    }

    [Fact]
    public void FilesDownloadItem_Parses()
    {
        var item = new FilesDownloadItem("\"my file.nfo\" 1234");
        Assert.Equal("my file.nfo", item.FileName);
        Assert.Equal(1234, item.FileSize);
    }

    [Fact]
    public void NewsbinStatus_Parses()
    {
        NewsbinStatus status = NewsbinStatus.FromLines(new[]
        {
            // Captured.
            "Speed\t0",
            "DownloadFolder\t1032968642560",
            "DataFolder\t1032968642560",
            "Queued\t0",
            "Speed Samples\t1\t2\t1\t1\t1\t1\t1\t1\t1\t1\t1\t1\t1\t1\t1\t1\t1\t1\t1\t1",
            "Unknown\t42",
        });
        Assert.Equal(0, status.Speed);
        Assert.Equal(1032968642560, status.DownloadFolderFreeSpace);
        Assert.Equal(1032968642560, status.DataFolderFreeSpace);
        Assert.Equal(0, status.QueuedFileSize);
        Assert.Equal(20, status.SpeedSamples.Count);
        Assert.Equal(2000, status.SpeedSamples[1]);
    }
}
