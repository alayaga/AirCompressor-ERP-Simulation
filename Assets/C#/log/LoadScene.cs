using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        // 2. 异步加载场景中
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("新主场景");
        asyncOperation.allowSceneActivation = false;

        // 3. 等待加载完成
        while (!asyncOperation.isDone)
        {
            // 4. 返回进度，0.9表示即将完成，剩余0.1是激活时间
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            // 5. 当返回进度达到0.9时，允许场景激活
            if (asyncOperation.progress >= 0.9f)
            {
                asyncOperation.allowSceneActivation = true;
            }

            await Task.Yield();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
