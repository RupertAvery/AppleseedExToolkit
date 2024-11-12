using AFSTools;

namespace AFSViewer;

public class TreeNode : BaseNotify
{
    private IEnumerable<TreeNode> _children;
    private Status _status;
    public long Offset { get; set; }
    public long Size { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public bool IsLoaded { get; set; }
    public string Path { get; set; }
    public Status Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public IEnumerable<TreeNode> Children
    {
        get => _children;
        set => SetField(ref _children, value);
    }
}
