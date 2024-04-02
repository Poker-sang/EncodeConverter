using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WinUI3Utilities;

namespace EncodeConverter.Misc;

public static class TranscodeHelper
{
    public static Task TranscodeDirectory(DirectoryInfo directory, int originalCodePage, int destinationCodePage, bool transcodeName, bool transcodeContent, Func<string, string, bool> predicate)
    {
        var originalEncoding = Encoding.GetEncoding(originalCodePage);
        var destinationEncoding = Encoding.GetEncoding(destinationCodePage);

        if (transcodeName)
        {
            var newName = TranscodeFullName(directory, originalEncoding);
            directory.MoveTo(newName);
        }
        return TranscodeDirectory(directory, originalEncoding, destinationEncoding, transcodeName, transcodeContent, predicate);
    }

    private static async Task TranscodeDirectory(DirectoryInfo directory, Encoding originalEncoding, Encoding destinationEncoding, bool transcodeName, bool transcodeContent, Func<string, string, bool> predicate)
    {
        foreach (var fileSystemInfo in directory.EnumerateFileSystemInfos())
        {
            switch (fileSystemInfo)
            {
                case FileInfo file:
                {
                    var newFullName = transcodeName ? TranscodeFullName(file, originalEncoding) : MoveAndTranscodeFullName(file, originalEncoding);
                    if (transcodeContent && predicate(fileSystemInfo.Name, fileSystemInfo.Extension))
                    {
                        var newFile = new FileInfo(newFullName);
                        await TranscodeFileContent(file, originalEncoding, destinationEncoding, newFile);
                        file.Delete();
                    }
                    else
                        file.MoveTo(newFullName);
                    break;
                }
                case DirectoryInfo dir:
                {
                    if (transcodeName)
                    {
                        var newName = TranscodeFullName(fileSystemInfo, originalEncoding);
                        dir.MoveTo(newName);
                    }
                    await TranscodeDirectory(dir, originalEncoding, destinationEncoding, transcodeName, transcodeContent, predicate);
                    break;
                }
            }
        }
    }

    public static async Task TranscodeFile(FileInfo file, int originalCodePage, int destinationCodePage, bool keepOriginalFile, bool transcodeName, bool transcodeContent)
    {
        var originalEncoding = Encoding.GetEncoding(originalCodePage);
        var destinationEncoding = Encoding.GetEncoding(destinationCodePage);

        FileInfo newFile;
        if (transcodeName)
        {
            if (transcodeContent)
            {
                newFile = new(MoveAndTranscodeFullName(file, originalEncoding));
                await TranscodeFileContent(file, originalEncoding, destinationEncoding, newFile);
            }
            else
            {
                var newName = MoveAndTranscodeFullName(file, originalEncoding);
                _ = file.CopyTo(newName);
            }
        }
        else
        {
            var originalFullName = file.FullName;
            file.MoveTo(file.GetFullNameWithOld());
            newFile = new(originalFullName);

            if (transcodeContent)
            {
                await TranscodeFileContent(file, originalEncoding, destinationEncoding, newFile);
            }
            else
            {
                ThrowHelper.InvalidOperation("No operation.");
            }
        }

        if (!keepOriginalFile)
            file.Delete();
    }

    public static string TranscodeString(string str, int strCodePage, int newCodePage)
    {
        var strEncoding = Encoding.GetEncoding(strCodePage);
        var newEncoding = Encoding.GetEncoding(newCodePage);

        return TranscodeString(str, strEncoding, newEncoding);
    }

    public static string TranscodeString(string str, Encoding strEncoding, Encoding newEncoding)
    {
        var bytes = strEncoding.GetBytes(str);
        return newEncoding.GetString(bytes);
    }

    public static string TranscodeNativeString(string str, Encoding originalEncoding)
    {
        return TranscodeString(str, EncodingHelper.SystemEncoding, originalEncoding);
    }

    public static string TranscodeStringToNative(string str, Encoding destinationEncoding)
    {
        return TranscodeString(str, destinationEncoding, EncodingHelper.SystemEncoding);
    }

    public static string TranscodeFullName(FileSystemInfo info, Encoding originalEncoding)
    {
        var bytes = EncodingHelper.SystemEncoding.GetBytes(info.Name);
        var newName = originalEncoding.GetString(bytes);
        var directoryName = Path.GetDirectoryName(info.FullName);
        return directoryName is null ? newName : Path.Combine(directoryName, newName);
    }

    public static async Task TranscodeFileContent(FileInfo file, Encoding originalEncoding, Encoding destinationEncoding, FileInfo destinationFile)
    {
        await using var originalFileStream = file.OpenRead();
        await using var destinationFileStream = destinationFile.OpenWrite();
        var buffer = new byte[file.Length];
        _ = await originalFileStream.ReadAsync(buffer);
        var originalContent = originalEncoding.GetString(buffer);
        var destinationContent = destinationEncoding.GetBytes(originalContent);
        await destinationFileStream.WriteAsync(destinationContent);
    }

    private static string MoveAndTranscodeFullName(FileInfo file, Encoding originalEncoding)
    {
        var newName = TranscodeFullName(file, originalEncoding);
        file.MoveTo(file.GetFullNameWithOld());
        return newName;
    }

    private static string GetFullNameWithOld(this FileSystemInfo info)
    {
        var fullnameExceptExtension = info.FullName[..^info.Extension.Length];
        var newExtension = ".old" + info.Extension;
        var newName = fullnameExceptExtension + newExtension;
        if (!File.Exists(newName) && !Directory.Exists(newName))
            return newName;
        for (var i = 0; ; ++i)
        {
            newName = $"{fullnameExceptExtension} ({i}){newExtension}";
            if (!File.Exists(newName) && !Directory.Exists(newName))
                return newName;
        }
    }
}
