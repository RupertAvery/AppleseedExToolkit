namespace FileFormats;

public class AFSAttribute
{
    public uint Header { get; set; }
    public uint PointerCount { get; set; }
    public uint Unknown1 { get; set; }
    public uint Unknown2 { get; set; }
    public uint[] Pointers { get; set; }
    public uint[] Sizes { get; set; }
}