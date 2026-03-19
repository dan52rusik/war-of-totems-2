using UnityEngine;
using UnityEditor;

/// <summary>
/// Создаёт набор начальных уровней.
/// Запуск: Unity меню → Tools → Create Default Levels
/// </summary>
public class CreateDefaultLevels
{
    [MenuItem("Tools/Create Default Levels")]
    public static void CreateLevels()
    {
        // Создать папку если нет
        if (!AssetDatabase.IsValidFolder("Assets/LevelData"))
        {
            AssetDatabase.CreateFolder("Assets", "LevelData");
        }

        // Уровень 1
        LevelData level1 = ScriptableObject.CreateInstance<LevelData>();
        level1.levelNumber = 1;
        level1.levelName = "Начало начал";
        level1.description = "Каменный век. Уничтожьте вражескую базу, используя первобытных воинов!";
        level1.startMoney = 200;
        level1.startXp = 0;
        level1.startAge = 1;
        level1.startSlots = 1;
        level1.enemyDifficulty = 0.7f;
        level1.enemyMaxAge = 1;
        level1.enemyMaxTroops = 3;
        level1.enemySpawnChance = 0.2f;
        level1.enemyTier2UnlockFrames = 2000;
        level1.enemyTier3UnlockFrames = 99999;
        level1.enemyAgeUpgradeFrames = 99999;
        level1.playerMaxAge = 1;
        level1.playerMaxTroopTier = 2;
        level1.coinsReward = 100;
        level1.xpReward = 50;
        AssetDatabase.CreateAsset(level1, "Assets/LevelData/Level_01.asset");

        // Уровень 2
        LevelData level2 = ScriptableObject.CreateInstance<LevelData>();
        level2.levelNumber = 2;
        level2.levelName = "Прорыв";
        level2.description = "Враг стал сильнее. Используйте все типы юнитов каменного века!";
        level2.startMoney = 200;
        level2.startXp = 0;
        level2.startAge = 1;
        level2.startSlots = 1;
        level2.enemyDifficulty = 0.9f;
        level2.enemyMaxAge = 1;
        level2.enemyMaxTroops = 5;
        level2.enemySpawnChance = 0.3f;
        level2.enemyTier2UnlockFrames = 1500;
        level2.enemyTier3UnlockFrames = 5000;
        level2.enemyAgeUpgradeFrames = 99999;
        level2.playerMaxAge = 1;
        level2.playerMaxTroopTier = 2;
        level2.coinsReward = 120;
        level2.xpReward = 60;
        AssetDatabase.CreateAsset(level2, "Assets/LevelData/Level_02.asset");

        // Уровень 3
        LevelData level3 = ScriptableObject.CreateInstance<LevelData>();
        level3.levelNumber = 3;
        level3.levelName = "Эпоха мечей";
        level3.description = "Переход во вторую эпоху! Разблокированы рыцари и катапульты.";
        level3.startMoney = 250;
        level3.startXp = 0;
        level3.startAge = 1;
        level3.startSlots = 1;
        level3.enemyDifficulty = 1.0f;
        level3.enemyMaxAge = 2;
        level3.enemyMaxTroops = 6;
        level3.enemySpawnChance = 0.3f;
        level3.enemyTier2UnlockFrames = 1500;
        level3.enemyTier3UnlockFrames = 4000;
        level3.enemyAgeUpgradeFrames = 7000;
        level3.playerMaxAge = 2;
        level3.playerMaxTroopTier = 2;
        level3.coinsReward = 200;
        level3.xpReward = 100;
        AssetDatabase.CreateAsset(level3, "Assets/LevelData/Level_03.asset");

        // Уровень 4
        LevelData level4 = ScriptableObject.CreateInstance<LevelData>();
        level4.levelNumber = 4;
        level4.levelName = "Осада";
        level4.description = "Враг укрепился башнями. Стройте свои и не отступайте!";
        level4.startMoney = 300;
        level4.startXp = 0;
        level4.startAge = 1;
        level4.startSlots = 1;
        level4.enemyDifficulty = 1.1f;
        level4.enemyMaxAge = 2;
        level4.enemyMaxTroops = 6;
        level4.enemySpawnChance = 0.35f;
        level4.enemyTier2UnlockFrames = 1200;
        level4.enemyTier3UnlockFrames = 3500;
        level4.enemyAgeUpgradeFrames = 6000;
        level4.playerMaxAge = 2;
        level4.playerMaxTroopTier = 2;
        level4.coinsReward = 250;
        level4.xpReward = 120;
        AssetDatabase.CreateAsset(level4, "Assets/LevelData/Level_04.asset");

        // Уровень 5
        LevelData level5 = ScriptableObject.CreateInstance<LevelData>();
        level5.levelNumber = 5;
        level5.levelName = "Эпоха пороха";
        level5.description = "Мушкетёры и пушки меняют правила игры!";
        level5.startMoney = 350;
        level5.startXp = 0;
        level5.startAge = 1;
        level5.startSlots = 1;
        level5.enemyDifficulty = 1.15f;
        level5.enemyMaxAge = 3;
        level5.enemyMaxTroops = 7;
        level5.enemySpawnChance = 0.3f;
        level5.enemyTier2UnlockFrames = 1500;
        level5.enemyTier3UnlockFrames = 4000;
        level5.enemyAgeUpgradeFrames = 7000;
        level5.playerMaxAge = 3;
        level5.playerMaxTroopTier = 2;
        level5.coinsReward = 350;
        level5.xpReward = 200;
        AssetDatabase.CreateAsset(level5, "Assets/LevelData/Level_05.asset");

        // Уровень 6
        LevelData level6 = ScriptableObject.CreateInstance<LevelData>();
        level6.levelNumber = 6;
        level6.levelName = "Индустриальная мощь";
        level6.description = "Танки и пулемёты! Враг не пощадит.";
        level6.startMoney = 400;
        level6.startXp = 0;
        level6.startAge = 1;
        level6.startSlots = 1;
        level6.enemyDifficulty = 1.25f;
        level6.enemyMaxAge = 4;
        level6.enemyMaxTroops = 8;
        level6.enemySpawnChance = 0.35f;
        level6.enemyTier2UnlockFrames = 1200;
        level6.enemyTier3UnlockFrames = 3500;
        level6.enemyAgeUpgradeFrames = 6500;
        level6.playerMaxAge = 4;
        level6.playerMaxTroopTier = 2;
        level6.coinsReward = 500;
        level6.xpReward = 300;
        AssetDatabase.CreateAsset(level6, "Assets/LevelData/Level_06.asset");

        // Уровень 7
        LevelData level7 = ScriptableObject.CreateInstance<LevelData>();
        level7.levelNumber = 7;
        level7.levelName = "Финальная битва";
        level7.description = "Эпоха будущего! Разблокирован Super Soldier. Уничтожьте врага навсегда!";
        level7.startMoney = 500;
        level7.startXp = 0;
        level7.startAge = 1;
        level7.startSlots = 1;
        level7.enemyDifficulty = 1.4f;
        level7.enemyMaxAge = 5;
        level7.enemyMaxTroops = 10;
        level7.enemySpawnChance = 0.35f;
        level7.enemyTier2UnlockFrames = 1000;
        level7.enemyTier3UnlockFrames = 3000;
        level7.enemyAgeUpgradeFrames = 6000;
        level7.playerMaxAge = 5;
        level7.playerMaxTroopTier = 3;
        level7.coinsReward = 1000;
        level7.xpReward = 500;
        AssetDatabase.CreateAsset(level7, "Assets/LevelData/Level_07.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Готово!",
            "Создано 7 уровней в Assets/LevelData/!\n\n" +
            "Теперь ваши уровни должны корректно отображаться в меню.",
            "OK");
    }
}
