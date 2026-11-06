namespace Bergdahl.NewsbinPro.Client;

/// <summary>
/// Status codes returned by Newsbin in the first three characters of a response line.
/// </summary>
public enum StatusCode
{
    /// <summary>Internal code: the response line didn't start with a numeric status.</summary>
    ParseError = 90,

    /// <summary>The command succeeded.</summary>
    OK = 200,
    /// <summary>The bandwidth limiter is on.</summary>
    StatusBwLimiterOn = 251,
    /// <summary>The bandwidth limiter is off.</summary>
    StatusBwLimiterOff = 252,
    /// <summary>Downloading is paused.</summary>
    StatusPauseOn = 253,
    /// <summary>Downloading is not paused.</summary>
    StatusPauseOff = 254,
    /// <summary>Newsbin is waiting for posted data, for example an NZB upload.</summary>
    StatusPostData = 300,

    /// <summary>Not enough parameters.</summary>
    ErrorNotEnoughParams = 401,
    /// <summary>Loading something where the min age is greater than the max age.</summary>
    ErrorAgeMinGreaterThanMax = 402,
    /// <summary>Trying to list something out of range.</summary>
    ErrorGroupListRange = 403,
    /// <summary>A range was requested where min &gt;= max.</summary>
    ErrorRangeMinGreaterThanMax = 404,
    /// <summary>A range was requested where one of the values is out of range.</summary>
    ErrorRangeExceeded = 405,
    /// <summary>A sort was requested with a column that is out of range.</summary>
    ErrorSortColumnExceeded = 406,
    /// <summary>Trying to load a group that isn't in the current sub list.</summary>
    ErrorGroupListNoGroup = 407,
    /// <summary>The group name to update is empty.</summary>
    ErrorUpdateGroupMissing = 408,
    /// <summary>The group to update wasn't found.</summary>
    ErrorUpdateGroupNotFound = 409,
    /// <summary>The search string isn't a valid regular expression.</summary>
    ErrorFindInvalidSearchString = 410,
    /// <summary>When uploading an NZB, the file name is blank.</summary>
    ErrorUploadMissingFileName = 411,
    /// <summary>When uploading an NZB, the destination file couldn't be opened.</summary>
    ErrorUploadUnableToSave = 412,
    /// <summary>Authentication is enabled but no password is set in Newsbin.</summary>
    ErrorAuthNoPasswordSet = 413,
    /// <summary>Authentication is enabled but Newsbin isn't registered.</summary>
    ErrorAuthNotRegistered = 414,
    /// <summary>Command not allowed because Newsbin isn't registered.</summary>
    ErrorUnregistered = 415,
    /// <summary>Variable format error, expected <c>section:variable</c>.</summary>
    ErrorVarFormatError = 416,
    /// <summary>Variable not found.</summary>
    ErrorVarVariableNotFound = 417,
    /// <summary>Failed to set the variable.</summary>
    ErrorVarUnableToSet = 418,
    /// <summary>Unable to save the current configuration.</summary>
    ErrorUnableToSaveConfig = 419,
    /// <summary>When uploading an NZB, the file size is missing.</summary>
    ErrorUploadMissingFileSize = 420,
    /// <summary>When uploading an NZB, the autoload folder couldn't be found.</summary>
    ErrorUploadNoAutoload = 421,
    /// <summary>When downloading, the file wasn't found.</summary>
    ErrorDownloadNoFile = 430,
    /// <summary>When downloading, the 300 message was never seen.</summary>
    ErrorDownloadIncomplete = 431,
    /// <summary>The filter profile wasn't found.</summary>
    ErrorFilterNotFound = 440,
    /// <summary>Authentication is required.</summary>
    AuthNeedAuth = 480,
    /// <summary>Not implemented.</summary>
    ErrorUnimplemented = 500
}
