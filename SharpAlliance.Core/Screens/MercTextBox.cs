using System;
using System.Threading.Tasks;

namespace SharpAlliance.Core.Screens
{
    public class MercTextBox
    {

        public ValueTask<bool> InitMercPopupBox()
        {
            return ValueTask.FromResult(true);
        }

        public void RemoveMercPopupBoxFromIndex(int iBoxId)
        {
        }

        public int PrepareMercPopupBox(int iId, MercTextBoxBackground ubMercBoxBackground, MercTextBoxBorder ubMercBoxBorder, string zString, int mSGBOX_DEFAULT_WIDTH, int v1, int v2, int v3, out int usTextBoxWidth, out int usTextBoxHeight)
        {
            usTextBoxHeight = 11;
            usTextBoxWidth = 56;

            return 0;
        }
    }


    // background enumeration
    public enum MercTextBoxBackground
    {
        BASIC_MERC_POPUP_BACKGROUND = 0,
        WHITE_MERC_POPUP_BACKGROUND,
        GREY_MERC_POPUP_BACKGROUND,
        DIALOG_MERC_POPUP_BACKGROUND,
        LAPTOP_POPUP_BACKGROUND,
        IMP_POPUP_BACKGROUND,
    };

    // border enumeration
    public enum MercTextBoxBorder
    {
        BASIC_MERC_POPUP_BORDER = 0,
        RED_MERC_POPUP_BORDER,
        BLUE_MERC_POPUP_BORDER,
        DIALOG_MERC_POPUP_BORDER,
        LAPTOP_POP_BORDER
    };
}
