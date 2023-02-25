namespace SharpAlliance.Core.SubSystems;

public class MessageSubSystem
{
    private string? pStringS = null;

    // first time adding any message to the message dialogue system
    private bool fFirstTimeInMessageSystem = true;
    private bool fDisableJustForIan = false;

    private bool fScrollMessagesHidden = false;
    private int uiStartOfPauseTime = 0;

    public void InitGlobalMessageList()
    {
        Globals.gubEndOfMapScreenMessageList = 0;
        Globals.gubStartOfMapScreenMessageList = 0;
        Globals.gubCurrentMapMessageString = 0;
    }
}
