using UnityEngine;
using System.Collections.Generic;

public class PositionManager : MonoBehaviour
{
    #region 单例
    private static PositionManager _instance;
    public static PositionManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("[PositionManager] 实例不存在");
            return _instance;
        }
    }
    #endregion

    public enum PositionType
    {
        SalesDesk,          // 销售员
        SalesManagerDesk,   // 销售主管
        ProductionManagerDesk, // 生产主管
        MaterialPicker,     // 领料员
        WarehouseDesk,      // 仓管员
        WarehouseManagerDesk, // 仓库主管
        Purchaser,          // 采购员
        PurchaseManager,    // 采购主管
        QualityInspector,   // 质检员
        TeamLeader_5,       // 五个班组长
        WorkshopLeader_5,   // 五车间班组长
        Waypoint1, Waypoint2, Waypoint3, Waypoint4, Waypoint5, Waypoint6,
        InboundLocation,    // 进货位置
        TruckView,          // 卡车视角
        PMC_Office          // PMC办公室
    }

    [System.Serializable]
    public class PositionEntry
    {
        public PositionType type;
        public Transform target;
    }

    [Header("📍 位置点配置")]
    [Tooltip("将场景中的空物体/路标点拖拽至此")]
    [SerializeField] private List<PositionEntry> positionEntries = new List<PositionEntry>();

    private Dictionary<PositionType, Transform> _positionDictionary = new Dictionary<PositionType, Transform>();
    private bool _isInitialized = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePositions();
        }
        else Destroy(gameObject);
    }

    public void InitializePositions()
    {
        if (_isInitialized) return;
        _positionDictionary.Clear();
        foreach (var entry in positionEntries)
        {
            if (entry.target != null)
                _positionDictionary[entry.type] = entry.target;
            else
                Debug.LogWarning($"[PositionManager] 未配置位置: {entry.type}");
        }
        _isInitialized = true;
    }

    public bool TryGetPosition(PositionType type, out Vector3 pos)
    {
        pos = Vector3.zero;
        if (!_isInitialized) InitializePositions();
        if (_positionDictionary.TryGetValue(type, out Transform t))
        {
            pos = t.position;
            return true;
        }
        return false;
    }

    public bool TryGetPositionTransform(PositionType type, out Transform t)
    {
        t = null;
        if (!_isInitialized) InitializePositions();
        return _positionDictionary.TryGetValue(type, out t) && t != null;
    }

    public Vector3 GetPosition(PositionType type)
    {
        if (TryGetPosition(type, out Vector3 pos)) return pos;
        Debug.LogError($"[PositionManager] 找不到位置: {type}");
        return Vector3.zero;
    }

    public Quaternion GetRotation(PositionType type)
    {
        if (!_isInitialized) InitializePositions();
        if (_positionDictionary.TryGetValue(type, out Transform t)) return t.rotation;
        Debug.LogError($"[PositionManager] 找不到位置旋转: {type}");
        return Quaternion.identity;
    }

    /// <summary>安全瞬移（自动处理CharacterController防穿地）</summary>
    public void SetObjectToPosition(GameObject target, PositionType posType, bool matchRotation = true)
    {
        if (target == null || !TryGetPositionTransform(posType, out Transform t)) return;

        var charCtrl = target.GetComponent<CharacterController>();
        if (charCtrl != null) charCtrl.enabled = false;

        target.transform.SetPositionAndRotation(t.position, matchRotation ? t.rotation : target.transform.rotation);

        if (charCtrl != null) charCtrl.enabled = true;
    }

    public void RegisterPosition(PositionType type, Transform pos)
    {
        if (pos == null) return;
        if (!_isInitialized) _isInitialized = true;
        _positionDictionary[type] = pos;
    }
}