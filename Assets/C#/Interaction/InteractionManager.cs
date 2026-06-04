using UnityEngine;

/// <summary>
/// 交互管理器
/// 负责监听玩家按键，触发与可交互物的交互
/// 挂在场景中的 InteractionManager 空物体上
/// </summary>
public class InteractionManager : MonoBehaviour
{
    #region 单例
    private static InteractionManager _instance;
    public static InteractionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InteractionManager>();
            }
            return _instance;
        }
    }
    #endregion

    #region Inspector引用

    [Header("=== 引用 ===")]
    [Tooltip("玩家身上的 PlayerInteractionDetector 组件")]
    public PlayerInteractionDetector detector;

    [Header("=== 交互提示UI ===")]
    [Tooltip("交互提示UI（屏幕中央的提示文字）")]
    public GameObject interactionPromptUI;

    [Tooltip("交互提示文字组件（TextMeshProUGUI 或 Text）")]
    public UnityEngine.UI.Text promptText;

    [Header("=== 设置 ===")]
    [Tooltip("交互提示显示距离（超过此距离隐藏提示）")]
    public float promptShowDistance = 5f;

    #endregion

    #region 常量

    /// <summary>
    /// 固定交互提示文字
    /// 具体任务内容由左侧 TaskGuidePanel 显示
    /// </summary>
    private const string INTERACT_PROMPT_TEXT = "按E交互";

    #endregion

    #region 私有字段

    private bool _isInteractionEnabled = true;
    private Interactables _lastInteractable;

    #endregion

    #region Unity生命周期

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始化时隐藏提示
        HidePrompt();
    }

    private void Start()
    {
        // 如果 Inspector 没绑定，尝试自动查找
        if (detector == null)
        {
            // 尝试从 Player 标签查找
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                detector = player.GetComponent<PlayerInteractionDetector>();
            }

            if (detector == null)
            {
                Debug.LogWarning("[InteractionManager] 未找到 PlayerInteractionDetector，请检查配置或在Inspector中手动绑定");
            }
        }
    }

    private void Update()
    {
        if (!_isInteractionEnabled) return;

        // 检测并更新交互提示
        UpdateInteractionPrompt();

        // 监听E键
        HandleInteractionInput();
    }

    #endregion

    #region 交互逻辑

    /// <summary>
    /// 处理交互输入（E键）
    /// </summary>
    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    /// <summary>
    /// 尝试交互
    /// </summary>
    private void TryInteract()
    {
        if (detector == null || detector.currentInteractable == null)
        {
            Debug.Log("[交互] 前方没有可交互物体");
            return;
        }

        Interactables target = detector.currentInteractable;

        // 检查当前任务是否需要与该人物交互
        if (!IsValidTargetForCurrentStep(target))
        {
            Debug.Log($"[交互] 当前任务不需要与 {target.npcName} 交互");
            // 可以显示提示信息给玩家
            return;
        }

        Debug.Log($"[交互] 与 {target.npcName} 交互，执行 {target.actionType}");

        // 调用交互物的 OnInteract 方法
        target.OnInteract();
    }

    /// <summary>
    /// 检查是否是当前任务的目标人物
    /// </summary>
    private bool IsValidTargetForCurrentStep(Interactables target)
    {
        // 获取当前流程
        FlowBase currentFlow = FlowTaskIntegration.Instance?.GetCurrentFlow();
        
        // 如果没有当前流程，允许交互（比如游戏刚开始）
        if (currentFlow == null) return true;
        
        // 检查当前步骤的目标NPC（支持多种流程类型）
        string targetNPC = null;
        
        // 优先检查分支流程（如果主流程正在运行分支）
        if (currentFlow is StandardSalesFlow standardFlow)
        {
            // 检查是否在分支流程中
            FlowBase branchFlow = GetCurrentBranchFlow(standardFlow);
            if (branchFlow != null)
            {
                // 使用分支流程的步骤
                targetNPC = GetTargetNPCFromFlow(branchFlow);
            }
            else
            {
                // 使用主流程的步骤
                var currentStep = standardFlow.GetCurrentStep();
                if (currentStep != null) targetNPC = currentStep.targetNPC;
            }
        }
        else if (currentFlow is CustomSalesFlow customFlow)
        {
            // 检查是否在分支流程中
            FlowBase branchFlow = GetCurrentBranchFlow(customFlow);
            if (branchFlow != null)
            {
                targetNPC = GetTargetNPCFromFlow(branchFlow);
            }
            else
            {
                var currentStep = customFlow.GetCurrentStep();
                if (currentStep != null) targetNPC = currentStep.targetNPC;
            }
        }
        else
        {
            // 直接获取当前流程的目标NPC
            targetNPC = GetTargetNPCFromFlow(currentFlow);
        }
        
        if (!string.IsNullOrEmpty(targetNPC))
        {
            // 检查 NPC 名称是否匹配（忽略大小写）
            if (!string.IsNullOrEmpty(target.npcName) &&
                !targetNPC.Equals(target.npcName, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"[交互] 目标不匹配：当前需要找 {targetNPC}，但你面对的是 {target.npcName}");
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 从流程获取目标NPC
    /// </summary>
    private string GetTargetNPCFromFlow(FlowBase flow)
    {
        if (flow is CustomSalesFlow customFlow)
        {
            var currentStep = customFlow.GetCurrentStep();
            return currentStep?.targetNPC;
        }
        else if (flow is CustomProductionFlow customProdFlow)
        {
            var currentStep = customProdFlow.GetCurrentStep();
            return currentStep?.targetNPC;
        }
        else if (flow is StandardDeliveryFlow deliveryFlow)
        {
            var currentStep = deliveryFlow.GetCurrentStep();
            return currentStep?.targetNPC;
        }
        else if (flow is StandardSalesBranchFlow salesBranchFlow)
        {
            var currentStep = salesBranchFlow.GetCurrentStep();
            return currentStep?.targetNPC;
        }
        else if (flow is StandardSalesFlow mainFlow)
        {
            var currentStep = mainFlow.GetCurrentStep();
            return currentStep?.targetNPC;
        }
        
        return null;
    }
    
    /// <summary>
    /// 获取 StandardSalesFlow 当前运行的分支流程
    /// </summary>
    private FlowBase GetCurrentBranchFlow(StandardSalesFlow mainFlow)
    {
        System.Reflection.FieldInfo branchFlowField = typeof(StandardSalesFlow).GetField("_currentBranchFlow",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (branchFlowField != null)
        {
            return branchFlowField.GetValue(mainFlow) as FlowBase;
        }

        return null;
    }

    /// <summary>
    /// 获取 CustomSalesFlow 当前运行的分支流程
    /// </summary>
    private FlowBase GetCurrentBranchFlow(CustomSalesFlow mainFlow)
    {
        System.Reflection.FieldInfo branchFlowField = typeof(CustomSalesFlow).GetField("_currentBranchFlow",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (branchFlowField != null)
        {
            return branchFlowField.GetValue(mainFlow) as FlowBase;
        }

        return null;
    }

    #endregion

    #region 提示UI

    /// <summary>
    /// 更新交互提示UI
    /// </summary>
    private void UpdateInteractionPrompt()
    {
        if (detector == null || detector.currentInteractable == null)
        {
            HidePrompt();
            return;
        }

        Interactables current = detector.currentInteractable;

        // 如果和上一次是同一个物体，不需要更新
        if (current == _lastInteractable) return;

        _lastInteractable = current;

        // 显示提示
        ShowPrompt(current.interactText);
    }

    /// <summary>
    /// 显示交互提示
    /// 固定显示"按E交互"，具体任务由左侧TaskGuidePanel负责
    /// </summary>
    public void ShowPrompt(string text = "")
    {
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(true);
        }

        if (promptText != null)
        {
            // 固定显示"按E交互"
            promptText.text = INTERACT_PROMPT_TEXT;
        }
    }

    /// <summary>
    /// 隐藏交互提示
    /// </summary>
    public void HidePrompt()
    {
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }

        _lastInteractable = null;
    }

    #endregion

    #region 控制方法（供其他脚本调用）

    /// <summary>
    /// 启用/禁用交互
    /// </summary>
    public void SetInteractionEnabled(bool enabled)
    {
        _isInteractionEnabled = enabled;

        if (!enabled)
        {
            HidePrompt();
        }
    }

    /// <summary>
    /// 获取当前可交互物
    /// </summary>
    public Interactables GetCurrentInteractable()
    {
        return detector?.currentInteractable;
    }

    #endregion
}
