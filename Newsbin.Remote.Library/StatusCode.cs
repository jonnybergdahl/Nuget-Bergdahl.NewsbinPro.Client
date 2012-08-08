using System;
using System.Linq;

namespace Newsbin.Remote
{
    public enum StatusCode
    {
        // Internal codes
        ParseError =    90,

        // Newsbin codes
	    OK = 200,
        StatusBwLimiterOn = 251,
        StatusBwLimiterOff = 252,
        StatusPauseOn = 253,
        StatusPauseOff = 254,
        StatusPostData = 300,

        ErrorNotEnoughParams = 401,
        ErrorAgeMinGreaterThanMax = 402,        // loading something min age > max age
        ErrorGroupListRange = 403,              // trying to list something out of range
        ErrorRangeMinGreaterThanMax = 404,      // requested a range and the min >= max;
        ErrorRangeExceeded = 405,               // requested a range and one of the values is out of range
        ErrorSortColumnExceeded = 406,          // requested a sort but, the column is out of range
        ErrorGroupListNoGroup = 407,            // trying to load a group that's not in the current sub list
        ErrorUpdateGroupMissing = 408,          // Update group name empty
        ErrorUpdateGroupNotFound = 409,         // Update group not found
        ErrorFindInvalidSearchString = 410,     // Search string isn't a valid RE
        ErrorUploadMissingFileName = 411,		// When uploading an NZB, the filename seems to be blank
        ErrorUploadUnableToSave = 412,		    // When uploading an NZB, unable to open the destination file
        ErrorAuthNoPasswordSet = 413,		    // Auth is enabled but, no password is set in Newsbin
        ErrorAuthNotRegistered = 414,		    // Auth is enabled but Newsbin isn't registered
        ErrorUnregistered = 415,		        // Command disallowed - Newsbin isn't registered
        ErrorVarFormatError = 416,		        // section:variable
        ErrorVarVariableNotFound = 417,		    // Var variable not found
        ErrorVarUnableToSet = 418,		        // Var failed to set variable
        ErrorUnableToSaveConfig = 419,		    // Unable to save the current configuration
        ErrorUploadMissingFileSize= 420,		// When uploading an NZB, unable to open the destination file
        ErrorUploadNoAutoload = 421,		    // When uploading an NZB, unable find the autoload folder	
        ErrorDownloadNoFile = 430,		        // When trying to download, the file wasn't found
        ErrorDownloadIncomplete = 431,		    // When trying to download 300 message was never seen
        ErrorFilterNotFound = 440,		        // When trying to add a filter profile, it wasn't found
        AuthNeedAuth = 480,
        ErrorUnimplemented = 500		        // Not implemented yet

    }
}
