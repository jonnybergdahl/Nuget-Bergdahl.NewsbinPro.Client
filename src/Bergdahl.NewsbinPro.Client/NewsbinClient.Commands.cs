using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bergdahl.NewsbinPro.Client;

// The high level commands. Each one has a synchronous and an async version. Unless noted otherwise
// they throw a CommunicationException when Newsbin answers with an error status (400 and up).
public partial class NewsbinClient
{
    #region Status

    /// <summary>Gets speed, free disk space and queue size.</summary>
    public NewsbinStatus GetStatus() =>
        NewsbinStatus.FromLines(SendListCommand(Command.Status).ListItems);

    /// <summary>Gets speed, free disk space and queue size.</summary>
    public async Task<NewsbinStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
        NewsbinStatus.FromLines((await SendListCommandAsync(Command.Status, null, cancellationToken).ConfigureAwait(continueOnCapturedContext)).ListItems);

    /// <summary>Returns true if the bandwidth limiter is on.</summary>
    public bool GetBandwidthLimiterState() =>
        SendCommand(Command.Bwl).StatusCode == StatusCode.StatusBwLimiterOn;

    /// <summary>Returns true if the bandwidth limiter is on.</summary>
    public async Task<bool> GetBandwidthLimiterStateAsync(CancellationToken cancellationToken = default) =>
        (await SendCommandAsync(Command.Bwl, null, cancellationToken).ConfigureAwait(continueOnCapturedContext)).StatusCode == StatusCode.StatusBwLimiterOn;

    /// <summary>Turns the bandwidth limiter on or off and returns the new state.</summary>
    public bool SetBandwidthLimiterState(bool state) =>
        SendCommand(Command.Bwl, OnOff(state)).StatusCode == StatusCode.StatusBwLimiterOn;

    /// <summary>Turns the bandwidth limiter on or off and returns the new state.</summary>
    public async Task<bool> SetBandwidthLimiterStateAsync(bool state, CancellationToken cancellationToken = default) =>
        (await SendCommandAsync(Command.Bwl, new[] { OnOff(state) }, cancellationToken).ConfigureAwait(continueOnCapturedContext)).StatusCode == StatusCode.StatusBwLimiterOn;

    /// <summary>Returns true if downloading is paused.</summary>
    public bool GetPausedState() =>
        SendCommand(Command.Pause).StatusCode == StatusCode.StatusPauseOn;

    /// <summary>Returns true if downloading is paused.</summary>
    public async Task<bool> GetPausedStateAsync(CancellationToken cancellationToken = default) =>
        (await SendCommandAsync(Command.Pause, null, cancellationToken).ConfigureAwait(continueOnCapturedContext)).StatusCode == StatusCode.StatusPauseOn;

    /// <summary>Pauses or resumes downloading and returns the new state.</summary>
    public bool SetPausedState(bool state) =>
        SendCommand(Command.Pause, OnOff(state)).StatusCode == StatusCode.StatusPauseOn;

    /// <summary>Pauses or resumes downloading and returns the new state.</summary>
    public async Task<bool> SetPausedStateAsync(bool state, CancellationToken cancellationToken = default) =>
        (await SendCommandAsync(Command.Pause, new[] { OnOff(state) }, cancellationToken).ConfigureAwait(continueOnCapturedContext)).StatusCode == StatusCode.StatusPauseOn;

    #endregion

    #region Groups and servers

    /// <summary>Lists the groups.</summary>
    public IReadOnlyList<string> ListGroups() => List(Command.ListGroups, Identity);

    /// <summary>Lists the groups.</summary>
    public Task<IReadOnlyList<string>> ListGroupsAsync(CancellationToken cancellationToken = default) =>
        ListAsync(Command.ListGroups, Identity, cancellationToken);

    /// <summary>Lists the groups of groups (GOGs).</summary>
    public IReadOnlyList<string> ListGogs() => List(Command.ListGogs, Identity);

    /// <summary>Lists the groups of groups (GOGs).</summary>
    public Task<IReadOnlyList<string>> ListGogsAsync(CancellationToken cancellationToken = default) =>
        ListAsync(Command.ListGogs, Identity, cancellationToken);

