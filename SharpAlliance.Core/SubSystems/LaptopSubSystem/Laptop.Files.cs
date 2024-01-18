using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

public partial class Laptop
{
    // the files record list
    private static List<FilesUnit> pFilesListHead = [];
    // are we in files mode
    private static bool fInFilesMode = false;
    private static bool fOnLastFilesPageFlag = false;
    private static bool fNewFilesInFileViewer;

    private static void GameInitFiles()
    {
        if ((files.FileExists(FILES_DAT_FILE) == true))
        {
            files.FileClearAttributes(FILES_DAT_FILE);
            files.FileDelete(FILES_DAT_FILE);
        }

        ClearFilesList();

        // add background check by RIS
        AddFilesToPlayersLog(BACKGROUND.ENRICO, 0, 255, null, null);

    }

    private static int AddFilesToPlayersLog(BACKGROUND ubCode, uint uiDate, byte ubFormat, string? pFirstPicFile, string? pSecondPicFile)
    {
        // adds Files item to player's log(Files List), returns unique id number of it
        // outside of the Files system(the code in this .c file), this is the only function you'll ever need
        int uiId = 0;

        // if not in Files mode, read in from file
        if (!fInFilesMode)
        {
            OpenAndReadFilesFile();
        }

        // process the actual data
        uiId = ProcessAndEnterAFilesRecord(ubCode, uiDate, ubFormat, pFirstPicFile, pSecondPicFile, false);

        // set unread flag, if nessacary
        CheckForUnreadFiles();

        // write out to file if not in Files mode
        if (!fInFilesMode)
        {
            OpenAndWriteFilesFile();
        }

        // return unique id of this transaction
        return uiId;
    }

    private static bool OpenAndWriteFilesFile()
    {
        // this procedure will open and write out data from the finance list
        Stream hFileHandle;
        int iBytesWritten = 0;

        // open file
        hFileHandle = files.FileOpen(FILES_DAT_FILE, FileAccess.Write, FileMode.OpenOrCreate, false);

        // if no file exits, do nothing
        if (!hFileHandle.CanWrite)
        {
            return (false);
        }

        // write info, while there are elements left in the list
        foreach(var pFile in pFilesListHead)
        {
            string pFirstFilePath = pFile.pPicFileNameList[0];
            string pSecondFilePath = pFile.pPicFileNameList[1];

            // now write date and amount, and code
            files.FileWrite(hFileHandle, (pFile.ubCode), sizeof(int), out var _);
            files.FileWrite(hFileHandle, (pFile.uiDate), sizeof(uint), out var _);
            files.FileWrite(hFileHandle, (pFirstFilePath), 128, out var _);
            files.FileWrite(hFileHandle, (pSecondFilePath), 128, out var _);
            files.FileWrite(hFileHandle, (pFile.ubFormat), sizeof(byte), out var _);
            files.FileWrite(hFileHandle, (pFile.fRead), sizeof(bool), out var _);
        }

        // close file
        files.FileClose(hFileHandle);
        // clear out the old list
        ClearFilesList();

        return (true);
    }

    private static void CheckForUnreadFiles()
    {
        bool fStatusOfNewFileFlag = fNewFilesInFileViewer;

        // willc heck for any unread files and set flag if any

        fNewFilesInFileViewer = false;


        foreach (var pFilesList in pFilesListHead)
        {
            // unread?...if so, set flag
            if (pFilesList.fRead == false)
            {
                fNewFilesInFileViewer = true;
            }
        }

        //if the old flag and the new flag arent the same, either create or destory the fast help region
        if (fNewFilesInFileViewer != fStatusOfNewFileFlag)
        {
            CreateFileAndNewEmailIconFastHelpText(LaptopText.LAPTOP_BN_HLP_TXT_YOU_HAVE_NEW_FILE, !fNewFilesInFileViewer);
        }
    }

    private static void OpenAndReadFilesFile()
    {
        // this procedure will open and read in data to the finance list
        Stream hFileHandle;
        BACKGROUND ubCode = 0;
        uint uiDate = 0;
        int iBytesRead = 0;
        uint uiByteCount = 0;
        string pFirstFilePath = string.Empty;
        string pSecondFilePath = string.Empty;
        byte ubFormat = 0;
        bool fRead = false;

        // clear out the old list
        ClearFilesList();

        // no file, return
        if (!(files.FileExists(FILES_DAT_FILE)))
        {
            return;
        }

        // open file
        hFileHandle = files.FileOpen(FILES_DAT_FILE, FileAccess.Read, FileMode.Open, false);

        // failed to get file, return
        if (!hFileHandle.CanRead)
        {
            return;
        }

        // make sure file is more than 0 length
        if (files.FileGetSize(hFileHandle) == 0)
        {
            files.FileClose(hFileHandle);
            return;
        }

        // file exists, read in data, continue until file end
        while (files.FileGetSize(hFileHandle) > uiByteCount)
        {
            // read in data
            //            files.FileRead(hFileHandle, &ubCode, sizeof(byte), out iBytesRead);
            //            files.FileRead(hFileHandle, &uiDate, sizeof(uint), out iBytesRead);
            //            files.FileRead(hFileHandle, &pFirstFilePath, 128, out iBytesRead);
            //            files.FileRead(hFileHandle, &pSecondFilePath, 128, out iBytesRead);
            //            files.FileRead(hFileHandle, &ubFormat, sizeof(byte), out iBytesRead);
            //            files.FileRead(hFileHandle, &fRead, sizeof(bool), out iBytesRead);
            // add transaction
            ProcessAndEnterAFilesRecord(ubCode, uiDate, ubFormat, pFirstFilePath, pSecondFilePath, fRead);

            // increment byte counter
            uiByteCount += sizeof(uint) + sizeof(byte) + 128 + 128 + sizeof(byte) + sizeof(bool);
        }

        // close file 
        files.FileClose(hFileHandle);
    }

    private static int ProcessAndEnterAFilesRecord(BACKGROUND ubCode, uint uiDate, byte ubFormat, string pFirstPicFile, string pSecondPicFile, bool fRead)
    {
        int uiId = 0;

        foreach (var pFile in pFilesListHead)
        {
            // check to see if the file is already there
            if (pFile.ubCode == ubCode)
            {
                // if so, return it's id number
                return (pFile.uiIdNumber);
            }

            // increment id number
            uiId = pFile.uiIdNumber + 1;
        }

        // set up information passed
        pFilesListHead.Add(new()
        {
            ubCode = ubCode,
            uiDate = uiDate,
            uiIdNumber = uiId,
            ubFormat = ubFormat,
            fRead = fRead,
            pPicFileNameList = [pFirstPicFile, pSecondPicFile],
        });

        // return unique id
        return uiId;
    }

    private static void ClearFilesList()
    {
        // remove each element from list of transactions
        pFilesListHead.Clear();
    }
}

public class FilesUnit
{
    public BACKGROUND ubCode; // the code index in the files code table
    public int ubFormat; // layout format
    public int uiIdNumber; // unique id number
    public uint uiDate; // time in the world in global time (resolution, minutes)
    public bool fRead;
    public string[] pPicFileNameList = new string[2];
};

// special codes for special files
public enum BACKGROUND
{
    ENRICO = 0,
    SLAY,
    MATRON,
    IMPOSTER,
    TIFFANY,
    REXALL,
    ELGIN,

};
