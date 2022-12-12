using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Avalonia.Media.Imaging;
using DevBaseColor.Image;
using Squalr.Engine.Utils.Extensions;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Color = Avalonia.Media.Color;

namespace OpenLyricsClient.Backend.Structure.Artwork
{
    [Serializable]
    public class Artwork
    {
        private byte[] _data;
        private ArtworkReturnCode _returnCode;
        private Color _artworkColor;

        public Artwork(byte[] data, ArtworkReturnCode returnCode)
        {
            this._data = data;
            this._returnCode = returnCode;

            if (!data.IsNullOrEmpty())
            {
                GroupColorCalculator colorCalculator = new GroupColorCalculator();
                colorCalculator.BigShift = 2.3;
                colorCalculator.SmallShift = 2.0;
                colorCalculator.Brightness = 40;
                this._artworkColor = colorCalculator.GetColorFromBitmap(this.ArtworkAsImage);
            }
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

        public Color ArtworkColor
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
