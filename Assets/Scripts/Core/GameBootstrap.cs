using Saves;
using Speam.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Speam.Core
{
    /// <summary>
    /// Создаёт [GameRoot] до загрузки первой сцены, поэтому Play можно нажимать
    /// из любой сцены, а не только из MainMenu.
    /// </summary>
    public static class GameBootstrap
    {
        static bool s_created;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            s_created = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            if (s_created || GameManager.Instance != null) return;
            s_created = true;

            var root = new GameObject("[GameRoot]");
            Object.DontDestroyOnLoad(root);

            root.AddComponent<SaveManager>();
            root.AddComponent<SceneLoader>();
            root.AddComponent<LoadingScreen>();
            root.AddComponent<GameManager>();

            CreateEventSystem(root.transform);
        }

        static void CreateEventSystem(Transform parent)
        {
            if (EventSystem.current != null) return;

            // Создаём выключенным: AssignDefaultActions должен отработать до OnEnable,
            // иначе модуль не подпишется на действия и кнопки не будут нажиматься.
            var go = new GameObject("EventSystem");
            go.SetActive(false);
            go.transform.SetParent(parent, false);

            go.AddComponent<EventSystem>();
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();

            go.SetActive(true);
        }
    }
}
