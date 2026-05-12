using UnityEngine;
using System.Collections;

// 场景1管理脚本 - 负责初始化场景1的事件流程
public class MySceneManager : MonoBehaviour
{
    #region 单例模式
    private static MySceneManager _instance;
    public static MySceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("Scene1Manager instance not found!");
            }
            return _instance;
        }
    }
    #endregion

    #region 生命周期方法
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 初始化场景1的事件流程
        InitializeSceneFlow();
        // 初始化场景1UI
        UIManager.Instance.InitializeUI();
    }
    #endregion

    #region 初始化方法
    /// <summary>
    /// 初始化场景1的事件流程
    /// </summary>
    private void InitializeSceneFlow()
    {
        Debug.Log("开始初始化场景1的事件流程");

        // 确保FlowManager已初始化
        if (FlowManager.Instance != null)
        {
            // 设置流程执行模式为顺序执行
            FlowManager.Instance.SetSequentialMode(true);

            // 注册并启动场景1需要的流程
            RegisterAndStartSceneFlows();

            Debug.Log("场景1的事件流程初始化完成");
        }
        else
        {
            Debug.LogError("FlowManager未初始化，无法设置场景1的事件流程");
        }
    }

    /// <summary>
    /// 注册并启动场景1的流程
    /// </summary>
    private void RegisterAndStartSceneFlows()
    {
        try
        {
            // 注册销售流程、采购流程和生产流程
            FlowManager.Instance.CreateAndRegisterFlow<SalesFlow>();
            FlowManager.Instance.CreateAndRegisterFlow<ProductionFlow>();
            FlowManager.Instance.CreateAndRegisterFlow<PurchaseFlow>();
            FlowManager.Instance.CreateAndRegisterFlow<ProductionFlow2>();
            FlowManager.Instance.CreateAndRegisterFlow<WarehouseFlow>();

            //FlowManager.Instance.StartFlow<WarehouseFlow>();

            //按顺序执行：销售流程 -> 生产流程 -> 采购流程
            FlowManager.Instance.StartFlowsSequence(
               typeof(SalesFlow),
               typeof(ProductionFlow),
               typeof(PurchaseFlow),
               typeof(ProductionFlow2),
               typeof(WarehouseFlow)
            );

            Debug.Log("销售流程、采购流程和生产流程已注册并按顺序启动");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"注册或启动场景1流程时发生错误: {e.Message}");
        }
    }
    #endregion

    #region 场景控制方法
    /// <summary>
    /// 重置场景1的状态
    /// </summary>
    public void ResetScene()
    {
        Debug.Log("重置场景1状态");
        // 停止所有当前运行的流程
        if (FlowManager.Instance != null)
        {
            FlowManager.Instance.StopAllFlows();
        }
        
        // 重新初始化流程
        InitializeSceneFlow();
    }

    /// <summary>
    /// 清理场景1资源
    /// </summary>
    public void CleanupScene()
    {
        Debug.Log("清理场景1资源");
        // 停止所有流程
        if (FlowManager.Instance != null)
        {
            FlowManager.Instance.StopAllFlows();
        }
        
        // 可以在这里添加其他清理逻辑
    }
    #endregion

    #region 事件监听方法
    /// <summary>
    /// 监听场景1特定事件
    /// </summary>
    private void SetupEventListeners()
    {
        // 可以在这里设置场景特定的事件监听
        Debug.Log("设置场景1事件监听");
    }

    /// <summary>
    /// 移除事件监听
    /// </summary>
    private void RemoveEventListeners()
    {
        // 移除设置的事件监听
        Debug.Log("移除场景1事件监听");
    }
    #endregion

    #region 私有辅助方法
    // 这里可以添加场景1特有的辅助方法
    #endregion

    private void OnDestroy()
    {
        // 移除事件监听
        RemoveEventListeners();
        
        // 清理场景资源
        CleanupScene();
    }
}