using UnityEngine;
using System.Collections;

/// <summary>
/// 流程任务UI集成示例
/// 演示如何在流程中集成任务引导UI
/// </summary>
public class FlowTaskUIExample : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private bool testOnStart = false;
    [SerializeField] private FlowType testFlowType = FlowType.Sales;
    
    private enum FlowType
    {
        Sales,
        Production,
        Purchase,
        Production2,
        Warehouse
    }

    private void Start()
    {
        if (testOnStart)
        {
            StartCoroutine(TestFlow());
        }
    }

    /// <summary>
    /// 测试流程
    /// </summary>
    private IEnumerator TestFlow()
    {
        yield return new WaitForSeconds(1f);
        
        FlowBase flow = null;
        
        switch (testFlowType)
        {
            case FlowType.Sales:
                flow = new SalesFlow();
                break;
            case FlowType.Production:
                flow = new ProductionFlow();
                break;
            case FlowType.Purchase:
                flow = new PurchaseFlow();
                break;
            case FlowType.Production2:
                flow = new ProductionFlow2();
                break;
            case FlowType.Warehouse:
                flow = new WarehouseFlow();
                break;
        }
        
        if (flow != null && FlowManager.Instance != null)
        {
            FlowManager.Instance.RegisterFlow(flow);
            FlowManager.Instance.StartFlow(flow.GetType());
            
            Debug.Log($"[示例] 已启动测试流程: {testFlowType}");
        }
    }

    #region 集成示例代码
    /// <summary>
    /// 示例1: 如何在现有流程中添加任务UI支持
    /// </summary>
    public class ExampleFlow : FlowBase
    {
        public override void StartFlow()
        {
            Debug.Log("示例流程开始");
            base.StartFlow();
            
            // ⭐ 只需添加这一行即可启用任务UI
            this.InitTaskUI();
        }

        protected override IEnumerator FlowCoroutine()
        {
            // 步骤1
            Debug.Log("执行步骤1");
            yield return new WaitForSeconds(1f);
            FlowStepTracker.CompleteStep(); // ⭐ 完成步骤
            
            // 步骤2
            Debug.Log("执行步骤2");
            yield return new WaitForSeconds(1f);
            FlowStepTracker.CompleteStep(); // ⭐ 完成步骤
            
            // 步骤3
            Debug.Log("执行步骤3");
            yield return new WaitForSeconds(1f);
            FlowStepTracker.CompleteStep(); // ⭐ 完成步骤
            
            FinishFlow();
        }
    }

    /// <summary>
    /// 示例2: 带UI交互的完整流程
    /// </summary>
    public class ExampleFlowWithUI : FlowBase
    {
        public override void StartFlow()
        {
            base.StartFlow();
            this.InitTaskUI(); // ⭐ 初始化任务UI
        }

        protected override IEnumerator FlowCoroutine()
        {
            GameObject currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.测试UI);
            
            // 步骤1: 等待玩家到达位置
            yield return WaitForPlayerReachPosition(
                PositionManager.Instance.GetPosition(PositionManager.PositionType.仓管员)
            );
            
            // 显示UI并等待点击
            UIManager.Instance.ShowUI(UIManager.UIType.测试UI);
            currentUI.GetComponent<TestUIScript>().SetTexts("测试NPC", "测试操作");
            yield return currentUI.GetComponent<TestUIScript>().WaitForButtonClick();
            UIManager.Instance.HideUI(UIManager.UIType.测试UI);
            
            // ⭐ 完成步骤
            FlowStepTracker.CompleteStep();
            yield return new WaitForSeconds(0.5f);
            
            // 重复上述模式执行更多步骤...
            
            FinishFlow();
        }
    }
    #endregion

    #region 快速集成工具方法
    /// <summary>
    /// 为现有流程快速添加任务UI支持
    /// 使用方法：在编辑器中选择此脚本，右键选择"快速集成任务UI"
    /// </summary>
    [ContextMenu("生成集成代码")]
    private void GenerateIntegrationCode()
    {
        string code = @"
// ==================== 快速集成代码 ====================
// 复制以下代码到你的Flow类中

// 1. 在StartFlow方法中添加：
public override void StartFlow()
{
    Debug.Log(""XXX流程开始"");
    base.StartFlow();
    this.InitTaskUI(); // ⭐ 添加这行
}

// 2. 在每个步骤的WaitForButtonClick之后添加：
yield return currentUI.GetComponent<TestUIScript>().WaitForButtonClick();
UIManager.Instance.HideUI(UIManager.UIType.测试UI);
FlowStepTracker.CompleteStep(); // ⭐ 添加这行
yield return new WaitForSeconds(1f);

// 3. 完成！任务UI会自动显示和更新
// ====================================================
";
        Debug.Log(code);
    }

    /// <summary>
    /// 显示所有可用流程的配置信息
    /// </summary>
    [ContextMenu("显示流程配置")]
    private void ShowFlowConfigs()
    {
        if (FlowTaskIntegration.Instance != null)
        {
            Debug.Log("=== 已配置的流程 ===");
            Debug.Log("1. SalesFlow - 销售流程 (9步)");
            Debug.Log("2. ProductionFlow - 生产流程1 (9步)");
            Debug.Log("3. PurchaseFlow - 采购流程 (21步)");
            Debug.Log("4. ProductionFlow2 - 生产流程2 (26步)");
            Debug.Log("5. WarehouseFlow - 仓库流程 (4步)");
            Debug.Log("==================");
        }
        else
        {
            Debug.LogWarning("FlowTaskIntegration未找到，请先添加到场景中");
        }
    }
    #endregion
}

