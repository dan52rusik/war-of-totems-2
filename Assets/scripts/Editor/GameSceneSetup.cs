using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor-скрипт для автоматической настройки GameScene.
/// Запуск: Unity меню → Tools → Setup Game Scene
/// </summary>
public class GameSceneSetup : EditorWindow
{
    [MenuItem("Tools/Setup Game Scene")]
    public static void SetupScene()
    {
        if (!EditorUtility.DisplayDialog("Setup Game Scene",
            "Это создаст все UI-объекты для игровой сцены.\nУбедитесь, что вы находитесь в GameScene.\n\nПродолжить?",
            "Да", "Отмена"))
        {
            return;
        }

        // ===================== CLEANUP =====================
        var oldLms = GameObject.FindGameObjectsWithTag("MainCamera");
        var oldLm = GameObject.Find("_LevelManager");
        if (oldLm) DestroyImmediate(oldLm);
        var oldCanvas = GameObject.Find("GameHUDCanvas");
        if (oldCanvas) DestroyImmediate(oldCanvas);
        var oldEvent = GameObject.Find("EventSystem");
        if (oldEvent) DestroyImmediate(oldEvent);

        // Также удаляем старые дубликаты, если их несколько (даже если у них приписка (1) в имени)
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (c.name != null && c.name.Contains("GameHUDCanvas")) DestroyImmediate(c.gameObject);
        }
        foreach (var l in FindObjectsByType<LevelManager>(FindObjectsSortMode.None))
        {
            if (l.gameObject.name != null && l.gameObject.name.Contains("_LevelManager")) DestroyImmediate(l.gameObject);
        }

        // ===================== LEVEL MANAGER =====================
        GameObject levelManagerObj = new GameObject("_LevelManager");
        LevelManager levelManager = levelManagerObj.AddComponent<LevelManager>();
        PlayerController playerController = levelManagerObj.AddComponent<PlayerController>();
        Undo.RegisterCreatedObjectUndo(levelManagerObj, "Create LevelManager");

