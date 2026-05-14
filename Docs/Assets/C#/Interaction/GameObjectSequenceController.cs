using System.Collections;
using UnityEngine;

public class GameObjectSequenceController : MonoBehaviour
{
    [Header("物体序列设置")]
    [Tooltip("子物体组件数组")]
    private GameObject[] objectComponents;

    /// <summary>
    /// 初始化方法，在OnEnable中调用
    /// </summary>
    private void Initialize()
    {
        // 获取所有子物体
        int childCount = transform.childCount;
        objectComponents = new GameObject[childCount];

        // 将子物体添加到数组并全部隐藏
        for (int i = 0; i < childCount; i++)
        {
            objectComponents[i] = transform.GetChild(i).gameObject;
            objectComponents[i].SetActive(false);
        }

        Debug.Log($"初始化完成，共找到 {childCount} 个子物体");
    }

    /// <summary>
    /// 在OnEnable时调用初始化方法
    /// </summary>
    private void OnEnable()
    {
        Initialize();
    }

    /// <summary>
    /// 公共协程：按照数组索引依次显示物体
    /// </summary>
    /// <param name="delayBetweenObjects">物体之间的显示延迟时间（秒）</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator ShowObjectsSequentially(float delayBetweenObjects = 0.5f)
    {
        if (objectComponents == null || objectComponents.Length == 0)
        {
            Debug.LogWarning("物体组件数组为空，请确保已初始化");
            yield break;
        }

        // 依次显示每个物体
        for (int i = 0; i < objectComponents.Length; i++)
        {
            if (objectComponents[i] != null)
            {
                objectComponents[i].SetActive(true);
                Debug.Log($"显示物体索引: {i}");
                
                // 等待指定时间后显示下一个物体
                yield return new WaitForSeconds(delayBetweenObjects);
            }
        }

        Debug.Log("所有物体显示完成");
    }

    /// <summary>
    /// 重置所有物体为隐藏状态
    /// </summary>
    public void ResetObjects()
    {
        if (objectComponents != null)
        {
            foreach (GameObject obj in objectComponents)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
            Debug.Log("所有物体已重置为隐藏状态");
        }
    }
}