namespace FileFormats.DAR;

public class DARHeader
{
    public static byte[] MAGIC = [0x44, 0x41, 0x52, 0x00];
    public byte[] Header { get; set; }
    public uint Version { get; set; }
    public uint PointerCount { get; set; }
    public uint Padding { get; set; }
    public uint[] Pointers { get; set; }
}