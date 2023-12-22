using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers.Library;

public record LibraryInitHeader(string LibraryName, bool OnCdRom, bool InitOnStart);
