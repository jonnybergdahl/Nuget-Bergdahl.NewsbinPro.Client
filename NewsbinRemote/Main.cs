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



namespace NewsbinRemote
{
    public partial class Main : Form
    {
        bool bIsConnected = false;
        RemoteCommand m_Command = new RemoteCommand();

        public Main()
        {
            InitializeComponent();

        }

        private void 
        OnConnect(object sender, EventArgs e)
        {
            IPAddress[] addrs = Dns.GetHostEntry("10.0.1.6").AddressList;

            Properties.Settings.Default.IPAddress = m_IPAddress.Text;
            Properties.Settings.Default.Port = m_Port.Text;
            Properties.Settings.Default.Key = m_Key.Text;
            Properties.Settings.Default.Password = m_Password.Text;

            Properties.Settings.Default.Groups = m_GroupText.Text;
            Properties.Settings.Default.MinAge = m_MinAge.Text;
            Properties.Settings.Default.MaxAge = m_MaxAge.Text;

            Properties.Settings.Default.Save();


            int nResult = m_Command.ConnectServer(m_IPAddress.Text, int.Parse(m_Port.Text));
            StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));
            if (480 == nResult)
            {
                nResult = m_Command.Auth(m_Key.Text, m_Password.Text);
                StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));
                if (nResult < 0)
                {
                    return;
                }
            }
            bIsConnected = true;

            foreach (IPAddress addr in addrs)
            {
                StatusList.Items.Add(new ListViewItem(addr.ToString()));
            }
            
        }

        private void OnTest(object sender, EventArgs e)
        {
            StatusList.Items.Clear();
            if (bIsConnected != true)
            {
                return;
            }
            try
            {
                m_Command.FilterAddGroup("anime/video");
                StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));

                m_Command.FilterSetMinSize(1000000);
                StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));

                m_Command.FilterSetMaxSize(5000000000);
                StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));

                m_Command.LoadGroup(10, 0);
                StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));

                long nCount = m_Command.LoadedCount();
                StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));

                if (nCount > 0)
                {
                    m_Command.LoadedList(1, nCount);
                    StringBuilder Lines = new StringBuilder();

                    int nLength = m_Command.ReadListItem(ref Lines);
                    while (0 != nLength)
                    {
                        StatusList.Items.Add(new ListViewItem(Lines.ToString()));
                        nLength = m_Command.ReadListItem(ref Lines);
                    }
                }


                m_Command.FilterSetTextFilter("gundam");
                StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));

                nCount = m_Command.LoadedCount();
                StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));

                if (nCount > 0)
                {
                    m_Command.LoadedList(1, nCount);
                    StringBuilder Lines = new StringBuilder();

                    int nLength = m_Command.ReadListItem(ref Lines);
                    while (0 != nLength)
                    {
                        StatusList.Items.Add(new ListViewItem(Lines.ToString()));
                        nLength = m_Command.ReadListItem(ref Lines);
                    }
                }

                m_Command.NZBUpload(@"X:\Test NZBs\Deadwood S02 720p BluRay x264 CtrlHD.nzb");

                m_Command.FileCount();
                StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));

