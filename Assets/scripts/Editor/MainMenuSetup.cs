using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Автоматическая настройка сцены Главного Меню.
/// Tools → Setup Main Menu Scene
/// </summary>
public class MainMenuSetup : EditorWindow
{
    // Палитра
    static readonly Color BG       = new Color(0.94f, 0.91f, 0.82f);
    static readonly Color BAR      = new Color(0.22f, 0.28f, 0.38f, 0.92f);
    static readonly Color ORANGE   = new Color(0.96f, 0.72f, 0.18f);
    static readonly Color GREEN    = new Color(0.35f, 0.75f, 0.25f);
    static readonly Color DKBTN    = new Color(0.28f, 0.32f, 0.42f);
    static readonly Color RED      = new Color(0.85f, 0.25f, 0.2f);
    static readonly Color GOLD     = new Color(1f, 0.85f, 0.15f);
    static readonly Color TXT      = new Color(0.18f, 0.16f, 0.14f);
    static readonly Color CARD     = new Color(0.98f, 0.96f, 0.92f);

    [MenuItem("Tools/Setup Main Menu Scene")]
    public static void SetupMenuScene()
    {
        if (!EditorUtility.DisplayDialog("Setup Main Menu",
            "Создать UI Главного Меню?", "Да", "Нет")) return;

        // Очистка
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            if (c.name.Contains("MainMenu")) DestroyImmediate(c.gameObject);
        var es2 = GameObject.Find("EventSystem");
        if (es2) DestroyImmediate(es2);

        // Камера
        if (Camera.main == null)
        {
            var co = new GameObject("Main Camera"); co.tag = "MainCamera";
            var cm = co.AddComponent<Camera>();
            cm.clearFlags = CameraClearFlags.SolidColor;
            cm.backgroundColor = BG; cm.orthographic = true;
            co.AddComponent<AudioListener>();
        }
        else Camera.main.backgroundColor = BG;

        // Canvas
        var canvasObj = new GameObject("MainMenuCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();
        var mc = canvasObj.AddComponent<MainMenuController>();

        if (!FindObjectOfType<UnityEngine.EventSystems.EventSystem>())
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ==================== MAIN PANEL ====================
        var mainPanel = FullPanel(canvasObj.transform, "MainPanel", BG);

        // TopBar
        var topBar = TopBar(mainPanel.transform, "TopBar");
        var coinsText = BarLabel(topBar.transform, "CoinText", "0", GOLD);

        // Заголовок
        var t1 = Label(mainPanel.transform, "Title", "WAR OF TOTEMS", 46, TXT, FontStyles.Bold);
        Pos(t1, 0.5f, 1f, 0, -95);
        var t2 = Label(mainPanel.transform, "Sub", "Выбери сражение!", 18, new Color(0.45f, 0.42f, 0.38f));
        Pos(t2, 0.5f, 1f, 0, -148);

        // Кнопки
        var playBtn = Btn(mainPanel.transform, "PlayBtn", "В  Б О Й", 340, 75, ORANGE, TXT, 32);
        Pos(playBtn, 0.5f, 0.5f, 0, 10);
        var shopBtn = Btn(mainPanel.transform, "ShopBtn", "М А Г А З И Н", 340, 65, DKBTN, Color.white, 26);
        Pos(shopBtn, 0.5f, 0.5f, 0, -80);
        var exitBtn = Btn(mainPanel.transform, "ExitBtn", "ВЫХОД", 200, 50, RED, Color.white, 22);
        Pos(exitBtn, 0.5f, 0.5f, 0, -160);

        // Чит
        var cheatBtn = Btn(mainPanel.transform, "CheatBtn", "+1000", 100, 32, new Color(0.4f, 0.4f, 0.4f), Color.white, 14);
        var chR = cheatBtn.GetComponent<RectTransform>();
        chR.anchorMin = chR.anchorMax = new Vector2(0, 0); chR.pivot = new Vector2(0, 0);
        chR.anchoredPosition = new Vector2(15, 10);

        // ==================== LEVELS PANEL ====================
        var levelsPanel = FullPanel(canvasObj.transform, "LevelsPanel", BG);
        var lBar = TopBar(levelsPanel.transform, "LBar");
        BarLabel(lBar.transform, "LTitle", "ВЫБОР УРОВНЯ", Color.white);

        var gridObj = new GameObject("LevelGrid");
        gridObj.transform.SetParent(levelsPanel.transform, false);
        var gR = gridObj.AddComponent<RectTransform>();
        gR.anchorMin = new Vector2(0.05f, 0.14f); gR.anchorMax = new Vector2(0.95f, 0.88f);
        gR.offsetMin = gR.offsetMax = Vector2.zero;
        var grid = gridObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(280, 60); grid.spacing = new Vector2(16, 14);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3; grid.childAlignment = TextAnchor.UpperCenter;

        var lBack = Btn(levelsPanel.transform, "BackBtn", "НАЗАД", 200, 50, DKBTN, Color.white, 22);
        var lbR = lBack.GetComponent<RectTransform>();
        lbR.anchorMin = lbR.anchorMax = new Vector2(0.5f, 0); lbR.pivot = new Vector2(0.5f, 0);
        lbR.anchoredPosition = new Vector2(0, 20);

        // ==================== SHOP PANEL ====================
        var shopPanel = FullPanel(canvasObj.transform, "ShopPanel", BG);
        var sBar = TopBar(shopPanel.transform, "SBar");
        BarLabel(sBar.transform, "STitle", "УЛУЧШЕНИЯ", Color.white);
        var shopCoins = BarLabel(sBar.transform, "ShopCoins", "0", GOLD,
            TextAlignmentOptions.Right, new Vector2(-20, 0));

        // --- ScrollView для карточек ---
        var scrollGo = new GameObject("ShopScroll");
        scrollGo.transform.SetParent(shopPanel.transform, false);
        var scrollR = scrollGo.AddComponent<RectTransform>();
        // Между TopBar (55px сверху) и кнопками (65px снизу)
        scrollR.anchorMin = new Vector2(0, 0);
        scrollR.anchorMax = new Vector2(1, 1);
        scrollR.offsetMin = new Vector2(0, 65);   // снизу отступ для кнопок
        scrollR.offsetMax = new Vector2(0, -55);   // сверху отступ для TopBar
        var scrollImg = scrollGo.AddComponent<Image>();
        scrollImg.color = Color.white;  // alpha MUST be 1 for Mask to work!
        scrollGo.AddComponent<Mask>().showMaskGraphic = false;
        var scrollRect = scrollGo.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 30f;

        // Content — контейнер с фиксированной высотой
        var content = new GameObject("Content");
        content.transform.SetParent(scrollGo.transform, false);
        var contentR = content.AddComponent<RectTransform>();
        contentR.anchorMin = new Vector2(0, 1);
        contentR.anchorMax = new Vector2(1, 1);
        contentR.pivot = new Vector2(0.5f, 1f);
        // Высота подсчитаем после создания всех карточек

        scrollRect.content = contentR;

        // --- Теперь все карточки — дочерние Content, yPos относительно Content ---
        float startY = -10f;
        float cardH = 90f;
        float gap = 12f;
        float y = startY;

        // Секция: Улучшения
        SectionHeader(content.transform, "УЛУЧШЕНИЯ БАЗЫ", y);
        y -= 38;

        TextMeshProUGUI hpLvl, hpPrice;
        var buyHp = ShopCard(content.transform, "HP", "Здоровье базы",
            "+100 HP базы за уровень", new Color(0.85f, 0.25f, 0.3f), y, out hpLvl, out hpPrice);
        y -= (cardH + gap);

        TextMeshProUGUI smLvl, smPrice;
        var buySm = ShopCard(content.transform, "$+", "Стартовый капитал",
            "+50 монет в начале боя", new Color(0.88f, 0.72f, 0.15f), y, out smLvl, out smPrice);
        y -= (cardH + gap);

        TextMeshProUGUI rewLvl, rewPrice;
        var buyRew = ShopCard(content.transform, "x2", "Мародёрство",
            "+25% награды за победу", new Color(0.3f, 0.65f, 0.85f), y, out rewLvl, out rewPrice);
        y -= (cardH + gap);

        TextMeshProUGUI discLvl, discPrice;
        var buyDisc = ShopCard(content.transform, "-%", "Скидки на войска",
            "-10% стоимости юнитов", new Color(0.4f, 0.72f, 0.32f), y, out discLvl, out discPrice);
        y -= (cardH + gap + 6);

        // Секция: Юниты
        SectionHeader(content.transform, "РАЗБЛОКИРОВКА ЮНИТОВ", y);
        y -= 38;

        TextMeshProUGUI t2Lvl, t2Price;
        var buyT2 = ShopCard(content.transform, "T2", "Tier 2 - Разведчик",
            "Быстрый ближний боец", new Color(0.3f, 0.55f, 0.85f), y, out t2Lvl, out t2Price);
        y -= (cardH + gap);

        TextMeshProUGUI t3Lvl, t3Price;
        var buyT3 = ShopCard(content.transform, "T3", "Tier 3 - Танк",
            "Медленный, но живучий", new Color(0.6f, 0.4f, 0.8f), y, out t3Lvl, out t3Price);
        y -= (cardH + gap);

        TextMeshProUGUI t4Lvl, t4Price;
        var buyT4 = ShopCard(content.transform, "T4", "Tier 4 - Элита",
            "Мощный и дальнобойный", new Color(0.88f, 0.6f, 0.15f), y, out t4Lvl, out t4Price);
        y -= (cardH + 20); // нижний отступ

        // Установить общую высоту контента
        contentR.sizeDelta = new Vector2(0, Mathf.Abs(y));

        // Нижние кнопки (ВНЕ ScrollView!)
        var sBack = Btn(shopPanel.transform, "BackToMainBtn", "НАЗАД", 200, 48, DKBTN, Color.white, 20);
        var sbR = sBack.GetComponent<RectTransform>();
        sbR.anchorMin = sbR.anchorMax = new Vector2(0.3f, 0); sbR.pivot = new Vector2(0.5f, 0);
        sbR.anchoredPosition = new Vector2(0, 10);

        var resetBtn = Btn(shopPanel.transform, "ResetBtn", "Сброс", 150, 48, RED, Color.white, 18);
        var rsR = resetBtn.GetComponent<RectTransform>();
        rsR.anchorMin = rsR.anchorMax = new Vector2(0.7f, 0); rsR.pivot = new Vector2(0.5f, 0);
        rsR.anchoredPosition = new Vector2(0, 10);

        // ==================== ASSIGN ====================
        mc.mainPanel = mainPanel;
        mc.levelsPanel = levelsPanel;
        mc.shopPanel = shopPanel;
        mc.totalCoinsText = coinsText;
        mc.shopCoinsText = shopCoins;
        mc.levelButtonsParent = gridObj.transform;

        mc.hpUpgradeLvlText = hpLvl; mc.hpPriceText = hpPrice;
        mc.rewardUpgradeLvlText = rewLvl; mc.rewardPriceText = rewPrice;
        mc.discountUpgradeLvlText = discLvl; mc.discountPriceText = discPrice;
        mc.startMoneyUpgradeLvlText = smLvl; mc.startMoneyPriceText = smPrice;
        mc.tier2LvlText = t2Lvl; mc.tier2PriceText = t2Price;
        mc.tier3LvlText = t3Lvl; mc.tier3PriceText = t3Price;
        mc.tier4LvlText = t4Lvl; mc.tier4PriceText = t4Price;

        // Уровни
        var guids = AssetDatabase.FindAssets("t:LevelData");
        mc.allLevels = new List<LevelData>();
        foreach (string g in guids)
            mc.allLevels.Add(AssetDatabase.LoadAssetAtPath<LevelData>(AssetDatabase.GUIDToAssetPath(g)));
        mc.allLevels.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));

