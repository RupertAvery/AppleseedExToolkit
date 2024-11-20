using FileFormats.AFS;
using FileFormats.DAR;
using System.Text;

namespace AFSTools;

public class AFSPatcher
{
    private static byte[] AFSHeader = [0x41, 0x46, 0x53, 0x00];
    private const int AFS_ENTRY_PADDING = 0x800;

    public void Build(AFSArchive archive, Stream outStream, string patchPath, HashSet<string> ignoredFiles)
    {
        var entries = archive.Entries;

        var writer = new BinaryWriter(outStream);

        writer.Write(AFSHeader);
        writer.Write((UInt32)entries.Length);

        var offset = entries[0].Offset;

        var offsets = new uint[entries.Length];
        var sizes = new uint[entries.Length];

        for (var i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            
            var name = Path.GetFileNameWithoutExtension(entry.Name);

            if (entry.Name.EndsWith(".dar") && !ignoredFiles.Contains(name))
            {
                offsets[i] = offset;
                var size = ComputeDarSize(entry.Offset, entry.Size, archive.Stream, name, Path.Combine(patchPath, name), ignoredFiles);
                sizes[i] = size;
                offset += size;
                offset = Util.Pad(offset, AFS_ENTRY_PADDING);
            }
            else
            {
                offsets[i] = offset;
                sizes[i] = entry.Size;
                offset += entry.Size;
                offset = Util.Pad(offset, AFS_ENTRY_PADDING);
            }
        }

        //UpdateEntries(project, entries, afsStream);

        for (var i = 0; i < entries.Length; i++)
        {
            writer.Write((UInt32)offsets[i]);
            writer.Write((UInt32)sizes[i]);
        }

        offset = Util.Pad(offset, AFS_ENTRY_PADDING);

        var attributeOffset = offset;
        var attributeSize = entries.Length * 0x30;

        writer.Write(attributeOffset);
        writer.Write(attributeSize);

        outStream.Seek(entries[0].Offset, SeekOrigin.Begin);

        for (var i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];

            outStream.Seek(offsets[i], SeekOrigin.Begin);

            var name = Path.GetFileNameWithoutExtension(entry.Name);

            // Check if a folder exists in the project files
            if (entry.Name.EndsWith(".dar") && !ignoredFiles.Contains(name))
            {
                // Check if patched. If true, update the last updated date

                var stream = GetDarEntry(entry.Offset, entry.Size, archive.Stream, name, Path.Combine(patchPath, name), ignoredFiles, true);
                
                stream.Seek(0, SeekOrigin.Begin);

                stream.CopyTo(outStream);
            }
            else

            {
                archive.Stream.Seek(entry.Offset, SeekOrigin.Begin);
                var buffer = new byte[entry.Size];
                archive.Stream.Read(buffer);
                writer.Write(buffer);
            }

            //entries[i].Offset;
            //entries[i].Size;
        }

        offset = (UInt32)outStream.Position;

        if (offset % 0x1000 > 0)
        {
            var padding = 0x1000 - offset % 0x1000;
            var buffer = new byte[padding];
            writer.Write(buffer);
            offset += padding;
        }

        offset = (UInt32)outStream.Position;

        for (var i = 0; i < entries.Length; i++)
        {
            var buffer = new byte[0x20];
            var entry = entries[i];
            Encoding.ASCII.GetBytes(entry.Name, 0, entry.Name.Length, buffer, 0);
            writer.Write(buffer);
            writer.Write((UInt16)entry.LastModifiedDate.Year);
            writer.Write((UInt16)entry.LastModifiedDate.Month);
            writer.Write((UInt16)entry.LastModifiedDate.Day);
            writer.Write((UInt16)entry.LastModifiedDate.Hour);
            writer.Write((UInt16)entry.LastModifiedDate.Minute);
            writer.Write((UInt16)entry.LastModifiedDate.Second);
            writer.Write(entry.CustomData);
        }

        offset = (UInt32)outStream.Position;

