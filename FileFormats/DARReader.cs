using System;
using System.Reflection;
using System.Reflection.PortableExecutable;

namespace FileFormats;

public class DARReader
{
    private readonly Stream _stream;
    private readonly long _size;
    private DARHeader _header;
    private long _streamOffset = 0;
    public DAREntry[] Entries { get; }
    public uint Blocks => _header.PointerCount;

    public DARReader(Stream stream, long streamOffset, long size)
    {
        _stream = stream;
        _size = size;
        _streamOffset = streamOffset;

        stream.Seek(streamOffset, SeekOrigin.Begin);

        var reader = new EndiannessAwareBinaryReader(stream);
        _header = new DARHeader();
        _header.Header = reader.ReadUInt32();
        _header.Version = reader.ReadUInt32();
        _header.PointerCount = reader.ReadUInt32();
        _header.Padding = reader.ReadUInt32();
        Entries = new DAREntry[_header.PointerCount];

        long lastOffset = 0;

        for (var i = 0; i < _header.PointerCount; i++)
        {
            var offset = reader.ReadUInt32();

            if (i > 0)
            {
                Entries[i - 1].Size = (uint)(offset - lastOffset);
            }

            Entries[i] = new DAREntry()
            {
                StreamOffset = _streamOffset,
                Offset = offset,
            };

            lastOffset = offset;
        }

        Entries[_header.PointerCount - 1].Size = (uint)(_size - lastOffset);

        for (var i = 0; i < _header.PointerCount; i++)
        {
            Entries[i].Type = "bin";

            _stream.Seek(_streamOffset + Entries[i].Offset, SeekOrigin.Begin);
            var header = new byte[4];
            _stream.Read(header);
            if (header.SequenceEqual((byte[])[0x54, 0x49, 0x4D, 0x32]))
            {
                Entries[i].Type = "tm2";
            }
            else if (header.SequenceEqual((byte[])[0x44, 0x41, 0x52, 0x00]))
            {
                Entries[i].Type = "dar";
            }
        }

    }

    public Stream ReadEntry(DAREntry entry)
    {
        long size = entry.Size;

        var buffer = new byte[size];

        _stream.Seek(entry.StreamOffset + entry.Offset, SeekOrigin.Begin);
        _stream.ReadExactly(buffer);

        var stream = new MemoryStream(buffer);

        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }

}

public class TIM2Reader
{
    TIM2Header _header;

    public TIM2Reader(Stream stream)
    {
        var reader = new EndiannessAwareBinaryReader(stream);

        _header = new TIM2Header();

        _header.Header = reader.ReadBytes(4);
        _header.Revision = reader.ReadByte();
        _header.Format = reader.ReadByte();
        _header.PictureCount = reader.ReadUInt16();
        _header.Reserved1 = reader.ReadUInt32();
        _header.Reserved2 = reader.ReadUInt32();

        _header.PictureInfos = new PictureInfo[_header.PictureCount];
        for (var i = 0; i < _header.PictureCount; i++)
        {
            var pictureInfo = new PictureInfo();

            pictureInfo.TotalSize = reader.ReadUInt32();
            pictureInfo.ClutSize = reader.ReadUInt32();
            pictureInfo.ImageSize = reader.ReadUInt32();
            pictureInfo.HeaderSize = reader.ReadUInt16();
            pictureInfo.ColorsUsed = reader.ReadUInt16();
            pictureInfo.PictureFormat = reader.ReadByte();
            pictureInfo.MipmapCount = reader.ReadByte();
            pictureInfo.ClutColorType = reader.ReadByte();
            pictureInfo.ImageColorType = reader.ReadByte();
            pictureInfo.ImageWidth = reader.ReadUInt16();
            pictureInfo.ImageHeight = reader.ReadUInt16();
            pictureInfo.GsTexRegister1 = reader.ReadUInt64();
            pictureInfo.GsTexRegister2 = reader.ReadUInt64();
            pictureInfo.GsFlagsRegister = reader.ReadUInt32();
            pictureInfo.GsClutRegister = reader.ReadUInt32();

            _header.PictureInfos[i] = pictureInfo;
        }

        for (var i = 0; i < _header.PictureCount; i++)
        {
            var pictureInfo = _header.PictureInfos[i];

            pictureInfo.PictureData = reader.ReadBytes((int)pictureInfo.ImageSize);
            pictureInfo.ClutData = reader.ReadBytes((int)pictureInfo.ClutSize);
        }
    }

    public PictureInfo GetPicture(int index)
    {
        return _header.PictureInfos[index];
    }
}

public class TIM2Header
{
    public byte[] Header { get; set; }
    public byte Revision { get; set; }
    public byte Format { get; set; }
    public ushort PictureCount { get; set; }
    public uint Reserved1 { get; set; }
    public uint Reserved2 { get; set; }
    public PictureInfo[] PictureInfos { get; set; }
}

public class Mipmap
{
    public int MiptbpRegister1 { get; set; }     // 00: Miptbp register
    public int MiptbpRegister2 { get; set; }     // 04: Miptbp register
    public int MiptbpRegister3 { get; set; }     // 08: Miptbp register
    public int MiptbpRegister4 { get; set; }     // 0c: Miptbp register
    public int[] Sizes { get; set; } = new int[8]; // 10: Array of sizes
}

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
    public byte[] PictureData { get; set; }
    public byte[] ClutData { get; set; }
}

public enum PixelStorageMode
{
    PSMCT32 = 0,         // RGBA32, uses 32-bit per pixel.
    PSMCT24 = 1,         // RGB24, uses 24-bit per pixel with the upper 8 bits unused.
    PSMCT16 = 2,         // RGBA16 unsigned, packs two pixels in 32-bit in little-endian order.
    PSMCT16S = 10,       // RGBA16 signed, packs two pixels in 32-bit in little-endian order.
    PSMT8 = 19,          // 8-bit indexed, packing 4 pixels per 32-bit.
    PSMT4 = 20,          // 4-bit indexed, packing 8 pixels per 32-bit.
    PSMT8H = 27,         // 8-bit indexed, with the upper 24 bits unused.
    PSMT4HL = 26,        // 4-bit indexed, with the upper 24 bits unused.
    PSMT4HH = 44,        // 4-bit indexed, evaluating bits 4-7 and discarding the rest.
    PSMZ32 = 48,         // 32-bit Z buffer.
    PSMZ24 = 49,         // 24-bit Z buffer with the upper 8 bits unused.
    PSMZ16 = 50,         // 16-bit unsigned Z buffer, packs two pixels in 32-bit in little-endian order.
    PSMZ16S = 58         // 16-bit signed Z buffer, packs two pixels in 32-bit in little-endian order.
}

public enum ColorType
{
    Undefined = 0,       // Undefined
    RGBA16 = 1,          // 16-bit RGBA (A1B5G5R5)
    RGB32 = 2,           // 32-bit RGB (X8B8G8R8)
    RGBA32 = 3,          // 32-bit RGBA (A8B8G8R8)
    Indexed4Bit = 4,     // 4-bit indexed
    Indexed8Bit = 5      // 8-bit indexed
}