        // Шаблон кнопки уровня
        var pfab = new GameObject("LevelButtonPrefab");
        pfab.transform.SetParent(canvasObj.transform, false);
        pfab.AddComponent<RectTransform>().sizeDelta = new Vector2(280, 60);
        pfab.AddComponent<Image>().color = CARD;
        pfab.AddComponent<Button>();
        pfab.AddComponent<Outline>().effectColor = new Color(0.6f, 0.55f, 0.45f, 0.4f);
        var ptGo = new GameObject("Text");
        ptGo.transform.SetParent(pfab.transform, false);
        var ptR = ptGo.AddComponent<RectTransform>();
        ptR.anchorMin = Vector2.zero; ptR.anchorMax = Vector2.one;
        ptR.offsetMin = new Vector2(10, 0); ptR.offsetMax = new Vector2(-10, 0);
        var ptT = ptGo.AddComponent<TextMeshProUGUI>();
        ptT.alignment = TextAlignmentOptions.Center; ptT.color = TXT; ptT.fontSize = 18;
        mc.levelButtonPrefab = pfab; pfab.SetActive(false);

        // ==================== EVENTS ====================
        Wire(playBtn, mc, "ShowLevels"); Wire(shopBtn, mc, "ShowShop");
        Wire(exitBtn, Application.Quit);
        Wire(lBack, mc, "ShowMain"); Wire(sBack, mc, "ShowMain");
        Wire(cheatBtn, mc, "AddCoinsCheat"); Wire(resetBtn, mc, "ResetProgress");
        Wire(buyHp, mc, "BuyBaseHp"); Wire(buyRew, mc, "BuyReward");
        Wire(buyDisc, mc, "BuyDiscount"); Wire(buySm, mc, "BuyStartMoney");
        Wire(buyT2, mc, "BuyTier2"); Wire(buyT3, mc, "BuyTier3"); Wire(buyT4, mc, "BuyTier4");

