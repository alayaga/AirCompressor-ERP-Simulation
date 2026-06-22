using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 定制产品销售流程
/// 销售阶段（客户询单>BOM>报价>提交PMC），完成后进入生产/采购分支选择
/// </summary>
public class CustomSalesFlow : FlowBase
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
        public UIManager.UIType? billType;
        public bool isBranchChoice = false;

        public StepData(string name, string desc, string npc, string location, Interactables.ActionType action, UIManager.UIType? billType = null, bool isBranch = false)
        {
            stepName = name;
            description = desc;
            targetNPC = npc;
            targetLocation = location;
            actionType = action;
            this.billType = billType;
            isBranchChoice = isBranch;
        }
    }

    // 流程信息
    private const string FLOW_NAME = "定制产品销售流程";
    private const string TASK_TITLE = "定制产品订单处理";
    private const string TASK_DESCRIPTION = "完成从客户询单到提交PMC的完整销售流程";

    // 步骤队列
    private Queue<StepData> _steps = new Queue<StepData>();
    private StepData _currentStep;
    private bool _isStepCompleted = false;
    private int _totalSteps;
    private int _currentStepIndex = 0;

    // 分支流程状态
    private bool _productionBranchCompleted = false;
    private bool _purchaseBranchCompleted = false;
    private bool _productionBranchSelected = false;
    private bool _purchaseBranchSelected = false;
    private bool _isInBranch = false;
    private FlowBase _currentBranchFlow = null;

    protected override IEnumerator FlowCoroutine()
    {
        Debug.Log("[CustomSalesFlow] 开始定制产品销售流程");

        // 初始化步骤队列
        InitializeSteps();
        _totalSteps = _steps.Count;

        // 显示流程信息到UI
        ShowTaskInfoToUI();

        // 执行所有步骤
        while (_steps.Count > 0)
        {
            _currentStep = _steps.Dequeue();
            _isStepCompleted = false;
            _currentStepIndex++;

            Debug.Log($"[步骤 {_currentStepIndex}/{_totalSteps}] {_currentStep.stepName}");

            // 显示当前步骤到UI
            ShowCurrentStepToUI();

            // 如果是分支选择步骤，循环供玩家反复选择
            if (_currentStep.isBranchChoice)
            {
                while (true)
                {
                    Debug.Log("[CustomSalesFlow] 等待玩家选择分支（[1]生产流程 [2]采购流程）");
                    ShowCurrentStepToUI();

                    _isStepCompleted = false;
                    while (!_isStepCompleted)
                    {
                        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                            SelectBranch(1);
                        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                            SelectBranch(2);

                        yield return null;
                    }

                    if (_productionBranchSelected)
                    {
                        Debug.Log("[CustomSalesFlow] 启动生产流程分支");
                        yield return StartBranchFlow<CustomProductionFlow>();
                        _productionBranchSelected = false;
                    }
                    else if (_purchaseBranchSelected)
                    {
                        Debug.Log("[CustomSalesFlow] 启动采购流程分支");
                        yield return StartBranchFlow<CustomPurchaseFlow>();
                        _purchaseBranchSelected = false;
                    }
                }
            }
            else
            {
                // 先等待玩家交互（系统步骤自动完成）
                if (_currentStep.targetNPC == "系统")
                {
                    Debug.Log($"[CustomSalesFlow] 系统步骤自动完成: {_currentStep.stepName}");
                    yield return new WaitForSeconds(1.5f);
                    _isStepCompleted = true;
                }
                else
                {
                    // 等待玩家走到NPC前按E
                    yield return new WaitUntil(() => _isStepCompleted);
                }

                // 交互完成后，如果步骤关联了单据，再打开单据面板
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
        Debug.Log("[CustomSalesFlow] 定制产品销售流程完成！");
    }

    /// <summary>
    /// 初始化所有步骤
    /// </summary>
    private void InitializeSteps()
    {
        _steps.Clear();

        // ===== 阶段1：客户询单 =====
        _steps.Enqueue(new StepData("客户询单", "客户咨询定制产品需求", "销售员", "销售办公室", Interactables.ActionType.Fill));

        // ===== 阶段2：销售订单处理 =====
        _steps.Enqueue(new StepData("填写销售订单", "录入客户需求和产品规格", "销售员", "销售办公室", Interactables.ActionType.Fill, UIManager.UIType.SalesOrder));
        _steps.Enqueue(new StepData("审核销售订单", "销售总监审核订单内容", "销售总监", "销售办公室", Interactables.ActionType.Approve, UIManager.UIType.SalesOrder));

        // ===== 阶段3：BOM单处理 =====
        _steps.Enqueue(new StepData("填写BOM单", "技术部根据订单生成物料清单", "技术员", "计划物控中心", Interactables.ActionType.Fill, UIManager.UIType.ProductionBOM));

        // ===== 阶段4：财务报价 =====
        _steps.Enqueue(new StepData("查看BOM单", "财务部查看物料清单", "财务主管", "财务部", Interactables.ActionType.View, UIManager.UIType.ProductionBOM));
        _steps.Enqueue(new StepData("计算成本并确认价格", "填写销售报价单", "财务主管", "财务部", Interactables.ActionType.Fill, UIManager.UIType.SalesQuotation));

        // ===== 阶段5：报价确认 =====
        _steps.Enqueue(new StepData("查看报价单", "销售员查看财务核算的报价", "销售员", "销售办公室", Interactables.ActionType.View, UIManager.UIType.SalesQuotation));
        _steps.Enqueue(new StepData("报价给客户", "将报价单发送给客户", "销售员", "销售办公室", Interactables.ActionType.Fill, UIManager.UIType.SalesQuotation));
        _steps.Enqueue(new StepData("客户确认", "客户确认报价并签订合同", "销售员", "销售办公室", Interactables.ActionType.View));

        // ===== 阶段6：提交PMC =====
        _steps.Enqueue(new StepData("提交销售订单给PMC", "PMC可查看销售订单", "销售员", "计划物控中心", Interactables.ActionType.Fill, UIManager.UIType.SalesOrder));

        // ===== 阶段7：分支选择（生产/采购）=====
        _steps.Enqueue(new StepData(
            "选择后续流程",
            "请选择要继续的流程分支\n[1]生产流程 [2]采购流程",
            "系统",
            "主界面",
            Interactables.ActionType.View,
            isBranch: true
        ));
    }

    /// <summary>
    /// 启动分支流程
    /// </summary>
    private IEnumerator StartBranchFlow<T>() where T : FlowBase, new()
    {
        Debug.Log($"[CustomSalesFlow] 启动分支流程: {typeof(T).Name}");

        _isInBranch = true;
        _currentBranchFlow = new T();

        if (FlowManager.Instance != null)
        {
            FlowManager.Instance.RegisterFlow(_currentBranchFlow);
        }

        _currentBranchFlow.StartFlow();

        // 等待分支流程完成
        yield return new WaitUntil(() => !_currentBranchFlow.IsRunning);

        // 标记分支完成
        if (typeof(T) == typeof(CustomProductionFlow))
        {
            _productionBranchCompleted = true;
        }

        _isInBranch = false;
        _currentBranchFlow = null;

        Debug.Log($"[CustomSalesFlow] 分支流程完成: {typeof(T).Name}");
    }

    /// <summary>
    /// 处理分支选择输入
    /// </summary>
    public void SelectBranch(int branchIndex)
    {
        if (!_isStepCompleted && _currentStep != null && _currentStep.isBranchChoice)
        {
            if (branchIndex == 1 && !_productionBranchSelected)
            {
                Debug.Log("[CustomSalesFlow] 选择生产流程分支");
                _productionBranchSelected = true;
                _isStepCompleted = true;
            }
            else if (branchIndex == 2 && !_purchaseBranchSelected)
            {
                Debug.Log("[CustomSalesFlow] 选择采购流程分支");
                _purchaseBranchSelected = true;
                _isStepCompleted = true;
            }
            else
            {
                Debug.LogWarning("[CustomSalesFlow] 无效的分支选择或该分支已完成");
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
            string description = _currentStep.description;
            if (_currentStep.isBranchChoice)
            {
                // 动态构建可用分支列表
                List<string> availableBranches = new List<string>();
                if (!_productionBranchCompleted) availableBranches.Add("[1]生产流程");
                if (!_purchaseBranchCompleted) availableBranches.Add("[2]采购流程");

                if (availableBranches.Count == 0)
                {
                    description = "所有分支都已完成";
                }
                else
                {
                    description = "请选择要继续的流程分支\n" + string.Join(" ", availableBranches);
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
            default: return "操作";
        }
    }

    #endregion

    #region 公共方法

    public override void MarkStepComplete()
    {
        Debug.Log($"[CustomSalesFlow] MarkStepComplete 被调用！当前步骤：{_currentStep?.stepName ?? "null"}");

        // 如果在分支流程中，传递完成信号给分支流程
        if (_isInBranch && _currentBranchFlow != null)
        {
            _currentBranchFlow.MarkStepComplete();
            return;
        }

        // 防止在分支选择步骤被意外调用
        if (_currentStep != null && _currentStep.isBranchChoice)
        {
            Debug.LogWarning("[CustomSalesFlow] 分支选择步骤不能通过 MarkStepComplete() 完成，必须通过 SelectBranch() 选择！");
            return;
        }

        _isStepCompleted = true;
    }

    public StepData GetCurrentStep()
    {
        return _currentStep;
    }

    public override string GetCurrentStepName() => _currentStep?.stepName;

    public int GetTotalSteps()
    {
        return _totalSteps;
    }

    public int GetCurrentStepIndex()
    {
        return _currentStepIndex;
    }

    public bool IsProductionBranchCompleted() => _productionBranchCompleted;
    public bool IsPurchaseBranchCompleted() => _purchaseBranchCompleted;
    public bool IsInBranch() => _isInBranch;

    #endregion
}
