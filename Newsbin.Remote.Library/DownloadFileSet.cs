using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newsbin.Remote
{
    public enum FileSetType
    {
        Header,
        Download,
        Repair,
        Unpack
    }

    public class DownloadFileSet
    {
        protected int id;
        protected FileSetType fileSetType;
        protected string subject;
        protected long fileSize;
        protected DateTime timeStamp;
        protected string poster;
        protected string downloadPath;
        protected long downloadedSize;
        protected long downloadedCurrentSize;
        protected string status;
        protected bool isPaused;
        protected DownloadPartStatus partStatus;

        public int Id
        {
            get { return this.id; }
        }

        public string Subject
        {
            get { return this.subject; }
        }

        public long FileSize
        {
            get { return this.fileSize; }
        }

        public DateTime TimeStamp
        {
            get { return this.timeStamp; }
        }

        public string Poster
        {
            get { return this.poster; }
        }


        public DownloadFileSet(string data)
        {
            this.partStatus = new DownloadPartStatus();
            string[] strings = data.Split('\t');
            string[] subStrings;
            this.id = int.Parse(strings[0]);
            this.subject = strings[1];
            this.status = strings[8];
            this.fileSetType = GetFileSetType(this.subject, this.status);
            this.isPaused = strings[9] == "1";
            switch (this.fileSetType)
            {
                case FileSetType.Header:
                    if (strings[4] == "")
                        this.subject = "Header download";
                    else
                        this.subject = strings[4];
                    // 383.3 KB/5.92 GB
                    subStrings = strings[7].Split('/');
                    this.downloadedCurrentSize = GetSize(subStrings[0]);
                    this.downloadedSize = GetSize(subStrings[1]);
                    break;
                case FileSetType.Download:
                    this.fileSize = long.Parse(strings[2]);
                    this.timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    this.timeStamp += TimeSpan.FromSeconds(long.Parse(strings[3]));
                    this.poster = strings[4];
                    this.downloadPath = strings[5];
                    subStrings = strings[7].Split('/');
                    this.downloadedCurrentSize = GetSize(subStrings[0]);
                    this.downloadedSize = GetSize(subStrings[1]);
                    this.partStatus = new DownloadPartStatus(this.status);
                    break;
                case FileSetType.Repair:
                    this.fileSize = long.Parse(strings[2]);
                    this.timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    this.timeStamp += TimeSpan.FromSeconds(long.Parse(strings[3]));
                    this.poster = strings[4];
                    this.downloadPath = strings[5];
                    this.downloadedSize = 100;
                    subStrings = this.status.Split('%');
                    this.downloadedCurrentSize = long.Parse(subStrings[0].Substring(12));
                    break;
                case FileSetType.Unpack:
                    this.fileSize = long.Parse(strings[2]);
                    this.timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    this.timeStamp += TimeSpan.FromSeconds(long.Parse(strings[3]));
                    this.poster = strings[4];
                    this.downloadPath = strings[5];
                    this.downloadedSize = 100;
                    subStrings = this.status.Split('%');
                    this.downloadedCurrentSize = long.Parse(subStrings[0].Substring(12));
                    break;
            }
        }

        protected FileSetType GetFileSetType(string subject, string status)
        {
            if (subject == "")
                return FileSetType.Header;
            if (status.StartsWith("PAR"))
                return FileSetType.Repair;
            if (status.StartsWith("UnRAR"))
                return FileSetType.Unpack;
            return FileSetType.Download;
        }

        protected long GetSize(string sizeString)
        {
            string[] strings = sizeString.Split(' ');
            long result = long.Parse(strings[0]);
            switch (strings[1])
            {
                case "KB":
                    result *= 1024;
                    break;
                case "MB":
                    result *= 1024 * 1024;
                    break;
                case "GB":
                    result *= 1024 * 1024 * 1024;
                    break;
            }
            return result;
        }
    }
}