        EditorUtility.DisplayDialog("Готово!", "Меню настроено! Сохраните Ctrl+S.", "OK");
    }

    // ======================= HELPERS =======================

    static GameObject FullPanel(Transform p, string name, Color c)
    {
        var go = new GameObject(name); go.transform.SetParent(p, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = c;
        return go;
    }

    static GameObject TopBar(Transform p, string name)
    {
        var go = new GameObject(name); go.transform.SetParent(p, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(0, 1); r.anchorMax = Vector2.one;
        r.pivot = new Vector2(0.5f, 1f);
        r.sizeDelta = new Vector2(0, 55); r.anchoredPosition = Vector2.zero;
        go.AddComponent<Image>().color = BAR;
        return go;
    }

    static TextMeshProUGUI BarLabel(Transform p, string name, string txt, Color c,
        TextAlignmentOptions align = TextAlignmentOptions.Center, Vector2 ofs = default)
    {
        var go = new GameObject(name); go.transform.SetParent(p, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(15, 0); r.offsetMax = new Vector2(-15, 0);
        if (ofs != default) r.anchoredPosition = ofs;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = txt; t.fontSize = 24; t.color = c;
        t.fontStyle = FontStyles.Bold; t.alignment = align;
        t.enableWordWrapping = false;
        return t;
    }

    static TextMeshProUGUI Label(Transform p, string name, string txt, float sz, Color c,
        FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name); go.transform.SetParent(p, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(1, 1);
        r.pivot = new Vector2(0.5f, 1f);
        r.sizeDelta = new Vector2(0, sz + 15);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = txt; t.fontSize = sz; t.color = c;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = style; t.enableWordWrapping = false;
        return t;
    }

    static void Pos(TextMeshProUGUI t, float ax, float ay, float ox, float oy)
    {
        var r = t.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(ax, ay);
        r.pivot = new Vector2(0.5f, 1f);
        r.anchoredPosition = new Vector2(ox, oy);
    }

    static void Pos(Button b, float ax, float ay, float ox, float oy)
    {
        var r = b.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(ax, ay);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = new Vector2(ox, oy);
    }

    static Button Btn(Transform p, string name, string txt,
        float w, float h, Color bg, Color tc, float fs)
    {
        var go = new GameObject(name); go.transform.SetParent(p, false);
        var r = go.AddComponent<RectTransform>(); r.sizeDelta = new Vector2(w, h);
        go.AddComponent<Image>().color = bg;
        var ol = go.AddComponent<Outline>();
        ol.effectColor = new Color(0, 0, 0, 0.15f); ol.effectDistance = new Vector2(0, -2);
        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        Color hi = bg * 1.12f; hi.a = 1f; cb.highlightedColor = hi;
        Color pr = bg * 0.82f; pr.a = 1f; cb.pressedColor = pr;
        cb.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.6f);
        btn.colors = cb;

        // Текст внутри кнопки
        var tGo = new GameObject("BtnText"); tGo.transform.SetParent(go.transform, false);
        var tR = tGo.AddComponent<RectTransform>();
        tR.anchorMin = Vector2.zero; tR.anchorMax = Vector2.one;
        tR.offsetMin = new Vector2(6, 0); tR.offsetMax = new Vector2(-6, 0);
        var t = tGo.AddComponent<TextMeshProUGUI>();
        t.text = txt; t.fontSize = fs; t.color = tc;
        t.fontStyle = FontStyles.Bold; t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = false; t.overflowMode = TextOverflowModes.Overflow;
        return btn;
    }

    // --- Заголовок секции ---
    static void SectionHeader(Transform p, string txt, float yPos)
    {
        var go = new GameObject("Section"); go.transform.SetParent(p, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(0.05f, 1); r.anchorMax = new Vector2(0.95f, 1);
        r.pivot = new Vector2(0.5f, 1f);
        r.sizeDelta = new Vector2(0, 32);
        r.anchoredPosition = new Vector2(0, yPos);

        // Тонкая подложка
        go.AddComponent<Image>().color = new Color(0.8f, 0.76f, 0.66f, 0.3f);

        var tGo = new GameObject("Txt"); tGo.transform.SetParent(go.transform, false);
        var tR = tGo.AddComponent<RectTransform>();
        tR.anchorMin = Vector2.zero; tR.anchorMax = Vector2.one;
        tR.offsetMin = new Vector2(12, 0); tR.offsetMax = Vector2.zero;
        var t = tGo.AddComponent<TextMeshProUGUI>();
        t.text = txt; t.fontSize = 16; t.color = new Color(0.48f, 0.44f, 0.4f);
        t.fontStyle = FontStyles.Bold; t.alignment = TextAlignmentOptions.Left;
        t.enableWordWrapping = false;
    }

    // --- Карточка улучшения ---
    static Button ShopCard(Transform parent, string icon, string title, string desc,
        Color accent, float yPos, out TextMeshProUGUI lvlText, out TextMeshProUGUI priceText)
    {
        // Карточка
        var card = new GameObject("Card_" + icon); card.transform.SetParent(parent, false);
        var cR = card.AddComponent<RectTransform>();
        cR.anchorMin = new Vector2(0.05f, 1); cR.anchorMax = new Vector2(0.95f, 1);
        cR.pivot = new Vector2(0.5f, 1f);
        cR.sizeDelta = new Vector2(0, 90);
        cR.anchoredPosition = new Vector2(0, yPos);
        card.AddComponent<Image>().color = CARD;
        var ol = card.AddComponent<Outline>();
        ol.effectColor = new Color(0.65f, 0.6f, 0.5f, 0.3f); ol.effectDistance = new Vector2(1, -2);

        // Цветная полоска слева
        var stripe = new GameObject("Stripe"); stripe.transform.SetParent(card.transform, false);
        var stR = stripe.AddComponent<RectTransform>();
        stR.anchorMin = new Vector2(0, 0); stR.anchorMax = new Vector2(0, 1);
        stR.pivot = new Vector2(0, 0.5f);
        stR.sizeDelta = new Vector2(6, 0); stR.anchoredPosition = Vector2.zero;
        stripe.AddComponent<Image>().color = accent;

        // Иконка
        var iGo = new GameObject("Icon"); iGo.transform.SetParent(card.transform, false);
        var iR = iGo.AddComponent<RectTransform>();
        iR.anchorMin = new Vector2(0, 0.5f); iR.anchorMax = new Vector2(0, 0.5f);
        iR.pivot = new Vector2(0, 0.5f);
        iR.anchoredPosition = new Vector2(16, 0); iR.sizeDelta = new Vector2(50, 50);
        iGo.AddComponent<Image>().color = new Color(accent.r, accent.g, accent.b, 0.15f);

        var iTxt = new GameObject("ILabel"); iTxt.transform.SetParent(iGo.transform, false);
        var itR = iTxt.AddComponent<RectTransform>();
        itR.anchorMin = Vector2.zero; itR.anchorMax = Vector2.one;
        itR.offsetMin = itR.offsetMax = Vector2.zero;
        var itT = iTxt.AddComponent<TextMeshProUGUI>();
        itT.text = icon; itT.fontSize = 20; itT.color = accent;
        itT.fontStyle = FontStyles.Bold; itT.alignment = TextAlignmentOptions.Center;
        itT.enableWordWrapping = false;

        // Название
        var nGo = new GameObject("Title"); nGo.transform.SetParent(card.transform, false);
        var nR = nGo.AddComponent<RectTransform>();
        nR.anchorMin = new Vector2(0, 0.5f); nR.anchorMax = new Vector2(1, 1);
        nR.offsetMin = new Vector2(76, 0); nR.offsetMax = new Vector2(-180, -6);
        var nT = nGo.AddComponent<TextMeshProUGUI>();
        nT.text = title; nT.fontSize = 18; nT.color = TXT;
        nT.fontStyle = FontStyles.Bold; nT.alignment = TextAlignmentOptions.Left;
        nT.enableWordWrapping = false;

        // Описание
        var dGo = new GameObject("Desc"); dGo.transform.SetParent(card.transform, false);
        var dR = dGo.AddComponent<RectTransform>();
        dR.anchorMin = new Vector2(0, 0); dR.anchorMax = new Vector2(1, 0.5f);
        dR.offsetMin = new Vector2(76, 6); dR.offsetMax = new Vector2(-180, 0);
        var dT = dGo.AddComponent<TextMeshProUGUI>();
        dT.text = desc; dT.fontSize = 13; dT.color = new Color(0.5f, 0.48f, 0.44f);
        dT.alignment = TextAlignmentOptions.Left;
        dT.enableWordWrapping = false;

        // Правая часть

        // Уровень
        var lvGo = new GameObject("Lvl"); lvGo.transform.SetParent(card.transform, false);
        var lvR = lvGo.AddComponent<RectTransform>();
        lvR.anchorMin = new Vector2(1, 0.65f); lvR.anchorMax = new Vector2(1, 1);
        lvR.pivot = new Vector2(1, 1);
        lvR.sizeDelta = new Vector2(165, 0); lvR.anchoredPosition = new Vector2(-8, -6);
        lvlText = lvGo.AddComponent<TextMeshProUGUI>();
        lvlText.text = "UR: 0/5"; lvlText.fontSize = 14; lvlText.color = accent;
        lvlText.fontStyle = FontStyles.Bold; lvlText.alignment = TextAlignmentOptions.Center;
        lvlText.enableWordWrapping = false;

        // Кнопка покупки
        var buyBtn = Btn(card.transform, "BuyBtn", "250", 155, 36, GREEN, Color.white, 17);
        var bR = buyBtn.GetComponent<RectTransform>();
        bR.anchorMin = new Vector2(1, 0); bR.anchorMax = new Vector2(1, 0.55f);
        bR.pivot = new Vector2(1, 0);
        bR.sizeDelta = new Vector2(155, 0);
        bR.anchoredPosition = new Vector2(-8, 8);

        // Скрытый текст цены (для контроллера)
        var prGo = new GameObject("Price"); prGo.transform.SetParent(card.transform, false);
        var prR = prGo.AddComponent<RectTransform>();
        prR.anchorMin = prR.anchorMax = new Vector2(1, 0); prR.sizeDelta = new Vector2(5, 1);
        priceText = prGo.AddComponent<TextMeshProUGUI>();
        priceText.text = "250"; priceText.fontSize = 1; priceText.color = Color.clear;

        return buyBtn;
    }

    // --- Events ---
    static void Wire(Button b, UnityEngine.Events.UnityAction a) =>
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(b.onClick, a);
    static void Wire(Button b, Object tgt, string m)
    {
        var a = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), tgt, m) as UnityEngine.Events.UnityAction;
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(b.onClick, a);
    }
}
