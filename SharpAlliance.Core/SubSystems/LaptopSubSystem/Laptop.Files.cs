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

    private const int FILES_PATH_SIZE = 128;

    private static void WritePath(BinaryWriter writer, string? path)
    {
        byte[] buffer = new byte[FILES_PATH_SIZE];
        if (!string.IsNullOrEmpty(path))
        {
            System.Text.Encoding.ASCII.GetBytes(path, 0, Math.Min(path.Length, FILES_PATH_SIZE - 1), buffer, 0);
        }

        writer.Write(buffer);
    }

    private static string ReadPath(BinaryReader reader)
    {
        byte[] buffer = reader.ReadBytes(FILES_PATH_SIZE);
        int length = Array.IndexOf(buffer, (byte)0);
        return System.Text.Encoding.ASCII.GetString(buffer, 0, length < 0 ? buffer.Length : length);
    }

    private static bool OpenAndWriteFilesFile()
    {
        // this procedure will open and write out data from the files list
        Stream hFileHandle = files.FileOpen(FILES_DAT_FILE, FileAccess.Write, FileMode.Create, false);

        // if no file exits, do nothing
        if (!hFileHandle.CanWrite)
        {
            return false;
        }

        using (var writer = new BinaryWriter(hFileHandle, System.Text.Encoding.ASCII, leaveOpen: true))
        {
            foreach (var pFile in pFilesListHead)
            {
                writer.Write((int)pFile.ubCode);
                writer.Write(pFile.uiDate);
                WritePath(writer, pFile.pPicFileNameList[0]);
                WritePath(writer, pFile.pPicFileNameList[1]);
                writer.Write((byte)pFile.ubFormat);
                writer.Write(pFile.fRead);
            }
        }

        files.FileClose(hFileHandle);
        return true;
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
        // this procedure will open and read in data to the files list
        ClearFilesList();

        // no file, return
        if (!files.FileExists(FILES_DAT_FILE))
        {
            return;
        }

        Stream hFileHandle = files.FileOpen(FILES_DAT_FILE, FileAccess.Read, FileMode.Open, false);

        // failed to get file, return
        if (!hFileHandle.CanRead)
        {
            return;
        }

        const int recordSize = sizeof(int) + sizeof(uint) + FILES_PATH_SIZE + FILES_PATH_SIZE + sizeof(byte) + sizeof(bool);

        using (var reader = new BinaryReader(hFileHandle, System.Text.Encoding.ASCII, leaveOpen: true))
        {
            // file exists, read in data, continue until file end
            while (hFileHandle.Length - hFileHandle.Position >= recordSize)
            {
                BACKGROUND ubCode = (BACKGROUND)reader.ReadInt32();
                uint uiDate = reader.ReadUInt32();
                string pFirstFilePath = ReadPath(reader);
                string pSecondFilePath = ReadPath(reader);
                byte ubFormat = reader.ReadByte();
                bool fRead = reader.ReadBoolean();

                ProcessAndEnterAFilesRecord(ubCode, uiDate, ubFormat, pFirstFilePath, pSecondFilePath, fRead);
            }
        }

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
