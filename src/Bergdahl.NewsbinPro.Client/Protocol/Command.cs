namespace Bergdahl.NewsbinPro.Client;

/// <summary>
/// The commands understood by the Newsbin Pro remote control interface.
/// </summary>
/// <remarks>The text sent for each command is available through <see cref="CommandStrings"/>.</remarks>
public enum Command
{
    /// <summary>No command.</summary>
    None,

    /// <summary><c>AUTH</c>: authenticate with the password hash.</summary>
    Auth,
    /// <summary><c>QUIT</c>: close the session.</summary>
    Quit,
    /// <summary><c>STATUS</c>: get speed, free space and queue size.</summary>
    Status,
    /// <summary><c>BWL</c>: get or set the bandwidth limiter.</summary>
    Bwl,
    /// <summary><c>PAUSE</c>: get or set the paused state.</summary>
    Pause,

    /// <summary><c>LIST SERVERS</c></summary>
    ListServers,
    /// <summary><c>LIST GOGS</c>: list groups of groups.</summary>
    ListGogs,
    /// <summary><c>LIST GROUPS</c></summary>
    ListGroups,
    /// <summary><c>LIST CHILDREN</c>: list the groups in a group of groups.</summary>
    ListChildren,
    /// <summary><c>LIST CHILDRENDETAILS</c></summary>
    ListChildrenDetails,

    /// <summary><c>LIST FILTERS</c></summary>
    ListFilters,
    /// <summary><c>FILTER SETMINSIZE</c></summary>
    FilterSetMinSize,
    /// <summary><c>FILTER SETMAXSIZE</c></summary>
    FilterSetMaxSize,
    /// <summary><c>FILTER ADDGROUPFILTER</c></summary>
    FilterAddGroupFilter,
    /// <summary><c>FILTER SETAGE</c></summary>
    FilterSetAge,
    /// <summary><c>FILTER SETTEXTFILTER</c></summary>
    FilterSetTextFilter,
    /// <summary><c>FILTER HIDEOLD</c></summary>
    FilterHideOld,
    /// <summary><c>FILTER ENABLEFILTERS</c></summary>
    FilterEnableFilters,
    /// <summary><c>FILTER MARKOLD</c> with an id.</summary>
    FilterMarkOld,
    /// <summary><c>FILTER MARKOLD</c> without an id.</summary>
    FilterMarkAllOld,
    /// <summary><c>FILTER SETPROFILE</c></summary>
    FilterSetProfile,
    /// <summary><c>FILTER CLEAR</c></summary>
    FilterClear,

    /// <summary><c>SEARCH LOCAL</c></summary>
    SearchLocal,
    /// <summary><c>SEARCH INTERNET</c></summary>
    SearchInternet,

    /// <summary><c>GROUP LOAD</c></summary>
    GroupLoad,
    /// <summary><c>GROUP CLEAR</c></summary>
    GroupClear,
    /// <summary><c>GROUP DELETE</c></summary>
    GroupDelete,

    /// <summary><c>LOADED COUNT</c></summary>
    LoadedCount,
    /// <summary><c>LOADED LIST</c></summary>
    LoadedList,
    /// <summary><c>LOADED SORT</c></summary>
    LoadedSort,
    /// <summary><c>LOADED CLEAR</c></summary>
    LoadedClear,
    /// <summary><c>LOADED FIND</c></summary>
    LoadedFind,
    /// <summary><c>LOADED DOWNLOAD</c></summary>
    LoadedDownload,
    /// <summary><c>LOADED GETNZB</c></summary>
    LoadedGetNzb,

    /// <summary><c>DOWNLOADS COUNT</c></summary>
    DownloadsCount,
    /// <summary><c>DOWNLOADS LOCK</c></summary>
    DownloadsLock,
    /// <summary><c>DOWNLOADS UNLOCK</c></summary>
    DownloadsUnlock,
    /// <summary><c>DOWNLOADS LIST</c></summary>
    DownloadsList,
    /// <summary><c>DOWNLOADS SORT</c></summary>
    DownloadsSort,
    /// <summary><c>DOWNLOADS CLEAR</c></summary>
    DownloadsClear,
    /// <summary><c>DOWNLOADS FIND</c></summary>
    DownloadsFind,
    /// <summary><c>DOWNLOADS ASSEMBLE</c></summary>
    DownloadsAssemble,
    /// <summary><c>DOWNLOADS DELETE</c></summary>
    DownloadsDelete,
    /// <summary><c>DOWNLOADS PAUSE</c></summary>
    DownloadsPause,
    /// <summary><c>DOWNLOADS RESUME</c></summary>
    DownloadsResume,
    /// <summary><c>DOWNLOADS MOVEUP</c></summary>
    DownloadsMoveUp,
    /// <summary><c>DOWNLOADS MOVEDOWN</c></summary>
    DownloadsMoveDown,
    /// <summary><c>DOWNLOADS MOVETOP</c></summary>
    DownloadsMoveTop,
    /// <summary><c>DOWNLOADS MOVEBOTTOM</c></summary>
    DownloadsMoveBottom,

    /// <summary><c>FILES COUNT</c></summary>
    FilesCount,
    /// <summary><c>FILES LIST</c></summary>
    FilesList,
    /// <summary><c>FILES DOWNLOAD</c></summary>
    FilesDownload,
    /// <summary><c>300 CONTINUE</c>: tells Newsbin to start sending file data.</summary>
    Continue,

    /// <summary><c>WISH COUNT</c></summary>
    WishCount,
    /// <summary><c>WISH LIST</c></summary>
    WishList,
    /// <summary><c>WISH DOWNLOAD</c></summary>
    WishDownload,

    /// <summary><c>UPLOAD NZBBINARY</c></summary>
    UploadNzbBinary
}
