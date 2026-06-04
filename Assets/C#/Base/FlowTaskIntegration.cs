using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 流程与任务UI集成管理器
/// </summary>
public class FlowTaskIntegration : MonoBehaviour
{
    #region 单例
    private static FlowTaskIntegration _instance;
    public static FlowTaskIntegration Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<FlowTaskIntegration>();
            return _instance;
        }
    }
    #endregion

    [Header("UI引用")]
    [SerializeField] private TaskGuidePanelNew taskGuidePanel;
    [SerializeField] private bool autoFindPanel = true;

    private Dictionary<System.Type, FlowTaskConfig> flowConfigs = new Dictionary<System.Type, FlowTaskConfig>();
    private FlowBase currentFlow;
    private FlowTaskConfig currentConfig;
    private int currentStepIndex = 0;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); return; }
        
        if (autoFindPanel && taskGuidePanel == null)
            taskGuidePanel = FindObjectOfType<TaskGuidePanelNew>();
        
        InitializeFlowConfigs();
    }

    private void InitializeFlowConfigs()
    {
        // ===== 定制产品销售流程 =====
        // 步骤配置在 CustomSalesFlow.InitializeSteps() 中定义

        //// 销售流程
        //flowConfigs[typeof(SalesFlow)] = new FlowTaskConfig
        //{
        //    flowName = "销售流程",
        //    taskTitle = "销售订单处理",
        //    taskDescription = "完成从报价到销售订单的完整流程",
        //    steps = new List<StepInfo>
        //    {
        //        new StepInfo("填写报价单", "填写客户报价单", "销售员", "销售办公室", "填写"),
        //        new StepInfo("审核报价单", "审核报价单", "销售主管", "销售办公室", "审核"),
        //        new StepInfo("下推报价单", "下推到销售合同", "销售员", "销售办公室", "下推"),
        //        new StepInfo("填写销售合同", "填写销售合同", "销售员", "销售办公室", "填写"),
        //        new StepInfo("审核销售合同", "审核销售合同", "销售主管", "销售办公室", "审核"),
        //        new StepInfo("下推销售合同", "下推到销售订单", "销售员", "销售办公室", "下推"),
        //        new StepInfo("填写销售订单", "填写销售订单", "销售员", "销售办公室", "填写"),
        //        new StepInfo("审核销售订单", "审核销售订单", "销售主管", "销售办公室", "审核"),
        //        new StepInfo("下推销售订单", "完成销售订单", "销售员", "销售办公室", "下推")
        //    }
        //};

        //// 生产流程1
        //flowConfigs[typeof(ProductionFlow)] = new FlowTaskConfig
        //{
        //    flowName = "生产流程",
        //    taskTitle = "生产准备工作",
        //    taskDescription = "完成生产工单和物料准备",
        //    steps = new List<StepInfo>
        //    {
        //        new StepInfo("填写生产工单", "填写生产工单", "生产主管", "生产办公室", "填写"),
        //        new StepInfo("审核生产工单", "审核生产工单", "生产主管", "生产办公室", "审核"),
        //        new StepInfo("下推生产工单", "下推到用料清单", "生产主管", "生产办公室", "下推"),
        //        new StepInfo("填写用料清单", "填写用料清单", "生产主管", "生产办公室", "填写"),
        //        new StepInfo("审核用料清单", "审核用料清单", "生产主管", "生产办公室", "审核"),
        //        new StepInfo("下推用料清单", "下推到领料申请", "生产主管", "生产办公室", "下推"),
        //        new StepInfo("填写领料申请", "填写领料申请", "领料员", "原料仓储区", "填写"),
        //        new StepInfo("审核领料申请", "审核领料申请", "生产主管", "生产办公室", "审核"),
        //        new StepInfo("下推领料申请", "下推到领料单", "领料员", "原料仓储区", "下推")
        //    }
        //};

        //// 采购流程
        //flowConfigs[typeof(PurchaseFlow)] = new FlowTaskConfig
        //{
        //    flowName = "采购流程",
        //    taskTitle = "物料采购",
        //    taskDescription = "完成从库存检查到物料入库的采购流程",
        //    steps = new List<StepInfo>
        //    {
        //        new StepInfo("库存检查", "查看生产领料单", "仓管员", "成品仓储区", "查看"),
        //        new StepInfo("确认库存不足", "确认需要采购", "仓管员", "成品仓储区", "确认"),
        //        new StepInfo("填写物料需求单", "填写物料清单", "仓管员", "成品仓储区", "填写"),
        //        new StepInfo("审核物料需求单", "审核需求单", "仓库主管", "仓库主管办公室", "审核"),
        //        new StepInfo("下推物料需求单", "下推到采购申请", "仓管员", "成品仓储区", "下推"),
        //        new StepInfo("填写采购需求申请", "填写采购申请", "采购员", "采购办公室", "填写"),
        //        new StepInfo("审核采购需求申请", "审核采购申请", "采购主管", "采购办公室", "审核"),
        //        new StepInfo("下推采购需求申请", "下推到采购申请单", "采购员", "采购办公室", "下推"),
        //        new StepInfo("填写采购申请单", "填写采购申请", "采购员", "采购办公室", "填写"),
        //        new StepInfo("审核采购申请单", "审核采购申请", "采购主管", "采购办公室", "审核"),
        //        new StepInfo("下推采购申请单", "下推到采购订单", "采购员", "采购办公室", "下推"),
        //        new StepInfo("填写采购订单", "填写采购订单", "采购员", "采购办公室", "填写"),
        //        new StepInfo("审核采购订单", "审核采购订单", "采购主管", "采购办公室", "审核"),
        //        new StepInfo("等待采购", "等待采购完成", "采购员", "采购办公室", "等待"),
        //        new StepInfo("下推采购订单", "下推到收料通知", "采购员", "采购办公室", "下推"),
        //        new StepInfo("填写收料通知单", "填写收料通知", "采购员", "采购办公室", "填写"),
        //        new StepInfo("审核收料通知单", "审核收料通知", "采购主管", "采购办公室", "审核"),
        //        new StepInfo("下推收料通知单", "下推到入库单", "采购员", "采购办公室", "下推"),
        //        new StepInfo("来料质检", "质检物料", "质检员", "质检处", "质检"),
        //        new StepInfo("填写采购入库单", "填写入库单", "仓管员", "成品仓储区", "填写"),
        //        new StepInfo("审核采购入库单", "审核入库单", "仓库主管", "仓库主管办公室", "审核")
        //    }
        //};

        //// 生产流程2
        //flowConfigs[typeof(ProductionFlow2)] = new FlowTaskConfig
        //{
        //    flowName = "生产流程2",
        //    taskTitle = "生产执行",
        //    taskDescription = "完成产品生产和质检入库",
        //    steps = new List<StepInfo>
        //    {
        //        new StepInfo("填写生产领料单", "填写领料单", "仓管员", "成品仓储区", "填写"),
        //        new StepInfo("审核生产领料单", "审核领料单", "仓库主管", "仓库主管办公室", "审核"),
        //        new StepInfo("领取物料", "领取物料", "领料员", "原料仓储区", "领料"),
        //        new StepInfo("填写工序计划单", "填写工序计划", "生产主管", "生产办公室", "填写"),
        //        new StepInfo("审核工序计划单", "审核工序计划", "生产主管", "生产办公室", "审核"),
        //        new StepInfo("下推工序计划单", "分配给班组长", "五个班组长", "生产区", "下推"),
        //        new StepInfo("填写工序任务", "填写工序任务", "五个班组长", "生产区", "填写"),
        //        new StepInfo("审核工序任务", "审核工序任务", "生产主管", "生产办公室", "审核"),
        //        new StepInfo("查看工序任务", "查看工序任务", "五个班组长", "生产区", "查看"),
        //        new StepInfo("领取原料", "领取原料", "领料员", "原料仓储区", "领取"),
        //        new StepInfo("开始组装", "开始组装", "所有班组长", "生产区", "组装"),
        //        new StepInfo("前4道工序汇报", "填写工序汇报", "4位班组长", "生产区", "填写"),
        //        new StepInfo("审核工序汇报", "审核工序汇报", "生产主管", "生产办公室", "审核"),
        //        new StepInfo("下推工序汇报", "下推到检验单", "4位班组长", "生产区", "下推"),
        //        new StepInfo("工序质检", "质检工序", "质检员", "质检处", "质检"),
        //        new StepInfo("填写工序检验单", "填写检验结果", "质检员", "质检处", "填写"),
        //        new StepInfo("最后工序汇报", "填写最后工序", "5车间班组长", "生产区", "填写"),
        //        new StepInfo("审核最后工序", "审核最后工序", "生产主管", "生产办公室", "审核"),
        //        new StepInfo("下推到生产汇报", "下推生产汇报", "5车间班组长", "生产区", "下推"),
        //        new StepInfo("填写生产汇报单", "填写总汇报", "5车间班组长", "生产区", "填写"),
        //        new StepInfo("审核生产汇报单", "审核总汇报", "生产主管", "生产办公室", "审核"),
        //        new StepInfo("下推生产汇报", "下推到检验单", "5车间班组长", "生产区", "下推"),
        //        new StepInfo("最终质检", "最终质检", "质检员", "质检处", "质检"),
        //        new StepInfo("填写最终检验单", "填写最终结果", "质检员", "质检处", "填写"),
        //        new StepInfo("审核最终检验", "审核最终检验", "生产主管", "生产办公室", "审核"),
        //        new StepInfo("成品入库", "成品入库", "5车间班组长", "生产区", "入库")
        //    }
        //};

        //// 仓库流程
        //flowConfigs[typeof(WarehouseFlow)] = new FlowTaskConfig
        //{
        //    flowName = "仓库流程",
        //    taskTitle = "仓库管理",
        //    taskDescription = "完成成品入库和销售出库",
        //    steps = new List<StepInfo>
        //    {
        //        new StepInfo("填写完工入库单", "填写入库单", "仓管员", "成品仓储区", "填写"),
        //        new StepInfo("审核完工入库单", "审核入库单", "仓库主管", "仓库主管办公室", "审核"),
        //        new StepInfo("填写销售出库单", "填写出库单", "仓管员", "成品仓储区", "填写"),
        //        new StepInfo("审核销售出库单", "审核出库单", "仓库主管", "仓库主管办公室", "审核")
        //    }
        //};
    }

    public void StartFlowWithUI(FlowBase flow)
    {
        if (flow == null) return;
        
        currentFlow = flow;
        
        // 让 CustomSalesFlow 自己管 UI
        flow.StartFlow();
    }

    public void CompleteCurrentStep()
    {
        Debug.Log($"[FlowTaskIntegration] CompleteCurrentStep 被调用");
        Debug.Log($"[FlowTaskIntegration] currentFlow 是否为空: {currentFlow == null}");
        
        // 调用流程的完成方法
        if (currentFlow != null)
        {
            // 如果是 StandardSalesFlow，检查是否在分支流程中
            if (currentFlow is StandardSalesFlow standardFlow)
            {
                FlowBase branchFlow = GetCurrentBranchFlow(standardFlow);
                if (branchFlow != null)
                {
                    Debug.Log($"[FlowTaskIntegration] 调用分支流程 {branchFlow.GetType().Name}.MarkStepComplete()");
                    branchFlow.MarkStepComplete();
                    return;
                }
            }

            // 如果是 CustomSalesFlow，检查是否在分支流程中
            if (currentFlow is CustomSalesFlow customFlow)
            {
                FlowBase branchFlow = GetCurrentBranchFlow(customFlow);
                if (branchFlow != null)
                {
                    Debug.Log($"[FlowTaskIntegration] 调用分支流程 {branchFlow.GetType().Name}.MarkStepComplete()");
                    branchFlow.MarkStepComplete();
                    return;
                }
            }
            
            Debug.Log($"[FlowTaskIntegration] 调用 currentFlow.MarkStepComplete()");
            currentFlow.MarkStepComplete();
        }
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

    //private System.Collections.IEnumerator DelayedShowNextStep()
    //{
    //    yield return new WaitForSeconds(0.5f);
    //    taskGuidePanel.UpdateProgress(currentStepIndex, currentConfig.steps.Count);
    //    ShowStep(currentStepIndex);
    //}

    //private void ShowStep(int stepIndex)
    //{
    //    if (currentConfig == null || taskGuidePanel == null) return;
    //    if (stepIndex < 0 || stepIndex >= currentConfig.steps.Count) return;
    //    
    //    StepInfo step = currentConfig.steps[stepIndex];
    //    taskGuidePanel.UpdateCurrentStep(step.title, step.description, step.targetNPC, step.targetLocation, step.actionType, step.hint);
    //}

    public void SetTaskGuidePanel(TaskGuidePanelNew panel) { taskGuidePanel = panel; }
    public FlowTaskConfig GetCurrentFlowConfig() => currentConfig;
    public int GetCurrentStepIndex() => currentStepIndex;
    public FlowBase GetCurrentFlow() => currentFlow;
}

[System.Serializable]
public class FlowTaskConfig
{
    public string flowName;
    public string taskTitle;
    public string taskDescription;
    public List<StepInfo> steps;
}

[System.Serializable]
public class StepInfo
{
    public string title;
    public string description;
    public string targetNPC;
    public string targetLocation;
    public string actionType;
    public string hint;
    
    public StepInfo(string title, string description, string targetNPC = "", 
                   string targetLocation = "", string actionType = "", string hint = "")
    {
        this.title = title;
        this.description = description;
        this.targetNPC = targetNPC;
        this.targetLocation = targetLocation;
        this.actionType = actionType;
        this.hint = hint;
    }
}
