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
			string host = m_IPAddress.Text; 
            IPAddress[] addrs = Dns.GetHostEntry(host).AddressList;

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
			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				string sPath = folderBrowserDialog1.SelectedPath;
                sPath += "\\";  // append backslash on the end
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
					m_Command.FileRetrieve(nIndex, sPath);
				}
			}
        }

        private void Main_Load(object sender, EventArgs e)
        {
            // Set up tooltips

            System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(this.button1, "Connect to Newsbin");  // Connect button
            ToolTip1.SetToolTip(this.button2, "Add a group or group folder for use by Search or Load");  // Add Group button
            ToolTip1.SetToolTip(this.m_ListFiles, "List downloaded files from the Files tab");  // List Files button
            ToolTip1.SetToolTip(this.button3, "Load headers for specified group of group folder");  // Load Group button
            ToolTip1.SetToolTip(this.button4, "Send an NZB file to Newsbin");  // Upload NZB button
            ToolTip1.SetToolTip(this.button6, "Clear Group filter");  // Clear Groups button
            ToolTip1.SetToolTip(this.button7, "Retrieve selected file from Newsbin to this machine");  // Upload Files button
            ToolTip1.SetToolTip(this.button8, "Queue selected files for download");  // Download Files button
            ToolTip1.SetToolTip(this.button9, "List files in the Downloads tab");  // List Downloads button
            ToolTip1.SetToolTip(this.button10, "Send NZB File to be processed by Newsbin");  // Autoload NZB button
            ToolTip1.SetToolTip(this.m_IPAddress, "Enter IP Address of machine Newsbin is running on");  // IP Address entry form
            ToolTip1.SetToolTip(this.m_Port, "Enter Port Newsbin is listening on (Default is 118)");  // Port entry form
            ToolTip1.SetToolTip(this.m_Key, "Enter the same key used in the copy of Newsbin you are connecting to");  // Key entry form
            ToolTip1.SetToolTip(this.m_GroupText, "Enter a group or group folder to use as a filter");  // Group entry form
            ToolTip1.SetToolTip(this.m_MinAge, "Enter a minimum post age to use as a filter");  // Minimum Age entry form
            ToolTip1.SetToolTip(this.m_MaxAge, "Enter a maximum post age to use as a filter");  // Maximum Age entry form
            ToolTip1.SetToolTip(this.m_Password, "Enter the password you configured within Newsbin");  // Password entry form
        }
    }
}
