using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 需求管理器
/// 负责在游戏开始时生成客户需求（空压机产品数量）
/// 调试阶段：固定需求为 2 台，支持标准/定制流程类型扩展
/// </summary>
public class DemandManager : MonoBehaviour
{
    #region 单例模式
    private static DemandManager _instance;
    public static DemandManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DemandManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("DemandManager");
                    _instance = go.AddComponent<DemandManager>();
                    Debug.Log("DemandManager 自动创建");
                }
            }
            return _instance;
        }
    }
    #endregion

    #region 配置参数（保留原有字段，调试时可忽略）
    [Header("需求生成配置")]
    [SerializeField]
    private int minAirCompressorDemand = 1;

    [SerializeField]
    private int maxAirCompressorDemand = 5;

    [Header("价格配置")]
    [SerializeField]
    private float minUnitPrice = 20000.0f;

    [SerializeField]
    private float maxUnitPrice = 30000.0f;

    // 新增：调试模式开关（Inspector 可勾）
    [Header("调试配置")]
    [Tooltip("调试阶段启用：固定需求数量，忽略随机范围")]
    [SerializeField]
    private bool isDebugMode = true;

    [Tooltip("调试阶段固定需求数量（流程图要求：2台）")]
    [SerializeField]
    private int debugFixedDemandCount = 2;

    [Tooltip("当前流程类型：由开始界面传入")]
    [SerializeField]
    private WorkflowType currentWorkflowType = WorkflowType.Standard;
    #endregion

    #region 流程类型枚举（与开始界面对齐）
    public enum WorkflowType { Standard, Custom }
    #endregion

    #region 私有字段
    private CustomerDemand currentDemand;
    #endregion

    #region 数据结构（保留原有结构）
    [System.Serializable]
    public class CustomerDemand
    {
        public int airCompressorCount;
        public string customerName;
        public float unitPrice;

        // 新增：携带流程类型，供下游模块判断
        public WorkflowType workflowType;
    }
    #endregion

    #region 生命周期
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject); // 防止场景切换时单例丢失
    }

    private void Start()
    {
        // 调试阶段：直接生成固定需求
        // 正式阶段可改为：等待开始界面传入类型后再生成
        GenerateNewDemand();
    }
    #endregion

    #region 公共方法
    public CustomerDemand GetCurrentDemand()
    {
        if (currentDemand == null)
        {
            GenerateNewDemand();
        }
        return currentDemand;
    }

    /// <summary>
    /// 新增：由开始界面调用，设置流程类型并生成需求
    /// </summary>
    public void GenerateDemandByWorkflow(WorkflowType type)
    {
        currentWorkflowType = type;
        GenerateNewDemand();
        Debug.Log($"[Demand] 流程类型已设置: {type} | 需求: {currentDemand?.airCompressorCount}台");
    }

    public void GenerateNewDemand()
    {
        // 调试逻辑：固定 2 台
        int demandCount = isDebugMode ? debugFixedDemandCount : Random.Range(minAirCompressorDemand, maxAirCompressorDemand + 1);

        string customerName = GenerateCustomerName();

        // 调试阶段：固定单价便于测试（可取消注释启用随机）
        float unitPrice = isDebugMode ? 25000.0f : Mathf.Round(Random.Range(minUnitPrice, maxUnitPrice) * 100) / 100;

        currentDemand = new CustomerDemand
        {
            airCompressorCount = demandCount,
            customerName = customerName,
            unitPrice = unitPrice,
            workflowType = currentWorkflowType // 携带流程类型
        };

        Debug.Log($"[Demand] 已生成 | 客户:{customerName} | 数量:{demandCount}台 | 类型:{currentWorkflowType}");
    }

    public void SetDemandRange(int min, int max)
    {
        minAirCompressorDemand = Mathf.Max(min, 1);
        maxAirCompressorDemand = Mathf.Max(max, minAirCompressorDemand);
    }

    /// <summary>
    /// 新增：供开始界面调用，设置流程类型（不立即生成需求）
    /// </summary>
    public void SetWorkflowType(WorkflowType type)
    {
        currentWorkflowType = type;
        Debug.Log($"[Demand] 流程类型预设置: {type}");
    }
    #endregion

    #region 私有方法
    private string GenerateCustomerName()
    {
        string[] customerNames = new string[]
        {
            "工业机械有限公司", "建筑工程集团", "汽车制造企业",
            "船舶修理厂", "矿山开发公司", "电子工厂",
            "食品加工厂", "医疗设备生产厂", "纺织企业", "家具制造公司"
        };
        return customerNames[Random.Range(0, customerNames.Length)];
    }
    #endregion
}