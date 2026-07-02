using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 标准销售流程（合并销售流程和发货流程）
/// 支持分支选择：玩家可以先选择完成销售流程或发货流程
/// </summary>
public class StandardSalesFlow : FlowBase
{
    /// <summary>
    /// 步骤信息结构
    /// </summary>
    [System.Serializable]
    public class StepData
    {
        public string stepName;
        public string description;
        public string targetNPC;
        public string targetLocation;
        public Interactables.ActionType actionType;
        public bool isBranchChoice = false; // 是否为分支选择步骤
        public UIManager.UIType? billType;  // 关联单据类型
        public DialogueConfig dialogueConfig; // 对话配置

        public StepData(string name, string desc, string npc, string location, Interactables.ActionType action, bool isBranch = false, UIManager.UIType? billType = null, DialogueConfig dialogueConfig = default)
        {
            stepName = name;
            description = desc;
            targetNPC = npc;
            targetLocation = location;
            actionType = action;
            isBranchChoice = isBranch;
            this.billType = billType;
            this.dialogueConfig = dialogueConfig;
        }
    }
    
    // 流程信息
    private const string FLOW_NAME = "标准产品主流程";
    private const string TASK_TITLE = "标准流程（销售+发货）";
    private const string TASK_DESCRIPTION = "完成销售流程和发货流程，可自由选择完成顺序";
    
    // 步骤队列（主流程步骤）
    private Queue<StepData> _mainSteps = new Queue<StepData>();
    private StepData _currentStep;
    private bool _isStepCompleted = false;
    private int _totalSteps;
    private int _currentStepIndex = 0;
    
    // 分支流程状态
    private bool _salesBranchCompleted = false;
    private bool _deliveryBranchCompleted = false;
    private bool _productionBranchCompleted = false;  // 生产流程分支
    private bool _purchaseBranchCompleted = false;    // 采购流程分支
    private bool _productionDeptPurchaseBranchCompleted = false;  // 生产部门提交采购流程分支
    private bool _salesBranchSelected = false;   // 待执行标记
    private bool _deliveryBranchSelected = false; // 待执行标记
    private bool _productionBranchSelected = false;   // 生产流程待执行
    private bool _purchaseBranchSelected = false;     // 采购流程待执行
    private bool _productionDeptPurchaseBranchSelected = false; // 生产采购流程待执行
    private bool _isInBranch = false;
    private FlowBase _currentBranchFlow = null;

