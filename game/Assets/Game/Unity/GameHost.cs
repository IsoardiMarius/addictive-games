using Game.Core;
using UnityEngine;

namespace Game.Unity
{
    /// <summary>
    /// Démarre le jeu tout seul, quelle que soit la scène ouverte : aucun réglage manuel dans l'éditeur.
    /// </summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Object.FindAnyObjectByType<GameHost>() != null) return;
            var go = new GameObject("DigForReal");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<GameHost>();
        }
    }

    /// <summary>
    /// Pont entre Unity et le jeu (Game.Core) : transmet l'écran, le doigt et le temps,
    /// puis affiche la liste de rectangles et joue les sons demandés. Toute la logique est dans Game.Core.
    /// </summary>
    public sealed class GameHost : MonoBehaviour
    {
        private GameApp _app;
        private Camera _cam;
        private MeshBatcher _batcher;
        private UnityAudio _audio;
        private int _w, _h;
        private Rect _safe;
        private bool _pointerDown;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;

            // Caméra dédiée : les autres caméras de la scène sont désactivées.
            foreach (Camera other in Camera.allCameras) other.enabled = false;
            var camGo = new GameObject("Camera");
            camGo.transform.SetParent(transform, false);
            _cam = camGo.AddComponent<Camera>();
            _cam.orthographic = true;
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = new Color(0.02f, 0.024f, 0.06f, 1f);
            _cam.nearClipPlane = 0.1f;
            _cam.farClipPlane = 100f;
            _cam.depth = 100f;
            _cam.allowHDR = false;
            _cam.allowMSAA = false;
            _cam.useOcclusionCulling = false;
            if (Object.FindAnyObjectByType<AudioListener>() == null) camGo.AddComponent<AudioListener>();

            TextAsset font = Resources.Load<TextAsset>("Fonts/GameFont");
            if (font == null)
            {
                Debug.LogError("Dig For Real : police introuvable (Assets/Game/Resources/Fonts/GameFont.bytes).");
                enabled = false;
                return;
            }

            float aspect = Screen.height / (float)Mathf.Max(1, Screen.width);
            float pixelsPerPoint = aspect >= 16f / 9f ? Screen.width / GameApp.ViewW : Screen.height / (GameApp.ViewW * 16f / 9f);
            _app = new GameApp(new UnityPlatform(), font.bytes, pixelsPerPoint);
            _batcher = new MeshBatcher(gameObject, _app.Atlas);
            _audio = new UnityAudio(gameObject);
            ApplyScreen();
        }

        private void ApplyScreen()
        {
            _w = Screen.width; _h = Screen.height; _safe = Screen.safeArea;
            float safeTop = Mathf.Max(0f, _h - (_safe.y + _safe.height));
            float safeBottom = Mathf.Max(0f, _safe.y);
            _app.Resize(_w, _h, safeTop, safeBottom);
            // Le jeu dessine en points, origine en haut à gauche, y vers le bas : la caméra cadre exactement cette zone.
            _cam.orthographicSize = _app.ViewH * 0.5f;
            _cam.transform.position = new Vector3(_app.ScreenW * 0.5f, -_app.ViewH * 0.5f, -10f);
        }

        private void Update()
        {
            if (_app == null) return;
            if (Screen.width != _w || Screen.height != _h || Screen.safeArea != _safe) ApplyScreen();
            ReadInput();
            _app.Frame(Time.unscaledDeltaTime);
            _batcher.Upload(_app.List);
            _audio.Flush(_app.Audio);
        }

        private void Pointer(bool down, float x, float yFromBottom)
        {
            float y = _h - yFromBottom;
            if (down && !_pointerDown) { _pointerDown = true; _app.PointerDown(x, y); }
            else if (down) _app.PointerMove(x, y);
            else if (_pointerDown) { _pointerDown = false; _app.PointerUp(x, y); }
        }

        private void ReadInput()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                bool down = t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled;
                Pointer(down, t.position.x, t.position.y);
            }
            else
            {
                Vector3 m = Input.mousePosition;
                Pointer(Input.GetMouseButton(0), m.x, m.y);
            }
            if (Input.GetKeyDown(KeyCode.Escape) && !_app.Back()) Application.Quit();
#elif DFR_HAS_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
            UnityEngine.InputSystem.Pointer p = UnityEngine.InputSystem.Pointer.current;
            if (p != null)
            {
                Vector2 pos = p.position.ReadValue();
                Pointer(p.press.isPressed, pos.x, pos.y);
            }
            UnityEngine.InputSystem.Keyboard k = UnityEngine.InputSystem.Keyboard.current;
            if (k != null && k.escapeKey.wasPressedThisFrame && !_app.Back()) Application.Quit();
#else
            // Aucun système d'entrée actif : voir Project Settings > Player > Active Input Handling.
#endif
        }

        private void OnApplicationPause(bool paused)
        {
            if (_app == null) return;
            if (paused) _app.OnPause(); else _app.OnResume();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (_app != null && !focus) _app.Save();
        }

        private void OnApplicationQuit()
        {
            if (_app != null) _app.OnPause();
        }
    }
}
