using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Контроллер внутриигрового интерфейса. Связывает кнопки на панели с методами GameManager.
/// </summary>
public class GameUIController : MonoBehaviour
{
    private GameManager gm;
    private LevelManager lm;

    [Header("Верхняя панель")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI ageText;

    [Header("Кнопки (для обновления состояния)")]
    public Button[] troopButtons; // Tier 1, 2, 3, 4
    public Button[] turretButtons;
    public Button buySlotButton;
    public Button upgradeAgeButton;
    public Button abilityButton;

    [Header("Панели")]
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject pausePanel;
    public TextMeshProUGUI winCoinsText; 
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI speedText;

    public void BindGameManager(GameManager gameManager)
    {
        gm = gameManager;
    }

    void Start()
    {
        if (gm == null)
        {
            gm = FindFirstObjectByType<GameManager>();
        }

        lm = FindObjectOfType<LevelManager>();
        
        if (gm == null) Debug.LogError("[UI] GameManager не найден!");
        
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    // ========== МЕТОДЫ УПРАВЛЕНИЯ (ВЫЗЫВАЮТСЯ ИЗ LEVELMANAGER) ==========

    public void Initialize(LevelData data, int startHp)
    {
        if (levelNameText != null) levelNameText.text = data.levelNumber + ". " + data.levelName;
        UpdateUI();
    }

    public void UpdateUI() 
    {
        // Принудительное обновление текстов, если нужно
    }

    public void ShowResult(bool win, int reward = 0)
    {
        if (win)
        {
            if (winPanel != null) winPanel.SetActive(true);
            if (winCoinsText != null) winCoinsText.text = "+" + reward + " МОНЕТ";
        }
        else
        {
            if (losePanel != null) losePanel.SetActive(true);
        }
    }

    public void ShowPause(bool paused)
    {
        if (pausePanel != null) pausePanel.SetActive(paused);
    }

    public void UpdateSpeedText(float speed)
    {
        if (speedText != null) speedText.text = "x" + speed;
    }

    void Update()
    {
        if (gm == null) return;

        // Обновляем тексты
        if (moneyText != null) moneyText.text = gm.money.ToString();
        if (xpText != null) xpText.text = gm.xp.ToString();
        if (hpText != null) hpText.text = Mathf.Max(0, gm.player_hp).ToString();
        if (ageText != null) ageText.text = "ЭПОХА " + gm.player_age;

        UpdateButtonsInteractable();
    }

    void UpdateButtonsInteractable()
    {
        // Здесь можно отключать кнопки, если не хватает денег
        // Но для начала просто дадим им работать
    }

    // ========== МЕТОДЫ ДЛЯ КНОПОК ==========

    public void SpawnTroop(int tier)
    {
        if (gm == null) return;

        bool spawned = false;

        // Привязываем tier к существующим командным методам,
        // чтобы всегда использовать одну и ту же логику проверок/стоимости.
        switch (tier)
        {
            case 0:
                spawned = gm.command_spawn_troop_tier_1();
                break;
            case 1:
                spawned = gm.command_spawn_troop_tier_2();
                break;
            case 2:
                spawned = gm.command_spawn_troop_tier_3();
                break;
            case 3:
                spawned = gm.command_spawn_troop_tier_4();
                break;
            default:
                Debug.LogWarning($"[UI] Неизвестный tier юнита: {tier}");
                return;
        }

        if (!spawned)
        {
            Debug.Log("[UI] Не удалось заспавнить юнита (скорее всего, не хватает денег или слот в очереди занят).");
        }
        else
        {
            Debug.Log($"[UI] Спавн юнита Tier {tier + 1}");
        }
    }

    public void BuySlot()
    {
        if (gm == null) return;
        gm.command_buy_slot();
    }

    public void UpgradeAge()
    {
        if (gm == null) return;
        gm.command_upgrade_age();
    }

    public void UseAbility()
    {
        if (gm == null) return;
        gm.command_use_ability();
    }

    // Методы обертки для Unity Events
    public void OnSpawnTier1() => SpawnTroop(0);
    public void OnSpawnTier2() => SpawnTroop(1);
    public void OnSpawnTier3() => SpawnTroop(2);
    public void OnSpawnTier4() => SpawnTroop(3);
}
