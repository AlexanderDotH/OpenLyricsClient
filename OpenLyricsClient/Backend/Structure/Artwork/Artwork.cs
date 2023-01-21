using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private string _filePath;
        
        public Artwork(byte[] data, string filePath, ArtworkReturnCode returnCode)
        {
            this._data = data;
            this._returnCode = returnCode;
            this._filePath = filePath;
        }
        
        public Artwork() : this(null, string.Empty, ArtworkReturnCode.FAILED) { }

        public async Task CalculateColor()
        {
            if (this._data.IsNullOrEmpty())
                return;
            
            Task t = Task.Factory.StartNew(() =>
            {
                CalculateArtworkColor();
            });
            
            await t;
        }

        private bool IsColorValid(Color color, double brightnessPercentage)
        {
            double brightness = GetBrightness(color);

            if (brightness < brightnessPercentage || 
                brightness < brightnessPercentage && brightness > 90)
                return false;

            if (color.G == 255 && color.B == 255 && color.R < 180)
                return false;

            if (color.R == 0 && color.G == 0 && color.B == 0)
                return false;
            
            return true;
        }
        
        private void CalculateArtworkColor()
        {
            try
            {
                double brightnessPercentage = 20;

                Color groupColor = GetGroupColor();
                if (IsColorValid(groupColor, brightnessPercentage))
                {
                    this._artworkColor = groupColor;
                    return;
                }
                
                Color nearestColor = GetNearesColor();
                if (IsColorValid(nearestColor, brightnessPercentage))
                {
                    this._artworkColor = nearestColor;
                    return;
                }
                
                Color brightestColor = GetBrightnessColor();
                if (IsColorValid(brightestColor, brightnessPercentage))
                {
                    this._artworkColor = brightestColor;
                    return;
                }
                
                this._artworkColor = new Color(255, 220, 20, 60);
            }
            catch (Exception e)
            {
                this._artworkColor = new Color(255, 220, 20, 60);
            }

            this._data = null;
        }
        
        private Color GetGroupColor()
        {
            GroupColorCalculator colorCalculator = new GroupColorCalculator();
            colorCalculator.BigShift = 2.0;
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

        public double GetBrightness(Color color)
        {
            double averageColor = (color.R + color.G + color.B) / 3.0D;
            double min = 100D / 255D;
            double calc = min * averageColor;

            return Math.Clamp(calc, 0, 100);
        }
        
        public double GetBrightness()
        {
            return GetBrightness(this._artworkColor);
        }
        
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

        public string FilePath
        {
            get => _filePath;
            set => _filePath = value;
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
