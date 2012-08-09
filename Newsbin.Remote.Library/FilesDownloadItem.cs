using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newsbin.Remote
{
    public class FilesDownloadItem
    {
        protected string fileName;
        protected long fileSize;

        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
        }

        public long FileSize
        {
            get { return this.fileSize; }
            set { this.fileSize = value; }
        }

        public FilesDownloadItem(string value)            
        {
            string[] strings = value.Split('"');
            this.fileName = strings[1];
            this.fileSize = long.Parse(strings[2]);

        }
    }
}
