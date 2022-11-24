using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Avalonia.Media.Imaging;
using Squalr.Engine.Utils.Extensions;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace OpenLyricsClient.Backend.Structure.Artwork
{
    [Serializable]
    public class Artwork
    {
        private byte[] _data;
        private ArtworkReturnCode _returnCode;

        public Artwork(byte[] data, ArtworkReturnCode returnCode)
        {
            this._data = data;
            this._returnCode = returnCode;
        }

        public Artwork() : this(null, ArtworkReturnCode.FAILED) { }

        public string ArtworkAsBase64String
        {
            get
            {
                if (this._data.IsNullOrEmpty())
                    return string.Empty;
                
                return Convert.ToBase64String(this._data);
            }
            set
            {
                if (value.IsNullOrEmpty())
                    return;
                
                this._data = Convert.FromBase64String(value);
            }
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
