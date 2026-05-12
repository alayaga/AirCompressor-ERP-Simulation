using UnityEngine;
using System.Collections;

/// <summary>
/// 生产流程
/// </summary>
public class ProductionFlow : FlowBase
{
    protected override IEnumerator FlowCoroutine()
    {
        Debug.Log("生产流程开始");

        var demand = DemandManager.Instance.GetCurrentDemand();
        string workOrderNo = "WO-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999);
        string currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // 生产工单流程
        yield return ProcessProductionOrder(demand, workOrderNo, currentTime);
        
        // 生产用料清单流程
        yield return ProcessMaterialList(demand, workOrderNo, currentTime);
        
        // 生产领料申请单流程
        yield return ProcessMaterialRequest(demand, workOrderNo, currentTime);
        
        FinishFlow();
    }

    private IEnumerator ProcessProductionOrder(DemandManager.CustomerDemand demand, string workOrderNo, string currentTime)
    {
        // 填写生产工单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.生产主管));
        
        var currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.生产工单);
        UIManager.Instance.ShowUI(UIManager.UIType.生产工单);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.生产主管);
        
        var manager = currentUI.GetComponent<FinishedProductOutboundManager>();
        yield return manager.WaitForFillButtonClick();
        
        string[] formData1 = new string[] {
            workOrderNo, currentTime, "AC-" + Random.Range(100, 999), "空气压缩机", "型号 D",
            demand.airCompressorCount.ToString(), "A 车间", System.DateTime.Now.ToString("yyyy-MM-dd"),
            "十天后", "生产主管", "已填写", "无"
        };
        
        string[] formData2 = new string[] { "生产主管", currentTime, "生产主管", currentTime, "", "" };
        
        manager.SetQuoteFormData(formData1, formData2, null);
        yield return manager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.生产工单);
        FlowStepTracker.CompleteStep();

        // 审核生产工单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.生产主管));
        UIManager.Instance.ShowUI(UIManager.UIType.生产工单);
        manager.SetQuoteFormData(formData1, formData2, null);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.生产主管);
        
        yield return manager.WaitForApproveButtonClick();
        formData2[4] = "生产主管";
        formData2[5] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        manager.SetQuoteFormData(formData1, formData2, null);
        UIManager.Instance.HideUI(UIManager.UIType.生产工单);
        FlowStepTracker.CompleteStep();

        // 下推生产工单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.生产主管));
        UIManager.Instance.ShowUI(UIManager.UIType.生产工单);
        manager.SetQuoteFormData(formData1, formData2, null);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.生产主管);
        
        yield return manager.WaitForPushButtonClick();
        yield return WaitForSeconds(0.5f);
        UIManager.Instance.HideUI(UIManager.UIType.生产工单);
        FlowStepTracker.CompleteStep();
    }

    private IEnumerator ProcessMaterialList(DemandManager.CustomerDemand demand, string workOrderNo, string currentTime)
    {
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.生产主管));
        
        var currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.生产用料清单);
        UIManager.Instance.ShowUI(UIManager.UIType.生产用料清单);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.生产主管);
        
        var manager = currentUI.GetComponent<FinishedProductOutboundManager>();
        yield return manager.WaitForFillButtonClick();
        
        currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        string[] formData1 = new string[] {
            "BOM-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999),
            workOrderNo, "AC-" + Random.Range(100, 999), "空气压缩机", "型号 D",
            demand.airCompressorCount.ToString()
        };
        
        string[] formData2 = new string[] { "生产主管", currentTime, "生产主管", currentTime, "", "" };
        
        string[][] tableData = new string[][] {
            new string[] {"PM001", "螺杆主机", "SK-100", demand.airCompressorCount.ToString(), "台"},
            new string[] {"PM002", "电机", "M-200", demand.airCompressorCount.ToString(), "台"},
            new string[] {"PM003", "气缸", "CY-300", demand.airCompressorCount.ToString(), "个"},
            new string[] {"PM004", "油气分离器", "SEP-400", demand.airCompressorCount.ToString(), "个"},
            new string[] {"PM005", "控制面板", "CTRL-500", demand.airCompressorCount.ToString(), "套"}
        };
        
        manager.SetQuoteFormData(formData1, formData2, tableData);
        yield return manager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.生产用料清单);
        FlowStepTracker.CompleteStep();

        // 审核
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.生产主管));
        UIManager.Instance.ShowUI(UIManager.UIType.生产用料清单);
        manager.SetQuoteFormData(formData1, formData2, tableData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.生产主管);
        
        yield return manager.WaitForApproveButtonClick();
        formData2[4] = "生产主管";
        formData2[5] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        manager.SetQuoteFormData(formData1, formData2, tableData);
        UIManager.Instance.HideUI(UIManager.UIType.生产用料清单);
        FlowStepTracker.CompleteStep();

        // 下推
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.生产主管));
        UIManager.Instance.ShowUI(UIManager.UIType.生产用料清单);
        manager.SetQuoteFormData(formData1, formData2, tableData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.生产主管);
        
        yield return manager.WaitForPushButtonClick();
        yield return WaitForSeconds(0.5f);
        UIManager.Instance.HideUI(UIManager.UIType.生产用料清单);
        FlowStepTracker.CompleteStep();
        
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.领料员));
    }

    private IEnumerator ProcessMaterialRequest(DemandManager.CustomerDemand demand, string workOrderNo, string currentTime)
    {
        var currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.生产用料申请单);
        UIManager.Instance.ShowUI(UIManager.UIType.生产用料申请单);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.领料员);
        
        var manager = currentUI.GetComponent<FinishedProductOutboundManager>();
        yield return manager.WaitForFillButtonClick();
        
        currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        string[] formData1 = new string[] {
            "REQ-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999),
            currentTime, workOrderNo, "车间 A", "领料员", "已填写", "无"
        };
        
        string[] formData2 = new string[] { "领料员", currentTime, "领料员", currentTime, "", "" };
        
        string[][] tableData = new string[][] {
            new string[] {"PM001", "螺杆主机", "SK-100", demand.airCompressorCount.ToString(), "台"},
            new string[] {"PM002", "电机", "M-200", demand.airCompressorCount.ToString(), "台"},
            new string[] {"PM003", "气缸", "CY-300", demand.airCompressorCount.ToString(), "个"},
            new string[] {"PM004", "油气分离器", "SEP-400", demand.airCompressorCount.ToString(), "个"},
            new string[] {"PM005", "控制面板", "CTRL-500", demand.airCompressorCount.ToString(), "套"}
        };
        
        manager.SetQuoteFormData(formData1, formData2, tableData);
        yield return manager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.生产用料申请单);
        FlowStepTracker.CompleteStep();
        
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.生产主管));

        // 审核
        UIManager.Instance.ShowUI(UIManager.UIType.生产用料申请单);
        manager.SetQuoteFormData(formData1, formData2, tableData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.生产主管);
        
        yield return manager.WaitForApproveButtonClick();
        formData2[4] = "生产主管";
        formData2[5] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        manager.SetQuoteFormData(formData1, formData2, tableData);
        UIManager.Instance.HideUI(UIManager.UIType.生产用料申请单);
        FlowStepTracker.CompleteStep();
        
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.领料员));

        // 下推
        UIManager.Instance.ShowUI(UIManager.UIType.生产用料申请单);
        manager.SetQuoteFormData(formData1, formData2, tableData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.领料员);
        
        yield return manager.WaitForPushButtonClick();
        yield return WaitForSeconds(0.5f);
        UIManager.Instance.HideUI(UIManager.UIType.生产用料申请单);
        FlowStepTracker.CompleteStep();
    }
    
    public override void StartFlow()
    {
        Debug.Log("生产流程开始");
        base.StartFlow();
        this.InitTaskUI();
    }
    
    public override void StopFlow()
    {
        Debug.Log("生产流程停止");
        base.StopFlow();
    }
    
    protected override void FinishFlow()
    {
        Debug.Log("生产流程结束");
        base.FinishFlow();
    }
}
