using Game.Core;
using UnityEngine;

namespace Game.Unity
{
    /// <summary>
    /// Services de l'appareil : sauvegarde locale, vibrations, presse-papiers, langue.
    /// Aucun accès réseau, aucune donnée transmise.
    /// </summary>
    public sealed class UnityPlatform : IPlatform
    {
        private const string SaveKey = "dfr_save_v1";

        public string LoadSave() { return PlayerPrefs.GetString(SaveKey, ""); }

        public void StoreSave(string data)
        {
            PlayerPrefs.SetString(SaveKey, data);
            PlayerPrefs.Save();
        }

        public void CopyText(string text) { GUIUtility.systemCopyBuffer = text; }

        public string Language
        {
            get { return Application.systemLanguage == SystemLanguage.French ? "fr" : "en"; }
        }

        public void Haptic(int strength)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidHaptics.Vibrate(strength == 0 ? 12 : (strength == 1 ? 24 : 45), strength == 0 ? 70 : (strength == 1 ? 140 : 255));
#endif
            // iOS : les vibrations fines demandent un petit module natif, prévu dans la feuille de route.
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    /// <summary>Vibrations courtes sur Android, par l'API système (aucune bibliothèque tierce).</summary>
    internal static class AndroidHaptics
    {
        private static AndroidJavaObject _vibrator;
        private static int _sdk;
        private static bool _ready, _failed;

        private static void Init()
        {
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }
                using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    _sdk = version.GetStatic<int>("SDK_INT");
                }
                _ready = _vibrator != null;
                // Référence jamais exécutée : elle fait ajouter par Unity la permission VIBRATE au manifeste Android.
#if !DFR_COMPILE_CHECK
                if (Time.frameCount < 0) Handheld.Vibrate();
#endif
            }
            catch (System.Exception)
            {
                _failed = true;
            }
        }

        public static void Vibrate(long milliseconds, int amplitude)
        {
            if (_failed) return;
            if (!_ready) { Init(); if (!_ready) { _failed = true; return; } }
            try
            {
                if (_sdk >= 26)
                {
                    using (var effectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                    using (var effect = effectClass.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, amplitude))
                    {
                        _vibrator.Call("vibrate", effect);
                    }
                }
                else _vibrator.Call("vibrate", milliseconds);
            }
            catch (System.Exception)
            {
                _failed = true;
            }
        }
    }
#endif
}
