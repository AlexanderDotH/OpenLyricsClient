using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Utils
{
    class TaskUtils
    {
        //Ripped straight from Stackoverflow
        // https://stackoverflow.com/questions/4238345/asynchronously-wait-for-taskt-to-complete-with-timeout#11191070
        public static async Task<TResult> CancelAfterAsync<TResult>(
            Func<CancellationToken, Task<TResult>> startTask,
            TimeSpan timeout, CancellationToken cancellationToken)
        {
            using (var timeoutCancellation = new CancellationTokenSource())
            using (var combinedCancellation = CancellationTokenSource
                       .CreateLinkedTokenSource(cancellationToken, timeoutCancellation.Token))
            {
                var originalTask = startTask(combinedCancellation.Token);
                var delayTask = Task.Delay(timeout, timeoutCancellation.Token);
                var completedTask = await Task.WhenAny(originalTask, delayTask);
                // Cancel timeout to stop either task:
                // - Either the original task completed, so we need to cancel the delay task.
                // - Or the timeout expired, so we need to cancel the original task.
                // Canceling will not affect a task, that is already completed.
                timeoutCancellation.Cancel();
                if (completedTask == originalTask)
                {
                    // original task completed
                    return await originalTask;
                }
                else
                {
                    // timeout
                    throw new TimeoutException();
                }
            }
        }
    }
}
