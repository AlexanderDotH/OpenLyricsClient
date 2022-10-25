﻿using DevBase.Async.Task;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Overwrite;

public class TaskRegister : DevBase.Async.Task.TaskRegister
{
    public void Register(out TaskSuspensionToken token, System.Threading.Tasks.Task task, object type, bool startAfterCreation = true)
    {
        if (!AvaloniaUtils.IsInPreviewerMode())
        {
            TaskSuspensionToken oToken;
            this.RegisterTask(out oToken, task, type, startAfterCreation);
            token = oToken;
        }

        token = new TaskSuspensionToken();
    }
}