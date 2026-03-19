using UnityEngine;

/// <summary>
/// ScriptableObject описывающий параметры одного уровня.
/// Создавайте через Assets → Create → Game → Level Data
/// </summary>
[CreateAssetMenu(fileName = "Level_01", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Основная информация")]
    public int levelNumber = 1;
    public string levelName = "Каменный век";
    [TextArea] public string description = "Уничтожьте вражескую базу!";

    [Header("Стартовые параметры игрока")]
    public int startMoney = 175;
    public int startXp = 0;
    public int startAge = 1;
    public int startSlots = 1;

    [Header("Параметры врага")]
    [Tooltip("Множитель урона/HP врагов. 1.0 = стандартно, 1.5 = сложнее")]
    public float enemyDifficulty = 1.0f;

    [Tooltip("Максимальная эпоха, до которой враг может дойти")]
    public int enemyMaxAge = 2;

    [Tooltip("Максимум вражеских юнитов на карте одновременно")]
    public int enemyMaxTroops = 6;

    [Tooltip("Шанс спавна вражеского юнита в секунду (0-1)")]
    public float enemySpawnChance = 0.3f;

    [Tooltip("Через сколько фреймов враг разблокирует tier 2")]
    public int enemyTier2UnlockFrames = 1500;

    [Tooltip("Через сколько фреймов враг разблокирует tier 3")]
    public int enemyTier3UnlockFrames = 5000;

    [Tooltip("Через сколько фреймов враг апгрейдит эпоху")]
    public int enemyAgeUpgradeFrames = 8000;

    [Header("Награда за победу")]
    public int coinsReward = 100;
    public int xpReward = 50;

    [Header("Разблокируемый контент")]
    [Tooltip("Максимальная эпоха, доступная игроку на этом уровне")]
    public int playerMaxAge = 2;

    [Tooltip("Доступные тиры юнитов (0-3)")]
    public int playerMaxTroopTier = 2;
}
