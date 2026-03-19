using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Контроллер внутриигрового интерфейса. Связывает кнопки на панели с методами GameManager.
/// Кнопки юнитов блокируются, если Tier не куплен в магазине.
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

    [Header("Кнопки юнитов")]
    public Button[] troopButtons; // Tier 1, 2, 3, 4

    [Header("Панели")]
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject pausePanel;
    public TextMeshProUGUI winCoinsText; 
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI speedText;

    // Какие тиры открыты (по умолчанию Tier 1 всегда открыт)
    private bool[] tierUnlocked = { true, false, false, false };

    public void BindGameManager(GameManager gameManager)
    {
        gm = gameManager;
    }

    void Start()
    {
        lm = FindObjectOfType<LevelManager>();
        
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // --- ДИНАМИЧЕСКАЯ ПРИВЯЗКА КНОПОК ПАУЗЫ И СКОРОСТИ ---
        // (чтобы клик точно срабатывал, даже если сломался UnityEditor Event Tool)
        if (lm != null)
        {
            var pauseBtnTrans = transform.Find("TopBar/PauseBtn");
            if (pauseBtnTrans == null && transform.parent != null)
                pauseBtnTrans = transform.parent.Find("TopBar/PauseBtn"); // Если скрипт где-то еще
            
            // Если не нашли так, ищем глобально
            Button pBtn = pauseBtnTrans ? pauseBtnTrans.GetComponent<Button>() : GameObject.Find("PauseBtn")?.GetComponent<Button>();
            if (pBtn)
            {
                pBtn.onClick.RemoveAllListeners();
                pBtn.onClick.AddListener(lm.OnPauseButton);
            }

            // --- Кнопка Скорости ---
            Button sBtn = GameObject.Find("SpeedBtn")?.GetComponent<Button>();
            if (sBtn)
            {
                sBtn.onClick.RemoveAllListeners();
                sBtn.onClick.AddListener(lm.OnSpeedUpButton);
            }
        }

        // Читаем из PlayerPrefs, какие тиры куплены в магазине
        tierUnlocked[0] = true; // Tier 1 всегда открыт
        tierUnlocked[1] = PlayerPrefs.GetInt("Unlock_Tier2", 0) >= 1;
        tierUnlocked[2] = PlayerPrefs.GetInt("Unlock_Tier3", 0) >= 1;
        tierUnlocked[3] = PlayerPrefs.GetInt("Unlock_Tier4", 0) >= 1;

        // Скрываем заблокированные кнопки
        UpdateTierButtons();
    }

    /// <summary>
    /// Автоматически находит GameManager, если он ещё не привязан
    /// </summary>
    void TryFindGM()
    {
        if (gm != null) return;
        gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            Debug.Log("[UI] GameManager найден!");
        }
    }

    // ========== МЕТОДЫ УПРАВЛЕНИЯ ==========

    public void Initialize(LevelData data, int startHp)
    {
        if (levelNameText != null) levelNameText.text = data.levelNumber + ". " + data.levelName;
    }

    public void UpdateUI() 
    {
        // вызывается при необходимости
    }

    public void ShowResult(bool win, int reward = 0)
    {
        string panelName = win ? "WinPanel" : "LosePanel";
        GameObject panel = win ? winPanel : losePanel;

        // --- Уровень 1: уже есть ссылка ---
        if (panel == null)
        {
            // --- Уровень 2: ищем среди дочерних Canvas ---
            Canvas myCanvas = GetComponentInParent<Canvas>();
            if (myCanvas != null)
                panel = FindChildByName(myCanvas.transform, panelName);
        }

        if (panel == null)
        {
            // --- Уровень 3: ищем в сцене ---
            panel = GameObject.Find(panelName);
        }

        if (panel == null)
        {
            // --- Уровень 4: перебираем все Canvas ---
            foreach (var c in FindObjectsOfType<Canvas>(true))
            {
                var found = FindChildByName(c.transform, panelName);
                if (found != null) { panel = found; break; }
            }
        }

        if (panel != null)
        {
            // Кэшируем для дальнейшего использования (OnRetryButton)
            if (win) winPanel = panel;
            else losePanel = panel;

            panel.SetActive(true);
            Debug.Log($"[UI] Панель {panelName} показана!");

            if (win && winCoinsText != null)
                winCoinsText.text = "+" + reward + " МОНЕТ";
        }
        else
        {
            Debug.LogError($"[UI] Панель {panelName} НЕ найдена!");
        }
    }

    /// <summary>Рекурсивный поиск дочернего объекта по имени</summary>
    GameObject FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child.gameObject;
            var found = FindChildByName(child, name);
            if (found != null) return found;
        }
        return null;
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
        TryFindGM();
        if (gm == null) return;

        // Обновляем тексты
        if (moneyText != null) moneyText.text = gm.money.ToString();
        if (xpText != null) xpText.text = gm.xp.ToString();
        if (hpText != null) hpText.text = Mathf.Max(0, gm.player_hp).ToString();
        if (ageText != null) ageText.text = "ЭПОХА " + gm.player_age;

        UpdateButtonsInteractable();
    }

    void UpdateTierButtons()
    {
        if (troopButtons == null) return;
        for (int i = 0; i < troopButtons.Length && i < tierUnlocked.Length; i++)
        {
            if (troopButtons[i] == null) continue;
            
            if (!tierUnlocked[i])
            {
                troopButtons[i].interactable = false;
                // Меняем текст на "Заблокировано"
                var title = troopButtons[i].transform.Find("Title");
                if (title != null)
                {
                    var tmp = title.GetComponent<TextMeshProUGUI>();
                    if (tmp != null) tmp.text = "Закрыт";
                }
                // Затемняем кнопку
                var img = troopButtons[i].GetComponent<Image>();
                if (img != null)
                {
                    Color c = img.color;
                    img.color = new Color(c.r * 0.3f, c.g * 0.3f, c.b * 0.3f, 0.5f);
                }
            }
        }
    }

    void UpdateButtonsInteractable()
    {
        if (troopButtons == null || gm == null) return;

        // Проверяем, хватает ли денег на каждый тир (для открытых)
        for (int i = 0; i < troopButtons.Length; i++)
        {
            if (troopButtons[i] == null) continue;
            if (!tierUnlocked[i])
            {
                troopButtons[i].interactable = false;
                continue;
            }
            
            int costIndex = (gm.player_age - 1) * 3 + i;
            if (costIndex < gm.od.troop_costs.Length)
            {
                troopButtons[i].interactable = gm.money >= gm.od.troop_costs[costIndex];
            }
        }
    }

    // ========== МЕТОДЫ ДЛЯ КНОПОК ==========

    public void SpawnTroop(int tier)
    {
        if (gm == null) return;
        if (tier >= tierUnlocked.Length || !tierUnlocked[tier])
        {
            Debug.Log("[UI] Этот юнит ещё не открыт! Купите в магазине.");
            return;
        }

        bool spawned = false;

        switch (tier)
        {
            case 0: spawned = gm.command_spawn_troop_tier_1(); break;
            case 1: spawned = gm.command_spawn_troop_tier_2(); break;
            case 2: spawned = gm.command_spawn_troop_tier_3(); break;
            case 3: spawned = gm.command_spawn_troop_tier_4(); break;
            default:
                Debug.LogWarning($"[UI] Неизвестный tier: {tier}");
                return;
        }

        if (spawned)
            Debug.Log($"[UI] Спавн юнита Tier {tier + 1}");
    }

    // Методы обертки для Unity Events
    public void OnSpawnTier1() => SpawnTroop(0);
    public void OnSpawnTier2() => SpawnTroop(1);
    public void OnSpawnTier3() => SpawnTroop(2);
    public void OnSpawnTier4() => SpawnTroop(3);
}
