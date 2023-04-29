using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Platform.Interfaces;
using Vortice.Mathematics.PackedVector;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers;

public class FileManager : IFileManager
{
    private static ILibraryManager library;
    //The FileDatabaseHeader
    public static DatabaseManagerHeader gFileDataBase { get; } = new();

    public FileManager(ILibraryManager libraryFileManager)
    {
        library = libraryFileManager;
    }

    public static bool IsInitialized { get; private set; }

    public ValueTask<bool> Initialize()
    {
        var lm = (library as LibraryFileManager)!;

        lm.gFileDataBase = gFileDataBase;
        gFileDataBase.pLibraries = lm.Libraries;

        gFileDataBase.fInitialized = true;
        IsInitialized = true;

        return ValueTask.FromResult(true);
    }

    public Stream FileOpen(string strFilename, FileAccess uiOptions, bool fDeleteOnClose = false)
    {
        Stream hFile = Stream.Null;
        FileStream? realFileStream = null;
        FileAccess dwAccess;
        FileOptions dwFlagsAndAttributes;
        //HDBFILE hDBFile;
        bool fExists;
        int dwCreationFlags;
        Stream hLibFile;

        //hDBFile = 0;
        dwCreationFlags = 0;

        // check if the file exists - note that we use the function FileExistsNoDB
        // because it doesn't check the databases, and we don't want to do that here
        fExists = File.Exists(strFilename);

        dwAccess = 0;
        if (uiOptions.HasFlag(FileAccess.Read))
        {
            dwAccess |= FileAccess.Read;
        }

        if (uiOptions.HasFlag(FileAccess.Write))
        {
            dwAccess |= FileAccess.Write;
        }

        dwFlagsAndAttributes = FileOptions.RandomAccess;
        if (fDeleteOnClose)
        {
            dwFlagsAndAttributes |= FileOptions.DeleteOnClose;
        }

        //if the file is on the disk
        if (false)
        {
            realFileStream = File.Open(
                strFilename,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            if (!realFileStream.SafeFileHandle.IsInvalid)
            {
                AddToOpenFile(realFileStream);

                return realFileStream;
            }

            //create a file handle for the 'real file'
            //hFile = CreateRealFileHandle(hRealFile);
        }
        else if (gFileDataBase.fInitialized)
        {
            //if the file is to be opened for writing, return an error cause you cant write a file that is in the database library
            if (fDeleteOnClose)
            {
                return Stream.Null;
            }

            //if the file doesnt exist on the harddrive, but it is to be created, dont try to load it from the file database
            if (uiOptions.HasFlag(FileAccess.Write))
            {
                return Stream.Null;
            }

            //If the file is in the library, get the stream to it.
            hLibFile = library.OpenFileFromLibrary(strFilename);
            //tried to open a file that wasnt in the database
            if (!hLibFile.CanRead)
            {
                return Stream.Null;
            }
            else
            {
                return hLibFile;      //return the file handle
            }
        }

        // if (!hFile.CanRead)
        // {
        //     if (uiOptions & FILE_CREATE_NEW)
        //     {
        //         dwCreationFlags = CREATE_NEW;
        //     }
        //     else if (uiOptions & FILE_CREATE_ALWAYS)
        //     {
        //         dwCreationFlags = CREATE_ALWAYS;
        //     }
        //     else if (uiOptions & FILE_OPEN_EXISTING || uiOptions & FILE_ACCESS_READ)
        //     {
        //         dwCreationFlags = OPEN_EXISTING;
        //     }
        //     else if (uiOptions & FILE_OPEN_ALWAYS)
        //     {
        //         dwCreationFlags = OPEN_ALWAYS;
        //     }
        //     else if (uiOptions & FILE_TRUNCATE_EXISTING)
        //     {
        //         dwCreationFlags = TRUNCATE_EXISTING;
        //     }
        //     else
        //     {
        //         dwCreationFlags = OPEN_ALWAYS;
        //     }
        // 
        // 
        //     hRealFile = CreateFile(strFilename, dwAccess, 0, null, dwCreationFlags,
        //                                     dwFlagsAndAttributes, null);
        //     if (hRealFile == INVALID_HANDLE_VALUE)
        //     {
        //         //int uiLastError = GetLastError();
        //         string zString;
        //         // FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, 0, uiLastError, 0, zString, 1024, null);
        // 
        //         return Stream.Null;
        //     }
        // 
        //     hFile = CreateRealFileHandle(hRealFile);
        // }

        if (!hFile.CanRead)
        {
            return Stream.Null;
        }

        return realFileStream;
    }

    private Stream AddToOpenFile(Stream hLibFile)
    {
        gFileDataBase.RealFiles.pRealFilesOpen.Add(new Library.RealFileOpen
        {
            Stream = hLibFile,
        });

        return hLibFile;
    }

    public void Dispose()
    {
    }

    public void FileClose(Stream fptr)
    {
    }

    private const int ROTATION_ARRAY_SIZE = 46;

    public bool FileExists(string pFilename)
    {
        var exists = false;

        // Check if file actually exists on disk...
        exists = File.Exists(pFilename);

        // look through libraries for file...
        if (!exists && LibraryFileManager.IsInitialized)
        {
            exists = library.CheckIfFileExistsInLibrary(pFilename);
        }

        return exists;
    }

    static byte[] ubRotationArray = new byte[]
    {
        132, 235, 125, 99, 15, 220, 140, 89, 205,
        132, 254, 144, 217, 78, 156, 58, 215, 76,
        163, 187, 55, 49, 65, 48, 156, 140, 201,
        68, 184, 13, 45, 69, 102, 185, 122, 225,
        23, 250, 160, 220, 114, 240, 64, 175, /*057 - C handles leading zeroes stupidly */ 47, 233
    };

    public unsafe ReadOnlySpan<byte> JA2EncryptedFileRead(Stream hFile, int uiBytesToRead, out int puiBytesRead)
    {
        uint uiLoop;
        byte ubArrayIndex = 0;
        byte ubLastByte = 0;
        byte ubLastByteForNextLoop;
        bool fRet;
        byte[] pMemBlock = new byte[uiBytesToRead];

        fRet = FileRead(hFile, ref pMemBlock, uiBytesToRead, out puiBytesRead);
        try
        {
            if (fRet)
            {
                for (uiLoop = 0; uiLoop < puiBytesRead; uiLoop++)
                {
                    ubLastByteForNextLoop = pMemBlock[uiLoop];
                    pMemBlock[uiLoop] -= (byte)(ubLastByte + ubRotationArray[ubArrayIndex]);
                    ubArrayIndex++;
                    if (ubArrayIndex >= ROTATION_ARRAY_SIZE)
                    {
                        ubArrayIndex = 0;
                    }

                    ubLastByte = ubLastByteForNextLoop;
                }
            }
        }
        catch (Exception e)
        {
            fRet = false;
        }

        return fRet
            ? pMemBlock
            : null;
    }

    public long SetFilePointer(Stream hLibraryHandle, int offset, SeekOrigin origin)
        => hLibraryHandle.Seek(offset, origin);

    public bool FileRead(Stream stream, ref byte[] buffer, int uiBytesToRead, out int uiBytesRead)
    {
        uiBytesRead = stream.Read(buffer, 0, uiBytesToRead);

        return true;
    }

    public bool FileRead(Stream stream, Span<byte> buffer, out uint bytesRead)
    {
        bytesRead = (uint)stream.Read(buffer);

        return true;
    }

    public bool FileSeek(Stream stream, ref uint uiStoredSize, SeekOrigin current)
    {
        stream.Seek(uiStoredSize, current);

        return true;
    }

    public bool FileRead<T>(Stream stream, ref T obj, int uiFileSectionSize, out int uiBytesRead)
        where T : struct
    {
        T[] buff = new T[1];
        var result = FileRead(stream, ref buff, uiFileSectionSize, out uiBytesRead);

        if (result)
        {
            obj = buff.FirstOrDefault();
        }

        return result;
    }

    public bool FileRead<T>(Stream stream, ref T[] obj, int uiFileSectionSize, out int uiBytesRead)
        where T : struct
    {
        var buffer = new byte[uiFileSectionSize];
        uiBytesRead = stream.Read(buffer, 0, uiFileSectionSize);
        var bufferSpan = new ReadOnlySpan<byte>(buffer);

        var tSpan = MemoryMarshal.Cast<byte, T>(bufferSpan);

        obj = tSpan.ToArray();
        return true;
    }

    public bool LoadEncryptedDataFromFile(string pFileName, out string pDestString, int seekFrom, int uiSeekAmount)
        => LoadEncryptedDataFromFile(pFileName, out pDestString, (uint)seekFrom, (uint)uiSeekAmount);

    public bool LoadEncryptedDataFromFile(string pFileName, out string pDestString, uint seekFrom, uint uiSeekAmount)
    {
        pDestString = string.Empty;

        using var stream = this.FileOpen(pFileName, FileAccess.Read, false);
        if (stream is null)
        {
            // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "LoadEncryptedDataFromFile: Failed to FileOpen");
            return false;
        }

        if (FileSeek(stream, ref seekFrom, SeekOrigin.Begin) == false)
        {
            FileClose(stream);
            // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "LoadEncryptedDataFromFile: Failed FileSeek");
            return false;
        }

        byte[] buff = new byte[uiSeekAmount];
        if (!FileRead(stream, ref buff, (int)uiSeekAmount, out var uiBytesRead))
        {
            FileClose(stream);
            // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "LoadEncryptedDataFromFile: Failed FileRead");
            return false;
        }

        pDestString = MemoryMarshal.Cast<byte, char>(buff).ToString();

        Span<char> span = pDestString.ToCharArray();
        // Decrement, by 1, any value > 32

        int i = 0;

        try
        {
            for (i = 0; (i < uiSeekAmount) && i < pDestString.Length && (pDestString[i] != '\0'); i++)
            {
                if (span[i] > 33)
                {
                    span[i] -= (char)1;
                }
            }
        }
        catch (Exception e)
        {

        }
        pDestString = span.Slice(0, i).ToString();
        FileClose(stream);

        return true;
    }

    public void FileWrite<T>(Stream stream, T value, int size, out int bytesWritten)
    {
        bytesWritten = size;
    }

    public FileAttributes FileGetAttributes(string filePath) => File.GetAttributes(filePath);

    public bool MakeFileManDirectory(string saveDir)
    {
        var result = Directory.CreateDirectory(saveDir);
        return result.Exists;
    }

    public bool FileDelete(string filePath)
    {
        File.Delete(filePath);

        return File.Exists(filePath);
    }
}
