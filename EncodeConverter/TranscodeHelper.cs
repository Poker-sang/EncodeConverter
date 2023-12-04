using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EncodeConverter;

public static class TranscodeHelper
{
    public static async Task Transcode(FileInfo file, int originalCodePage, int destinationCodePage, bool keepOriginalFile, bool transcodeName, bool transcodeContent)
    {
        var originalEncoding = Encoding.GetEncoding(originalCodePage);
        var destinationEncoding = Encoding.GetEncoding(destinationCodePage);

        FileInfo newFile;
        if (transcodeName)
        {
            if (transcodeContent)
            {
                newFile = new(TranscodeName(file, originalEncoding));
                await TranscodeContent(file, originalEncoding, destinationEncoding, newFile);
            }
            else
            {
                var newName = TranscodeName(file, originalEncoding);
                _ = file.CopyTo(newName);
            }
        }
        else
        {
            var originalFullName = file.FullName;
            file.MoveTo(file.GetNewName());
            newFile = new(originalFullName);

            if (transcodeContent)
            {
                await TranscodeContent(file, originalEncoding, destinationEncoding, newFile);
            }
            else
            {
                throw new Exception("No operation.");
            }
        }

        if (!keepOriginalFile)
            file.Delete();
    }

    public static string TranscodeName(FileInfo file, Encoding originalEncoding)
    {
        var originalName = file.Name;
        file.MoveTo(file.GetNewName());
        var bytes = EncodingHelper.SystemEncoding.GetBytes(originalName);
        var newName = originalEncoding.GetString(bytes);
        return $"{file.DirectoryName}\\{newName}";
    }

    public static async Task TranscodeContent(FileInfo file, Encoding originalEncoding, Encoding destinationEncoding, FileInfo destinationFile)
    {
        await using var originalFileStream = file.OpenRead();
        await using var destinationFileStream = destinationFile.OpenWrite();
        var buffer = new byte[file.Length];
        var length = await originalFileStream.ReadAsync(buffer);
        var originalContent = originalEncoding.GetString(buffer);
        var destinationContent = destinationEncoding.GetBytes(originalContent);
        await destinationFileStream.WriteAsync(destinationContent);
    }

    private static string GetNewName(this FileInfo file)
    {
        var fullnameExceptExtension = file.FullName[..^file.Extension.Length];
        var newExtension = ".old" + file.Extension;
        var newName = fullnameExceptExtension + newExtension;
        if (!File.Exists(newName))
            return newName;
        for (var i = 0; ; ++i)
        {
            newName = $"{fullnameExceptExtension} ({i}){newExtension}";
            if (!File.Exists(newName))
                return newName;
        }
    }
}
