namespace FileFormats.DAR;

public class DAREntry
{
    public uint Offset { get; set; }
    public uint Size { get; set; }
    public long StreamOffset { get; set; }
    public string Type { get; set; }
    public byte Attribute { get; set; }
}