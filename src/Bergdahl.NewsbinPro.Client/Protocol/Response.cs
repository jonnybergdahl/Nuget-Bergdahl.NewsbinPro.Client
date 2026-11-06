using System;
using System.Globalization;

namespace Bergdahl.NewsbinPro.Client;

/// <summary>
/// A single response line from Newsbin, split into status code and message.
/// </summary>
public class Response
{
    /// <summary>The status code, or <see cref="Client.StatusCode.ParseError"/> if the line had none.</summary>
    public StatusCode StatusCode { get; }

    /// <summary>The text following the status code.</summary>
    public string Message { get; }

    /// <summary>True for status codes below 400.</summary>
    public bool IsSuccess => StatusCode != StatusCode.ParseError && (int)StatusCode < 400;

    /// <summary>Parses a response line such as <c>200 OK</c>.</summary>
    /// <param name="response">The raw response line.</param>
    public Response(string response)
    {
        if (response is null)
            throw new ArgumentNullException(nameof(response));

        if (response.Length >= 3 && int.TryParse(response.Substring(0, 3), NumberStyles.None, CultureInfo.InvariantCulture, out int status))
        {
            StatusCode = (StatusCode)status;
            Message = response.Length > 4 ? response.Substring(4) : "";
        }
        else
        {
            StatusCode = StatusCode.ParseError;
            Message = response;
        }
    }

    /// <inheritdoc />
    public override string ToString() => $"{StatusCode} {Message}";
}
