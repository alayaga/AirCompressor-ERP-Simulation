using UnityEngine;
using System.Collections;

// 采购流程类 - 继承自FlowBase
public class PurchaseFlow : FlowBase
{
    #region 构造方法
    // 构造函数
    public PurchaseFlow() : base()
    {
    }
    #endregion

    #region 重写方法
    // 重写流程协程 - 实现采购部分的具体流程逻辑
    protected override IEnumerator FlowCoroutine()
    {
        Debug.Log("采购流程协程开始执行");

        GameObject currentUI;
        
        // ========== 库存检查流程 - 使用测试UI显示 ==========
        // 1. 仓管员仓管员【查看】生产领料单 - 核实库存
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓管员));
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.测试UI);
        UIManager.Instance.ShowUI(UIManager.UIType.测试UI);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.仓管员);
        
        var testUIScript = currentUI.GetComponent<TestUIScript>();
        if (testUIScript != null)
        {
            // 显示生产领料单信息
            string displayInfo = "操作: 查看生产领料单\n" +
                                "人员: 仓管员\n" +
                                "日期: " + System.DateTime.Now.ToString("yyyy-MM-dd") + "\n\n" +
                                "查看内容:\n" +
                                "- 螺杆主机 (PM001)\n" +
                                "- 电机 (PM002)\n" +
                                "- 气缸 (PM003)";
            
            testUIScript.SetTexts(displayInfo, "确认查看");
            yield return testUIScript.WaitForButtonClick();
        }
        
        UIManager.Instance.HideUI(UIManager.UIType.测试UI);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // 2. 库存不足提示 - 使用测试UI显示
        Debug.Log("====== 库存状态: 库存不足 ======");
        
        // 直接显示库存不足信息，不使用库存系统进行实际检查
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓管员));
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.测试UI);
        UIManager.Instance.ShowUI(UIManager.UIType.测试UI);
        
        testUIScript = currentUI.GetComponent<TestUIScript>();
        if (testUIScript != null)
        {
            // 显示库存不足信息
            string inventoryInfo = "库存检查结果:\n\n" +
                                  "状态: 库存不足\n" +
                                  "检查人员: 仓管员\n" +
                                  "检查日期: " + System.DateTime.Now.ToString("yyyy-MM-dd") + "\n\n" +
                                  "需要采购以下物料:\n" +
                                  "- 螺杆主机\n" +
                                  "- 电机\n" +
                                  "- 气缸";
            
            testUIScript.SetTexts(inventoryInfo, "确认库存不足");
            yield return testUIScript.WaitForButtonClick();
        }
        
        UIManager.Instance.HideUI(UIManager.UIType.测试UI);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // ========== 物料需求单流程 ==========
        // 3. 仓管员仓管员【填】物料需求单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓管员));
        
        // 获取并显示物料需求单UI
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.物料需求单);
        UIManager.Instance.ShowUI(UIManager.UIType.物料需求单);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.仓管员);
        
        // 获取组件并等待填写按钮点击
        var materialDemandManager = currentUI.GetComponent<FinishedProductOutboundManager>();
        yield return materialDemandManager.WaitForFillButtonClick();
        
        // 填写按钮点击后，生成并设置物料需求单数据
        string currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // 第一组参数：物料需求单基本信息（需求单编号、需求部门、需求人、需求日期、紧急程度、用途）
        string[] demandFormData1 = new string[] {
            "MRD-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999), // 需求单编号
            System.DateTime.Now.ToString("yyyy-MM-dd"), // 需求日期
            "AAA",
            "编码 B",
            "空气压缩机",
            "型号 A", // 需求部门
            "10", // 需求人
        };
        
        // 第二组参数：人员时间信息（创建人、创建时间、修改人、修改时间、审核人、审核时间）
        string[] demandFormData2 = new string[] {
            "仓管员", // 创建人
            currentTime, // 创建时间
            "仓管员", // 修改人
            currentTime, // 修改时间
            "", // 审核人（待审核）
            "" // 审核时间（待审核）
        };
        
        // 第三组参数：物料明细信息（物料编码、物料名称、规格型号、需求数量、库存单位）
        // 检查InventoryManager获取库存不足信息
        string[][] demandTableData = new string[][] {
            new string[] {"PM001", "螺杆主机", "SK-100","台", "10", "10","0","10","无"},
            new string[] {"PM002", "电机", "M-200","台", "10", "10","0","10","无"},
            new string[] {"PM003", "气缸", "CY-300","台", "10", "10","0","10","无"},
            new string[] {"PM004", "油气分离器", "SEP-400","台", "10", "10","0","10","无"},
            new string[] {"PM005", "控制面板", "CTRL-500", "台", "10", "10", "0", "10", "无" }
        };
        
        // 设置表单数据
        materialDemandManager.SetQuoteFormData(demandFormData1, demandFormData2, demandTableData);
        
        // 等待提交按钮点击
        yield return materialDemandManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.物料需求单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // 4. 仓库主管仓库主管【审核】物料需求单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓库主管));
        UIManager.Instance.ShowUI(UIManager.UIType.物料需求单);
        materialDemandManager.SetQuoteFormData(demandFormData1, demandFormData2, demandTableData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.仓库主管);
        
        // 等待审核按钮点击
        yield return materialDemandManager.WaitForApproveButtonClick();
        
        // 更新审核信息
        demandFormData2[4] = "仓库主管"; // 审核人
        demandFormData2[5] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 审核时间
        materialDemandManager.SetQuoteFormData(demandFormData1, demandFormData2, demandTableData);
        
        UIManager.Instance.HideUI(UIManager.UIType.物料需求单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // 5. 仓管员仓管员【下推】物料需求单（到）采购需求申请单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓管员));
        UIManager.Instance.ShowUI(UIManager.UIType.物料需求单);
        materialDemandManager.SetQuoteFormData(demandFormData1, demandFormData2, demandTableData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.仓管员);
        
        // 等待下推按钮点击
        yield return materialDemandManager.WaitForPushButtonClick();
        
        UIManager.Instance.HideUI(UIManager.UIType.物料需求单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // ========== 采购需求申请单流程 ==========
        // 6. 采购员采购员【填】采购需求申请单（紧急程度为急）
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购员));
        
        // 获取并显示采购需求申请单UI
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.采购需求申请单);
        UIManager.Instance.ShowUI(UIManager.UIType.采购需求申请单);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.采购员);
        
        // 获取组件并等待填写按钮点击
        var purchaseReqManager = currentUI.GetComponent<FinishedProductOutboundManager>();
        yield return purchaseReqManager.WaitForFillButtonClick();
        
        // 填写按钮点击后，生成并设置采购需求申请单数据
        currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // 第一组参数：采购需求申请单基本信息（申请编号、申请部门、申请人、需求日期、紧急程度、关联需求单）
        string[] purchaseReqFormData1 = new string[] {
            "PRQ-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999), // 申请编号
            currentTime,
            "采购部", // 申请部门
            "采购员", // 申请人
            "已填写", // 需求日期
            "急", // 紧急程度
        };
        
        // 第二组参数：人员时间信息（创建人、创建时间、修改人、修改时间、审核人、审核时间）
        string[] purchaseReqFormData2 = new string[] {
            "采购员", // 创建人
            currentTime, // 创建时间
            "采购员", // 修改人
            currentTime, // 修改时间
            "", // 审核人（待审核）
            "" // 审核时间（待审核）
        };
        
        // 第三组参数：物料明细信息（与物料需求单保持一致）
        string[][] purchaseReqTableData = demandTableData;
        
        // 设置表单数据
        purchaseReqManager.SetQuoteFormData(purchaseReqFormData1, purchaseReqFormData2, purchaseReqTableData);
        
        // 等待提交按钮点击
        yield return purchaseReqManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.采购需求申请单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // 7. 采购主管采购主管【审核】采购需求申请单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购主管));
        UIManager.Instance.ShowUI(UIManager.UIType.采购需求申请单);
        purchaseReqManager.SetQuoteFormData(purchaseReqFormData1, purchaseReqFormData2, purchaseReqTableData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.采购主管);
        
        // 等待审核按钮点击
        yield return purchaseReqManager.WaitForApproveButtonClick();
        
        // 更新审核信息
        purchaseReqFormData2[4] = "采购主管"; // 审核人
        purchaseReqFormData2[5] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 审核时间
        purchaseReqManager.SetQuoteFormData(purchaseReqFormData1, purchaseReqFormData2, purchaseReqTableData);
        
        UIManager.Instance.HideUI(UIManager.UIType.采购需求申请单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // 8. 采购员采购员【下推】采购需求申请单（到）采购申请单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购员));
        UIManager.Instance.ShowUI(UIManager.UIType.采购需求申请单);
        purchaseReqManager.SetQuoteFormData(purchaseReqFormData1, purchaseReqFormData2, purchaseReqTableData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.采购员);
        
        // 等待下推按钮点击
        yield return purchaseReqManager.WaitForPushButtonClick();
        
        UIManager.Instance.HideUI(UIManager.UIType.采购需求申请单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // ========== 采购申请单流程 ==========
        // 9. 采购员采购员【填】采购申请单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购员));
        
        // 获取并显示采购申请单UI
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.采购申请单);
        UIManager.Instance.ShowUI(UIManager.UIType.采购申请单);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.采购员);
        
        // 获取组件并等待填写按钮点击
        var purchaseAppManager = currentUI.GetComponent<FinishedProductOutboundManager>();
        yield return purchaseAppManager.WaitForFillButtonClick();
        
        // 填写按钮点击后，生成并设置采购申请单数据
        currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // 第一组参数：采购申请单基本信息（申请编号、申请部门、申请人、申请日期、紧急程度、关联需求申请单）
        string[] purchaseAppFormData1 = new string[] {
            "PAP-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999), // 申请编号
            "采购部", // 申请部门
            "采购员", // 申请人
            System.DateTime.Now.ToString("yyyy-MM-dd"), // 申请日期
            "急", // 紧急程度
            purchaseReqFormData1[0] // 关联采购需求申请单编号
        };
        
        // 第二组参数：人员时间信息（创建人、创建时间、修改人、修改时间、审核人、审核时间）
        string[] purchaseAppFormData2 = new string[] {
            "采购员", // 创建人
            currentTime, // 创建时间
            "采购员", // 修改人
            currentTime, // 修改时间
            "", // 审核人（待审核）
            "" // 审核时间（待审核）
        };
        
        // 第三组参数：物料明细信息（与采购需求申请单保持一致）
        string[][] purchaseAppTableData = purchaseReqTableData;
        
        // 设置表单数据
        purchaseAppManager.SetQuoteFormData(purchaseAppFormData1, purchaseAppFormData2, purchaseAppTableData);
        
        // 等待提交按钮点击
        yield return purchaseAppManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.采购申请单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // 10. 采购主管采购主管【审核】采购申请单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购主管));
        UIManager.Instance.ShowUI(UIManager.UIType.采购申请单);
        purchaseAppManager.SetQuoteFormData(purchaseAppFormData1, purchaseAppFormData2, purchaseAppTableData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.采购主管);
        
        // 等待审核按钮点击
        yield return purchaseAppManager.WaitForApproveButtonClick();
        
        // 更新审核信息
        purchaseAppFormData2[4] = "采购主管"; // 审核人
        purchaseAppFormData2[5] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 审核时间
        purchaseAppManager.SetQuoteFormData(purchaseAppFormData1, purchaseAppFormData2, purchaseAppTableData);
        
        UIManager.Instance.HideUI(UIManager.UIType.采购申请单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // 11. 采购员采购员【下推】采购申请单（到）采购订单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购员));
        UIManager.Instance.ShowUI(UIManager.UIType.采购申请单);
        purchaseAppManager.SetQuoteFormData(purchaseAppFormData1, purchaseAppFormData2, purchaseAppTableData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.采购员);
        
        // 等待下推按钮点击
        yield return purchaseAppManager.WaitForPushButtonClick();
        
        UIManager.Instance.HideUI(UIManager.UIType.采购申请单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // ========== 采购订单流程 ==========
        // 12. 采购员采购员【填】采购订单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购员));
        
        // 获取并显示采购订单UI
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.采购订单);
        UIManager.Instance.ShowUI(UIManager.UIType.采购订单);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.采购员);
        
        // 获取组件并等待填写按钮点击
        var purchaseOrderManager = currentUI.GetComponent<FinishedProductOutboundManager>();
        yield return purchaseOrderManager.WaitForFillButtonClick();
        
        // 填写按钮点击后，生成并设置采购订单数据
        currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // 第一组参数：采购订单基本信息（订单编号、供应商名称、采购员、订单日期、预计到货日期、关联申请单）
        string[] orderFormData1 = new string[] {
            "PO-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999), // 订单编号
            currentTime,
            "采购部",
            "采购员", // 采购员
            "机械零部件供应商", // 供应商名称
            "已填写",
            "无"
        };
        
        // 第二组参数：人员时间信息（创建人、创建时间、修改人、修改时间、审核人、审核时间）
        string[] orderFormData2 = new string[] {
            "采购员", // 创建人
            currentTime, // 创建时间
            "采购员", // 修改人
            currentTime, // 修改时间
            "", // 审核人（待审核）
            "" // 审核时间（待审核）
        };
        
        // 第三组参数：物料明细信息（与采购申请单保持一致）
        string[][] orderTableData = purchaseAppTableData;
        
        // 设置表单数据
        purchaseOrderManager.SetQuoteFormData(orderFormData1, orderFormData2, orderTableData);
        
        // 等待提交按钮点击
        yield return purchaseOrderManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.采购订单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // 13. 采购主管采购主管【审核】采购订单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购主管));
        UIManager.Instance.ShowUI(UIManager.UIType.采购订单);
        purchaseOrderManager.SetQuoteFormData(orderFormData1, orderFormData2, orderTableData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.采购主管);
        
        // 等待审核按钮点击
        yield return purchaseOrderManager.WaitForApproveButtonClick();
        
        // 更新审核信息
        orderFormData2[4] = "采购主管"; // 审核人
        orderFormData2[5] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 审核时间
        purchaseOrderManager.SetQuoteFormData(orderFormData1, orderFormData2, orderTableData);
        
        UIManager.Instance.HideUI(UIManager.UIType.采购订单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // 14. 采购订单【采购过程】
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购员));

        //卡车进站
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.卡车视角);
        yield return ObjectManager.Instance.GetObject(ObjectManager.ObjectType.卡车).GetComponent<ObjectMovementController>()?.MoveToPositionsSequentially();

        //卸货过程
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.进货位置);
        yield return ObjectManager.Instance.GetObject(ObjectManager.ObjectType.原料仓储).GetComponent<GameObjectSequenceController>()?.ShowObjectsSequentially();

        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.采购员);
        ObjectManager.Instance.GetObject(ObjectManager.ObjectType.卡车).SetActive(false);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // 15. 采购员采购员【下推】采购订单（到）收料通知单（可重复下推）
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.采购订单);
        UIManager.Instance.ShowUI(UIManager.UIType.采购订单);
        
        // 重新获取组件
        purchaseOrderManager = currentUI.GetComponent<FinishedProductOutboundManager>();
        purchaseOrderManager.SetQuoteFormData(orderFormData1, orderFormData2, orderTableData);

        // 等待下推按钮点击
        yield return purchaseOrderManager.WaitForPushButtonClick();
        
        UIManager.Instance.HideUI(UIManager.UIType.采购订单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // ========== 收料通知单流程 ==========
        // 16. 采购员采购员【填】收料通知单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购员));
        
        // 获取并显示收料通知单UI
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.收料通知单);
        UIManager.Instance.ShowUI(UIManager.UIType.收料通知单);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.采购员);
        
        // 获取组件并等待填写按钮点击
        var receiveNoticeManager = currentUI.GetComponent<FinishedProductOutboundManager>();
        yield return receiveNoticeManager.WaitForFillButtonClick();
        
        // 填写按钮点击后，生成并设置收料通知单数据
        currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // 第一组参数：收料通知单基本信息（通知单编号、供应商名称、预计到货日期、通知日期、通知人、关联订单）
        string[] noticeFormData1 = new string[] {
            "RN-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999), // 通知单编号
            "机械零部件供应商", // 供应商名称
            System.DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"), // 预计到货日期
            System.DateTime.Now.ToString("yyyy-MM-dd"), // 通知日期
            "采购员", // 通知人
            orderFormData1[0] // 关联采购订单编号
        };
        
        // 第二组参数：人员时间信息（创建人、创建时间、修改人、修改时间、审核人、审核时间）
        string[] noticeFormData2 = new string[] {
            "采购员", // 创建人
            currentTime, // 创建时间
            "采购员", // 修改人
            currentTime, // 修改时间
            "", // 审核人（待审核）
            "" // 审核时间（待审核）
        };
        
        // 第三组参数：物料明细信息（与采购订单保持一致）
        string[][] noticeTableData = orderTableData;
        
        // 设置表单数据
        receiveNoticeManager.SetQuoteFormData(noticeFormData1, noticeFormData2, noticeTableData);
        
        // 等待提交按钮点击
        yield return receiveNoticeManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.收料通知单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // 17. 采购主管采购主管【审核】收料通知单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购主管));
        UIManager.Instance.ShowUI(UIManager.UIType.收料通知单);
        receiveNoticeManager.SetQuoteFormData(noticeFormData1, noticeFormData2, noticeTableData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.采购主管);
        
        // 等待审核按钮点击
        yield return receiveNoticeManager.WaitForApproveButtonClick();
        
        // 更新审核信息
        noticeFormData2[4] = "采购主管"; // 审核人
        noticeFormData2[5] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 审核时间
        receiveNoticeManager.SetQuoteFormData(noticeFormData1, noticeFormData2, noticeTableData);
        
        UIManager.Instance.HideUI(UIManager.UIType.收料通知单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // 18. 采购员采购员【下推】收料通知单（到）采购入库单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购员));
        UIManager.Instance.ShowUI(UIManager.UIType.收料通知单);
        receiveNoticeManager.SetQuoteFormData(noticeFormData1, noticeFormData2, noticeTableData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.采购员);
        
        // 等待下推按钮点击
        yield return receiveNoticeManager.WaitForPushButtonClick();
        
        UIManager.Instance.HideUI(UIManager.UIType.收料通知单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // ========== 质检流程 ==========
        // 19. 质检员质检员【质检】
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.质检员));
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.测试UI);
        UIManager.Instance.ShowUI(UIManager.UIType.测试UI);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.质检员);

        currentUI.GetComponent<TestUIScript>().SetTexts("准备进行质量检查","质检");

        yield return currentUI.GetComponent<TestUIScript>().WaitForButtonClick();
        
        UIManager.Instance.HideUI(UIManager.UIType.测试UI);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // ========== 采购入库单流程 ==========
        // 20. 仓管员仓管员【填】采购入库单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓管员));
        
        // 获取并显示采购入库单UI
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.采购入库单);
        UIManager.Instance.ShowUI(UIManager.UIType.采购入库单);

        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.仓管员);
        
        // 获取组件并等待填写按钮点击
        var inboundManager = currentUI.GetComponent<FinishedProductOutboundManager>();
        yield return inboundManager.WaitForFillButtonClick();
        
        // 填写按钮点击后，生成并设置采购入库单数据
        currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // 第一组参数：采购入库单基本信息（入库单编号、供应商名称、入库仓库、入库日期、入库人、关联收料通知单）
        string[] inboundFormData1 = new string[] {
            "IN-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999), // 入库单编号
            "机械零部件供应商", // 供应商名称
            "主仓库", // 入库仓库
            System.DateTime.Now.ToString("yyyy-MM-dd"), // 入库日期
            "仓管员", // 入库人
            noticeFormData1[0] // 关联收料通知单编号
        };
        
        // 第二组参数：人员时间信息（创建人、创建时间、修改人、修改时间、审核人、审核时间）
        string[] inboundFormData2 = new string[] {
            "仓管员", // 创建人
            currentTime, // 创建时间
            "仓管员", // 修改人
            currentTime, // 修改时间
            "", // 审核人（待审核）
            "" // 审核时间（待审核）
        };
        
        // 第三组参数：物料明细信息（与收料通知单保持一致，物料编码、物料名称、规格型号、入库数量、库存单位）
        string[][] inboundTableData = noticeTableData;
        
        // 设置表单数据
        inboundManager.SetQuoteFormData(inboundFormData1, inboundFormData2, inboundTableData);
        
        // 等待提交按钮点击
        yield return inboundManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.采购入库单);
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(0.5f);

        // 21. 仓库主管仓库主管【审核】采购入库单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓库主管));
        UIManager.Instance.ShowUI(UIManager.UIType.采购入库单);
        inboundManager.SetQuoteFormData(inboundFormData1, inboundFormData2, inboundTableData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.仓库主管);
        
        // 等待审核按钮点击
        yield return inboundManager.WaitForApproveButtonClick();
        
        // 更新审核信息
        inboundFormData2[4] = "仓库主管"; // 审核人
        inboundFormData2[5] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 审核时间
        inboundManager.SetQuoteFormData(inboundFormData1, inboundFormData2, inboundTableData);
        
        UIManager.Instance.HideUI(UIManager.UIType.采购入库单);
        FlowStepTracker.CompleteStep();
        
        // 审核通过后，更新库存管理系统
        // 根据入库单明细，添加对应零件到库存
        Debug.Log("====== 更新库存管理系统 ======");
        int inboundQty = 10; // 从表格数据获取入库数量
        
        // 添加所有零件的库存
        InventoryManager.Instance.AddAllInventory(inboundQty);
        Debug.Log($"采购入库：所有零件库存各增加{inboundQty}");
        
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("采购流程正常结束");
        
        // 流程结束
        FinishFlow();
    }
    
    // 可以根据需要重写其他方法
    public override void StartFlow()
    {
        Debug.Log("采购流程开始");
        base.StartFlow();
        
        // 初始化任务UI
        this.InitTaskUI();
    }
    
    public override void StopFlow()
    {
        Debug.Log("采购流程停止");
        base.StopFlow();
    }
    
    protected override void FinishFlow()
    {
        Debug.Log("采购流程完整结束");
        base.FinishFlow();
    }
    #endregion
    
    #region 私有方法
    // 这里可以添加采购流程特有的私有方法
    
    /// <summary>
    /// 执行库存检查（已弃用 - 已在主流程中使用正式UI实现）
    /// </summary>
    private IEnumerator CheckInventory(string materialName)
    {
        // 此方法已弃用，库存检查已在主流程中使用库存检查单UI实现
        yield return null;
    }
    
    /// <summary>
    /// 执行质检流程（已弃用 - 已在主流程中使用正式UI实现）
    /// </summary>
    private IEnumerator ExecuteQualityCheck(string itemName)
    {
        // 此方法已弃用，质检流程已在主流程中使用来料检验单UI实现
        yield return null;
    }
    
    /// <summary>
    /// 执行审批流程（已弃用 - 已在主流程中使用正式UI实现）
    /// </summary>
    private IEnumerator ExecuteApprovalProcess(string workerName, string documentName, 
        PositionManager.PositionType workerPos, string approverName, PositionManager.PositionType approverPos)
    {
        // 此方法已弃用，审批流程已在主流程中使用各类单据UI实现
        yield return null;
    }
    #endregion
}

