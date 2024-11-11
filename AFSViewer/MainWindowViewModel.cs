using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AFSViewer;

public class MainWindowViewModel : BaseNotify
{
    private IEnumerable<TreeViewItem> _nodes;
    private string _data;
    private string _binView;
    private TreeViewItem _selectedNode;
    private string _item1;
    private ICommand _openArchiveCommand;
    private string _item2;
    private string _item3;
    private string _item4;
    private BitmapSource _image;
    private Visibility _imageVisibility;
    private Visibility _textVisibility;

    public MainWindowViewModel()
    {
        _binView = "Item";
        _textVisibility = Visibility.Visible;
        _imageVisibility = Visibility.Hidden;
    }

    public IEnumerable<string> BinViewItems => ["Shift-JIS", "Hexadecimal", "Item"];

    public TreeViewItem SelectedNode
    {
        get => _selectedNode;
        set => SetField(ref _selectedNode, value);
    }

    public IEnumerable<TreeViewItem> Nodes
    {
        get => _nodes;
        set => SetField(ref _nodes, value);
    }

    public string Data
    {
        get => _data;
        set => SetField(ref _data, value);
    }

    public ICommand OpenArchiveCommand
    {
        get => _openArchiveCommand;
        set => SetField(ref _openArchiveCommand, value);
    }

    public string BinView
    {
        get => _binView;
        set => SetField(ref _binView, value);
    }

    public string Item1
    {
        get => _item1;
        set => SetField(ref _item1, value);
    }

    public string Item2
    {
        get => _item2;
        set => SetField(ref _item2, value);
    }

    public string Item3
    {
        get => _item3;
        set => SetField(ref _item3, value);
    }

    public string Item4
    {
        get => _item4;
        set => SetField(ref _item4, value);
    }

    public BitmapSource Image
    {
        get => _image;
        set => SetField(ref _image, value);
    }

    public Visibility ImageVisibility
    {
        get => _imageVisibility;
        set => SetField(ref _imageVisibility, value);
    }

    public Visibility TextVisibility
    {
        get => _textVisibility;
        set => SetField(ref _textVisibility, value);
    }
}
