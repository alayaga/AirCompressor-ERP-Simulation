using UnityEngine;
using System.Collections;

/// <summary>
/// 合同处理步骤：封装原 ProcessContract 逻辑
/// </summary>
public class ContractStep : FlowStepBase
{
    private DemandManager.CustomerDemand _demand;

    public ContractStep(DemandManager.CustomerDemand demand)
        : base("合同流程") => _demand = demand;

    public override IEnumerator Execute(FlowContext context)
    {
        OnEnter(context);

        // 复用原 ProcessContract 逻辑
        var currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.通用UI);
        UIManager.Instance.ShowUI(UIManager.UIType.通用UI);

        // 1. 初始化空白模板（只定义一次）
        string contractContent = "客户名称：    \n\n产品信息：         \n空压机数量：     台\n " +
                                 "单价：¥          /台\n总价：¥          \n\n " +
                                 "交货条款：        \n付款方式：        \n质量保证：        \n违约责任：         ";

        currentUI.GetComponent<GeneralUIScript>().SetAllTexts("销售合同", contractContent);
        currentUI.GetComponent<GeneralUIScript>().ShowUIMode(UIFormMode.Fill);
        yield return currentUI.GetComponent<GeneralUIScript>().WaitForFillButtonClick();

        // 2. 填充实际数据（使用 _demand，不是 demand）
        contractContent = $"客户名称：{_demand.customerName}\n\n产品信息：\n空压机数量：{_demand.airCompressorCount}台\n " +
                          $"单价：¥{_demand.unitPrice:N2}/台\n总价：¥{_demand.airCompressorCount * _demand.unitPrice:N2}\n\n " +
                          "交货条款：订单确认后 15 个工作日内送达指定地点\n付款方式：30% 预付款，70% 到货验收后 7 日内付清\n " +
                          "质量保证：整机保修一年，核心部件保修两年\n违约责任：按《合同法》相关规定执行";

        currentUI.GetComponent<GeneralUIScript>().SetContentText(contractContent);
        currentUI.GetComponent<GeneralUIScript>().SetAllUIVisibility(false);
        yield return new WaitForSeconds(1f);
        UIManager.Instance.HideUI(UIManager.UIType.通用UI);
        FlowStepTracker.CompleteStep();

        // 审核阶段
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.销售主管));
        UIManager.Instance.ShowUI(UIManager.UIType.通用UI);
        currentUI.GetComponent<GeneralUIScript>().ShowUIMode(UIFormMode.Approve);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.销售主管);
        yield return currentUI.GetComponent<GeneralUIScript>().WaitForApproveButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.通用UI);
        FlowStepTracker.CompleteStep();

        // 下推阶段
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.销售员));
        UIManager.Instance.ShowUI(UIManager.UIType.通用UI);
        currentUI.GetComponent<GeneralUIScript>().ShowUIMode(UIFormMode.PushDown);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.销售员);
        yield return currentUI.GetComponent<GeneralUIScript>().WaitForForwardButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.通用UI);
        FlowStepTracker.CompleteStep();

        OnExit(context);
    }
}