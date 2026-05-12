using UnityEngine;
using System.Collections.Generic;
using System;

// 库存管理器 - 负责管理压缩机零件的库存
public class InventoryManager : MonoBehaviour
{
    #region 单例模式
    private static InventoryManager _instance;
    public static InventoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("InventoryManager instance not found!");
            }
            return _instance;
        }
    }
    #endregion

    #region 枚举定义
    // 零件类型枚举 - 用于标识不同的压缩机零件
    public enum PartType
    {
        螺杆主机,
        电机,
        气缸,
        油气分离器,
        空气过滤器,
        机油过滤器,
        冷却器,
        控制面板,
        进气阀,
        机架底座,
        钣金机壳,
    }
    #endregion

    #region 私有字段
    // 存储所有零件库存的字典
    private Dictionary<PartType, int> _inventoryDictionary = new Dictionary<PartType, int>();

    // 是否已经初始化
    private bool _isInitialized = false;
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
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        // 初始化所有零件的库存为0
        foreach (PartType partType in Enum.GetValues(typeof(PartType)))
        {
            _inventoryDictionary[partType] = 0;
        }
        _isInitialized = true;
        Debug.Log("库存系统初始化完成");
    }
    #endregion

    #region 公共方法
    public bool AddAllInventory(int quantity)
    {
        if (!_isInitialized)
        {
            Debug.LogError("库存系统未初始化");
            return false;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning("添加的数量必须大于0");
            return false;
        }

        foreach (PartType partType in Enum.GetValues(typeof(PartType)))
        {
            _inventoryDictionary[partType] += quantity;
            Debug.Log($"添加库存 - 零件: {partType}, 数量: {quantity}, 当前库存: {_inventoryDictionary[partType]}");
        }
        return true;
    }

    public bool AddInventory(PartType partType, int quantity)
    {
        if (!_isInitialized)
        {
            Debug.LogError("库存系统未初始化");
            return false;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning("添加的数量必须大于0");
            return false;
        }

        if (!_inventoryDictionary.ContainsKey(partType))
        {
            Debug.LogError($"未知的零件类型: {partType}");
            return false;
        }

        _inventoryDictionary[partType] += quantity;
        Debug.Log($"添加库存 - 零件: {partType}, 数量: {quantity}, 当前库存: {_inventoryDictionary[partType]}");
        return true;
    }

    public bool RemoveInventory(PartType partType, int quantity)
    {
        if (!_isInitialized)
        {
            Debug.LogError("库存系统未初始化");
            return false;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning("移除的数量必须大于0");
            return false;
        }

        if (!_inventoryDictionary.ContainsKey(partType))
        {
            Debug.LogError($"未知的零件类型: {partType}");
            return false;
        }

        if (_inventoryDictionary[partType] < quantity)
        {
            Debug.LogWarning($"库存不足 - 零件: {partType}, 当前库存: {_inventoryDictionary[partType]}, 所需数量: {quantity}");
            return false;
        }

        _inventoryDictionary[partType] -= quantity;
        Debug.Log($"移除库存 - 零件: {partType}, 数量: {quantity}, 当前库存: {_inventoryDictionary[partType]}");
        return true;
    }

    public int GetInventoryCount(PartType partType)
    {
        if (!_isInitialized)
        {
            Debug.LogError("库存系统未初始化");
            return 0;
        }

        if (!_inventoryDictionary.ContainsKey(partType))
        {
            Debug.LogError($"未知的零件类型: {partType}");
            return 0;
        }

        return _inventoryDictionary[partType];
    }

    public bool HasEnoughInventory(PartType partType, int quantity)
    {
        return GetInventoryCount(partType) >= quantity;
    }

    public bool RemoveAllInventory(int quantity)
    {
        if (!_isInitialized)
        {
            Debug.LogError("库存系统未初始化");
            return false;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning("移除的数量必须大于0");
            return false;
        }

        // 先检查所有零件是否有足够的库存
        foreach (PartType partType in Enum.GetValues(typeof(PartType)))
        {
            if (_inventoryDictionary[partType] < quantity)
            {
                Debug.LogWarning($"库存不足 - 零件: {partType}, 当前库存: {_inventoryDictionary[partType]}, 所需数量: {quantity}");
                return false;
            }
        }

        // 如果所有零件都有足够库存，则执行移除操作
        foreach (PartType partType in Enum.GetValues(typeof(PartType)))
        {
            _inventoryDictionary[partType] -= quantity;
            Debug.Log($"移除库存 - 零件: {partType}, 数量: {quantity}, 当前库存: {_inventoryDictionary[partType]}");
        }
        return true;
    }
    #endregion
}