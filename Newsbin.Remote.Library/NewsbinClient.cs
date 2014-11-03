///
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

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
		protected bool continueOnCapturedContext = true;
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
            this.client = new TcpClient();
        }

		public NewsbinClient(string serverAddress, int port, string password, bool continueOnCapturedContext)
		{
			this.serverAddress = serverAddress;
			this.port = port;
			this.password = password;
			this.client = new TcpClient();
			this.continueOnCapturedContext = continueOnCapturedContext;
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

		public async Task<bool> ConnectAsync()
		{
			await Task.Factory.FromAsync(client.BeginConnect, client.EndConnect, this.serverAddress, this.port, null).ConfigureAwait(continueOnCapturedContext);
			if (this.ClientConnected != null)
				this.ClientConnected(this, new ClientConnectedEventArgs());

			this.streamReader = new StreamReader(client.GetStream());
			this.streamWriter = new StreamWriter(client.GetStream());

			Response response = new Response(await this.ReadLineAsync().ConfigureAwait(continueOnCapturedContext));

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

				response = await SendCommandAsync(Command.Auth, AuthenticationHelper.GetAuthString(this.password, passwordSalt)).ConfigureAwait(continueOnCapturedContext);
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

		public async Task DisconnectAsync()
		{
			await SendCommandAsync(Command.Quit);
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

		public async Task<NewsbinStatus> GetStatusAsync()
		{
			ListResponse response = await SendListCommandAsync(Command.Status).ConfigureAwait(continueOnCapturedContext);
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

		public async Task<bool> GetBandwidthLimiterStateAsync()
		{
			Response response = await SendCommandAsync(Command.Bwl).ConfigureAwait(continueOnCapturedContext);
			return response.StatusCode == StatusCode.StatusBwLimiterOn;
		}

        public bool SetBandwidthLimiterState(bool state)
        {
            Response response = SendCommand(Command.Bwl, state ? "ON" : "OFF");
            return response.StatusCode == StatusCode.StatusBwLimiterOn;
        }

		public async Task<bool> SetBandwidthLimiterStateAsync(bool state)
		{
			Response response = await SendCommandAsync(Command.Bwl, state ? "ON" : "OFF").ConfigureAwait(continueOnCapturedContext);
			return response.StatusCode == StatusCode.StatusBwLimiterOn;
		}

        public bool GetPausedState()
        {
            Response response = SendCommand(Command.Pause);
            return response.StatusCode == StatusCode.StatusPauseOn;
        }

		public async Task<bool> GetPausedStateAsync()
		{
			Response response = await SendCommandAsync(Command.Pause);
			return response.StatusCode == StatusCode.StatusPauseOn;
		}

        public bool SetPausedState(bool state)
        {
            Response response = SendCommand(Command.Pause, state ? "ON" : "OFF");
            return response.StatusCode == StatusCode.StatusPauseOn;
        }

		public async Task<bool> SetPausedStateAsync(bool state)
		{
			Response response = await SendCommandAsync(Command.Pause, state ? "ON" : "OFF");
			return response.StatusCode == StatusCode.StatusPauseOn;
		}

        #endregion

        #region Group list methods

        public IEnumerable<string> ListGroups()
        {
            ListResponse response = SendListCommand(Command.ListGroups);
            return response.ListItems;
        }

		public async Task<IEnumerable<string>> ListGroupsAsync()
		{
			ListResponse response = await SendListCommandAsync(Command.ListGroups);
			return response.ListItems;
		}

        public IEnumerable<string> ListGogs()
        {
            ListResponse response = SendListCommand(Command.ListGogs);
            return response.ListItems;
        }

		public async Task<IEnumerable<string>> ListGogsAsync()
		{
			ListResponse response = await SendListCommandAsync(Command.ListGogs);
			return response.ListItems;
		}

        public IEnumerable<string> ListChildren(string gogName)
        {
            ListResponse response = SendListCommand(Command.ListChildren, gogName);
            return response.ListItems;
        }

		public async Task<IEnumerable<string>> ListChildrenAsync(string gogName)
		{
			ListResponse response = await SendListCommandAsync(Command.ListChildren, gogName);
			return response.ListItems;
		}

        public IEnumerable<string> ListChildrenDetails(string gogName)
        {
            ListResponse response = SendListCommand(Command.ListChildrenDetails, gogName);
            return response.ListItems;
        }

		public async Task<IEnumerable<string>> ListChildrenDetailsAsync(string gogName)
		{
			ListResponse response = await SendListCommandAsync(Command.ListChildrenDetails, gogName);
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

		public async Task<GogTree> ListGogsWithChildrenAsync()
		{
			ListResponse response = await SendListCommandAsync(Command.ListChildren);
			IEnumerable<string> gogNames = response.ListItems;
			GogTree result = new GogTree();
			foreach (string gogName in gogNames)
			{
				ListResponse subResponse = await SendListCommandAsync(Command.ListChildrenDetails);
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

		public async Task<IEnumerable<string>> ListServersAsync()
		{
			ListResponse response = await SendListCommandAsync(Command.ListServers);
			return response.ListItems;
		}

        #endregion

        #region Filter methods

        public IEnumerable<string> ListFilters()
        {
            ListResponse response = SendListCommand(Command.ListFilters);
            return response.ListItems;
        }

		public async Task<IEnumerable<string>> ListFiltersAsync()
		{
			ListResponse response = await SendListCommandAsync(Command.ListFilters);
			return response.ListItems;
		}

        public void SetFilterMinSize(long minSize)
        {
            SendCommand(Command.FilterSetMinSize, minSize.ToString());
        }

		public async Task SetFilterMinSizeAsync(long minSize)
		{
			await SendCommandAsync(Command.FilterSetMinSize, minSize.ToString());
		}

        public void SetFilterMaxSize(long maxSize)
        {
            SendCommand(Command.FilterSetMaxSize, maxSize.ToString());
        }

		public async Task SetFilterMaxSizeAsync(long maxSize)
		{
			await SendCommandAsync(Command.FilterSetMaxSize, maxSize.ToString());
		}

        public void AddGroupFilter(string groupName)
        {
            SendCommand(Command.FilterAddGroupFilter, groupName.ToString());
        }

		public async Task AddGroupFilterAsync(string groupName)
		{
			await SendCommandAsync(Command.FilterAddGroupFilter, groupName.ToString());
		}
		
		public void SetFilterAge(long hours)
        {
            SendCommand(Command.FilterSetAge, hours.ToString());
        }

		public async Task SetFilterAgeAsync(long hours)
		{
			await SendCommandAsync(Command.FilterSetAge, hours.ToString());
		}
		
		public void SetTextFilter(string text)
        {
            SendCommand(Command.FilterSetTextFilter, text);
        }

		public async Task SetTextFilterAsync(string text)
		{
			await SendCommandAsync(Command.FilterSetTextFilter, text);
		}

        public void HideOld(bool hide)
        {
            SendCommand(Command.FilterHideOld, hide ? "1" : "0");
        }

		public async Task HideOldAsync(bool hide)
		{
			await SendCommandAsync(Command.FilterHideOld, hide ? "1" : "0");
		}

        public void EnableFilters(bool enable)
        {
            SendCommand(Command.FilterEnableFilters, enable ? "1" : "0");
        }

		public async Task EnableFiltersAsync(bool enable)
		{
			await SendCommandAsync(Command.FilterEnableFilters, enable ? "1" : "0");
		}
		
		public void ClearFilters()
        {
            SendCommand(Command.FilterClear);
        }

		public async Task ClearFiltersAsync()
		{
			await SendCommandAsync(Command.FilterClear);
		}
		
		public void MarkOld(int id)
        {
            SendCommand(Command.FilterMarkOld, id.ToString());
        }

		public async Task MarkOldAsync(int id)
		{
			await SendCommandAsync(Command.FilterMarkOld, id.ToString());
		}

        public void MarkAllOld()
        {
            SendCommand(Command.FilterMarkAllOld);
        }

		public async Task MarkAllOldAsync()
		{
			await SendCommandAsync(Command.FilterMarkAllOld);
		}
		
		public void SetFilterProfile(string profileName)
        {
            SendCommand(Command.FilterSetProfile, profileName);
        }

		public async Task SetFilterProfileAsync(string profileName)
		{
			await SendCommandAsync(Command.FilterSetProfile, profileName);
		}

        #endregion

        #region Search methods

        public void SearchLocal(int minAge, int maxAge, string searchString)
        {
            SendCommand(Command.SearchLocal, minAge.ToString(), maxAge.ToString(), searchString);
        }

		public async Task SearchLocalAsync(int minAge, int maxAge, string searchString)
		{
			await SendCommandAsync(Command.SearchLocal, minAge.ToString(), maxAge.ToString(), searchString);
		}
		
		public void SearchInternet(int minAge, int maxAge, string searchString)
        {
            SendCommand(Command.SearchInternet, minAge.ToString(), maxAge.ToString(), searchString);
        }

		public async Task SearchInternetAsync(int minAge, int maxAge, string searchString)
		{
			await SendCommandAsync(Command.SearchInternet, minAge.ToString(), maxAge.ToString(), searchString);
		}

		#endregion

        #region Group methods

        public long LoadGroup(int minAge, int maxAge)
        {
            return SendLongCommand(Command.GroupLoad, minAge.ToString(), maxAge.ToString());
        }

		public async Task<long> LoadGroupAsync(int minAge, int maxAge)
		{
			return await SendLongCommandAsync(Command.GroupLoad, minAge.ToString(), maxAge.ToString());
			
		}

		public void ClearGroup()
        {
            SendCommand(Command.GroupClear);
        }

		public async Task ClearGroupAsync()
		{
			await SendCommandAsync(Command.GroupClear);
		}

        public void DeleteFromGroup(int id)
        {
            SendCommand(Command.GroupDelete, id.ToString());
        }

		public async Task DeleteFromGroupAsync(int id)
		{
			await SendCommandAsync(Command.GroupDelete, id.ToString());
		}

        #endregion

        #region Loaded methods

        public long GetLoadedCount()
        {
            return SendLongCommand(Command.LoadedCount);
        }

		public async Task<long> GetLoadedCountAsync()
		{
			return await SendLongCommandAsync(Command.LoadedCount);
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

		public async Task<IEnumerable<GroupFileSet>> ListLoadedAsync(int startIndex, int endIndex)
		{
			ListResponse response = await SendListCommandAsync(Command.LoadedList, startIndex.ToString(), endIndex.ToString());
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

		public async Task SortLoadedAsync(int column, bool ascending)
		{
			await SendCommandAsync(Command.LoadedSort, column.ToString(), ascending ? "1" : "0");
		}

        public void ClearLoaded()
        {
            SendCommand(Command.LoadedClear);
        }

		public async Task ClearLoadedAsync()
		{
			await SendCommandAsync(Command.LoadedClear);
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

		public async Task<IEnumerable<GroupFileSet>> FindLoadedAsync(string text)
		{
			ListResponse response = await SendListCommandAsync(Command.LoadedFind, text);
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

		public async Task DownloadFromLoadedAsync(int id)
		{
			await SendCommandAsync(Command.LoadedDownload, id.ToString());
		}
		
		public void GetNzbFromLoaded(int minIndex, int maxIndex)
        {
            SendCommand(Command.LoadedGetNzb, minIndex.ToString(), maxIndex.ToString());
        }

		public async Task GetNzbFromLoadedAsync(int minIndex, int maxIndex)
		{
			await SendCommandAsync(Command.LoadedGetNzb, minIndex.ToString(), maxIndex.ToString());
		}

		#endregion

        #region Downloads methods

        public long GetDownloadsCount()
        {
            return SendLongCommand(Command.DownloadsCount);
        }

		public async Task<long> GetDownloadsCountAsync()
		{
			return await SendLongCommandAsync(Command.DownloadsCount);
		}

        public void LockDownloads()
        {
            SendCommand(Command.DownloadsLock);
        }

		public async Task LockDownloadsAsync()
		{
			await SendCommandAsync(Command.DownloadsLock);
		}
		
		public void UnlockDownloads()
        {
            SendCommand(Command.DownloadsUnlock);
        }

		public async Task UnlockDownloadsAsync()
		{
			await SendCommandAsync(Command.DownloadsUnlock);
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

		public async Task<IEnumerable<DownloadFileSet>> GetDownloadsListAsync(int startIndex, int endIndex)
		{
			ListResponse response = await SendListCommandAsync(Command.DownloadsList, startIndex.ToString(), endIndex.ToString());
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

		public async Task SortDownloadsListAsync(int column, bool ascending)
		{
			await SendCommandAsync(Command.DownloadsSort, column.ToString(), ascending ? "1" : "0");
		}

		public void ClearDownloadList()
        {
            SendCommand(Command.DownloadsClear);
        }

		public async Task ClearDownloadListAsync()
		{
			await SendCommandAsync(Command.DownloadsClear);
		}

		public void FindInDownloadList(string text)
        {
            SendCommand(Command.DownloadsFind, text);
        }

		public async Task FindInDownloadListAsync(string text)
		{
			await SendCommandAsync(Command.DownloadsFind, text);
		}
		
		public void AssembleDownload(int id)
        {
            SendCommand(Command.DownloadsAssemble, id.ToString());
        }

		public async Task AssembleDownloadAsync(int id)
		{
			await SendCommandAsync(Command.DownloadsAssemble, id.ToString());
		}

		public void DeleteDownload(int id)
        {
            SendCommand(Command.DownloadsDelete, id.ToString());
        }

		public async Task DeleteDownloadAsync(int id)
		{
			await SendCommandAsync(Command.DownloadsDelete, id.ToString());
		}

        public void PauseDownload(int id)
        {
            SendCommand(Command.DownloadsPause, id.ToString());
        }

		public async Task PauseDownloadAsync(int id)
		{
			await SendCommandAsync(Command.DownloadsPause, id.ToString());
		}
		
		public void ResumeDownload(int id)
        {
            SendCommand(Command.DownloadsResume, id.ToString());
        }

		public async Task ResumeDownloadAsync(int id)
		{
			await SendCommandAsync(Command.DownloadsResume, id.ToString());
		}

        public void DownloadMoveUp(int id)
        {
            SendCommand(Command.DownloadsMoveUp, id.ToString());
        }

		public async Task DownloadMoveUpAsync(int id)
		{
			await SendCommandAsync(Command.DownloadsMoveUp, id.ToString());
		}

        public void DownloadMoveDown(int id)
        {
            SendCommand(Command.DownloadsMoveDown, id.ToString());
        }

		public async Task DownloadMoveDownAsync(int id)
		{
			await SendCommandAsync(Command.DownloadsMoveDown, id.ToString());
		}
		
		public void DownloadMoveTop(int id)
        {
            SendCommand(Command.DownloadsMoveTop, id.ToString());
        }

		public async Task DownloadMoveTopAsync(int id)
		{
			await SendCommandAsync(Command.DownloadsMoveTop, id.ToString());
		}

        public void DownloadMoveBottom(int id)
        {
            SendCommand(Command.DownloadsMoveBottom, id.ToString());
        }

		public async Task DownloadMoveBottomAsync(int id)
		{
			await SendCommandAsync(Command.DownloadsMoveBottom, id.ToString());
		}

        #endregion

        #region Files methods

        public long GetFilesCount()
        {
            return SendLongCommand(Command.FilesCount);
        }

		public async Task<long> GetFilesCountAsync()
		{
			return await SendLongCommandAsync(Command.FilesCount);
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

		public async Task<IEnumerable<FilesFileSet>> GetFilesListAsync(int startIndex, int endIndex)
		{
			ListResponse response = await SendListCommandAsync(Command.FilesList, startIndex.ToString(), endIndex.ToString());
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

		public async Task DownloadFromFilesAsync(string fileName, int id)
		{
			using (Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			{
				await DownloadFromFilesAsync(stream, id);
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

		public async Task DownloadFromFilesAsync(Stream stream, int id)
		{
			Response response = await SendCommandAsync(Command.FilesDownload, id.ToString());
			if (response.StatusCode == StatusCode.OK)
			{
				FilesDownloadItem downloadItem = new FilesDownloadItem(response.Message);
				await SendCommandWithoutResponseAsync(Command.Continue);

				byte[] buffer = new byte[4 * 1024];
				long totalRead = 0;
				int bytesRead = 0;
				while (totalRead < downloadItem.FileSize)
				{
					long bytesToRead = downloadItem.FileSize - totalRead;
					if (bytesToRead > buffer.Length)
						bytesToRead = buffer.Length;
					bytesRead = await client.GetStream().ReadAsync(buffer, 0, (int)bytesToRead);
					totalRead += bytesToRead;
					await stream.WriteAsync(buffer, 0, bytesRead);
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

		public async Task<long> GetWishCountAsync()
		{
			return await SendLongCommandAsync(Command.WishCount);
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

		public async Task<IEnumerable<WishFileSet>> GetWishListAsync(int startIndex, int endIndex)
		{
			ListResponse response = await SendListCommandAsync(Command.WishList, startIndex.ToString(), endIndex.ToString());
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

		public async Task DownloadFromWishAsync(int id)
		{
			await SendCommandAsync(Command.WishDownload, id.ToString());
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

		public Task UploadNzbAsync(string fileName)
		{
			using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				return UploadNzbAsync(Path.GetFileName(fileName), stream, stream.Length);
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

		public async Task UploadNzbAsync(string fileName, Stream stream, long size)
		{
			Response response = await SendCommandAsync(Command.UploadNzbBinary, fileName, size.ToString());
			if (response.StatusCode == StatusCode.StatusPostData)
			{
				int bytesRead;
				byte[] buffer = new byte[4 * 1024];
				do
				{
					bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
					await client.GetStream().WriteAsync(buffer, 0, bytesRead);
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

		protected Task<Response> SendCommandAsync(Command command)
		{
			return SendCommandAsync(command, "", "");
		}

        protected Response SendCommand(Command command, string argument1)
        {
            return SendCommand(command, argument1, "");
        }

		protected Task<Response> SendCommandAsync(Command command, string argument1)
		{
			return SendCommandAsync(command, argument1, "");
		}

        protected Response SendCommand(Command command, string argument1, string argument2)
        {
            return SendCommand(command, argument1, argument2, "");
        }

		protected Task<Response> SendCommandAsync(Command command, string argument1, string argument2)
		{
			return SendCommandAsync(command, argument1, argument2, "");
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

		protected async Task<Response> SendCommandAsync(Command command, string argument1, string argument2, string argument3)
		{
			string commandLine = CommandStrings.Strings[command];
			if (argument1.Length > 0)
				commandLine += " " + argument1;
			if (argument2.Length > 0)
				commandLine += " " + argument2;
			if (argument3.Length > 0)
				commandLine += " " + argument3;

			await this.WriteLineAsync(commandLine).ConfigureAwait(continueOnCapturedContext);
			if (this.ClientSentCommand != null)
				this.ClientSentCommand(this, new ClientSentCommandEventArgs(commandLine));

			Response response = new Response(await this.ReadLineAsync().ConfigureAwait(continueOnCapturedContext));
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

		public async Task SendCommandWithoutResponseAsync(Command command)
		{
			string commandLine = CommandStrings.Strings[command];
			await this.WriteLineAsync(commandLine);
			if (this.ClientSentCommand != null)
				this.ClientSentCommand(this, new ClientSentCommandEventArgs(commandLine));
		}

        public ListResponse SendListCommand(Command command)
        {
            return SendListCommand(command, "", "");
        }

		public Task<ListResponse> SendListCommandAsync(Command command)
		{
			return SendListCommandAsync(command, "", "");
		}

        public ListResponse SendListCommand(Command command, string argument)
        {
            return SendListCommand(command, argument, "");
        }

		public Task<ListResponse> SendListCommandAsync(Command command, string argument)
		{
			return SendListCommandAsync(command, argument, "");
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

		public async Task<ListResponse> SendListCommandAsync(Command command, string argument1, string argument2)
		{
			string commandLine = CommandStrings.Strings[command];
			if (argument1.Length > 0)
				commandLine += " " + argument1;
			if (argument2.Length > 0)
				commandLine += " " + argument2;

			await this.WriteLineAsync(commandLine).ConfigureAwait(continueOnCapturedContext);
			if (this.ClientSentCommand != null)
				this.ClientSentCommand(this, new ClientSentCommandEventArgs(commandLine));

			ListResponse result = new ListResponse(await this.ReadLineAsync().ConfigureAwait(continueOnCapturedContext));
			if (this.ClientReceivedResponse != null)
				this.ClientReceivedResponse(this, new ClientReceivedResponseEventArgs(result));

			if (result.StatusCode == StatusCode.ParseError)
				throw new CommunicationException("Error", command, result);
			if (result.StatusCode != StatusCode.OK)
				throw new CommunicationException(result.Message, command, result);

			string data = await ReadLineAsync().ConfigureAwait(continueOnCapturedContext);
			while (data != ".")
			{
				if (this.ClientRecievedListItem != null)
					this.ClientRecievedListItem(this, new ClientReceivedListItemEventArgs(data));
				result.AddListItem(data);
				data = await ReadLineAsync().ConfigureAwait(continueOnCapturedContext);
			}
			return result;
		}

        public long SendLongCommand(Command command)
        {
            return SendLongCommand(command, "", "");
        }

		public Task<long> SendLongCommandAsync(Command command)
		{
			return SendLongCommandAsync(command, "", "");
		}

        public long SendLongCommand(Command command, string argument)
        {
            return SendLongCommand(command, argument, "");
        }

		public Task<long> SendLongCommandAsync(Command command, string argument)
		{
			return SendLongCommandAsync(command, argument, "");
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

		public async Task<long> SendLongCommandAsync(Command command, string argument1, string argument2)
		{
			Response response = await SendCommandAsync(command, argument1, argument2);
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

		protected async Task WriteLineAsync(string command)
		{
			await this.streamWriter.WriteLineAsync(command).ConfigureAwait(continueOnCapturedContext);
			await this.streamWriter.FlushAsync().ConfigureAwait(continueOnCapturedContext);
		}

        protected string ReadLine()
        {
            return this.streamReader.ReadLine();
        }

		protected Task<string> ReadLineAsync()
		{
			return this.streamReader.ReadLineAsync();
		}

        #endregion
    }
}
