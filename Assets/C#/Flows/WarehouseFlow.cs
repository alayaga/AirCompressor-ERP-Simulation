using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// 仓库流程
/// </summary>
public class WarehouseFlow : FlowBase
{
    private GameObject _playerObject;
    
    protected override IEnumerator FlowCoroutine()
    {        
        Debug.Log("仓库流程开始");

        // 完工入库单流程
        yield return ProcessInboundOrder();
        
        // 销售出库单流程
        yield return ProcessOutboundOrder();
        
        // 流程完成提示
        yield return ShowCompletionMessage();
        
        FinishFlow();
    }

    private IEnumerator ProcessInboundOrder()
    {
        var currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.完工入库单);
        
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓管员));
        TeleportPlayerToPosition(PositionManager.PositionType.仓管员);
        UIManager.Instance.ShowUI(UIManager.UIType.完工入库单);
        
        var manager = currentUI.GetComponent<FinishedProductOutboundManager>();
        
        string[] inputBoxContents = new string[] {
            "业务日期: 2025-11-21", "库管员: 库管员 B", "备注: 入库测试单据 2"
        };
        
        string[] inputBoxContents2 = new string[] {
            "创建人: 李四", "创建时间: 2025-11-21 09:00:00",
            "修改人: 李四", "修改时间: 2025-11-21 10:00:00",
            "审核人: 王五", "审核时间: 2025-11-21 11:00:00"
        };
        
        string[][] tableRowData = new string[][] {
            new string[] { "MAT008", "配件 X", "M1", "个", "25", "仓库 C", "合格", "备注 X" },
            new string[] { "MAT009", "配件 Y", "M2", "箱", "18", "仓库 D", "待检", "备注 Y" },
            new string[] { "MAT010", "配件 Z", "M3", "套", "32", "仓库 E", "合格", "备注 Z" }
        };
        
        yield return manager.WaitForFillButtonClick();
        manager.SetQuoteFormData(inputBoxContents, inputBoxContents2, tableRowData);
        yield return manager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.完工入库单);
        FlowStepTracker.CompleteStep();

        // 审核
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓库主管));
        TeleportPlayerToPosition(PositionManager.PositionType.仓库主管);
        UIManager.Instance.ShowUI(UIManager.UIType.完工入库单);
        manager.SetQuoteFormData(inputBoxContents, inputBoxContents2, tableRowData);
        yield return manager.WaitForApproveButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.完工入库单);
        FlowStepTracker.CompleteStep();
    }

    private IEnumerator ProcessOutboundOrder()
    {
        var currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.销售出库单);

        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓管员));
        TeleportPlayerToPosition(PositionManager.PositionType.仓管员);
        UIManager.Instance.ShowUI(UIManager.UIType.销售出库单);
        
        var manager = currentUI.GetComponent<FinishedProductOutboundManager>();
        
        string[] outboundInputBoxContents = new string[] {
            "单据编号: CKDJ-251123-005", "业务日期: 2025-11-23", "收货客户: 客户 D",
            "销售员: 销售员 D", "库管员: 库管员 D", "单据状态: 已审核", "备注: 正式销售出库单"
        };
        
        string[] outboundInputBoxContents2 = new string[] {
            "创建人: 周八", "创建时间: 2025-11-23 09:30:00",
            "修改人: 吴九", "修改时间: 2025-11-23 10:15:00",
            "审核人: 郑十", "审核时间: 2025-11-23 11:40:00"
        };
        
        string[][] outboundTableRowData = new string[][] {
            new string[] { "MAT013", "仪器 X", "P1", "台", "3", "仓库 H", "出库备注 3" },
            new string[] { "MAT014", "仪器 Y", "P2", "套", "6", "仓库 I", "出库备注 4" },
            new string[] { "MAT015", "仪器 Z", "P3", "个", "12", "仓库 J", "出库备注 5" }
        };
        
        yield return manager.WaitForFillButtonClick();
        manager.SetQuoteFormData(outboundInputBoxContents, outboundInputBoxContents2, outboundTableRowData);
        yield return manager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.销售出库单);
        FlowStepTracker.CompleteStep();

        // 审核
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓库主管));
        TeleportPlayerToPosition(PositionManager.PositionType.仓库主管);
        UIManager.Instance.ShowUI(UIManager.UIType.销售出库单);
        manager.SetQuoteFormData(outboundInputBoxContents, outboundInputBoxContents2, outboundTableRowData);
        yield return manager.WaitForApproveButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.销售出库单);
        FlowStepTracker.CompleteStep();
    }

    private IEnumerator ShowCompletionMessage()
    {
        yield return new WaitForSeconds(1f);
        
        var currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.测试UI);
        UIManager.Instance.ShowUI(UIManager.UIType.测试UI);
        
        var testUI = currentUI.GetComponent<TestUIScript>();
        testUI.SetTexts("仓库流程完成", "完工入库单流程已完成\n销售出库单流程已完成\n\n所有仓库相关单据已处理完毕");
        
        yield return testUI.WaitForButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.测试UI);
        FlowStepTracker.CompleteStep();
    }
    
    private void TeleportPlayerToPosition(PositionManager.PositionType positionType)
    {        
        if (_playerObject == null)
            _playerObject = ObjectManager.Instance?.GetObject(ObjectManager.ObjectType.Player);
        
        if (_playerObject != null)
            PositionManager.Instance?.SetObjectToPosition(_playerObject, positionType);
    }
    
    public override void StartFlow()
    {        
        Debug.Log("仓库流程开始");
        base.StartFlow();
        this.InitTaskUI();
    }
    
    public override void StopFlow()
    {        
        Debug.Log("仓库流程停止");
        base.StopFlow();
    }
    
    protected override void FinishFlow()
    {        
        Debug.Log("仓库流程结束");
        base.FinishFlow();
        SceneManager.LoadScene(0);
    }
}
