using System.Collections.Generic;
using System.Linq; // N�cessaire pour le tri (Sort)
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote; // Touche �
    [SerializeField] private bool startVisible = false;

    [Header("Performance Monitoring")]
    [SerializeField] private float refreshRate = 0.5f; // Rafra�chir le texte 2x par seconde

    private bool _isVisible;
    private Rect _windowRect = new Rect(20, 20, 250, 450); // Fen�tre un peu plus haute

    // Stats variables
    private List<float> _frameTimes = new List<float>();
    private const int MAX_SAMPLES = 200; // Echantillon sur ~3 secondes � 60fps
    private float _timer;

    // Display strings (mis en cache pour �viter le GC)
    private string _fpsText = "-";
    private string _avgText = "-";
    private string _lowText = "-";
    private Color _perfColor = Color.white;

    private GUIStyle _boldLabelStyle;
    private GUIStyle _miniLabelStyle;
    private bool _stylesInitialized = false;

    private void Start()
    {
        _isVisible = startVisible;
    }

    private void InitializeGUIStyles()
    {
        if (_stylesInitialized) return;

        _boldLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold
        };

        _miniLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10
        };

        _stylesInitialized = true;
    }

    private void Update()
    {
        // 1. Toggle Fen�tre
        if (Input.GetKeyDown(toggleKey))
        {
            _isVisible = !_isVisible;
        }

        if (Time.timeScale == 0f) return;

        // 2. Collecte des donn�es (M�me si fen�tre ferm�e, pour avoir des stats pr�tes)
        // On utilise unscaledDeltaTime pour avoir les vrais FPS m�me si le jeu est ralenti/acc�l�r�
        float dt = Time.unscaledDeltaTime;
        if (dt > 0 && dt < 1.0f)
        {
            _frameTimes.Add(dt);
            if (_frameTimes.Count > MAX_SAMPLES) _frameTimes.RemoveAt(0);
        }

        // 3. Calcul p�riodique (pour ne pas faire ramer l'UI)
        _timer += dt;
        if (_timer >= refreshRate)
        {
            CalculatePerformance();
            _timer = 0f;
        }
    }
    private void OnDestroy()
    {
        _frameTimes.Clear();
        _frameTimes = null;
    }

    private void CalculatePerformance()
    {
        if (_frameTimes.Count == 0) return;

        // FPS Actuel
        float currentFps = 1.0f / Time.unscaledDeltaTime;
        _fpsText = $"{currentFps:F0}";

        // FPS Moyen
        float totalTime = 0f;
        foreach (float t in _frameTimes) totalTime += t;
        float avgFps = _frameTimes.Count / totalTime;
        _avgText = $"{avgFps:F0}";

        // 1% Low (Le pire 1% des frames)
        // On trie les temps de frame (du plus petit au plus grand)
        // Les "pires" frames (les plus longues) sont � la fin de la liste.
        List<float> sortedTimes = new List<float>(_frameTimes);
        sortedTimes.Sort();

        // On prend l'index � 99% de la liste (le d�but des 1% pires)
        int index = Mathf.Clamp(Mathf.FloorToInt(sortedTimes.Count * 0.99f), 0, sortedTimes.Count - 1);
        float worstTime = sortedTimes[index];
        float low1Fps = 1.0f / worstTime;

        _lowText = $"{low1Fps:F0}";

        // Code couleur simple
        if (avgFps > 50) _perfColor = Color.green;
        else if (avgFps > 30) _perfColor = Color.yellow;
        else _perfColor = Color.red;
    }

    private void OnGUI()
    {
        InitializeGUIStyles();

        if (!_isVisible) return;

        _windowRect = GUI.Window(0, _windowRect, DrawWindowContent, "Game Director (Debug)");
    }

    private void DrawWindowContent(int windowID)
    {
        GUILayout.BeginVertical();

        // --- SECTION PERFORMANCE (En haut ou en bas, ici en haut pour visibilit�) ---
        GUILayout.Label("--- PERFORMANCE ---", _boldLabelStyle);

        GUI.contentColor = _perfColor;
        GUILayout.BeginHorizontal();
        GUILayout.Label($"FPS: {_fpsText}", GUILayout.Width(70));
        GUILayout.Label($"Avg: {_avgText}", GUILayout.Width(70));
        GUILayout.Label($"1% Low: {_lowText}");
        GUILayout.EndHorizontal();
        GUI.contentColor = Color.white;

        GUILayout.Space(10);

        // --- GESTION DU TEMPS ---
        GUILayout.Label($"Time Scale: {Time.timeScale:F1}");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Pause")) Time.timeScale = 0f;
        if (GUILayout.Button("1x")) Time.timeScale = 1f;
        if (GUILayout.Button("2x")) Time.timeScale = 2f;
        if (GUILayout.Button("5x")) Time.timeScale = 5f;
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // --- JOUEUR ---
        GUILayout.Label("Player");
        if (GUILayout.Button("Toggle God Mode"))
        {
            if (PlayerController.Instance) PlayerController.Instance.ToggleGodMode();
        }
        if (GUILayout.Button("Heal Full"))
        {
            if (PlayerController.Instance) PlayerController.Instance.Heal(9999);
        }

        GUILayout.Space(10);

        // --- PROGRESSION ---
        GUILayout.Label("Progression");
        if (GUILayout.Button("+1 Level Up"))
        {
            if (LevelManager.Instance)
            {
                LevelManager.Instance.AddExperience(LevelManager.Instance.experienceToNextLevel);
            }
        }
        if (GUILayout.Button("+1000 XP"))
        {
            if (LevelManager.Instance) LevelManager.Instance.AddExperience(1000);
        }

        GUILayout.Space(10);

        // --- COMBAT ---
        GUILayout.Label("Combat");
        if (GUILayout.Button("KILL ALL ENEMIES"))
        {
            if (EnemyManager.Instance) EnemyManager.Instance.DebugKillAllEnemies();
        }

        GUILayout.Space(10);

        // --- ENEMY SCALING ---
        GUILayout.Label("Enemy Scaling");
        if (EnemyScalingManager.Instance != null)
        {
            GUILayout.Label($"HP Mult: {EnemyScalingManager.Instance.HpMultiplier:F2}x");
            GUILayout.Label($"DMG Mult: {EnemyScalingManager.Instance.DamageMultiplier:F2}x");
        }
        else
        {
            GUILayout.Label("(No Scaling Manager)", _miniLabelStyle);
        }

        GUILayout.Space(10);

        // --- M�MOIRE ---
        GUILayout.Label("Memory");
        if (GUILayout.Button("Force Cleanup (GC + Pools)"))
        {
            MemoryManager.ForceCleanup();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Spawn Horde (Debug)"))
        {
            // Hack rapide : utilise le pool directement
            if (EnemyPool.Instance != null && PlayerController.Instance != null)
            {
                // Pour tester, assure-toi d'avoir un EnemyData/Prefab charg� quelque part ou utilise WaveManager
                Debug.Log("Pour faire spawn, utilise plut�t l'acc�l�ration x5 du WaveManager !");
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Info: Appuie sur '�' pour fermer", _miniLabelStyle);

        GUILayout.EndVertical();

        GUI.DragWindow();
    }
}