using System;
using System.Linq;

namespace Newsbin.Remote
{
    public static class StatusCodes
    {
        // Internal codes
        public const string ParseError =    "090";

        // Newsbin codes
	    public const string OK =             "200";
        public const string BW_LIMITER_ON = "251";
    public const string STATUS_BW_LIMITER_OFF				= "252";
    public const string STATUS_PAUSE_ON						= "253";
    public const string STATUS_PAUSE_OFF = "254";
    //STATUS_POST_DATA					= 300,	
    //ERROR_NOT_ENOUGH_PARAMS				= 401,
    //ERROR_AGE_MIN_GREATER_THAN_MAX      = 402,      // loading something min age > max age
    //ERROR_GROUP_LIST_RANGE				= 403,      // trying to list something out of range
    //ERROR_RANGE_MIN_GREATER_THAN_MAX    = 404,      // requested a range and the min >= max;
    //ERROR_RANGE_EXCEEDED				= 405,      // requested a range and one of the values is out of range
    //ERROR_SORT_COLUMN_EXCEEDED			= 406,      // requested a sort but, the column is out of range
    //ERROR_GROUP_LIST_NO_GROUP			= 407,      // trying to load a group that's not in the current sub list
    //ERROR_UPDATE_GROUP_MISSING			= 408,      // Update group name empty
    //ERROR_UPDATE_GROUP_NOT_FOUND		= 409,      // Update group not found
    //ERROR_FIND_INVALID_SEARCH_STRING    = 410,      // Search string isn't a valid RE
    //ERROR_UPLOAD_MISSING_FILENAME		= 411,		// When uploading an NZB, the filename seems to be blank
    //ERROR_UPLOAD_UNABLE_TO_SAVE			= 412,		// When uploading an NZB, unable to open the destination file
    //ERROR_AUTH_NOPASSWORD_SET			= 413,		// Auth is enabled but, no password is set in Newsbin
    //ERROR_AUTH_NOT_REGISTERED			= 414,		// Auth is enabled but Newsbin isn't registered
    //ERROR_UNREGISTERED					= 415,		// Command disallowed - Newsbin isn't registered
    //ERROR_VAR_FORMAT_ERROR				= 416,		// section:variable
    //ERROR_VAR_VARIABLE_NOT_FOUND		= 417,		// Var variable not found
    //ERROR_VAR_UNABLE_TO_SET				= 418,		// Var failed to set variable
    //ERROR_UNABLE_TO_SAVE_CONFIG			= 419,		// Unable to save the current configuration
    //ERROR_UPLOAD_MISSING_FILESIZE		= 420,		// When uploading an NZB, unable to open the destination file
    //ERROR_UPLOAD_NO_AUTOLOAD			= 421,		// When uploading an NZB, unable find the autoload folder	
    //ERROR_DOWNLOAD_NOFILE				= 430,		// When trying to download, the file wasn't found
    //ERROR_DOWNLOAD_INCOMPLETE			= 431,		// When trying to download 300 message was never seen
    //ERROR_FILTER_NOT_FOUND              = 440,		// When trying to add a filter profile, it wasn't found
        public const string AuthNeedAuth = "480";
    //ERROR_UNIMPLEMENTED					= 500		// Not implemented yet

    }
}
