using System;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Image = System.Drawing.Image;

namespace OpenLyricsClient.Frontend.Structure;

public class ListBoxElement
{
    public string Text { get; set; }
    public Bitmap Image { get; set; }
}