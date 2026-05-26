using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 标准产品生产流程
/// 从PMC排产到成品入库的完整流程
/// </summary>
public class StandardProductionFlow : FlowBase
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
    private const string FLOW_NAME = "标准产品生产流程";
    private const string TASK_TITLE = "生产流程";
    private const string TASK_DESCRIPTION = "完成从PMC排产到成品入库的完整流程";

    // 步骤队列
    private Queue<StepData> _steps = new Queue<StepData>();
    private StepData _currentStep;
    private bool _isStepCompleted = false;
    private int _totalSteps;
    private int _currentStepIndex = 0;

    protected override IEnumerator FlowCoroutine()
    {
        Debug.Log("[StandardProductionFlow] 开始标准产品生产流程");

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

            // 判断是否为自动步骤（系统操作）
            bool isAutoStep = _currentStep.targetNPC == "系统";

            if (isAutoStep)
            {
                // 自动步骤：等待3秒后自动完成
                Debug.Log($"[StandardProductionFlow] 系统步骤自动完成: {_currentStep.stepName}");
                yield return new WaitForSeconds(3f);
                _isStepCompleted = true;
            }
            else
            {
                // 手动步骤：等待玩家完成（通过按E触发 CompleteStep）
                yield return new WaitUntil(() => _isStepCompleted);
            }

            Debug.Log($"[完成] {_currentStep.stepName}");

            // 更新进度到UI
            UpdateProgressToUI();

            yield return new WaitForSeconds(0.5f);
        }

        // 显示流程完成
        ShowTaskCompleteToUI();
        Debug.Log("[StandardProductionFlow] 标准产品生产流程完成！");
    }

    /// <summary>
    /// 初始化所有步骤（根据标准产品流程v1.3流程图 - 标准产品生产流程）
    /// </summary>
    private void InitializeSteps()
    {
        _steps.Clear();

        // ===== 标准产品生产流程 =====
        // PMC填写排产单开始，到成品入库结束

        _steps.Enqueue(new StepData("PMC填写排产单", "PMC主管填写排产单，点击提交后生产主管可查看", "PMC主管", "PMC部", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("仓管员发料", "仓管员根据排产单发料", "仓管员", "质检区", Interactables.ActionType.Pick));
        _steps.Enqueue(new StepData("车间主管派工", "车间主管进行派工", "车间主管", "生产部", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("班组长填派工单", "车间班组长填写派工单", "车间班组长", "生产区", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("工人领料", "工人到仓库领料", "工人", "备料区", Interactables.ActionType.Pick));
        _steps.Enqueue(new StepData("工人生产", "工人进行生产作业", "工人", "生产区", Interactables.ActionType.View));
        _steps.Enqueue(new StepData("工序汇报", "车间班组长进行工序汇报", "车间班组长", "生产区", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("半成品转运", "工人将半成品转运到下一工序", "工人", "生产区", Interactables.ActionType.View));
        _steps.Enqueue(new StepData("4车间组装", "工人在4车间进行组装", "工人", "生产区", Interactables.ActionType.View));
        _steps.Enqueue(new StepData("成品入库", "仓管员将成品入库", "仓管员", "质检区", Interactables.ActionType.Fill));
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
        Debug.Log($"[StandardProductionFlow] MarkStepComplete 被调用！_isStepCompleted 设置为 true");
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