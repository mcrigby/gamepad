namespace System.IO;

public static class FileStreamExtensions
{
    public static async Task<int> ReadWithTimeoutAsync(this FileStream filestream, byte[] buffer, int offset, int length, int timeout = 500)
    {
        Task<int> readTask = filestream.ReadAsync(buffer, offset, length);
        Task timeoutTask = Task.Delay(timeout);

        var result = await Task.Factory.ContinueWhenAny<int>(new Task[] { readTask, timeoutTask }, completedTask => {
            if (completedTask == readTask)
                return readTask.Result;

            return 0;
        });

        return result;
    }

    public static async Task<int> ReadWithCancellationAsync(this FileStream filestream, byte[] buffer, int offset, int length, CancellationToken cancellationToken)
    {
        Task<int> readTask = filestream.ReadAsync(buffer, offset, length);
        Task cancellationTask = Task.Run(async () => { while(!cancellationToken.IsCancellationRequested) await Task.Delay(500); });

        var result = await Task.Factory.ContinueWhenAny<int>(new Task[] { readTask, cancellationTask }, completedTask => {
            if (completedTask == readTask)
                return readTask.Result;

            return 0;
        });

        return result;
    }
}