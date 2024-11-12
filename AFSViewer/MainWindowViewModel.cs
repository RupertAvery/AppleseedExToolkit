using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AFSViewer;

public class MainWindowViewModel : BaseNotify
{
    private IEnumerable<TreeNode> _nodes;
    private string _data;
    private string _binView;
    private TreeNode? _selectedNode;
    private string _item1;
    private ICommand _openArchiveCommand;
    private string _item2;
    private string _item3;
    private string _item4;
    private BitmapSource _image;
    private Visibility _imageVisibility;
    private Visibility _textVisibility;
    private ICommand _setDataView;
    private string _editText;
    private ICommand _extractNodeCommand;
    private bool _isLoaded;
    private bool _isNodeSelected;

    public MainWindowViewModel()
    {
        _binView = "Shift-JIS";
        _textVisibility = Visibility.Visible;
        _imageVisibility = Visibility.Hidden;
    }

    public bool IsLoaded
    {
        get => _isLoaded;
        set => SetField(ref _isLoaded, value);
    }

    public bool IsNodeSelected
    {
        get => _isNodeSelected;
        set => SetField(ref _isNodeSelected, value);
    }

    public IEnumerable<string> BinViewItems => ["Shift-JIS", "Hexadecimal", "Item"];

    public TreeNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            SetField(ref _selectedNode, value);
            IsNodeSelected = _selectedNode != null;
        }
    }

    public IEnumerable<TreeNode> Nodes
    {
        get => _nodes;
        set => SetField(ref _nodes, value);
    }

    public string Data
    {
        get => _data;
        set => SetField(ref _data, value);
    }

    public string EditText
    {
        get => _editText;
        set => SetField(ref _editText, value);
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

    public ICommand SetDataViewCommand
    {
        get => _setDataView;
        set => SetField(ref _setDataView, value);
    }

    public ICommand ExtractNodeCommand
    {
        get => _extractNodeCommand;
        set => SetField(ref _extractNodeCommand, value);
    }
}