    protected override IEnumerator FlowCoroutine()
    {
        Debug.Log("[StandardSalesFlow] 开始标准产品销售流程");

        // 初始化主步骤队列
        InitializeMainSteps();
        _totalSteps = _mainSteps.Count;

        // 显示流程信息到UI
        ShowTaskInfoToUI();

        // 执行主流程步骤
        while (_mainSteps.Count > 0)
        {
            // 取出一个步骤
            _currentStep = _mainSteps.Dequeue();
            _isStepCompleted = false;
            _currentStepIndex++;

            Debug.Log($"[步骤 {_currentStepIndex}/{_totalSteps}] {_currentStep.stepName}");

            // 显示当前步骤到 UI
            ShowCurrentStepToUI();

            // 如果是分支选择步骤，循环供玩家反复选择
            if (_currentStep.isBranchChoice)
            {
                while (true)
                {
                    Debug.Log("[StandardSalesFlow] 等待玩家选择（[1]销售-PMC [2]销售-发货 [3]生产 [4]采购 [5]生产采购）");
                    ShowCurrentStepToUI();

                    _isStepCompleted = false;
                    while (!_isStepCompleted)
                    {
                        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                            SelectBranch(1);
                        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                            SelectBranch(2);
                        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                            SelectBranch(3);
                        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
                            SelectBranch(4);
                        else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
                            SelectBranch(5);

                        yield return null;
                    }

                    if (_salesBranchSelected)
                    {
                        yield return StartBranchFlow<StandardSalesBranchFlow>();
                        _salesBranchSelected = false;
                    }
                    else if (_deliveryBranchSelected)
                    {
                        yield return StartBranchFlow<StandardDeliveryFlow>();
                        _deliveryBranchSelected = false;
                    }
                    else if (_productionBranchSelected)
                    {
                        yield return StartBranchFlow<StandardProductionFlow>();
                        _productionBranchSelected = false;
                    }
                    else if (_purchaseBranchSelected)
                    {
                        yield return StartBranchFlow<StandardPurchaseFlow>();
                        _purchaseBranchSelected = false;
                    }
                    else if (_productionDeptPurchaseBranchSelected)
                    {
                        yield return StartBranchFlow<ProductionDeptPurchaseFlow>();
                        _productionDeptPurchaseBranchSelected = false;
                    }
                }
            }
            else
            {
                // 系统步骤自动完成，普通步骤等待E交互
                if (_currentStep.targetNPC == "系统")
                {
                    Debug.Log($"[StandardSalesFlow] 系统步骤自动完成: {_currentStep.stepName}");
                    yield return new WaitForSeconds(1.5f);
                    _isStepCompleted = true;
                }
                else
                {
                    // 等待玩家走到NPC前按E
                    yield return new WaitUntil(() => _isStepCompleted);
                }

                // 交互完成后，打开单据（支持退出重新进入）
                if (_currentStep.billType != null)
                {
                    bool stepDone = false;
                    while (!stepDone)
                    {
                        _isStepCompleted = false;
                        yield return WaitForBillComplete(_currentStep.billType.Value, _currentStep.targetNPC, _currentStep.actionType);
                        if (_isStepCompleted)
                            stepDone = true;
                        else
                            yield return new WaitUntil(() => _isStepCompleted);
                    }
                }
            }

            Debug.Log($"[完成] {_currentStep.stepName}");

            // 更新进度到UI
            UpdateProgressToUI();

            yield return new WaitForSeconds(0.5f);
        }

        // 显示流程完成
        ShowTaskCompleteToUI();
        Debug.Log("[StandardMainFlow] 标准产品销售流程完成！");
    }

    /// <summary>
    /// 初始化主流程步骤
    /// </summary>
    private void InitializeMainSteps()
    {
        _mainSteps.Clear();

        // 主流程开始（系统步骤，3秒后自动进入分支选择）
        _mainSteps.Enqueue(new StepData("流程开始", "欢迎进入标准产品流程（此步骤自动进行）", "系统", "主界面", Interactables.ActionType.View));

        // 分支选择步骤1（首次选择）
        _mainSteps.Enqueue(new StepData("选择分支", "请选择要完成的流程分支\n[1]销售-PMC [2]销售-发货 [3]生产流程 [4]采购流程 [5]生产采购", "系统", "主界面", Interactables.ActionType.View, true));

        // 分支选择步骤2（继续选择）
        _mainSteps.Enqueue(new StepData("继续分支", "请选择下一个分支流程\n[1]销售-PMC [2]销售-发货 [3]生产流程 [4]采购流程 [5]生产采购", "系统", "主界面", Interactables.ActionType.View, true));

        // 分支选择步骤3（继续选择）
        _mainSteps.Enqueue(new StepData("继续分支", "请选择下一个分支流程\n[1]销售-PMC [2]销售-发货 [3]生产流程 [4]采购流程 [5]生产采购", "系统", "主界面", Interactables.ActionType.View, true));

        // 分支选择步骤4（继续选择）
        _mainSteps.Enqueue(new StepData("继续分支", "请选择下一个分支流程\n[1]销售-PMC [2]销售-发货 [3]生产流程 [4]采购流程 [5]生产采购", "系统", "主界面", Interactables.ActionType.View, true));

        // 分支选择步骤5（最后一个分支）
        _mainSteps.Enqueue(new StepData("继续分支", "请选择最后一个分支流程\n[1]销售-PMC [2]销售-发货 [3]生产流程 [4]采购流程 [5]生产采购", "系统", "主界面", Interactables.ActionType.View, true));

        // 流程结束
        _mainSteps.Enqueue(new StepData("流程结束", "所有流程已完成，任务圆满结束！（此步骤自动进行）", "系统", "主界面", Interactables.ActionType.View));
    }

