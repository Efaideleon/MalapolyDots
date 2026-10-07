using System;
using UnityEngine;

namespace Assets.Common
{
    [Serializable]
    public sealed class GamePreferencesData
    {
        public float Volume = 1f;
        public bool Muted;
        public float CameraSensitivity = 1f;
        public float ZoomSensitivity = 1f;
        public bool InvertVertical;
        public bool ReducedCameraMotion;

        public float EffectiveVolume => Muted ? 0f : Volume;

        public void Validate()
        {
            Volume = Clamp(Volume, 0f, 1f);
            CameraSensitivity = Clamp(CameraSensitivity, 0.25f, 2.5f);
            ZoomSensitivity = Clamp(ZoomSensitivity, 0.25f, 2.5f);
        }

        static float Clamp(float value, float minimum, float maximum) =>
            float.IsNaN(value) || float.IsInfinity(value) ? 1f : Mathf.Clamp(value, minimum, maximum);

        public static GamePreferencesData FromJson(string json)
        {
            var data = new GamePreferencesData();
            try { if (!string.IsNullOrEmpty(json)) JsonUtility.FromJsonOverwrite(json, data); }
            catch (ArgumentException) { data = new GamePreferencesData(); }
            data.Validate();
            return data;
        }
    }

    public static class GamePreferences
    {
        public const string StorageKey = "Malapoly.Settings.v1";
        static GamePreferencesData current;
        public static GamePreferencesData Current => current ??= GamePreferencesData.FromJson(PlayerPrefs.GetString(StorageKey, ""));
        public static event Action Changed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Initialize()
        {
            current = null;
            Changed = null;
            Apply();
        }

        public static void Apply()
        {
            Current.Validate();
            AudioListener.volume = Current.EffectiveVolume;
            Changed?.Invoke();
        }

        public static void Save()
        {
            Current.Validate();
            PlayerPrefs.SetString(StorageKey, JsonUtility.ToJson(Current));
            PlayerPrefs.Save();
        }

        public static void Reload()
        {
            current = GamePreferencesData.FromJson(PlayerPrefs.GetString(StorageKey, ""));
            Apply();
        }

        public static void ResetDefaults()
        {
            current = new GamePreferencesData();
            Apply();
            Save();
        }
    }
}
