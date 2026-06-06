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

    /// <summary>
    /// 打开单据UI并等待用户操作完成
    /// 如果单据系统未配置（无Config或无Panel），自动回退到按E完成模式
    /// </summary>
    protected IEnumerator WaitForBillComplete(UIManager.UIType billType, string roleName)
    {
        Debug.Log($"[FlowBase] OPEN BILL: {billType}, 角色: {roleName}");

        // 1. 获取单据配置
        var config = BillButtonConfigLoader.GetConfig(billType);
        if (config == null)
        {
            Debug.LogError($"[FlowBase] FAIL: 未找到 BillButtonConfig: {billType}");
            yield break;
        }
        Debug.Log($"[FlowBase] OK: 配置已加载 [{config.billName}]");

        // 2. 根据角色获取可见按钮
        var buttons = config.GetButtonsForRole(roleName);
        Debug.Log($"[FlowBase] 可见按钮: {string.Join(", ", buttons)}");

        // 3. 打开UI面板
        UIManager.Instance.ShowUI(billType);

        // 4. 找到面板并配置按钮
        if (!UIManager.Instance.TryGetUI(billType, out GameObject panel) || panel == null)
        {
            Debug.LogError($"[FlowBase] FAIL: 面板未注册: {billType} (检查UIManager.uiEntries)");
            UIManager.Instance.HideUI(billType);
            yield break;
        }
        Debug.Log($"[FlowBase] OK: 面板={panel.name}, active={panel.activeSelf}");

        // 尝试获取管理器组件
        var uiForm = panel.GetComponent<UIFormBase>();
        var outboundMgr = panel.GetComponent<FinishedProductOutboundManager>();
        var quoteMgr = panel.GetComponent<QuoteFormManager>();

        if (uiForm != null)
        {
            Debug.Log($"[FlowBase] OK: UIFormBase");
            uiForm.ConfigureButtons(buttons);
            uiForm.ShowUIMode(UIFormMode.Fill);
        }
        else if (outboundMgr != null)
        {
            Debug.Log($"[FlowBase] OK: FinishedProductOutboundManager");
            outboundMgr.ConfigureButtons(buttons);
        }
        else if (quoteMgr != null)
        {
            Debug.Log($"[FlowBase] OK: QuoteFormManager");
            quoteMgr.ConfigureButtons(buttons);
        }
        else
        {
            Debug.LogError($"[FlowBase] FAIL: 面板上没有任何按钮管理组件!");
            UIManager.Instance.HideUI(billType);
            yield break;
        }

        // 4.5 解锁鼠标 + 禁用玩家移动，让玩家能点击单据按钮
        ShowCursor();
        DisablePlayerInput();

        // 5. 等待用户操作完成（点击提交/审核/退出等按钮）
        yield return new WaitUntil(() =>
            panel == null || !panel.activeSelf ||
            (uiForm != null && uiForm.IsCompleted) ||
            (outboundMgr != null && outboundMgr.IsCompleted) ||
            (quoteMgr != null && quoteMgr.IsCompleted));

        // 6. 关闭UI、恢复鼠标和移动、标记步骤完成
        UIManager.Instance.HideUI(billType);
        HideCursor();
        EnablePlayerInput();
        MarkStepComplete();
        Debug.Log($"[FlowBase] 单据操作完成: {billType}");
    }

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void DisablePlayerInput()
    {
        var player = ObjectManager.Instance?.GetObject(ObjectManager.ObjectType.Player);
        player?.GetComponent<SimpleFirstPersonController>()?.SetPlayerInputEnabled(false);
    }

    private void EnablePlayerInput()
    {
        var player = ObjectManager.Instance?.GetObject(ObjectManager.ObjectType.Player);
        player?.GetComponent<SimpleFirstPersonController>()?.SetPlayerInputEnabled(true);
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