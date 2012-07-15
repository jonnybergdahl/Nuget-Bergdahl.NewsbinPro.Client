using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newsbin.Remote
{
    public class WishFileSet
    {
        protected int id;
        protected string subject;
        protected long fileSize;
        protected DateTime timeStamp;
        protected string poster;
        protected string downloadPath;
        protected string statusString;
        protected int fileCount;
        protected int parCount;

        public string DownloadPath
        {
            get { return this.downloadPath; }
        }

        public string StatusString
        {
            get { return this.statusString; }
        }

        public int FileCount
        {
            get { return this.fileCount; }
        }

        public int ParCount
        {
            get { return this.parCount; }
        }

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

        public WishFileSet(string data)
        {
            string[] strings = data.Split('\t');
            this.id = int.Parse(strings[0]);
            this.subject = strings[1];
            this.fileSize = long.Parse(strings[2]);
            this.timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            this.timeStamp += TimeSpan.FromSeconds(long.Parse(strings[3]));
            this.poster = strings[4];
            this.downloadPath = strings[5];
            this.fileSize = long.Parse(strings[7]);
            this.statusString = strings[8];
            string[] subStrings = this.statusString.Split(']');
            subStrings = subStrings[0].Split(' ');
            this.fileCount = int.Parse(subStrings[0]);
            this.parCount = int.Parse(subStrings[2]);
        }
    }
}
