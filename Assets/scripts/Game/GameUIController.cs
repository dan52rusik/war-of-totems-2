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
    private bool levelManagerButtonsBound;

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

        ResolveUiReferences();

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        BindLevelManagerButtons();

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

    void TryFindLM()
    {
        if (lm != null) return;
        lm = FindFirstObjectByType<LevelManager>();
    }

    void ResolveUiReferences()
    {
        if (pausePanel == null)
        {
            var p = transform.Find("PausePanel");
            if (p != null) pausePanel = p.gameObject;
            else pausePanel = GameObject.Find("PausePanel");
        }

        if (winPanel == null)
        {
            var p = transform.Find("WinPanel");
            if (p != null) winPanel = p.gameObject;
            else winPanel = GameObject.Find("WinPanel");
        }

        if (losePanel == null)
        {
            var p = transform.Find("LosePanel");
            if (p != null) losePanel = p.gameObject;
            else losePanel = GameObject.Find("LosePanel");
        }
    }

    void BindLevelManagerButtons()
    {
        TryFindLM();
        ResolveUiReferences();

        if (lm == null)
        {
            levelManagerButtonsBound = false;
            return;
        }

        var pBtnTrans = transform.Find("TopBar/PauseBtn");
        if (pBtnTrans == null && transform.parent != null) pBtnTrans = transform.parent.Find("TopBar/PauseBtn");
        Button pBtn = pBtnTrans ? pBtnTrans.GetComponent<Button>() : GameObject.Find("PauseBtn")?.GetComponent<Button>();
        if (pBtn != null)
        {
            pBtn.onClick.RemoveAllListeners();
            pBtn.onClick.AddListener(lm.OnPauseButton);
        }

        Button sBtn = GameObject.Find("SpeedBtn")?.GetComponent<Button>();
        if (sBtn != null)
        {
            sBtn.onClick.RemoveAllListeners();
            sBtn.onClick.AddListener(lm.OnSpeedUpButton);
        }

        if (pausePanel != null)
        {
            var rBtn = FindChildByName(pausePanel.transform, "ResumeBtn")?.GetComponent<Button>();
            if (rBtn != null)
            {
                rBtn.onClick.RemoveAllListeners();
                rBtn.onClick.AddListener(lm.OnPauseButton);
            }

            var rtBtn = FindChildByName(pausePanel.transform, "RetryBtn")?.GetComponent<Button>();
            if (rtBtn != null)
            {
                rtBtn.onClick.RemoveAllListeners();
                rtBtn.onClick.AddListener(lm.OnRetryButton);
            }

            var mBtn = FindChildByName(pausePanel.transform, "PauseMenuBtn")?.GetComponent<Button>();
            if (mBtn != null)
            {
                mBtn.onClick.RemoveAllListeners();
                mBtn.onClick.AddListener(lm.OnMainMenuButton);
            }

            var cxBtn = FindChildByName(pausePanel.transform, "CloseBtn")?.GetComponent<Button>();
            if (cxBtn != null)
            {
                cxBtn.onClick.RemoveAllListeners();
                cxBtn.onClick.AddListener(lm.OnPauseButton);
            }
        }

        levelManagerButtonsBound = true;
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
        ResolveUiReferences();
        if (!levelManagerButtonsBound) BindLevelManagerButtons();
        if (pausePanel != null) pausePanel.SetActive(paused);
        else Debug.LogError("[UI] ShowPause вызван, но окно паузы не найдено!");
    }

    public void UpdateSpeedText(float speed)
    {
        if (speedText != null) speedText.text = "x" + speed;
    }

    void Update()
    {
        if (!levelManagerButtonsBound)
        {
            BindLevelManagerButtons();
        }

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
