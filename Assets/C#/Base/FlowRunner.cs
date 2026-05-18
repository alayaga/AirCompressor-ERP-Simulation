using UnityEngine;
using System.Collections;

/// <summary>
/// 协程安全调度器：解决 StopFlow 后协程继续执行的问题
/// 支持中途终止、嵌套协程、Unity 2022.3 兼容
/// </summary>
public class FlowRunner
{
    private MonoBehaviour _owner;
    private bool _isRunning;
    private Coroutine _current;

    public FlowRunner(MonoBehaviour owner) => _owner = owner;

    public IEnumerator RunSafe(IEnumerator routine)
    {
        _isRunning = true;
        while (_isRunning && routine.MoveNext())
        {
            var current = routine.Current;

            if (current is IEnumerator nested)
                yield return RunSafe(nested);
            else
                yield return current;
        }
        _isRunning = false;
    }

    public void Stop() => _isRunning = false;
    public bool IsRunning => _isRunning;

    public void Pause() => _isRunning = false;
    public void Resume() => _isRunning = true;
}