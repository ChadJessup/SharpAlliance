namespace SharpAlliance.Core;

public struct DragDropEvent
{
    public string File { get; }

    public DragDropEvent(string file)
    {
        File = file;
    }
}
