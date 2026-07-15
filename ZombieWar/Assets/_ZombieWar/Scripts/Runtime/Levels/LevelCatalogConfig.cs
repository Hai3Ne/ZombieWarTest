using UnityEngine;

namespace ZombieWar.Levels
{
    [CreateAssetMenu(menuName = "Zombie War/Levels/Level Catalog", fileName = "LevelCatalog")]
    public sealed class LevelCatalogConfig : ScriptableObject
    {
        #region Config
        [SerializeField] private LevelDefinition[] _levels;
        #endregion

        public LevelDefinition[] Levels => _levels;

        public int EnabledLevelCount
        {
            get
            {
                int count = 0;
                if (_levels == null)
                {
                    return count;
                }
                for (int i = 0; i < _levels.Length; i++)
                {
                    if (_levels[i] != null && _levels[i].IsValid)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public void Configure(LevelDefinition[] levels)
        {
            _levels = levels;
        }

        public LevelDefinition GetEnabledLevel(int enabledIndex)
        {
            if (_levels == null || enabledIndex < 0)
            {
                return null;
            }

            int current = 0;
            for (int i = 0; i < _levels.Length; i++)
            {
                LevelDefinition level = _levels[i];
                if (level == null || !level.IsValid)
                {
                    continue;
                }
                if (current == enabledIndex)
                {
                    return level;
                }
                current++;
            }
            return null;
        }

        public LevelDefinition GetLevelBySceneName(string sceneName)
        {
            if (_levels == null || string.IsNullOrWhiteSpace(sceneName))
            {
                return null;
            }

            for (int i = 0; i < _levels.Length; i++)
            {
                LevelDefinition level = _levels[i];
                if (level != null && level.IsValid && level.SceneName == sceneName)
                {
                    return level;
                }
            }
            return null;
        }

        public string GetNextSceneName(string currentSceneName)
        {
            if (_levels == null)
            {
                return string.Empty;
            }

            int currentEnabledIndex = -1;
            int enabledCount = 0;
            for (int i = 0; i < _levels.Length; i++)
            {
                LevelDefinition level = _levels[i];
                if (level == null || !level.IsValid)
                {
                    continue;
                }
                if (level.SceneName == currentSceneName)
                {
                    currentEnabledIndex = enabledCount;
                }
                enabledCount++;
            }

            if (enabledCount == 0)
            {
                return string.Empty;
            }
            int nextIndex = currentEnabledIndex >= 0 ? (currentEnabledIndex + 1) % enabledCount : 0;
            LevelDefinition next = GetEnabledLevel(nextIndex);
            return next != null ? next.SceneName : string.Empty;
        }
    }
}
