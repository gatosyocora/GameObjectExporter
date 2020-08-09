using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

// Copyright (c) 2020 gatosyocora
// MIT License

namespace Gatosyocora.GameObjectExporter
{
    public class GameObjectExporter : Editor
    {
        public const string PROGRESS_WINDOW_TITLE = "GameObjectExporter";

        public const string PROGRESS_WINDOW_INFO = "Exporting Object...";

        [MenuItem("GameObject/Export Unitypackage/Default", false, 1)]
        public static void ExportGameObjectDefault()
        {
            ExportGameObject();
        }

        [MenuItem("GameObject/Export Unitypackage/ignore Shader", false, 2)]
        public static void ExportGameObjectIncludeShader()
        {
            ExportGameObject(ignoreShader: true);
        }

        [MenuItem("GameObject/Export Unitypackage/ignore DynamicBone", false, 3)]
        public static void ExportGameObjectIncludeDynamicBone()
        {
            ExportGameObject(ignoreDynamicBone: true);
        }

        [MenuItem("GameObject/Export Unitypackage/ignore Shader and DynamicBone", false, 4)]
        public static void ExportGameObjectIncludeShaderAndDynamicBone()
        {
            ExportGameObject(ignoreShader: true, ignoreDynamicBone: true);
        }

        [MenuItem("GameObject/Export Unitypackage/README", false, 999)]
        public static void ShowReadme()
        {
            EditorUtility.DisplayDialog(
                            "Readme",
                            "Default: VRCSDKとEditor拡張を含めない\n" +
                            "ignore Shader: Default + Shaderを含めない\n" +
                            "ignore DynamicBone: Default + DynamicBoneを含めない\n" +
                            "ignore Shader and DynamicBone: 上記すべてを含めない\n" +
                            "\n" +
                            "DesktopにGameObject名.unitypackageで書き出されます（上書きされる）",
                            "OK");
        }

        public static void ExportGameObject(bool ignoreShader = false, bool ignoreDynamicBone = false)
        {
            EditorUtility.DisplayProgressBar(PROGRESS_WINDOW_TITLE, PROGRESS_WINDOW_INFO, 0f);

            var targetObj = Selection.activeObject as GameObject;
            var c = '/';
            var depenciesAssetPaths = EditorUtility.CollectDependencies(new UnityEngine.Object[] { targetObj })
                                        .Select(asset => AssetDatabase.GetAssetPath(asset))
                                        .Where(p => !p.Contains($"{c}VRCSDK{c}"))
                                        .Where(p => !p.Contains($"{c}Editor{c}"))
                                        .Where(p => !ignoreDynamicBone || !p.Contains($"{c}DynamicBone{c}"))
                                        .ToArray();

            var shaderRootFolders = depenciesAssetPaths
                            .Where(p => Path.GetExtension(p) == ".shader")
                            .Select(p =>
                            {
                                var folders = p.Split(c);
                                return $"{folders[0]}{c}{folders[1]}{c}";
                            })
                            .Distinct()
                            .ToArray();

            if (ignoreShader)
            {
                depenciesAssetPaths = depenciesAssetPaths
                                            .Where(p => !shaderRootFolders.Any(s => p.StartsWith(s)))
                                            .ToArray();
            }
            // Shaderを含める場合はそのShaderのEditor拡張は含める
            else
            {
                var shaderFiles = AssetDatabase.FindAssets("")
                                    .Select(g => AssetDatabase.GUIDToAssetPath(g))
                                    .Where(p => shaderRootFolders.Any(s => p.StartsWith(s)))
                                    .ToArray();

                depenciesAssetPaths = depenciesAssetPaths
                                        .Concat(shaderFiles)
                                        .Distinct()
                                        .ToArray();
            }

            EditorUtility.DisplayProgressBar(PROGRESS_WINDOW_TITLE, PROGRESS_WINDOW_INFO, 0.2f);

            var exportObj = Instantiate(targetObj);
            var prefabPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/{targetObj.name}.prefab");
            PrefabUtility.SaveAsPrefabAsset(exportObj, prefabPath, out bool success);

            var exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                                            $"{targetObj.name}.unitypackage");
            if (File.Exists(exportPath))
            {
                exportPath = AssetDatabase.GenerateUniqueAssetPath(exportPath);
            }
            var exportAssetPaths = depenciesAssetPaths.Concat(new string[] { prefabPath }).ToArray();

            EditorUtility.DisplayProgressBar(PROGRESS_WINDOW_TITLE, PROGRESS_WINDOW_INFO, 0.4f);

            AssetDatabase.ExportPackage(exportAssetPaths, exportPath, ExportPackageOptions.Default);

            AssetDatabase.DeleteAsset(prefabPath);
            GameObject.DestroyImmediate(exportObj);

            EditorUtility.ClearProgressBar();
        }
    }
}