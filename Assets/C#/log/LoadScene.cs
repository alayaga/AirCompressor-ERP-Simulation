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
        // 2. 异步加载主场景
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("新主场景");
        asyncOperation.allowSceneActivation = false;

        // 3. 等待加载完成
        while (!asyncOperation.isDone)
        {
            // 4. 加载进度（0.9表示加载完成，剩下0.1是场景激活）
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            // 5. 当加载进度达到0.9时，允许场景激活
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
