namespace SharpAlliance.Core.Managers;

public enum BufferState
{
    READY = 0x00,
    BUSY = 0x01,
    DIRTY = 0x02,
    DISABLED = 0x03,
}
