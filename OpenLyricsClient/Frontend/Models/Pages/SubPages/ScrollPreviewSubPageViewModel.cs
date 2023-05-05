using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DynamicData;
using OpenLyricsClient.Shared.Structure.Lyrics;

namespace OpenLyricsClient.Frontend.Models.Pages.SubPages;

public class ScrollPreviewSubPageViewModel
{
    public ObservableCollection<LyricPart> _lyricParts;
    private LyricPart _lyricPart;
    
    public ScrollPreviewSubPageViewModel()
    {
        this._lyricParts = new ObservableCollection<LyricPart>();

        LyricPart part1 = new LyricPart(0, "We're no strangers to love");
        LyricPart part2 = new LyricPart(500, "You know the rules and so do I");
        LyricPart part3 = new LyricPart(1000, "A full commitment's what I'm thinking of");
        LyricPart part4 = new LyricPart(1500, "You wouldn't get this from any other guy");

        this.CurrentLyricParts.AddRange(new List<LyricPart>() {part1, part2, part3, part4});
        this.CurrentLyricPart = part1;
    }
    
    public ObservableCollection<LyricPart> CurrentLyricParts
    {
        get => _lyricParts;
        set
        {
            _lyricParts = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentLyricParts"));
        }
    }
    
    public LyricPart CurrentLyricPart
    {
        get => _lyricPart;
        set
        {
            _lyricPart = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentLyricPart"));
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}