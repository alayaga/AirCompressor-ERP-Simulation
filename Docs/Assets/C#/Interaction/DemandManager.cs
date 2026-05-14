using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 需求管理器
/// 负责在游戏开始时随机生成客户需求（空压机产品数量）
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
                // 尝试在场景中查找
                _instance = FindObjectOfType<DemandManager>();
                
                // 如果场景中没有，则创建一个新的
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
    
    #region 配置参数
    [Header("需求生成配置")]
    [SerializeField]
    private int minAirCompressorDemand = 1; // 最小需求数量
    
    [SerializeField]
    private int maxAirCompressorDemand = 5; // 最大需求数量
    
    [Header("价格配置")]
    [SerializeField]
    private float minUnitPrice = 20000.0f; // 最小单价
    
    [SerializeField]
    private float maxUnitPrice = 30000.0f; // 最大单价
    #endregion
    
    #region 私有字段
    private CustomerDemand currentDemand; // 当前客户需求
    #endregion
    
    #region 数据结构
    /// <summary>
    /// 客户需求数据结构
    /// </summary>
    [System.Serializable]
    public class CustomerDemand
    {        
        public int airCompressorCount; // 空压机需求数量
        public string customerName;    // 客户名称
        public float unitPrice;        // 单价（元）
    }
    #endregion
    
    #region 生命周期方法
    private void Awake()
    {        
        // 实现单例模式
        if (_instance != null && _instance != this)
        {            
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
    }
    
    private void Start()
    {        
        // 游戏开始时生成初始需求
        GenerateNewDemand();
    }
    

    #endregion
    
    #region 公共方法
    public CustomerDemand GetCurrentDemand()
    {        
        // 如果当前需求为空，自动生成新需求
        if (currentDemand == null)
        {
            GenerateNewDemand();
        }
        return currentDemand;
    }
    
    public void GenerateNewDemand()
    {        
        // 生成随机需求数量
        int demandCount = Random.Range(minAirCompressorDemand, maxAirCompressorDemand + 1);
        
        // 生成客户名称和描述
        string customerName = GenerateCustomerName();
        
        // 生成随机单价（保留两位小数）
        float unitPrice = Mathf.Round(Random.Range(minUnitPrice, maxUnitPrice) * 100) / 100;
        
        // 创建新的需求对象
        currentDemand = new CustomerDemand
        {            
            airCompressorCount = demandCount,
            customerName = customerName,
            unitPrice = unitPrice
        };
    }
    
    public void SetDemandRange(int min, int max)
    {        
        minAirCompressorDemand = Mathf.Max(min, 1); // 确保最小值至少为1
        maxAirCompressorDemand = Mathf.Max(max, minAirCompressorDemand); // 确保最大值不小于最小值
    }
    #endregion
    
    #region 私有方法
    private string GenerateCustomerName()
    {        
        // 客户名称列表
        string[] customerNames = new string[]
        {            
            "工业机械有限公司",
            "建筑工程集团",
            "汽车制造企业",
            "船舶修理厂",
            "矿山开发公司",
            "电子工厂",
            "食品加工厂",
            "医疗设备生产厂",
            "纺织企业",
            "家具制造公司"
        };
        
        return customerNames[Random.Range(0, customerNames.Length)];
    }
    #endregion
}