    /// <summary>
    /// 启动分支流程
    /// </summary>
    private IEnumerator StartBranchFlow<T>() where T : FlowBase, new()
    {
        Debug.Log($"[StandardSalesFlow] 启动分支流程: {typeof(T).Name}");
        
        _isInBranch = true;
        _currentBranchFlow = new T();
        
        // 注册并启动分支流程
        if (FlowManager.Instance != null)
        {
            FlowManager.Instance.RegisterFlow(_currentBranchFlow);
        }
        
        // 启动分支流程
        _currentBranchFlow.StartFlow();
        
        // 等待分支流程完成
        yield return new WaitUntil(() => !_currentBranchFlow.IsRunning);
        
        // 标记分支完成
        if (typeof(T) == typeof(StandardSalesBranchFlow))
        {
            _salesBranchCompleted = true;
        }
        else if (typeof(T) == typeof(StandardDeliveryFlow))
        {
            _deliveryBranchCompleted = true;
        }
        else if (typeof(T) == typeof(StandardProductionFlow))
        {
            _productionBranchCompleted = true;
        }
        else if (typeof(T) == typeof(StandardPurchaseFlow))
        {
            _purchaseBranchCompleted = true;
        }
        else if (typeof(T) == typeof(ProductionDeptPurchaseFlow))
        {
            _productionDeptPurchaseBranchCompleted = true;
        }
        
        _isInBranch = false;
        _currentBranchFlow = null;
        
        Debug.Log($"[StandardSalesFlow] 分支流程完成: {typeof(T).Name}");
    }

    /// <summary>
    /// 处理分支选择输入
    /// </summary>
    public void SelectBranch(int branchIndex)
    {
        if (!_isStepCompleted && _currentStep != null && _currentStep.isBranchChoice)
        {
            if (branchIndex == 1 && !_salesBranchSelected)
            {
                Debug.Log("[StandardSalesFlow] 选择销售-PMC流程分支");
                _salesBranchSelected = true;  // 标记待执行
                _isStepCompleted = true;
            }
            else if (branchIndex == 2 && !_deliveryBranchSelected)
            {
                Debug.Log("[StandardSalesFlow] 选择销售-发货流程分支");
                _deliveryBranchSelected = true;  // 标记待执行
                _isStepCompleted = true;
            }
            else if (branchIndex == 3 && !_productionBranchSelected)
            {
                Debug.Log("[StandardSalesFlow] 选择生产流程分支");
                _productionBranchSelected = true;  // 标记待执行
                _isStepCompleted = true;
            }
            else if (branchIndex == 4 && !_purchaseBranchSelected)
            {
                Debug.Log("[StandardSalesFlow] 选择采购流程分支");
                _purchaseBranchSelected = true;  // 标记待执行
                _isStepCompleted = true;
            }
            else if (branchIndex == 5 && !_productionDeptPurchaseBranchSelected)
            {
                Debug.Log("[StandardSalesFlow] 选择生产部门提交采购流程分支");
                _productionDeptPurchaseBranchSelected = true;  // 标记待执行
                _isStepCompleted = true;
            }
            else
            {
                Debug.LogWarning("[StandardSalesFlow] 无效的分支选择或该分支已完成");
            }
        }
    }

    #region UI更新方法

