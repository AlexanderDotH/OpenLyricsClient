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
               CalculateColor();
            }
        }

        private void CalculateColor()
        {
            this._artworkColor = GetGroupColor();

            double brightnessPercentage = 10;
                
            if (GetBrightness(this._artworkColor) < brightnessPercentage)
            {
                this._artworkColor = GetBrightnessColor();

                if (GetBrightness(this._artworkColor) < brightnessPercentage)
                {
                    this._artworkColor = GetNearesColor();

                    if (GetBrightness(this._artworkColor) < brightnessPercentage)
                    {
                        this._artworkColor = new Color(255, 220, 20, 60);
                    }
                }
            }
        }
        
        private Color GetGroupColor()
        {
            GroupColorCalculator colorCalculator = new GroupColorCalculator();
            colorCalculator.BigShift = 2.3;
            colorCalculator.SmallShift = 2.0;
            colorCalculator.Brightness = 40;

            return colorCalculator.GetColorFromBitmap(this.ArtworkAsImage);
        }
        
        private Color GetBrightnessColor()
        {
            BrightestColorCalculator colorCalculator = new BrightestColorCalculator();
            return colorCalculator.GetColorFromBitmap(this.ArtworkAsImage);
        }
        
        private Color GetNearesColor()
        {
            NearestColorCalculator colorCalculator = new NearestColorCalculator();
            colorCalculator.PixelSteps = 20;
            return colorCalculator.GetColorFromBitmap(this.ArtworkAsImage);
        }

        private double GetBrightness(Color color)
        {
            double averageColor = 0;

            averageColor += color.R;
            averageColor += color.G;
            averageColor += color.B;

            averageColor /= 3;

            double min = 0.3921568627;
            double calc = min * averageColor;

            if (calc < 0)
                calc = 0;

            if (calc > 100)
                calc = 100;
            
            return calc;
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