//                m_Command.FileRetrieve(1, @"X:\Temp\");

                StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));

            
            }
            catch (SocketException se)
            {
                ListViewItem item1 = new ListViewItem(se.Message);
                StatusList.Items.Add(item1);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void OnUploadNZB(object sender, EventArgs e)
        {
            StatusList.Items.Clear();
            if (bIsConnected != true)
            {
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "NZB files (*.nzb|*.nzb|All files (*.*)|*.*";
            dialog.InitialDirectory = @"X:\";
            dialog.Title = "Select a text file";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                String sFilename = dialog.FileName;
                m_Command.NZBUpload(sFilename);

                int nCount = m_Command.LoadedCount();
                StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));

                if (nCount > 0)
                {
                    m_Command.LoadedList(1, nCount);
                    StringBuilder Lines = new StringBuilder();

                    StatusList.Items.Clear();
                    int nLength = m_Command.ReadListItem(ref Lines);
                    while (0 != nLength)
                    {
                        StatusList.Items.Add(new ListViewItem(Lines.ToString()));
                        nLength = m_Command.ReadListItem(ref Lines);
                    }
                }
            }
        }

        private void OnAutoloadNZB(object sender, EventArgs e)
        {
            StatusList.Items.Clear();
            if (bIsConnected != true)
            {
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "NZB files (*.nzb|*.nzb|All files (*.*)|*.*";
            dialog.InitialDirectory = @"X:\";
            dialog.Title = "Select a text file";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                String sFilename = dialog.FileName;
                m_Command.NZBUploadAutoload(sFilename);
                StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));
            }
        }


        private void OnAddGroup(object sender, EventArgs e)
        {
            Properties.Settings.Default.Groups = m_GroupText.Text;
            Properties.Settings.Default.Save();

            StatusList.Items.Clear();
            if (bIsConnected != true)
            {
                return;
            }
            m_Command.FilterAddGroup(m_GroupText.Text);
            StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));
        }

        private void OnClearGroups(object sender, EventArgs e)
        {
            StatusList.Items.Clear();
            if (bIsConnected != true)
            {
                return;
            }
            m_Command.FilterClearGroups();
            StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));
        }

        private void OnListFiles(object sender, EventArgs e)
        {
            StatusList.Items.Clear();
            int nCount = m_Command.FileCount();
            StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));
            if (nCount > 0)
            {
                m_Command.FileList(1, nCount);
                StringBuilder Lines = new StringBuilder();


                int nLength = m_Command.ReadListItem(ref Lines);
                while (0 != nLength)
                {
                    StatusList.Items.Add(new ListViewItem(Lines.ToString()));
                    nLength = m_Command.ReadListItem(ref Lines);
                }
            }
        }

        private void OnLoadGroup(object sender, EventArgs e)
        {
            StatusList.Items.Clear();

            Properties.Settings.Default.MaxAge = m_MaxAge.Text;
            Properties.Settings.Default.MinAge = m_MinAge.Text;
            Properties.Settings.Default.Save();

            m_Command.LoadGroup(int.Parse(m_MinAge.Text), int.Parse(m_MaxAge.Text));
            
            int nCount = m_Command.LoadedCount();

            StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));
            if (nCount > 0)
            {
                m_Command.LoadedList(1, nCount);
                StringBuilder Lines = new StringBuilder();

                int nLength = m_Command.ReadListItem(ref Lines);
                while (0 != nLength)
                {
                    StatusList.Items.Add(new ListViewItem(Lines.ToString()));
                    nLength = m_Command.ReadListItem(ref Lines);
                }
            }
        }

        private void OnDownloadFiles(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection Selected = StatusList.SelectedItems;
            foreach (ListViewItem item in Selected)
            {
                string[] vResults = item.SubItems[0].Text.Split('\t');
                Debug.Assert(vResults.Length >= 2);
                if (vResults.Length < 2)
                {
                    Debug.Assert(vResults.Length < 2);
                    return;
                }
                int nIndex = int.Parse(vResults[0]);
                m_Command.LoadedDownload(nIndex);
            }

        }

        private void OnListDownloads(object sender, EventArgs e)
        {
            StatusList.Items.Clear();
            int nCount = m_Command.DownloadCount();
            StatusList.Items.Add(new ListViewItem(m_Command.GetReplyString()));
            if (nCount > 0)
            {
                m_Command.DownloadList(1, nCount);
                StringBuilder Lines = new StringBuilder();


                int nLength = m_Command.ReadListItem(ref Lines);
                while (0 != nLength)
                {
                    StatusList.Items.Add(new ListViewItem(Lines.ToString()));
                    nLength = m_Command.ReadListItem(ref Lines);
                }
            }
        }

        private void OnUploadFiles(object sender, EventArgs e)
        {
            String  sPath = @"X:\Temp\";
            ListView.SelectedListViewItemCollection Selected = StatusList.SelectedItems;
            foreach (ListViewItem item in Selected)
            {
                string[] vResults = item.SubItems[0].Text.Split('\t');
                Debug.Assert(vResults.Length >= 2);
                if (vResults.Length < 2)
                {
                    Debug.Assert(vResults.Length < 2);
                    return;
                }
                int nIndex = int.Parse(vResults[0]);
                m_Command.FileRetrieve(nIndex,sPath);
            }
        }
    }
}
