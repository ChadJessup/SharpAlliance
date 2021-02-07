namespace SharpAlliance.Core.SubSystems
{
    public class MessageBoxSubSystem
    {
        // message box return codes
        public const int MSG_BOX_RETURN_OK = 1;// ENTER or on OK button
        public const int MSG_BOX_RETURN_YES = 2;// ENTER or YES button
        public const int MSG_BOX_RETURN_NO = 3;// ESC, Right Click or NO button
        public const int MSG_BOX_RETURN_CONTRACT = 4;// contract button
        public const int MSG_BOX_RETURN_LIE = 5;					// LIE BUTTON

        public bool gfInMsgBox { get; set; } = false;
    }
}
