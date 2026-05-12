using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 位置管理器
/// </summary>
public class PositionManager : MonoBehaviour
{
    #region 单例
    private static PositionManager _instance;
    public static PositionManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("PositionManager 未初始化!");
            return _instance;
        }
    }
    #endregion

    public enum PositionType
    {
        销售员,
        销售主管,
        生产主管,
        领料员,
        仓管员,
        仓库主管,
        采购员,
        采购主管,
        质检员,
        五个班组长,
        五车间班组长,
        位置1,
        位置2,
        位置3,
        位置4,
        位置5,
        位置6,
        进货位置,
        卡车视角
    }

    private Dictionary<PositionType, Transform> _positionDictionary = new Dictionary<PositionType, Transform>();
    private bool _isInitialized = false;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitializePositions();
    }

    public void InitializePositions()
    {
        if (_isInitialized) return;

        _positionDictionary.Clear();
        int childCount = transform.childCount;
        
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (i < Enum.GetValues(typeof(PositionType)).Length)
            {
                PositionType posType = (PositionType)i;
                _positionDictionary.Add(posType, child);
                Debug.Log($"位置初始化: {posType} -> {child.name}");
            }
        }
        _isInitialized = true;
    }

    public Vector3 GetPosition(PositionType positionType)
    {
        if (!_isInitialized) InitializePositions();
        if (_positionDictionary.TryGetValue(positionType, out Transform t))
            return t.position;
        
        Debug.LogError($"找不到位置: {positionType}");
        return Vector3.zero;
    }

    public Quaternion GetRotation(PositionType positionType)
    {
        if (!_isInitialized) InitializePositions();
        if (_positionDictionary.TryGetValue(positionType, out Transform t))
            return t.rotation;
        
        Debug.LogError($"找不到位置: {positionType}");
        return Quaternion.identity;
    }

    public Transform GetPositionTransform(PositionType positionType)
    {
        if (!_isInitialized) InitializePositions();
        if (_positionDictionary.TryGetValue(positionType, out Transform t))
            return t;
        
        Debug.LogError($"找不到位置: {positionType}");
        return null;
    }

    public void SetObjectToPosition(GameObject target, PositionType positionType, bool matchRotation = true)
    {
        if (target == null)
        {
            Debug.LogError("目标物体不能为空");
            return;
        }

        if (!_isInitialized) InitializePositions();

        if (_positionDictionary.TryGetValue(positionType, out Transform t))
        {
            target.transform.position = t.position;
            if (matchRotation) target.transform.rotation = t.rotation;
            Debug.Log($"{target.name} 已移动到 {positionType}");
        }
        else
            Debug.LogError($"找不到位置: {positionType}");
    }
}