using System.Reflection.PortableExecutable;

namespace FileFormats.TIM2;

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

        var streamOffset = stream.Position;
        _header.StreamOffset = streamOffset;
    }

    public (PictureInfo, PictureData) GetPicture(int index, Stream stream)
    {
        var offset = _header.StreamOffset;

        for (var i = 0; i < index; i++)
        {
            offset += _header.PictureInfos[index].TotalSize;
        }

        var pictureInfo = _header.PictureInfos[index];

        stream.Seek(offset, SeekOrigin.Begin);

        var data = new PictureData();

        var reader = new EndiannessAwareBinaryReader(stream);

        data.PixelData = reader.ReadBytes((int)pictureInfo.ImageSize);
        data.ClutData = reader.ReadBytes((int)pictureInfo.ClutSize);

        var colorStorageMode = pictureInfo.GsTexRegister1 >> 55 & 1;

        if (colorStorageMode == 0)
        {
            // Swizzle 2 blocks of 0x20 bytes after every 0x20 bytes

            var k = 0x20;

            while (k < data.ClutData.Length)
            {
                var temp = new byte[0x20];
                var e = k + 0x20;

                Array.Copy(data.ClutData[k..e], temp, 0x20);

                if (k + 0x20 < data.ClutData.Length)
                {
                    for (var j = 0; j < 0x20; j++)
                    {
                        data.ClutData[k + j] = data.ClutData[k + j + 0x20];
                    }

                    k += 0x20;
                    for (var j = 0; j < 0x20; j++)
                    {
                        data.ClutData[k + j] = temp[j];
                    }

                    k += 0x60;
                }
                else
                {
                    break;
                }
            }
        }

        if (pictureInfo.ImageColorType == 4)
        {
            // Swizzle pixel data when 4bpp
            for (var i = 0; i < data.PixelData.Length; i++)
            {
                // Swap high and low nybbles
                var temp = data.PixelData[i];
                temp = (byte)(((temp & 0x0F) << 4) | ((temp & 0xF0) >> 4));
                data.PixelData[i] = temp;
            }
        }

        return (pictureInfo, data);
    }
}