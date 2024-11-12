using System.IO;

namespace FileFormats.AFS
{
    public class AFSEntry
    {
        public uint Offset { get; set; }
        public uint Size { get; set; }
        public string Name { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public uint CustomData { get; set; }
    }
}