    private void ShowTaskInfoToUI()
    {
        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.UpdateTaskInfo(FLOW_NAME, TASK_TITLE, TASK_DESCRIPTION, _currentStepIndex, _totalSteps);
        }
    }

    private void ShowCurrentStepToUI()
    {
        if (TaskGuidePanelNew.Instance != null && _currentStep != null)
        {
            // 如果是分支选择步骤，动态更新描述
            string description = _currentStep.description;
            if (_currentStep.isBranchChoice)
            {
                // 构建可用分支列表
                List<string> availableBranches = new List<string>();
                if (!_salesBranchCompleted) availableBranches.Add("[1]销售-PMC");
                if (!_deliveryBranchCompleted) availableBranches.Add("[2]销售-发货");
                if (!_productionBranchCompleted) availableBranches.Add("[3]生产流程");
                if (!_purchaseBranchCompleted) availableBranches.Add("[4]采购流程");
                if (!_productionDeptPurchaseBranchCompleted) availableBranches.Add("[5]生产采购");
                
                if (availableBranches.Count == 0)
                {
                    description = "所有分支都已完成，继续主流程";
                }
                else
                {
                    description = "请选择要完成的流程分支\n" + string.Join(" ", availableBranches);
                }
            }
            
            string actionText = GetActionText(_currentStep.actionType);
            TaskGuidePanelNew.Instance.UpdateCurrentStep(
                _currentStep.stepName,
                description,
                _currentStep.targetNPC,
                _currentStep.targetLocation,
                actionText
            );
        }
    }

    private void UpdateProgressToUI()
    {
        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.UpdateProgress(_currentStepIndex, _totalSteps);
        }
    }

    private void ShowTaskCompleteToUI()
    {
        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.CompleteTask();
        }
    }

    private string GetActionText(Interactables.ActionType action)
    {
        switch (action)
        {
            case Interactables.ActionType.Fill: return "填写";
            case Interactables.ActionType.Approve: return "审核";
            case Interactables.ActionType.PushDown: return "下推";
            case Interactables.ActionType.View: return "查看";
            case Interactables.ActionType.Pick: return "领取";
            case Interactables.ActionType.Deliver: return "交付";
            case Interactables.ActionType.Ship: return "发货";
            case Interactables.ActionType.Sign: return "签字";
            default: return "操作";
        }
    }

    #endregion

    #region 公共方法

    public override void MarkStepComplete()
    {
        Debug.Log($"[StandardMainFlow] MarkStepComplete 被调用！当前步骤：{_currentStep?.stepName ?? "null"}");
        
        // 如果在分支流程中，传递完成信号给分支流程
        if (_isInBranch && _currentBranchFlow != null)
        {
            _currentBranchFlow.MarkStepComplete();
            return;
        }
        
        // 防止在分支选择步骤被意外调用（绕过分支选择逻辑）
        if (_currentStep != null && _currentStep.isBranchChoice)
        {
            Debug.LogWarning("[StandardMainFlow] 分支选择步骤不能通过 MarkStepComplete() 完成，必须通过 SelectBranch() 选择！");
            Debug.LogWarning($"[StandardMainFlow] 调用堆栈：{System.Environment.StackTrace}");
            return; // 直接返回，不设置 _isStepCompleted
        }
        
        _isStepCompleted = true;
    }

    public override DialogueConfig GetCurrentStepDialogueConfig()
    {
        // 如果在分支流程中，转发给分支流程
        if (_isInBranch && _currentBranchFlow != null)
            return _currentBranchFlow.GetCurrentStepDialogueConfig();
        return _currentStep?.dialogueConfig ?? DialogueConfig.None;
    }

    public StepData GetCurrentStep()
    {
        return _currentStep;
    }

    public int GetTotalSteps()
    {
        return _totalSteps;
    }

    public int GetCurrentStepIndex()
    {
        return _currentStepIndex;
    }

    public bool IsSalesBranchCompleted() => _salesBranchCompleted;
    public bool IsDeliveryBranchCompleted() => _deliveryBranchCompleted;
    public bool IsProductionDeptPurchaseBranchCompleted() => _productionDeptPurchaseBranchCompleted;
    public bool IsInBranch() => _isInBranch;

    #endregion
}
