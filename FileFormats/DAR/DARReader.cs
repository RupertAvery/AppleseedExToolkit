using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Reflection.PortableExecutable;
using FileFormats.TIM2;

namespace FileFormats.DAR;

public class DARReader
{
    private readonly Stream _stream;

    public DARReader(Stream stream)
    {
        _stream = stream;
    }

    public DAREntry[] GetEntries(long streamOffset, long size)
    {
        _stream.Seek(streamOffset, SeekOrigin.Begin);

        var reader = new EndiannessAwareBinaryReader(_stream);
        var _header = new DARHeader();
        _header.Header = reader.ReadBytes(4);
        if (!_header.Header.SequenceEqual(DARHeader.MAGIC))
        {
            throw new Exception("Invalid DAR Header!");
        }

        _header.Version = reader.ReadUInt32();
        _header.EntryCount = reader.ReadUInt32();
        _header.Reserved = reader.ReadUInt32();
        var Entries = new DAREntry[_header.EntryCount];

        long lastOffset = 0;

        for (var i = 0; i < _header.EntryCount; i++)
        {
            var offset = reader.ReadUInt32();

            if (i > 0)
            {
                Entries[i - 1].Size = (uint)(offset - lastOffset);
                if (Entries[i - 1].Size == 0)
                {
                    var x = 1;
                }
            }

            Entries[i] = new DAREntry()
            {
                StreamOffset = streamOffset,
                Offset = offset,
            };

            lastOffset = offset;
        }


        var position = _stream.Position;

        if (position % 0x10 > 0)
        {
            _stream.Seek(0x10 - (position % 0x10), SeekOrigin.Current);
        }


        if (_header.EntryCount > 0)
        {
            for (var i = 0; i < _header.EntryCount; i++)
            {
                Entries[i].Attribute = reader.ReadByte();
            }
        }


        position = _stream.Position;

        if (position % 0x10 > 0)
        {
            _stream.Seek(0x10 - (position % 0x10), SeekOrigin.Current);
        }



        if (_header.EntryCount > 0)
        {
            //if (size == 0)
            //{
            //    var x = 1;
            //}

            Entries[_header.EntryCount - 1].Size = Math.Max(0, (uint)(size - lastOffset));

            for (var i = 0; i < _header.EntryCount; i++)
            {
                Entries[i].Type = "bin";

                _stream.Seek(streamOffset + Entries[i].Offset, SeekOrigin.Begin);
                var header = new byte[4];
                _stream.Read(header);
                if (header.SequenceEqual(TIM2Header.MAGIC))
                {
                    Entries[i].Type = "tm2";
                }
                else if (header.SequenceEqual(DARHeader.MAGIC))
                {
                    Entries[i].Type = "dar";
                }
            }
        }

        return Entries;
    }

    //public Stream ReadEntry(DAREntry entry)
    //{
    //    long size = entry.Size;

    //    var buffer = new byte[size];

    //    _stream.Seek(entry.StreamOffset + entry.Offset, SeekOrigin.Begin);
    //    _stream.ReadExactly(buffer);

    //    var stream = new MemoryStream(buffer);

    //    stream.Seek(0, SeekOrigin.Begin);

    //    return stream;
    //}

}