using System;
using Xunit;

namespace Bergdahl.NewsbinPro.Client.Tests;

public class ProtocolTests
{
    [Fact]
    public void AuthString_IsUpperCaseMd5OfPasswordAndSalt()
    {
        // MD5("abc") = 900150983cd24fb0d6963f7d28e17f72
        Assert.Equal("900150983CD24FB0D6963F7D28E17F72", AuthenticationHelper.GetAuthString("a", "bc"));
    }

    [Theory]
    [InlineData("200 OK", StatusCode.OK, "OK")]
    [InlineData("200 Status List Follows ", StatusCode.OK, "Status List Follows ")]
    [InlineData("252 BW Limiter Off", StatusCode.StatusBwLimiterOff, "BW Limiter Off")]
    [InlineData("254 Download Resumed", StatusCode.StatusPauseOff, "Download Resumed")]
    [InlineData("405 Loaded List Range exceeded", StatusCode.ErrorRangeExceeded, "Loaded List Range exceeded")]
    [InlineData("253 Paused", StatusCode.StatusPauseOn, "Paused")]
    [InlineData("405 Range exceeded", StatusCode.ErrorRangeExceeded, "Range exceeded")]
    [InlineData("200", StatusCode.OK, "")]
    [InlineData("hello", StatusCode.ParseError, "hello")]
    [InlineData("", StatusCode.ParseError, "")]
    public void Response_ParsesStatusAndMessage(string line, StatusCode status, string message)
    {
        var response = new Response(line);
        Assert.Equal(status, response.StatusCode);
        Assert.Equal(message, response.Message);
    }

    [Theory]
    [InlineData("200 OK", true)]
    [InlineData("300 Send data", true)]
    [InlineData("401 Not enough params", false)]
    [InlineData("xyz", false)]
    public void Response_IsSuccess(string line, bool expected) =>
        Assert.Equal(expected, new Response(line).IsSuccess);

    [Fact]
    public void ParseGreeting_ExtractsVersionAndSalt()
    {
        // Captured from Newsbin 6.91RC4.
        NewsbinClient.ParseGreeting("Newsbin Server 6.91RC4 - 13B1737F145E1F15 Need to authenticate", out string? version, out string salt);
        Assert.Equal("Newsbin Server 6.91RC4", version);
        Assert.Equal("13B1737F145E1F15", salt);
    }

    [Fact]
    public void ParseGreeting_ToleratesShortSalt()
    {
        NewsbinClient.ParseGreeting("Newsbin - x", out string? version, out string salt);
        Assert.Equal("Newsbin", version);
        Assert.Equal("", salt);
    }

    [Fact]
    public void BuildCommandLine_SkipsEmptyArguments()
    {
        Assert.Equal("LOADED LIST 0 10", NewsbinClient.BuildCommandLine(Command.LoadedList, new[] { "0", "", "10" }));
        Assert.Equal("STATUS", NewsbinClient.BuildCommandLine(Command.Status, null));
    }

    [Fact]
    public void BuildCommandLine_RejectsLineBreaks()
    {
        Assert.Throws<ArgumentException>(() => NewsbinClient.BuildCommandLine(Command.FilterSetTextFilter, new[] { "a\r\nQUIT" }));
    }

    [Fact]
    public void EveryCommandHasAString()
    {
        foreach (Command command in Enum.GetValues(typeof(Command)))
            Assert.True(CommandStrings.Strings.ContainsKey(command), command.ToString());
    }

    [Fact]
    public void CommunicationException_ToStringIncludesCommandAndResponse()
    {
        var ex = new CommunicationException("Boom", Command.Status, new Response("401 Missing"));
        string text = ex.ToString();
        Assert.Contains("Boom", text);
        Assert.Contains("Status", text);
        Assert.Contains("ErrorNotEnoughParams Missing", text);
    }
}
