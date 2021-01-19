namespace SharpAlliance.Core.SubSystems
{
    public class MessageSubSystem
    {
        private string gpDisplayList;
        private string[] gMapScreenMessageList = new string[256];
        private string? pStringS = null;
        private sbyte gubStartOfMapScreenMessageList = 0;
        private sbyte gubEndOfMapScreenMessageList = 0;
        private sbyte gubCurrentMapMessageString = 0;

        // first time adding any message to the message dialogue system
        private bool fFirstTimeInMessageSystem = true;
        private bool fDisableJustForIan = false;

        private bool fScrollMessagesHidden = false;
        private int uiStartOfPauseTime = 0;

        public void InitGlobalMessageList()
        {
            this.gubEndOfMapScreenMessageList = 0;
            this.gubStartOfMapScreenMessageList = 0;
            this.gubCurrentMapMessageString = 0;
        }
    }
}
