using UnityEngine;

/// <summary>
/// Связывает UI-кнопки игрока с командами GameManager.
/// Размещается на Canvas или отдельном объекте в игровой сцене.
/// Кнопки UI привязываются к публичным методам этого скрипта через Inspector.
/// </summary>
public class PlayerController : MonoBehaviour
{
    GameManager gm;
    LevelData levelData;
    GameHUD hud;

    public void Initialize(GameManager gameManager, LevelData data)
    {
        gm = gameManager;
        levelData = data;
    }

    void Start()
    {
        hud = FindObjectOfType<GameHUD>();
    }

    // ========== ЮНИТЫ ==========

    public void OnSpawnTroop1()
    {
        if (gm == null || gm.game_status != 0) return;
        if (gm.player_troops_queue.Count >= 20) return;
        bool success = gm.command_spawn_troop_tier_1();
        if (!success && hud != null) hud.ShowMessage("Недостаточно ресурсов!");
    }

    public void OnSpawnTroop2()
    {
        if (gm == null || gm.game_status != 0) return;
        if (gm.player_troops_queue.Count >= 20) return;
        bool success = gm.command_spawn_troop_tier_2();
        if (!success && hud != null) hud.ShowMessage("Недостаточно ресурсов!");
    }

    public void OnSpawnTroop3()
    {
        if (gm == null || gm.game_status != 0) return;
        if (gm.player_troops_queue.Count >= 20) return;
        bool success = gm.command_spawn_troop_tier_3();
        if (!success && hud != null) hud.ShowMessage("Недостаточно ресурсов!");
    }

    public void OnSpawnTroop4()
    {
        if (gm == null || gm.game_status != 0) return;
        if (gm.player_age != 5) return;
        if (gm.player_troops_queue.Count >= 20) return;
        bool success = gm.command_spawn_troop_tier_4();
        if (!success && hud != null) hud.ShowMessage("Недостаточно ресурсов!");
    }

    // ========== БАШНИ ==========

    public void OnSpawnTurret1()
    {
        if (gm == null || gm.game_status != 0) return;
        bool success = gm.command_spawn_turret_tier1();
        if (!success && hud != null)
        {
            if (gm.available_slots <= 0)
                hud.ShowMessage("Нет свободных слотов!");
            else
                hud.ShowMessage("Недостаточно ресурсов!");
        }
    }

    public void OnSpawnTurret2()
    {
        if (gm == null || gm.game_status != 0) return;
        bool success = gm.command_spawn_turret_tier2();
        if (!success && hud != null)
        {
            if (gm.available_slots <= 0)
                hud.ShowMessage("Нет свободных слотов!");
            else
                hud.ShowMessage("Недостаточно ресурсов!");
        }
    }

    public void OnSpawnTurret3()
    {
        if (gm == null || gm.game_status != 0) return;
        bool success = gm.command_spawn_turret_tier3();
        if (!success && hud != null)
        {
            if (gm.available_slots <= 0)
                hud.ShowMessage("Нет свободных слотов!");
            else
                hud.ShowMessage("Недостаточно ресурсов!");
        }
    }

    // ========== ПРОДАЖА БАШЕН ==========

    public void OnSellTurret(int slot)
    {
        if (gm == null || gm.game_status != 0) return;
        switch (slot)
        {
            case 0: gm.command_sell_spot0(); break;
            case 1: gm.command_sell_spot1(); break;
            case 2: gm.command_sell_spot2(); break;
            case 3: gm.command_sell_spot3(); break;
        }
    }

    // ========== СЛОТЫ ==========

    public void OnBuySlot()
    {
        if (gm == null || gm.game_status != 0) return;
        bool success = gm.command_buy_slot();
        if (!success && hud != null)
        {
            if (gm.total_slots >= 4)
                hud.ShowMessage("Максимум слотов!");
            else
                hud.ShowMessage("Недостаточно ресурсов!");
        }
    }

    // ========== ЭПОХИ ==========

    public void OnUpgradeAge()
    {
        if (gm == null || gm.game_status != 0) return;

        // Проверяем ограничение уровня
        if (levelData != null && gm.player_age >= levelData.playerMaxAge)
        {
            if (hud != null) hud.ShowMessage("Максимальная эпоха для этого уровня!");
            return;
        }

        bool success = gm.command_upgrade_age();
        if (!success && hud != null)
        {
            hud.ShowMessage("Недостаточно опыта!");
        }
    }

    // ========== СПОСОБНОСТЬ ==========

    public void OnUseAbility()
    {
        if (gm == null || gm.game_status != 0) return;
        bool success = gm.command_use_ability();
        if (!success && hud != null)
        {
            hud.ShowMessage("Способность перезаряжается!");
        }
    }

    // ========== Вспомогательные методы ==========

    /// <summary>
    /// Проверяет, может ли игрок спавнить юнита данного тира
    /// </summary>
    public bool CanSpawnTroop(int tier)
    {
        if (gm == null || gm.game_status != 0) return false;
        if (gm.player_troops_queue.Count >= 20) return false;

        // Проверка ограничения уровня
        if (levelData != null && tier > levelData.playerMaxTroopTier) return false;

        return gm.check_spawn_player_troop(tier);
    }

    /// <summary>
    /// Проверяет, может ли игрок купить башню данного тира
    /// </summary>
    public bool CanBuyTurret(int tier)
    {
        if (gm == null || gm.game_status != 0) return false;
        return gm.check_buy_turret_player(tier);
    }

    /// <summary>
    /// Получить текущий GameManager (для HUD)
    /// </summary>
    public GameManager GetGameManager()
    {
        return gm;
    }
}
