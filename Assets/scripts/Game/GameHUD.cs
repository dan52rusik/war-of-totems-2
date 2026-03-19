using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Управляет всем игровым UI:
/// - верхняя панель (деньги, XP, HP, эпоха)
/// - нижняя панель (кнопки юнитов и башен)
/// - окна победы/поражения/паузы
/// - всплывающие сообщения
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("Верхняя панель — Ресурсы")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI playerHpText;
    public TextMeshProUGUI enemyHpText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI speedText;

    [Header("Способность")]
    public Button abilityButton;
    public Image abilityCooldownFill;
    public TextMeshProUGUI abilityCooldownText;

    [Header("Кнопки юнитов")]
    public Button[] troopButtons;         // 0-3 для тиров 1-4
    public TextMeshProUGUI[] troopCostTexts;  // стоимость каждого юнита

    [Header("Кнопки башен")]
    public Button[] turretButtons;        // 0-2 для тиров 1-3
    public TextMeshProUGUI[] turretCostTexts;
    public Button buySlotButton;
    public TextMeshProUGUI slotsText;

    [Header("Кнопка эпохи")]
    public Button upgradeAgeButton;
    public TextMeshProUGUI upgradeAgeCostText;

    [Header("Панели результатов")]
    public GameObject winPanel;
    public TextMeshProUGUI winCoinsText;
    public GameObject losePanel;
    public GameObject pausePanel;

    [Header("Сообщения")]
    public TextMeshProUGUI messageText;

    GameManager gm;
    LevelData levelData;
    Data.Only_Data od;
    Coroutine messageCoroutine;

    public void Initialize(GameManager gameManager, LevelData data)
    {
        gm = gameManager;
        levelData = data;
        od = new Data.Only_Data();

        // Скрыть панели
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (messageText != null) messageText.gameObject.SetActive(false);

        // Показать название уровня
        if (levelNameText != null && levelData != null)
        {
            levelNameText.text = "Уровень " + levelData.levelNumber + ": " + levelData.levelName;
        }

        // Скрыть юнит4, если не 5-я эпоха
        UpdateTroopButtonVisibility();
    }

    void Update()
    {
        if (gm == null || gm.game_status != 0) return;

        UpdateResourceTexts();
        UpdateButtonStates();
        UpdateAbilityCooldown();
        UpdateTroopButtonVisibility();
    }

    void UpdateResourceTexts()
    {
        if (moneyText != null) moneyText.text = FormatNumber(gm.money);
        if (xpText != null) xpText.text = FormatNumber(gm.xp);
        if (playerHpText != null) playerHpText.text = "" + gm.player_hp;
        if (enemyHpText != null) enemyHpText.text = "" + gm.enemy_hp;
        if (ageText != null) ageText.text = "Эпоха " + gm.player_age;
        if (slotsText != null) slotsText.text = gm.available_slots + "/" + gm.total_slots;
    }

    void UpdateButtonStates()
    {
        // Кнопки юнитов
        if (troopButtons != null)
        {
            for (int i = 0; i < troopButtons.Length && i < 4; i++)
            {
                if (troopButtons[i] == null) continue;
                bool canSpawn = gm.check_spawn_player_troop(i) && gm.player_troops_queue.Count < 20;

                // Проверяем ограничения уровня
                if (levelData != null && i > levelData.playerMaxTroopTier)
                    canSpawn = false;

                troopButtons[i].interactable = canSpawn;

                // Обновляем стоимость
                if (troopCostTexts != null && i < troopCostTexts.Length && troopCostTexts[i] != null)
                {
                    int costIndex = (gm.player_age - 1) * 3 + i;
                    if (i == 3) costIndex = 15; // super soldier
                    if (costIndex < od.troop_costs.Length)
                    {
                        troopCostTexts[i].text = FormatNumber(od.troop_costs[costIndex]);
                    }
                }
            }
        }

        // Кнопки башен
        if (turretButtons != null)
        {
            for (int i = 0; i < turretButtons.Length && i < 3; i++)
            {
                if (turretButtons[i] == null) continue;
                turretButtons[i].interactable = gm.check_buy_turret_player(i);

                if (turretCostTexts != null && i < turretCostTexts.Length && turretCostTexts[i] != null)
                {
                    int costIndex = i + (gm.player_age - 1) * 3;
                    if (costIndex < od.turret_cost.Length)
                    {
                        turretCostTexts[i].text = FormatNumber(od.turret_cost[costIndex]);
                    }
                }
            }
        }

        // Кнопка покупки слота
        if (buySlotButton != null)
        {
            buySlotButton.interactable = gm.check_buy_slot_player();
        }

        // Кнопка эпохи
        if (upgradeAgeButton != null)
        {
            bool canUpgrade = gm.check_upgrade_age_player();
            if (levelData != null && gm.player_age >= levelData.playerMaxAge)
                canUpgrade = false;
            upgradeAgeButton.interactable = canUpgrade;

            if (upgradeAgeCostText != null && gm.player_age <= 4)
            {
                upgradeAgeCostText.text = FormatNumber(od.xp_cost[gm.player_age - 1]) + " XP";
            }
        }
    }

    void UpdateAbilityCooldown()
    {
        if (abilityButton != null)
        {
            bool ready = gm.check_use_ability();
            abilityButton.interactable = ready;

            if (abilityCooldownFill != null)
            {
                abilityCooldownFill.fillAmount = ready ? 0f : gm.ability_time / 60f;
            }

            if (abilityCooldownText != null)
            {
                if (ready)
                    abilityCooldownText.text = "ГОТОВО";
                else
                    abilityCooldownText.text = Mathf.CeilToInt(gm.ability_time) + "с";
            }
        }
    }

    void UpdateTroopButtonVisibility()
    {
        // Кнопка T4 видна только в 5-й эпохе
        if (troopButtons != null && troopButtons.Length > 3 && troopButtons[3] != null)
        {
            troopButtons[3].gameObject.SetActive(gm != null && gm.player_age == 5);
        }
    }

    // ========== Результаты ==========

    public void ShowResult(bool won, int coins)
    {
        if (won)
        {
            if (winPanel != null)
            {
                winPanel.SetActive(true);
                if (winCoinsText != null)
                    winCoinsText.text = "+" + coins + " монет";
            }
        }
        else
        {
            if (losePanel != null)
            {
                losePanel.SetActive(true);
            }
        }
    }

    public void ShowPause(bool paused)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(paused);
        }
    }

    public void UpdateSpeedText(float speed)
    {
        if (speedText != null)
        {
            speedText.text = "x" + speed.ToString("0.#");
        }
    }

    // ========== Всплывающие сообщения ==========

    public void ShowMessage(string msg)
    {
        if (messageText == null) return;

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        messageCoroutine = StartCoroutine(HideMessage());
    }

    IEnumerator HideMessage()
    {
        yield return new WaitForSecondsRealtime(2f);
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    // ========== Утилиты ==========

    string FormatNumber(int number)
    {
        if (number >= 1000000)
            return (number / 1000000f).ToString("0.#") + "M";
        if (number >= 1000)
            return (number / 1000f).ToString("0.#") + "K";
        return number.ToString();
    }
}
