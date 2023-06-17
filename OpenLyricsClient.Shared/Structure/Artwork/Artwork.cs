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
using Colourful;
using DevBase.Avalonia.Color.Extensions;
using DevBase.Avalonia.Color.Image;
using DevBase.Avalonia.Extension.Color.Image;
using DevBase.Avalonia.Extension.Converter;
using DevBase.Avalonia.Extension.Extension;
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
                this._artworkColor = GetClusterColor();
            });

            await t;
        }

        private Color GetClusterColor()
        {
            try
            {
                LabClusterColorCalculator labCluster = new LabClusterColorCalculator();
                return labCluster.GetColorFromBitmap(this.ArtworkAsImage);
            }
            catch (Exception e)
            {
                RGBToLabConverter converter = new RGBToLabConverter();
                return this._artworkColor
                    .Normalize()
                    .ToRgbColor()
                    .ToLabColor(converter)
                    .ToPastel()
                    .ToRgbColor(converter)
                    .DeNormalize();
            }
        }

        public double GetBrightness()
        {
            return this._artworkColor.BrightnessPercentage();
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
