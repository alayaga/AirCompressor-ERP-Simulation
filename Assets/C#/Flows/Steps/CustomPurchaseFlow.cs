using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 定制产品采购流程（共13步）
/// 从PMC查看销售订单到采购入库的完整流程
/// 步骤4~13与标准产品原料采购流程相同
/// </summary>
public class CustomPurchaseFlow : FlowBase
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

        public StepData(string name, string desc, string npc, string location, Interactables.ActionType action, UIManager.UIType? billType = null)
        {
            stepName = name;
            description = desc;
            targetNPC = npc;
            targetLocation = location;
            actionType = action;
            this.billType = billType;
        }
    }

    // 流程信息
    private const string FLOW_NAME = "定制产品采购流程";
    private const string TASK_TITLE = "采购流程";
    private const string TASK_DESCRIPTION = "完成从PMC采购申请到采购入库的完整流程";

    // 步骤队列
    private Queue<StepData> _steps = new Queue<StepData>();
    private StepData _currentStep;
    private bool _isStepCompleted = false;
    private int _totalSteps;
    private int _currentStepIndex = 0;

    protected override IEnumerator FlowCoroutine()
    {
        Debug.Log("[CustomPurchaseFlow] 开始定制产品采购流程");

        InitializeSteps();
        _totalSteps = _steps.Count;
        ShowTaskInfoToUI();

        while (_steps.Count > 0)
        {
            _currentStep = _steps.Dequeue();
            _isStepCompleted = false;
            _currentStepIndex++;
            Debug.Log($"[步骤 {_currentStepIndex}/{_totalSteps}] {_currentStep.stepName}");
            ShowCurrentStepToUI();

            Debug.Log($"[FLOW STEP] {_currentStep.stepName} | billType={_currentStep.billType} | NPC={_currentStep.targetNPC}");
            bool isAutoStep = _currentStep.targetNPC == "供应商" || _currentStep.targetNPC == "系统";
            if (_currentStep.billType != null)
            {
                yield return WaitForBillComplete(_currentStep.billType.Value, _currentStep.targetNPC, _currentStep.actionType);

                if (!_isStepCompleted) yield return new WaitUntil(() => _isStepCompleted);
            }
            else if (isAutoStep)
            {
                Debug.Log($"[CustomPurchaseFlow] 自动步骤：{_currentStep.stepName}，等待5秒后自动完成");
                yield return new WaitForSeconds(5f);
                _isStepCompleted = true;
            }
            else
            {
                yield return new WaitUntil(() => _isStepCompleted);
            }

            Debug.Log($"[完成] {_currentStep.stepName}");
            UpdateProgressToUI();
            yield return new WaitForSeconds(0.5f);
        }

        ShowTaskCompleteToUI();
        Debug.Log("[CustomPurchaseFlow] 定制产品采购流程完成！");
    }

    /// <summary>
    /// 初始化所有步骤（共13步）
    /// </summary>
    private void InitializeSteps()
    {
        _steps.Clear();

        // ========================================================
        // 阶段1: PMC申请阶段（步骤1~3，定制特有）
        // ========================================================

        _steps.Enqueue(new StepData(
            "PMC查看销售订单",
            "PMC查看销售订单内容，了解定制产品需求",
            "PMC主管",
            "计划物控中心",
            Interactables.ActionType.View
        ));

        _steps.Enqueue(new StepData(
            "PMC查看BOM单",
            "PMC下查对应BOM单，了解物料需求",
            "PMC主管",
            "计划物控中心",
            Interactables.ActionType.View,
            UIManager.UIType.ProductionBOM
        ));

        _steps.Enqueue(new StepData(
            "PMC填写采购申请单",
            "填写采购申请单；点：提交",
            "PMC主管",
            "计划物控中心",
            Interactables.ActionType.Fill
        ));

        // ========================================================
        // 阶段2: 采购执行与入库（步骤4~13，与标准采购流程相同）
        // ========================================================

        _steps.Enqueue(new StepData(
            "采购主管审核采购申请单",
            "采购主管审核采购申请单；点：审核；跟单员可查看",
            "采购主管",
            "采购部",
            Interactables.ActionType.Approve,
            UIManager.UIType.PurchaseRequest
        ));

        _steps.Enqueue(new StepData(
            "跟单员查看采购计划",
            "跟单员查看采购计划",
            "跟单员",
            "采购部",
            Interactables.ActionType.View,
            UIManager.UIType.PurchaseRequest
        ));

        _steps.Enqueue(new StepData(
            "跟单员填写采购订单",
            "跟单员填写采购订单并邮件给供应商",
            "跟单员",
            "采购部",
            Interactables.ActionType.Fill,
            UIManager.UIType.PurchaseOrder
        ));

        _steps.Enqueue(new StepData(
            "供应商送货",
            "供应商电话联系跟单员后送货",
            "供应商",
            "供应商处",
            Interactables.ActionType.View
        ));

        _steps.Enqueue(new StepData(
            "供应商填写送货通知单",
            "供应商填写送货通知单（随货）",
            "供应商",
            "供应商处",
            Interactables.ActionType.Fill
        ));

        _steps.Enqueue(new StepData(
            "仓库到货收货",
            "仓库进行到货收货",
            "仓管员B",
            "仓库",
            Interactables.ActionType.View
        ));

        _steps.Enqueue(new StepData(
            "跟单员制作收料通知单",
            "跟单员制作收料通知单；点：提交；自动下推",
            "跟单员",
            "采购部",
            Interactables.ActionType.Fill,
            UIManager.UIType.ReceiptNotice
        ));

        _steps.Enqueue(new StepData(
            "仓管员查看收料通知单",
            "仓管员查看收料通知单，进行质检",
            "仓管员B",
            "仓库",
            Interactables.ActionType.View,
            UIManager.UIType.ReceiptNotice
        ));

        _steps.Enqueue(new StepData(
            "仓管员填写来料检验单",
            "仓管员填写来料检验单；点：提交；自动下推",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Fill
        ));

        _steps.Enqueue(new StepData(
            "仓管员填写采购入库单",
            "仓管员填写采购入库单；点：提交",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Fill,
            UIManager.UIType.PurchaseInbound
        ));
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
            string actionText = GetActionText(_currentStep.actionType);
            TaskGuidePanelNew.Instance.UpdateCurrentStep(
                _currentStep.stepName,
                _currentStep.description,
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
        Debug.Log($"[CustomPurchaseFlow] MarkStepComplete 被调用！_isStepCompleted 设置为 true");
        _isStepCompleted = true;
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

    #endregion
}
