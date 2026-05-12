using UnityEngine;

/// <summary>
/// 世界空间UI面向玩家脚本
/// 用于使世界坐标的画布始终面向玩家相机
/// </summary>
public class WorldSpaceUIFacer : MonoBehaviour
{
    [SerializeField]
    private Transform playerTransform;
    
    [SerializeField]
    private GameObject canvasObject;
    
    private Camera playerCamera;
    
    private void Start()
    {
        // 如果没有指定玩家，尝试获取玩家对象
        if (playerTransform == null)
        {
            if (ObjectManager.Instance != null)
            {
                GameObject playerObject = ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player);
                if (playerObject != null)
                {
                    playerTransform = playerObject.transform;
                }
            }
            
            // 如果仍然没有找到，尝试通过标签查找
            if (playerTransform == null)
            {
                GameObject playerObject = GameObject.FindWithTag("Player");
                if (playerObject != null)
                {
                    playerTransform = playerObject.transform;
                }
            }
        }
        
        // 获取玩家相机
        if (playerTransform != null)
        {
            playerCamera = playerTransform.GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }
        
        // 如果没有指定画布，默认使用自身或父物体上的Canvas组件
        if (canvasObject == null)
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvasObject = canvas.gameObject;
            }
            else
            {
                canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    canvasObject = canvas.gameObject;
                }
            }
        }
    }
    
    private void LateUpdate()
    {
        // 确保有玩家相机和画布对象
        if (playerCamera != null && canvasObject != null)
        {
            // 计算目标旋转，使画布面向相机
            Vector3 directionToCamera = playerCamera.transform.position - canvasObject.transform.position;
            directionToCamera.y = 0; // 可选：保持画布水平，只在XZ平面上旋转
            
            if (directionToCamera != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);
                canvasObject.transform.rotation = targetRotation;
            }
        }
    }
}