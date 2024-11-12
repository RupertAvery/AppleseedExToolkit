using System.Reflection.PortableExecutable;
using System.Text;

namespace FileFormats.AFS;

public class AFSArchive : IDisposable
{
    private readonly Stream _stream;
    private readonly long _size;
    private AFSHeader _header;
    private long _streamOffset = 0;

    public AFSEntry[] Entries { get; private set; }

    private const int MAX_ENTRY_NAME_LENGTH = 0x20;

    public uint Blocks => _header.PointerCount;
    public Stream Stream => _stream;

    public AFSArchive(Stream stream, long size)
    {
        _stream = stream;
        _size = size;
        _streamOffset = stream.Position;
    }

    public AFSArchive(string filename)
    {
        _stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

        var fileInfo = new FileInfo(filename);

        _size = fileInfo.Length;
        _streamOffset = _stream.Position;
    }


    public void Open()
    {
        var reader = new EndiannessAwareBinaryReader(_stream);
        _header = new AFSHeader();

        _header.Header = reader.ReadUInt32();
        _header.PointerCount = reader.ReadUInt32();

        Entries = new AFSEntry[_header.PointerCount];

        for (var i = 0; i < _header.PointerCount; i++)
        {
            Entries[i] = new AFSEntry
            {
                Offset = reader.ReadUInt32(),
                Size = reader.ReadUInt32()
            };
        }

        uint attributeDataOffset = reader.ReadUInt32();
        uint attributeDataSize = reader.ReadUInt32();

        _stream.Seek(attributeDataOffset, SeekOrigin.Begin);

        for (var i = 0; i < _header.PointerCount; i++)
        {
            byte[] name = new byte[MAX_ENTRY_NAME_LENGTH];
            _stream.Read(name, 0, name.Length);

            Entries[i].Name = Encoding.ASCII.GetString(name).Trim(['\0']);

            Entries[i].LastModifiedDate = new DateTime(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16());
            Entries[i].CustomData = reader.ReadUInt32();
        }
    }

    public Stream GetEntryStream(AFSEntry entry)
    {
        long size = entry.Size;

        var buffer = new byte[size];

        _stream.Seek(_streamOffset + entry.Offset, SeekOrigin.Begin);
        _stream.ReadExactly(buffer);

        var stream = new MemoryStream(buffer);

        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }

    public void Dispose()
    {
        _stream.Dispose();
    }

    //public async ValueTask DisposeAsync()
    //{
    //    await _stream.DisposeAsync();
    //}
}