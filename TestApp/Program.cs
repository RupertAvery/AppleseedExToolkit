// See https://aka.ms/new-console-template for more information

using FileFormats;
using System.Text;


//var filename = @"D:\roms\ps2\ARC.AFS";
var filename = @"D:\roms\ps2\FULL_AFS_FILE_DUMP\mf\mf_3.tm2";

using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

var tim2Reader = new TIM2Reader(stream);

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