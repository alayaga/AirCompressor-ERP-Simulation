using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    #region 单例
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("[UIManager] 实例不存在");
            return _instance;
        }
    }
    #endregion

    public enum UIType
    {
        TestUI, CommonUI,
        SalesQuotation, SalesOrder,
        ProductionWorkOrder, ProductionBOM, PickList, DispatchOrder,
        ProductionMaterialList, PurchaseRequest, PurchaseOrder, ReceiptNotice, PurchaseInbound,
        WeeklyProductionPlan, ProcessReport, ProductionReport, DeliveryNotice,
        FinishedInbound, SalesOutbound, ExitGameUI,
        ProductionSchedule,  // 排产单
        ProductionReturn     // 生产退料入库单
    }

    [System.Serializable]
    public class UIEntry
    {
        public UIType type;
        public GameObject panel;
    }

    [Header("️ UI面板配置")]
    [Tooltip("将Canvas下的Panel子物体拖拽至此")]
    [SerializeField] private List<UIEntry> uiEntries = new List<UIEntry>();

    private Dictionary<UIType, GameObject> _uiDictionary = new Dictionary<UIType, GameObject>();
    private bool _isInitialized = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUI();
        }
        else Destroy(gameObject);
    }

    public void InitializeUI()
    {
        if (_isInitialized) return;
        _uiDictionary.Clear();
        foreach (var entry in uiEntries)
        {
            if (entry.panel != null)
            {
                entry.panel.SetActive(false); // 初始化默认隐藏
                _uiDictionary[entry.type] = entry.panel;
            }
        }
        _isInitialized = true;
    }

    public bool TryGetUI(UIType type, out GameObject panel)
    {
        panel = null;
        if (!_isInitialized) InitializeUI();
        return _uiDictionary.TryGetValue(type, out panel) && panel != null;
    }

    public void ShowUI(UIType type)
    {
        if (TryGetUI(type, out GameObject panel))
        {
            panel.SetActive(true);
            Debug.Log($"[UIManager] 显示: {type}");
        }
        else Debug.LogError($"[UIManager] 找不到UI: {type}");
    }

    public void HideUI(UIType type)
    {
        if (TryGetUI(type, out GameObject panel))
        {
            panel.SetActive(false);
            Debug.Log($"[UIManager] 隐藏: {type}");
        }
    }

    /// <summary>独占显示（关闭其他所有面板，仅打开指定面板）</summary>
    public void ShowOnlyUI(UIType type)
    {
        if (!_isInitialized) InitializeUI();
        foreach (var kvp in _uiDictionary)
            kvp.Value.SetActive(kvp.Key == type);
        Debug.Log($"[UIManager] 独占显示: {type}");
    }

    public bool IsUIVisible(UIType type)
    {
        if (TryGetUI(type, out GameObject panel)) return panel.activeSelf;
        return false;
    }
}