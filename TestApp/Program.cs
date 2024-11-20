// See https://aka.ms/new-console-template for more information

using FileFormats;
using System.Text;
using AFSTools;
using FileFormats.AFS;
using Ps2IsoTools.UDF;
using Ps2IsoTools.UDF.Files;


var srcIso = @"D:\roms\ps2\Appleseed EX [NTSC-J] [SLPM-66551].iso";
var targetIso = @"D:\roms\ps2\Test.iso";


//using (var reader = new UdfReader(srcIso))
//{
//    // Get list of all files
//    List<string> fullNames = reader.GetAllFileFullNames();

//    // FileIdentifiers are used to reference files on the ISO
//    FileIdentifier? fileId = reader.GetFileByName("ARC.AFS");

//    if (fileId is not null)
//    {
//        var arcStream = new FileStream(@"D:\roms\ps2\ARC.AFS", FileMode.Create, FileAccess.Write);

//        var fstream = reader.GetFileStream(fileId);

//        fstream.CopyTo(arcStream);

//        arcStream.Flush();
//    }
////}
//BuildISO();
PatchAFS();
//PatchHW();

void BuildISO()
{

    FileStream fs = null;

    using (var editor = new UdfEditor(srcIso))
    {
        var fileId = editor.GetFileByName("ARC.AFS");
        if (fileId is not null)
        {
            //// Write directly to a specific file on the ISO
            //// Does not require Rebuild()
            //using (BinaryWriter bw = new(editor.GetFileStream(fileId)))
            //{
            //    var data = new byte[] { 0xFF, 0xFF };
            //    bw.Write(data);
            //}

            // Replace a file entirely
            // Has optional bool parameter to make a copy of the Stream in RAM (default: false)
            // ISO must be rebuilt with Rebuild() to save changes
            // Does not edit file name, only contents
            //using (FileStream fs = File.Open(@"D:\roms\ps2\ARC.AFS", FileMode.Open))
            //{
            //    editor.ReplaceFileStream(fileId, fs, true);
            //}

            // Remove a file from the ISO
            // Requires Rebuild()
            editor.RemoveFile(fileId);

            // Add a file, same as UdfBuilder
            // Requires Rebuild()
            fs = File.Open(@"D:\roms\ps2\ARC.AFS", FileMode.Open);
            editor.AddFile("ARC.AFS", fs);



            // Add a directory, same as UdfBuilder
            // Requires Rebuild()
            //editor.AddDirectory(@"NewDirectory\DATA");
        }

        // Rebuild the ISO (Creates entirely new meta data using UdfBuilder)
        // Optional string parameter to output a new ISO file
        // Overwriting the current ISO will temporarily cause a spike in RAM usage ~2x total ISO size
        //  as all of the files will be stored in MemoryStreams during the process
        editor.Rebuild(targetIso);

    }

    fs?.Dispose();

}

//var filename = @"D:\roms\ps2\ARC.AFS";
//var filename = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\mf\mf_3.tm2";
//var filename = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw_73\hw_73_30.dar";
//var outname = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw_73\hw_73_30a.dar";
//var patchPath = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw_73_30";


//var filename = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw\hw_73.dar";
//var outname = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw\hw_73a.dar";
//var patchPath = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw_73";


void PatchAFS()
{
    var filename = @"D:\roms\ps2\ARC.AFS";
    var outname = @"D:\roms\ps2\ARC2.AFS";
    var patchPath = @"D:\roms\ps2\Appleseed EX\Project";



    var archive = new AFSArchive(filename);

    archive.Open();

    var builder = new AFSPatcher();

    var fileInfo = new FileInfo(filename);

    var ignoredFiles = new HashSet<string>()
    {
        "gw",
        //"gw_19",
        //"gw_36_12",
        "hw_73_12",
        "hw_73_13",
        "hw_73_1",
        "hw_73_2"
    };


    using var outstream = new FileStream(outname, FileMode.Create, FileAccess.Write);

    builder.Build(archive, outstream, patchPath, ignoredFiles);

    outstream.Flush();
}


void PatchHW()
{
    var filename = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw.dar";
    var outname = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw2.dar";
    var patchPath = @"D:\roms\ps2\Appleseed EX\Project\hw";


    using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

    var builder = new AFSPatcher();

    var fileInfo = new FileInfo(filename);

    var ignoredFiles = new HashSet<string>()
    {
        "hw_73_12",
        "hw_73_13",
        "hw_73_1",
        "hw_73_2"
    };

    using var memoryStream = builder.GetDarEntry(0, fileInfo.Length, stream, "hw", patchPath, ignoredFiles, true);

    using var outstream = new FileStream(outname, FileMode.Create, FileAccess.Write);

    memoryStream.Seek(0, SeekOrigin.Begin);
    memoryStream.CopyTo(outstream);

    outstream.Flush();
}



//var fileInfo = new FileInfo(filename);

//var reader = new AFSArchive(stream, fileInfo.Length);



//var filename = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw.dar";

//using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

//var fileInfo = new FileInfo(filename);

//var reader = new DARReader(stream, fileInfo.Length);

//var block = reader.ReadBlock(73);

//var reader2 = new DARReader(block, block.Length);

//var subblock = reader2.ReadBlock(30);

//var reader3 = new DARReader(subblock, subblock.Length);

//var subblock2 = reader3.ReadBlock(1);

//Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
//var japaneseEncoding = Encoding.GetEncoding("Shift-JIS");
//var japaneseString = japaneseEncoding.GetString(((MemoryStream)subblock2).ToArray());
//Console.OutputEncoding = Encoding.UTF8;
//Console.WriteLine(japaneseString);