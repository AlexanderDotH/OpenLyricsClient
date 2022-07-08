using System.IO;

namespace OpenLyricsClient.Backend.Utils
{
    public class FileUtils
    {
        public static FileInfo SafeFileReadAccess(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
                return null;

            string temp = Path.GetTempFileName();
            FileInfo newFile = new FileInfo(temp);

            if (newFile.Exists)
                File.Delete(newFile.FullName);

            File.Copy(fileInfo.FullName, newFile.FullName);

            return newFile;
        }

    }
}
