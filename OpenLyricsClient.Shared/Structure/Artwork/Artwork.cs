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
using DevBase.Avalonia.Color.Extensions;
using DevBase.Avalonia.Color.Image;
using Squalr.Engine.Utils.Extensions;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Color = Avalonia.Media.Color;

namespace OpenLyricsClient.Shared.Structure.Artwork
{
    [Serializable]
    public class Artwork
    {
        private byte[] _data;
        private ArtworkReturnCode _returnCode;
        private Color _artworkColor;
        private string _filePath;

        private bool _artworkApplied;
        
        public Artwork(byte[] data, string filePath, ArtworkReturnCode returnCode)
        {
            this._data = data;
            this._returnCode = returnCode;
            this._filePath = filePath;

            this._artworkApplied = false;
            
            this._artworkColor = new Color(255, 220, 20, 60);
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
            double brightness = color.BrightnessPercentage();

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
                double brightnessPercentage = 30;

                Color clusterColor = GetClusterColor();
                if (IsColorValid(clusterColor, brightnessPercentage))
                {
                    this._artworkColor = clusterColor;
                    return;
                }
                
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
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private Color GetClusterColor()
        {
            ClusterColorCalculator clusterColorCalculator = new ClusterColorCalculator();

            clusterColorCalculator.PredefinedDataset = true;
            clusterColorCalculator.FilterSaturation = true;
            clusterColorCalculator.FilterBrightness = false;

            clusterColorCalculator.SmallShift = 1.0d;
            clusterColorCalculator.BigShift = 1.0d;
            
            clusterColorCalculator.MinSaturation = 30;
            clusterColorCalculator.MaxRange = 3;
            clusterColorCalculator.Clusters = 10;
            
            return clusterColorCalculator.GetColorFromBitmap(this.ArtworkAsImage);
        }
        
        private Color GetGroupColor()
        {
            GroupColorCalculator colorCalculator = new GroupColorCalculator();
            colorCalculator.BigShift = 1.5;
            colorCalculator.SmallShift = 1;
            colorCalculator.Brightness = 70;

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

        public bool ArtworkApplied
        {
            get => _artworkApplied;
            set => _artworkApplied = value;
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
        
        public Color DarkArtworkColor
        {
            get => new Color(
                255,
                (byte)Math.Clamp(this._artworkColor.R - 10, 0, 255),
                (byte)Math.Clamp(this._artworkColor.G - 10, 0, 255),
                (byte)Math.Clamp(this._artworkColor.B - 10, 0, 255));
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
