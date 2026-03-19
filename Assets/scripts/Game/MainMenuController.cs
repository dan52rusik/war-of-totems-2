using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Главный контроллер меню: управление уровнями, магазином и прогрессией.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Панели")]
    public GameObject levelsPanel;
    public GameObject shopPanel;
    public GameObject mainPanel;

    [Header("UI Ресурсы")]
    public TextMeshProUGUI totalCoinsText;

    [Header("Уровни")]
    public Transform levelButtonsParent;
    public GameObject levelButtonPrefab;
    public List<LevelData> allLevels; // Список всех уровней (назначается через setup или вручную)

    [Header("Магазин — Тексты Уровней")]
    public TextMeshProUGUI hpUpgradeLvlText;
    public TextMeshProUGUI rewardUpgradeLvlText;
    public TextMeshProUGUI discountUpgradeLvlText;
    public TextMeshProUGUI startMoneyUpgradeLvlText;

    [Header("Магазин — Цены")]
    public TextMeshProUGUI hpPriceText;
    public TextMeshProUGUI rewardPriceText;
    public TextMeshProUGUI discountPriceText;
    public TextMeshProUGUI startMoneyPriceText;

    // Цены на улучшения (массив по уровням)
    int[] upgradeCosts = { 250, 600, 1500, 4000, 10000 };

    void Start()
    {
        // Если список уровней пуст (напр. не настроен в инспекторе), попробуем найти их
        if (allLevels == null || allLevels.Count == 0)
        {
            allLevels = new List<LevelData>(Resources.LoadAll<LevelData>("Levels"));
            
            // Если и в Resources нет, попробуем найти через AssetDatabase (только в Editor)
#if UNITY_EDITOR
            if (allLevels.Count == 0)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:LevelData");
                foreach (var guid in guids)
                {
                    allLevels.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid)));
                }
            }
#endif
            allLevels.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));
        }

        ShowMain();
        UpdateUI();
    }

    public void UpdateUI()
    {
        int coins = PlayerPrefs.GetInt("PlayerCoins", 0);
        if (totalCoinsText != null) totalCoinsText.text = "Монеты: " + coins;

        UpdateShopUI();
    }

    // ========== НАВИГАЦИЯ ==========

    public void ShowLevels()
    {
        mainPanel.SetActive(false);
        shopPanel.SetActive(false);
        levelsPanel.SetActive(true);
        RefreshLevelButtons();
    }

    public void ShowShop()
    {
        mainPanel.SetActive(false);
        levelsPanel.SetActive(false);
        shopPanel.SetActive(true);
        UpdateShopUI();
    }

    public void ShowMain()
    {
        levelsPanel.SetActive(false);
        shopPanel.SetActive(false);
        mainPanel.SetActive(true);
        UpdateUI();
    }

    // ========== УРОВНИ ==========

    void RefreshLevelButtons()
    {
        // Автоматический поиск родителей, если ссылки потерялись
        if (levelButtonsParent == null) 
        {
            var gridObj = GameObject.Find("LevelGrid");
            if (gridObj != null) levelButtonsParent = gridObj.transform;
        }

        if (levelButtonPrefab == null)
        {
            levelButtonPrefab = GameObject.Find("LevelButtonPrefab");
        }

        if (levelButtonsParent == null || levelButtonPrefab == null)
        {
            Debug.LogError("[Menu] LevelButtonsParent или Prefab не найдены в сцене!");
            return;
        }

        // Очистить старые кнопки
        foreach (Transform child in levelButtonsParent)
        {
            if (child.gameObject == levelButtonPrefab) continue; // Не удаляем сам шаблон, если он в сетке
            Destroy(child.gameObject);
        }

        if (allLevels == null) return;
        int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);

        foreach (var level in allLevels)
        {
            GameObject btnObj = Instantiate(levelButtonPrefab, levelButtonsParent);
            btnObj.SetActive(true); // Включаем, т.к. шаблон в сцене может быть скрыт
            
            bool isUnlocked = level.levelNumber <= maxUnlocked;
            bool isCompleted = PlayerPrefs.GetInt("Level_" + level.levelNumber + "_completed", 0) == 1;

            // Настройка текста кнопки
            TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.text = level.levelNumber + ". " + level.levelName + (isCompleted ? " [пройден]" : "");
                Debug.Log($"[Menu] Создана кнопка для уровня {level.levelNumber}: {level.levelName}");
            }
            else
            {
                Debug.LogWarning("[Menu] Не найден текстовый компонент на кнопке уровня!");
            }

            Button btn = btnObj.GetComponent<Button>();
            btn.interactable = isUnlocked;

            // Привязка клика
            btn.onClick.AddListener(() => StartLevel(level));

            // Визуальное состояние заблокированного уровня
            if (!isUnlocked)
            {
                txt.color = new Color(1, 1, 1, 0.3f);
            }
        }
    }

    void StartLevel(LevelData level)
    {
        // Сохраняем выбранный уровень в PlayerPrefs или передаем через статику
        // Для простоты: наш LevelManager в GameScene будет искать Level_{N}
        PlayerPrefs.SetInt("CurrentLevelToLoad", level.levelNumber);
        PlayerPrefs.Save();
        SceneManager.LoadScene("GameScene");
    }

    // ========== МАГАЗИН ==========

    void UpdateShopUI()
    {
        UpdateUpgradeItem("Upgrade_BaseHp", hpUpgradeLvlText, hpPriceText);
        UpdateUpgradeItem("Upgrade_Reward", rewardUpgradeLvlText, rewardPriceText);
        UpdateUpgradeItem("Upgrade_Discount", discountUpgradeLvlText, discountPriceText);
        UpdateUpgradeItem("Upgrade_StartMoney", startMoneyUpgradeLvlText, startMoneyPriceText);
    }

    void UpdateUpgradeItem(string key, TextMeshProUGUI lvlTxt, TextMeshProUGUI priceTxt)
    {
        int lvl = PlayerPrefs.GetInt(key, 0);
        if (lvlTxt != null) lvlTxt.text = "УР: " + lvl;

        if (lvl < upgradeCosts.Length)
        {
            if (priceTxt != null) priceTxt.text = upgradeCosts[lvl].ToString();
        }
        else
        {
            if (priceTxt != null) priceTxt.text = "MAX";
            // Можно деактивировать кнопку покупки
        }
    }

    public void BuyUpgrade(string key)
    {
        int lvl = PlayerPrefs.GetInt(key, 0);
        if (lvl >= upgradeCosts.Length) return;

        int cost = upgradeCosts[lvl];
        int coins = PlayerPrefs.GetInt("PlayerCoins", 0);

        if (coins >= cost)
        {
            PlayerPrefs.SetInt("PlayerCoins", coins - cost);
            PlayerPrefs.SetInt(key, lvl + 1);
            PlayerPrefs.Save();
            UpdateShopUI();
            UpdateUI();
        }
        else
        {
            Debug.Log("Недостаточно монет!");
        }
    }

    // Методы для кнопок магазина (вызываются из Inspector)
    public void BuyBaseHp() => BuyUpgrade("Upgrade_BaseHp");
    public void BuyReward() => BuyUpgrade("Upgrade_Reward");
    public void BuyDiscount() => BuyUpgrade("Upgrade_Discount");
    public void BuyStartMoney() => BuyUpgrade("Upgrade_StartMoney");

    // ========== ЧИТЫ / ТЕСТ ==========

    public void AddCoinsCheat()
    {
        PlayerPrefs.SetInt("PlayerCoins", PlayerPrefs.GetInt("PlayerCoins", 0) + 1000);
        UpdateUI();
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
