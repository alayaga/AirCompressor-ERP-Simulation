using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 流程基类
/// </summary>
public abstract class FlowBase
{
    public event Action OnFlowStart;
    public event Action OnFlowEnd;
    protected bool _isRunning = false;
    private Coroutine _currentCoroutine = null;
    private MonoBehaviour _coroutineRunner = null;
    private FlowRunner _safeRunner;
    protected bool _allowSafeStop = true;

    public bool IsRunning => _isRunning;

    public FlowBase()
    {
        _coroutineRunner = GetCoroutineRunner();
    }

    public virtual void StartFlow()
    {
        if (_isRunning)
        {
            Debug.LogWarning($"流程 {GetType().Name} 已在运行中");
            return;
        }

        if (_coroutineRunner == null)
        {
            _coroutineRunner = GetCoroutineRunner();
            if (_coroutineRunner == null)
            {
                Debug.LogError($"无法获取协程运行器，流程 {GetType().Name} 启动失败");
                return;
            }
        }

        Debug.Log($"开始流程: {GetType().Name}");
        _isRunning = true;
        OnFlowStart?.Invoke();
        _currentCoroutine = _coroutineRunner.StartCoroutine(FlowCoroutine());
    }

    public virtual void StopFlow()
    {
        if (!_isRunning)
        {
            Debug.LogWarning($"流程 {GetType().Name} 未在运行");
            return;
        }

        Debug.Log($"停止流程: {GetType().Name}");

        if (_allowSafeStop && _safeRunner != null)
        {
            _safeRunner.Stop();
        }

        if (_currentCoroutine != null && _coroutineRunner != null)
        {
            _coroutineRunner.StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
        }

        _isRunning = false;
        OnFlowEnd?.Invoke();
    }

    protected abstract IEnumerator FlowCoroutine();

    /// <summary>
    /// 完成当前步骤（由外部调用）
    /// </summary>
    public virtual void MarkStepComplete()
    {
        // 基类不做任何事，子类可重写
    }

    protected MonoBehaviour GetCoroutineRunner()
    {
        if (ObjectManager.Instance != null)
        {
            GameObject playerObject = ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player);
            if (playerObject != null)
            {
                MonoBehaviour behaviour = playerObject.GetComponent<MonoBehaviour>();
                if (behaviour != null) return behaviour;
                return playerObject.AddComponent<MonoBehaviourHelper>();
            }
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            MonoBehaviour behaviour = player.GetComponent<MonoBehaviour>();
            if (behaviour != null) return behaviour;
            return player.AddComponent<MonoBehaviourHelper>();
        }

        Debug.LogError("未找到合适的协程运行器");
        return null;
    }

    private class MonoBehaviourHelper : MonoBehaviour { }

    protected IEnumerator WaitForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    protected IEnumerator WaitUntil(Func<bool> condition)
    {
        yield return new WaitUntil(condition);
    }

    protected IEnumerator WaitForFrame()
    {
        yield return null;
    }

    protected IEnumerator WaitForFixedUpdate()
    {
        yield return new WaitForFixedUpdate();
    }

    protected IEnumerator WaitForPlayerReachPosition(Vector3 targetPosition, float tolerance = 1.5f)
    {
        Debug.Log($"等待玩家到达: {targetPosition}");

        GameObject playerObject = ObjectManager.Instance?.GetObject(ObjectManager.ObjectType.Player);
        if (playerObject == null)
        {
            Debug.LogWarning("未找到玩家对象");
            yield break;
        }

        // ✅ 修改：将 PlayerController 改为 SimpleFirstPersonController
        SimpleFirstPersonController playerController = playerObject.GetComponent<SimpleFirstPersonController>();
        playerController?.SetPlayerInputEnabled(true);

        while (Vector3.Distance(playerObject.transform.position, targetPosition) > tolerance)
        {
            yield return null;
        }

        playerController?.SetPlayerInputEnabled(false);
        Debug.Log($"玩家已到达: {targetPosition}");
    }

    protected virtual void FinishFlow()
    {
        Debug.Log($"流程结束: {GetType().Name}");
        _isRunning = false;
        _currentCoroutine = null;
        OnFlowEnd?.Invoke();
    }

    protected FlowRunner GetSafeRunner()
    {
        if (_safeRunner == null)
        {
            var runner = GetCoroutineRunner();
            if (runner != null)
                _safeRunner = new FlowRunner(runner);
        }
        return _safeRunner;
    }

    protected IEnumerator SafeWait(IEnumerator routine)
    {
        if (_allowSafeStop && _safeRunner != null)
        {
            return GetSafeRunner().RunSafe(routine);
        }
        return routine;
    }
}