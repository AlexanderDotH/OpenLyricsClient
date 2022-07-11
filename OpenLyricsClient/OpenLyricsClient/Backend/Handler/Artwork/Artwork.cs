using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Handler.Artwork
{
    public class Artwork
    {
        private byte[] _data;

        public Artwork(byte[] data)
        {
            this._data = data;
        }

        public string ArtworkAsBase64String
        {
            get
            {
                if (this._data == null)
                    return string.Empty;

                return Encoding.UTF8.GetString(
                    Convert.FromBase64String(
                        Encoding.UTF8.GetString(this._data)));
            }
            set
            {
                if (value == null)
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
    }
}
