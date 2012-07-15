using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newsbin.Remote
{
    public class NewsbinStatus
    {
        protected int speed;
        protected long downloadFolderFreeSpace;
        protected long dataFolderFreeSpace;
        protected long queuedFileSize;
        protected readonly List<long> speedSamples;

        public int Speed
        {
            get { return this.speed; }
        }

        public long DownloadFolderFreeSpace
        {
            get { return this.downloadFolderFreeSpace; }
        }

        public long DataFolderFreeSpace
        {
            get { return this.dataFolderFreeSpace; }
        }

        public long QueuedFileSize
        {
            get { return this.queuedFileSize; }
        }

        public List<long> SpeedSamples
        {
            get { return this.speedSamples; }
        }

        public NewsbinStatus()
        {
            speedSamples = new List<long>();
        }

        public void Add(string data)
        {
            string[] strings = data.Split('\t');
            switch (strings[0])
            {
                case "Speed":
                    this.speed = int.Parse(strings[1]);
                    break;
                case "DownloadFolder":
                    this.downloadFolderFreeSpace = long.Parse(strings[1]);
                    break;
                case "DataFolder":
                    this.dataFolderFreeSpace = long.Parse(strings[1]);
                    break;
                case "Queued":
                    this.queuedFileSize = long.Parse(strings[1]);
                    break;
                case "Speed Samples":
                    for (int i = 1; i < strings.Length; i++)
                    {
                        this.speedSamples.Add(long.Parse(strings[i]) * 1000);
                    }
                    break;
            }
        }
    }
}
