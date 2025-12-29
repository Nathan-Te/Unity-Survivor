#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace SurvivorGame.Localization.Editor
{
    /// <summary>
    /// Editor tools to help with localization workflow.
    /// Access via: Tools > Localization Tools
    /// </summary>
    public class LocalizationEditorTools : EditorWindow
    {
        private Vector2 _scrollPosition;
        private List<string> _foundHardcodedStrings = new List<string>();

        [MenuItem("Tools/Localization Tools")]
        public static void ShowWindow()
        {
            var window = GetWindow<LocalizationEditorTools>("Localization Tools");
            window.minSize = new Vector2(400, 300);
        }

        private void OnGUI()
        {
            GUILayout.Label("Localization Utilities", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // Quick Links
            if (GUILayout.Button("Open README", GUILayout.Height(30)))
            {
                string path = "Assets/Scripts/Localization/README.md";
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (asset != null)
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
            }

            if (GUILayout.Button("Open Setup Guide", GUILayout.Height(30)))
            {
                string path = "Assets/Scripts/Localization/LOCALIZATION_SETUP.md";
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (asset != null)
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
            }

            if (GUILayout.Button("Open Quick Reference", GUILayout.Height(30)))
            {
                string path = "Assets/Scripts/Localization/QUICK_REFERENCE.md";
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (asset != null)
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
            }

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);

            GUILayout.Label("Asset Creation", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (GUILayout.Button("Create LocalizedString Asset", GUILayout.Height(25)))
            {
                CreateLocalizedStringAsset();
            }

            if (GUILayout.Button("Create Localization Table Asset", GUILayout.Height(25)))
            {
                CreateLocalizationTableAsset();
            }

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);

            GUILayout.Label("Validation", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (GUILayout.Button("Find LocalizationManager in Scene", GUILayout.Height(25)))
            {
                FindLocalizationManager();
            }

            if (GUILayout.Button("Validate Localization Tables", GUILayout.Height(25)))
            {
                ValidateLocalizationTables();
            }

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);

            GUILayout.Label("Migration Helpers", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Use these tools to find scripts that need migration to the localization system.",
                MessageType.Info
            );

            if (GUILayout.Button("Scan for Hardcoded French Strings", GUILayout.Height(25)))
            {
                ScanForHardcodedStrings();
            }

            // Display results
            if (_foundHardcodedStrings.Count > 0)
            {
                EditorGUILayout.Space(10);
                GUILayout.Label($"Found {_foundHardcodedStrings.Count} potential issues:", EditorStyles.boldLabel);

                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));
                foreach (var result in _foundHardcodedStrings)
                {
                    EditorGUILayout.LabelField(result, EditorStyles.wordWrappedLabel);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void CreateLocalizedStringAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create LocalizedString",
                "NewLocalizedString",
                "asset",
                "Choose location for LocalizedString asset",
                "Assets/ScriptableObjects/Localization"
            );

            if (!string.IsNullOrEmpty(path))
            {
                var asset = ScriptableObject.CreateInstance<LocalizedString>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
                Debug.Log($"Created LocalizedString at: {path}");
            }
        }

        private void CreateLocalizationTableAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Localization Table",
                "NewLocalizationTable",
                "asset",
                "Choose location for Localization Table",
                "Assets/ScriptableObjects/Localization"
            );

            if (!string.IsNullOrEmpty(path))
            {
                var asset = ScriptableObject.CreateInstance<LocalizationTable>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
                Debug.Log($"Created LocalizationTable at: {path}");
            }
        }

        private void FindLocalizationManager()
        {
            var manager = Object.FindFirstObjectByType<LocalizationManager>();

            if (manager != null)
            {
                Selection.activeGameObject = manager.gameObject;
                EditorGUIUtility.PingObject(manager.gameObject);
                Debug.Log("✓ LocalizationManager found in scene!");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "LocalizationManager Not Found",
                    "No LocalizationManager found in the current scene.\n\n" +
                    "Create one:\n" +
                    "1. Create Empty GameObject\n" +
                    "2. Name it 'LocalizationManager'\n" +
                    "3. Add LocalizationManager component\n" +
                    "4. Assign localization tables",
                    "OK"
                );
            }
        }

        private void ValidateLocalizationTables()
        {
            var tables = FindAssetsByType<LocalizationTable>();

            if (tables.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Tables Found",
                    "No LocalizationTable assets found in project.\n\n" +
                    "Create tables using the 'Create Localization Table Asset' button above.",
                    "OK"
                );
                return;
            }

            Debug.Log("=== Localization Table Validation ===");

            foreach (var table in tables)
            {
                Debug.Log($"\n<b>Table: {table.TableName}</b>");
                var keys = table.GetAllKeys();
                Debug.Log($"  Entries: {keys.Count}");

                if (string.IsNullOrEmpty(table.TableName))
                {
                    Debug.LogWarning($"  ⚠ Table at {AssetDatabase.GetAssetPath(table)} has no TableName set!", table);
                }
            }

            Debug.Log("\n=== Validation Complete ===");
            EditorUtility.DisplayDialog(
                "Validation Complete",
                $"Found {tables.Count} localization table(s).\n\nCheck Console for details.",
                "OK"
            );
        }

        private void ScanForHardcodedStrings()
        {
            _foundHardcodedStrings.Clear();

            // Common French words that indicate hardcoded strings
            string[] frenchIndicators = new string[]
            {
                "Ennemis", "ennemis",
                "Kills", "kills",
                "Score", "score",
                "Combo", "combo",
                "Récompense", "récompense",
                "Choisissez", "choisissez",
                "Appliquer", "appliquer",
                "Incompatible", "incompatible",
                "Modificateur", "modificateur",
                "Bannissement", "bannissement"
            };

            // Find all .cs files in UI folders
            string[] uiFolders = new string[]
            {
                "Assets/Scripts/UI",
                "Assets/Scripts/Progression"
            };

            List<string> filesToCheck = new List<string>();
            foreach (var folder in uiFolders)
            {
                string[] files = System.IO.Directory.GetFiles(folder, "*.cs", System.IO.SearchOption.AllDirectories);
                filesToCheck.AddRange(files);
            }

            foreach (var filePath in filesToCheck)
            {
                string fileContent = System.IO.File.ReadAllText(filePath);
                string fileName = System.IO.Path.GetFileName(filePath);

                foreach (var indicator in frenchIndicators)
                {
                    if (fileContent.Contains($"\"{indicator}"))
                    {
                        _foundHardcodedStrings.Add($"⚠ {fileName}: Found \"{indicator}\"");
                    }
                }
            }

            if (_foundHardcodedStrings.Count == 0)
            {
                _foundHardcodedStrings.Add("✓ No obvious hardcoded strings found!");
            }

            Debug.Log($"Scanned {filesToCheck.Count} UI scripts. Found {_foundHardcodedStrings.Count} potential issues.");
        }

        private List<T> FindAssetsByType<T>() where T : Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }
    }
}
#endif
