using FileFormats.AFS;
using FileFormats.DAR;
using System;
using System.Formats.Tar;
using System.IO;
using System.Reflection.PortableExecutable;

namespace AFSTools;

public class AFSBuilder
{

    public void Build(ApexProject project, Stream afsStream, AFSArchive archive, Stream outStream)
    {
        var entries = archive.Entries;

        var writer = new BinaryWriter(outStream);

        writer.Write((byte[])[44, 44, 44, 44]);
        writer.Write((UInt16)entries.Length);

        UpdateEntries(project, entries, afsStream);

        for (var i = 0; i < entries.Length; i++)
        {
            writer.Write((UInt32)entries[i].Offset);
            writer.Write((UInt32)entries[i].Size);
        }

        //uint attributeDataOffset = reader.ReadUInt32();
        //uint attributeDataSize = reader.ReadUInt32();

        //foreach (var entry in Entries)
        //{

        //}
    }

    private void UpdateEntries(ApexProject project, AFSEntry[] entries, Stream afsStream)
    {
        for (var i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
        }



        for (var i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];

            // Check if a folder exists in the project files
            if (entry.Name.EndsWith(".dar"))
            {
                GetDarEntry(entry.Offset, entry.Size, afsStream, "", "", null);
            }
            else
            {
                afsStream.Seek(entry.Offset, SeekOrigin.Begin);
                var buffer = new byte[entry.Size];
                afsStream.Read(buffer);
            }

            //entries[i].Offset;
            //entries[i].Size;
        }

    }

    public uint ComputeDarSize(long offset, long size, Stream stream, string name, string patchPath)
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

                if (darEntry.Type == "dar" && filename != "hw_73_12" && filename != "hw_73_13" && filename != "hw_73_1" && filename != "hw_73_2")
                {
                    //if (filename == "hw_73" || filename == "hw_74" || filename == "hw_73_30")
                    //{
                    //    var x = 1;
                    //}

                    var len = ComputeDarSize(darEntry.StreamOffset + darEntry.Offset, darEntry.Size, stream, filename, Path.Combine(patchPath, filename));

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

        var outPadding = darSize % 0x10;

        if (outPadding > 0)
        {
            darSize += 0x10 - outPadding;
        }

        return darSize;
    }

    public Stream GetDarEntry(long offset, long size, Stream stream, string name, string patchPath, HashSet<string> ignoredFiles)
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

                    var darSize = ComputeDarSize(darEntry.StreamOffset + darEntry.Offset, darEntry.Size, stream, filename, Path.Combine(patchPath, filename));

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
        writer.Write((uint)0);

        // Write Offsets
        for (var i = 0; i < entries.Length; i++)
        {
            writer.Write((UInt32)newOffsets[i]);
        }

        var padding = initialOffset - outputStream.Position;

        writer.Write(new byte[padding]);

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
                    using var rstream = GetDarEntry(darEntry.StreamOffset + darEntry.Offset, darEntry.Size, stream, filename, Path.Combine(patchPath, filename), ignoredFiles);

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