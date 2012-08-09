using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newsbin.Remote
{
    public class FilesFileSet
    {
        protected int id;
        protected string subject;
        protected long fileSize;
        protected DateTime timeStamp;
        protected string poster;
        protected string fileName;

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

        public string FileName
        {
            get { return this.fileName; }
        }

        public FilesFileSet(string data)
        {
            string[] strings = data.Split('\t');
            this.id = int.Parse(strings[0]);
            this.subject = strings[1];
            this.fileSize = long.Parse(strings[2]);
            this.timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            this.timeStamp += TimeSpan.FromSeconds(long.Parse(strings[3]));
            this.poster = strings[4];
            this.fileName = strings[5];
        }

        public override string ToString()
        {
            return String.Format("id={0};subject={1};fileSize={2};timeStamp={3};poster={4};fileName={5}",
                                this.id,
                                this.subject,
                                this.fileSize,
                                this.timeStamp,
                                this.poster,
                                this.fileName);
        }
    }
}
