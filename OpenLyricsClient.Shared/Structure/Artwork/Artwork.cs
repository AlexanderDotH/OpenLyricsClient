using Avalonia.Media.Imaging;
using OpenLyricsClient.Shared.Structure.Palette;
using OpenLyricsClient.Shared.Utils;
using Squalr.Engine.Utils.Extensions;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace OpenLyricsClient.Shared.Structure.Artwork
{
    [Serializable]
    public class Artwork
    {
        private byte[] _data;
        private ArtworkReturnCode _returnCode;
        private ColorPalette _artworkColor;
        private string _filePath;

        private bool _artworkApplied;
        private bool _artworkCalculated;
        
        public Artwork(byte[] data, ArtworkReturnCode returnCode)
        {
            this._data = data;
            this._returnCode = returnCode;

            this._artworkApplied = false;
        }
        
        public Artwork() : this(null, ArtworkReturnCode.FAILED) { }
        
        public bool ArtworkApplied
        {
            get => _artworkApplied;
            set => _artworkApplied = value;
        }

        public bool ArtworkCalculated
        {
            get => _artworkCalculated;
            set => _artworkCalculated = value;
        }

        public IBitmap ArtworkAsImage
        {
            get
            {
                if (this._data.IsNullOrEmpty())
                    return null;
                
                MemoryStream ms = new MemoryStream(this._data);
                Bitmap map = new Bitmap((Stream)ms);
                return map;
            }
        }

        public string FilePath
        {
            get => _filePath;
            set => _filePath = value;
        }

        public ColorPalette ArtworkColor
        {
            get => this._artworkColor;
            set => this._artworkColor = value;
        }

        public byte[] Data
        {
            get => this._data;
        }

        public ArtworkReturnCode ReturnCode
        {
            get => this._returnCode;
        }
    }
}
