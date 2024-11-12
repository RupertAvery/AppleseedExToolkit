namespace FileFormats.TIM2;

public class TIM2Header
{
    public static byte[] MAGIC = [0x54, 0x49, 0x4D, 0x32];
    public byte[] Header { get; set; }
    public byte Revision { get; set; }
    public byte Format { get; set; }
    public ushort PictureCount { get; set; }
    public uint Reserved1 { get; set; }
    public uint Reserved2 { get; set; }
    public PictureInfo[] PictureInfos { get; set; }
    public long StreamOffset { get; set; }
}