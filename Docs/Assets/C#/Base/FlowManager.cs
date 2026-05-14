using UnityEngine;
using System.Collections.Generic;
using System;

// 流程管理器 - 负责管理所有流程的运行顺序
public class FlowManager : MonoBehaviour
{
    #region 单例模式
    private static FlowManager _instance;
    public static FlowManager Instance
    {
        get
        {   
            if (_instance == null)
            {
                _instance = FindObjectOfType<FlowManager>();
            }
            return _instance;
        }
    }
    #endregion

    #region 私有字段
    // 存储所有流程的字典
    private Dictionary<Type, FlowBase> _flowDictionary = new Dictionary<Type, FlowBase>();
    
    // 流程队列 - 用于顺序执行流程
    private Queue<Type> _flowQueue = new Queue<Type>();
    
    // 当前正在执行的流程
    private FlowBase _currentFlow = null;
    
    // 是否按顺序执行流程
    private bool _isSequentialMode = false;
    
    // 是否已经初始化
    private bool _isInitialized = false;
    #endregion

    #region 生命周期方法
    private void Awake()
    {
        if (_instance == null)
        {   
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {   
            Destroy(gameObject);
        }
    }
    #endregion

    #region 流程注册方法
    // 手动注册流程
    public void RegisterFlow(FlowBase flow)
    {   
        if (flow == null)
        {   
            Debug.LogError("注册的流程不能为空");
            return;
        }
        
        Type flowType = flow.GetType();
        if (!_flowDictionary.ContainsKey(flowType))
        {   
            _flowDictionary.Add(flowType, flow);
            Debug.Log($"流程注册: {flowType.Name}");
        }
        else
        {   
            Debug.LogWarning($"流程类型 {flowType.Name} 已存在，更新引用");
            _flowDictionary[flowType] = flow;
        }
    }
    
    // 创建并注册流程
    public T CreateAndRegisterFlow<T>() where T : FlowBase, new()
    {
        T flow = new T();
        RegisterFlow(flow);
        return flow;
    }
    
    // 移除流程
    public void UnregisterFlow(Type flowType)
    {   
        if (_flowDictionary.ContainsKey(flowType))
        {   
            FlowBase flow = _flowDictionary[flowType];
            if (flow != null && flow.IsRunning)
            {   
                flow.StopFlow();
            }
            _flowDictionary.Remove(flowType);
            Debug.Log($"流程移除: {flowType.Name}");
        }
        else
        {   
            Debug.LogWarning($"找不到要移除的流程类型: {flowType.Name}");
        }
    }
    
    // 移除特定类型的流程
    public void UnregisterFlow<T>() where T : FlowBase
    {
        UnregisterFlow(typeof(T));
    }
    #endregion

    #region 流程控制方法
    // 启动指定类型的流程
    public T StartFlow<T>() where T : FlowBase, new()
    {   
        Type flowType = typeof(T);
        
        // 如果流程不存在，先创建并注册
        if (!_flowDictionary.ContainsKey(flowType))
        {
            CreateAndRegisterFlow<T>();
        }
        
        FlowBase flow = StartFlow(flowType);
        return flow as T;
    }
    
    // 启动指定类型的流程
    public FlowBase StartFlow(Type flowType)
    {   
        
        if (_flowDictionary.TryGetValue(flowType, out FlowBase flow))
        {   
            if (_isSequentialMode && _currentFlow != null && _currentFlow.IsRunning)
            {   
                // 如果是顺序模式且当前有流程在运行，将新流程加入队列
                _flowQueue.Enqueue(flowType);
                Debug.Log($"流程 {flowType.Name} 已加入执行队列");
            }
            else
            {   
                // 启动流程
                flow.OnFlowEnd += OnFlowEndHandler;
                flow.StartFlow();
                _currentFlow = flow;
                
                Debug.Log($"流程 {flowType.Name} 已启动");
            }
            
            return flow;
        }
        else
        {   
            Debug.LogError($"找不到流程类型: {flowType.Name}");
            return null;
        }
    }
    
    // 启动多个流程（并行）
    public void StartFlowsParallel(params Type[] flowTypes)
    {   
        // 临时禁用顺序模式
        bool wasSequential = _isSequentialMode;
        _isSequentialMode = false;
        
        foreach (Type flowType in flowTypes)
        {   
            StartFlow(flowType);
        }
        
        // 恢复之前的模式
        _isSequentialMode = wasSequential;
    }
    
    // 启动多个流程（顺序）
    public void StartFlowsSequence(params Type[] flowTypes)
    {   
        // 确保启用顺序模式
        bool wasSequential = _isSequentialMode;
        _isSequentialMode = true;
        
        // 如果当前没有运行的流程，启动第一个
        if (_currentFlow == null || !_currentFlow.IsRunning)
        {   
            if (flowTypes.Length > 0)
            {   
                StartFlow(flowTypes[0]);
                
                // 将剩余流程加入队列
                for (int i = 1; i < flowTypes.Length; i++)
                {   
                    _flowQueue.Enqueue(flowTypes[i]);
                }
            }
        }
        else
        {   
            // 如果当前有运行的流程，全部加入队列
            foreach (Type flowType in flowTypes)
            {   
                _flowQueue.Enqueue(flowType);
            }
        }
        
        // 恢复之前的模式
        _isSequentialMode = wasSequential;
    }
    
    // 停止指定类型的流程
    public void StopFlow<T>() where T : FlowBase
    {   
        StopFlow(typeof(T));
    }
    
    // 停止指定类型的流程
    public void StopFlow(Type flowType)
    {
        
        if (_flowDictionary.TryGetValue(flowType, out FlowBase flow))
        {   
            flow.OnFlowEnd -= OnFlowEndHandler;
            flow.StopFlow();
            
            if (_currentFlow == flow)
            {   
                _currentFlow = null;
                // 如果有等待的流程，启动下一个
                StartNextQueuedFlow();
            }
        }
        else
        {   
            Debug.LogError($"找不到流程类型: {flowType.Name}");
        }
    }
    
    // 停止多个流程
    public void StopMultipleFlows(params Type[] flowTypes)
    {   
        foreach (Type flowType in flowTypes)
        {   
            StopFlow(flowType);
        }
    }
    
    // 停止所有流程
    public void StopAllFlows()
    {   
        foreach (var flow in _flowDictionary.Values)
        {   
            flow.OnFlowEnd -= OnFlowEndHandler;
            flow.StopFlow();
        }
        
        _currentFlow = null;
        _flowQueue.Clear();
        
        Debug.Log("所有流程已停止");
    }
    
    // 设置顺序执行模式
    public void SetSequentialMode(bool isSequential)
    {   
        _isSequentialMode = isSequential;
        Debug.Log($"流程执行模式已设置为: {(isSequential ? "顺序执行" : "并行执行")}");
    }
    
    // 获取当前运行模式
    public bool IsSequentialMode()
    {   
        return _isSequentialMode;
    }
    #endregion

    #region 队列管理方法
    // 启动队列中的下一个流程
    private void StartNextQueuedFlow()
    {   
        if (_flowQueue.Count > 0)
        {   
            Type nextFlowType = _flowQueue.Dequeue();
            StartFlow(nextFlowType);
        }
    }
    
    // 清空流程队列
    public void ClearFlowQueue()
    {   
        _flowQueue.Clear();
        Debug.Log("流程队列已清空");
    }
    
    // 获取队列中的流程数量
    public int GetQueuedFlowCount()
    {   
        return _flowQueue.Count;
    }
    
    // 添加多个流程到队列
    public void EnqueueMultipleFlows(params Type[] flowTypes)
    {   
        foreach (Type flowType in flowTypes)
        {   
            _flowQueue.Enqueue(flowType);
            Debug.Log($"流程 {flowType.Name} 已加入队列");
        }
        
        // 如果当前没有运行流程且是顺序模式，开始执行队列中的流程
        if (_isSequentialMode && _currentFlow == null)
        {   
            StartNextQueuedFlow();
        }
    }
    
    // 检查是否有流程在队列中
    public bool HasQueuedFlows()
    {   
        return _flowQueue.Count > 0;
    }
    #endregion

    #region 事件处理方法
    // 流程结束事件处理
    private void OnFlowEndHandler()
    {   
        // 移除事件监听
        if (_currentFlow != null)
        {   
            _currentFlow.OnFlowEnd -= OnFlowEndHandler;
            _currentFlow = null;
            
            // 如果有等待的流程，启动下一个
            StartNextQueuedFlow();
        }
    }
    #endregion

    #region 获取流程方法
    // 获取指定类型的流程
    public T GetFlow<T>() where T : FlowBase
    {   
        Type flowType = typeof(T);
        if (_flowDictionary.TryGetValue(flowType, out FlowBase flow))
        {   
            return flow as T;
        }
        return null;
    }
    
    // 获取指定类型的流程
    public FlowBase GetFlow(Type flowType)
    {   
        if (_flowDictionary.TryGetValue(flowType, out FlowBase flow))
        {   
            return flow;
        }
        return null;
    }
    
    // 获取所有已注册的流程
    public List<FlowBase> GetAllFlows()
    {   
        return new List<FlowBase>(_flowDictionary.Values);
    }
    
    // 检查流程是否已注册
    public bool IsFlowRegistered<T>() where T : FlowBase
    {   
        return _flowDictionary.ContainsKey(typeof(T));
    }
    
    // 检查流程是否正在运行
    public bool IsFlowRunning<T>() where T : FlowBase
    {   
        T flow = GetFlow<T>();
        return flow != null && flow.IsRunning;
    }
    #endregion
}