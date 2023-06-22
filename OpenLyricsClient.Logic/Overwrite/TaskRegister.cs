using DevBase.Async.Task;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Logic.Overwrite;

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
    
    public void Register(Action action, object type, bool startAfterCreation = true)
    {
        if (!AvaloniaUtils.IsInPreviewerMode())
        {
            this.RegisterTask(action, type, startAfterCreation);
        }
    }
}