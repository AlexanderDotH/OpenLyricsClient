﻿using System;
using Avalonia;
using OpenLyricsClient.Backend.Structure;
using X11;

namespace OpenLyricsClient.Backend.Utils;

public class X11
{
    public static Window GetFocusedWindow()
    {
        using (var wm = new XWindowManager())
        {
            wm.Open(null);

            XWindowInfo windowInfo = wm.GetFocusedWindow();
            return new Window(windowInfo.WmName, windowInfo.WmClass.InstanceName);
        }

        return null;
    }
}