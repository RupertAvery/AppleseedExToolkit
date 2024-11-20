namespace AFSViewer;

public class PropertyPaneModel : BaseNotify
{
    private string _attribute;

    public string Attribute
    {
        get => _attribute;
        set => SetField(ref _attribute, value);
    }
}