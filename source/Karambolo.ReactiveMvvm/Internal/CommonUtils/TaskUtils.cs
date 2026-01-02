using System;
using System.Threading.Tasks;

namespace Karambolo.Common
{
    internal static class TaskUtils
    {
        public static async void FireAndForget(this Task task, Action<Exception> exceptionHandler, bool propagateCancellation = false)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            try { await task.ConfigureAwait(false); }
            catch (OperationCanceledException) when (!propagateCancellation) { }
            catch (Exception ex) when (exceptionHandler != null) { exceptionHandler(ex); }
        }
    }
}
