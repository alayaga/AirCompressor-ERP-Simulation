using UnityEngine;

public class Trigger_MaterialBox : MonoBehaviour
{
    [Header("触发设置")]
    public QualityInspectionManager manager;
    public float interactDistance = 3.0f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (manager == null)
            {
                Debug.LogError("错误：纸箱身上的 Manager 槽位是空的！请拖入质检机！");
                return;
            }
            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);

            // 使用 RaycastAll 进行 X 光穿透扫描
            // 穿透货架，打穿沿途的所有碰撞体，全部抓取出来
            RaycastHit[] hits = Physics.RaycastAll(ray, interactDistance);

            bool foundTarget = false;

            // 在所有被打穿的物体清单里，翻找有没有“纸箱”
            foreach (RaycastHit hit in hits)
            {
                // 打印出 X 光到底穿透了哪些东西，方便你观察
                Debug.Log($"[X光扫描] 射线穿透了：{hit.collider.gameObject.name}");

                // 如果清单里包含了这个脚本挂载的物体（或者它的子物体）
                if (hit.collider.gameObject == this.gameObject || hit.collider.transform.IsChildOf(this.transform))
                {
                    Debug.Log("X光成功无视了货架，锁定了纸箱！开始触发质检动画！");
                    manager.CallInspectRawMaterial();
                    foundTarget = true;
                    break; // 找到了就立刻停止翻找
                }
            }

            if (!foundTarget && hits.Length > 0)
            {
                Debug.LogWarning("射线穿透了货架，但是没碰到里面的纸箱（可能没对准，或者碰撞体太小）");
            }
        }
    }
}