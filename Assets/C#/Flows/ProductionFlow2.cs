using UnityEngine;
using System.Collections;

// 生产流程类（部分2） - 继承自FlowBase
public class ProductionFlow2 : FlowBase
{
    #region 构造方法
    // 构造函数
    public ProductionFlow2() : base()
    {    
    }
    #endregion

    #region 重写方法
    // 重写流程协程 - 实现生产流程逻辑
    protected override IEnumerator FlowCoroutine()
    {        
        Debug.Log("生产流程2协程开始执行");

        GameObject currentUI;

        #region 1. 生产领料单流程
        Debug.Log("====== 生产领料单流程开始 ======");
        
        // 1.1 仓管员仓管员【填】生产领料单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓管员));
        // 获取并显示生产领料单UI
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.生产领料单);
        UIManager.Instance.ShowUI(UIManager.UIType.生产领料单);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.仓管员);
        
        // 获取组件并等待填写按钮点击
        var outboundManager = currentUI.GetComponent<FinishedProductOutboundManager>();
        yield return outboundManager.WaitForFillButtonClick();
        
        // 填写按钮点击后，生成并设置生产领料单数据
        string currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // 从DemandManager获取需求信息
        var demand = DemandManager.Instance.GetCurrentDemand();
        string productCode = "AC-" + Random.Range(100, 999);
        
        // 第一组参数：单据基本信息（单据编号、领料日期、领料人、产品编码、仓库、真实领料日期）
        string[] formData1 = new string[] {
            "LLD-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999), // 单据编号
            System.DateTime.Now.ToString("yyyy-MM-dd"), // 领料日期
            "仓管员", // 领料人
            productCode, // 产品编码
            "主仓库", // 仓库
            currentTime // 真实领料日期（当地时间）
        };
        
        // 第二组参数：人员时间信息（创建人、创建时间、修改人、修改时间、业务人、最后操作人、审核时间）
        string[] formData2 = new string[] {
            "仓管员", // 创建人
            currentTime, // 创建时间
            "仓管员", // 修改人
            currentTime, // 修改时间
            "仓管员", // 业务人
            "仓管员", // 最后操作人
            "" // 审核时间（待审核）
        };
        
        // 第三组参数：物料明细信息（物料编码、物料名称、规格型号、库存批号、备注）
        // 与InventoryManager关联
        string batchNo = "LOT" + System.DateTime.Now.ToString("yyyyMMdd");
        string[][] tableData = new string[][] {
            new string[] {"PM001", "螺杆主机", "SK-100", batchNo, "生产领料"},
            new string[] {"PM002", "电机", "M-200", batchNo, "生产领料"},
            new string[] {"PM003", "气缸", "CY-300", batchNo, "生产领料"}
        };
        
        // 设置表单数据
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        
        // 等待提交按钮点击
        yield return outboundManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.生产领料单);
        FlowStepTracker.CompleteStep();
        
        // 1.2 仓库主管仓库主管【审核】生产领料单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓库主管));
        // 再次显示生产领料单UI，显示待审核的单据
        UIManager.Instance.ShowUI(UIManager.UIType.生产领料单);
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.仓库主管);
        
        // 等待审核按钮点击
        yield return outboundManager.WaitForApproveButtonClick();
        
        // 更新审核信息
        formData2[5] = "仓库主管"; // 最后操作人更新为审核人
        formData2[6] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 审核时间
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        
        UIManager.Instance.HideUI(UIManager.UIType.生产领料单);
        FlowStepTracker.CompleteStep();
        
        // 1.3 领料员领料员根据生产领料单【领料】
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.领料员));
        // 显示已审核的生产领料单，供领料员查看
        UIManager.Instance.ShowUI(UIManager.UIType.测试UI);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.领料员);

        // 显示已审核的单据信息
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.测试UI);
        currentUI.GetComponent<TestUIScript>().SetTexts("生产领料单", "领料");

        // 等待提交按钮点击（作为领料确认）
        yield return currentUI.GetComponent<TestUIScript>().WaitForButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.测试UI);
        FlowStepTracker.CompleteStep();

        Debug.Log("====== 生产领料单流程结束 ======");
        #endregion

        #region 2. 工序计划单流程
        Debug.Log("====== 工序计划单流程开始 ======");
        
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.生产主管));

        // 2.1 生产主管生产主管【填】工序计划单（5个工序单独填）
        // 先获取一次UI对象和组件引用
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.工序计划单);
        outboundManager = currentUI.GetComponent<FinishedProductOutboundManager>();
        
        for (int i = 1; i <= 5; i++)
        {
            yield return new WaitForSeconds(1f);
            // 先显示UI
            UIManager.Instance.ShowUI(UIManager.UIType.工序计划单);
            PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.生产主管);
            // 复用之前获取的currentUI和outboundManager
            
            // 生成当前时间
            currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            // 第一组参数：单据基本信息
            formData1 = new string[] {
                "XZGD-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + i.ToString("000"), // 单据编号
                System.DateTime.Now.ToString("yyyy-MM-dd"), // 计划日期
                "WIP-2024-" + System.DateTime.Now.ToString("MMdd"), // 关联工单编号
                "AC-100", // 产品编码
                "空气压缩机", // 产品名称
                "标准型", // 规格型号
                "5", // 生产数量
                "组装车间", // 生产车间
                "计划中", // 单据状态
                "第" + i + "道工序计划" // 备注
            };
            
            // 第二组参数：人员时间信息
            formData2 = new string[] {
                "生产主管", // 创建人
                currentTime, // 创建时间
                "生产主管", // 修改人
                currentTime, // 修改时间
                "", // 审核人（待审核）
                "" // 审核时间（待审核）
            };
            
            // 第三组参数：工序明细信息
            tableData = new string[][] {
                new string[] {"G" + i.ToString("00"), "工序" + i, "工序" + i + "详细描述", "2024-01-" + (10 + i).ToString(), "2024-01-" + (12 + i).ToString(), "张师傅", "工序计划备注"}
            };
            
            // 等待填写按钮点击
            yield return outboundManager.WaitForFillButtonClick();
            // 设置表单数据
            outboundManager.SetQuoteFormData(formData1, formData2, tableData);
            
            // 等待提交按钮点击
            yield return outboundManager.WaitForSubmitButtonClick();
            
            // 隐藏UI
            UIManager.Instance.HideUI(UIManager.UIType.工序计划单);
        }
        FlowStepTracker.CompleteStep();

        // 2.2 生产主管生产主管【审核】工序计划单
        yield return new WaitForSeconds(1f);
        // 先显示UI
        UIManager.Instance.ShowUI(UIManager.UIType.工序计划单);
        // 复用之前获取的currentUI和outboundManager
        
        // 生成当前时间
        currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // 设置审核表单数据
        formData1 = new string[] {
            "XZGD-" + System.DateTime.Now.ToString("yyyyMMdd"), // 单据编号
            System.DateTime.Now.ToString("yyyy-MM-dd"), // 计划日期
            "WIP-2024-" + System.DateTime.Now.ToString("MMdd"), // 关联工单编号
            "AC-100", // 产品编码
            "空气压缩机", // 产品名称
            "标准型", // 规格型号
            "5", // 生产数量
            "组装车间", // 生产车间
            "待审核", // 单据状态
            "全部工序计划审核" // 备注
        };
        
        formData2 = new string[] {
            "生产主管", // 创建人
            currentTime, // 创建时间
            "生产主管", // 修改人
            currentTime, // 修改时间
            "", // 审核人（待审核）
            "" // 审核时间（待审核）
        };
        
        tableData = new string[][] {
            new string[] {"G01-G05", "全部工序", "5道工序计划审核", "2024-01-11", "2024-01-17", "生产主管", "全部工序计划审核"}
        };
        
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        
        // 等待审核按钮点击
        yield return outboundManager.WaitForApproveButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.工序计划单);
        FlowStepTracker.CompleteStep();

        // 2.3 5个班组长【下推】工序计划单（到）5个工序任务（同时进行）
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.五个班组长));
        // 先显示UI
        UIManager.Instance.ShowUI(UIManager.UIType.工序计划单);
        // 复用之前获取的currentUI和outboundManager
        
        // 设置下推表单数据
        formData1 = new string[] {
            "XZGD-" + System.DateTime.Now.ToString("yyyyMMdd"), // 单据编号
            System.DateTime.Now.ToString("yyyy-MM-dd"), // 计划日期
            "WIP-2024-" + System.DateTime.Now.ToString("MMdd"), // 关联工单编号
            "AC-100", // 产品编码
            "空气压缩机", // 产品名称
            "标准型", // 规格型号
            "5", // 生产数量
            "组装车间", // 生产车间
            "已审核", // 单据状态
            "下推至工序任务" // 备注
        };
        
        formData2 = new string[] {
            "生产主管", // 创建人
            currentTime, // 创建时间
            "五位班组长", // 修改人
            currentTime, // 修改时间
            "生产主管", // 审核人
            currentTime // 审核时间
        };
        
        tableData = new string[][] {
            new string[] {"G01-G05", "全部工序", "下推至5个工序任务", "2024-01-11", "2024-01-17", "五位班组长", "下推完成"}
        };
        
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        
        // 等待下推按钮点击
        yield return outboundManager.WaitForPushButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.工序计划单);
        FlowStepTracker.CompleteStep();

        Debug.Log("====== 工序计划单流程结束 ======");
        #endregion

        #region 3. 工序任务流程
        Debug.Log("====== 工序任务流程开始 ======");

        // 3.1 5个班组长【填】工序任务
        yield return new WaitForSeconds(1f);
        UIManager.Instance.ShowUI(UIManager.UIType.工序计划单);
        // 复用之前获取的currentUI和outboundManager
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.五个班组长);
        
        // 生成工序任务数据
        currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        formData1 = new string[] {
            "GXRW-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999),
            System.DateTime.Now.ToString("yyyy-MM-dd"),
            "WIP-2024-" + System.DateTime.Now.ToString("MMdd"),
            "AC-100",
            "空气压缩机",
            "组装车间",
            "5个工序任务",
            "待审核",
            "五位班组长填写"
        };
        
        formData2 = new string[] {
            "五位班组长",
            currentTime,
            "五位班组长",
            currentTime,
            "",
            ""
        };
        
        tableData = new string[][] {
            new string[] {"G01", "工序1", "任务描述1", "张师傅", "2024-01-11", "进行中"},
            new string[] {"G02", "工序2", "任务描述2", "李师傅", "2024-01-12", "进行中"},
            new string[] {"G03", "工序3", "任务描述3", "王师傅", "2024-01-13", "进行中"},
            new string[] {"G04", "工序4", "任务描述4", "赵师傅", "2024-01-14", "进行中"},
            new string[] {"G05", "工序5", "任务描述5", "刘师傅", "2024-01-15", "进行中"}
        };
        
        yield return outboundManager.WaitForFillButtonClick();
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return outboundManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.工序计划单);
        FlowStepTracker.CompleteStep();

        // 3.2 生产主管生产主管【审核】工序任务
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.生产主管));
        UIManager.Instance.ShowUI(UIManager.UIType.工序计划单);
        // 复用之前获取的currentUI和outboundManager
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.生产主管);
        
        // 更新审核信息
        formData1[7] = "已审核";
        formData2[4] = "生产主管";
        formData2[5] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return outboundManager.WaitForApproveButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.工序计划单);
        FlowStepTracker.CompleteStep();

        Debug.Log("====== 工序任务流程结束 ======");
        #endregion

        #region 4. 生产准备与执行
        Debug.Log("====== 生产准备与执行开始 ======");

        // 保存工序任务数据（formData1, formData2, tableData当前是工序任务的数据）
        string[] taskFormData1 = formData1;
        string[] taskFormData2 = formData2;
        string[][] taskTableData = tableData;

        // 4.1 5位班组长【查看】工序任务
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.五个班组长));
        UIManager.Instance.ShowUI(UIManager.UIType.测试UI);
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.测试UI);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.五个班组长);

        // 显示已审核的工序任务供查看
        currentUI.GetComponent<TestUIScript>().SetTexts("工序任务", "查看");
        yield return currentUI.GetComponent<TestUIScript>().WaitForButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.测试UI);
        FlowStepTracker.CompleteStep();

        // 4.2 找领料员领料员领原料
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.领料员));
        UIManager.Instance.ShowUI(UIManager.UIType.测试UI);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.领料员);

        currentUI.GetComponent<TestUIScript>().SetTexts("生产领料单", "领料");
        yield return currentUI.GetComponent<TestUIScript>().WaitForButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.测试UI);
        FlowStepTracker.CompleteStep();
        
        // 4.3 5位班组长【开始组装】
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.五个班组长));
        UIManager.Instance.ShowUI(UIManager.UIType.测试UI);
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.测试UI);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.五个班组长);
        
        // 设置测试UI文本并等待确认
        currentUI.GetComponent<TestUIScript>().SetTexts("开始组装空压机", "确认开始");
        yield return currentUI.GetComponent<TestUIScript>().WaitForButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.测试UI);
        FlowStepTracker.CompleteStep();
        
        // 确保玩家控制已启用，允许自由移动
        GameObject player = ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player);
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetPlayerInputEnabled(true);
                Debug.Log("玩家控制已启用，可以自由移动");
            }
        }
        
        // 4.3.1 空压机自动组装过程（后台运行 - 不阻塞玩家操作）
        GameObject airCompressor = ObjectManager.Instance.GetObject(ObjectManager.ObjectType.空压机);
        MeshRendererSequence meshRendererSequence = airCompressor.GetComponent<MeshRendererSequence>();
        
        if (meshRendererSequence != null && airCompressor != null)
        {
            Debug.Log("====== 空压机组装过程开始（后台运行） ======");
            
            // 获取协程运行器并启动独立的协程进行空压机组装，不阻塞主流程
            MonoBehaviour coroutineRunner = GetCoroutineRunner();
            if (coroutineRunner != null)
            {
                coroutineRunner.StartCoroutine(AssembleAirCompressorInBackground(airCompressor, meshRendererSequence));
                Debug.Log("空压机正在后台组装中，玩家可以自由移动");
            }
            else
            {
                Debug.LogError("无法获取协程运行器，跳过空压机组装动画");
            }
        }
        else
        {
            Debug.LogWarning("空压机对象或MeshRendererSequence组件未找到，跳过组装动画");
        }

        Debug.Log("====== 生产准备与执行结束 ======");
        #endregion

        #region 5. 前4道工序汇报流程
        Debug.Log("====== 前4道工序汇报流程开始 ======");

        // 5.1 班组长【填】工序汇报单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.五个班组长));
        UIManager.Instance.ShowUI(UIManager.UIType.工序汇报单);
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.工序汇报单);
        outboundManager = currentUI.GetComponent<FinishedProductOutboundManager>();
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.五个班组长);
        
        // 生成工序汇报单数据（前4道工序）
        currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        formData1 = new string[] {
            "GXHB-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999),
            System.DateTime.Now.ToString("yyyy-MM-dd"),
            "WIP-2024-" + System.DateTime.Now.ToString("MMdd"),
            "AC-100",
            "前4道工序汇报",
            "待审核",
            "4位班组长填写"
        };
        
        formData2 = new string[] {
            "4位班组长",
            currentTime,
            "4位班组长",
            currentTime,
            "",
            ""
        };
        
        tableData = new string[][] {
            new string[] {"G01", "工序1", "已完成", "5", "0", "合格"},
            new string[] {"G02", "工序2", "已完成", "5", "0", "合格"},
            new string[] {"G03", "工序3", "已完成", "5", "0", "合格"},
            new string[] {"G04", "工序4", "已完成", "5", "0", "合格"}
        };
        
        yield return outboundManager.WaitForFillButtonClick();
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return outboundManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.工序汇报单);
        FlowStepTracker.CompleteStep();

        // 5.2 生产主管生产主管【审核】工序汇报单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.生产主管));
        UIManager.Instance.ShowUI(UIManager.UIType.工序汇报单);
        // 复用之前获取的currentUI和outboundManager
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.生产主管);
        
        // 更新审核信息
        formData1[5] = "已审核";
        formData2[4] = "生产主管";
        formData2[5] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return outboundManager.WaitForApproveButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.工序汇报单);
        FlowStepTracker.CompleteStep();

        // 5.3 班组长【下推】工序汇报单（到）生产检验单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.五个班组长));
        UIManager.Instance.ShowUI(UIManager.UIType.工序汇报单);
        // 复用之前获取的currentUI和outboundManager
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.五个班组长);
        
        formData1[6] = "下推至生产检验单";
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return outboundManager.WaitForPushButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.工序汇报单);
        FlowStepTracker.CompleteStep();

        // 5.4 质检员陈梓谦【质检】
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.质检员));
        UIManager.Instance.ShowUI(UIManager.UIType.测试UI);
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.测试UI);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.质检员);
        
        // 设置测试UI文本并等待确认
        currentUI.GetComponent<TestUIScript>().SetTexts("前4道工序检验", "质检完成");
        yield return currentUI.GetComponent<TestUIScript>().WaitForButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.测试UI);
        FlowStepTracker.CompleteStep();

        // 5.5 质检员陈梓谦【填】生产检验单
        yield return new WaitForSeconds(1f);
        // 生成生产检验单数据
        currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        formData1 = new string[] {
            "SCJY-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999),
            System.DateTime.Now.ToString("yyyy-MM-dd"),
            "GXHB-" + System.DateTime.Now.ToString("yyyyMMdd"),
            "AC-100",
            "前4道工序检验",
            "质检中",
            "质检员质检"
        };
        
        formData2 = new string[] {
            "质检员",
            currentTime,
            "质检员",
            currentTime,
            "",
            ""
        };
        
        tableData = new string[][] {
            new string[] {"工序1", "外观检查", "合格", "通过"},
            new string[] {"工序2", "尺寸检查", "合格", "通过"},
            new string[] {"工序3", "功能检查", "合格", "通过"},
            new string[] {"工序4", "性能检查", "合格", "通过"}
        };
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.质检员);

        UIManager.Instance.ShowUI(UIManager.UIType.工序汇报单);
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.工序汇报单);
        outboundManager = currentUI.GetComponent<FinishedProductOutboundManager>();
        formData1[5] = "已完成";
        yield return outboundManager.WaitForFillButtonClick();
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return outboundManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.工序汇报单);
        FlowStepTracker.CompleteStep();

        Debug.Log("====== 前4道工序汇报流程结束 ======");
        #endregion

        #region 6. 第5道工序（最后一道）汇报流程
        Debug.Log("====== 第5道工序汇报流程开始 ======");
        
        // 6.1 5车间班组长班组长【填】工序汇报单（选择：是最后一道工序）
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.五车间班组长));
        UIManager.Instance.ShowUI(UIManager.UIType.工序汇报单);
        // 复用之前获取的currentUI和outboundManager
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.五车间班组长);
        
        // 生成第5道工序汇报单数据
        currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        formData1 = new string[] {
            "GXHB-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999),
            System.DateTime.Now.ToString("yyyy-MM-dd"),
            "WIP-2024-" + System.DateTime.Now.ToString("MMdd"),
            "AC-100",
            "第5道工序汇报（最后工序）",
            "待审核",
            "5车间班组长填写"
        };
        
        formData2 = new string[] {
            "5车间班组长",
            currentTime,
            "5车间班组长",
            currentTime,
            "",
            ""
        };
        
        tableData = new string[][] {
            new string[] {"G05", "工序5（总装）", "已完成", "5", "0", "合格"}
        };
        
        yield return outboundManager.WaitForFillButtonClick();
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return outboundManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.工序汇报单);
        FlowStepTracker.CompleteStep();
        
        // 6.2 生产主管生产主管【审核】工序汇报单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.生产主管));
        UIManager.Instance.ShowUI(UIManager.UIType.工序汇报单);
        // 复用之前获取的currentUI和outboundManager
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.生产主管);
        
        // 更新审核信息
        formData1[5] = "已审核";
        formData2[4] = "生产主管";
        formData2[5] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return outboundManager.WaitForApproveButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.工序汇报单);
        FlowStepTracker.CompleteStep();
        
        // 6.3 5车间班组长【下推】工序汇报单（到）生产汇报单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.五车间班组长));
        UIManager.Instance.ShowUI(UIManager.UIType.工序汇报单);
        // 复用之前获取的currentUI和outboundManager
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.五车间班组长);
        
        formData1[6] = "下推到生产汇报单";
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return outboundManager.WaitForPushButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.工序汇报单);
        FlowStepTracker.CompleteStep();

        Debug.Log("====== 第5道工序汇报流程结束 ======");
        #endregion

        #region 7. 生产汇报单流程
        Debug.Log("====== 生产汇报单流程开始 ======");
        
        // 7.1 5车间班组长班组长【填】生产汇报单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.五车间班组长));
        UIManager.Instance.ShowUI(UIManager.UIType.生产汇报单);
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.生产汇报单);
        outboundManager = currentUI.GetComponent<FinishedProductOutboundManager>();
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.五车间班组长);
        
        // 生成生产汇报单数据
        currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        formData1 = new string[] {
            "SCHB-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999),
            System.DateTime.Now.ToString("yyyy-MM-dd"),
            "WIP-2024-" + System.DateTime.Now.ToString("MMdd"),
            "AC-100",
            "空气压缩机",
            "5",
            "待审核",
            "5车间班组长填写"
        };
        
        formData2 = new string[] {
            "5车间班组长",
            currentTime,
            "5车间班组长",
            currentTime,
            "",
            ""
        };
        
        tableData = new string[][] {
            new string[] {"全部工序", "5道工序全部完成", "5", "0", "100%", "合格"}
        };
        
        yield return outboundManager.WaitForFillButtonClick();
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return outboundManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.生产汇报单);
        FlowStepTracker.CompleteStep();
        
        // 7.2 生产主管生产主管【审核】生产汇报单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.生产主管));
        UIManager.Instance.ShowUI(UIManager.UIType.生产汇报单);
        // 复用之前获取的currentUI和outboundManager
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.生产主管);
        
        // 更新审核信息
        formData1[6] = "已审核";
        formData2[4] = "生产主管";
        formData2[5] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return outboundManager.WaitForApproveButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.生产汇报单);
        FlowStepTracker.CompleteStep();
        
        // 7.3 5车间班组长【下推】生产汇报单（到）生产检验单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.五车间班组长));
        UIManager.Instance.ShowUI(UIManager.UIType.生产汇报单);
        // 复用之前获取的currentUI和outboundManager
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.五车间班组长);
        
        formData1[7] = "下推到生产检验单";
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return outboundManager.WaitForPushButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.生产汇报单);
        FlowStepTracker.CompleteStep();

        Debug.Log("====== 生产汇报单流程结束 ======");
        #endregion

        #region 8. 最终生产检验流程
        Debug.Log("====== 最终生产检验流程开始 ======");
        
        // 8.1 质检员陈梓谦【质检】
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.质检员));
        UIManager.Instance.ShowUI(UIManager.UIType.测试UI);
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.测试UI);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.质检员);

        currentUI.GetComponent<TestUIScript>().SetTexts("准备进行质量检查", "质检");

        yield return currentUI.GetComponent<TestUIScript>().WaitForButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.测试UI);
        FlowStepTracker.CompleteStep();

        // 8.2 质检员陈梓谦【填】生产检验单
        // 生成最终生产检验单数据
        currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        formData1 = new string[] {
            "SCJY-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999),
            System.DateTime.Now.ToString("yyyy-MM-dd"),
            "SCHB-" + System.DateTime.Now.ToString("yyyyMMdd"),
            "AC-100",
            "成品最终检验",
            "质检中",
            "质检员质检"
        };

        formData2 = new string[] {
            "质检员",
            currentTime,
            "质检员",
            currentTime,
            "",
            ""
        };

        tableData = new string[][] {
            new string[] {"外观检验", "外观完整无损", "合格", "通过"},
            new string[] {"性能检验", "性能指标达标", "合格", "通过"},
            new string[] {"安全检验", "安全标准合格", "合格", "通过"},
            new string[] {"最终验收", "整体质量合格", "合格", "通过"}
        };

        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return new WaitForSeconds(1f);
        UIManager.Instance.ShowUI(UIManager.UIType.工序汇报单);
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.工序汇报单);
        outboundManager = currentUI.GetComponent<FinishedProductOutboundManager>();
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.质检员);
        
        formData1[5] = "已完成";
        yield return outboundManager.WaitForFillButtonClick();
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return outboundManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.工序汇报单);
        FlowStepTracker.CompleteStep();
        
        // 8.3 生产主管生产主管【审核】生产检验单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.生产主管));
        UIManager.Instance.ShowUI(UIManager.UIType.工序汇报单);
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.工序汇报单);
        outboundManager = currentUI.GetComponent<FinishedProductOutboundManager>();
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.生产主管);
        
        // 更新审核信息
        formData2[4] = "生产主管";
        formData2[5] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return outboundManager.WaitForApproveButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.工序汇报单);
        FlowStepTracker.CompleteStep();

        Debug.Log("====== 最终生产检验流程结束 ======");
        #endregion

        #region 9. 成品入库
        Debug.Log("====== 成品入库流程开始 ======");
        
        // 9.1 5号车间班组长班组长【将成品送进仓库】
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.五车间班组长));
        UIManager.Instance.ShowUI(UIManager.UIType.完工入库单);
        currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.完工入库单);
        outboundManager = currentUI.GetComponent<FinishedProductOutboundManager>();
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.五车间班组长);
        
        // 生成成品入库单数据
        currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        formData1 = new string[] {
            "CPRK-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999),
            System.DateTime.Now.ToString("yyyy-MM-dd"),
            "SCHB-" + System.DateTime.Now.ToString("yyyyMMdd"),
            "AC-100",
            "空气压缩机",
            "5",
            "主仓库",
            "5车间班组长送入仓库"
        };
        
        formData2 = new string[] {
            "5车间班组长",
            currentTime,
            "5车间班组长",
            currentTime,
            "",
            ""
        };
        
        tableData = new string[][] {
            new string[] {"AC-100", "空气压缩机", "标准型", "5", "主仓库", "A区01架"}
        };
        
        yield return outboundManager.WaitForFillButtonClick();
        outboundManager.SetQuoteFormData(formData1, formData2, tableData);
        yield return outboundManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.完工入库单);
        FlowStepTracker.CompleteStep();

        Debug.Log("====== 成品入库流程结束 ======");
        #endregion

        yield return new WaitForSeconds(1f);
        
        // 流程结束
        FinishFlow();
    }
    
    // 可以根据需要重写其他方法
    public override void StartFlow()
    {        
        Debug.Log("生产流程2开始");
        base.StartFlow();
        
        // 初始化任务UI
        this.InitTaskUI();
    }
    
    public override void StopFlow()
    {        
        Debug.Log("生产流程2停止");
        base.StopFlow();
    }
    
    protected override void FinishFlow()
    {        
        Debug.Log("生产流程2正常结束");
        base.FinishFlow();
    }
    #endregion
    
    #region 私有方法
    /// <summary>
    /// 空压机后台组装协程（不阻塞主流程）
    /// </summary>
    private IEnumerator AssembleAirCompressorInBackground(GameObject airCompressor, MeshRendererSequence meshRendererSequence)
    {
        // 加快速度，让组装过程更流畅
        float moveSpeed = 3.0f;
        
        // 依次移动到位置1-6，快速组装
        for (int i = 1; i <= 6; i++)
        {
            PositionManager.PositionType targetPositionType = (PositionManager.PositionType)System.Enum.Parse(typeof(PositionManager.PositionType), "位置" + i);
            
            // 只移动空压机，不控制玩家
            yield return MoveAirCompressorOnly(airCompressor, targetPositionType, moveSpeed);
            Debug.Log($"[后台组装] 空压机已移动到位置{i}");
            
            // 从位置2开始，每到一个位置就显示组装部件
            if (i >= 2)
            {
                meshRendererSequence.ShowNextRenderer();
                Debug.Log($"[后台组装] 在位置{i}组装部件{i-1}");
            }
            
            // 减少等待时间，加快流程
            yield return new WaitForSeconds(0.2f);
        }
        
        Debug.Log("====== 空压机组装过程完成（后台） ======");
    }
    
    /// <summary>
    /// 只移动空压机，不控制玩家位置（优化后的组装流程）
    /// </summary>
    private IEnumerator MoveAirCompressorOnly(GameObject targetObject, PositionManager.PositionType targetPositionType, float moveSpeed)
    {
        if (targetObject == null)
        {
            Debug.LogError("目标物体为空");
            yield break;
        }
        
        Vector3 startPosition = targetObject.transform.position;
        Vector3 endPosition = PositionManager.Instance.GetPosition(targetPositionType);
        Quaternion startRotation = targetObject.transform.rotation;
        Quaternion endRotation = PositionManager.Instance.GetRotation(targetPositionType);
        
        // 计算移动距离和时间
        float distance = Vector3.Distance(startPosition, endPosition);
        float journeyLength = distance / moveSpeed;
        float startTime = Time.time;
        
        // 平滑移动和旋转（只移动空压机）
        while (Time.time - startTime < journeyLength)
        {
            float journeyFraction = (Time.time - startTime) / journeyLength;
            targetObject.transform.position = Vector3.Lerp(startPosition, endPosition, journeyFraction);
            targetObject.transform.rotation = Quaternion.Slerp(startRotation, endRotation, journeyFraction);
            
            yield return null; // 等待下一帧
        }
        
        // 确保精确到达目标位置和旋转
        targetObject.transform.position = endPosition;
        targetObject.transform.rotation = endRotation;
    }
    
    // 这里可以添加生产流程特有的私有方法
    #endregion
}