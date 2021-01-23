namespace SharpAlliance.Core.SubSystems
{
    public class Interface
    {
        public InterfacePanelDefines gsCurInterfacePanel { get; internal set; }
    }

    public enum InterfacePanelDefines
    {
        SM_PANEL,
        TEAM_PANEL,
        NUM_UI_PANELS
    }
}
