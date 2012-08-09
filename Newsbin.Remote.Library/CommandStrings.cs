using System;
using System.Linq;
using System.Collections.Generic;

namespace Newsbin.Remote
{
    public class CommandStrings: Dictionary<Command, string>
    {
        public static CommandStrings Strings = new CommandStrings();

        public CommandStrings()
        {
            this.Add(Command.None, "");
            this.Add(Command.Auth, "AUTH");

            this.Add(Command.Status, "STATUS");
            this.Add(Command.Bwl, "BWL");
            this.Add(Command.Pause, "PAUSE");
            this.Add(Command.ListServers, "LIST SERVERS");

            this.Add(Command.ListGogs, "LIST GOGS");
            this.Add(Command.ListGroups, "LIST GROUPS");
            this.Add(Command.ListChildren, "LIST CHILDREN");
            this.Add(Command.ListChildrenDetails, "LIST CHILDRENDETAILS");

            this.Add(Command.ListFilters, "LIST FILTERS");
            this.Add(Command.FilterSetMinSize, "FILTER SETMINSIZE");
            this.Add(Command.FilterSetMaxSize, "FILTER SETMAXSIZE");
            this.Add(Command.FilterAddGroupFilter, "FILTER ADDGROUPFILTER");
            this.Add(Command.FilterSetAge, "FILTER SETAGE");
            this.Add(Command.FilterSetTextFilter, "FILTER SETTEXTFILTER");
            this.Add(Command.FilterHideOld, "FILTER HIDEOLD");
            this.Add(Command.FilterEnableFilters, "FILTER ENABLEFILTERS");
            this.Add(Command.FilterMarkOld, "FILTER MARKOLD");
            this.Add(Command.FilterMarkAllOld, "FILTER MARKOLD");
            this.Add(Command.FilterSetProfile, "FILTER SETPROFILE");
            this.Add(Command.FilterClear, "FILTER CLEAR");

            this.Add(Command.SearchLocal, "SEARCH LOCAL");
            this.Add(Command.SearchInternet, "SEARCH INTERNET");

            this.Add(Command.GroupLoad, "GROUP LOAD");
            this.Add(Command.GroupClear, "GROUP CLEAR");
            this.Add(Command.GroupDelete, "GROUP DELETE");

            this.Add(Command.LoadedCount, "LOADED COUNT");
            this.Add(Command.LoadedList, "LOADED LIST");
            this.Add(Command.LoadedSort, "LOADED SORT");
            this.Add(Command.LoadedClear, "LOADED CLEAR");
            this.Add(Command.LoadedFind, "LOADED FIND");
            this.Add(Command.LoadedDownload, "LOADED DOWNLOAD");
            this.Add(Command.LoadedGetNzb, "LOADED GETNZB");

            this.Add(Command.DownloadsCount, "DOWNLOADS COUNT");
            this.Add(Command.DownloadsLock, "DOWNLOADS LOCK");
            this.Add(Command.DownloadsUnlock, "DOWNLOADS UNLOCK");
            this.Add(Command.DownloadsList, "DOWNLOADS LIST");
            this.Add(Command.DownloadsSort, "DOWNLOADS SORT");
            this.Add(Command.DownloadsClear, "DOWNLOADS CLEAR");
            this.Add(Command.DownloadsFind, "DOWNLOADS FIND");
            this.Add(Command.DownloadsAssemble, "DOWNLOADS ASSEMBLE");
            this.Add(Command.DownloadsDelete, "DOWNLOADS DELETE");
            this.Add(Command.DownloadsPause, "DOWNLOADS PAUSE");
            this.Add(Command.DownloadsResume, "DOWNLOADS RESUME");
            this.Add(Command.DownloadsMoveUp, "DOWNLOADS MOVEUP");
            this.Add(Command.DownloadsMoveDown, "DOWNLOADS MOVEDOWN");
            this.Add(Command.DownloadsMoveTop, "DOWNLOADS MOVETOP");
            this.Add(Command.DownloadsMoveBottom, "DOWNLOADS MOVEBOTTOM");

            this.Add(Command.FilesCount, "FILES COUNT");
            this.Add(Command.FilesList, "FILES LIST");
            this.Add(Command.FilesDownload, "FILES DOWNLOAD");
            this.Add(Command.Continue, "300 CONTINUE");

            this.Add(Command.WishCount, "WISH COUNT");
            this.Add(Command.WishList, "WISH LIST");
            this.Add(Command.WishDownload, "WISH DOWNLOAD");

            this.Add(Command.UploadNzbBinary, "UPLOAD NZBBINARY");

            this.Add(Command.Quit, "QUIT");
        }
    }
}
