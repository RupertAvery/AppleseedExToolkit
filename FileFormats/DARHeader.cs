namespace FileFormats;

public class DARHeader
{
    public uint Header { get; set; }
    public uint Version { get; set; }
    public uint PointerCount { get; set; }
    public uint Padding { get; set; }
    public uint[] Pointers { get; set; }
}