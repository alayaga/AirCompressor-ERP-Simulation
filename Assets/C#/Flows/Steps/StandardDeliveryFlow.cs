using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 标准产品发货流程
/// 从销售订单到发货完成的完整流程
/// </summary>
public class StandardDeliveryFlow : FlowBase
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
    private const string FLOW_NAME = "标准产品发货流程";
    private const string TASK_TITLE = "销售发货流程";
    private const string TASK_DESCRIPTION = "完成从销售订单到发货的完整流程";
    
    // 步骤队列
    private Queue<StepData> _steps = new Queue<StepData>();
    private StepData _currentStep;
    private bool _isStepCompleted = false;
    private int _totalSteps;
    private int _currentStepIndex = 0;
    
    // 库存检查相关
    private bool _hasStock = false;  // 是否有现货
    private bool _stockChecked = false;  // 是否已检查库存

    protected override IEnumerator FlowCoroutine()
    {
        Debug.Log("[StandardDeliveryFlow] 开始标准产品发货流程");

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

            // 判断是否为自动步骤（客户操作或系统操作）
            bool isAutoStep = _currentStep.targetNPC == "客户" || _currentStep.targetNPC == "系统";
            
            if (isAutoStep)
            {
                // 自动步骤：等待5秒后自动完成
                Debug.Log($"[StandardDeliveryFlow] 自动步骤：{_currentStep.stepName}，等待5秒后自动完成");
                yield return new WaitForSeconds(5f);
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
            
            // 库存检查判断逻辑（在"检查库存"步骤后执行）
            if (_currentStep.stepName == "检查库存")
            {
                _stockChecked = true;
                // 模拟库存检查结果（实际项目中应该从数据库或库存系统获取）
                _hasStock = CheckStock();
                
                if (_hasStock)
                {
                    Debug.Log("[StandardDeliveryFlow] 库存充足，跳过生产流程");
                    // 跳过生产步骤，直接进入发货流程
                    SkipProductionSteps();
                }
                else
                {
                    Debug.Log("[StandardDeliveryFlow] 库存不足，需要执行生产流程");
                }
            }

            yield return new WaitForSeconds(0.5f);
        }

        // 显示流程完成
        ShowTaskCompleteToUI();
        Debug.Log("[StandardDeliveryFlow] 标准产品发货流程完成！");
    }

    /// <summary>
    /// 初始化所有步骤（根据 标准产品流程v1.3 流程图 - 标准产品销售-发货流程）
    /// </summary>
    private void InitializeSteps()
    {
        _steps.Clear();

        // ===== 标准产品销售-发货流程（完整流程）=====
        
        // ========== 阶段1：订单处理 ==========
        _steps.Enqueue(new StepData("客户下单", "客户通过官网选择机型、数量，点击下单，自动生成销售订单", "客户", "系统", Interactables.ActionType.View));
        _steps.Enqueue(new StepData("审核销售订单", "销售总监审核销售订单，点击审核后仓库可查看", "销售总监", "销售部", Interactables.ActionType.Approve));
        _steps.Enqueue(new StepData("查看销售订单", "仓管员查看销售订单", "仓管员", "质检区", Interactables.ActionType.View));
        _steps.Enqueue(new StepData("检查库存", "仓管员检查现有库存", "仓管员", "质检区", Interactables.ActionType.View));
        
        // ========== 阶段2：生产流程（无现货时执行）==========-
        _steps.Enqueue(new StepData("PMC填写排产单", "PMC填写每日排产单，提交后自动下推给仓管员和车间主管", "PMC主管", "PMC部", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("仓管员发料", "仓管员按生产用料清单发料到备料区", "仓管员", "质检区", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("车间主管派工", "车间主管按生产工单派工给班组长，填写4个车间的工序计划单", "车间主管", "生产部", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("班组长填派工单", "1/2/3/4车间班组长查看工序计划单，填写工人个人派工单", "车间班组长", "生产区", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("工人领料", "工人查看派工单，到备料区领料并填写领料单", "工人", "备料区", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("工人生产", "工人进行生产加工", "工人", "生产区", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("工序汇报", "1/2/3车间班组长检查并填写工序汇报单", "车间班组长", "生产区", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("半成品转运", "1/2/3车间员工将半成品送往4车间", "工人", "生产区", Interactables.ActionType.View));
        _steps.Enqueue(new StepData("4车间组装", "4车间负责组装，组装完成后将成品送往仓库", "工人", "生产区", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("成品入库", "成品入库，更新库存", "仓管员", "质检区", Interactables.ActionType.Fill));
        
        // ========== 阶段3：发货流程（有现货或生产完成后执行）==========
        _steps.Enqueue(new StepData("填写发货通知单", "仓管员填写发货通知单", "仓管员", "质检区", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("审核发货通知单", "仓库主管审核发货通知单", "仓库主管", "仓库部", Interactables.ActionType.Approve));
        _steps.Enqueue(new StepData("包装出库", "仓库包装出库，发货完成", "仓管员", "质检区", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("填写销售出库单", "仓管员填写销售出库单（由发货通知单下推）", "仓管员", "质检区", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("审核销售出库单", "仓库主管审核销售出库单", "仓库主管", "仓库部", Interactables.ActionType.Approve));
        _steps.Enqueue(new StepData("客户签收", "客户收货，在发货通知单上签字", "客户", "客户处", Interactables.ActionType.View));
    }

    /// <summary>
    /// 检查库存（模拟）
    /// 实际项目中应该从数据库或库存系统获取真实库存数据
    /// </summary>
    /// <returns>true=有现货，false=库存不足</returns>
    private bool CheckStock()
    {
        // 模拟库存检查（70%概率有货）
        bool hasStock = Random.value > 0.3f;
        
        // 更新UI显示库存状态
        if (TaskGuidePanelNew.Instance != null)
        {
            string hintText = hasStock 
                ? "库存充足，可以直接发货\n(跳过生产流程)" 
                : "库存不足，需要生产";
            TaskGuidePanelNew.Instance.UpdateHintText(hintText);
        }
        
        return hasStock;
    }

    /// <summary>
    /// 跳过生产步骤，直接进入发货流程
    /// </summary>
    private void SkipProductionSteps()
    {
        // 定义需要跳过的生产步骤名称
        List<string> stepsToSkip = new List<string>
        {
            "PMC填写排产单",
            "仓管员发料",
            "车间主管派工",
            "班组长填派工单",
            "工人领料",
            "工人生产",
            "工序汇报",
            "半成品转运",
            "4车间组装",
            "成品入库"
        };
        
        // 创建新队列，只保留发货及之后的步骤
        Queue<StepData> remainingSteps = new Queue<StepData>();
        
        while (_steps.Count > 0)
        {
            StepData step = _steps.Dequeue();
            
            if (!stepsToSkip.Contains(step.stepName))
            {
                remainingSteps.Enqueue(step);
            }
            else
            {
                Debug.Log($"[StandardDeliveryFlow] 跳过步骤：{step.stepName}");
            }
        }
        
        // 更新步骤队列
        _steps = remainingSteps;
        
        // 更新总步骤数（减去跳过的步骤数）
        _totalSteps = _currentStepIndex + _steps.Count;
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
        Debug.Log($"[StandardDeliveryFlow] MarkStepComplete 被调用！_isStepCompleted 设置为 true");
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