    /// <summary>Lists the groups in a group of groups.</summary>
    public IReadOnlyList<string> ListChildren(string gogName) => List(Command.ListChildren, Identity, gogName);

    /// <summary>Lists the groups in a group of groups.</summary>
    public Task<IReadOnlyList<string>> ListChildrenAsync(string gogName, CancellationToken cancellationToken = default) =>
        ListAsync(Command.ListChildren, Identity, cancellationToken, gogName);

    /// <summary>Lists the groups in a group of groups, with details.</summary>
    public IReadOnlyList<string> ListChildrenDetails(string gogName) => List(Command.ListChildrenDetails, Identity, gogName);

    /// <summary>Lists the groups in a group of groups, with details.</summary>
    public Task<IReadOnlyList<string>> ListChildrenDetailsAsync(string gogName, CancellationToken cancellationToken = default) =>
        ListAsync(Command.ListChildrenDetails, Identity, cancellationToken, gogName);

    /// <summary>Lists all groups of groups and the groups in each.</summary>
    public GogTree ListGogsWithChildren()
    {
        var result = new GogTree();
        foreach (string gogName in ListGogs())
        {
            result.AddGroupOfGroups(gogName);
            foreach (string groupName in ListChildren(gogName))
                result.Add(gogName, groupName);
        }
        return result;
    }

