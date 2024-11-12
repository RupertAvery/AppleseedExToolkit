// See https://aka.ms/new-console-template for more information

using FileFormats;
using System.Text;
using AFSTools;


//var filename = @"D:\roms\ps2\ARC.AFS";
//var filename = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\mf\mf_3.tm2";
//var filename = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw_73\hw_73_30.dar";
//var outname = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw_73\hw_73_30a.dar";
//var patchPath = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw_73_30";


//var filename = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw\hw_73.dar";
//var outname = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw\hw_73a.dar";
//var patchPath = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw_73";

var filename = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw.dar";
var outname = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw2.dar";
var patchPath = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\hw";


using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

var builder = new AFSBuilder();

var fileInfo = new FileInfo(filename);

var ignoredFiles = new HashSet<string>()
{
    "hw_73_12",
    "hw_73_13",
    "hw_73_1",
    "hw_73_2"
};

using var memoryStream = builder.GetDarEntry(0, fileInfo.Length, stream, "hw", patchPath, ignoredFiles);

using var outstream = new FileStream(outname, FileMode.Create, FileAccess.Write);

memoryStream.Seek(0, SeekOrigin.Begin);
memoryStream.CopyTo(outstream);

outstream.Flush();


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