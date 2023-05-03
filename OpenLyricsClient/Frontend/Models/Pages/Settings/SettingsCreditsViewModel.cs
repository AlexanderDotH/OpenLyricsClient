﻿using System.Reactive;
using OpenLyricsClient.Backend.Utils;
using ReactiveUI;

namespace OpenLyricsClient.Frontend.Models.Pages.Settings;

public class SettingsCreditsViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> OpenEimaenGithubCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenD3SOXGithubCommand { get; }


    public SettingsCreditsViewModel()
    {
        OpenEimaenGithubCommand = ReactiveCommand.Create(()=>ProcessUtils.OpenBrowser("https://github.com/Eimaen"));
        OpenD3SOXGithubCommand = ReactiveCommand.Create(()=>ProcessUtils.OpenBrowser("https://github.com/D3SOX"));
    }
}