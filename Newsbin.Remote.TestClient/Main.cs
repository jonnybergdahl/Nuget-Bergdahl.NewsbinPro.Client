using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using Newsbin.Remote;

namespace Newsbin.Remote.TestClient
{
    public partial class Main : Form
    {
        NewsbinClient client = null;

        public Main()
        {
            InitializeComponent();            
            lbCommands.Items.AddRange(Enum.GetNames(typeof(Command)));
            lbCommands.Items.Remove(Command.None.ToString());
            lbCommands.Items.Remove(Command.Auth.ToString());
            lbCommands.Items.Remove(Command.Quit.ToString());
        }

        void ClientSentCommand(object sender, ClientSentCommandEventArgs e)
        {
            Log("-> (" + e.ThreadId.ToString() + ") " + e.Command);
        }

        void ClientRecievedListItem(object sender, ClientReceivedListItemEventArgs e)
        {
            Log(" - (" + e.ThreadId.ToString() + ") " + e.ListItem);
        }

        void ClientReceivedResponse(object sender, ClientReceivedResponseEventArgs e)
        {
            Log("<- (" + e.ThreadId.ToString() + ") " + e.Response);
        }

        void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
			if (bExuteCommand.InvokeRequired)
			{
				bExuteCommand.Invoke(new Action<object, ClientDisconnectedEventArgs>(ClientDisconnected), sender, e);
				return;
			}
            Log("Client disconnected");
            bExuteCommand.Enabled = false;
            bConnect.Enabled = true;
            bDisconnect.Enabled = false;
        }

        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
			if (bExuteCommand.InvokeRequired)
			{
				bExuteCommand.Invoke(new Action<object, ClientConnectedEventArgs>(ClientConnected), sender, e);
				return;
			}
			Log("Client connected");
            bExuteCommand.Enabled = true;
            bConnect.Enabled = false;
            bDisconnect.Enabled = true;
        }

        protected void Log(string logLine)
        {
			if (lbLog.InvokeRequired)
			{
				lbLog.Invoke(new Action<string>(Log), logLine);
				return;
			}
			lbLog.Items.Add(logLine);
			ScrollToLastLine(lbLog);
		}

        protected void ScrollToLastLine(ListBox listBox)
        {
            int numberOfVisibleItems = listBox.ClientSize.Height / listBox.ItemHeight;

            if (listBox.TopIndex == listBox.Items.Count - numberOfVisibleItems - 1)
            {
                listBox.TopIndex = listBox.Items.Count - numberOfVisibleItems + 1;
            } 
        }

        private void lbCommands_SelectedIndexChanged(object sender, EventArgs e)
        {
            Command command = (Command)Enum.Parse(typeof(Command), lbCommands.SelectedItem.ToString());
            int arguments = 0;

            switch (command)
            {
                case Command.Auth:
                    arguments = 1;
                    lArgument1.Text = "Password hash";
                    break;
                case Command.Bwl:
                    arguments = 1;
                    lArgument1.Text = "State (None/0/1)";
                    break;
                case Command.Pause:
                    arguments = 1;
                    lArgument1.Text = "State (None/0/1)";
                    break;
                case Command.ListChildren:
                case Command.ListChildrenDetails:
                    arguments = 1;
                    lArgument1.Text = "GoG name";
                    break;
                case Command.FilterSetMinSize:
                case Command.FilterSetMaxSize:
                    arguments = 1;
                    lArgument1.Text = "Size";
                    break;
                case Command.FilterAddGroupFilter:
                    arguments = 1;
                    lArgument1.Text = "Group name";
                    break;
                case Command.FilterSetAge:
                    arguments = 1;
                    lArgument1.Text = "Age";
                    break;
                case Command.FilterSetTextFilter:
                case Command.DownloadsFind:
                    arguments = 1;
                    lArgument1.Text = "Search string";
                    break;
                case Command.FilterHideOld:
                case Command.FilterEnableFilters:
                    arguments = 1;
                    lArgument1.Text = "State (0/1)";
                    break;
                case Command.FilterMarkOld:
                case Command.GroupDelete:
                case Command.LoadedDownload:
                case Command.DownloadsAssemble:
                case Command.DownloadsDelete:
                case Command.DownloadsPause:
                case Command.DownloadsResume:
                case Command.DownloadsMoveUp:
                case Command.DownloadsMoveDown:
                case Command.DownloadsMoveTop:
                case Command.DownloadsMoveBottom:
                case Command.WishDownload:
                    arguments = 1;
                    lArgument1.Text = "Set id";
                    break;
                case Command.FilesDownload:
                    arguments = 2;
                    lArgument1.Text = "File name";
                    lArgument2.Text = "Set id";
                    break;
                case Command.FilterSetProfile:
                    arguments = 1;
                    lArgument1.Text = "Filter name";
                    break;
                case Command.SearchLocal:
                case Command.SearchInternet:
                    arguments = 3;
                    lArgument1.Text = "Min age";
                    lArgument2.Text = "Max age";
                    lArgument3.Text = "Search string";
                    break;
                case Command.GroupLoad:
                    arguments = 2;
                    lArgument1.Text = "Min age";
                    lArgument2.Text = "Max age";
                    break;
                case Command.LoadedList:
                case Command.DownloadsList:
                case Command.FilesList:
                case Command.WishList:
                    arguments = 2;
                    lArgument1.Text = "Start index";
                    lArgument2.Text = "End index";
                    break;
                case Command.LoadedFind:
                    arguments = 1;
                    lArgument1.Text = "Search string";
                    break;
                case Command.LoadedSort:
                case Command.DownloadsSort:
                    arguments = 2;
                    lArgument1.Text = "Column";
                    lArgument2.Text = "Ascending (0/1)";
                    break;
                case Command.LoadedGetNzb:
                    arguments = 2;
                    lArgument1.Text = "Min index";
                    lArgument2.Text = "Max index";
                    break;
                case Command.UploadNzbBinary:
                    arguments = 1;
                    lArgument1.Text = "NZB File Name";
                    break;
            }
            lArgument1.Visible = arguments > 0;
            tbArgument1.Visible = arguments > 0;
            lArgument2.Visible = arguments > 1;
            tbArgument2.Visible = arguments > 1;
            lArgument3.Visible = arguments > 2;
            tbArgument3.Visible = arguments > 2;
        }

        private async void bExuteCommand_Click(object sender, EventArgs e)
        {
            bool bResult;
            long lResult;
            IEnumerable<string> listResult;
            lbResponse.Items.Clear();
            Command command = (Command)Enum.Parse(typeof(Command), lbCommands.SelectedItem.ToString());
            switch (command)
            {
                case Command.Status:
                    NewsbinStatus status;
					if (cbUseAsync.Checked)
						status = await client.GetStatusAsync();
					else
						status = client.GetStatus();
                    lbResponse.Items.Add("DataFolderFreeSpace: " + status.DataFolderFreeSpace.ToString());
                    lbResponse.Items.Add("DownloadFolderFreeSpace: " + status.DownloadFolderFreeSpace.ToString());
                    lbResponse.Items.Add("QueuedFileSize: " + status.QueuedFileSize.ToString());
                    lbResponse.Items.Add("Speed: " + status.Speed.ToString());
                    for (int i=0; i < status.SpeedSamples.Count; i++)
                    {
                        lbResponse.Items.Add(" " + i.ToString() + ": " + status.SpeedSamples[i].ToString());
                    }                       
                    break;
                case Command.Bwl:
					if (tbArgument1.Text == "")
					{
						if (cbUseAsync.Checked)
							bResult = await client.GetBandwidthLimiterStateAsync();
						else
							bResult = client.GetBandwidthLimiterState();
					}
					else
					{
						if (cbUseAsync.Checked)
							bResult = await client.SetBandwidthLimiterStateAsync(GetArgument1AsBool());
						else
							bResult = client.SetBandwidthLimiterState(GetArgument1AsBool());
					}
                    lbResponse.Items.Add(bResult.ToString());
                    break;
                case Command.Pause:
					if (tbArgument1.Text == "")
					{
						if (cbUseAsync.Checked)
							bResult = await client.GetPausedStateAsync();
						else
							bResult = client.GetPausedState();
					}
					else
					{
						if (cbUseAsync.Checked)
							bResult = await client.SetPausedStateAsync(GetArgument1AsBool());
						else
							bResult = client.SetPausedState(GetArgument1AsBool());
					}
                    lbResponse.Items.Add(bResult.ToString());
                    break;
                case Command.ListServers:
					if (cbUseAsync.Checked)
						listResult = await client.ListServersAsync();
					else
						listResult = client.ListServers();
                    foreach (string item in listResult)
                        lbResponse.Items.Add(item);
                    break;
                case Command.ListGogs:
					if (cbUseAsync.Checked)
						listResult = await client.ListGogsAsync();
					else
						listResult = client.ListGogs();
                    foreach (string item in listResult)
                        lbResponse.Items.Add(item);
                    break;
                case Command.ListGroups:
					if (cbUseAsync.Checked)
						listResult = await client.ListGroupsAsync();
					else
						listResult = client.ListGroups();
                    foreach (string item in listResult)
                        lbResponse.Items.Add(item);
                    break;
                case Command.ListChildren:
					if (cbUseAsync.Checked)
						listResult = await client.ListChildrenAsync(tbArgument1.Text);
					else
						listResult = client.ListChildren(tbArgument1.Text);
                    foreach (string item in listResult)
                        lbResponse.Items.Add(item);
                    break;
                case Command.ListChildrenDetails:
					if (cbUseAsync.Checked)
						listResult = await client.ListChildrenDetailsAsync(tbArgument1.Text);
					else
						listResult = client.ListChildrenDetails(tbArgument1.Text);
                    foreach (string item in listResult)
                        lbResponse.Items.Add(item);
                    break;        
                case Command.ListFilters:
					if (cbUseAsync.Checked)
						listResult = await client.ListFiltersAsync();
					else
						listResult = client.ListFilters();
                    foreach (string item in listResult)
                        lbResponse.Items.Add(item);
                    break;
                case Command.FilterSetMinSize:
					if (cbUseAsync.Checked)
						await client.SetFilterMinSizeAsync(GetArgument1AsLong());
					else
						client.SetFilterMinSize(GetArgument1AsLong());
                    break;
                case Command.FilterSetMaxSize:
					if (cbUseAsync.Checked)
						await client.SetFilterMaxSizeAsync(GetArgument1AsLong());
					else
						client.SetFilterMaxSize(GetArgument1AsLong());
                    break;
                case Command.FilterAddGroupFilter:
					if (cbUseAsync.Checked)
						await client.AddGroupFilterAsync(tbArgument1.Text);
					else
						client.AddGroupFilter(tbArgument1.Text);
                    break;
                case Command.FilterSetAge:
                    if (cbUseAsync.Checked)
						await client.SetFilterAgeAsync(GetArgument1AsLong());
					else
						client.SetFilterAge(GetArgument1AsLong());
                    break;
                case Command.FilterSetTextFilter:
					if (cbUseAsync.Checked)
						await client.SetTextFilterAsync(tbArgument1.Text);
					else
						client.SetTextFilter(tbArgument1.Text);
                    break;
                case Command.FilterHideOld:
					if (cbUseAsync.Checked)
						await client.HideOldAsync(GetArgument1AsBool());
					else
						client.HideOld(GetArgument1AsBool());
                    break;
                case Command.FilterEnableFilters:
					if (cbUseAsync.Checked)
						await client.EnableFiltersAsync(GetArgument1AsBool());
					else
						client.EnableFilters(GetArgument1AsBool());
                    break;
                case Command.FilterMarkOld:
					if (cbUseAsync.Checked)
						await client.MarkOldAsync(GetArgument1AsInt());
					else
						client.MarkOld(GetArgument1AsInt());
                    break;
                case Command.FilterMarkAllOld:
					if (cbUseAsync.Checked)
						await client.MarkAllOldAsync();
					else
						client.MarkAllOld();
                    break;
                case Command.FilterSetProfile:
					if (cbUseAsync.Checked)
						await client.SetFilterProfileAsync(tbArgument1.Text);
					else
						client.SetFilterProfile(tbArgument1.Text);
                    break;
                case Command.FilterClear:
					if (cbUseAsync.Checked)
						await client.ClearFiltersAsync();
					else
						client.ClearFilters();
                    break;
                case Command.SearchLocal:
					if (cbUseAsync.Checked)
						await client.SearchLocalAsync(GetArgument1AsInt(), GetArgument2AsInt(), tbArgument3.Text);
					else
						client.SearchLocal(GetArgument1AsInt(), GetArgument2AsInt(), tbArgument3.Text);
                    break;
                case Command.SearchInternet:
					if (cbUseAsync.Checked)
						await client.SearchInternetAsync(GetArgument1AsInt(), GetArgument2AsInt(), tbArgument3.Text);
					else
						client.SearchInternet(GetArgument1AsInt(), GetArgument2AsInt(), tbArgument3.Text);
                    break;
                case Command.GroupLoad:
					if (cbUseAsync.Checked)
						lResult = await client.LoadGroupAsync(GetArgument1AsInt(), GetArgument2AsInt());
					else
						lResult = client.LoadGroup(GetArgument1AsInt(), GetArgument2AsInt());
                    lbResponse.Items.Add(lResult.ToString());
                    break;
                case Command.GroupClear:
					if (cbUseAsync.Checked)
						await client.ClearGroupAsync();
					else
						client.ClearGroup();
                    break;
                case Command.GroupDelete:
					if (cbUseAsync.Checked)
						await client.DeleteFromGroupAsync(GetArgument1AsInt());
					else
						client.DeleteFromGroup(GetArgument1AsInt());
                    break;
                case Command.LoadedCount:
					if (cbUseAsync.Checked)
						lResult = await client.GetLoadedCountAsync();
					else
						lResult = client.GetLoadedCount();
                    lbResponse.Items.Add(lResult.ToString());
                    break;
                case Command.LoadedList:
					IEnumerable<GroupFileSet> groupFileSet;
					if (cbUseAsync.Checked)
						groupFileSet = await client.ListLoadedAsync(GetArgument1AsInt(), GetArgument2AsInt());
					else
						groupFileSet = client.ListLoaded(GetArgument1AsInt(), GetArgument2AsInt());
                    foreach (GroupFileSet item in groupFileSet)
                    {
                        lbResponse.Items.Add(item.ToString());
                    }
                    break;
                case Command.LoadedSort:
					if (cbUseAsync.Checked)
						await client.SortLoadedAsync(GetArgument1AsInt(), GetArgument2AsBool());
					else
						client.SortLoaded(GetArgument1AsInt(), GetArgument2AsBool());
                    break;
                case Command.LoadedClear:
					if (cbUseAsync.Checked)
	                    await client.ClearLoadedAsync();
					else
						client.ClearLoaded();
                    break;
                case Command.LoadedFind:
					if (cbUseAsync.Checked)
						await client.FindLoadedAsync(tbArgument1.Text);
					else
						client.FindLoaded(tbArgument1.Text);
                    break;
                case Command.LoadedDownload:
					if (cbUseAsync.Checked)
						await client.DownloadFromLoadedAsync(GetArgument1AsInt());
					else
						client.DownloadFromLoaded(GetArgument1AsInt());
                    break;
                case Command.LoadedGetNzb:
					if (cbUseAsync.Checked)
						await client.GetNzbFromLoadedAsync(GetArgument1AsInt(), GetArgument2AsInt());
					else
						client.GetNzbFromLoaded(GetArgument1AsInt(), GetArgument2AsInt());
                    break;
                case Command.DownloadsCount:
					if (cbUseAsync.Checked)
						lResult = await client.GetDownloadsCountAsync();
					else
						lResult = client.GetDownloadsCount();
                    lbResponse.Items.Add(lResult.ToString());
                    break;
                case Command.DownloadsLock:
					if (cbUseAsync.Checked)
						await client.LockDownloadsAsync();
					else
						client.LockDownloads();
                    break;
                case Command.DownloadsUnlock:
					if (cbUseAsync.Checked)
						await client.UnlockDownloadsAsync();
					else
						client.UnlockDownloads();
                    break;
                case Command.DownloadsList:
					IEnumerable<DownloadFileSet> downloads;
					if (cbUseAsync.Checked)
						downloads = await client.GetDownloadsListAsync(GetArgument1AsInt(), GetArgument2AsInt());
					else
						downloads = client.GetDownloadsList(GetArgument1AsInt(), GetArgument2AsInt());
                    foreach (DownloadFileSet item in downloads)
                        lbResponse.Items.Add(item.ToString());
                    break;
                case Command.DownloadsSort:
					if (cbUseAsync.Checked)
						await client.SortDownloadsListAsync(GetArgument1AsInt(), GetArgument2AsBool());
					else
						client.SortDownloadsList(GetArgument1AsInt(), GetArgument2AsBool());
                    break;
                case Command.DownloadsClear:
					if (cbUseAsync.Checked)
						await client.ClearDownloadListAsync();
					else
						client.ClearDownloadList();
                    break;
                case Command.DownloadsFind:
					if (cbUseAsync.Checked)
						await client.FindInDownloadListAsync(tbArgument1.Text);
					else
						client.FindInDownloadList(tbArgument1.Text);
                    break;
                case Command.DownloadsAssemble:
					if (cbUseAsync.Checked)
						await client.AssembleDownloadAsync(GetArgument1AsInt());
					else
						client.AssembleDownload(GetArgument1AsInt());
                    break;
                case Command.DownloadsDelete:
					if (cbUseAsync.Checked)
						await client.DeleteDownloadAsync(GetArgument1AsInt());
					else
						client.DeleteDownload(GetArgument1AsInt());
                    break;
                case Command.DownloadsPause:
					if (cbUseAsync.Checked)
                    client.PauseDownload(GetArgument1AsInt());
                    break;
                case Command.DownloadsResume:
					if (cbUseAsync.Checked)
						await client.ResumeDownloadAsync(GetArgument1AsInt());
					else
						client.ResumeDownload(GetArgument1AsInt());
                    break;
                case Command.DownloadsMoveUp:
					if (cbUseAsync.Checked)
						await client.DownloadMoveUpAsync(GetArgument1AsInt());
					else
						client.DownloadMoveUp(GetArgument1AsInt());
                    break;
                case Command.DownloadsMoveDown:
					if (cbUseAsync.Checked)
						await client.DownloadMoveDownAsync(GetArgument1AsInt());
					else
						client.DownloadMoveDown(GetArgument1AsInt());
                    break;
                case Command.DownloadsMoveTop:
					if (cbUseAsync.Checked)
						await client.DownloadMoveTopAsync(GetArgument1AsInt());
					else
						client.DownloadMoveTop(GetArgument1AsInt());
                    break;
                case Command.DownloadsMoveBottom:
					if (cbUseAsync.Checked)
						await client.DownloadMoveBottomAsync(GetArgument1AsInt());
					else
						client.DownloadMoveBottom(GetArgument1AsInt());
                    break;
                case Command.FilesCount:
					if (cbUseAsync.Checked)
						lResult = await client.GetFilesCountAsync();
					else
						lResult = client.GetFilesCount();
                    lbResponse.Items.Add(lResult.ToString());
                    break;
                case Command.FilesList:
					IEnumerable<FilesFileSet> fileSets;
					if (cbUseAsync.Checked)
						fileSets = await client.GetFilesListAsync(GetArgument1AsInt(), GetArgument2AsInt());
					else
						fileSets = client.GetFilesList(GetArgument1AsInt(), GetArgument2AsInt());
                    foreach (FilesFileSet item in fileSets)
                        lbResponse.Items.Add(item.ToString());
                    break;
                case Command.FilesDownload:
					if (cbUseAsync.Checked)
						await client.DownloadFromFilesAsync(tbArgument1.Text, GetArgument2AsInt());
					else
						client.DownloadFromFiles(tbArgument1.Text, GetArgument2AsInt());
                    break;
                case Command.WishCount:
					if (cbUseAsync.Checked)
						lResult = await client.GetWishCountAsync();
					else
						lResult = client.GetWishCount();
                    lbResponse.Items.Add(lResult.ToString());
                    break;
                case Command.WishList:
					IEnumerable<WishFileSet> wishSets;
					if (cbUseAsync.Checked)
						wishSets = await client.GetWishListAsync(GetArgument1AsInt(), GetArgument2AsInt());
					else
						wishSets = client.GetWishList(GetArgument1AsInt(), GetArgument2AsInt());
                    foreach (WishFileSet item in wishSets)
                        lbResponse.Items.Add(item.ToString());
                    break;
                case Command.WishDownload:
					if (cbUseAsync.Checked)
						await client.DownloadFromWishAsync(GetArgument1AsInt());
					else
						client.DownloadFromWish(GetArgument1AsInt());
                    break;
                case Command.UploadNzbBinary:
					if (cbUseAsync.Checked)
						await client.UploadNzbAsync(tbArgument1.Text);
					else
						client.UploadNzb(tbArgument1.Text);
                    break;
            }
        }

        private bool GetArgument1AsBool()
        {
            return tbArgument1.Text == "1";
        }

        private bool GetArgument2AsBool()
        {
            return tbArgument1.Text == "1";
        }

        private int GetArgument1AsInt()
        {
            int result;
            if (int.TryParse(tbArgument1.Text, out result))
                return result;
            return 0;
        }

        private long GetArgument1AsLong()
        {
            long result;
            if (long.TryParse(tbArgument1.Text, out result))
                return result;
            return 0;
        }

        private int GetArgument2AsInt()
        {
            int result;
            if (int.TryParse(tbArgument2.Text, out result))
                return result;
            return 0; 
        }

        private long GetArgument2AsLong()
        {
            long result;
            if (long.TryParse(tbArgument2.Text, out result))
                return result;
            return 0;
        }

        private async void bConnect_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.IPAddress = m_IPAddress.Text;
            Properties.Settings.Default.Port = m_Port.Text;
            Properties.Settings.Default.Password = m_Password.Text;
			Properties.Settings.Default.UseAsync = cbUseAsync.Checked;
            Properties.Settings.Default.Save();

            this.client = new NewsbinClient(m_IPAddress.Text, Convert.ToInt32(m_Port.Text), m_Password.Text);
            this.client.ClientConnected += new ClientConnectedEventHandler(ClientConnected);
            this.client.ClientDisconnected += new ClientDisconnectedEventHandler(ClientDisconnected);
            this.client.ClientReceivedResponse += new ClientReceivedResponseEventHandler(ClientReceivedResponse);
            this.client.ClientRecievedListItem += new ClientReceivedListItemEventHandler(ClientRecievedListItem);
            this.client.ClientSentCommand += new ClientSentCommandEventHandler(ClientSentCommand);
			if (cbUseAsync.Checked)
				await this.client.ConnectAsync();
			else
				this.client.Connect();
        }

        private async void bDisconnect_Click(object sender, EventArgs e)
        {
			if (cbUseAsync.Checked)
				await this.client.DisconnectAsync();
			else
				this.client.Disconnect();
        }
    }
}
