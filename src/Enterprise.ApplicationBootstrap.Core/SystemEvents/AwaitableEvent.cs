using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core.SystemEvents;

/// <summary>
/// Wrapper over <see cref="TaskCompletionSource"/>, that is going to let code save link for task in different points of system.
/// </summary>
public class AwaitableEvent
{
    private readonly TaskCompletionSource _completionSource;

    /// <summary> c-tor. </summary>
    public AwaitableEvent()
    {
        _completionSource = new TaskCompletionSource();
    }

    /// <summary>
    /// Task that represents promise of event in future.
    /// </summary>
    [NotNull]
    public Task Awaitable => _completionSource.Task;

    /// <summary>
    /// Marks task as completed, letting all awaits continue.
    /// </summary>
    public void Complete()
    {
        _completionSource.TrySetResult();
    }
}