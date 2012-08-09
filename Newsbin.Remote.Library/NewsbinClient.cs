///
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace Newsbin.Remote
{
    #region Event handlers

    public delegate void ClientConnectedEventHandler(object sender, ClientConnectedEventArgs e);
    public delegate void ClientDisconnectedEventHandler(object sender, ClientDisconnectedEventArgs e);
    public delegate void ClientSentCommandEventHandler(object sender, ClientSentCommandEventArgs e);
    public delegate void ClientReceivedResponseEventHandler(object sender, ClientReceivedResponseEventArgs e);
    public delegate void ClientReceivedListItemEventHandler(object sender, ClientReceivedListItemEventArgs e);

    #endregion

    public class NewsbinClient
    {
        #region Private variables

        protected string serverAddress;
        protected int port;
        protected string password;
        protected string newsbinVersion;
        protected TcpClient client;
        protected StreamWriter streamWriter;
        protected StreamReader streamReader;

        #endregion

        public event ClientConnectedEventHandler ClientConnected;
        public event ClientDisconnectedEventHandler ClientDisconnected;
        public event ClientSentCommandEventHandler ClientSentCommand;
        public event ClientReceivedResponseEventHandler ClientReceivedResponse;
        public event ClientReceivedListItemEventHandler ClientRecievedListItem;

        #region Public properties

        public bool Connected
        {
            get { return this.client.Connected; }
        }

        public string NewsbinVersion
        {
            get { return this.newsbinVersion; }
        }

        #endregion

        #region Constructor

        public NewsbinClient(string serverAddress, int port, string password)
        {
            this.serverAddress = serverAddress;
            this.port = port;
            this.password = password;
            client = new TcpClient();
        }

        #endregion

        #region Public methods

        #region Connection methods

        public bool Connect()
        {
            client.Connect(this.serverAddress, this.port);

            if (this.ClientConnected != null)
                this.ClientConnected(this, new ClientConnectedEventArgs());

            this.streamReader = new StreamReader(client.GetStream());
            this.streamWriter = new StreamWriter(client.GetStream());

            Response response = new Response(this.ReadLine());
            if (this.ClientReceivedResponse != null)
                this.ClientReceivedResponse(this, new ClientReceivedResponseEventArgs(response));

            if (response.StatusCode == StatusCode.AuthNeedAuth)
            {
                string passwordSalt = "";
                string[] strings = response.Message.Split('-');
                if (strings.Length == 2)
                {
                    this.newsbinVersion = strings[0].Trim();
                    passwordSalt = strings[1].Substring(1, 16);
                }

                response = SendCommand(Command.Auth, AuthenticationHelper.GetAuthString(this.password, passwordSalt));
            }

            if (response.StatusCode != StatusCode.OK)
            {
                DisposeConnection();
                throw new CommunicationException("Authentication failure", Command.Auth, response);
            }

            return true;
        }

        public void Disconnect()
        {
            SendCommand(Command.Quit);
            DisposeConnection();
        }

        private void DisposeConnection()
        {
            this.client.GetStream().Dispose();
            this.client.Close();
            this.streamReader = null;
            this.streamWriter = null;
            if (this.ClientDisconnected != null)
                this.ClientDisconnected(this, new ClientDisconnectedEventArgs());
        }

        #endregion

        #region Status methods

        public NewsbinStatus GetStatus()
        {
            ListResponse response = SendListCommand(Command.Status);
            NewsbinStatus result = new NewsbinStatus();
            foreach (string data in response.ListItems)
            {
                result.Add(data);
            }
            return result;
        }

        public bool GetBandwidthLimiterState()
        {
            Response response = SendCommand(Command.Bwl);
            return response.StatusCode == StatusCode.StatusBwLimiterOn;
        }

        public bool SetBandwidthLimiterState(bool state)
        {
            Response response = SendCommand(Command.Bwl, state ? "ON" : "OFF");
            return response.StatusCode == StatusCode.StatusBwLimiterOn;
        }

        public bool GetPausedState()
        {
            Response response = SendCommand(Command.Pause);
            return response.StatusCode == StatusCode.StatusPauseOn;
        }

        public bool SetPausedState(bool state)
        {
            Response response = SendCommand(Command.Pause, state ? "ON" : "OFF");
            return response.StatusCode == StatusCode.StatusPauseOn;
        }

        #endregion

        #region Group list methods

        public IEnumerable<string> ListGroups()
        {
            ListResponse response = SendListCommand(Command.ListGroups);
            return response.ListItems;
        }

        public IEnumerable<string> ListGogs()
        {
            ListResponse response = SendListCommand(Command.ListGogs);
            return response.ListItems;
        }

        public IEnumerable<string> ListChildren(string gogName)
        {
            ListResponse response = SendListCommand(Command.ListChildren, gogName);
            return response.ListItems;
        }

        public IEnumerable<string> ListChildrenDetails(string gogName)
        {
            ListResponse response = SendListCommand(Command.ListChildrenDetails, gogName);
            return response.ListItems;
        }

        public GogTree ListGogsWithChildren()
        {
            ListResponse response = SendListCommand(Command.ListChildren);
            IEnumerable<string> gogNames = response.ListItems;
            GogTree result = new GogTree();
            foreach (string gogName in gogNames)
            {
                ListResponse subResponse = SendListCommand(Command.ListChildrenDetails);
                IEnumerable<string> groupNames = subResponse.ListItems;
                foreach (string groupName in groupNames)
                {
                    result.Add(gogName, groupName);
                }
            }
            return result;
        }

        #endregion

        #region Server methods

        public IEnumerable<string> ListServers()
        {
            ListResponse response = SendListCommand(Command.ListServers);
            return response.ListItems;
        }

        #endregion

        #region Filter methods

        public IEnumerable<string> ListFilters()
        {
            ListResponse response = SendListCommand(Command.ListFilters);
            return response.ListItems;
        }

        public void SetFilterMinSize(long minSize)
        {
            SendCommand(Command.FilterSetMinSize, minSize.ToString());
        }

        public void SetFilterMaxSize(long maxSize)
        {
            SendCommand(Command.FilterSetMaxSize, maxSize.ToString());
        }

        public void AddGroupFilter(string groupName)
        {
            SendCommand(Command.FilterAddGroupFilter, groupName.ToString());
        }

        public void SetFilterAge(long hours)
        {
            SendCommand(Command.FilterSetAge, hours.ToString());
        }

        public void SetTextFilter(string text)
        {
            SendCommand(Command.FilterSetTextFilter, text);
        }

        public void HideOld(bool hide)
        {
            SendCommand(Command.FilterHideOld, hide ? "1" : "0");
        }

        public void EnableFilters(bool enable)
        {
            SendCommand(Command.FilterEnableFilters, enable ? "1" : "0");
        }

        public void ClearFilters()
        {
            SendCommand(Command.FilterClear);
        }

        public void MarkOld(int id)
        {
            SendCommand(Command.FilterMarkOld, id.ToString());
        }

        public void MarkAllOld()
        {
            SendCommand(Command.FilterMarkAllOld);
        }

        public void SetFilterProfile(string profileName)
        {
            SendCommand(Command.FilterSetProfile, profileName);
        }

        #endregion

        #region Search methods

        public void SearchLocal(int minAge, int maxAge, string searchString)
        {
            SendCommand(Command.SearchLocal, minAge.ToString(), maxAge.ToString(), searchString);
        }

        public void SearchInternet(int minAge, int maxAge, string searchString)
        {
            SendCommand(Command.SearchInternet, minAge.ToString(), maxAge.ToString(), searchString);
        }

        #endregion

        #region Group methods

        public long LoadGroup(int minAge, int maxAge)
        {
            return SendLongCommand(Command.GroupLoad, minAge.ToString(), maxAge.ToString());
        }

        public void ClearGroup()
        {
            SendCommand(Command.GroupClear);
        }

        public void DeleteFromGroup(int id)
        {
            SendCommand(Command.GroupDelete, id.ToString());
        }

        #endregion

        #region Loaded methods

        public long GetLoadedCount()
        {
            return SendLongCommand(Command.LoadedCount);
        }

        public IEnumerable<GroupFileSet> ListLoaded(int startIndex, int endIndex)
        {
            ListResponse response = SendListCommand(Command.LoadedList, startIndex.ToString(), endIndex.ToString());
            List<GroupFileSet> result = new List<GroupFileSet>();
            foreach (string set in response.ListItems)
            {
                result.Add(new GroupFileSet(set));
            }
            return result;
        }

        public void SortLoaded(int column, bool ascending)
        {
            SendCommand(Command.LoadedSort, column.ToString(), ascending ? "1" : "0");
        }

        public void ClearLoaded()
        {
            SendCommand(Command.LoadedClear);
        }

        public IEnumerable<GroupFileSet> FindLoaded(string text)
        {
            ListResponse response = SendListCommand(Command.LoadedFind, text);
            List<GroupFileSet> result = new List<GroupFileSet>();
            foreach (string set in response.ListItems)
            {
                result.Add(new GroupFileSet(set));
            }
            return result;
        }

        public void DownloadFromLoaded(int id)
        {
            SendCommand(Command.LoadedDownload, id.ToString());
        }

        public void GetNzbFromLoaded(int minIndex, int maxIndex)
        {
            SendCommand(Command.LoadedGetNzb, minIndex.ToString(), maxIndex.ToString());
        }

        #endregion

        #region Downloads methods

        public long GetDownloadsCount()
        {
            return SendLongCommand(Command.DownloadsCount);
        }

        public void LockDownloads()
        {
            SendCommand(Command.DownloadsLock);
        }

        public void UnlockDownloads()
        {
            SendCommand(Command.DownloadsUnlock);
        }

        public IEnumerable<DownloadFileSet> GetDownloadsList(int startIndex, int endIndex)
        {
            ListResponse response = SendListCommand(Command.DownloadsList, startIndex.ToString(), endIndex.ToString());
            List<DownloadFileSet> result = new List<DownloadFileSet>();
            foreach (string set in response.ListItems)
            {
                result.Add(new DownloadFileSet(set));
            }
            return result;            
        }

        public void SortDownloadsList(int column, bool ascending)
        {
            SendCommand(Command.DownloadsSort, column.ToString(), ascending ? "1" : "0");
        }

        public void ClearDownloadList()
        {
            SendCommand(Command.DownloadsClear);
        }

        public void FindInDownloadList(string text)
        {
            SendCommand(Command.DownloadsFind, text);
        }

        public void AssembleDownload(int id)
        {
            SendCommand(Command.DownloadsAssemble, id.ToString());
        }

        public void DeleteDownload(int id)
        {
            SendCommand(Command.DownloadsDelete, id.ToString());
        }

        public void PauseDownload(int id)
        {
            SendCommand(Command.DownloadsPause, id.ToString());
        }

        public void ResumeDownload(int id)
        {
            SendCommand(Command.DownloadsResume, id.ToString());
        }

        public void DownloadMoveUp(int id)
        {
            SendCommand(Command.DownloadsMoveUp, id.ToString());
        }

        public void DownloadMoveDown(int id)
        {
            SendCommand(Command.DownloadsMoveDown, id.ToString());
        }

        public void DownloadMoveTop(int id)
        {
            SendCommand(Command.DownloadsMoveTop, id.ToString());
        }

        public void DownloadMoveBottom(int id)
        {
            SendCommand(Command.DownloadsMoveBottom, id.ToString());
        }

        #endregion

        #region Files methods

        public long GetFilesCount()
        {
            return SendLongCommand(Command.FilesCount);
        }

        public IEnumerable<FilesFileSet> GetFilesList(int startIndex, int endIndex)
        {
            ListResponse response = SendListCommand(Command.FilesList, startIndex.ToString(), endIndex.ToString());
            List<FilesFileSet> result = new List<FilesFileSet>();
            foreach (string set in response.ListItems)
            {
                result.Add(new FilesFileSet(set));
            }
            return result;
        }

        public void DownloadFromFiles(string fileName, int id)
        {
            using (Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                DownloadFromFiles(stream, id);
            }
        }

        public void DownloadFromFiles(Stream stream, int id)
        {
            Response response = SendCommand(Command.FilesDownload, id.ToString());
            if (response.StatusCode == StatusCode.OK)
            {
                FilesDownloadItem downloadItem = new FilesDownloadItem(response.Message);
                SendCommandWithoutResponse(Command.Continue);

                byte[] buffer = new byte[4 * 1024];
                long totalRead = 0;
                int bytesRead = 0;
                while (totalRead < downloadItem.FileSize)
                {
                    long bytesToRead = downloadItem.FileSize - totalRead;
                    if (bytesToRead > buffer.Length)
                        bytesToRead = buffer.Length;
                    bytesRead = client.GetStream().Read(buffer, 0, (int)bytesToRead);
                    totalRead += bytesToRead;
                    stream.Write(buffer, 0, bytesRead);
                }
                response = GetResponse();
            }
            if (response.StatusCode != StatusCode.OK)
                throw new CommunicationException("Download error", Command.FilesDownload, response);            
        }

        #endregion

        #region Wish methods

        public long GetWishCount()
        {
            return SendLongCommand(Command.WishCount);
        }

        public IEnumerable<WishFileSet> GetWishList(int startIndex, int endIndex)
        {
            ListResponse response = SendListCommand(Command.WishList, startIndex.ToString(), endIndex.ToString());
            List<WishFileSet> result = new List<WishFileSet>();
            foreach (string set in response.ListItems)
            {
                result.Add(new WishFileSet(set));
            }
            return result;
        }

        public void DownloadFromWish(int id)
        {
            SendCommand(Command.WishDownload, id.ToString());
        }

        #endregion

        #region Upload methods

        public void UploadNzb(string fileName)
        {
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                UploadNzb(Path.GetFileName(fileName), stream, stream.Length);
            }
        }

        public void UploadNzb(string fileName, Stream stream, long size)
        {
            Response response = SendCommand(Command.UploadNzbBinary, fileName, size.ToString());
            if (response.StatusCode == StatusCode.StatusPostData)
            {
                int bytesRead;
                byte[] buffer = new byte[4 * 1024];
                do
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    client.GetStream().Write(buffer, 0, bytesRead);
                }
                while (bytesRead == buffer.Length);
                response = new Response(this.ReadLine());
            }

            if (response.StatusCode != StatusCode.OK)
                throw new CommunicationException("NZB Upload error", Command.UploadNzbBinary, response);
        }

        #endregion

        #endregion

        #region Private helper methods

        protected Response SendCommand(Command command)
        {
            return SendCommand(command, "", "");
        }

        protected Response SendCommand(Command command, string argument1)
        {
            return SendCommand(command, argument1, "");
        }

        protected Response SendCommand(Command command, string argument1, string argument2)
        {
            return SendCommand(command, argument1, argument2, "");
        }

        protected Response SendCommand(Command command, string argument1, string argument2, string argument3)
        {
            string commandLine = CommandStrings.Strings[command];
            if (argument1.Length > 0)
                commandLine += " " + argument1;
            if (argument2.Length > 0)
                commandLine += " " + argument2;
            if (argument3.Length > 0)
                commandLine += " " + argument3;

            this.WriteLine(commandLine);
            if (this.ClientSentCommand != null)
                this.ClientSentCommand(this, new ClientSentCommandEventArgs(commandLine));
            
            Response response = new Response(this.ReadLine());
            if (this.ClientReceivedResponse != null)
                this.ClientReceivedResponse(this, new ClientReceivedResponseEventArgs(response));

            if (response.StatusCode == StatusCode.ParseError)
                throw new CommunicationException("Error", command, response);
            return response;
        }

        public void SendCommandWithoutResponse(Command command)
        {
            string commandLine = CommandStrings.Strings[command];
            this.WriteLine(commandLine);
            if (this.ClientSentCommand != null)
                this.ClientSentCommand(this, new ClientSentCommandEventArgs(commandLine));            
        }

        public ListResponse SendListCommand(Command command)
        {
            return SendListCommand(command, "", "");
        }

        public ListResponse SendListCommand(Command command, string argument)
        {
            return SendListCommand(command, argument, "");
        }

        public ListResponse SendListCommand(Command command, string argument1, string argument2)
        {
            string commandLine = CommandStrings.Strings[command];
            if (argument1.Length > 0)
                commandLine += " " + argument1;
            if (argument2.Length > 0)
                commandLine += " " + argument2;
            this.WriteLine(commandLine);
            if (this.ClientSentCommand != null)
                this.ClientSentCommand(this, new ClientSentCommandEventArgs(commandLine));

            ListResponse result = new ListResponse(this.ReadLine());
            if (this.ClientReceivedResponse != null)
                this.ClientReceivedResponse(this, new ClientReceivedResponseEventArgs(result));

            if (result.StatusCode == StatusCode.ParseError)
                throw new CommunicationException("Error", command, result);
            if (result.StatusCode != StatusCode.OK)
                throw new CommunicationException(result.Message, command, result);

            string data = ReadLine();
            while (data != ".")
            {
                if (this.ClientRecievedListItem != null)
                    this.ClientRecievedListItem(this, new ClientReceivedListItemEventArgs(data));
                result.AddListItem(data);
                data = ReadLine();
            }
            return result;
        }

        public long SendLongCommand(Command command)
        {
            return SendLongCommand(command, "", "");
        }

        public long SendLongCommand(Command command, string argument)
        {
            return SendLongCommand(command, argument, "");
        }

        public long SendLongCommand(Command command, string argument1, string argument2)
        {
            Response response = SendCommand(command, argument1, argument2);
            if (response.StatusCode == StatusCode.OK)
            {
                string[] strings = response.Message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return long.Parse(strings[0]);
            }
            return 0;
        }

        public Response GetResponse()
        {
            return new Response(this.ReadLine());
        }

        protected void WriteLine(string command)
        {
            this.streamWriter.WriteLine(command);
            this.streamWriter.Flush();
        }

        protected string ReadLine()
        {
            return this.streamReader.ReadLine();
        }

        #endregion
    }
}
