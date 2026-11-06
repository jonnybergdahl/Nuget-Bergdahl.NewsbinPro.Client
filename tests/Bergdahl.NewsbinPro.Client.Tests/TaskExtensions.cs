using System;
using System.Threading.Tasks;

namespace Bergdahl.NewsbinPro.Client.Tests;

internal static class TaskExtensions
{
    /// <summary>
    /// Fails with a <see cref="TimeoutException"/> if the task doesn't finish in time.
    /// Works like <c>Task.WaitAsync</c>, which doesn't exist on .NET Framework.
    /// </summary>
    public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
    {
        if (await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false) != task)
            throw new TimeoutException($"The task didn't finish within {timeout}.");
        return await task.ConfigureAwait(false);
    }
}
