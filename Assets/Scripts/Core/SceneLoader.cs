using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Speam.Core
{
    /// <summary>Асинхронная загрузка одной сцены с прогрессом.</summary>
    public class SceneLoader : MonoBehaviour
    {
        public IEnumerator Load(string scenePathOrName, Action<float> onProgress)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(scenePathOrName, LoadSceneMode.Single);
            if (operation == null)
            {
                Debug.LogError(
                    $"SceneLoader: не удалось начать загрузку сцены '{scenePathOrName}'. " +
                    "Проверьте, что она добавлена в File → Build Settings.");
                yield break;
            }

            // Не активируем сцену, пока не покажем прогресс до конца.
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                onProgress?.Invoke(Mathf.Clamp01(operation.progress / 0.9f));
                yield return null;
            }

            onProgress?.Invoke(1f);
            yield return null;

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                onProgress?.Invoke(1f);
                yield return null;
            }
        }
    }
}
