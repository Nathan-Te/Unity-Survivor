using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace SurvivorGame.Localization
{
    public class RuneLocalizationBatchCreator : EditorWindow
    {
        private string _sourceFolder = "Assets/ScriptableObjects/Runes";
        private string _outputFolder = "Assets/ScriptableObjects/Localization/Runes";
        private Vector2 _scrollPosition;
        private RuneSO[] _foundRunes;
        private bool[] _selectedRunes;

        [MenuItem("Tools/Localization/Rune Localization Batch Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<RuneLocalizationBatchCreator>("Rune Loc Batch Creator");
            window.minSize = new Vector2(500, 400);
        }

        private void OnEnable()
        {
            ScanForRunes();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Rune Localization Batch Creator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This tool scans for RuneSO assets and creates corresponding RuneLocalizationData assets.\n\n" +
                "Workflow:\n" +
                "1. Specify source folder containing RuneSO assets\n" +
                "2. Click 'Scan For Runes' to find all runes\n" +
                "3. Select which runes to create localization data for\n" +
                "4. Click 'Create Selected' to generate assets",
                MessageType.Info);
            EditorGUILayout.Space(10);

            // Folder selection
            EditorGUILayout.LabelField("Folders", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _sourceFolder = EditorGUILayout.TextField("Source Folder", _sourceFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string path = EditorUtility.OpenFolderPanel("Select RuneSO Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    _sourceFolder = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _outputFolder = EditorGUILayout.TextField("Output Folder", _outputFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    _outputFolder = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (GUILayout.Button("🔍 Scan For Runes", GUILayout.Height(30)))
            {
                ScanForRunes();
            }

            EditorGUILayout.Space(10);

            // Results
            if (_foundRunes == null || _foundRunes.Length == 0)
            {
                EditorGUILayout.HelpBox("No RuneSO assets found. Click 'Scan For Runes' to search.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField($"Found {_foundRunes.Length} Runes", EditorStyles.boldLabel);

            // Select all / none buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                for (int i = 0; i < _selectedRunes.Length; i++)
                    _selectedRunes[i] = true;
            }
            if (GUILayout.Button("Select None"))
            {
                for (int i = 0; i < _selectedRunes.Length; i++)
                    _selectedRunes[i] = false;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Rune list
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));
            for (int i = 0; i < _foundRunes.Length; i++)
            {
                if (_foundRunes[i] == null) continue;

                EditorGUILayout.BeginHorizontal();

                _selectedRunes[i] = EditorGUILayout.Toggle(_selectedRunes[i], GUILayout.Width(20));

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(_foundRunes[i], typeof(RuneSO), false);
                EditorGUI.EndDisabledGroup();

                // Check if localization data already exists
                string expectedPath = GetOutputPath(_foundRunes[i]);
                bool exists = File.Exists(expectedPath);
                if (exists)
                {
                    GUI.color = Color.yellow;
                    GUILayout.Label("EXISTS", GUILayout.Width(60));
                    GUI.color = Color.white;
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            // Create button
            int selectedCount = _selectedRunes.Count(x => x);
            GUI.enabled = selectedCount > 0;

            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button($"✓ Create Selected ({selectedCount})", GUILayout.Height(40)))
            {
                CreateSelectedLocalizationData();
            }
            GUI.backgroundColor = originalColor;

            GUI.enabled = true;
        }

        private void ScanForRunes()
        {
            if (!Directory.Exists(_sourceFolder))
            {
                Debug.LogWarning($"Source folder does not exist: {_sourceFolder}");
                _foundRunes = new RuneSO[0];
                _selectedRunes = new bool[0];
                return;
            }

            // Find all RuneSO assets
            string[] guids = AssetDatabase.FindAssets("t:RuneSO", new[] { _sourceFolder });
            _foundRunes = new RuneSO[guids.Length];
            _selectedRunes = new bool[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                _foundRunes[i] = AssetDatabase.LoadAssetAtPath<RuneSO>(path);
                _selectedRunes[i] = false;
            }

            Debug.Log($"Found {_foundRunes.Length} RuneSO assets in {_sourceFolder}");
        }

        private void CreateSelectedLocalizationData()
        {
            // Ensure output folder exists
            if (!Directory.Exists(_outputFolder))
            {
                Directory.CreateDirectory(_outputFolder);
                AssetDatabase.Refresh();
            }

            int created = 0;
            int skipped = 0;

            for (int i = 0; i < _foundRunes.Length; i++)
            {
                if (!_selectedRunes[i] || _foundRunes[i] == null)
                    continue;

                string outputPath = GetOutputPath(_foundRunes[i]);

                // Check if already exists
                if (File.Exists(outputPath))
                {
                    if (!EditorUtility.DisplayDialog("Overwrite Existing Asset?",
                        $"Localization data already exists for '{_foundRunes[i].name}'.\n\nOverwrite?",
                        "Overwrite", "Skip"))
                    {
                        skipped++;
                        continue;
                    }
                }

                // Create new RuneLocalizationData
                RuneLocalizationData data = CreateInstance<RuneLocalizationData>();
                data.name = $"RuneLoc_{_foundRunes[i].name}";

                // Auto-link and import
                typeof(RuneLocalizationData)
                    .GetField("linkedRune", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(data, _foundRunes[i]);

                data.GetType().GetMethod("ImportFromLinkedRune",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(data, null);

                // Save asset
                AssetDatabase.CreateAsset(data, outputPath);
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Batch Creation Complete",
                $"Created: {created}\nSkipped: {skipped}",
                "OK");

            Debug.Log($"Batch creation complete. Created {created} assets, skipped {skipped}.");
        }

        private string GetOutputPath(RuneSO rune)
        {
            return Path.Combine(_outputFolder, $"RuneLoc_{rune.name}.asset");
        }
    }
}
