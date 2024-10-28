// Cristian Pop - https://boxophobic.com/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Boxophobic.StyledGUI;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace TheVisualEngineLite
{
    public class TVELiteMaterialUpgrader : EditorWindow
    {
        float GUI_HALF_EDITOR_WIDTH = 220;

        List<string> activeScenePaths;

        bool checkAllProjectMaterials = true;

        bool requiresSceneRestart = false;
        bool validMaterialsUpgraded = false;

        bool showAdditionalSettings = false;

        Color bannerColor;
        string bannerText;
        static TVELiteMaterialUpgrader window;
        //Vector2 scrollPosition = Vector2.zero;

        [MenuItem("Window/BOXOPHOBIC/The Visual Engine Lite/Material Upgrader", false, 2007)]
        public static void ShowWindow()
        {
            window = GetWindow<TVELiteMaterialUpgrader>(false, "Material Upgrader Lite", true);
            window.minSize = new Vector2(600, 300);
        }

        void OnEnable()
        {
            bannerColor = new Color(0.75f, 0.75f, 0.75f);
            bannerText = "Material Upgrader Lite";
        }

        void OnGUI()
        {
            GUI_HALF_EDITOR_WIDTH = this.position.width / 2.0f;

            StyledGUI.DrawWindowBanner(bannerColor, bannerText);

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            if (EditorApplication.isCompiling)
            {
                GUI.enabled = false;
            }
            else
            {
                GUI.enabled = true;
            }

            if (validMaterialsUpgraded)
            {
                EditorGUILayout.HelpBox("All processed materials have been upgraded! You can run the upgrader at any time if needed!", MessageType.Info, true);

                GUILayout.Space(15);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Show Additional Settings", GUILayout.Width(GUI_HALF_EDITOR_WIDTH - 100));
                showAdditionalSettings = EditorGUILayout.Toggle(showAdditionalSettings);
                GUILayout.EndHorizontal();

                GUILayout.Space(15);

                if (showAdditionalSettings)
                {
                    if (GUILayout.Button("Restart Material Upgrading", GUILayout.Height(24)))
                    {
                        validMaterialsUpgraded = false;
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Material upgrading is required when switching render pipelines or when upgrading to a newer version. Run the material upgrader after the packages using the Lite shaders are installed!", MessageType.Info, true);

                if (!checkAllProjectMaterials)
                {
                    EditorGUILayout.HelpBox("When Check All Project Materials is disabled, the upgrader will only check folder containing a .litecollection file!", MessageType.Warning, true);
                }

                GUILayout.Space(15);

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Check All Project Materials", "When enabled, the upgrader checks all project materials regardless if the folder contains a .litecollection file inside."), GUILayout.Width(GUI_HALF_EDITOR_WIDTH - 100));
                checkAllProjectMaterials = EditorGUILayout.Toggle(checkAllProjectMaterials);
                GUILayout.EndHorizontal();

                GUILayout.Space(15);

                if (GUILayout.Button("Upgrade Project Materials", GUILayout.Height(24)))
                {
                    GetCurrentScenesSaving();
                    UpgradeMaterials();

                    RestartActiveScenes();

                    validMaterialsUpgraded = true;

                    GUIUtility.ExitGUI();
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
        }

        void UpgradeMaterials()
        {
            var allMaterialGUIDs = AssetDatabase.FindAssets("t:material", null);

            int count = 0;

            if (checkAllProjectMaterials)
            {
                var materialCount = allMaterialGUIDs.Length;

                foreach (var guids in allMaterialGUIDs)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids);

                    if (EditorUtility.DisplayCancelableProgressBar("The Visual Engine Lite", "Processing " + Path.GetFileNameWithoutExtension(path), (float)count * (1.0f / (float)materialCount)))
                    {
                        break;
                    }

                    if (path.Contains("Packages"))
                    {
                        continue;
                    }

                    var material = AssetDatabase.LoadAssetAtPath<Material>(path);

                    if (material == null)
                    {
                        continue;
                    }

                    TVELiteUtils.SetMaterialSettings(material);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    TVELiteUtils.UnloadMaterialFromMemory(material);

                    count++;
                }
            }
            else
            {
                var liteMaterialPaths = new List<string>();
                var liteCollectionPaths = AssetDatabase.FindAssets("glob:\"" + "*.litecollection" + "\"");

                for (int i = 0; i < liteCollectionPaths.Length; i++)
                {
                    var collectionPath = AssetDatabase.GUIDToAssetPath(liteCollectionPaths[i]);
                    var collectionFolder = Path.GetDirectoryName(collectionPath);
                    var collectionMaterials = Directory.GetFiles(collectionFolder, "*.mat", SearchOption.AllDirectories);

                    if (collectionMaterials != null)
                    {
                        liteMaterialPaths.AddRange(collectionMaterials);
                    }
                }

                var materialCount = liteMaterialPaths.Count;

                foreach (var path in liteMaterialPaths)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("The Visual Engine Lite", "Processing " + Path.GetFileNameWithoutExtension(path), (float)count * (1.0f / (float)materialCount)))
                    {
                        break;
                    }

                    if (path.Contains("Packages"))
                    {
                        continue;
                    }

                    var material = AssetDatabase.LoadAssetAtPath<Material>(path);

                    if (material == null)
                    {
                        continue;
                    }

                    TVELiteUtils.SetMaterialSettings(material);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    TVELiteUtils.UnloadMaterialFromMemory(material);

                    count++;

                    EditorUtility.DisplayProgressBar("The Vegetatin Engine Lite", "Processing " + Path.GetFileName(path), (float)count * (1.0f / (float)liteMaterialPaths.Count));
                }
            }

            EditorUtility.ClearProgressBar();
            Debug.Log("<b>[The Visual Engine Lite]</b> " + count + " project materials have been checked and upgraded!");
        } 

        void GetCurrentScenesSaving()
        {
            activeScenePaths = new List<string>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                activeScenePaths.Add(scene.path);
            }

            for (int i = 0; i < activeScenePaths.Count; i++)
            {
                var activeScenePath = activeScenePaths[i];

                if (activeScenePath == "")
                {
                    var saveScene = EditorUtility.DisplayDialog("Save Untitled Scene?", "The current scene is not saved to disk! Would you like to save it?", "Save New Scene", "No");

                    if (saveScene)
                    {
                        var currentScene = SceneManager.GetSceneByPath(activeScenePath);

                        var savePath = EditorUtility.SaveFilePanelInProject("Save Scene", "New Scene", "unity", "Save scene to disk!", "Assets");

                        if (savePath != "")
                        {
                            EditorSceneManager.SaveScene(currentScene, savePath);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();

                            activeScenePaths[i] = savePath;
                        }
                    }
                }
                else
                {
                    var currentScene = SceneManager.GetSceneByPath(activeScenePath);

                    if (currentScene.isDirty)
                    {
                        var saveScene = EditorUtility.DisplayDialog("Save Scene " + currentScene.name + "?", "The current scene is modified! Would you like to save it?", "Save Scene", "No");

                        if (saveScene)
                        {
                            EditorSceneManager.SaveScene(currentScene);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                    }
                }
            }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            requiresSceneRestart = true;
        }

        void RestartActiveScenes()
        {
            if (requiresSceneRestart)
            {
                for (int i = 0; i < activeScenePaths.Count; i++)
                {
                    var activeScenePath = activeScenePaths[i];

                    if (File.Exists(activeScenePath))
                    {
                        var scene = SceneManager.GetSceneByPath(activeScenePath);

                        if (i == 0)
                        {
                            EditorSceneManager.OpenScene(activeScenePath);
                        }
                        else
                        {
                            EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Additive);
                        }

                        EditorSceneManager.SaveScene(scene);

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    requiresSceneRestart = false;
                }
            }
        }
    }
}
