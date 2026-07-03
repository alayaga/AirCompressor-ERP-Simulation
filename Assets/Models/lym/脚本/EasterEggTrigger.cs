using UnityEngine;

public class EasterEggTrigger : MonoBehaviour
{
    [Header("--- 彩蛋交互设置 ---")]
    [Tooltip("玩家必须靠近彩蛋多少米以内才能触发")]
    public float interactDistance = 3f;

    void Update()
    {
        // 只有当玩家按下 E 键时，才进行射线计算，平时完全不消耗性能
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 1. 获取屏幕正中心的坐标
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

            // 2. 从主摄像机的屏幕正中心向前方发射一条射线
            Ray ray = Camera.main.ScreenPointToRay(screenCenter);
            RaycastHit hit;

            // 3. 物理射线检测：限制最大距离为你设置的 interactDistance
            if (Physics.Raycast(ray, out hit, interactDistance))
            {
                // 4. 判断射线打中的物体，是不是挂着这个脚本的“彩蛋”本身
                if (hit.collider.gameObject == this.gameObject)
                {
                    Debug.Log("发现彩蛋！呼叫总导演，启动流水线！");

                    // 精准调用你的全流程动画接口
                    FindObjectOfType<ProductionPipelineManager>().CallFullProduction();
                }
            }
        }
    }
}