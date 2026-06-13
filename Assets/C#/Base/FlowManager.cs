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
    private Dictionary<Type, FlowBase> _flowDictionary = new Dictionary<Type, FlowBase>();
    private Queue<Type> _flowQueue = new Queue<Type>();
    private FlowBase _currentFlow = null;
    private bool _isSequentialMode = false;

    private Dictionary<Type, FlowConfigBase> _configCache = new Dictionary<Type, FlowConfigBase>();
    private Dictionary<Type, List<Type>> _flowDependencies = new Dictionary<Type, List<Type>>();
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

    public T CreateAndRegisterFlow<T>() where T : FlowBase, new()
    {
        T flow = new T();
        RegisterFlow(flow);
        return flow;
    }

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

    public void UnregisterFlow<T>() where T : FlowBase
    {
        UnregisterFlow(typeof(T));
    }
#endregion

#region 流程控制方法
    public T StartFlow<T>() where T : FlowBase, new()
    {   
        Type flowType = typeof(T);
        
        if (!_flowDictionary.ContainsKey(flowType))
        {
            CreateAndRegisterFlow<T>();
        }
        
        FlowBase flow = StartFlow(flowType);
        return flow as T;
    }

    public FlowBase StartFlow(Type flowType)
    {   
        if (_flowDictionary.TryGetValue(flowType, out FlowBase flow))
        {   
            if (_isSequentialMode && _currentFlow != null && _currentFlow.IsRunning)
            {   
                _flowQueue.Enqueue(flowType);
                Debug.Log($"流程 {flowType.Name} 已加入执行队列");
            }
            else
            {   
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

    public void StartFlowsParallel(params Type[] flowTypes)
    {   
        bool wasSequential = _isSequentialMode;
        _isSequentialMode = false;
        
        foreach (Type flowType in flowTypes)
        {   
            StartFlow(flowType);
        }
        
        _isSequentialMode = wasSequential;
    }

    public void StartFlowsSequence(params Type[] flowTypes)
    {   
        bool wasSequential = _isSequentialMode;
        _isSequentialMode = true;
        
        if (_currentFlow == null || !_currentFlow.IsRunning)
        {   
            if (flowTypes.Length > 0)
            {   
                StartFlow(flowTypes[0]);
                
                for (int i = 1; i < flowTypes.Length; i++)
                {   
                    _flowQueue.Enqueue(flowTypes[i]);
                }
            }
        }
        else
        {   
            foreach (Type flowType in flowTypes)
            {   
                _flowQueue.Enqueue(flowType);
            }
        }
        
        _isSequentialMode = wasSequential;
    }

    public void StopFlow<T>() where T : FlowBase
    {   
        StopFlow(typeof(T));
    }

    public void StopFlow(Type flowType)
    {
        if (_flowDictionary.TryGetValue(flowType, out FlowBase flow))
        {   
            flow.OnFlowEnd -= OnFlowEndHandler;
            flow.StopFlow();
            
            if (_currentFlow == flow)
            {   
                _currentFlow = null;
                StartNextQueuedFlow();
            }
        }
        else
        {   
            Debug.LogError($"找不到流程类型: {flowType.Name}");
        }
    }

    public void StopMultipleFlows(params Type[] flowTypes)
    {   
        foreach (Type flowType in flowTypes)
        {   
            StopFlow(flowType);
        }
    }

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

    public void SetSequentialMode(bool isSequential)
    {   
        _isSequentialMode = isSequential;
        Debug.Log($"流程执行模式已设置为: {(isSequential ? "顺序执行" : "并行执行")}");
    }

    public bool IsSequentialMode()
    {   
        return _isSequentialMode;
    }
#endregion

#region 队列管理方法
    private void StartNextQueuedFlow()
    {   
        if (_flowQueue.Count > 0)
        {   
            Type nextFlowType = _flowQueue.Dequeue();
            StartFlow(nextFlowType);
        }
    }

    public void ClearFlowQueue()
    {   
        _flowQueue.Clear();
        Debug.Log("流程队列已清空");
    }

    public int GetQueuedFlowCount()
    {   
        return _flowQueue.Count;
    }

    public void EnqueueMultipleFlows(params Type[] flowTypes)
    {   
        foreach (Type flowType in flowTypes)
        {   
            _flowQueue.Enqueue(flowType);
            Debug.Log($"流程 {flowType.Name} 已加入队列");
        }
        
        if (_isSequentialMode && _currentFlow == null)
        {   
            StartNextQueuedFlow();
        }
    }

    public bool HasQueuedFlows()
    {   
        return _flowQueue.Count > 0;
    }
#endregion

#region 事件处理方法
    private void OnFlowEndHandler()
    {   
        if (_currentFlow != null)
        {   
            _currentFlow.OnFlowEnd -= OnFlowEndHandler;
            _currentFlow = null;
            StartNextQueuedFlow();
        }
    }
#endregion

#region 获取流程方法
    public T GetFlow<T>() where T : FlowBase
    {   
        Type flowType = typeof(T);
        if (_flowDictionary.TryGetValue(flowType, out FlowBase flow))
        {   
            return flow as T;
        }
        return null;
    }

    public FlowBase GetFlow(Type flowType)
    {   
        if (_flowDictionary.TryGetValue(flowType, out FlowBase flow))
        {   
            return flow;
        }
        return null;
    }

    public List<FlowBase> GetAllFlows()
    {   
        return new List<FlowBase>(_flowDictionary.Values);
    }

    public bool IsFlowRegistered<T>() where T : FlowBase
    {   
        return _flowDictionary.ContainsKey(typeof(T));
    }

    public bool IsFlowRunning<T>() where T : FlowBase
    {   
        T flow = GetFlow<T>();
        return flow != null && flow.IsRunning;
    }
#endregion

#region 配置驱动接口
    public abstract class FlowConfigBase : ScriptableObject
    {
        public abstract Type FlowType { get; }
        public abstract void ApplyToFlow(FlowBase flow);
    }

    public void RegisterFlowConfig(FlowConfigBase config)
    {
        if (config == null) return;
        _configCache[config.FlowType] = config;
        Debug.Log($"流程配置注册: {config.FlowType.Name}");
    }

    public void SetFlowDependency<TDependent, TDependency>() 
        where TDependent : FlowBase 
        where TDependency : FlowBase
    {
        var dependent = typeof(TDependent);
        var dependency = typeof(TDependency);
        
        if (!_flowDependencies.ContainsKey(dependent))
            _flowDependencies[dependent] = new List<Type>();
        
        _flowDependencies[dependent].Add(dependency);
        Debug.Log($"流程依赖: {dependent.Name} 依赖 {dependency.Name}");
    }
#endregion
}