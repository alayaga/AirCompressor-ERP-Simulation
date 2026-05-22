using UnityEngine;
using System.Collections.Generic;

public class ObjectManager : MonoBehaviour
{
    #region 单例
    private static ObjectManager _instance;
    public static ObjectManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("[ObjectManager] 实例不存在，请检查场景配置");
            return _instance;
        }
    }
    #endregion

    public enum ObjectType
    {
        Player,           // 玩家
        AirCompressor,    // 空压机
        RawMaterialStorage, // 原料仓储
        Truck             // 卡车
        // 后续新增物体直接在此追加
    }

    [System.Serializable]
    public class ObjectEntry
    {
        public ObjectType type;
        public GameObject target;
    }

    [Header(" 场景对象配置")]
    [Tooltip("请勿依赖Hierarchy顺序，直接在Inspector中拖拽赋值")]
    [SerializeField] private List<ObjectEntry> objectEntries = new List<ObjectEntry>();

    private Dictionary<ObjectType, GameObject> _objectDictionary = new Dictionary<ObjectType, GameObject>();
    private bool _isInitialized = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeObjects();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeObjects()
    {
        if (_isInitialized) return;
        _objectDictionary.Clear();
        foreach (var entry in objectEntries)
        {
            if (entry.target != null)
                _objectDictionary[entry.type] = entry.target;
            else
                Debug.LogWarning($"[ObjectManager] 未配置对象: {entry.type}");
        }
        _isInitialized = true;
    }

    /// <summary>安全获取，避免NullReference中断流程</summary>
    public bool TryGetObject(ObjectType type, out GameObject obj)
    {
        obj = null;
        if (!_isInitialized) InitializeObjects();
        return _objectDictionary.TryGetValue(type, out obj) && obj != null;
    }

    public GameObject GetObject(ObjectType type)
    {
        if (TryGetObject(type, out GameObject obj)) return obj;
        Debug.LogError($"[ObjectManager] 找不到物体: {type}");
        return null;
    }

    public void RegisterObject(ObjectType type, GameObject target)
    {
        if (target == null) return;
        if (!_isInitialized) _isInitialized = true;
        _objectDictionary[type] = target;
    }
}