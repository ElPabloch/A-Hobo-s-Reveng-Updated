using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;
using TMPro;

namespace Hobo.Editor
{
    public class PixelImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Hobo/Resources/Art/"))
                return;
            var t = (TextureImporter)assetImporter;
            t.textureType = TextureImporterType.Default;
            t.filterMode = FilterMode.Point;
            t.mipmapEnabled = false;
            t.textureCompression = TextureImporterCompression.Uncompressed;
            t.npotScale = TextureImporterNPOTScale.None;
            t.alphaIsTransparency = true;
            t.maxTextureSize = 2048;
        }
    }

    public static class HoboSetup
    {
        [MenuItem("Hobo/Prepare Campaign")]
        public static string Prepare()
        {
            if (EditorApplication.isPlaying)
                throw new Exception("Stop Play Mode first");
            if (EditorSceneManager.GetActiveScene().isDirty)
                EditorSceneManager.SaveOpenScenes();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new(15, 10, -10);
            camera.orthographic = true;
            camera.orthographicSize = 10;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.035f, .05f, .055f);
            camera.allowHDR = camera.allowMSAA = camera.allowDynamicResolution = false;
            if (!(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
                throw new Exception("URP required");
            var pp = camera.gameObject.AddComponent<PixelPerfectCamera>();
            pp.assetsPPU = 16;
            pp.refResolutionX = 480;
            pp.refResolutionY = 300;
            pp.gridSnapping = PixelPerfectCamera.GridSnapping.PixelSnapping;
            pp.cropFrame = PixelPerfectCamera.CropFrame.Windowbox;
            var game = new GameObject("Campaign").AddComponent<CampaignGame>();
            game.gameCamera = camera;
            string fontPath = "Assets/Hobo/Resources/HoboFont.asset";
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            if (font == null)
            {
                font = TMP_FontAsset.CreateFontAsset(Resources.Load<Font>("Roboto-Regular"), 32, 4, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
                font.name = "HoboFont";
                AssetDatabase.CreateAsset(font, fontPath);
                AssetDatabase.AddObjectToAsset(font.material, font);
                foreach (var tex in font.atlasTextures)
                    AssetDatabase.AddObjectToAsset(tex, font);
                string glyphs = "";
                for (int c = 32; c < 127; c++)
                    glyphs += (char)c;
                for (int c = 0x400; c <= 0x45f; c++)
                    glyphs += (char)c;
                glyphs += "—–→·«»№…";
                font.TryAddCharacters(glyphs);
                EditorUtility.SetDirty(font);
            }

            game.font = font;
            QualitySettings.antiAliasing = 0;
            PlayerSettings.companyName = "Smolnogorsk42";
            PlayerSettings.productName = "A Hobo's Reveng";
            PlayerSettings.defaultScreenWidth = 960;
            PlayerSettings.defaultScreenHeight = 640;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = true;
            Directory.CreateDirectory("Assets/Hobo/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Hobo/Scenes/Campaign.unity");
            EditorBuildSettings.scenes = new[]{new EditorBuildSettingsScene("Assets/Hobo/Scenes/Campaign.unity", true)};
            AssetDatabase.SaveAssets();
            return "Campaign scene prepared";
        }

        [MenuItem("Hobo/Build Windows")]
        public static string Build()
        {
            Directory.CreateDirectory("Builds/Windows");
            var r = UnityEditor.BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes = new[]{"Assets/Hobo/Scenes/Campaign.unity"}, locationPathName = "Builds/Windows/A Hobos Reveng.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.None});
            if (r.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new Exception("Build failed: " + r.summary.result);
            return r.summary.outputPath;
        }
    }
}

