namespace FileFormats.TIM2;

public class PictureInfo
{
    public uint TotalSize { get; set; }           // 00: Total size in bytes used by the picture
    public uint ClutSize { get; set; }            // 04: Clut size in bytes used by the palette
    public uint ImageSize { get; set; }           // 08: Image size in bytes used by the bitmap
    public ushort HeaderSize { get; set; }        // 0c: Header size
    public ushort ColorsUsed { get; set; }        // 0e: Number of colors used by the clut
    public byte PictureFormat { get; set; }      // 10: Picture format
    public byte MipmapCount { get; set; }        // 11: Mipmap count
    public byte ClutColorType { get; set; }      // 12: Clut color type
    public byte ImageColorType { get; set; }     // 13: Image color type
    public ushort ImageWidth { get; set; }        // 14: Image width in pixels
    public ushort ImageHeight { get; set; }       // 16: Image height in pixels
    public ulong GsTexRegister1 { get; set; }     // 18: GsTex register (1)
    public ulong GsTexRegister2 { get; set; }     // 20: GsTex register (2)
    public uint GsFlagsRegister { get; set; }     // 28: Gs flags register
    public uint GsClutRegister { get; set; }      // 2c: Gs clut register
    public long StreamOffset { get; set; }
}

public class PictureData
{
    public byte[] PixelData { get; set; }
    public byte[] ClutData { get; set; }

}