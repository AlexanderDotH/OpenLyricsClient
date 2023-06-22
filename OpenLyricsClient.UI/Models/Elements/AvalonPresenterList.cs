using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;
using OpenLyricsClient.UI.Structure;

namespace OpenLyricsClient.UI.Models.Elements;

public class AvalonPresenterList : TemplatedControl
{
    public static DirectProperty<AvalonPresenterList, List<AvalonPresenterElement>> ElementsProperty = 
        AvaloniaProperty.RegisterDirect<AvalonPresenterList, List<AvalonPresenterElement>>(nameof(Elements), 
            o => o.Elements, 
            (o, v) => o.Elements = v);

    private List<AvalonPresenterElement> _elements;

    public List<AvalonPresenterElement> Elements
    {
        get
        {
            return this._elements;
        }
        set
        {
            SetAndRaise(ElementsProperty, ref _elements, value);
        }
    }
}