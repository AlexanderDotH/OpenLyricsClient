using System;
using System.IO;
using System.Text;
using SpotifyAPI.Web;
using Squalr.Engine.Utils.Extensions;

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

        public static byte[] ReadFile(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
                return null;

            byte[] buffer = new byte[fileInfo.Length];

            try
            {
                using (FileStream fileStream = fileInfo.OpenRead())
                {
                    using (BinaryReader reader = new BinaryReader(fileStream))
                    {
                        buffer = reader.ReadBytes(buffer.Length);
                    }
                }
                
                return buffer;
            }
            catch (Exception e) { }

            return null;
        }

        public static byte[] ReadFile(string filePath)
        {
            return ReadFile(new FileInfo(filePath));
        }

        public static string ReadFileString(FileInfo fileInfo)
        {
            byte[] data = ReadFile(fileInfo);

            if (!DataValidator.ValidateData(data))
                return null;
            
            return Encoding.UTF8.GetString(data);
        }
        
        public static string ReadFileString(string filePath)
        {
            byte[] data = ReadFile(new FileInfo(filePath));

            if (!DataValidator.ValidateData(data))
                return null;
            
            return Encoding.UTF8.GetString(data);
        }

        public static void WriteFile(FileInfo fileInfo, byte[] data)
        {
            if (data.IsNullOrEmpty())
                return;
            
            using (FileStream fileStream = File.Create(fileInfo.FullName))
            {
                fileStream.Write(data, 0, data.Length);
            }
        }

        public static void WriteFileString(FileInfo fileInfo, string data)
        {
            WriteFile(fileInfo, Encoding.UTF8.GetBytes(data));
        }
        
        public static void WriteFileString(string filePath, string data)
        {
            WriteFile(new FileInfo(filePath), Encoding.UTF8.GetBytes(data));
        }
        
    }
}
