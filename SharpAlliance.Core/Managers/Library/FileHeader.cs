namespace SharpAlliance.Core.Managers.Library
{
    public readonly struct FileHeader
    {
        public string pFileName { get; init; }
        public int uiFileLength { get; init; }
        public int uiFileOffset { get; init; }

        public override string ToString()
            => this.pFileName;
    }
}
