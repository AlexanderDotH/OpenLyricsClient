using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Squalr.Engine.Utils.Extensions;

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

        public Image ArtworkAsImage
        {
            get
            {
                if (this._data == null)
                    return null;

                MemoryStream stream = new MemoryStream(this._data);
                return Image.FromStream(stream);
            }
            set
            {
                if (this._data == null)
                    return;

                if (value == null)
                    return;

                MemoryStream stream = new MemoryStream(this._data);
                value.Save(stream, ImageFormat.Png);
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
