using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.EditorTools
{
    /// <summary>
    /// Configure le projet et construit les versions de test, sans réglage manuel.
    /// Menu « Dig For Real » dans l'éditeur, ou en ligne de commande :
    ///   Unity -batchmode -quit -projectPath game -executeMethod Game.EditorTools.ProjectSetup.BuildAndroid
    /// </summary>
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";
        // Identifiant provisoire : à remplacer par le vôtre avant toute publication.
        private const string AppId = "com.example.digforreal";
        private const string AppVersion = "0.1.0";

        static ProjectSetup()
        {
            EditorApplication.delayCall += AutoSetup;
        }

        private static string DoneKey { get { return "DigForReal.Setup.v1." + Application.dataPath; } }

        private static void AutoSetup()
        {
            if (EditorPrefs.GetBool(DoneKey, false) && File.Exists(ScenePath)) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            Scene active = SceneManager.GetActiveScene();
            if (!File.Exists(ScenePath) && (!string.IsNullOrEmpty(active.path) || active.isDirty))
            {
                Debug.Log("Dig For Real : utilisez le menu « Dig For Real > 1. Configurer le projet » pour terminer l'installation.");
                return;
            }
            Apply();
        }

        [MenuItem("Dig For Real/1. Configurer le projet")]
        public static void Apply()
        {
            PlayerSettings.companyName = "DigForReal";
            PlayerSettings.productName = "Dig For Real";
            PlayerSettings.bundleVersion = AppVersion;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)24;
            // Les téléphones récents n'acceptent que le 64 bits, qui exige IL2CPP.
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
#if UNITY_2021_2_OR_NEWER
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, AppId);
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS, AppId);
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
#else
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, AppId);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, AppId);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
#endif
            EnsureScene();
            EditorPrefs.SetBool(DoneKey, true);
            AssetDatabase.SaveAssets();
            Debug.Log("Dig For Real : projet configuré. Appuyez sur Lecture pour jouer.");
        }

        /// <summary>Le jeu se crée tout seul au lancement ; il lui faut seulement une scène vide dans la liste de build.</summary>
        private static void EnsureScene()
        {
            if (!File.Exists(ScenePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        [MenuItem("Dig For Real/2. Construire l'APK Android de test")]
        public static void BuildAndroid()
        {
            Apply();
            EditorUserBuildSettings.buildAppBundle = false;
            Build(BuildTarget.Android, "Builds/DigForReal-test.apk");
        }

        [MenuItem("Dig For Real/3. Générer le projet Xcode (iOS)")]
        public static void BuildIos()
        {
            Apply();
            Build(BuildTarget.iOS, "Builds/iOS");
        }

        private static void Build(BuildTarget target, string location)
        {
            Directory.CreateDirectory("Builds");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = location,
                target = target,
                options = BuildOptions.None
            };
            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
            UnityEditor.Build.Reporting.BuildSummary summary = report.summary;
            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                Debug.Log("Dig For Real : build réussi → " + summary.outputPath + " (" + (summary.totalSize / (1024 * 1024)) + " Mo)");
            else
            {
                Debug.LogError("Dig For Real : échec du build (" + summary.result + ", " + summary.totalErrors + " erreur(s)).");
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }
    }
}
