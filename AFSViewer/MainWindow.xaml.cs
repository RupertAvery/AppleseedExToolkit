using System;
using System.ComponentModel;
using System.Drawing;
using System.Formats.Tar;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using AFSTools;
using FileFormats;
using FileFormats.AFS;
using FileFormats.DAR;
using FileFormats.TIM2;
using Microsoft.Win32;
using Color = System.Windows.Media.Color;
using Path = System.IO.Path;

namespace AFSViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _model;

    private Stream? _source = null;

    private ApexProject? _apexProject;

    public MainWindow()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


        _model = new MainWindowViewModel();
        InitializeComponent();
        DataContext = _model;

        _model.OpenArchiveCommand = new AsyncCommand<object>((o) => OpenArchive());
        _model.SetDataViewCommand = new RelayCommand<string>(SetDataView);
        _model.ExtractNodeCommand = new RelayCommand<object>((o) => ExtractNode());

        Closing += OnClosing;
        //var filename = @"D:\roms\ps2\ARC.AFS";
        //LoadArchive(filename);

        _apexProject = new ApexProject();
        _apexProject.Path = "D:\\roms\\ps2\\Appleseed EX\\Project";

        _model.PropertyChanged += ModelOnPropertyChanged;
    }

    private void ExtractNode()
    {
        if (_model.SelectedNode == null) return;
        try
        {
            if (_model.SelectedNode.Type == "tm2")
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG files|*.png|JPG files|*.jpg|BMP files|*.bmp|All files|*.*";
                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(_model.SelectedNode.Name) + ".png";

                var result = saveFileDialog.ShowDialog(this);

                if (result.HasValue && result.Value)
                {
                    using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        _source.Seek(_model.SelectedNode.Offset, SeekOrigin.Begin);

                        var source = GetBitmapSourceFromTim(_source);

                        var extension = Path.GetExtension(saveFileDialog.FileName).ToLower();

                        BitmapEncoder encoder = extension switch
                        {
                            ".jpg" => new JpegBitmapEncoder(),
                            ".png" => new PngBitmapEncoder(),
                            ".bmp" => new BmpBitmapEncoder(),
                            _ => throw new Exception("Unsupported file type")
                        };

                        encoder.Frames.Add(BitmapFrame.Create(source));
                        encoder.Save(fileStream);
                    }
                }
            }

            else
            {
                var filter = "";

                var saveFileDialog = new SaveFileDialog();

                if (_model.SelectedNode.Type == "dar")
                {
                    filter = "DAR files|*.dar|";
                }
                else
                {
                }

                saveFileDialog.FileName = _model.SelectedNode.Name;

                saveFileDialog.Filter = filter + "All files|*.*";

                var result = saveFileDialog.ShowDialog(this);

                if (result.HasValue && result.Value)
                {
                    using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        _source.Seek(_model.SelectedNode.Offset, SeekOrigin.Begin);

                        _source.CopyTo(fileStream);

                        fileStream.Flush();
                    }
                }
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Error Extracting Resource", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            throw;
        }
    }


    private void SetDataView(string s)
    {
        _model.BinView = s;
    }

    private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.BinView))
        {
            UpdateView(_model.SelectedNode);
        }
        if (e.PropertyName == nameof(MainWindowViewModel.EditText))
        {
            TrySaveApex(_model.SelectedNode);
        }
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        _source?.Dispose();
    }

    private bool _isLoadingItem = false;

    private void TrySaveApex(TreeNode? node)
    {
        if (_isLoadingItem) return;
        if (node == null) return;

        if (!_apexProject.Files.TryGetValue(node.Path, out var apexFile))
        {
            apexFile = new ApexFile()
            {
                Path = node.Path,
                Type = _model.BinView,
            };

            _apexProject.Files.Add(node.Path, apexFile);
        }


        var path = Path.Combine(_apexProject.Path, apexFile.Path + ".txt");

        var dir = Path.GetDirectoryName(path);

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using var file = new FileStream(path, FileMode.Create, FileAccess.Write);

        var writer = new StreamWriter(file, Encoding.UTF8);

        writer.Write(_model.EditText);

        writer.Flush();

        file.Flush();
    }

    private async Task OpenArchive()
    {
        var dialog = new OpenFileDialog();

        dialog.Filter = "AFS files|*.afs|DAR files|*.dar|All files|*.*";

        var result = dialog.ShowDialog(this);

        if (result is true)
        {
            await Task.Run(() => LoadArchive(dialog.FileName));
        }
    }

    private void LoadArchive(string filename)
    {
        var nodes = new List<TreeNode>();


        var fileInfo = new FileInfo(filename);

        _source?.Dispose();

        _source = new FileStream(filename, FileMode.Open, FileAccess.Read);


        if (filename.ToLower().EndsWith(".afs"))
        {
            var afsArchive = new AFSArchive(_source, fileInfo.Length);

            afsArchive.Open();

            foreach (var entry in afsArchive.Entries)
            {
                var name = Path.GetFileNameWithoutExtension(entry.Name);

                var node = new TreeNode()
                {
                    Offset = entry.Offset,
                    Name = entry.Name,
                    Size = entry.Size,
                    Path = name,
                    Type = Path.GetExtension(entry.Name)[1..].ToLower()
                };

                if (node.Type == "dar")
                {
                    node.Children = new List<TreeNode>() { new TreeNode() };
                }

                nodes.Add(node);
            }
        }
        else

        if (filename.ToLower().EndsWith(".dar"))
        {
            var reader = new DARReader(_source);

            var entries = reader.GetEntries(0, fileInfo.Length);

            // afsArchive.Open();
            var index = 0;

            var parentName = System.IO.Path.GetFileNameWithoutExtension(filename);

            foreach (var entry in entries)
            {
                var childname = $"{parentName}_{index}";

                var child = new TreeNode()
                {
                    Offset = entry.StreamOffset + entry.Offset,
                    Name = $"{childname}.{entry.Type}",
                    Path = $"{parentName}\\{childname}",
                    Size = entry.Size,
                    Type = entry.Type
                };

                if (child.Type == "dar")
                {
                    child.Children = new List<TreeNode>() { new TreeNode() };
                }

                nodes.Add(child);

                index++;
            }

        }

        Dispatcher.Invoke(() => { _model.Nodes = nodes; });

        //Dispatcher.Invoke(() => { _model.Nodes = nodes.OrderBy(d => d.Name); });
    }

    private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
    {
        if (((TreeViewItem)e.OriginalSource).DataContext is TreeNode { Type: "dar" } node && !node.IsLoaded)
        {
            try
            {
                var reader = new DARReader(_source);

                var entries = reader.GetEntries(node.Offset, node.Size);

                var nodes = new List<TreeNode>();
                var parentName = System.IO.Path.GetFileNameWithoutExtension(node.Name);
                var index = 0;

                foreach (var entry in entries)
                {
                    var childname = $"{parentName}_{index}";

                    var child = new TreeNode()
                    {
                        Offset = entry.StreamOffset + entry.Offset,
                        Name = $"{childname}.{entry.Type}",
                        Path = $"{node.Path}\\{childname}",
                        Size = entry.Size,
                        Type = entry.Type
                    };

                    if (child.Type == "dar")
                    {
                        child.Children = new List<TreeNode>() { new TreeNode() };
                    }

                    nodes.Add(child);

                    index++;
                }

                node.Children = nodes;
                node.IsLoaded = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, "Failed to decode", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is TreeViewItem tvi)
        {
            if (!tvi.IsSelected)
            {
                return;
            }


            if (tvi.DataContext is DAREntry darEntry)
            {

            }
        }
    }

    private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is TreeNode node)
        {
            _model.SelectedNode = node;
            UpdateView(node);
        }
    }

    private void UpdateView(TreeNode? node)
    {
        _isLoadingItem = true;

        _model.Item1 = "";
        _model.Item2 = "";
        _model.Item3 = "";
        _model.Item4 = "";
        _model.Data = "";
        _model.Image = null;
        _model.EditText = "";

        _model.ImageVisibility = Visibility.Hidden;
        _model.TextVisibility = Visibility.Visible;

        if (node == null) return;

        switch (node.Type)
        {
            case "tm2":
                {
                    _source.Seek(node.Offset, SeekOrigin.Begin);

                    try
                    {
                        _model.Image = GetBitmapSourceFromTim(_source);
                        _model.ImageVisibility = Visibility.Visible;
                        _model.TextVisibility = Visibility.Hidden;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(this, "Failed to decode", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    break;
                }
            case "bin":
                {
                    _source.Seek(node.Offset, SeekOrigin.Begin);

                    var buffer = new byte[node.Size];
                    _source.Read(buffer);

                    switch (_model.BinView)
                    {
                        case "Shift-JIS":
                            _model.Data = DecodeShiftJIS(buffer);

                            if (_apexProject.Files.TryGetValue(node.Path, out var apexFile))
                            {
                                var path = Path.Combine(_apexProject.Path, apexFile.Path + ".txt");

                                if (File.Exists(path))
                                {
                                    _model.EditText = File.ReadAllText(path, Encoding.UTF8);
                                }

                            }
                            else
                            {
                                var path = Path.Combine(_apexProject.Path, node.Path + ".txt");

                                if (File.Exists(path))
                                {
                                    _model.EditText = File.ReadAllText(path, Encoding.UTF8);
                                }
                            }

                            break;

                        case "Hexadecimal":
                            {
                                var sb = new StringBuilder();

                                var width = 0x10;

                                var c = 0;
                                var start = 0;
                                var hexes = new StringBuilder();
                                var chars = new StringBuilder();

                                foreach (var b in buffer)
                                {
                                    hexes.Append($"{b:X2} ");
                                    if (b == '\r' || b == '\n' || b < 0x20 || b > 0x7F)
                                    {
                                        chars.Append($".");
                                    }
                                    else
                                    {
                                        chars.Append($"{(char)b}");
                                    }
                                    c++;
                                    if (c == width)
                                    {
                                        c = 0;
                                        sb.Append($"{start:X4}: ");
                                        sb.Append(hexes);
                                        sb.Append("  ");
                                        sb.Append(chars);
                                        sb.AppendLine();
                                        chars.Clear();
                                        hexes.Clear();
                                        start += width;
                                    }
                                }

                                _model.Data = sb.ToString();
                                break;
                            }

                        case "Item":
                            {
                                if (buffer.Length > 0x11F)
                                {

                                    _model.Item1 = DecodeShiftJIS(buffer.AsSpan(0xA0, 0x20));
                                    _model.Item2 = DecodeShiftJIS(buffer.AsSpan(0xC0, 0x30));
                                    _model.Item3 = DecodeShiftJIS(buffer.AsSpan(0xF0, 0x20));
                                    _model.Item4 = DecodeShiftJIS(buffer.AsSpan(0x110, 0x10));
                                }

                                break;
                            }
                    }
                    break;
                }
        }

        _isLoadingItem = false;

    }

    private static BitmapSource GetBitmapSourceFromTim(Stream stream)
    {
        var reader = new TIM2Reader(stream);

        var (picture, data) = reader.GetPicture(0, stream);

        BitmapPalette? palette = null;

        // Generate a palette for indexed image types
        if (picture.ImageColorType == 4 || picture.ImageColorType == 5)
        {
            var colors = new List<Color>(data.ClutData.Length);

            for (var i = 0; i < data.ClutData.Length; i += 4)
            {
                colors.Add(Color.FromArgb(
                    (byte)(data.ClutData[i + 3] == 128 ? 255 : 0),
                    (byte)(data.ClutData[i]),
                    (byte)(data.ClutData[i + 1]),
                    (byte)(data.ClutData[i + 2])
                ));
            }

            palette = new BitmapPalette(colors);
        }

        var pixelFormat = picture.ImageColorType switch
        {
            1 => PixelFormats.Bgr555,  // Untested
            2 => PixelFormats.Rgb24,
            3 => PixelFormats.Bgra32,  // Untested
            4 => PixelFormats.Indexed4,
            5 => PixelFormats.Indexed8
        };

        var stride = (int)(picture.ImageWidth * (pixelFormat.BitsPerPixel / 8f));

        var source = BitmapSource.Create(picture.ImageWidth, 
            picture.ImageHeight, 
            96, 96,
            pixelFormat, 
            palette, 
            data.PixelData,
            stride
            );

        return source;
    }

    private static string DecodeShiftJIS(Span<byte> buffer)
    {
        var japaneseEncoding = Encoding.GetEncoding("Shift-JIS");
        var japaneseString = japaneseEncoding.GetString(buffer);

        return japaneseString.Replace("\\n", "\n");
    }

    private void UIElement_OnDrop(object sender, DragEventArgs e)
    {
        if (sender is TreeView treeView)
        {
            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] dataString = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (dataString.Length > 0)
                {
                    var extension = Path.GetExtension(dataString[0]).ToLower();

                    if (extension == ".afs" || extension == ".dar")
                    {
                        Task.Run(() => LoadArchive(dataString[0]));
                    }
                }
            }
        }
    }
}