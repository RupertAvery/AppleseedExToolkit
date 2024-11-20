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
using System.Text.Json;
using System.Reflection;
using Ps2IsoTools.UDF;
using Ps2IsoTools.UDF.Files;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;

namespace AFSViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _model;

    private Stream? _source = null;
    private string? _archivePath = null;
    private string? _projectPath = null;

    private ApexProject? _apexProject;

    public MainWindow()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        _model = new MainWindowViewModel();
        InitializeComponent();
        DataContext = _model;

        _model.OpenArchiveCommand = new RelayCommand<object>((o) => OpenArchive());
        _model.SetDataViewCommand = new RelayCommand<string>(SetDataView);
        _model.ExtractNodeCommand = new RelayCommand<object>((o) => ExtractNode());

        _model.NewProjectCommand = new RelayCommand<object>((o) => CreateProject());
        _model.LoadProjectCommand = new RelayCommand<object>((o) => LoadProject());
        _model.SaveProjectCommand = new RelayCommand<object>((o) => SaveProject());
        _model.SaveProjectAsCommand = new RelayCommand<object>((o) => SaveProjectAs());
        _model.EditProjectCommand = new RelayCommand<object>((o) => EditProject());
        _model.BuildProjectCommand = new RelayCommand<object>((o) => BuildProject());

        Closing += OnClosing;

        _model.PropertyChanged += ModelOnPropertyChanged;


        var menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
        Action setAlignmentValue = () =>
        {
            if (SystemParameters.MenuDropAlignment && menuDropAlignmentField != null) menuDropAlignmentField.SetValue(null, false);
        };
        setAlignmentValue();
        SystemParameters.StaticPropertyChanged += (sender, e) => { setAlignmentValue(); };

        // LoadProject("D:\\roms\\ps2\\Appleseed EX\\Project\\project.json");
    }

    private async void CreateProject()
    {
        try
        {
            var window = new ProjectWindow();
            window.Owner = this;
            window.Title = "New Project";
            window.ShowDialog();

            if (window.DialogResult.GetValueOrDefault())
            {
                var archivePath = Path.Combine(window.Model.ProjectPath, "ARC.AFS");
                _projectPath = Path.Combine(window.Model.ProjectPath, "project.apex");

                _apexProject = new ApexProject
                {
                    ArchivePath = archivePath,
                    SourceISO = window.Model.ISOPath,
                    TargetISO = window.Model.PatchedISOPath,
                    Path = window.Model.ProjectPath
                };

                SaveProject();

                ShowMessage("Extracting ARC.AFS...");

                await Task.Delay(2000);

                ExtractAFS(_apexProject.SourceISO, _apexProject.ArchivePath);

                ShowMessage("Loading ARC.AFS...");

                await Task.Run(() => LoadArchive(archivePath));

            }

        }
        finally
        {
            HideMessage();

        }
       

    }

    private async void BuildProject()
    {
        try
        {
            var archive = new AFSArchive(_apexProject.ArchivePath);

            archive.Open();

            // These DARs seem to have malformed data
            // Setting them as ignored will copy them over
            // as-is from the source AFS file
            var ignoredFiles = new HashSet<string>()
            {
                "gw",
                "hw_73_12",
                "hw_73_13",
                "hw_73_1",
                "hw_73_2"
            };

            ShowMessage("Patching AFS...");

            var patchedAFSPath = Path.Combine(_apexProject.Path, "Patched_ARC.AFS");

            var builder = new AFSPatcher();

            using (var outstream = new FileStream(patchedAFSPath, FileMode.Create, FileAccess.Write))
            {
                builder.Build(archive, outstream, _apexProject.Path, ignoredFiles);
                outstream.Flush();
            }

            await Task.Delay(2000);

            ShowMessage("Writing ISO...");

            await Task.Run(() => BuildISO());
        }
        finally
        {
            HideMessage();
        }
    }


    private ProcessWindow? processWindow;

    private void ShowMessage(string message)
    {
        processWindow ??= new ProcessWindow();
        processWindow.Owner = this;
        processWindow.Show();
        processWindow.SetMessage(message);
    }

    private void HideMessage()
    {
        processWindow?.Hide();
    }

    void BuildISO()
    {
        using (var editor = new UdfEditor(_apexProject.SourceISO))
        {
            var fileId = editor.GetFileByName("ARC.AFS");

            editor.RemoveFile(fileId);

            var patchedAFSPath = Path.Combine(_apexProject.Path, "Patched_ARC.AFS");

            using (var fs = File.Open(patchedAFSPath, FileMode.Open))
            {
                editor.AddFile("ARC.AFS", fs);
                editor.Rebuild(_apexProject.TargetISO);
            }
        }
    }

    private void EditProject()
    {
        var window = new ProjectWindow();
        window.Owner = this;
        window.Model.ProjectPath = _apexProject.Path;
        window.Model.ISOPath = _apexProject.SourceISO;
        window.Model.PatchedISOPath = _apexProject.TargetISO;

        window.Title = "Edit Project";
        window.ShowDialog();

        if (window.DialogResult.GetValueOrDefault())
        {
            var archivePath = Path.Combine(window.Model.ProjectPath, "ARC.AFS");
            _projectPath = Path.Combine(window.Model.ProjectPath, "project.apex");

            _apexProject = new ApexProject
            {
                ArchivePath = _archivePath,
                SourceISO = window.Model.ISOPath,
                TargetISO = window.Model.PatchedISOPath,
                Path = window.Model.ProjectPath
            };

            SaveProject();

            if (!File.Exists(_apexProject.ArchivePath))
            {
                ExtractAFS(_apexProject.SourceISO, _apexProject.ArchivePath);
            }
        }

    }

    private void ExtractAFS(string srcIso, string outputPath)
    {

        using var reader = new UdfReader(srcIso);

        FileIdentifier? fileId = reader.GetFileByName("ARC.AFS");

        if (fileId is not null)
        {
            using var arcStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

            using var fstream = reader.GetFileStream(fileId);

            fstream.CopyTo(arcStream);

            arcStream.Flush();
        }
    }

    private void LoadProject()
    {
        var openFileDialog = new OpenFileDialog();

        openFileDialog.Filter = "Project files|*.apex";

        var result = openFileDialog.ShowDialog();

        if (result.HasValue && result.Value)
        {
            LoadProjectFile(openFileDialog.FileName);
        }
    }

    private void LoadProjectFile(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

        _projectPath = path;

        _apexProject = JsonSerializer.Deserialize<ApexProject>(stream);

        LoadArchive(_apexProject.ArchivePath);

        ScanProjectPath();

        _model.HasProject = true;
    }

    private void ScanProjectPath()
    {
        var files = Directory.EnumerateFiles(_apexProject.Path, "*.*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var temp = _apexProject.Path;

            if (!temp.EndsWith("\\"))
            {
                temp = temp + "\\";
            }

            var path = file.Replace(temp, "");

            if (!_apexProject.Files.TryGetValue(path, out var apexFile))
            {
                apexFile = new ApexFile()
                {
                    Path = path
                };

                _apexProject.Files.Add(path, apexFile);
            }

            UpdateNodeArtifacts(_model.Nodes, apexFile);
        }
    }

    private void UpdateNodeArtifacts(IEnumerable<TreeNode> nodes)
    {
        if (_apexProject == null) return;
        foreach (var apexFile in _apexProject.Files.Values)
        {
            UpdateNodeArtifacts(nodes, apexFile);
        }
    }

    private void UpdateNodeArtifacts(IEnumerable<TreeNode> nodes, ApexFile apexFile)
    {
        foreach (var node in nodes)
        {
            if (node.Path != null)
            {
                //if (node.Path.Contains("_30") && apexFile.Path.Contains("_30"))
                //{
                //    var x = 1;
                //}

                string nodePath = node.Path;

                if (node.Type == "dar")
                {
                    nodePath = node.Path + "\\";
                }
                else
                {
                    nodePath = node.Path + $".{node.Type}";
                }

                var hasArtifiact = apexFile.Path.StartsWith(nodePath);

                if (hasArtifiact)
                {
                    var temp = node;
                    do
                    {
                        temp.HasArtifact = hasArtifiact;
                        temp = temp.Parent;
                    } while (temp != null);
                }


                if (node.Children != null)
                {
                    UpdateNodeArtifacts(node.Children, apexFile);
                }
            }
        }
    }

    private void SaveProject()
    {
        using var stream = new FileStream(_projectPath, FileMode.Create, FileAccess.Write);

        JsonSerializer.Serialize(stream, _apexProject, new JsonSerializerOptions() { WriteIndented = true });

        stream.Flush();
    }

    private void SaveProjectAs()
    {
        var saveFileDialog = new SaveFileDialog();

        saveFileDialog.Filter = "Project files|*.apex";

        var result = saveFileDialog.ShowDialog();

        if (result.HasValue && result.Value)
        {
            _projectPath = saveFileDialog.FileName;

            SaveProject();
        }
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

        var nodePath = node.Path + ".bin";

        if (!_apexProject.Files.TryGetValue(nodePath, out var apexFile))
        {
            apexFile = new ApexFile()
            {
                Path = nodePath,
                Type = _model.BinView,
            };

            _apexProject.Files.Add(nodePath, apexFile);
        }
        else
        {
            apexFile.Path = nodePath;
        }


        var path = Path.Combine(_apexProject.Path, apexFile.Path);

        var dir = Path.GetDirectoryName(path);

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (_model.EditText.Trim([' ', '\0']).Length > 0)
        {
            var data = EncodeShiftJIS(_model.EditText);

            using var file = new FileStream(path, FileMode.Create, FileAccess.Write);

            file.Write(data);

            if (file.Length % 0x10 > 0)
            {
                var padding = new byte[0x10 - file.Length % 0x10];
                file.Write(padding);
            }

            file.Flush();

            UpdateNodeArtifacts([node], apexFile);
        }
        else
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            node.HasArtifact = false;

            _apexProject.Files.Remove(nodePath);
        }

    }

    private void OpenArchive()
    {
        var dialog = new OpenFileDialog();
        dialog.Title = "Select an archive";
        dialog.Filter = "AFS files|*.afs|DAR files|*.dar|All files|*.*";

        var result = dialog.ShowDialog(this);

        if (result is true)
        {
            LoadArchive(dialog.FileName);
        }
    }

    private void LoadArchive(string filename)
    {
        _archivePath = filename;

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
                    Path = $"{parentName}\\{childname}" + (entry.Type == "dar" ? "" : $".{entry.Type}"),
                    Size = entry.Size,
                    Type = entry.Type,
                    Attribute = $"{entry.Attribute:X2}",
                };

                if (child.Type == "dar")
                {
                    child.Children = new List<TreeNode>() { new TreeNode() };
                }

                nodes.Add(child);

                index++;
            }

        }

        var root = new TreeNode()
        {
            Name = filename,
            Type = "root",
            Path = "/",
            Children = nodes
        };

        _model.Nodes = new List<TreeNode>()
        {
            root
        };


        Dispatcher.Invoke(() =>
        {
            var tvi = FindTviFromObjectRecursive(ArcTreeView, root);
            tvi.IsExpanded = true;

        });
    }

    private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
    {
        if (((TreeViewItem)e.OriginalSource).DataContext is TreeNode { Type: "dar" } node)
        {
            try
            {
                if (!node.IsLoaded)
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
                            Type = entry.Type,
                            Parent = node,
                            Attribute = $"{entry.Attribute:X2}",
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


                UpdateNodeArtifacts(node.Children);
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
        _model.Properties.Attribute = "";

        _model.ImageVisibility = Visibility.Hidden;
        _model.TextVisibility = Visibility.Visible;

        if (node == null) return;

        _model.Properties.Attribute = node.Attribute;

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

                            if (_apexProject != null)
                            {
                                if (_apexProject.Files.TryGetValue(node.Path, out var apexFile))
                                {
                                    var path = Path.Combine(_apexProject.Path, apexFile.Path + ".bin");

                                    if (File.Exists(path))
                                    {
                                        _model.EditText = DecodeShiftJIS(File.ReadAllBytes(path));
                                    }

                                }
                                else
                                {
                                    var path = Path.Combine(_apexProject.Path, node.Path + ".bin");

                                    if (File.Exists(path))
                                    {
                                        _model.EditText = DecodeShiftJIS(File.ReadAllBytes(path));
                                    }
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
        if (picture.ImageColorType is 4 or 5)
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


    private static Span<byte> EncodeShiftJIS(string text)
    {
        text = text.Replace("\r\n", "\\n").Replace("\n", "\\n");

        var japaneseEncoding = Encoding.GetEncoding("Shift-JIS");
        var japaneseBytes = japaneseEncoding.GetBytes(text);

        return japaneseBytes;
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
                        LoadArchive(dataString[0]);
                    }
                }
            }
        }
    }

    public static TreeViewItem? FindTviFromObjectRecursive(ItemsControl ic, object o)
    {
        //Search for the object model in first level children (recursively)
        if (ic.ItemContainerGenerator.ContainerFromItem(o) is TreeViewItem tvi) return tvi;
        //Loop through user object models
        foreach (object i in ic.Items)
        {
            //Get the TreeViewItem associated with the iterated object model
            if (ic.ItemContainerGenerator.ContainerFromItem(i) is TreeViewItem tvi2)
            {
                var tvi3 = FindTviFromObjectRecursive(tvi2, o);
                if (tvi3 != null) return tvi3;
            }
        }
        return null;
    }

    private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
    {
        var converter = new KeyConverter();
        var str = converter.ConvertToString(e.Key);

        if (_model.Nodes != null)
        {
            TreeNode? startNode = _model.SelectedNode;
            if (startNode == null)
            {
                startNode = _model.Nodes.First();
            }

            var foundStart = false;
            var foundNode = FindNodeStartsWith(_model.Nodes, startNode, str, 0, ref foundStart);

            if (foundNode != null)
            {
                var tvi = FindTviFromObjectRecursive(ArcTreeView, foundNode);

                if (tvi != null)
                {
                    tvi.IsSelected = true;
                }

            }

            _model.SelectedNode = foundNode;
        }
    }

    private TreeNode? FindNodeStartsWith(IEnumerable<TreeNode> nodes, TreeNode currentNode, string text, int depth, ref bool foundStart)
    {
        var index = 0;
        foreach (var node in nodes)
        {
            if (node == currentNode)
            {
                foundStart = true;
            }

            if (foundStart && node != currentNode && node.Name != null && node.Name.ToLower().StartsWith(text.ToLower()))
            {
                return node;
            }

            var tvi = FindTviFromObjectRecursive(ArcTreeView, node);

            if (node.Children != null && tvi.IsExpanded)
            {
                var child = FindNodeStartsWith(node.Children, currentNode, text, depth + 1, ref foundStart);

                if (child != null)
                {
                    return child;
                }
            }

            index++;
        }

        return null;
    }

    private void TranslatedText_OnKeyUp(object sender, KeyEventArgs e)
    {
        CalculateCharData();
    }

    private void CalculateCharData()
    {
        var ci = TranslatedText.CaretIndex;

        var s = TranslatedText.Text.Substring(0, ci);

        var lines = s.Split(["\n", "\r\n", "\r"], StringSplitOptions.None);

        var lineCount = lines.Length;

        _model.Page = (lineCount / 4) + 1;
        _model.Line = ((lineCount - 1) % 4) + 1;
        _model.Character = ci - s.LastIndexOf("\n") - 1;
        _model.Characters = TranslatedText.Text.Length;
        _model.Bytes = (int)Util.Pad((uint)EncodeShiftJIS(TranslatedText.Text).Length, 0x10);
    }

    private void TranslatedText_OnGotFocus(object sender, RoutedEventArgs e)
    {
        CalculateCharData();
    }
}