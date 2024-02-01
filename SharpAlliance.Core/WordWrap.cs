using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;

namespace SharpAlliance.Core;

public class WordWrap
{
    private const int IAN_WRAP_NO_SHADOW = 32;

    // Defines for coded text For use with IanDisplayWrappedString()
    private const char TEXT_SPACE = (char)32;
    private const char TEXT_CODE_NEWLINE = (char)177;
    private const char TEXT_CODE_BOLD = (char)178;
    private const char TEXT_CODE_CENTER = (char)179;
    private const char TEXT_CODE_NEWCOLOR = (char)180;
    private const char TEXT_CODE_DEFCOLOR = (char)181;

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

    public static int IanWrappedStringHeight(int usPosX, int usPosY, int usWidth, int ubGap, FontStyle uiFont, FontColor ubColor, string pString, FontColor ubBackGroundColor, bool fDirty, TextJustifies uiFlags)
    {
        int usHeight;
        int usSourceCounter = 0, usDestCounter = 0, usWordLengthPixels, usLineLengthPixels = 0, usPhraseLengthPixels = 0;
        int usLinesUsed = 1, usLocalWidth = usWidth;
        FontStyle uiLocalFont = uiFont;
        TextJustifies usJustification = TextJustifies.LEFT_JUSTIFIED;
        int usLocalPosX = usPosX;
        FontColor ubLocalColor = ubColor;
        bool fBoldOn = false;
        int iTotalHeight = 0;
        char[] zLineString = new char[640];
        char[] zWordString = new char[640];

        usHeight = FontSubSystem.WFGetFontHeight(uiFont);

        return 104;
        // simply a cut and paste operation on Ian Display Wrapped, but will not write string to screen
        // since this all we want to do, everything IanWrapped will do but without displaying string

        do
        {
            // each character goes towards building a new word
            if (pString[usSourceCounter] != (char)TEXT_CODE.TEXT_SPACE && pString[usSourceCounter] != 0)
            {
                zWordString[usDestCounter++] = pString[usSourceCounter];
            }
            else
            {
                // we hit a space (or end of record), so this is the END of a word!

                // is this a special CODE?
                if (zWordString[0] >= (char)TEXT_CODE.NEWLINE && zWordString[0] <= (char)TEXT_CODE.DEFCOLOR)
                {
                    switch ((TEXT_CODE)zWordString[0])
                    {
                        case TEXT_CODE.CENTER:

                            if (usJustification != TextJustifies.CENTER_JUSTIFIED)
                            {
                                usJustification = TextJustifies.CENTER_JUSTIFIED;

                                // erase this word string we've been building - it was just a code
                                Array.Fill(zWordString, '\0');

                                // erase the line string, we're starting from scratch
                                Array.Fill(zLineString, '\0');

                                // reset the line length - we're starting from scratch
                                usLineLengthPixels = '\0';

                                // reset dest char counter
                                usDestCounter = '\0';
                            }
                            else    // turn OFF centering...
                            {

                                // increment Y position for next time
                                usPosY += (FontSubSystem.WFGetFontHeight(uiLocalFont)) + ubGap;

                                // we just used a line, so note that
                                usLinesUsed++;

                                // reset x position
                                usLocalPosX = usPosX;

                                // erase this word string we've been building - it was just a code
                                Array.Fill(zWordString, '\0');

                                // erase the line string, we're starting from scratch
                                Array.Fill(zLineString, '\0');

                                // reset the line length
                                usLineLengthPixels = '\0';

                                // reset dest char counter
                                usDestCounter = '\0';

                                // turn off centering...
                                usJustification = TextJustifies.LEFT_JUSTIFIED;
                            }

                            break;



                        case TEXT_CODE.NEWLINE:

                            // NEWLINE character!


                            // increment Y position for next time
                            usPosY += (FontSubSystem.WFGetFontHeight(uiLocalFont)) + ubGap;

                            // we just used a line, so note that
                            usLinesUsed++;

                            // reset x position
                            usLocalPosX = usPosX;

                            // erase this word string we've been building - it was just a code
                            Array.Fill(zWordString, '\0');

                            // erase the line string, we're starting from scratch
                            Array.Fill(zLineString, '\0');

                            // reset the line length
                            usLineLengthPixels = '\0';

                            // reset width 
                            usLocalWidth = usWidth;

                            // reset dest char counter
                            usDestCounter = '\0';

                            break;


                        case TEXT_CODE.BOLD:

                            if (!fBoldOn)
                            {

                                // calc length of what we just wrote
                                usPhraseLengthPixels = FontSubSystem.WFStringPixLength(new string(zLineString), uiLocalFont);
                                // calculate new x position for next time
                                usLocalPosX += usPhraseLengthPixels;

                                // shorten width for next time
                                usLocalWidth -= usLineLengthPixels;

                                // erase this word string we've been building - it was just a code
                                Array.Fill(zWordString, '\0');

                                // erase the line string, we're starting from scratch
                                Array.Fill(zLineString, '\0');

                                // turn bold ON
                                uiLocalFont = FontStyle.FONT10ARIALBOLD;
                                FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);
                                fBoldOn = true;

                                // reset dest char counter
                                usDestCounter = '\0';
                            }
                            else
                            {

                                // calc length of what we just wrote
                                usPhraseLengthPixels = FontSubSystem.WFStringPixLength(new string(zLineString), uiLocalFont);

                                // calculate new x position for next time
                                usLocalPosX += usPhraseLengthPixels;

                                // shorten width for next time
                                usLocalWidth -= usLineLengthPixels;

                                // erase this word string we've been building - it was just a code
                                Array.Fill(zWordString, '\0');

                                // erase the line string, we're starting from scratch
                                Array.Fill(zLineString, '\0');

                                // turn bold OFF
                                uiLocalFont = uiFont;
                                fBoldOn = false;

                                // reset dest char counter
                                usDestCounter = '\0';
                            }

                            break;



                        case TEXT_CODE.NEWCOLOR:

                            // the new color value is the next character in the word
                            if (zWordString[1] != (char)TEXT_CODE.TEXT_SPACE && zWordString[1] < 256)
                            {
                                ubLocalColor = (FontColor)zWordString[1];
                            }

                            ubLocalColor = (FontColor)184;

                            // calc length of what we just wrote
                            usPhraseLengthPixels = FontSubSystem.WFStringPixLength(new string(zLineString), uiLocalFont);

                            // calculate new x position for next time
                            usLocalPosX += usPhraseLengthPixels;

                            // shorten width for next time
                            usLocalWidth -= usLineLengthPixels;

                            // erase this word string we've been building - it was just a code
                            Array.Fill(zWordString, '\0');

                            // erase the line string, we're starting from scratch
                            Array.Fill(zLineString, '\0');

                            // reset dest char counter
                            usDestCounter = '\0';
                            break;



                        case TEXT_CODE.DEFCOLOR:


                            // calc length of what we just wrote
                            usPhraseLengthPixels = FontSubSystem.WFStringPixLength(new string(zLineString), uiLocalFont);

                            // calculate new x position for next time
                            usLocalPosX += usPhraseLengthPixels;

                            // shorten width for next time
                            usLocalWidth -= usLineLengthPixels;

                            // erase this word string we've been building - it was just a code
                            Array.Fill(zWordString, '\0');

                            // erase the line string, we're starting from scratch
                            Array.Fill(zLineString, '\0');

                            // change color back to default color
                            ubLocalColor = ubColor;

                            // reset dest char counter
                            usDestCounter = '\0';
                            break;


                    }       // end of switch of CODES

                }
                else // not a special character
                {
                    // terminate the string TEMPORARILY
                    zWordString[usDestCounter] = (char)0;

                    // get the length (in pixels) of this word
                    usWordLengthPixels = FontSubSystem.WFStringPixLength(new string(zWordString), uiLocalFont);

                    // add a space (in case we add another word to it)
                    zWordString[usDestCounter++] = (char)32;

                    // RE-terminate the string
                    zWordString[usDestCounter] = (char)0;

                    // can we fit it onto the length of our "line" ?
                    if ((usLineLengthPixels + usWordLengthPixels) <= usWidth)
                    {
                        // yes we can fit this word.

                        // get the length AGAIN (in pixels with the SPACE) for this word
                        usWordLengthPixels = FontSubSystem.WFStringPixLength(new string(zWordString), uiLocalFont);

                        // calc new pixel length for the line
                        usLineLengthPixels += usWordLengthPixels;

                        // reset dest char counter
                        usDestCounter = '\0';

                        // add the word (with the space) to the line
                        zLineString = wcscat(new string(zLineString), new string(zWordString)).ToCharArray();
                    }
                    else
                    {
                        // can't fit this word!


                        // increment Y position for next time
                        usPosY += (FontSubSystem.WFGetFontHeight(uiLocalFont)) + ubGap;

                        // reset x position
                        usLocalPosX = usPosX;

                        // we just used a line, so note that
                        usLinesUsed++;

                        // start off next line string with the word we couldn't fit
                        zLineString = wcscpy(zWordString.ToString()!).ToCharArray();

                        // remeasure the line length
                        usLineLengthPixels = FontSubSystem.WFStringPixLength(new string(zLineString), uiLocalFont);

                        // reset dest char counter
                        usDestCounter = '\0';

                        // reset width 
                        usLocalWidth = usWidth;
                    }
                }       // end of this word was NOT a special code

            }



        } while (pString[usSourceCounter++] != 0);

        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        // return how many Y pixels we used
        return (usLinesUsed * (FontSubSystem.WFGetFontHeight(uiFont) + ubGap)); // +ubGap
    }

    //
    // Pass in, the x,y location for the start of the string,
    //					the width of the buffer (how many pixels wide for word wrapping)
    //					the gap in between the lines
    //
    internal static int IanDisplayWrappedString(Point usPos, int usWidth, int ubGap, FontStyle uiFont, FontColor ubColor, string pString, FontColor ubBackGroundColor, bool fDirty, IAN_TEXT_FLAGS uiFlags)
    {
        int usHeight;
        int usSourceCounter = 0, usDestCounter = 0, usWordLengthPixels, usLineLengthPixels = 0, usPhraseLengthPixels = 0;
        int usLinesUsed = 1, usLocalWidth = usWidth;
        FontStyle uiLocalFont = uiFont;
        TextJustifies usJustification = TextJustifies.LEFT_JUSTIFIED;
        int usLocalPosX = usPos.X;
        FontColor ubLocalColor = ubColor;
        bool fBoldOn = false;

        char[] zLineString = new char[128];
        char[] zWordString = new char[64];

        usHeight = FontSubSystem.WFGetFontHeight(uiFont);

        var words = pString.Split(" ", StringSplitOptions.None);
        List<string> currentLine = [];

        foreach (var word in words)
        {
            if (word[0] >= TEXT_CODE_NEWLINE && word[0] <= TEXT_CODE_DEFCOLOR)
            {
                switch (word[0])
                {
                    case TEXT_CODE_NEWLINE:
                        FontSubSystem.DrawTextToScreen(
                            string.Join(' ', currentLine),
                            new(usLocalPosX, usPos.Y),
                            usLocalWidth,
                            uiLocalFont,
                            ubLocalColor,
                            ubBackGroundColor,
                            usJustification);

                        usLineLengthPixels = 0;
                        currentLine.Clear();
                        usPos.Y += (FontSubSystem.WFGetFontHeight(uiLocalFont)) + ubGap;//; // +ubGap
                        usLocalPosX = usPos.X;
                        usLinesUsed++;
                        break;
                    case TEXT_CODE_BOLD:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                usWordLengthPixels = FontSubSystem.WFStringPixLength(word, uiLocalFont);

                if ((usLineLengthPixels + usWordLengthPixels) < usWidth)
                {
                    // get the length AGAIN (in pixels with the SPACE) for this word
                    usWordLengthPixels = FontSubSystem.WFStringPixLength(word + " ", uiLocalFont);

                    // calc new pixel length for the line
                    usLineLengthPixels += usWordLengthPixels;
                    currentLine.Add(word);

                }
                else
                {
                    // shadow control
                    if (uiFlags.HasFlag(IAN_TEXT_FLAGS.IAN_WRAP_NO_SHADOW))
                    {
                        // turn off shadow
                        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);
                    }

                    // Display what we have up to now
                    FontSubSystem.DrawTextToScreen(
                        string.Join(' ', currentLine),
                        new(usLocalPosX, usPos.Y),
                        usLocalWidth,
                        uiLocalFont,
                        ubLocalColor,
                        ubBackGroundColor,
                        usJustification);

                    usLineLengthPixels = 0;
                    currentLine.Clear();

                    // shadow control
                    if (uiFlags.HasFlag(IAN_TEXT_FLAGS.IAN_WRAP_NO_SHADOW))
                    {
                        // turn off shadow
                        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
                    }

                    // increment Y position for next time
                    usPos.Y += (FontSubSystem.WFGetFontHeight(uiLocalFont)) + ubGap;//; // +ubGap

                    // reset x position
                    usLocalPosX = usPos.X;

                    // we just used a line, so note that
                    usLinesUsed++;
                }
            }
        }

        // shadow control
        if (uiFlags.HasFlag(IAN_TEXT_FLAGS.IAN_WRAP_NO_SHADOW))
        {
            // turn off shadow
            FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);
        }

        // draw the paragraph
        FontSubSystem.DrawTextToScreen(string.Join(' ', currentLine), new(usLocalPosX, usPos.Y), usLocalWidth, uiLocalFont, ubLocalColor, ubBackGroundColor, usJustification);

        // shadow control
        if (uiFlags.HasFlag(IAN_TEXT_FLAGS.IAN_WRAP_NO_SHADOW))
        {
            // turn on shadow
            FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        }

        // return how many Y pixels we used
        return (usLinesUsed * (FontSubSystem.WFGetFontHeight(uiFont) + ubGap)); // +ubGap
    }
}

public enum TEXT_CODE
{
    NEWLINE = 177,
    BOLD = 178,
    CENTER = 179,
    NEWCOLOR = 180,
    DEFCOLOR = 181,

    TEXT_SPACE = 32,
}

[Flags]
public enum IAN_TEXT_FLAGS
{
    LEFT_JUSTIFIED = 0x00000001,
    CENTER_JUSTIFIED = 0x00000002,
    RIGHT_JUSTIFIED = 0x00000004,
    TEXT_SHADOWED = 0x00000008,
    INVALIDATE_TEXT = 0x00000010,
    DONT_DISPLAY_TEXT = 0x00000020,//Wont display the text.  Used if you just want to get how many lines will be displayed
    IAN_WRAP_NO_SHADOW = 32,
}
