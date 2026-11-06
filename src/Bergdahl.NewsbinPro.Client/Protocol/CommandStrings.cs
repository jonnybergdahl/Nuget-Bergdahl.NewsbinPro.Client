using System.Collections.Generic;

namespace Bergdahl.NewsbinPro.Client;

/// <summary>
/// Maps each <see cref="Command"/> to the text sent to Newsbin.
/// </summary>
public static class CommandStrings
{
    /// <summary>The command text for every <see cref="Command"/> value.</summary>
    public static IReadOnlyDictionary<Command, string> Strings { get; } = new Dictionary<Command, string>
    {
        [Command.None] = "",
        [Command.Auth] = "AUTH",
        [Command.Quit] = "QUIT",

        [Command.Status] = "STATUS",
        [Command.Bwl] = "BWL",
        [Command.Pause] = "PAUSE",

        [Command.ListServers] = "LIST SERVERS",
        [Command.ListGogs] = "LIST GOGS",
        [Command.ListGroups] = "LIST GROUPS",
        [Command.ListChildren] = "LIST CHILDREN",
        [Command.ListChildrenDetails] = "LIST CHILDRENDETAILS",

        [Command.ListFilters] = "LIST FILTERS",
        [Command.FilterSetMinSize] = "FILTER SETMINSIZE",
        [Command.FilterSetMaxSize] = "FILTER SETMAXSIZE",
        [Command.FilterAddGroupFilter] = "FILTER ADDGROUPFILTER",
        [Command.FilterSetAge] = "FILTER SETAGE",
        [Command.FilterSetTextFilter] = "FILTER SETTEXTFILTER",
        [Command.FilterHideOld] = "FILTER HIDEOLD",
        [Command.FilterEnableFilters] = "FILTER ENABLEFILTERS",
        [Command.FilterMarkOld] = "FILTER MARKOLD",
        [Command.FilterMarkAllOld] = "FILTER MARKOLD",
        [Command.FilterSetProfile] = "FILTER SETPROFILE",
        [Command.FilterClear] = "FILTER CLEAR",

        [Command.SearchLocal] = "SEARCH LOCAL",
        [Command.SearchInternet] = "SEARCH INTERNET",

        [Command.GroupLoad] = "GROUP LOAD",
        [Command.GroupClear] = "GROUP CLEAR",
        [Command.GroupDelete] = "GROUP DELETE",

        [Command.LoadedCount] = "LOADED COUNT",
        [Command.LoadedList] = "LOADED LIST",
        [Command.LoadedSort] = "LOADED SORT",
        [Command.LoadedClear] = "LOADED CLEAR",
        [Command.LoadedFind] = "LOADED FIND",
        [Command.LoadedDownload] = "LOADED DOWNLOAD",
        [Command.LoadedGetNzb] = "LOADED GETNZB",

        [Command.DownloadsCount] = "DOWNLOADS COUNT",
        [Command.DownloadsLock] = "DOWNLOADS LOCK",
        [Command.DownloadsUnlock] = "DOWNLOADS UNLOCK",
        [Command.DownloadsList] = "DOWNLOADS LIST",
        [Command.DownloadsSort] = "DOWNLOADS SORT",
        [Command.DownloadsClear] = "DOWNLOADS CLEAR",
        [Command.DownloadsFind] = "DOWNLOADS FIND",
        [Command.DownloadsAssemble] = "DOWNLOADS ASSEMBLE",
        [Command.DownloadsDelete] = "DOWNLOADS DELETE",
        [Command.DownloadsPause] = "DOWNLOADS PAUSE",
        [Command.DownloadsResume] = "DOWNLOADS RESUME",
        [Command.DownloadsMoveUp] = "DOWNLOADS MOVEUP",
        [Command.DownloadsMoveDown] = "DOWNLOADS MOVEDOWN",
        [Command.DownloadsMoveTop] = "DOWNLOADS MOVETOP",
        [Command.DownloadsMoveBottom] = "DOWNLOADS MOVEBOTTOM",

        [Command.FilesCount] = "FILES COUNT",
        [Command.FilesList] = "FILES LIST",
        [Command.FilesDownload] = "FILES DOWNLOAD",
        [Command.Continue] = "300 CONTINUE",

        [Command.WishCount] = "WISH COUNT",
        [Command.WishList] = "WISH LIST",
        [Command.WishDownload] = "WISH DOWNLOAD",

        [Command.UploadNzbBinary] = "UPLOAD NZBBINARY",
    };
}
