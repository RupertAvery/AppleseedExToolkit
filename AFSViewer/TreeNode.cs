using AFSTools;

namespace AFSViewer;

public class TreeNode : BaseNotify
{
    private IEnumerable<TreeNode>? _children;
    private Status _status;
    private bool _hasArtifact;

    public TreeNode()
    {
    }

    public long Offset { get; set; }
    public long Size { get; set; }
    public string? Type { get; set; }
    public string? Name { get; set; }
    public bool IsLoaded { get; set; }
    public string? Path { get; set; }

    public bool HasArtifact
    {
        get => _hasArtifact;
        set => SetField(ref _hasArtifact, value);
    }

    public Status Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public IEnumerable<TreeNode>? Children
    {
        get => _children;
        set => SetField(ref _children, value);
    }

    public TreeNode? Parent { get; set; }

    public string Attribute { get; set; }
}
