using ATL;
using System;

namespace AudioMerge.App.Models
{
    public class AudioFile
    {
        public string FilePath { get; }

        public string Artist => metaData.Artist;

        public string Title => metaData.Title;

        public string Album => metaData.Album;

        public TimeSpan Duration => TimeSpan.FromSeconds(metaData.Duration);

        private readonly Track metaData;

        public AudioFile(string filePath)
        {
            FilePath = filePath;
            metaData = new Track(FilePath);
        }
    }
}