        if (offset % 0x800 > 0)
        {
            var padding = 0x800 - offset % 0x800;
            var buffer = new byte[padding];
            writer.Write(buffer);
            offset += padding;
        }
    }


    public uint ComputeDarSize(long offset, long size, Stream stream, string name, string patchPath, HashSet<string> ignoredFiles)
    {
        stream.Seek(offset, SeekOrigin.Begin);

        var reader = new DARReader(stream);

        var entries = reader.GetEntries(offset, size);

        var headerSize = 0x10;

        //long baseOffset = headerSize + entries.Length * 4;
        //var padding = 0x10 - baseOffset % 0x10;
        //baseOffset += padding;
        //// Dummy
        //baseOffset += 0x10;

        if (entries.Length == 0) return 0x10;

        var initialOffset = entries[0].Offset;

        //long baseOffset = entries[0].Offset;

        var entrySizes = new uint[entries.Length];

        for (var i = 0; i < entries.Length; i++)
        {
            var filename = $"{name}_{i}";

            var filenameext = $"{filename}.{entries[i].Type}";

            var patchFile = Path.Combine(patchPath, filenameext);

            if (File.Exists(patchFile))
            {
                var fileInfo = new FileInfo(patchFile);
                entrySizes[i] = (uint)fileInfo.Length;
            }
            else
            {
                var darEntry = entries[i];

                if (darEntry.Type == "dar" && !ignoredFiles.Contains(filename))
                {
                    //if (filename == "hw_73" || filename == "hw_74" || filename == "hw_73_30")
                    //{
                    //    var x = 1;
                    //}

                    var len = ComputeDarSize(darEntry.StreamOffset + darEntry.Offset, darEntry.Size, stream, filename, Path.Combine(patchPath, filename), ignoredFiles);

                    entrySizes[i] = (uint)len;
                }
                else
                {
                    entrySizes[i] = (uint)darEntry.Size;
                }
            }
        }

        //int darSize = headerSize + entries.Length * 4 + padding + dummy;

        uint darSize = initialOffset;

        // Write Data
        for (var i = 0; i < entries.Length; i++)
        {
            darSize += entrySizes[i];
        }

        //var outPadding = darSize % 0x10;

        //if (outPadding > 0)
        //{
        //    darSize += 0x10 - outPadding;
        //}

        darSize = Util.Pad(darSize, 0x10);

        return darSize;
    }

    public Stream GetDarEntry(long offset, long size, Stream stream, string name, string patchPath, HashSet<string> ignoredFiles, bool topLevel)
    {
        // get the original DAR to enumerate the entries
        stream.Seek(offset, SeekOrigin.Begin);

        var reader = new DARReader(stream);

        var entries = reader.GetEntries(offset, size);

        var headerSize = 0x10;

        //long baseOffset = headerSize + entries.Length * 4;
        //var padding = 0x10 - baseOffset % 0x10;
        //baseOffset += padding;
        //// Dummy
        //baseOffset += 0x10;

        if (entries.Length == 0)
        {
            var outputStream2 = new MemoryStream();

            stream.Seek(offset, SeekOrigin.Begin);
            var buffer = new byte[0x10];
            stream.Read(buffer);
            outputStream2.Write(buffer);
            return outputStream2;
        }

        var initialOffset = entries[0].Offset;

        long baseOffset = entries[0].Offset;

        var newOffsets = new uint[entries.Length];

        for (var i = 0; i < entries.Length; i++)
        {
            var filename = $"{name}_{i}";
            var filenameext = $"{filename}.{entries[i].Type}";

            var patchFile = Path.Combine(patchPath, filenameext);

            if (File.Exists(patchFile))
            {
                var fileInfo = new FileInfo(patchFile);

                entries[i] = new DARPatchEntry()
                {
                    Path = patchFile,
                };

                newOffsets[i] = (uint)baseOffset;

                baseOffset += fileInfo.Length;

            }
            else
            {
                var darEntry = entries[i];

                newOffsets[i] = (uint)baseOffset;

                if (darEntry.Type == "dar" && !ignoredFiles.Contains(filename))
                {
                    //if (filename == "hw_73" || filename == "hw_74" || filename == "hw_73_30")
                    //{
                    //    var x = 1;
                    //}

                    var darSize = ComputeDarSize(darEntry.StreamOffset + darEntry.Offset, darEntry.Size, stream, filename, Path.Combine(patchPath, filename), ignoredFiles);

                    baseOffset += darSize;
                }
                else
                {
                    baseOffset += darEntry.Size;
                }
            }
        }


        var outputStream = new MemoryStream();

        var writer = new BinaryWriter(outputStream);

        // Write Header
        writer.Write(DARHeader.MAGIC);
        writer.Write((uint)1);
        writer.Write((uint)entries.Length);
        writer.Write((uint)(topLevel ? 1 :0));

        // Write Offsets
        for (var i = 0; i < entries.Length; i++)
        {
            writer.Write((UInt32)newOffsets[i]);
        }

        var padding = initialOffset - outputStream.Position;

        stream.Seek(offset + outputStream.Position, SeekOrigin.Begin);
        var buffer3 = new byte[padding];
        stream.Read(buffer3);
        writer.Write(buffer3);

        //writer.Write(new byte[padding]);

        // Write Dummy???
        //var dummy = new byte[0x10];
        //writer.Write(dummy);

        // Write Data
        for (var i = 0; i < entries.Length; i++)
        {
            var darEntry = entries[i];

            var filename = $"{name}_{i}";

            var filenameext = $"{filename}.{entries[i].Type}";

            if (newOffsets[i] != outputStream.Position)
            {
                throw new Exception("Offsets do not match!");
            }

            if (darEntry is DARPatchEntry patchEntry)
            {
                var buffer = File.ReadAllBytes(patchEntry.Path);
                writer.Write(buffer);
            }
            else
            {
                if (darEntry.Type == "dar" && !ignoredFiles.Contains(filename))
                {
                    using var rstream = GetDarEntry(darEntry.StreamOffset + darEntry.Offset, darEntry.Size, stream, filename, Path.Combine(patchPath, filename), ignoredFiles, false);

                    rstream.Position = 0;

                    rstream.CopyTo(outputStream);
                }
                else
                {
                    // Copy DAR entry to output stream
                    stream.Seek(darEntry.StreamOffset + darEntry.Offset, SeekOrigin.Begin);
                    var buffer = new byte[darEntry.Size];
                    stream.Read(buffer);
                    writer.Write(buffer);
                }

            }
        }

        var outPadding = outputStream.Position % 0x10;

        if (outPadding > 0)
        {
            writer.Write(new byte[0x10 - outPadding]);
        }

        return outputStream;
    }

}