using UnityEngine;

public class Trigger_Compressor : MonoBehaviour
{
    [Header("触发设置")]
    public QualityInspectionManager manager;
    public float interactDistance = 3.0f;

    private Camera mainCamera;

    void Start()
    {
        // 如果按标签找不到相机，就强制搜寻场景里的任意相机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
            if (mainCamera != null)
            {
                Debug.LogWarning($"提示：主相机未设置 'MainCamera' 标签，已自动绑定普通相机 [{mainCamera.name}]");
            }
            else
            {
                Debug.LogError("错误：场景中完全找不到任何 Camera 组件！");
            }
        }
    }

    void Update()
    {
        if (mainCamera == null || manager == null) return;

        // 在 Scene 窗口画出一条红线，方便观察射线有没有射中
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);

        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, interactDistance))
            {
                // 核心追踪：在控制台显示准星到底撞到了谁
                Debug.Log($"射线成功命中物体：[{hit.collider.gameObject.name}]");

                if (hit.collider.gameObject == this.gameObject || hit.collider.transform.IsChildOf(this.transform))
                {
                    Debug.Log("身份确认，开始触发空压机质检！");
                    manager.CallInspectCompressor();
                }
            }
        }
    }
}