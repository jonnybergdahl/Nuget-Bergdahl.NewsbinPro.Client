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
            Log("-> " + e.Command);
        }

        void ClientRecievedListItem(object sender, ClientReceivedListItemEventArgs e)
        {
            Log(" - " + e.ListItem);
        }

        void ClientReceivedResponse(object sender, ClientReceivedResponseEventArgs e)
        {
            Log("<- " + e.Response);
        }

        void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Log("Client disconnected");
            bExuteCommand.Enabled = false;
            bConnect.Enabled = true;
            bDisconnect.Enabled = false;
        }

        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            Log("Client connected");
            bExuteCommand.Enabled = true;
            bConnect.Enabled = false;
            bDisconnect.Enabled = true;
        }

        protected void Log(string logLine)
        {
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
                case Command.LoadedFind:
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
                case Command.FilesDownload:
                case Command.WishDownload:
                    arguments = 1;
                    lArgument1.Text = "Set id";
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
            }
            lArgument1.Visible = arguments > 0;
            tbArgument1.Visible = arguments > 0;
            lArgument2.Visible = arguments > 1;
            tbArgument2.Visible = arguments > 1;
            lArgument3.Visible = arguments > 2;
            tbArgument3.Visible = arguments > 2;
        }

        private void bExuteCommand_Click(object sender, EventArgs e)
        {
            bool bResult;
            long lResult;
            IEnumerable<string> listResult;
            lbResponse.Items.Clear();
            Command command = (Command)Enum.Parse(typeof(Command), lbCommands.SelectedItem.ToString());
            switch (command)
            {
                case Command.Status:
                    NewsbinStatus status = client.GetStatus();
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
                        bResult = client.GetBandwidthLimiterState();
                    else
                        bResult = client.SetBandwidthLimiterState(GetArgument1AsBool());
                    lbResponse.Items.Add(bResult.ToString());
                    break;
                case Command.Pause:
                    if (tbArgument1.Text == "")
                        bResult = client.GetPausedState();
                    else
                        bResult = client.SetPausedState(GetArgument1AsBool());
                    lbResponse.Items.Add(bResult.ToString());
                    break;
                case Command.ListServers:
                    listResult = client.ListServers();
                    foreach (string item in listResult)
                        lbResponse.Items.Add(item);
                    break;
                case Command.ListGogs:
                    listResult = client.ListGogs();
                    foreach (string item in listResult)
                        lbResponse.Items.Add(item);
                    break;
                case Command.ListGroups:
                    listResult = client.ListGroups();
                    foreach (string item in listResult)
                        lbResponse.Items.Add(item);
                    break;
                case Command.ListChildren:
                    listResult = client.ListChildren(tbArgument1.Text);
                    foreach (string item in listResult)
                        lbResponse.Items.Add(item);
                    break;
                case Command.ListChildrenDetails:
                    listResult = client.ListChildrenDetails(tbArgument1.Text);
                    foreach (string item in listResult)
                        lbResponse.Items.Add(item);
                    break;        
                case Command.ListFilters:
                    listResult = client.ListFilters();
                    foreach (string item in listResult)
                        lbResponse.Items.Add(item);
                    break;
                case Command.FilterSetMinSize:
                    client.SetFilterMinSize(GetArgument1AsLong());
                    break;
                case Command.FilterSetMaxSize:
                    client.SetFilterMaxSize(GetArgument1AsLong());
                    break;
                case Command.FilterAddGroupFilter:
                    client.AddGroupFilter(tbArgument1.Text);
                    break;
                case Command.FilterSetAge:
                    client.SetFilterAge(GetArgument1AsLong());
                    break;
                case Command.FilterSetTextFilter:
                    client.SetTextFilter(tbArgument1.Text);
                    break;
                case Command.FilterHideOld:
                    client.HideOld(GetArgument1AsBool());
                    break;
                case Command.FilterEnableFilters:
                    client.EnableFilters(GetArgument1AsBool());
                    break;
                case Command.FilterMarkOld:
                    client.MarkOld(GetArgument1AsInt());
                    break;
                case Command.FilterMarkAllOld:
                    client.MarkAllOld();
                    break;
                case Command.FilterSetProfile:
                    client.SetFilterProfile(tbArgument1.Text);
                    break;
                case Command.SearchLocal:
                    client.SearchLocal(GetArgument1AsInt(), GetArgument2AsInt(), tbArgument3.Text);
                    break;
                case Command.SearchInternet:
                    client.SearchInternet(GetArgument1AsInt(), GetArgument2AsInt(), tbArgument3.Text);
                    break;
                case Command.GroupLoad:
                    lResult = client.LoadGroup(GetArgument1AsInt(), GetArgument2AsInt());
                    lbResponse.Items.Add(lResult.ToString());
                    break;
                case Command.GroupClear:
                    client.ClearGroup();
                    break;
                case Command.GroupDelete:
                    client.DeleteFromGroup(GetArgument1AsInt());
                    break;
                case Command.LoadedCount:
                    lResult = client.GetLoadedCount();
                    lbResponse.Items.Add(lResult.ToString());
                    break;
                case Command.LoadedList:
                    IEnumerable<GroupFileSet> groupFileSet = client.ListLoaded(GetArgument1AsInt(), GetArgument2AsInt());
                    foreach (GroupFileSet item in groupFileSet)
                    {
                        lbResponse.Items.Add(item.ToString());
                    }
                    break;
                case Command.LoadedSort:
                    client.SortLoaded(GetArgument1AsInt(), GetArgument2AsBool());
                    break;
                case Command.LoadedClear:
                    client.ClearLoaded();
                    break;
                case Command.LoadedFind:
                    client.FindLoaded(tbArgument1.Text);
                    break;
                case Command.LoadedDownload:
                    client.DownloadFromLoaded(GetArgument1AsInt());
                    break;
                case Command.LoadedGetNzb:
                    client.GetNzbFromLoaded(GetArgument1AsInt(), GetArgument2AsInt());
                    break;
                case Command.DownloadsCount:
                    lResult = client.GetDownloadsCount();
                    lbResponse.Items.Add(lResult.ToString());
                    break;
                case Command.DownloadsLock:
                    client.LockDownloads();
                    break;
                case Command.DownloadsUnlock:
                    client.UnlockDownloads();
                    break;
                case Command.DownloadsList:
                    IEnumerable<DownloadPartStatus> downloads = client.GetDownloadsList(GetArgument1AsInt(), GetArgument2AsInt());
                    foreach (DownloadPartStatus item in downloads)
                        lbResponse.Items.Add(item.ToString());
                    break;
                case Command.DownloadsSort:
                    client.SortDownloadsList(GetArgument1AsInt(), GetArgument2AsBool());
                    break;
                case Command.DownloadsClear:
                    client.ClearDownloadList();
                    break;
                case Command.DownloadsFind:
                    client.FindInDownloadList(tbArgument1.Text);
                    break;
                case Command.DownloadsAssemble:
                    client.AssembleDownload(GetArgument1AsInt());
                    break;
                case Command.DownloadsDelete:
                    client.DeleteDownload(GetArgument1AsInt());
                    break;
                case Command.DownloadsPause:
                    client.PauseDownload(GetArgument1AsInt());
                    break;
                case Command.DownloadsResume:
                    client.ResumeDownload(GetArgument1AsInt());
                    break;
                case Command.DownloadsMoveUp:
                    client.DownloadMoveUp(GetArgument1AsInt());
                    break;
                case Command.DownloadsMoveDown:
                    client.DownloadMoveDown(GetArgument1AsInt());
                    break;
                case Command.DownloadsMoveTop:
                    client.DownloadMoveTop(GetArgument1AsInt());
                    break;
                case Command.DownloadsMoveBottom:
                    client.DownloadMoveBottom(GetArgument1AsInt());
                    break;
                case Command.FilesCount:
                    lResult = client.GetFilesCount();
                    lbResponse.Items.Add(lResult.ToString());
                    break;
                case Command.FilesList:
                    IEnumerable<FilesFileSet> fileSets = client.GetFilesList(GetArgument1AsInt(), GetArgument2AsInt());
                    foreach (FilesFileSet item in fileSets)
                        lbResponse.Items.Add(item.ToString());
                    break;
                case Command.FilesDownload:
                    client.DownloadFromFiles(GetArgument1AsInt());
                    break;
                case Command.WishCount:
                    lResult = client.GetWishCount();
                    lbResponse.Items.Add(lResult.ToString());
                    break;
                case Command.WishList:                    
                    IEnumerable<FilesFileSet> wishSets = client.GetWishList(GetArgument1AsInt(), GetArgument2AsInt());
                    foreach (FilesFileSet item in wishSets)
                        lbResponse.Items.Add(item.ToString());
                    break;
                case Command.WishDownload:
                    client.DownloadFromWish(GetArgument1AsInt());
                    break;
                case Command.UploadNzbBinary:
                    // Not implemented
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

        private void bConnect_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.IPAddress = m_IPAddress.Text;
            Properties.Settings.Default.Port = m_Port.Text;
            Properties.Settings.Default.Password = m_Password.Text;
            Properties.Settings.Default.Save();

            this.client = new NewsbinClient(m_IPAddress.Text, Convert.ToInt32(m_Port.Text), m_Password.Text);
            this.client.ClientConnected += new ClientConnectedEventHandler(ClientConnected);
            this.client.ClientDisconnected += new ClientDisconnectedEventHandler(ClientDisconnected);
            this.client.ClientReceivedResponse += new ClientReceivedResponseEventHandler(ClientReceivedResponse);
            this.client.ClientRecievedListItem += new ClientReceivedListItemEventHandler(ClientRecievedListItem);
            this.client.ClientSentCommand += new ClientSentCommandEventHandler(ClientSentCommand);
            this.client.Connect();
        }

        private void bDisconnect_Click(object sender, EventArgs e)
        {
            this.client.Disconnect();
        }
    }
}
