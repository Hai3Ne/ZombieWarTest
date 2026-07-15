using System;
using UnityEngine;

namespace ZombieWar.Core
{
    public static class GameOptions
    {
        private const string SfxKey = "ZombieWar.Options.Sfx";
        private const string MusicKey = "ZombieWar.Options.Music";
        private const string CameraShakeKey = "ZombieWar.Options.CameraShake";

        public static bool SfxEnabled => PlayerPrefs.GetInt(SfxKey, 1) == 1;
        public static bool MusicEnabled => PlayerPrefs.GetInt(MusicKey, 1) == 1;
        public static bool CameraShakeEnabled => PlayerPrefs.GetInt(CameraShakeKey, 1) == 1;
        public static event Action Changed;

        public static void SetSfxEnabled(bool enabled)
        {
            SetOption(SfxKey, enabled);
        }

        public static void SetMusicEnabled(bool enabled)
        {
            SetOption(MusicKey, enabled);
        }

        public static void SetCameraShakeEnabled(bool enabled)
        {
            SetOption(CameraShakeKey, enabled);
        }

        private static void SetOption(string key, bool enabled)
        {
            int value = enabled ? 1 : 0;
            if (PlayerPrefs.GetInt(key, 1) == value)
            {
                return;
            }

            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
