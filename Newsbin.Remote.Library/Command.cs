using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newsbin.Remote
{
    public enum Command
    {
        None, 

        Auth,
        Quit,
        Status,
        Bwl,
        Pause,

        ListServers,
        ListGogs,
        ListGroups,
        ListChildren,
        ListChildrenDetails,
        
        ListFilters,
        FilterSetMinSize,
        FilterSetMaxSize,
        FilterAddGroupFilter,
        FilterSetAge,
        FilterSetTextFilter,
        FilterHideOld,
        FilterEnableFilters,
        FilterMarkOld,
        FilterMarkAllOld,
        FilterSetProfile,

        SearchLocal,
        SearchInternet,

        GroupLoad,
        GroupClear,
        GroupDelete,

        LoadedCount,
        LoadedList,
        LoadedSort,
        LoadedClear,
        LoadedFind,
        LoadedDownload,
        LoadedGetNzb,

        DownloadsCount,
        DownloadsLock,
        DownloadsUnlock,
        DownloadsList,
        DownloadsSort,
        DownloadsClear,
        DownloadsFind,
        DownloadsAssemble,
        DownloadsDelete,
        DownloadsPause,
        DownloadsResume,
        DownloadsMoveUp,
        DownloadsMoveDown,
        DownloadsMoveTop,
        DownloadsMoveBottom,

        FilesCount,
        FilesList,
        FilesDownload,

        WishCount,
        WishList,
        WishDownload,

        UploadNzbBinary
    }
}
