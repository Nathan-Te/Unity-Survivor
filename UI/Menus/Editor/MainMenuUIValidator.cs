using UnityEngine;
using UnityEditor;
using SurvivorGame.UI;

namespace SurvivorGame.Editor
{
    /// <summary>
    /// Custom editor validator for MainMenuUI to ensure all critical references are assigned.
    /// </summary>
    [CustomEditor(typeof(MainMenuUI))]
    public class MainMenuUIValidator : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            MainMenuUI menuUI = (MainMenuUI)target;

            // Add a visual separator
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

            // Check goldText reference
            var goldTextProperty = serializedObject.FindProperty("goldText");
            if (goldTextProperty != null && goldTextProperty.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(
                    "WARNING: 'Gold Text' is not assigned! Player gold will not display in the main menu. " +
                    "Please assign a TextMeshProUGUI component to show the player's gold amount.",
                    MessageType.Error
                );

                if (GUILayout.Button("Find Gold Text Automatically"))
                {
                    // Try to find a TextMeshProUGUI with "gold" in the name
                    var textComponents = menuUI.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                    foreach (var text in textComponents)
                    {
                        if (text.name.ToLower().Contains("gold"))
                        {
                            goldTextProperty.objectReferenceValue = text;
                            serializedObject.ApplyModifiedProperties();
                            Debug.Log($"[MainMenuUIValidator] Auto-assigned Gold Text: {text.name}");
                            break;
                        }
                    }

                    if (goldTextProperty.objectReferenceValue == null)
                    {
                        Debug.LogWarning("[MainMenuUIValidator] Could not find a TextMeshProUGUI with 'gold' in the name. Please assign manually.");
                    }
                }
            }
            else if (goldTextProperty != null && goldTextProperty.objectReferenceValue != null)
            {
                // Check if goldText has SimpleLocalizedText or LocalizedTextMeshPro attached
                var goldTextComponent = goldTextProperty.objectReferenceValue as TMPro.TextMeshProUGUI;
                if (goldTextComponent != null)
                {
                    var simpleLocalizedText = goldTextComponent.GetComponent<SurvivorGame.Localization.SimpleLocalizedText>();
                    var localizedTextMeshPro = goldTextComponent.GetComponent<SurvivorGame.Localization.LocalizedTextMeshPro>();

                    if (simpleLocalizedText != null || localizedTextMeshPro != null)
                    {
                        EditorGUILayout.HelpBox(
                            "CRITICAL ERROR: Gold Text has a localization component attached (SimpleLocalizedText or LocalizedTextMeshPro)! " +
                            "This component will OVERWRITE the dynamic gold value. Gold is a DYNAMIC text, not a static localized text. " +
                            "Remove the localization component from this GameObject.",
                            MessageType.Error
                        );

                        if (GUILayout.Button("Remove Localization Component Automatically"))
                        {
                            if (simpleLocalizedText != null)
                            {
                                DestroyImmediate(simpleLocalizedText);
                                Debug.Log($"[MainMenuUIValidator] Removed SimpleLocalizedText from {goldTextComponent.name}");
                            }
                            if (localizedTextMeshPro != null)
                            {
                                DestroyImmediate(localizedTextMeshPro);
                                Debug.Log($"[MainMenuUIValidator] Removed LocalizedTextMeshPro from {goldTextComponent.name}");
                            }
                            EditorUtility.SetDirty(goldTextComponent.gameObject);
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Gold Text is properly assigned and has no conflicting components.", MessageType.Info);
                    }
                }
            }

            // Validate panels
            ValidatePanel("mainMenuPanel");
            ValidatePanel("levelSelectionPanel");
            ValidatePanel("upgradesPanel");
            ValidatePanel("settingsPanel");
            ValidatePanel("leaderboardPanel");
        }

        private void ValidatePanel(string panelName)
        {
            var property = serializedObject.FindProperty(panelName);
            if (property != null && property.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox($"'{panelName}' is not assigned!", MessageType.Warning);
            }
        }
    }
}
