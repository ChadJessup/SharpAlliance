using System;
using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class WordWrap
{
    public static bool ReduceStringLength(string pString, int uiWidthToFitIn, FontStyle uiFont)
    {
        string OneChar;
        string zTemp = string.Empty;
        string zStrDots = string.Empty;
        int uiDotWidth;
        int uiTempStringPixWidth = 0;
        int uiStringPixWidth;
        bool fDone = false;
        int uiSrcStringCntr = 0;
        int uiOneCharWidth = 0;

//        uiStringPixWidth = WFStringPixLength(pString, uiFont);

        OneChar = "\0";
        //zTemp[0] = '\0';

        //if the string is wider then the loaction
//        if (uiStringPixWidth <= uiWidthToFitIn)
//        {
//            //leave
//            return (true);
//        }

        //addd the '...' to the string
//        wcscpy(zStrDots, "...");

        //get the width of the '...'
//        uiDotWidth = FontSubSystem.StringPixLength(zStrDots, uiFont);

        //since the temp strig will contain the '...' add the '...' width to the temp string now
//        uiTempStringPixWidth = uiDotWidth;

        //loop through and add each character, 1 at a time
        while (!fDone)
        {
            //get the next char
//            OneChar = pString[uiSrcStringCntr];

            //get the width of the character
            uiOneCharWidth = FontSubSystem.StringPixLength(OneChar.ToString(), uiFont);

            //will the new char + the old string be too wide for the width
            if ((uiTempStringPixWidth + uiOneCharWidth) <= uiWidthToFitIn)
            {
                //add the new char to the string
//                zTemp = wcscat(OneChar);

                //add the new char width to the string width
                uiTempStringPixWidth += uiOneCharWidth;

                //increment to the next string
                uiSrcStringCntr++;
            }

            //yes the string would be too long if we add the new char, stop adding characters
            else
            {
                //we are done
                fDone = true;
            }
        }


        //combine the temp string and the '...' to form the finished string
        pString = wprintf("%s%s", zTemp, zStrDots);

        return true;
    }

    public static int IanWrappedStringHeight(int v1, int v2, int usTextWidth, int v3, FontStyle tEXT_POPUP_FONT, FontColor mERC_TEXT_COLOR, string pString, FontColor fONT_MCOLOR_BLACK, bool v4, TextJustifies lEFT_JUSTIFIED)
    {
        return 0;
    }
}