    /// <summary>Lists all groups of groups and the groups in each.</summary>
    public async Task<GogTree> ListGogsWithChildrenAsync(CancellationToken cancellationToken = default)
    {
        var result = new GogTree();
        foreach (string gogName in await ListGogsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext))
        {
            result.AddGroupOfGroups(gogName);
            foreach (string groupName in await ListChildrenAsync(gogName, cancellationToken).ConfigureAwait(continueOnCapturedContext))
                result.Add(gogName, groupName);
        }
        return result;
    }

    /// <summary>Lists the news servers.</summary>
    public IReadOnlyList<string> ListServers() => List(Command.ListServers, Identity);

    /// <summary>Lists the news servers.</summary>
    public Task<IReadOnlyList<string>> ListServersAsync(CancellationToken cancellationToken = default) =>
        ListAsync(Command.ListServers, Identity, cancellationToken);

    #endregion

    #region Filters

    /// <summary>Lists the filter profiles.</summary>
    public IReadOnlyList<string> ListFilters() => List(Command.ListFilters, Identity);

    /// <summary>Lists the filter profiles.</summary>
    public Task<IReadOnlyList<string>> ListFiltersAsync(CancellationToken cancellationToken = default) =>
        ListAsync(Command.ListFilters, Identity, cancellationToken);

    /// <summary>Sets the minimum size filter.</summary>
    public void SetFilterMinSize(long minSize) => Execute(Command.FilterSetMinSize, Arg(minSize));

    /// <summary>Sets the minimum size filter.</summary>
    public Task SetFilterMinSizeAsync(long minSize, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.FilterSetMinSize, cancellationToken, Arg(minSize));

    /// <summary>Sets the maximum size filter.</summary>
    public void SetFilterMaxSize(long maxSize) => Execute(Command.FilterSetMaxSize, Arg(maxSize));

    /// <summary>Sets the maximum size filter.</summary>
    public Task SetFilterMaxSizeAsync(long maxSize, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.FilterSetMaxSize, cancellationToken, Arg(maxSize));

    /// <summary>Adds a group filter.</summary>
    public void AddGroupFilter(string groupName) => Execute(Command.FilterAddGroupFilter, groupName);

    /// <summary>Adds a group filter.</summary>
    public Task AddGroupFilterAsync(string groupName, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.FilterAddGroupFilter, cancellationToken, groupName);

    /// <summary>Sets the age filter in hours.</summary>
    public void SetFilterAge(long hours) => Execute(Command.FilterSetAge, Arg(hours));

    /// <summary>Sets the age filter in hours.</summary>
    public Task SetFilterAgeAsync(long hours, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.FilterSetAge, cancellationToken, Arg(hours));

    /// <summary>Sets the text filter.</summary>
    public void SetTextFilter(string text) => Execute(Command.FilterSetTextFilter, text);

    /// <summary>Sets the text filter.</summary>
    public Task SetTextFilterAsync(string text, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.FilterSetTextFilter, cancellationToken, text);

    /// <summary>Hides or shows old file sets.</summary>
    public void HideOld(bool hide) => Execute(Command.FilterHideOld, Flag(hide));

    /// <summary>Hides or shows old file sets.</summary>
    public Task HideOldAsync(bool hide, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.FilterHideOld, cancellationToken, Flag(hide));

    /// <summary>Turns filters on or off.</summary>
    public void EnableFilters(bool enable) => Execute(Command.FilterEnableFilters, Flag(enable));

    /// <summary>Turns filters on or off.</summary>
    public Task EnableFiltersAsync(bool enable, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.FilterEnableFilters, cancellationToken, Flag(enable));

    /// <summary>Clears all filters.</summary>
    public void ClearFilters() => Execute(Command.FilterClear);

    /// <summary>Clears all filters.</summary>
    public Task ClearFiltersAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.FilterClear, cancellationToken);

    /// <summary>Marks a file set as old.</summary>
    public void MarkOld(int id) => Execute(Command.FilterMarkOld, Arg(id));

    /// <summary>Marks a file set as old.</summary>
    public Task MarkOldAsync(int id, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.FilterMarkOld, cancellationToken, Arg(id));

    /// <summary>Marks all file sets as old.</summary>
    public void MarkAllOld() => Execute(Command.FilterMarkAllOld);

    /// <summary>Marks all file sets as old.</summary>
    public Task MarkAllOldAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.FilterMarkAllOld, cancellationToken);

    /// <summary>Selects a filter profile.</summary>
    public void SetFilterProfile(string profileName) => Execute(Command.FilterSetProfile, profileName);

    /// <summary>Selects a filter profile.</summary>
    public Task SetFilterProfileAsync(string profileName, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.FilterSetProfile, cancellationToken, profileName);

    #endregion

    #region Search

    /// <summary>Searches the local database.</summary>
    /// <param name="minAge">Minimum age.</param>
    /// <param name="maxAge">Maximum age.</param>
    /// <param name="searchString">The text to search for.</param>
    public void SearchLocal(int minAge, int maxAge, string searchString) =>
        Execute(Command.SearchLocal, Arg(minAge), Arg(maxAge), searchString);

    /// <inheritdoc cref="SearchLocal(int, int, string)"/>
    public Task SearchLocalAsync(int minAge, int maxAge, string searchString, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.SearchLocal, cancellationToken, Arg(minAge), Arg(maxAge), searchString);

    /// <summary>Searches the internet search service.</summary>
    /// <param name="minAge">Minimum age.</param>
    /// <param name="maxAge">Maximum age.</param>
    /// <param name="searchString">The text to search for.</param>
    public void SearchInternet(int minAge, int maxAge, string searchString) =>
        Execute(Command.SearchInternet, Arg(minAge), Arg(maxAge), searchString);

    /// <inheritdoc cref="SearchInternet(int, int, string)"/>
    public Task SearchInternetAsync(int minAge, int maxAge, string searchString, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.SearchInternet, cancellationToken, Arg(minAge), Arg(maxAge), searchString);

    #endregion

    #region Group

    /// <summary>Loads the selected group and returns the number of loaded file sets.</summary>
    public long LoadGroup(int minAge, int maxAge) => SendLongCommand(Command.GroupLoad, Arg(minAge), Arg(maxAge));

    /// <summary>Loads the selected group and returns the number of loaded file sets.</summary>
    public Task<long> LoadGroupAsync(int minAge, int maxAge, CancellationToken cancellationToken = default) =>
        SendLongCommandAsync(Command.GroupLoad, new[] { Arg(minAge), Arg(maxAge) }, cancellationToken);

    /// <summary>Clears the group.</summary>
    public void ClearGroup() => Execute(Command.GroupClear);

    /// <summary>Clears the group.</summary>
    public Task ClearGroupAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.GroupClear, cancellationToken);

    /// <summary>Deletes a file set from the group.</summary>
    public void DeleteFromGroup(int id) => Execute(Command.GroupDelete, Arg(id));

    /// <summary>Deletes a file set from the group.</summary>
    public Task DeleteFromGroupAsync(int id, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.GroupDelete, cancellationToken, Arg(id));

    #endregion

    #region Loaded

    /// <summary>Returns the number of loaded file sets.</summary>
    public long GetLoadedCount() => SendLongCommand(Command.LoadedCount);

    /// <summary>Returns the number of loaded file sets.</summary>
    public Task<long> GetLoadedCountAsync(CancellationToken cancellationToken = default) =>
        SendLongCommandAsync(Command.LoadedCount, null, cancellationToken);

    /// <summary>Lists loaded file sets from <paramref name="startIndex"/> to <paramref name="endIndex"/>. Indexes start at 1 and both ends are included; a range outside the list throws with <see cref="StatusCode.ErrorRangeExceeded"/>.</summary>
    public IReadOnlyList<GroupFileSet> ListLoaded(int startIndex, int endIndex) =>
        List(Command.LoadedList, s => new GroupFileSet(s), Arg(startIndex), Arg(endIndex));

    /// <summary>Lists loaded file sets from <paramref name="startIndex"/> to <paramref name="endIndex"/>. Indexes start at 1 and both ends are included; a range outside the list throws with <see cref="StatusCode.ErrorRangeExceeded"/>.</summary>
    public Task<IReadOnlyList<GroupFileSet>> ListLoadedAsync(int startIndex, int endIndex, CancellationToken cancellationToken = default) =>
        ListAsync(Command.LoadedList, s => new GroupFileSet(s), cancellationToken, Arg(startIndex), Arg(endIndex));

    /// <summary>Sorts the loaded file sets.</summary>
    public void SortLoaded(int column, bool ascending) => Execute(Command.LoadedSort, Arg(column), Flag(ascending));

    /// <summary>Sorts the loaded file sets.</summary>
    public Task SortLoadedAsync(int column, bool ascending, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.LoadedSort, cancellationToken, Arg(column), Flag(ascending));

    /// <summary>Clears the loaded file sets.</summary>
    public void ClearLoaded() => Execute(Command.LoadedClear);

    /// <summary>Clears the loaded file sets.</summary>
    public Task ClearLoadedAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.LoadedClear, cancellationToken);

    /// <summary>Finds loaded file sets matching a text.</summary>
    public IReadOnlyList<GroupFileSet> FindLoaded(string text) =>
        List(Command.LoadedFind, s => new GroupFileSet(s), text);

    /// <summary>Finds loaded file sets matching a text.</summary>
    public Task<IReadOnlyList<GroupFileSet>> FindLoadedAsync(string text, CancellationToken cancellationToken = default) =>
        ListAsync(Command.LoadedFind, s => new GroupFileSet(s), cancellationToken, text);

    /// <summary>Queues a loaded file set for download.</summary>
    public void DownloadFromLoaded(int id) => Execute(Command.LoadedDownload, Arg(id));

    /// <summary>Queues a loaded file set for download.</summary>
    public Task DownloadFromLoadedAsync(int id, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.LoadedDownload, cancellationToken, Arg(id));

    /// <summary>Creates an NZB from a range of loaded file sets.</summary>
    public void GetNzbFromLoaded(int minIndex, int maxIndex) => Execute(Command.LoadedGetNzb, Arg(minIndex), Arg(maxIndex));

    /// <summary>Creates an NZB from a range of loaded file sets.</summary>
    public Task GetNzbFromLoadedAsync(int minIndex, int maxIndex, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.LoadedGetNzb, cancellationToken, Arg(minIndex), Arg(maxIndex));

    #endregion

    #region Downloads

    /// <summary>Returns the number of entries in the download list.</summary>
    public long GetDownloadsCount() => SendLongCommand(Command.DownloadsCount);

    /// <summary>Returns the number of entries in the download list.</summary>
    public Task<long> GetDownloadsCountAsync(CancellationToken cancellationToken = default) =>
        SendLongCommandAsync(Command.DownloadsCount, null, cancellationToken);

    /// <summary>Locks the download list so indexes stay stable while you page through it.</summary>
    public void LockDownloads() => Execute(Command.DownloadsLock);

    /// <summary>Locks the download list so indexes stay stable while you page through it.</summary>
    public Task LockDownloadsAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.DownloadsLock, cancellationToken);

    /// <summary>Unlocks the download list.</summary>
    public void UnlockDownloads() => Execute(Command.DownloadsUnlock);

    /// <summary>Unlocks the download list.</summary>
    public Task UnlockDownloadsAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.DownloadsUnlock, cancellationToken);

    /// <summary>Lists download entries from <paramref name="startIndex"/> to <paramref name="endIndex"/>. Indexes start at 1 and both ends are included; a range outside the list throws with <see cref="StatusCode.ErrorRangeExceeded"/>.</summary>
    public IReadOnlyList<DownloadFileSet> GetDownloadsList(int startIndex, int endIndex) =>
        List(Command.DownloadsList, s => new DownloadFileSet(s), Arg(startIndex), Arg(endIndex));

    /// <summary>Lists download entries from <paramref name="startIndex"/> to <paramref name="endIndex"/>. Indexes start at 1 and both ends are included; a range outside the list throws with <see cref="StatusCode.ErrorRangeExceeded"/>.</summary>
    public Task<IReadOnlyList<DownloadFileSet>> GetDownloadsListAsync(int startIndex, int endIndex, CancellationToken cancellationToken = default) =>
        ListAsync(Command.DownloadsList, s => new DownloadFileSet(s), cancellationToken, Arg(startIndex), Arg(endIndex));

    /// <summary>Sorts the download list.</summary>
    public void SortDownloadsList(int column, bool ascending) => Execute(Command.DownloadsSort, Arg(column), Flag(ascending));

    /// <summary>Sorts the download list.</summary>
    public Task SortDownloadsListAsync(int column, bool ascending, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.DownloadsSort, cancellationToken, Arg(column), Flag(ascending));

    /// <summary>Clears the download list.</summary>
    public void ClearDownloadList() => Execute(Command.DownloadsClear);

    /// <summary>Clears the download list.</summary>
    public Task ClearDownloadListAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.DownloadsClear, cancellationToken);

    /// <summary>Finds entries in the download list.</summary>
    public void FindInDownloadList(string text) => Execute(Command.DownloadsFind, text);

    /// <summary>Finds entries in the download list.</summary>
    public Task FindInDownloadListAsync(string text, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.DownloadsFind, cancellationToken, text);

    /// <summary>Assembles a download.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Assumed to work like <see cref="DeleteDownload"/>; not verified.
    /// </param>
    public void AssembleDownload(int index) => Execute(Command.DownloadsAssemble, Arg(index));

    /// <summary>Assembles a download.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Assumed to work like <see cref="DeleteDownload"/>; not verified.
    /// </param>
    /// <param name="cancellationToken">Cancels the call and closes the connection.</param>
    public Task AssembleDownloadAsync(int index, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.DownloadsAssemble, cancellationToken, Arg(index));

    /// <summary>Deletes a download.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Verified against Newsbin 6.91.
    /// </param>
    public void DeleteDownload(int index) => Execute(Command.DownloadsDelete, Arg(index));

    /// <summary>Deletes a download.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Verified against Newsbin 6.91.
    /// </param>
    /// <param name="cancellationToken">Cancels the call and closes the connection.</param>
    public Task DeleteDownloadAsync(int index, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.DownloadsDelete, cancellationToken, Arg(index));

    /// <summary>Pauses a download.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Assumed to work like <see cref="DeleteDownload"/>; not verified.
    /// </param>
    public void PauseDownload(int index) => Execute(Command.DownloadsPause, Arg(index));

    /// <summary>Pauses a download.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Assumed to work like <see cref="DeleteDownload"/>; not verified.
    /// </param>
    /// <param name="cancellationToken">Cancels the call and closes the connection.</param>
    public Task PauseDownloadAsync(int index, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.DownloadsPause, cancellationToken, Arg(index));

    /// <summary>Resumes a download.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Assumed to work like <see cref="DeleteDownload"/>; not verified.
    /// </param>
    public void ResumeDownload(int index) => Execute(Command.DownloadsResume, Arg(index));

    /// <summary>Resumes a download.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Assumed to work like <see cref="DeleteDownload"/>; not verified.
    /// </param>
    /// <param name="cancellationToken">Cancels the call and closes the connection.</param>
    public Task ResumeDownloadAsync(int index, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.DownloadsResume, cancellationToken, Arg(index));

    /// <summary>Moves a download one step up.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Assumed to work like <see cref="DeleteDownload"/>; not verified.
    /// </param>
    public void DownloadMoveUp(int index) => Execute(Command.DownloadsMoveUp, Arg(index));

    /// <summary>Moves a download one step up.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Assumed to work like <see cref="DeleteDownload"/>; not verified.
    /// </param>
    /// <param name="cancellationToken">Cancels the call and closes the connection.</param>
    public Task DownloadMoveUpAsync(int index, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.DownloadsMoveUp, cancellationToken, Arg(index));

    /// <summary>Moves a download one step down.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Assumed to work like <see cref="DeleteDownload"/>; not verified.
    /// </param>
    public void DownloadMoveDown(int index) => Execute(Command.DownloadsMoveDown, Arg(index));

    /// <summary>Moves a download one step down.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Assumed to work like <see cref="DeleteDownload"/>; not verified.
    /// </param>
    /// <param name="cancellationToken">Cancels the call and closes the connection.</param>
    public Task DownloadMoveDownAsync(int index, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.DownloadsMoveDown, cancellationToken, Arg(index));

    /// <summary>Moves a download to the top.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Assumed to work like <see cref="DeleteDownload"/>; not verified.
    /// </param>
    public void DownloadMoveTop(int index) => Execute(Command.DownloadsMoveTop, Arg(index));

    /// <summary>Moves a download to the top.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Assumed to work like <see cref="DeleteDownload"/>; not verified.
    /// </param>
    /// <param name="cancellationToken">Cancels the call and closes the connection.</param>
    public Task DownloadMoveTopAsync(int index, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.DownloadsMoveTop, cancellationToken, Arg(index));

    /// <summary>Moves a download to the bottom.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Assumed to work like <see cref="DeleteDownload"/>; not verified.
    /// </param>
    public void DownloadMoveBottom(int index) => Execute(Command.DownloadsMoveBottom, Arg(index));

    /// <summary>Moves a download to the bottom.</summary>
    /// <param name="index">
    /// Zero-based position in the download list: the first entry is 0, although <see cref="GetDownloadsList"/> counts
    /// from 1. This is not <see cref="DownloadFileSet.Id"/>. Assumed to work like <see cref="DeleteDownload"/>; not verified.
    /// </param>
    /// <param name="cancellationToken">Cancels the call and closes the connection.</param>
    public Task DownloadMoveBottomAsync(int index, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.DownloadsMoveBottom, cancellationToken, Arg(index));

    #endregion

    #region Files

    /// <summary>Returns the number of files on the Files tab.</summary>
    public long GetFilesCount() => SendLongCommand(Command.FilesCount);

    /// <summary>Returns the number of files on the Files tab.</summary>
    public Task<long> GetFilesCountAsync(CancellationToken cancellationToken = default) =>
        SendLongCommandAsync(Command.FilesCount, null, cancellationToken);

    /// <summary>Lists files on the Files tab from <paramref name="startIndex"/> to <paramref name="endIndex"/>. Indexes start at 1 and both ends are included; a range outside the list throws with <see cref="StatusCode.ErrorRangeExceeded"/>.</summary>
    public IReadOnlyList<FilesFileSet> GetFilesList(int startIndex, int endIndex) =>
        List(Command.FilesList, s => new FilesFileSet(s), Arg(startIndex), Arg(endIndex));

    /// <summary>Lists files on the Files tab from <paramref name="startIndex"/> to <paramref name="endIndex"/>. Indexes start at 1 and both ends are included; a range outside the list throws with <see cref="StatusCode.ErrorRangeExceeded"/>.</summary>
    public Task<IReadOnlyList<FilesFileSet>> GetFilesListAsync(int startIndex, int endIndex, CancellationToken cancellationToken = default) =>
        ListAsync(Command.FilesList, s => new FilesFileSet(s), cancellationToken, Arg(startIndex), Arg(endIndex));

    /// <summary>Downloads a file from the Files tab to a local file. The local file is deleted if the download fails.</summary>
    /// <param name="fileName">The local file to create or overwrite.</param>
    /// <param name="id">The id of the file, see <see cref="FilesFileSet.Id"/>.</param>
    /// <returns>The file name and size announced by Newsbin.</returns>
    public FilesDownloadItem DownloadFromFiles(string fileName, int id)
    {
        try
        {
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                return DownloadFromFiles(stream, id);
        }
        catch
        {
            TryDelete(fileName);
            throw;
        }
    }

    /// <inheritdoc cref="DownloadFromFiles(string, int)"/>
    /// <param name="fileName">The local file to create or overwrite.</param>
    /// <param name="id">The id of the file, see <see cref="FilesFileSet.Id"/>.</param>
    /// <param name="cancellationToken">Cancels the download and closes the connection.</param>
    public async Task<FilesDownloadItem> DownloadFromFilesAsync(string fileName, int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, useAsync: true))
                return await DownloadFromFilesAsync(stream, id, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
        catch
        {
            TryDelete(fileName);
            throw;
        }
    }

    /// <summary>Downloads a file from the Files tab into a stream.</summary>
    /// <remarks>Newsbin 6.91 answered 405 to every <c>FILES DOWNLOAD</c> argument tried (index, id, ranges), so this hasn't been verified against a live server.</remarks>
    /// <param name="stream">The stream to write the file to.</param>
    /// <param name="id">The id of the file, see <see cref="FilesFileSet.Id"/>.</param>
    /// <returns>The file name and size announced by Newsbin.</returns>
    public FilesDownloadItem DownloadFromFiles(Stream stream, int id)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        Response response = SendCommand(Command.FilesDownload, Arg(id));
        FilesDownloadItem item = GetDownloadItem(response);
        SendCommandWithoutResponse(Command.Continue);
        CopyExactly(Network, stream, item.FileSize);
        EnsureDownloadComplete(ReadResponse());
        return item;
    }

    /// <inheritdoc cref="DownloadFromFiles(Stream, int)"/>
    /// <param name="stream">The stream to write the file to.</param>
    /// <param name="id">The id of the file, see <see cref="FilesFileSet.Id"/>.</param>
    /// <param name="cancellationToken">Cancels the download and closes the connection.</param>
    public async Task<FilesDownloadItem> DownloadFromFilesAsync(Stream stream, int id, CancellationToken cancellationToken = default)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        Response response = await SendCommandAsync(Command.FilesDownload, new[] { Arg(id) }, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        FilesDownloadItem item = GetDownloadItem(response);
        await SendCommandWithoutResponseAsync(Command.Continue, null, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        await CopyExactlyAsync(Network, stream, item.FileSize, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        EnsureDownloadComplete(await ReadResponseAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext));
        return item;
    }

    private static FilesDownloadItem GetDownloadItem(Response response)
    {
        if (response.StatusCode != StatusCode.OK)
            throw new CommunicationException("Download error", Command.FilesDownload, response);
        return new FilesDownloadItem(response.Message);
    }

    private static void EnsureDownloadComplete(Response response)
    {
        if (response.StatusCode != StatusCode.OK)
            throw new CommunicationException("Download error", Command.FilesDownload, response);
    }

    private static void TryDelete(string fileName)
    {
        try
        {
            File.Delete(fileName);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
        }
    }

    #endregion

    #region Wish list

    /// <summary>Returns the number of entries in the wish list.</summary>
    public long GetWishCount() => SendLongCommand(Command.WishCount);

    /// <summary>Returns the number of entries in the wish list.</summary>
    public Task<long> GetWishCountAsync(CancellationToken cancellationToken = default) =>
        SendLongCommandAsync(Command.WishCount, null, cancellationToken);

    /// <summary>Lists wish list entries from <paramref name="startIndex"/> to <paramref name="endIndex"/>. Indexes start at 1 and both ends are included; a range outside the list throws with <see cref="StatusCode.ErrorRangeExceeded"/>.</summary>
    public IReadOnlyList<WishFileSet> GetWishList(int startIndex, int endIndex) =>
        List(Command.WishList, s => new WishFileSet(s), Arg(startIndex), Arg(endIndex));

    /// <summary>Lists wish list entries from <paramref name="startIndex"/> to <paramref name="endIndex"/>. Indexes start at 1 and both ends are included; a range outside the list throws with <see cref="StatusCode.ErrorRangeExceeded"/>.</summary>
    public Task<IReadOnlyList<WishFileSet>> GetWishListAsync(int startIndex, int endIndex, CancellationToken cancellationToken = default) =>
        ListAsync(Command.WishList, s => new WishFileSet(s), cancellationToken, Arg(startIndex), Arg(endIndex));

    /// <summary>Queues a wish list entry for download.</summary>
    public void DownloadFromWish(int id) => Execute(Command.WishDownload, Arg(id));

    /// <summary>Queues a wish list entry for download.</summary>
    public Task DownloadFromWishAsync(int id, CancellationToken cancellationToken = default) =>
        ExecuteAsync(Command.WishDownload, cancellationToken, Arg(id));

    #endregion

    #region NZB upload

    /// <summary>Uploads a local NZB file to Newsbin.</summary>
    /// <param name="fileName">Path to the NZB file.</param>
    public void UploadNzb(string fileName)
    {
        using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            UploadNzb(Path.GetFileName(fileName), stream, stream.Length);
    }

    /// <summary>Uploads a local NZB file to Newsbin.</summary>
    /// <param name="fileName">Path to the NZB file.</param>
    /// <param name="cancellationToken">Cancels the upload and closes the connection.</param>
    public async Task UploadNzbAsync(string fileName, CancellationToken cancellationToken = default)
    {
        using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true))
            await UploadNzbAsync(Path.GetFileName(fileName), stream, stream.Length, cancellationToken).ConfigureAwait(continueOnCapturedContext);
    }

    /// <summary>Uploads NZB data from a stream.</summary>
    /// <param name="fileName">The file name Newsbin should use.</param>
    /// <param name="stream">The NZB data.</param>
    /// <param name="size">Number of bytes to read from <paramref name="stream"/>.</param>
    public void UploadNzb(string fileName, Stream stream, long size)
    {
        ValidateUpload(fileName, stream, size);
        Response response = SendCommand(Command.UploadNzbBinary, fileName, Arg(size));
        if (response.StatusCode == StatusCode.StatusPostData)
        {
            CopyExactly(stream, Network, size);
            response = ReadResponse();
        }
        EnsureUploaded(response);
    }

    /// <inheritdoc cref="UploadNzb(string, Stream, long)"/>
    /// <param name="fileName">The file name Newsbin should use.</param>
    /// <param name="stream">The NZB data.</param>
    /// <param name="size">Number of bytes to read from <paramref name="stream"/>.</param>
    /// <param name="cancellationToken">Cancels the upload and closes the connection.</param>
    public async Task UploadNzbAsync(string fileName, Stream stream, long size, CancellationToken cancellationToken = default)
    {
        ValidateUpload(fileName, stream, size);
        Response response = await SendCommandAsync(Command.UploadNzbBinary, new[] { fileName, Arg(size) }, cancellationToken)
            .ConfigureAwait(continueOnCapturedContext);
        if (response.StatusCode == StatusCode.StatusPostData)
        {
            await CopyExactlyAsync(stream, Network, size, cancellationToken).ConfigureAwait(continueOnCapturedContext);
            response = await ReadResponseAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
        EnsureUploaded(response);
    }

    private static void ValidateUpload(string fileName, Stream stream, long size)
    {
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException("A file name is required.", nameof(fileName));
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size), size, "The size can't be negative.");
    }

    private static void EnsureUploaded(Response response)
    {
        if (response.StatusCode != StatusCode.OK)
            throw new CommunicationException("NZB Upload error", Command.UploadNzbBinary, response);
    }

    #endregion
}
