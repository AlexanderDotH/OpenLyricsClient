using System.Collections.Generic;
using OpenLyricsClient.Backend.Romanization;

namespace OpenLyricsClient.Backend.Settings.Sections.Romanization;

public class Structure
{
    public List<RomanizeSelection> Selections { get; set; }
}