        // === Автоматический поиск и назначение GameInstance prefab ===
        string[] prefabGuids = AssetDatabase.FindAssets("GameInstance t:Prefab");
        GameObject envPrefab = null;
        if (prefabGuids.Length > 0)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[0]);
            envPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            levelManager.environmentPrefab = envPrefab;
            Debug.Log("[Setup] Найден и назначен Environment Prefab: " + prefabPath);
        }
        else
        {
            Debug.LogWarning("[Setup] GameInstance prefab не найден! Назначьте вручную.");
        }

        // === Добавить Data компонент и скопировать спрайты из prefab ===
        Data dataComponent = levelManagerObj.AddComponent<Data>();
        if (envPrefab != null)
        {
            Data prefabData = envPrefab.GetComponent<Data>();
            if (prefabData != null)
            {
                // Копируем массивы спрайтов из prefab
                dataComponent.troop_sprites = prefabData.troop_sprites;
                dataComponent.turret_sprites = prefabData.turret_sprites;
                dataComponent.bases_sprites = prefabData.bases_sprites;
                dataComponent.ability_sprites = prefabData.ability_sprites;
                dataComponent.bases_towers_sprites = prefabData.bases_towers_sprites;
                Debug.Log("[Setup] Спрайты скопированы из GameInstance prefab.");
            }
            else
            {
                Debug.LogWarning("[Setup] Компонент Data не найден на GameInstance prefab. Заполните спрайты вручную.");
            }
        }
        levelManager.dataObject = dataComponent;

        // === Автоматический поиск LevelData ===
        string[] levelGuids = AssetDatabase.FindAssets("Level_01 t:LevelData");
        if (levelGuids.Length > 0)
        {
            string levelPath = AssetDatabase.GUIDToAssetPath(levelGuids[0]);
            LevelData ld = AssetDatabase.LoadAssetAtPath<LevelData>(levelPath);
            levelManager.levelData = ld;
            Debug.Log("[Setup] Найден и назначен Level Data: " + levelPath);
        }
        else
        {
            // Попробуем найти любой LevelData
            string[] anyLevelGuids = AssetDatabase.FindAssets("t:LevelData");
            if (anyLevelGuids.Length > 0)
            {
                string levelPath = AssetDatabase.GUIDToAssetPath(anyLevelGuids[0]);
                LevelData ld = AssetDatabase.LoadAssetAtPath<LevelData>(levelPath);
                levelManager.levelData = ld;
                Debug.Log("[Setup] Назначен Level Data: " + levelPath);
            }
            else
            {
                Debug.LogWarning("[Setup] LevelData ассеты не найдены. Создайте через Tools → Create Default Levels.");
            }
        }

        // ===================== MAIN CAMERA =====================
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
            mainCam.orthographic = true;
            mainCam.orthographicSize = 7f;
            mainCam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.nearClipPlane = 0.3f;
            mainCam.farClipPlane = 1000f;
            camObj.AddComponent<AudioListener>();
            camObj.transform.position = new Vector3(0f, 4.56f, -10.99119f);
            Undo.RegisterCreatedObjectUndo(camObj, "Create Main Camera");
            Debug.Log("[Setup] Создана Main Camera.");
        }

        // ===================== CANVAS (HUD) =====================
        GameObject canvasObj = new GameObject("GameHUDCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        GameUIController uiController = canvasObj.AddComponent<GameUIController>();
        Undo.RegisterCreatedObjectUndo(canvasObj, "Create GameHUD Canvas");
        Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");

        // EventSystem (если нет)
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
        }

        // ===================== TOP BAR =====================
        GameObject topBar = CreatePanel(canvasObj.transform, "TopBar",
            new Color(0.05f, 0.05f, 0.12f, 0.9f),
            TextAnchor.UpperCenter);
        RectTransform topBarRect = topBar.GetComponent<RectTransform>();
        topBarRect.anchorMin = new Vector2(0, 1);
        topBarRect.anchorMax = new Vector2(1, 1);
        topBarRect.pivot = new Vector2(0.5f, 1);
        topBarRect.sizeDelta = new Vector2(0, 80);
        topBarRect.anchoredPosition = Vector2.zero;

        // Outline на TopBar для стиля
        Outline topOutline = topBar.AddComponent<Outline>();
        topOutline.effectColor = new Color(0.3f, 0.5f, 1f, 0.4f);
        topOutline.effectDistance = new Vector2(0, -2);

        HorizontalLayoutGroup topLayout = topBar.AddComponent<HorizontalLayoutGroup>();
        topLayout.spacing = 30;
        topLayout.padding = new RectOffset(20, 20, 10, 10);
        topLayout.childAlignment = TextAnchor.MiddleCenter;
        topLayout.childControlWidth = false;
        topLayout.childControlHeight = true;
        topLayout.childForceExpandWidth = false;
        topLayout.childForceExpandHeight = true;

        // Тексты верхней панели
        TextMeshProUGUI moneyText = CreateTMPText(topBar.transform, "MoneyText", "175", 24, new Color(1f, 0.85f, 0.2f), 180);
        TextMeshProUGUI xpText = CreateTMPText(topBar.transform, "XpText", "0", 24, new Color(0.5f, 0.8f, 1f), 150);
        TextMeshProUGUI playerHpText = CreateTMPText(topBar.transform, "PlayerHpText", "500", 24, new Color(0.3f, 1f, 0.4f), 160);

        // Центральный блок — Эпоха
        TextMeshProUGUI ageText = CreateTMPText(topBar.transform, "AgeText", "Эпоха 1", 28, Color.white, 200);
        ageText.alignment = TextAlignmentOptions.Center;
        ageText.fontStyle = FontStyles.Bold;

        TextMeshProUGUI enemyHpText = CreateTMPText(topBar.transform, "EnemyHpText", "500", 24, new Color(1f, 0.35f, 0.35f), 160);

        // Кнопки управления в TopBar
        TextMeshProUGUI speedText;
        Button pauseBtn = CreateStyledButton(topBar.transform, "PauseBtn", "Пауза", 120, 50,
            new Color(0.3f, 0.3f, 0.4f), out _);
        Button speedBtn = CreateStyledButton(topBar.transform, "SpeedBtn", "x1", 100, 50,
            new Color(0.2f, 0.35f, 0.5f), out speedText);

        // ===================== LEVEL NAME (поверх TopBar) =====================
        TextMeshProUGUI levelNameText = CreateTMPText(canvasObj.transform, "LevelNameText", "Уровень 1: Каменный век", 18, new Color(0.7f, 0.7f, 0.85f), 400);
        RectTransform levelNameRect = levelNameText.GetComponent<RectTransform>();
        levelNameRect.anchorMin = new Vector2(0.5f, 1);
        levelNameRect.anchorMax = new Vector2(0.5f, 1);
        levelNameRect.pivot = new Vector2(0.5f, 1);
        levelNameRect.anchoredPosition = new Vector2(0, -82);
        levelNameRect.sizeDelta = new Vector2(400, 30);
        levelNameText.alignment = TextAlignmentOptions.Center;

        // ===================== BOTTOM BAR =====================
        GameObject bottomBar = CreatePanel(canvasObj.transform, "BottomBar",
            new Color(0.05f, 0.05f, 0.12f, 0.92f),
            TextAnchor.MiddleLeft);
        RectTransform bottomBarRect = bottomBar.GetComponent<RectTransform>();
        bottomBarRect.anchorMin = new Vector2(0, 0);
        bottomBarRect.anchorMax = new Vector2(1, 0);
        bottomBarRect.pivot = new Vector2(0.5f, 0);
        bottomBarRect.sizeDelta = new Vector2(0, 140);
        bottomBarRect.anchoredPosition = Vector2.zero;

        Outline bottomOutline = bottomBar.AddComponent<Outline>();
        bottomOutline.effectColor = new Color(0.3f, 0.5f, 1f, 0.4f);
        bottomOutline.effectDistance = new Vector2(0, 2);

        HorizontalLayoutGroup bottomLayout = bottomBar.AddComponent<HorizontalLayoutGroup>();
        bottomLayout.spacing = 8;
        bottomLayout.padding = new RectOffset(15, 15, 8, 8);
        bottomLayout.childAlignment = TextAnchor.MiddleLeft;
        bottomLayout.childControlWidth = false;
        bottomLayout.childControlHeight = false;
        bottomLayout.childForceExpandWidth = false;
        bottomLayout.childForceExpandHeight = false;

        // --- Секция юнитов (ЕДИНСТВЕННАЯ) ---
        GameObject troopSection = CreateSection(bottomBar.transform, "TroopSection", "ЮНИТЫ", 800);

        Button[] troopButtons = new Button[4];
        TextMeshProUGUI[] troopCostTexts = new TextMeshProUGUI[4];

        string[] troopNames = { "Tier 1", "Tier 2", "Tier 3", "Tier 4" };
        Color[] troopColors = {
            new Color(0.2f, 0.45f, 0.2f),
            new Color(0.2f, 0.35f, 0.55f),
            new Color(0.45f, 0.2f, 0.5f),
            new Color(0.6f, 0.45f, 0.1f)
        };
        string[] troopCosts = { "15", "25", "100", "150K" };

        Transform troopBtnParent = troopSection.transform.Find("ButtonContainer");
        for (int i = 0; i < 4; i++)
        {
            TextMeshProUGUI costText;
            troopButtons[i] = CreateUnitButton(troopBtnParent, "TroopBtn" + (i + 1),
                troopNames[i], troopCosts[i], troopColors[i], out costText);
            troopCostTexts[i] = costText;
        }

        // ===================== WIN PANEL =====================
        GameObject winPanel = CreateResultPanel(canvasObj.transform, "WinPanel",
            "ПОБЕДА!", new Color(0.1f, 0.3f, 0.1f, 0.95f));
        Transform winCenter = winPanel.transform.Find("CenterBlock");
        TextMeshProUGUI winCoinsText = winCenter.Find("SubText").GetComponent<TextMeshProUGUI>();

        Button winNextBtn = CreateStyledButton(winCenter.Find("ButtonContainer"),
            "NextLevelBtn", "Следующий уровень", 300, 55,
            new Color(0.15f, 0.4f, 0.15f), out _);
        Button winMenuBtn = CreateStyledButton(winCenter.Find("ButtonContainer"),
            "MenuBtn", "Меню", 200, 55,
            new Color(0.3f, 0.3f, 0.4f), out _);
        Button winRetryBtn = CreateStyledButton(winCenter.Find("ButtonContainer"),
            "RetryBtn", "Заново", 200, 55,
            new Color(0.3f, 0.25f, 0.1f), out _);

        winPanel.SetActive(false);

        // ===================== LOSE PANEL =====================
        GameObject losePanel = CreateResultPanel(canvasObj.transform, "LosePanel",
            "ПОРАЖЕНИЕ!", new Color(0.35f, 0.08f, 0.08f, 0.95f));
        Transform loseCenter = losePanel.transform.Find("CenterBlock");

        Button loseRetryBtn = CreateStyledButton(loseCenter.Find("ButtonContainer"),
            "RetryBtn", "Попробовать снова", 300, 55,
            new Color(0.4f, 0.15f, 0.15f), out _);
        Button loseMenuBtn = CreateStyledButton(loseCenter.Find("ButtonContainer"),
            "MenuBtn", "Меню", 200, 55,
            new Color(0.3f, 0.3f, 0.4f), out _);

        losePanel.SetActive(false);

        // ===================== WIRE UI EVENTS =====================
        // Подключаем кнопки спавна
        AddButtonEvent(troopButtons[0], uiController, "OnSpawnTier1");
        AddButtonEvent(troopButtons[1], uiController, "OnSpawnTier2");
        AddButtonEvent(troopButtons[2], uiController, "OnSpawnTier3");
        AddButtonEvent(troopButtons[3], uiController, "OnSpawnTier4");

        // Подключаем панели победы/поражения через LevelManager
        LevelManager lm = levelManagerObj.GetComponent<LevelManager>();
        AddButtonEvent(winNextBtn, lm, "OnNextLevelButton");
        AddButtonEvent(winMenuBtn, lm, "OnMainMenuButton");
        AddButtonEvent(winRetryBtn, lm, "OnRetryButton");
        AddButtonEvent(loseRetryBtn, lm, "OnRetryButton");
        AddButtonEvent(loseMenuBtn, lm, "OnMainMenuButton");

        // Привязываем тексты в UIController
        uiController.moneyText = moneyText;
        uiController.xpText = xpText;
        uiController.hpText = playerHpText;
        uiController.ageText = ageText;

        Debug.Log("[Setup] UI События успешно подключены!");

        // ===================== PAUSE PANEL =====================
        GameObject pausePanel = CreateResultPanel(canvasObj.transform, "PausePanel",
            "ПАУЗА", new Color(0.1f, 0.1f, 0.2f, 0.95f));

        Transform pCenter = pausePanel.transform.Find("CenterBlock");
        Transform pBtns = pCenter.Find("ButtonContainer");

        Button pauseResumeBtn = CreateStyledButton(pBtns, "ResumeBtn", "Продолжить", 160, 55, new Color(0.15f, 0.35f, 0.15f), out _);
        Button pauseRetryBtn = CreateStyledButton(pBtns, "RetryBtn", "Заново", 160, 55, new Color(0.4f, 0.3f, 0.15f), out _);
        Button pauseMenuBtn = CreateStyledButton(pBtns, "PauseMenuBtn", "Меню", 160, 55, new Color(0.3f, 0.3f, 0.4f), out _);

        // Кнопка закрытия (крестик)
        Button closeBtn = CreateStyledButton(pCenter, "CloseBtn", "X", 45, 45, new Color(0.7f, 0.2f, 0.2f, 0.9f), out _);
        RectTransform closeRect = closeBtn.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.pivot = new Vector2(1, 1);
        closeRect.anchoredPosition = new Vector2(-15, -15);

        pausePanel.SetActive(false);

        // ===================== MESSAGE TEXT =====================
        TextMeshProUGUI messageText = CreateTMPText(canvasObj.transform, "MessageText",
            "", 26, new Color(1f, 0.8f, 0.2f), 600);
        RectTransform msgRect = messageText.GetComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0.5f, 0.5f);
        msgRect.anchorMax = new Vector2(0.5f, 0.5f);
        msgRect.pivot = new Vector2(0.5f, 0.5f);
        msgRect.anchoredPosition = new Vector2(0, 100);
        msgRect.sizeDelta = new Vector2(600, 50);
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.fontStyle = FontStyles.Bold;

        // Фон для сообщения
        Image msgBg = messageText.gameObject.AddComponent<Image>();
        msgBg.color = new Color(0, 0, 0, 0.7f);
        msgBg.raycastTarget = false;
        // Поместить текст поверх фона
        messageText.raycastTarget = false;

        messageText.gameObject.SetActive(false);

        // ===================== WIRE EVERYTHING =====================

        // GameUIController assignments
        uiController.moneyText = moneyText;
        uiController.xpText = xpText;
        uiController.hpText = playerHpText;
        uiController.ageText = ageText;
        uiController.levelNameText = levelNameText;
        uiController.speedText = speedText;
        uiController.winPanel = winPanel;
        uiController.winCoinsText = winCoinsText;
        uiController.losePanel = losePanel;
        uiController.pausePanel = pausePanel;
        uiController.troopButtons = troopButtons;

        // Control buttons → LevelManager
        AddButtonEvent(speedBtn, levelManager, "OnSpeedUpButton");

        // LevelManager
        levelManager.gameHUD = uiController;

        // Win panel buttons
        AddButtonEvent(winNextBtn, levelManager, "OnNextLevelButton");
        AddButtonEvent(winMenuBtn, levelManager, "OnMainMenuButton");
        AddButtonEvent(winRetryBtn, levelManager, "OnRetryButton");

        // Lose panel buttons
        AddButtonEvent(loseRetryBtn, levelManager, "OnRetryButton");
        AddButtonEvent(loseMenuBtn, levelManager, "OnMainMenuButton");

        // Pause panel buttons (привязываются ДИНАМИЧЕСКИ в GameUIController)

        // Пометить сцену как изменённую
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Готово!",
            "Игровая сцена настроена!\n\n" +
            "Автоматически назначены:\n" +
            "✅ Environment Prefab (GameInstance)\n" +
            "✅ Data Object (со спрайтами)\n" +
            (levelManager.levelData != null ? "✅ Level Data\n" : "⚠️ Level Data — создайте через Tools → Create Default Levels\n") +
            "\nСохраните сцену (Ctrl+S) и нажмите Play!",
            "OK");

        Selection.activeGameObject = levelManagerObj;
    }

    // ===================== HELPER METHODS =====================

    static GameObject CreatePanel(Transform parent, string name, Color color, TextAnchor childAlignment)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        Image img = panel.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = true;
        return panel;
    }

    static TextMeshProUGUI CreateTMPText(Transform parent, string name, string text,
        float fontSize, Color color, float width)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, 40);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.enableAutoSizing = false;
        tmp.raycastTarget = false;

        return tmp;
    }

    static Button CreateStyledButton(Transform parent, string name, string text,
        float width, float height, Color bgColor, out TextMeshProUGUI buttonText)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.25f, 1.25f, 1.3f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.75f);
        colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.5f);
        btn.colors = colors;

        // Обводка
        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.22f);
        outline.effectDistance = new Vector2(1, -1);

        // Текст на кнопке
        buttonText = CreateTMPText(btnObj.transform, "Text", text, 20, Color.white, width - 10);
        RectTransform textRect = buttonText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;

        // Тень текста
        Shadow textShadow = buttonText.gameObject.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0, 0, 0, 0.5f);
        textShadow.effectDistance = new Vector2(1, -1);

        return btn;
    }

    static Button CreateUnitButton(Transform parent, string name, string title, string cost,
        Color bgColor, out TextMeshProUGUI costText)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(105, 90);

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.2f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.8f);
        colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.6f);
        btn.colors = colors;

        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(1, 1, 1, 0.12f);
        outline.effectDistance = new Vector2(1, -1);

        // Название юнита
        TextMeshProUGUI titleText = CreateTMPText(btnObj.transform, "Title", title,
            16, Color.white, 95);
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.5f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = Vector2.zero;
        titleRect.anchoredPosition = new Vector2(0, -2);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;

        // Стоимость
        costText = CreateTMPText(btnObj.transform, "Cost", cost,
            14, new Color(1f, 0.85f, 0.3f), 95);
        RectTransform costRect = costText.GetComponent<RectTransform>();
        costRect.anchorMin = new Vector2(0, 0);
        costRect.anchorMax = new Vector2(1, 0.45f);
        costRect.sizeDelta = Vector2.zero;
        costRect.anchoredPosition = Vector2.zero;
        costText.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    static GameObject CreateSection(Transform parent, string name, string label, float width)
    {
        GameObject section = new GameObject(name);
        section.transform.SetParent(parent, false);
        RectTransform rect = section.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, 120);

        VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2;
        vlg.padding = new RectOffset(5, 5, 0, 0);
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Лейбл секции
        TextMeshProUGUI labelText = CreateTMPText(section.transform, "Label", label, 12,
            new Color(0.5f, 0.6f, 0.8f), width);
        RectTransform labelRect = labelText.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(width, 18);
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.fontStyle = FontStyles.Bold;
        labelText.characterSpacing = 5;

        // Контейнер кнопок
        GameObject btnContainer = new GameObject("ButtonContainer");
        btnContainer.transform.SetParent(section.transform, false);
        RectTransform btnRect = btnContainer.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(width, 95);

        HorizontalLayoutGroup hlg = btnContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 6;
        hlg.padding = new RectOffset(2, 2, 2, 2);
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        return section;
    }

    static void CreateSeparator(Transform parent)
    {
        GameObject sep = new GameObject("Separator");
        sep.transform.SetParent(parent, false);
        RectTransform rect = sep.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(2, 100);
        Image img = sep.AddComponent<Image>();
        img.color = new Color(0.3f, 0.4f, 0.6f, 0.5f);
    }

    static GameObject CreateResultPanel(Transform parent, string name, string title, Color bgColor)
    {
        // === Полноэкранный затемняющий фон ===
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.82f);

        // === Центральная карточка ===
        GameObject card = new GameObject("CenterBlock");
        card.transform.SetParent(panel.transform, false);
        RectTransform cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(560, 340);
        cardRect.anchoredPosition = Vector2.zero;

        Image cardBg = card.AddComponent<Image>();
        cardBg.color = bgColor;

        // Обводка карточки
        Outline cardOutline = card.AddComponent<Outline>();
        cardOutline.effectColor = new Color(1f, 1f, 1f, 0.18f);
        cardOutline.effectDistance = new Vector2(3, -3);

        // === Заголовок (ПОБЕДА! / ПОРАЖЕНИЕ!) ===
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(card.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0, 85);
        titleRect.anchoredPosition = new Vector2(0, -30);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = title;
        titleTmp.fontSize = 52;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = Color.white;

        // Тень заголовка
        Shadow titleShadow = titleObj.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0, 0, 0, 0.6f);
        titleShadow.effectDistance = new Vector2(2, -2);

        // === Декоративная линия под заголовком ===
        GameObject line = new GameObject("Divider");
        line.transform.SetParent(card.transform, false);
        RectTransform lineRect = line.AddComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.1f, 1f);
        lineRect.anchorMax = new Vector2(0.9f, 1f);
        lineRect.pivot = new Vector2(0.5f, 1f);
        lineRect.sizeDelta = new Vector2(0, 2);
        lineRect.anchoredPosition = new Vector2(0, -125);
        Image lineImg = line.AddComponent<Image>();
        lineImg.color = new Color(1f, 1f, 1f, 0.15f);

        // === Подтекст (награда) ===
        GameObject subObj = new GameObject("SubText");
        subObj.transform.SetParent(card.transform, false);
        RectTransform subRect = subObj.AddComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0f, 1f);
        subRect.anchorMax = new Vector2(1f, 1f);
        subRect.pivot = new Vector2(0.5f, 1f);
        subRect.sizeDelta = new Vector2(0, 45);
        subRect.anchoredPosition = new Vector2(0, -140);
        TextMeshProUGUI subTmp = subObj.AddComponent<TextMeshProUGUI>();
        subTmp.text = "";
        subTmp.fontSize = 24;
        subTmp.alignment = TextAlignmentOptions.Center;
        subTmp.color = new Color(1f, 0.88f, 0.35f);
        subTmp.fontStyle = FontStyles.Bold;

        // === Контейнер кнопок (фиксированный, по центру снизу) ===
        GameObject btnContainer = new GameObject("ButtonContainer");
        btnContainer.transform.SetParent(card.transform, false);
        RectTransform btnContRect = btnContainer.AddComponent<RectTransform>();
        btnContRect.anchorMin = new Vector2(0f, 0f);
        btnContRect.anchorMax = new Vector2(1f, 0f);
        btnContRect.pivot = new Vector2(0.5f, 0f);
        btnContRect.sizeDelta = new Vector2(0, 65);
        btnContRect.anchoredPosition = new Vector2(0, 30);

        HorizontalLayoutGroup hlg = btnContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 14;
        hlg.padding = new RectOffset(24, 24, 0, 0);
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        return panel;
    }

    static void AddButtonEvent(Button button, Object target, string methodName)
    {
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            button.onClick,
            System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction),
                target,
                methodName
            ) as UnityEngine.Events.UnityAction
        );
    }
}
