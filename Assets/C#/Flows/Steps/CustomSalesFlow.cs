using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 定制产品销售流程
/// 从客户询单到提交销售订单给PMC的完整流程
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
        
        public StepData(string name, string desc, string npc, string location, Interactables.ActionType action)
        {
            stepName = name;
            description = desc;
            targetNPC = npc;
            targetLocation = location;
            actionType = action;
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
            // 取出一个步骤
            _currentStep = _steps.Dequeue();
            _isStepCompleted = false;
            _currentStepIndex++;

            Debug.Log($"[步骤 {_currentStepIndex}/{_totalSteps}] {_currentStep.stepName}");

            // 显示当前步骤到UI
            ShowCurrentStepToUI();

            // 等待玩家完成此步骤（通过按E触发 CompleteStep）
            yield return new WaitUntil(() => _isStepCompleted);

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
        _steps.Enqueue(new StepData("填写销售订单", "录入客户需求和产品规格", "销售员", "销售办公室", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("审核销售订单", "销售总监审核订单内容", "销售总监", "销售办公室", Interactables.ActionType.Approve));

        // ===== 阶段3：BOM单处理 =====
        _steps.Enqueue(new StepData("填写BOM单", "技术部根据订单生成物料清单", "技术员", "技术部", Interactables.ActionType.Fill));

        // ===== 阶段4：财务报价 =====
        _steps.Enqueue(new StepData("查看BOM单", "财务部查看物料清单", "财务主管", "财务部", Interactables.ActionType.View));
        _steps.Enqueue(new StepData("计算成本并确认价格", "填写销售报价单", "财务主管", "财务部", Interactables.ActionType.Fill));

        // ===== 阶段5：报价确认 =====
        _steps.Enqueue(new StepData("查看报价单", "销售员查看财务核算的报价", "销售员", "销售办公室", Interactables.ActionType.View));
        _steps.Enqueue(new StepData("报价给客户", "将报价单发送给客户", "销售员", "销售办公室", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("客户确认", "客户确认报价并签订合同", "销售员", "销售办公室", Interactables.ActionType.View));

        // ===== 阶段6：提交PMC =====
        _steps.Enqueue(new StepData("提交销售订单给PMC", "PMC可查看销售订单", "销售员", "PMC办公室", Interactables.ActionType.Fill));
    }

    #region UI更新方法

    /// <summary>
    /// 显示流程信息到UI
    /// </summary>
    private void ShowTaskInfoToUI()
    {
        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.UpdateTaskInfo(FLOW_NAME, TASK_TITLE, TASK_DESCRIPTION, _currentStepIndex, _totalSteps);
        }
    }

    /// <summary>
    /// 显示当前步骤到UI
    /// </summary>
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

    /// <summary>
    /// 更新进度到UI
    /// </summary>
    private void UpdateProgressToUI()
    {
        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.UpdateProgress(_currentStepIndex, _totalSteps);
        }
    }

    /// <summary>
    /// 显示任务完成到UI
    /// </summary>
    private void ShowTaskCompleteToUI()
    {
        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.CompleteTask();
        }
    }

    /// <summary>
    /// 获取操作类型文字
    /// </summary>
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

    /// <summary>
    /// 完成当前步骤（重写基类方法，由 InteractionManager 调用）
    /// </summary>
    public override void MarkStepComplete()
    {
        Debug.Log($"[CustomSalesFlow] MarkStepComplete 被调用！_isStepCompleted 设置为 true");
        _isStepCompleted = true;
    }

    /// <summary>
    /// 获取当前步骤信息
    /// </summary>
    public StepData GetCurrentStep()
    {
        return _currentStep;
    }

    /// <summary>
    /// 获取总步骤数
    /// </summary>
    public int GetTotalSteps()
    {
        return _totalSteps;
    }

    /// <summary>
    /// 获取当前步骤索引（从1开始）
    /// </summary>
    public int GetCurrentStepIndex()
    {
        return _currentStepIndex;
    }

    #endregion
}
