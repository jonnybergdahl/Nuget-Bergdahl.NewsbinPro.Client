using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newsbin.Remote
{
    public class GroupFileSet
    {
        protected int id;
        protected string subject;
        protected long fileSize;
        protected DateTime timeStamp;
        protected string poster;
        protected int fileCount;
        protected int parCount;
        protected bool isOld;

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

        public int FileCount
        {
            get { return this.fileCount; }
        }

        public int ParCount
        {
            get { return this.parCount; }
        }

        public bool IsOld
        {
            get { return this.isOld; }
        }

        public GroupFileSet(string data)
        {
            string[] strings = data.Split('\t');
            this.id = int.Parse(strings[0]);
            this.subject = strings[1];
            this.fileSize = long.Parse(strings[2]);
            this.timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            this.timeStamp += TimeSpan.FromSeconds(long.Parse(strings[3]));
            this.poster = strings[4];
            this.fileCount = int.Parse(strings[5]);
            this.parCount = int.Parse(strings[6]);
            this.isOld = strings[7] == "0";
        }

        public override string ToString()
        {
            return String.Format("id={0};subject={1};fileSize={2};timeStamp={3};poster={4};fileCount={5};parCount={6};isOld={7}",
                                this.id,
                                this.subject,
                                this.fileSize,
                                this.timeStamp,
                                this.poster,
                                this.fileCount,
                                this.parCount,
                                this.isOld);
        }
    }
}
