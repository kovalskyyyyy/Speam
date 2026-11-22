using System;
using System.Collections;
using System.IO;
using DialogueSystem.Runtime;
using Saves;
using Speam.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace Speam.Core
{
    public enum GameFlowState
    {
        Boot,
        MainMenu,
        Loading,
        Gameplay,
        Paused
    }

    /// <summary>
    /// Единственный оркестратор игры: меню, загрузка сцен, новая игра и продолжение.
    /// Живёт на [GameRoot] и не уничтожается при смене сцен.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameFlowState State { get; private set; } = GameFlowState.Boot;
        public bool CanPlayerAct => State == GameFlowState.Gameplay;
        public bool IsLoading => State == GameFlowState.Loading;

        public event Action<GameFlowState> StateChanged;

        /// <summary>Путь сцены меню без расширения .unity.</summary>
        public string MainMenuScenePath { get; private set; }

        /// <summary>Путь игровой сцены без расширения .unity.</summary>
        public string GameplayScenePath { get; private set; }

        SceneLoader m_loader;
        LoadingScreen m_loading;
        bool m_transitioning;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            m_loader = GetComponent<SceneLoader>();
            if (m_loader == null) m_loader = gameObject.AddComponent<SceneLoader>();

            m_loading = GetComponent<LoadingScreen>();
            if (m_loading == null) m_loading = gameObject.AddComponent<LoadingScreen>();

            ResolveScenePaths();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        // ---------------- Публичное API ----------------

        public void StartNewGame()
        {
            if (IsLoading) return;

            GameState.Clear();
            StartCoroutine(LoadRoutine(GameplayScenePath, null, GameFlowState.Gameplay));
        }

        public void ContinueGame(int slot = 0)
        {
            if (IsLoading) return;

            var save = SaveManager.Instance != null ? SaveManager.Instance.Read(slot) : null;
            if (save == null)
            {
                Debug.LogWarning($"GameManager: сейв в слоте {slot} не найден или повреждён.");
                return;
            }

            StartCoroutine(LoadRoutine(GameplayScenePath, save, GameFlowState.Gameplay));
        }

        public void GoToMainMenu()
        {
            if (IsLoading) return;

            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            StartCoroutine(LoadRoutine(MainMenuScenePath, null, GameFlowState.MainMenu));
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public bool SaveGame(int slot = 0)
        {
            return SaveManager.Instance != null && SaveManager.Instance.Save(slot);
        }

        public void SetPaused(bool paused)
        {
            if (paused)
            {
                if (State != GameFlowState.Gameplay) return;
                Time.timeScale = 0f;
                SetState(GameFlowState.Paused);
            }
            else
            {
                if (State != GameFlowState.Paused) return;
                Time.timeScale = 1f;
                SetState(GameFlowState.Gameplay);
            }
        }

        // ---------------- Загрузка ----------------

        IEnumerator LoadRoutine(string scenePath, SaveData save, GameFlowState target)
        {
            m_transitioning = true;
            SetState(GameFlowState.Loading);
            AbortDialogue();

            if (m_loading != null)
            {
                m_loading.Show();
                m_loading.SetProgress(0f);
            }

            yield return m_loader.Load(scenePath, progress =>
            {
                if (m_loading != null) m_loading.SetProgress(progress);
            });

            if (save != null)
            {
                if (SaveManager.Instance != null) SaveManager.Instance.Apply(save);
            }
            else if (target == GameFlowState.Gameplay)
            {
                PlacePlayerAtStart();
            }

            if (m_loading != null)
            {
                m_loading.SetProgress(1f);
                yield return null;
                m_loading.Hide();
            }

            Time.timeScale = 1f;
            m_transitioning = false;
            SetState(target);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureEventSystem();

            bool isMenu = IsScene(scene, MainMenuScenePath);
            bool isGameplay = IsScene(scene, GameplayScenePath);

            if (isMenu) EnsureMainMenu();
            if (isGameplay) EnsureGameplayHud();

            if (m_transitioning) return;

            SetState(isMenu ? GameFlowState.MainMenu : GameFlowState.Gameplay);
        }

        /// <summary>
        /// Оставляет ровно один EventSystem: наш, с InputSystemUIInputModule.
        /// EventSystem из сцены с StandaloneInputModule ломает uGUI при включённом Input System.
        /// </summary>
        void EnsureEventSystem()
        {
            var mine = GetComponentInChildren<EventSystem>(true);
            if (mine == null)
            {
                // Выключенный объект: AssignDefaultActions должен успеть до OnEnable.
                var go = new GameObject("EventSystem");
                go.SetActive(false);
                go.transform.SetParent(transform, false);

                go.AddComponent<EventSystem>();
                var module = go.AddComponent<InputSystemUIInputModule>();
                module.AssignDefaultActions();

                go.SetActive(true);
                mine = go.GetComponent<EventSystem>();
            }

            foreach (var other in FindObjectsByType<EventSystem>(FindObjectsInactive.Include))
            {
                if (other == null || other == mine) continue;
                Destroy(other.gameObject);
            }
        }

        // Reference UI prefabs located in Assets/Resources/UI/
        // Designer creates: Assets/Resources/UI/MainMenu.prefab and Assets/Resources/UI/GameplayHUD.prefab
        // No Inspector assignment needed - works automatically via Resources.Load

        void EnsureMainMenu()
        {
            if (FindAnyObjectByType<MainMenuController>() != null) return;

            // Try to load from Resources folder
            var prefab = Resources.Load<GameObject>("UI/MainMenu");
            if (prefab != null)
            {
                Instantiate(prefab, transform);
                return;
            }

            // Fallback: create basic (same as original code)
            new GameObject("[MainMenuUI]").AddComponent<MainMenuController>();
        }

        void EnsureGameplayHud()
        {
            if (FindAnyObjectByType<GameplayHud>() != null) return;

            // Try to load from Resources folder
            var prefab = Resources.Load<GameObject>("UI/GameplayHUD");
            if (prefab != null)
            {
                Instantiate(prefab, transform);
                return;
            }

            // Fallback: create basic
            new GameObject("[GameplayHUD]").AddComponent<GameplayHud>();
        }

        static void AbortDialogue()
        {
            if (DialogueRunner.Instance != null) DialogueRunner.Instance.Abort();
        }

        static void PlacePlayerAtStart()
        {
            var spawn = PlayerSpawnPoint.Find("Start");
            if (spawn == null) return;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            Vector3 position = spawn.transform.position;
            var agent = player.GetComponentInChildren<NavMeshAgent>();
            if (agent != null) agent.Warp(position);
            player.transform.position = position;
        }

        // ---------------- Вспомогательное ----------------

        void SetState(GameFlowState state)
        {
            if (State == state) return;
            State = state;
            StateChanged?.Invoke(state);
        }

        void ResolveScenePaths()
        {
            // Полные пути из Build Settings: так мы не путаем одноимённые сцены.
            MainMenuScenePath = SceneUtility.GetScenePathByBuildIndex(0);
            GameplayScenePath = SceneUtility.GetScenePathByBuildIndex(1);

            if (string.IsNullOrEmpty(MainMenuScenePath))
                MainMenuScenePath = "Assets/Scenes/Build/MainMenu.unity";

            if (string.IsNullOrEmpty(GameplayScenePath))
                GameplayScenePath = "Assets/Scenes/WalkingTest/WalkingTest.unity";
        }

        static bool IsScene(Scene scene, string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath)) return false;

            if (string.Equals(scene.path, scenePath, StringComparison.OrdinalIgnoreCase)) return true;

            // Запасной вариант — сравнение по имени без расширения.
            string name = Path.GetFileNameWithoutExtension(scenePath);
            return !string.IsNullOrEmpty(name)
                && string.Equals(scene.name, name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
