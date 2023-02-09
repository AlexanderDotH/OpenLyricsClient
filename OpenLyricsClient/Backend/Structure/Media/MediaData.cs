using System;
using System.IO;

namespace OpenLyricsClient.Backend.Structure.Media;

public class MediaData
{
    private FileInfo _fileInfo;
    private byte[] _fileContent;
    private MediaReturnCode _mediaReturnCode;

    public MediaData()
    {
        this._mediaReturnCode = MediaReturnCode.FAILED;
    }
    
    public MediaData(FileInfo fileInfo, byte[] fileContent)
    {
        this._fileInfo = fileInfo;
        this._fileContent = fileContent;
        this._mediaReturnCode = MediaReturnCode.SUCCESS;
    }

    public FileInfo FileInfo
    {
        get => this._fileInfo;
        set => this._fileInfo = value;
    }

    public byte[] FileContent
    {
        get => this._fileContent;
        set => this._fileContent = value;
    }
}