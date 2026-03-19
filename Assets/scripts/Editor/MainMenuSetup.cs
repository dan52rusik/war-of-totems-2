using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Автоматическая настройка сцены Главного Меню.
/// Запуск: Unity меню → Tools → Setup Main Menu Scene
/// </summary>
public class MainMenuSetup : EditorWindow
{
    [MenuItem("Tools/Setup Main Menu Scene")]
    public static void SetupMenuScene()
    {
        if (!EditorUtility.DisplayDialog("Setup Main Menu",
            "Это создаст все UI-объекты для Главного Меню.\nУбедитесь, что вы находитесь в пустой сцене (напр. MainMenu).\n\nЗаменить содержимое?", "Да", "Нет"))
        {
            return;
        }

        // ===================== MAIN CAMERA =====================
        if (Camera.main == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.12f);
            cam.orthographic = true;
            camObj.AddComponent<AudioListener>();
            Undo.RegisterCreatedObjectUndo(camObj, "Create Main Camera");
        }

        // ===================== CANVAS =====================
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        MainMenuController menuController = canvasObj.AddComponent<MainMenuController>();
        Undo.RegisterCreatedObjectUndo(canvasObj, "Create MainMenu Canvas");

        // EventSystem
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
        }

        // ===================== BACKGROUND =====================
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.12f, 1f);

        // ===================== MAIN PANEL =====================
        GameObject mainPanel = CreateUIPanel(canvasObj.transform, "MainPanel", Color.clear);
        TextMeshProUGUI title = CreateTMPText(mainPanel.transform, "Title", "WAR OF TOTEMS II", 80, Color.white, 800);
        title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 300);
        title.alignment = TextAlignmentOptions.Center;
        title.fontStyle = FontStyles.Bold;

        GameObject btnContainer = new GameObject("ButtonContainer");
        btnContainer.transform.SetParent(mainPanel.transform, false);
        VerticalLayoutGroup vlg = btnContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;

        Button playBtn = CreateStyledButton(btnContainer.transform, "PlayBtn", "В БОЙ", 300, 80, new Color(0.15f, 0.6f, 0.2f));
        Button shopBtn = CreateStyledButton(btnContainer.transform, "ShopBtn", "МАГАЗИН", 300, 80, new Color(0.1f, 0.35f, 0.6f));
        Button exitBtn = CreateStyledButton(btnContainer.transform, "ExitBtn", "ВЫХОД", 300, 80, new Color(0.4f, 0.15f, 0.15f));

        // Монеты в углу
        TextMeshProUGUI coinsText = CreateTMPText(mainPanel.transform, "TotalCoinsText", "Монеты: 0", 28, new Color(1f, 0.85f, 0.2f), 300);
        coinsText.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
        coinsText.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        coinsText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, -50);
        coinsText.alignment = TextAlignmentOptions.Right;

        // ===================== LEVELS PANEL =====================
        GameObject levelsPanel = CreateUIPanel(canvasObj.transform, "LevelsPanel", Color.black * 0.5f);
        CreateTMPText(levelsPanel.transform, "LTitle", "ВЫБОР УРОВНЯ", 40, Color.white, 400).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 400);

        GameObject gridObj = new GameObject("LevelGrid");
        gridObj.transform.SetParent(levelsPanel.transform, false);
        GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(300, 60);
        grid.spacing = new Vector2(20, 20);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        grid.childAlignment = TextAnchor.UpperCenter;
        gridObj.GetComponent<RectTransform>().sizeDelta = new Vector2(1400, 400);
        gridObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50);

        Button lBackBtn = CreateStyledButton(levelsPanel.transform, "BackBtn", "НАЗАД", 200, 60, Color.gray);
        lBackBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -450);

        // ===================== SHOP PANEL =====================
        GameObject shopPanel = CreateUIPanel(canvasObj.transform, "ShopPanel", Color.black * 0.7f);
        CreateTMPText(shopPanel.transform, "STitle", "МАГАЗИН УЛУЧШЕНИЙ", 40, Color.cyan, 600).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 400);

        GameObject shopContainer = new GameObject("ShopList");
        shopContainer.transform.SetParent(shopPanel.transform, false);
        VerticalLayoutGroup shopVlg = shopContainer.AddComponent<VerticalLayoutGroup>();
        shopVlg.spacing = 15;
        shopContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 600);

        // Создаем 4 строки магазина
        TextMeshProUGUI hpLvl, hpPrice;
        Button buyHp = CreateShopRow(shopContainer.transform, "КРЕПКИЕ СТЕНЫ (+HP)", out hpLvl, out hpPrice);

        TextMeshProUGUI rewLvl, rewPrice;
        Button buyRew = CreateShopRow(shopContainer.transform, "МАРОДЁРСТВО (+MONEY)", out rewLvl, out rewPrice);

        TextMeshProUGUI discLvl, discPrice;
        Button buyDisc = CreateShopRow(shopContainer.transform, "СКИДКИ (-COST)", out discLvl, out discPrice);

        TextMeshProUGUI smLvl, smPrice;
        Button buySm = CreateShopRow(shopContainer.transform, "ИНВЕСТИЦИИ (+START MONEY)", out smLvl, out smPrice);

        Button sBackBtn = CreateStyledButton(shopPanel.transform, "BackToMainBtn", "НАЗАД", 200, 60, Color.gray);
        sBackBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -450);

        // ===================== RESET BTN (CHEAT) =====================
        Button cheatBtn = CreateStyledButton(mainPanel.transform, "CheatBtn", "+1000", 120, 40, new Color(0.2f, 0.2f, 0.2f));
        cheatBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        cheatBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
        cheatBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(70, 30);

        // ===================== ASSIGN TO CONTROLLER =====================
        menuController.mainPanel = mainPanel;
        menuController.levelsPanel = levelsPanel;
        menuController.shopPanel = shopPanel;
        menuController.totalCoinsText = coinsText;
        menuController.levelButtonsParent = gridObj.transform;

        menuController.hpUpgradeLvlText = hpLvl;
        menuController.hpPriceText = hpPrice;
        menuController.rewardUpgradeLvlText = rewLvl;
        menuController.rewardPriceText = rewPrice;
        menuController.discountUpgradeLvlText = discLvl;
        menuController.discountPriceText = discPrice;
        menuController.startMoneyUpgradeLvlText = smLvl;
        menuController.startMoneyPriceText = smPrice;

        // Поиск уровней и префаба
        string[] levelGuids = AssetDatabase.FindAssets("t:LevelData");
        menuController.allLevels = new List<LevelData>();
        foreach (string guid in levelGuids)
        {
            menuController.allLevels.Add(AssetDatabase.LoadAssetAtPath<LevelData>(AssetDatabase.GUIDToAssetPath(guid)));
        }
        menuController.allLevels.Sort((a,b) => a.levelNumber.CompareTo(b.levelNumber));

        // Внимание: Чтобы это работало как префаб, его нужно сохранить в ассеты.
        // Но мы можем оставить его в сцене как шаблон.
        GameObject dummyBtn = new GameObject("LevelButtonPrefab");
        dummyBtn.transform.SetParent(canvasObj.transform, false);
        dummyBtn.AddComponent<RectTransform>().sizeDelta = new Vector2(300, 60);
        dummyBtn.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
        dummyBtn.AddComponent<Button>();

        GameObject btnText = new GameObject("Text");
        btnText.transform.SetParent(dummyBtn.transform, false);
        TextMeshProUGUI btxt = btnText.AddComponent<TextMeshProUGUI>();
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
        btxt.fontSize = 22;

        menuController.levelButtonPrefab = dummyBtn;
        dummyBtn.SetActive(false); // Скрываем шаблон

        // ===================== WIRE EVENTS =====================
        AddClick(playBtn, menuController, "ShowLevels");
        AddClick(shopBtn, menuController, "ShowShop");
        AddClick(exitBtn, Application.Quit); // Для билда
        AddClick(lBackBtn, menuController, "ShowMain");
        AddClick(sBackBtn, menuController, "ShowMain");
        AddClick(cheatBtn, menuController, "AddCoinsCheat");

        AddClick(buyHp, menuController, "BuyBaseHp");
        AddClick(buyRew, menuController, "BuyReward");
        AddClick(buyDisc, menuController, "BuyDiscount");
        AddClick(buySm, menuController, "BuyStartMoney");

        EditorUtility.DisplayDialog("Готово!", "Сцена Главного Меню настроена!\nНе забудьте сохранить сцену (Ctrl+S) и добавить в Build Settings.", "OK");
    }

    // Вспомогательные методы
    static GameObject CreateUIPanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        if (color != Color.clear)
        {
            Image img = panel.AddComponent<Image>();
            img.color = color;
        }
        return panel;
    }

    static TextMeshProUGUI CreateTMPText(Transform parent, string name, string text, float size, Color color, float width)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, size + 10);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    static Button CreateStyledButton(Transform parent, string name, string text, float w, float h, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>().sizeDelta = new Vector2(w, h);
        obj.AddComponent<Image>().color = color;
        Button btn = obj.AddComponent<Button>();
        CreateTMPText(obj.transform, "Text", text, 24, Color.white, w-10);
        return btn;
    }

    static Button CreateShopRow(Transform parent, string label, out TextMeshProUGUI lvl, out TextMeshProUGUI price)
    {
        GameObject row = new GameObject("ShopRow");
        row.transform.SetParent(parent, false);
        row.AddComponent<RectTransform>().sizeDelta = new Vector2(750, 70);
        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(10,10,5,5); hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleLeft; hlg.childControlWidth = false;

        CreateTMPText(row.transform, "Label", label, 20, Color.white, 300).alignment = TextAlignmentOptions.Left;
        lvl = CreateTMPText(row.transform, "Lvl", "УР: 0", 18, Color.cyan, 100);
        price = CreateTMPText(row.transform, "Price", "250", 18, Color.yellow, 100);
        Button buy = CreateStyledButton(row.transform, "BuyBtn", "КУПИТЬ", 150, 50, new Color(0.2f, 0.4f, 0.6f));

        return buy;
    }

    static void AddClick(Button b, UnityEngine.Events.UnityAction action) => UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(b.onClick, action);
    static void AddClick(Button b, Object target, string method)
    {
        var action = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), target, method) as UnityEngine.Events.UnityAction;
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(b.onClick, action);
    }
}
