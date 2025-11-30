using UnityEngine;

public class GameDirector : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote; // Touche ² (au dessus de Tab)
    [SerializeField] private bool startVisible = false;

    private bool _isVisible;
    private Rect _windowRect = new Rect(20, 20, 250, 350); // Position et taille

    private void Start()
    {
        _isVisible = startVisible;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            _isVisible = !_isVisible;
        }
    }

    private void OnGUI()
    {
        if (!_isVisible) return;

        // Création d'une fenêtre draggable
        _windowRect = GUI.Window(0, _windowRect, DrawWindowContent, "Game Director (Debug)");
    }

    private void DrawWindowContent(int windowID)
    {
        GUILayout.BeginVertical();

        // --- GESTION DU TEMPS ---
        GUILayout.Label("Time Scale: " + Time.timeScale.ToString("F1"));
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
            // Note: Tu devras peut-être passer _currentHp en public ou faire une méthode Heal()
            // Pour l'instant on suppose que tu l'ajouteras
            Debug.Log("Heal triggered (à implémenter dans PlayerController)");
        }

        GUILayout.Space(10);

        // --- PROGRESSION ---
        GUILayout.Label("Progression");
        if (GUILayout.Button("+1 Level Up"))
        {
            if (LevelManager.Instance)
            {
                // Donne exactement ce qu'il faut pour passer le niveau
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

        if (GUILayout.Button("Spawn Horde (10)"))
        {
            // Petit hack pour faire spawn 10 ennemis autour du joueur
            for (int i = 0; i < 10; i++)
            {
                // On utilise la méthode interne du WaveManager si on la rendait publique, 
                // ou on passe par le pool manuellement.
                // Pour simplifier, on ne le fait pas ici sans modifier WaveManager.
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Info: Appuie sur '²' pour fermer");

        GUILayout.EndVertical();

        // Rend la fenêtre déplaçable
        GUI.DragWindow();
    }
}