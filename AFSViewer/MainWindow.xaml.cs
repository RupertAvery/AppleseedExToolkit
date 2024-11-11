using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using FileFormats;
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;

namespace AFSViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _model;

    private AFSArchive? _afsArchive;

    public MainWindow()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


        _model = new MainWindowViewModel();
        InitializeComponent();
        DataContext = _model;

        _model.OpenArchiveCommand = new AsyncCommand<object>((o) => OpenArchive());
        Closing += OnClosing;
        //var filename = @"D:\roms\ps2\ARC.AFS";
        //LoadArchive(filename);

        _model.PropertyChanged += ModelOnPropertyChanged;
    }

    private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.BinView))
        {
            RenderItem(_model.SelectedNode);
        }
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        _afsArchive?.Dispose();
    }

    private async Task OpenArchive()
    {
        var dialog = new OpenFileDialog();
        dialog.Filter = "AFS Archives|*.afs";
        var result = dialog.ShowDialog(this);

        if (result is true)
        {
            await Task.Run(() => LoadArchive(dialog.FileName));
        }
    }

    private void LoadArchive(string filename)
    {
        var nodes = new List<TreeViewItem>();

        var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

        var fileInfo = new FileInfo(filename);

        _afsArchive?.Dispose();

        _afsArchive = new AFSArchive(stream, fileInfo.Length);

        _afsArchive.Open();

        foreach (var entry in _afsArchive.Entries)
        {
            Dispatcher.Invoke(() =>
            {
                var tvi = new TreeViewItem()
                {
                    DataContext = entry,
                    Header = entry.Name,
                };

                if (entry.Name.EndsWith(".dar"))
                {
                    tvi.ItemsSource = new List<TreeViewItem>() { new TreeViewItem() { DataContext = null } };
                }
                nodes.Add(tvi);
            });
        }

        Dispatcher.Invoke(() => { _model.Nodes = nodes; });
    }

    private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
    {
        TreeViewItem tvi = e.OriginalSource as TreeViewItem;

        if (tvi.DataContext is AFSEntry afsEntry)
        {
            if (afsEntry.Name.EndsWith(".dar"))
            {
                var reader = new DARReader(_afsArchive.Stream, afsEntry.Offset, afsEntry.Size);

                var nodes = new List<TreeViewItem>();

                var name = System.IO.Path.GetFileNameWithoutExtension((string)tvi.Header);

                var index = 0;

                foreach (var entry in reader.Entries)
                {
                    var tvic = new TreeViewItem()
                    {
                        DataContext = entry,
                        Header = $"{name}_{index}.{entry.Type}",
                    };

                    nodes.Add(tvic);

                    if (entry.Type == "dar")
                    {
                        tvic.ItemsSource = new List<TreeViewItem>() { new TreeViewItem() { DataContext = null } };
                    }

                    index++;
                }


                tvi.ItemsSource = nodes;
            }
        }
        else if (tvi.DataContext is DAREntry darEntry)
        {
            if (darEntry.Type == "dar")
            {
                try
                {
                    var reader = new DARReader(_afsArchive.Stream, darEntry.StreamOffset + darEntry.Offset, darEntry.Size);

                    var nodes = new List<TreeViewItem>();

                    var name = System.IO.Path.GetFileNameWithoutExtension((string)tvi.Header);

                    var index = 0;

                    foreach (var entry in reader.Entries)
                    {
                        var tvic = new TreeViewItem()
                        {
                            DataContext = entry,
                            Header = $"{name}_{index}.{entry.Type}",
                        };

                        nodes.Add(tvic);

                        if (entry.Type == "dar")
                        {
                            tvic.ItemsSource = new List<TreeViewItem>() { new TreeViewItem() { DataContext = null } };
                        }

                        index++;
                    }

                    tvi.ItemsSource = nodes;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(this, "Failed to decode", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }



            }
        }
    }

    private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is TreeViewItem tvi)
        {
            if (!tvi.IsSelected)
            {
                return;
            }

            //if (tvi.DataContext is AFSEntry afsEntry)
            //{
            //    if (afsEntry.Name.EndsWith(".dar"))
            //    {
            //        var filename = @"D:\roms\ps2\ARC.AFS";

            //        using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            //        stream.Seek(afsEntry.Offset, SeekOrigin.Begin);

            //        var reader = new DARReader(stream, afsEntry.Size);

            //        var nodes = new List<TreeViewItem>();

            //        var name = System.IO.Path.GetFileNameWithoutExtension((string)tvi.Header);

            //        var index = 0;

            //        foreach (var entry in reader.Entries)
            //        {
            //            nodes.Add(new TreeViewItem()
            //            {
            //                DataContext = entry,
            //                Header = $"{name}_{index}",
            //            });
            //            index++;
            //        }


            //        tvi.ItemsSource = nodes;
            //    }
            //}

            if (tvi.DataContext is DAREntry darEntry)
            {
                //if (darEntry.Name.EndsWith(".dar"))
                //{
                //    var filename = @"D:\roms\ps2\ARC.AFS";

                //    using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                //    var reader = new DARReader(stream, darEntry.Size);

                //    var nodes = new List<TreeViewItem>();

                //    foreach (var entry in reader.Entries)
                //    {
                //        nodes.Add(new TreeViewItem()
                //        {
                //            DataContext = entry,
                //            Header = System.IO.Path.GetFileNameWithoutExtension((string)tvi.Header),
                //        });
                //    }


                //    tvi.ItemsSource = nodes;
                //}
            }
        }
    }

    private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var tvi = (TreeViewItem)e.NewValue;
        _model.SelectedNode = tvi;
        RenderItem(tvi);
    }



    private void RenderItem(TreeViewItem tvi)
    {
        _model.Item1 = "";
        _model.Item2 = "";
        _model.Item3 = "";
        _model.Item4 = "";
        _model.Data = "";
        _model.Image = null;
        _model.ImageVisibility = Visibility.Hidden;
        _model.TextVisibility = Visibility.Visible;

        switch (tvi?.DataContext)
        {
            case AFSEntry afsEntry:
            {
                if (afsEntry.Name.EndsWith("tm2"))
                {
                    _afsArchive.Stream.Seek(afsEntry.Offset, SeekOrigin.Begin);

                    try
                    {
                        _model.Image = ReadTIM(_afsArchive.Stream);
                        _model.ImageVisibility = Visibility.Visible;
                        _model.TextVisibility = Visibility.Hidden;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(this, "Failed to decode", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                }

                break;
            }
            case DAREntry { Type: "tm2" } darEntry:
                _afsArchive.Stream.Seek(darEntry.StreamOffset + darEntry.Offset, SeekOrigin.Begin);

                try
                {
                    _model.Image = ReadTIM(_afsArchive.Stream);
                    _model.ImageVisibility = Visibility.Visible;
                    _model.TextVisibility = Visibility.Hidden;
                }
                catch (Exception e)
                {
                    MessageBox.Show(this, "Failed to decode", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                break;
            case DAREntry darEntry:
            {
                if (darEntry.Type == "bin")
                {
                    _afsArchive.Stream.Seek(darEntry.StreamOffset + darEntry.Offset, SeekOrigin.Begin);

                    var buffer = new byte[darEntry.Size];
                    _afsArchive.Stream.Read(buffer);

                    switch (_model.BinView)
                    {
                        case "Shift-JIS":
                            _model.Data = Decode(buffer);
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
                                _model.Item1 = Decode(buffer[0xA0..0xBF]);
                                _model.Item2 = Decode(buffer[0xC0..0xEF]);
                                _model.Item3 = Decode(buffer[0xF0..0x10F]);
                                _model.Item4 = Decode(buffer[0x110..0x11F]);
                            }

                            break;
                        }
                    }
                }

                break;
            }
        }
    }

    private static BitmapSource ReadTIM(Stream stream)
    {
        var reader = new TIM2Reader(stream);

        var picture = reader.GetPicture(0);

        var colors = new List<Color>(picture.ClutData.Length);

        var colorStorageMode = picture.GsTexRegister1 >> 55 & 1;


        if (colorStorageMode == 0)
        {
            var i = 0x20;
            while (i < picture.ClutData.Length)
            {
                var temp = new byte[0x20];
                var e = i + 0x20;
                Array.Copy(picture.ClutData[i..e], temp, 0x20);
                if (i + 0x20 < picture.ClutData.Length)
                {
                    for (var j = 0; j < 0x20; j++)
                    {
                        picture.ClutData[i + j] = picture.ClutData[i + j + 0x20];
                    }
                    i += 0x20;
                    for (var j = 0; j < 0x20; j++)
                    {
                        picture.ClutData[i + j] = temp[j];
                    }
                    i += 0x60;
                }
                else
                {
                    break;
                }
            }

            // Swizzle
        }

        for (var i = 0; i < picture.ClutData.Length; i += 4)
        {
            colors.Add(Color.FromArgb(
                (byte)(picture.ClutData[i + 3] == 128 ? 255 : 0),
                (byte)(picture.ClutData[i]),
                (byte)(picture.ClutData[i + 1]),
                (byte)(picture.ClutData[i + 2])
            ));
        }

        var palette = new BitmapPalette(colors);

        var pixelFormat = picture.ImageColorType switch
        {
            4 => PixelFormats.Indexed4,
            5 => PixelFormats.Indexed8
        };

        if (picture.ImageColorType == 4)
        {
            for (var i = 0; i < picture.PictureData.Length; i++)
            {
                var temp = picture.PictureData[i];
                temp = (byte)(((temp & 0x0F) << 4) | ((temp & 0xF0) >> 4));
                picture.PictureData[i] = temp;
            }
        }

        var source = BitmapSource.Create(picture.ImageWidth, picture.ImageHeight, 96, 96,
            pixelFormat, palette, picture.PictureData, (int)(picture.ImageWidth * (pixelFormat.BitsPerPixel / 8f)));

        //using (var fileStream = new FileStream(@"D:\roms\ps2\FULL_AFS_FILE_DUMP\vmbg2.bmp", FileMode.Create))
        //{
        //    BitmapEncoder encoder = new BmpBitmapEncoder();
        //    encoder.Frames.Add(BitmapFrame.Create(source));
        //    encoder.Save(fileStream);
        //}

        return source;
    }

    private static string Decode(byte[] buffer)
    {
        var japaneseEncoding = Encoding.GetEncoding("Shift-JIS");
        var japaneseString = japaneseEncoding.GetString(buffer);

        return japaneseString.Replace("\\n", "\n");
    }

}


public class VisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (string)value == (string)parameter ? Visibility.Visible : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}