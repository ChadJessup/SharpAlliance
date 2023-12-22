namespace SharpAlliance.Core.Managers;

public enum VideoManagerState
{
    Off = 0x00,
    On = 0x01,
    ShuttingDown = 0x02,
    Suspended = 0x04,
}
