using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newsbin.Remote
{
    public class DownloadPartStatus
    {
        protected int fileCount;
        protected int parCount;
        protected int downloaded;
        protected int downloading;
        protected int incomplete;
        protected int paused;
        protected int retries;

        public int FileCount
        {
            get { return this.fileCount; }
        }

        public int ParCount
        {
            get { return this.parCount; }
        }

        public int Downloaded
        {
            get { return this.downloaded; }
        }

        public int Downloading
        {
            get { return this.downloading; }
        }

        public int Incomplete
        {
            get { return this.incomplete; }
        }

        public int Paused
        {
            get { return this.paused; }
        }

        public int Retries
        {
            get { return this.retries; }
        }

        public DownloadPartStatus()
        { 
        }

        // [102 files, 16 pars] D:36 DL:4 In:0 P:15 R:0
        public DownloadPartStatus(string data)
        {
            if (data.IndexOf(']') == -1)
                return;
            string temp = data.Substring(0, data.IndexOf(']'));
            string[] parts = temp.Split(' ');
            if (parts.Length != 4)
                return;

            this.fileCount = int.Parse(parts[0].Substring(1));
            this.parCount = int.Parse(parts[2]);

            temp = data.Substring(data.IndexOf(']') + 1);
            parts = temp.Split(' ');
            foreach (string part in parts)
            {
                string[] subParts = part.Split(':');
                switch (subParts[0])
                {
                    case "D":
                        this.downloaded = int.Parse(subParts[1]);
                        break;
                    case "DL":
                        this.downloading = int.Parse(subParts[1]);
                        break;
                    case "In":
                        this.incomplete = int.Parse(subParts[1]);
                        break;
                    case "P":
                        this.paused = int.Parse(subParts[1]);
                        break;
                    case "R":
                        this.retries = int.Parse(subParts[1]);
                        break;
                }
            }
        }

        public override string ToString()
        {
            return String.Format("fileCount:{0};parCount:{1};downloaded:{2};downloading:{3};incomplete:{4};paused:{5};retries:{6}",
                                this.fileCount,
                                this.parCount,
                                this.downloaded,
                                this.downloading,
                                this.incomplete,
                                this.paused,
                                this.retries);
        }
    }
}
