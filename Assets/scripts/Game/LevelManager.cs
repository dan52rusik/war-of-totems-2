using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Управляет жизненным циклом игрового уровня:
/// инициализация → игровой процесс → победа/поражение → результаты.
/// Размещается на корневом объекте игровой сцены.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("UI Ссылки")]
    public LevelData levelData;
    public GameObject environmentPrefab;  // GameInstance prefab
    public Data dataObject;
    public GameUIController gameHUD;

    [Header("Настройки")]
    public float gameSpeed = 1f;

    // Текущее окружение
    GameObject currentEnvironment;
    GameManager currentGameManager;
    Enemy_AI currentEnemyAI;

    // Состояние уровня
    public enum LevelState { NotStarted, Playing, Won, Lost }
    public LevelState State { get; private set; } = LevelState.NotStarted;

    // Событие для UI
    public System.Action<LevelState> OnStateChanged;

    void Start()
    {
        if (gameHUD == null)
        {
            gameHUD = FindFirstObjectByType<GameUIController>();
            if (gameHUD == null)
            {
                Debug.LogWarning("[LevelManager] Game HUD не назначен и не найден в сцене.");
            }
        }

        if (environmentPrefab == null)
        {
            Debug.LogError("[LevelManager] Environment Prefab не назначен! Перетащите GameInstance из Assets/GameObjects/ в поле Environment Prefab в Inspector.");
            return;
        }
        if (dataObject == null)
        {
            Debug.Log("[LevelManager] Data Object не назначен — будет использован Data из GameInstance prefab.");
        }
        if (levelData == null)
        {
            Debug.LogWarning("[LevelManager] Level Data не назначен. Будут использованы параметры по умолчанию. Создайте уровни через Tools → Create Default Levels.");
        }
        StartLevel();
    }

    /// <summary>
    /// Запускает текущий уровень
    /// </summary>
    public void StartLevel()
    {
        if (environmentPrefab == null)
        {
            Debug.LogError("[LevelManager] Environment Prefab не назначен!");
            return;
        }

        // Очистить предыдущее окружение, если есть
        if (currentEnvironment != null)
        {
            Destroy(currentEnvironment);
        }

        // Создать окружение
        currentEnvironment = Instantiate(environmentPrefab, Vector3.zero, Quaternion.identity);

        // Настроить GameManager
        currentGameManager = currentEnvironment.GetComponent<GameManager>();
        currentEnemyAI = currentEnvironment.GetComponent<Enemy_AI>();

        // Используем Data из самого GameInstance (там уже заполнены спрайты)
        // Если на prefab есть Data — берём его, иначе пробуем внешний
        Data envData = currentEnvironment.GetComponent<Data>();
        if (envData != null)
        {
            currentGameManager.data_object = envData;
        }
        else if (dataObject != null)
        {
            currentGameManager.data_object = dataObject;
        }
        else
        {
            Debug.LogError("[LevelManager] Data Object не найден ни на окружении, ни назначен вручную!");
            return;
        }
        currentGameManager.scale = gameSpeed;

        // ВАЖНО: Явно сбросить все значения, т.к. prefab может хранить "грязные" данные
        currentGameManager.player_hp = 500;
        currentGameManager.enemy_hp = 500;
        currentGameManager.game_status = 0;
        currentGameManager.player_age = 1;
        currentGameManager.enemy_age = 1;
        currentGameManager.money = 175;
        currentGameManager.emoney = 100;
        currentGameManager.xp = 0;
        currentGameManager.ability_time = 0;
        currentGameManager.available_slots = 1;
        currentGameManager.total_slots = 1;
        currentGameManager.battle_place = 0.5f;

        // Применить параметры уровня
        ApplyLevelData();

        // Применяем улучшения игрока
        ApplyUpgrades();

        // Скрываем старые текстовые UI элементы ("New Text")
        HideOldUI();

        // Настроить камеру
        AlignCamera();

        // Передать GM в HUD и PlayerController
        // Автопоиск GameUIController, если ссылка потерялась
        if (gameHUD == null)
        {
            gameHUD = FindObjectOfType<GameUIController>();
            if (gameHUD != null)
                Debug.Log("[LM] GameUIController найден автоматически!");
            else
                Debug.LogWarning("[LM] GameUIController НЕ найден! Окна победы/поражения не будут работать.");
        }

        // Настроить HUD
        if (gameHUD != null)
        {
            gameHUD.BindGameManager(currentGameManager);
            gameHUD.Initialize(levelData, currentGameManager.player_hp);
            Debug.Log("[LM] HUD привязан к GameManager");
        }

        // Найти PlayerController и передать ему GM
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            pc.Initialize(currentGameManager, levelData);
        }

        Time.timeScale = gameSpeed;
        State = LevelState.Playing;
        OnStateChanged?.Invoke(State);

        // Запускаем проверку состояния с задержкой (чтобы GameManager.Start() успел отработать)
        StartCoroutine(CheckGameStatus());
    }

    /// <summary>
    /// Применяет данные LevelData к менеджерам
    /// </summary>
    void ApplyLevelData()
    {
        if (levelData == null) return;

        // Параметры игрока
        currentGameManager.money = levelData.startMoney;
        currentGameManager.xp = levelData.startXp;
        currentGameManager.diff = levelData.enemyDifficulty;
        currentGameManager.identifier = levelData.levelNumber;

        // Стартовая эпоха игрока (если > 1, нужно апгрейдить)
        for (int i = 1; i < levelData.startAge; i++)
        {
            currentGameManager.upgrade_age_player();
        }

        // Стартовые слоты
        for (int i = 1; i < levelData.startSlots; i++)
        {
            // Даём бесплатные слоты — временно добавим денег
            int savedMoney = currentGameManager.money;
            currentGameManager.money = 999999;
            currentGameManager.buy_slot_player();
            currentGameManager.money = savedMoney;
        }

        // Настраиваем Enemy AI
        if (currentEnemyAI != null)
        {
            currentEnemyAI.maxTroops = levelData.enemyMaxTroops;
            currentEnemyAI.spawnChance = levelData.enemySpawnChance;
            currentEnemyAI.maxAge = levelData.enemyMaxAge;
            currentEnemyAI.tier2Frames = levelData.enemyTier2UnlockFrames;
            currentEnemyAI.tier3Frames = levelData.enemyTier3UnlockFrames;
            currentEnemyAI.ageUpFrames = levelData.enemyAgeUpgradeFrames;
        }

        // Устанавливаем state чтобы использовать скриптовый AI
        // Ставим state <= 0 чтобы GameManager.Start() запустил Custom_state0 → Protocol_age1
        currentGameManager.state = 0;
    }

    void AlignCamera()
    {
        if (Camera.main == null) return;
        Camera.main.transform.position = currentEnvironment.transform.position
            + new Vector3(0f, 0.0f, -10.99119f);
    }

    void HideOldUI()
    {
        // Ищем надписи по именам, которые были в оригинале
        string[] oldNames = { "status", "money_txt", "xp_txt", "money (1)", "xp (1)" };
        foreach (var name in oldNames)
        {
            var obj = GameObject.Find(name);
            if (obj != null) obj.SetActive(false);
        }
    }

    void ApplyUpgrades()
    {
        if (currentGameManager == null) return;

        // 1. Бонус к HP базы (+100 за уровень)
        int hpLvl = PlayerPrefs.GetInt("Upgrade_BaseHp", 0);
        currentGameManager.player_hp += hpLvl * 100;

        // 2. Стартовые деньги (+50 за уровень)
        int moneyLvl = PlayerPrefs.GetInt("Upgrade_StartMoney", 0);
        currentGameManager.money += moneyLvl * 50;

        // 3. Скидки на юнитов (-10% за уровень)
        int discountLvl = PlayerPrefs.GetInt("Upgrade_Discount", 0);
        float discountFactor = 1f - (discountLvl * 0.1f);
        if (discountFactor < 0.5f) discountFactor = 0.5f; // Кап 50%

        for (int i = 0; i < currentGameManager.od.troop_costs.Length; i++)
        {
            currentGameManager.od.troop_costs[i] = Mathf.CeilToInt(currentGameManager.od.troop_costs[i] * discountFactor);
        }
    }

    /// <summary>
    /// Проверяет статус игры каждые 0.25 секунд
    /// </summary>
    IEnumerator CheckGameStatus()
    {
        // Ждём совсем чуть-чуть для инициализации
        yield return new WaitForSeconds(0.1f);

        while (State == LevelState.Playing)
        {
            yield return new WaitForSeconds(0.25f);

            if (currentGameManager == null) yield break;

            // Основной путь — через game_status из GameManager
            if (currentGameManager.game_status == 2) // Враг уничтожен
            {
                OnLevelWon();
                yield break;
            }
            else if (currentGameManager.game_status == 1) // Игрок проиграл
            {
                OnLevelLost();
                yield break;
            }

            // Резервный путь — по прямой проверке HP, если по какой‑то причине
            // game_status не был обновлён.
            if (currentGameManager.player_hp <= 0)
            {
                OnLevelLost();
                yield break;
            }
            if (currentGameManager.enemy_hp <= 0)
            {
                OnLevelWon();
                yield break;
            }
        }
    }

    void OnLevelWon()
    {
        State = LevelState.Won;
        Time.timeScale = 0f;
        OnStateChanged?.Invoke(State);
        Debug.Log("[LM] === ПОБЕДА! ===");

        // Гарантированно ищем HUD
        EnsureHUD();

        if (gameHUD != null)
        {
            gameHUD.ShowResult(true, levelData != null ? levelData.coinsReward : 100);
            Debug.Log("[LM] Окно победы показано через HUD");
        }
        else
        {
            // Резервный путь — ищем панель напрямую
            ShowPanelDirect("WinPanel");
        }

        SaveProgress();
    }

    void OnLevelLost()
    {
        State = LevelState.Lost;
        Time.timeScale = 0f;
        OnStateChanged?.Invoke(State);
        Debug.Log("[LM] === ПОРАЖЕНИЕ! ===");

        EnsureHUD();

        if (gameHUD != null)
        {
            gameHUD.ShowResult(false, 0);
            Debug.Log("[LM] Окно поражения показано через HUD");
        }
        else
        {
            ShowPanelDirect("LosePanel");
        }
    }

    /// <summary>
    /// Гарантированный поиск GameUIController
    /// </summary>
    void EnsureHUD()
    {
        if (gameHUD != null) return;

        // Ищем через FindObjectOfType
        gameHUD = FindObjectOfType<GameUIController>();
        if (gameHUD != null)
        {
            Debug.Log("[LM] HUD найден через FindObjectOfType");
            return;
        }

        // Ищем по имени канваса
        GameObject hudCanvas = GameObject.Find("GameHUDCanvas");
        if (hudCanvas != null)
        {
            gameHUD = hudCanvas.GetComponent<GameUIController>();
            if (gameHUD != null)
            {
                Debug.Log("[LM] HUD найден через GameHUDCanvas");
                return;
            }
        }

        Debug.LogWarning("[LM] HUD НЕ найден! Покажем панель напрямую.");
    }

    /// <summary>
    /// Резервный способ показать панель победы/поражения, если HUD не найден
    /// </summary>
    void ShowPanelDirect(string panelName)
    {
        // Ищем по всем корневым объектам, включая неактивные
        foreach (var canvas in FindObjectsOfType<Canvas>(true))
        {
            Transform panel = canvas.transform.Find(panelName);
            if (panel != null)
            {
                panel.gameObject.SetActive(true);
                Debug.Log($"[LM] Панель {panelName} показана напрямую!");
                return;
            }
        }
        Debug.LogError($"[LM] Панель {panelName} не найдена нигде в сцене!");
    }

    void SaveProgress()
    {
        if (levelData == null) return;

        // Сохраняем, что уровень пройден
        string key = "Level_" + levelData.levelNumber + "_completed";
        PlayerPrefs.SetInt(key, 1);

        // Добавляем монеты
        int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
        PlayerPrefs.SetInt("PlayerCoins", currentCoins + levelData.coinsReward);

        // Разблокируем следующий уровень
        int maxUnlockedLevel = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
        if (levelData.levelNumber >= maxUnlockedLevel)
        {
            PlayerPrefs.SetInt("MaxUnlockedLevel", levelData.levelNumber + 1);
        }

        PlayerPrefs.Save();
    }

    // --- UI кнопки ---

    public void OnRetryButton()
    {
        Time.timeScale = 1f;
        StartLevel();
    }

    public void OnNextLevelButton()
    {
        // TODO: загрузить следующий LevelData и перезапустить
        Time.timeScale = 1f;
        Debug.Log("Next level — будет реализовано с экраном выбора уровней");
    }

    public void OnMainMenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnPauseButton()
    {
        if (Time.timeScale > 0)
        {
            Time.timeScale = 0f;
            if (gameHUD != null) gameHUD.ShowPause(true);
        }
        else
        {
            Time.timeScale = gameSpeed;
            if (gameHUD != null) gameHUD.ShowPause(false);
        }
    }

    public void OnSpeedUpButton()
    {
        if (gameSpeed < 4f)
        {
            gameSpeed *= 2f;
        }
        else
        {
            gameSpeed = 1f;
        }
        Time.timeScale = gameSpeed;
        if (gameHUD != null) gameHUD.UpdateSpeedText(gameSpeed);
    }
}
