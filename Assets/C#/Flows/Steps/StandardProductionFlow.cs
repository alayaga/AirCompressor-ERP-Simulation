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
        public UIManager.UIType? billType;  // 对应单据类型，null=无单据
        public DialogueConfig dialogueConfig; // 对话配置
        public StepData(string name, string desc, string npc, string location, Interactables.ActionType action, UIManager.UIType? billType = null, DialogueConfig dialogueConfig = default)
        {
            stepName = name;
            description = desc;
            targetNPC = npc;
            targetLocation = location;
            actionType = action;
            this.billType = billType;
            this.dialogueConfig = dialogueConfig;
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

            if (_currentStep.targetNPC == "系统")
            {
                // 弹窗提示（如"后续车间同上"）
                if (TaskGuidePanelNew.Instance != null)
                {
                    TaskGuidePanelNew.Instance.UpdateHintText(_currentStep.description);
                }
                Debug.Log($"[StandardProductionFlow] 系统步骤自动完成: {_currentStep.stepName}");
                yield return new WaitForSeconds(3f);
                // 清除提示
                if (TaskGuidePanelNew.Instance != null)
                {
                    TaskGuidePanelNew.Instance.UpdateHintText("");
                }
                _isStepCompleted = true;
            }
            else
            {
                yield return new WaitUntil(() => _isStepCompleted);
            }

            if (_currentStep.billType != null)
            {
                bool stepDone = false;
                while (!stepDone)
                {
                    _isStepCompleted = false;
                    yield return WaitForBillComplete(_currentStep.billType.Value, _currentStep.targetNPC, _currentStep.actionType);
                    if (!_billOpenSuccess)
                    {
                        Debug.LogError($"[StandardProductionFlow] 单据 {_currentStep.billType} 打开失败，跳过步骤: {_currentStep.stepName}");
                        stepDone = true;
                    }
                    else if (_isStepCompleted)
                        stepDone = true;
                    else
                        yield return new WaitUntil(() => _isStepCompleted);
                }
            }

            Debug.Log($"[完成] {_currentStep.stepName}");
            UpdateProgressToUI();
            yield return new WaitForSeconds(0.5f);
        }

        ShowTaskCompleteToUI();
        Debug.Log("[StandardProductionFlow] 标准产品生产流程完成！");
    }    private void InitializeSteps()
    {
        _steps.Clear();

        // ===== 标准产品生产流程 v1.5 =====
        // targetNPC 使用场景实际命名（1/2/3/4车间），UI描述使用弯管/焊接/配电/总装

        // 阶段1：计划与排产（保留 Standard 自身 PMC 步骤）
        _steps.Enqueue(new StepData("制作一周生产计划", "根据销售计划与库存情况，制作一周生产计划并提交", "PMC主管", "计划物控中心", Interactables.ActionType.Fill, UIManager.UIType.WeeklyProductionPlan));
        _steps.Enqueue(new StepData("PMC填写每日排产单", "填写每日排产单，点击提交后生产主管可查看", "PMC主管", "计划物控中心", Interactables.ActionType.Fill, UIManager.UIType.ProductionSchedule));

        // ========================================================
        // 以下复用定制产品生产流程（targetNPC 已对齐场景命名）
        // ========================================================

        _steps.Enqueue(new StepData(
            "仓管员发料到备料区",
            "根据排产单与生产用料清单，将物料发放到备料区",
            "仓管员A",
            "备料区",
            Interactables.ActionType.Deliver
        ));

        // ========================================================
        // 阶段2: 弯管车间（1车间）
        // ========================================================

        _steps.Enqueue(new StepData(
            "1车间主管填写生产工单",
            "填写弯管车间（1车间）生产工单",
            "1车间主管",
            "1车间-弯管",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProductionWorkOrder
        ));

        _steps.Enqueue(new StepData(
            "1车间班组长填写派工单",
            "查看弯管车间（1车间）生产工单；填写工人个人的派工单",
            "1车间班组长",
            "1车间-弯管",
            Interactables.ActionType.Fill,
            UIManager.UIType.DispatchOrder
        ));

        _steps.Enqueue(new StepData(
            "1车间工人到备料区领料",
            "查看自己的派工单，根据派工单到备料区领料",
            "1车间工人",
            "备料区",
            Interactables.ActionType.Pick
        ));

        _steps.Enqueue(new StepData(
            "1车间工人填写领料单",
            "填写领料单；点：提交",
            "1车间工人",
            "备料区",
            Interactables.ActionType.Fill,
            UIManager.UIType.PickList
        ));

        _steps.Enqueue(new StepData(
            "1车间工人生产",
            "工人进行弯管车间（1车间）生产操作",
            "1车间工人",
            "1车间-弯管",
            Interactables.ActionType.View
        ));

        _steps.Enqueue(new StepData(
            "1车间工人退料实物送仓库",
            "将生产多余物料退回仓库",
            "1车间工人",
            "1车间-弯管",
            Interactables.ActionType.Deliver
        ));

        _steps.Enqueue(new StepData(
            "仓管员质检确认退料",
            "对弯管车间（1车间）退料进行质检确认",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Approve
        ));

        _steps.Enqueue(new StepData(
            "仓管员制生产退料入库单",
            "填写生产退料入库单",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProductionReturn
        ));

        _steps.Enqueue(new StepData(
            "1车间工人签字确认退料",
            "在生产退料入库单上签字",
            "1车间工人",
            "1车间-弯管",
            Interactables.ActionType.Sign,
            UIManager.UIType.ProductionReturn
        ));

        _steps.Enqueue(new StepData(
            "1车间班组长检查并填写工序汇报单",
            "弯管车间（1车间）生产完成后检查；填写工序汇报单",
            "1车间班组长",
            "1车间-弯管",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProcessReport
        ));

        _steps.Enqueue(new StepData(
            "1车间工人将半成品送往2车间",
            "将弯管车间（1车间）半成品送往焊接车间（2车间）",
            "1车间工人",
            "2车间-焊接",
            Interactables.ActionType.Deliver
        ));

        // ========================================================
        // 阶段3: 焊接车间（2车间）
        // ========================================================

        _steps.Enqueue(new StepData(
            "2车间主管填写生产工单",
            "填写焊接车间（2车间）生产工单",
            "2车间主管",
            "2车间-焊接",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProductionWorkOrder
        ));

        _steps.Enqueue(new StepData(
            "2车间班组长填写派工单",
            "查看焊接车间（2车间）生产工单；填写工人个人的派工单",
            "2车间班组长",
            "2车间-焊接",
            Interactables.ActionType.Fill,
            UIManager.UIType.DispatchOrder
        ));

        _steps.Enqueue(new StepData(
            "2车间工人生产",
            "工人进行焊接车间（2车间）生产操作",
            "2车间工人",
            "2车间-焊接",
            Interactables.ActionType.View
        ));

        _steps.Enqueue(new StepData(
            "2车间班组长检查并填写工序汇报单",
            "弯管车间（1车间）已填写工序汇报单，无需重复填写",
            "系统",
            "-",
            Interactables.ActionType.View
        ));

        _steps.Enqueue(new StepData(
            "2车间工人将半成品送往3车间",
            "将焊接车间（2车间）半成品送往配电车间（3车间）",
            "2车间工人",
            "3车间-配电",
            Interactables.ActionType.Deliver
        ));

        // ========================================================
        // 阶段4: 配电车间（3车间）
        // ========================================================

        _steps.Enqueue(new StepData(
            "3车间主管填写生产工单",
            "填写配电车间（3车间）生产工单",
            "3车间主管",
            "3车间-配电",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProductionWorkOrder
        ));

        _steps.Enqueue(new StepData(
            "3车间班组长填写派工单",
            "查看配电车间（3车间）生产工单；填写工人个人的派工单",
            "3车间班组长",
            "3车间-配电",
            Interactables.ActionType.Fill,
            UIManager.UIType.DispatchOrder
        ));

        _steps.Enqueue(new StepData(
            "3车间工人生产",
            "工人进行配电车间（3车间）生产操作",
            "3车间工人",
            "3车间-配电",
            Interactables.ActionType.View
        ));

        _steps.Enqueue(new StepData(
            "3车间班组长检查并填写工序汇报单",
            "弯管车间（1车间）已填写工序汇报单，无需重复填写",
            "系统",
            "-",
            Interactables.ActionType.View
        ));

        _steps.Enqueue(new StepData(
            "3车间工人将半成品送往4车间",
            "将配电车间（3车间）半成品送往总装车间（4车间）",
            "3车间工人",
            "4车间-总装",
            Interactables.ActionType.Deliver
        ));

        // ========================================================
        // 阶段5: 总装车间（4车间）及入库
        // ========================================================

        _steps.Enqueue(new StepData(
            "4车间主管填写生产工单",
            "填写总装车间（4车间）生产工单",
            "4车间主管",
            "4车间-总装",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProductionWorkOrder
        ));

        _steps.Enqueue(new StepData(
            "4车间班组长填写派工单",
            "查看总装车间（4车间）生产工单；填写工人个人的派工单",
            "4车间班组长",
            "4车间-总装",
            Interactables.ActionType.Fill,
            UIManager.UIType.DispatchOrder
        ));

        _steps.Enqueue(new StepData(
            "4车间工人总装",
            "总装车间（4车间）负责组装来自弯管（1车间）/焊接（2车间）/配电（3车间）的半成品",
            "4车间工人",
            "4车间-总装",
            Interactables.ActionType.View
        ));

        _steps.Enqueue(new StepData(
            "4车间工人将成品送往仓库",
            "组装完成后由总装车间工人（4车间）将成品送往仓库",
            "4车间工人",
            "仓库",
            Interactables.ActionType.Deliver
        ));

        _steps.Enqueue(new StepData(
            "仓管员质检确认成品",
            "总装完成的成品经质检合格后，完成入库",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Approve
        ));

        _steps.Enqueue(new StepData(
            "仓管员填写完工入库单",
            "填写完工入库单；工人可查看",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Fill,
            UIManager.UIType.FinishedInbound
        ));

        _steps.Enqueue(new StepData(
            "工人在完工入库单签字",
            "总装车间工人在完工入库单上签字",
            "4车间工人",
            "仓库",
            Interactables.ActionType.Approve,
            UIManager.UIType.FinishedInbound
        ));

        _steps.Enqueue(new StepData(
            "自动通知销售员产品入库",
            "系统自动通知销售员产品已入库",
            "系统",
            "-",
            Interactables.ActionType.View
        ));
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
            case Interactables.ActionType.Ship: return "发货";
            case Interactables.ActionType.Sign: return "签字";
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

    public override DialogueConfig GetCurrentStepDialogueConfig()
    {
        return _currentStep?.dialogueConfig ?? DialogueConfig.None;
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