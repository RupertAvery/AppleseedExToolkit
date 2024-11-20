using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AFSViewer;

public class MainWindowViewModel : BaseNotify
{
    private IEnumerable<TreeNode>? _nodes;
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
    private bool _hasProject;
    private bool _isProjectDirty;
    private PropertyPaneModel _properties;
    private int _page;
    private int _line;
    private int _character;
    private int _bytes;
    private int _characters;
    private string? _editItem1;
    private string? _editItem2;
    private string? _editItem3;
    private string? _editItem4;
    private bool _isTextDirty;
    private bool _isItemDirty;
    private string _editTextMode;

    public MainWindowViewModel()
    {
        //_binView = "Shift-JIS";
        _binView = "Item";
        _textVisibility = Visibility.Visible;
        _imageVisibility = Visibility.Hidden;
        Properties = new PropertyPaneModel();
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

    public IEnumerable<TreeNode>? Nodes
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
        set
        {
            SetField(ref _editText, value);
            if (!IsUpdatingView)
            {
                IsTextDirty = true;
            }
        }
    }

    public int Page
    {
        get => _page;
        set => SetField(ref _page, value);
    }

    public int Line
    {
        get => _line;
        set => SetField(ref _line, value);
    }

    public int Character
    {
        get => _character;
        set => SetField(ref _character, value);
    }

    public int Bytes
    {
        get => _bytes;
        set => SetField(ref _bytes, value);
    }

    public ICommand OpenArchiveCommand
    {
        get => _openArchiveCommand;
        set => SetField(ref _openArchiveCommand, value);
    }

    public ICommand NewProjectCommand { get; set; }

    public ICommand LoadProjectCommand { get; set; }

    public ICommand SaveProjectCommand { get; set; }

    public ICommand SaveProjectAsCommand { get; set; }

    public ICommand EditProjectCommand { get; set; }

    public ICommand BuildProjectCommand { get; set; }

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

    public byte[] ItemBuffer { get; set; }

    public bool IsUpdatingView { get; set; }

    public string? EditItem1
    {
        get => _editItem1;
        set
        {
            SetField(ref _editItem1, value);
            if (!IsUpdatingView)
            {
                IsItemDirty = true;
            }
        }
    }

    public string? EditItem2
    {
        get => _editItem2;
        set
        {
            SetField(ref _editItem2, value);
            if (!IsUpdatingView)
            {
                IsItemDirty = true;
            }

        }
    }

    public string? EditItem3
    {
        get => _editItem3;
        set
        {
            SetField(ref _editItem3, value);
            if (!IsUpdatingView)
            {
                IsItemDirty = true;
            }

        }
    }

    public string? EditItem4
    {
        get => _editItem4;
        set
        {
            SetField(ref _editItem4, value);
            if (!IsUpdatingView)
            {
                IsItemDirty = true;
            }

        }
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

    public bool HasProject
    {
        get => _hasProject;
        set => SetField(ref _hasProject, value);
    }

    public bool IsProjectDirty
    {
        get => _isProjectDirty;
        set => SetField(ref _isProjectDirty, value);
    }

    public PropertyPaneModel Properties
    {
        get => _properties;
        set => SetField(ref _properties, value);
    }

    public int Characters
    {
        get => _characters;
        set => SetField(ref _characters, value);
    }

    public bool IsTextDirty
    {
        get => _isTextDirty;
        set => SetField(ref _isTextDirty, value);
    }

    public bool IsItemDirty
    {
        get => _isItemDirty;
        set => SetField(ref _isItemDirty, value);
    }

    public string EditTextMode
    {
        get => _editTextMode;
        set => SetField(ref _editTextMode, value);
    }
}