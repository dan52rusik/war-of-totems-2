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

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
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
        if (moneyText != null) moneyText.text = "💰 " + gm.money;
        if (xpText != null) xpText.text = "⭐ " + gm.xp;
        if (hpText != null) hpText.text = "❤ " + Mathf.Max(0, gm.player_hp);
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
        if (gm == null || gm.od == null) return;

        int cost = gm.od.troop_costs[(gm.player_age - 1) * 3 + (tier > 2 ? 2 : tier)]; // Фикс индекса для Tier 4
        
        if (gm.money >= cost)
        {
            // Вызываем оригинальный метод спавна
            gm.dispatch_spawn_troop(tier, true);
            Debug.Log($"[UI] Спавн юнита Tier {tier+1}");
        }
        else
        {
            Debug.Log("[UI] Недостаточно денег!");
        }
    }

    public void BuySlot()
    {
        gm.buy_slot_player();
    }

    public void UpgradeAge()
    {
        gm.upgrade_age_player();
    }

    public void UseAbility()
    {
        gm.command_use_ability();
    }

    // Методы обертки для Unity Events
    public void OnSpawnTier1() => SpawnTroop(0);
    public void OnSpawnTier2() => SpawnTroop(1);
    public void OnSpawnTier3() => SpawnTroop(2);
    public void OnSpawnTier4() => SpawnTroop(3);
}
