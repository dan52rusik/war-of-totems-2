using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Linq;

public class Troop : MonoBehaviour
{

    [HideInInspector] public bool isPlayer;

    [HideInInspector] public GameManager game_manager;
    [HideInInspector] public Data.TroopData troop_data; // has all the data for the troops
    [HideInInspector] public GameObject next_troop;
    [HideInInspector] public GameObject prev_troop;
    [HideInInspector] public GameObject attacked_gm;
    [HideInInspector] public bool is_regenerating = false;
    [HideInInspector] public int max_health;

    bool is_moving;
    bool attacking_range = false;
    bool attacking_melee = false;
    bool melee_routine = false;
    bool range_routine = false;
    

    [SerializeField]
    TextMeshProUGUI hp, attacking, moving, additonal;
    [SerializeField]
    GameObject local_canvas;
    [SerializeField] SpriteRenderer spriteR;
    [SerializeField] BoxCollider2D Box;
    SpriteRenderer silhouetteRenderer;
    SpriteRenderer bodyRenderer;
    SpriteRenderer crestRenderer;
    SpriteRenderer weaponRenderer;
    SpriteRenderer headRenderer;
    SpriteRenderer leftArmRenderer;
    SpriteRenderer rightArmRenderer;
    SpriteRenderer leftLegRenderer;
    SpriteRenderer rightLegRenderer;
    float visualScaleMultiplier = 1f;
    static Dictionary<string, Sprite> proceduralSprites = new Dictionary<string, Sprite>();

    // HP Bar
    private Image hpFill;
    private bool hpBarCreated = false;


    Coroutine inst_melee;
    Coroutine inst_range;

    [System.NonSerialized]
    public bool info = false;

    Data data;
    float laneLocalY;
    float animationSeed;
    Vector3 bodyBasePosition;
    Vector3 crestBasePosition;
    Vector3 weaponBasePosition;
    Vector3 headBasePosition;
    Vector3 leftArmBasePosition;
    Vector3 rightArmBasePosition;
    Vector3 leftLegBasePosition;
    Vector3 rightLegBasePosition;

    void manage_texts()
    {
        if (info)
        {
            int health = troop_data.health;
            if (hp != null) hp.text = "" + health;
            if (moving != null) moving.text = "" + is_moving;
            if (attacking != null) attacking.text = "" + attacking_melee + "\n" + attacking_range;
        }
        else
        {
            // Скрываем дебаг-тексты, если они не нужны
            if (hp != null && hp.gameObject.activeSelf) hp.gameObject.SetActive(false);
            if (moving != null && moving.gameObject.activeSelf) moving.gameObject.SetActive(false);
            if (attacking != null && attacking.gameObject.activeSelf) attacking.gameObject.SetActive(false);
        }

        UpdateHPBar(troop_data.health);
    }

    void UpdateHPBar(int health)
    {
        if (!hpBarCreated) CreateHPBar();
        if (hpFill != null)
        {
            float fill = Mathf.Clamp01((float)health / max_health);
            hpFill.fillAmount = fill;
            // Цвет: зелёный для игрока, красный для врага
            hpFill.color = isPlayer ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f);
        }
    }

    void CreateHPBar()
    {
        if (local_canvas == null) return;
        
        // Фон баре
        GameObject bgObj = new GameObject("HP_BG");
        bgObj.transform.SetParent(local_canvas.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(100, 10);
        bgRect.anchoredPosition = new Vector2(0, 50); // Над головой
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.5f);

        // Заполнение
        GameObject fillObj = new GameObject("HP_Fill");
        fillObj.transform.SetParent(bgObj.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = new Vector2(-4, -4); // Отступ
        
        hpFill = fillObj.AddComponent<Image>();
        hpFill.type = Image.Type.Filled;
        hpFill.fillMethod = Image.FillMethod.Horizontal;
        hpFill.fillAmount = 1f;
        
        hpBarCreated = true;
    }

    void manage_sprites()
    {
        int id = troop_data.id;
        if (data.troop_sprites != null && id >= 0 && id < data.troop_sprites.Length)
        {
            spriteR.sprite = data.troop_sprites[id];
            if (silhouetteRenderer != null)
            {
                silhouetteRenderer.sprite = spriteR.sprite;
            }
        }
        // Не спамим варнинг — спрайты могут быть не назначены
    }

    
    void ApplyVisualStyle()
    {
        if (spriteR == null || troop_data == null) return;

        int age = Mathf.Clamp(troop_data.id / 3, 0, 4);
        int role = troop_data.id == 15 ? 3 : troop_data.id % 3;

        Color[] agePalette =
        {
            new Color(0.64f, 0.69f, 0.7f),
            new Color(0.58f, 0.68f, 0.72f),
            new Color(0.54f, 0.64f, 0.68f),
            new Color(0.5f, 0.6f, 0.64f),
            new Color(0.44f, 0.56f, 0.62f)
        };

        Color[] rolePalette =
        {
            new Color(0.88f, 0.98f, 0.9f),
            new Color(0.84f, 0.92f, 1.02f),
            new Color(0.96f, 0.9f, 0.86f),
            new Color(1.02f, 0.98f, 0.84f)
        };

        float[] roleScale =
        {
            0.96f,
            0.92f,
            1.08f,
            1.18f
        };

        Color teamTint = isPlayer
            ? new Color(0.96f, 1f, 0.98f, 1f)
            : new Color(0.82f, 0.9f, 1.06f, 1f);

        Color finalTint = agePalette[age] * rolePalette[Mathf.Clamp(role, 0, rolePalette.Length - 1)] * teamTint;
        finalTint.a = 1f;
        spriteR.color = new Color(finalTint.r * 0.5f, finalTint.g * 0.5f, finalTint.b * 0.5f, 0.04f);
        visualScaleMultiplier = roleScale[Mathf.Clamp(role, 0, roleScale.Length - 1)];

        EnsureSilhouette();
        EnsureAttachmentRenderers();
        ConfigureAttachments(age, role, finalTint);
    }

    void EnsureSilhouette()
    {
        if (spriteR == null) return;

        if (silhouetteRenderer == null)
        {
            Transform existing = transform.Find("VisualSilhouette");
            if (existing != null)
            {
                silhouetteRenderer = existing.GetComponent<SpriteRenderer>();
            }
        }

        if (silhouetteRenderer == null)
        {
            GameObject silhouette = new GameObject("VisualSilhouette");
            silhouette.transform.SetParent(transform, false);
            silhouette.transform.localPosition = new Vector3(0.05f, -0.06f, 0f);
            silhouette.transform.localRotation = Quaternion.identity;
            silhouette.transform.localScale = Vector3.one;
            silhouetteRenderer = silhouette.AddComponent<SpriteRenderer>();
        }

        silhouetteRenderer.sprite = spriteR.sprite;
        silhouetteRenderer.color = isPlayer
            ? new Color(0.08f, 0.12f, 0.05f, 0.18f)
            : new Color(0.06f, 0.08f, 0.14f, 0.18f);
        silhouetteRenderer.sortingLayerID = spriteR.sortingLayerID;
        silhouetteRenderer.sortingOrder = spriteR.sortingOrder - 1;
    }

    static Sprite GetProceduralSprite(string key)
    {
        if (proceduralSprites.TryGetValue(key, out Sprite cached))
        {
            return cached;
        }

        Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color white = Color.white;
        Color[] pixels = Enumerable.Repeat(clear, 32 * 32).ToArray();

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float nx = (x + 0.5f) / 32f;
                float ny = (y + 0.5f) / 32f;
                bool fill = false;

                switch (key)
                {
                    case "cape":
                        fill = ny > 0.12f && ny < 0.9f && nx > 0.34f - ny * 0.08f && nx < 0.66f + ny * 0.08f;
                        break;
                    case "armor":
                        fill = ny > 0.26f && ny < 0.84f && nx > 0.28f && nx < 0.72f;
                        fill &= !(ny > 0.68f && (nx < 0.38f || nx > 0.62f));
                        fill |= ny > 0.58f && ny < 0.66f && nx > 0.3f && nx < 0.7f;
                        fill |= ny > 0.18f && ny < 0.3f && nx > 0.22f && nx < 0.78f;
                        break;
                    case "torso":
                        fill = ny > 0.14f && ny < 0.9f && nx > 0.28f && nx < 0.72f;
                        fill &= !(ny > 0.76f && (nx < 0.4f || nx > 0.6f));
                        fill |= ny > 0.52f && ny < 0.62f && nx > 0.24f && nx < 0.76f;
                        break;
                    case "mask":
                        fill = nx > 0.24f && nx < 0.76f && ny > 0.24f && ny < 0.78f;
                        fill &= !(ny > 0.42f && ny < 0.54f && nx > 0.36f && nx < 0.64f);
                        fill |= ny > 0.72f && nx > 0.34f && nx < 0.66f;
                        break;
                    case "modern_face":
                        fill = nx > 0.34f && nx < 0.66f && ny > 0.22f && ny < 0.7f;
                        fill |= ny > 0.16f && ny < 0.28f && nx > 0.4f && nx < 0.6f;
                        fill &= !(ny > 0.48f && ny < 0.58f && nx > 0.38f && nx < 0.62f);
                        break;
                    case "modern_head":
                        fill = nx > 0.26f && nx < 0.74f && ny > 0.22f && ny < 0.78f;
                        fill |= ny > 0.68f && nx > 0.18f && nx < 0.82f;
                        fill &= !(ny > 0.38f && ny < 0.5f && nx > 0.34f && nx < 0.66f);
                        break;
                    case "helmet_shell":
                        fill = nx > 0.22f && nx < 0.78f && ny > 0.34f && ny < 0.84f;
                        fill &= !(ny > 0.34f && ny < 0.48f && nx > 0.34f && nx < 0.66f);
                        fill |= ny > 0.74f && nx > 0.18f && nx < 0.82f;
                        fill |= ny > 0.44f && ny < 0.54f && nx > 0.16f && nx < 0.26f;
                        break;
                    case "visor":
                        fill = ny > 0.42f && ny < 0.56f && nx > 0.22f && nx < 0.78f;
                        fill &= !(nx < 0.28f || nx > 0.72f);
                        break;
                    case "arm_modern":
                        fill = nx > 0.38f && nx < 0.62f && ny > 0.14f && ny < 0.88f;
                        fill |= ny > 0.68f && nx > 0.3f && nx < 0.7f;
                        fill |= ny > 0.12f && ny < 0.22f && nx > 0.28f && nx < 0.72f;
                        break;
                    case "leg_modern":
                        fill = nx > 0.4f && nx < 0.6f && ny > 0.14f && ny < 0.9f;
                        fill |= ny > 0.14f && ny < 0.24f && nx > 0.3f && nx < 0.7f;
                        fill |= ny > 0.76f && ny < 0.9f && nx > 0.26f && nx < 0.72f;
                        break;
                    case "limb":
                        fill = nx > 0.32f && nx < 0.68f && ny > 0.08f && ny < 0.92f;
                        fill |= ny > 0.74f && nx > 0.24f && nx < 0.76f;
                        break;
                    case "totem":
                        fill = nx > 0.34f && nx < 0.66f && ny > 0.12f && ny < 0.96f;
                        fill |= ny > 0.72f && nx > 0.18f && nx < 0.82f;
                        fill |= ny > 0.42f && ny < 0.52f && nx > 0.22f && nx < 0.78f;
                        break;
                    case "helmet":
                        fill = ny > 0.4f && ny < 0.82f && nx > 0.24f && nx < 0.76f;
                        fill &= !(ny > 0.44f && ny < 0.56f && nx > 0.38f && nx < 0.62f);
                        fill |= ny > 0.74f && nx > 0.34f && nx < 0.66f;
                        break;
                    case "crest":
                        fill = ny > 0.25f && nx > 0.5f - ny * 0.35f && nx < 0.5f + ny * 0.35f;
                        break;
                    case "feather":
                        float featherX = (nx - 0.5f) / 0.18f;
                        float featherY = (ny - 0.52f) / 0.38f;
                        fill = featherX * featherX + featherY * featherY < 1f && nx + ny > 0.62f;
                        break;
                    case "spear":
                        fill = nx > 0.46f && nx < 0.54f && ny > 0.08f && ny < 0.86f;
                        fill |= ny >= 0.86f && nx > 0.38f && nx < 0.62f && Mathf.Abs(nx - 0.5f) < (1f - ny) * 1.8f;
                        break;
                    case "club":
                        fill = nx > 0.46f && nx < 0.54f && ny > 0.08f && ny < 0.74f;
                        fill |= nx > 0.34f && nx < 0.66f && ny > 0.68f && ny < 0.92f;
                        break;
                    case "blade":
                        fill = nx > 0.46f && nx < 0.54f && ny > 0.1f && ny < 0.68f;
                        fill |= ny >= 0.68f && nx > 0.34f && nx < 0.66f && Mathf.Abs(nx - 0.5f) < (1f - ny) * 2.2f;
                        fill |= ny > 0.18f && ny < 0.28f && nx > 0.3f && nx < 0.7f;
                        break;
                    case "bow":
                        float dx = nx - 0.5f;
                        float dy = ny - 0.5f;
                        fill = Mathf.Abs(Mathf.Abs(dx) - 0.18f - dy * dy * 0.55f) < 0.05f && ny > 0.14f && ny < 0.86f;
                        break;
                    case "shield":
                        fill = ny > 0.18f && ny < 0.88f && nx > 0.18f && nx < 0.82f;
                        fill &= !(ny < 0.34f && (nx < 0.3f || nx > 0.7f));
                        fill &= !(ny > 0.78f && Mathf.Abs(nx - 0.5f) > 0.14f);
                        break;
                    case "dagger":
                        fill = nx > 0.47f && nx < 0.53f && ny > 0.14f && ny < 0.72f;
                        fill |= ny >= 0.72f && nx > 0.4f && nx < 0.6f && Mathf.Abs(nx - 0.5f) < (1f - ny) * 1.7f;
                        fill |= ny > 0.18f && ny < 0.24f && nx > 0.32f && nx < 0.68f;
                        break;
                    case "rifle":
                        fill = ny > 0.44f && ny < 0.58f && nx > 0.12f && nx < 0.86f;
                        fill |= ny > 0.56f && ny < 0.82f && nx > 0.28f && nx < 0.42f;
                        fill |= ny > 0.3f && ny < 0.44f && nx > 0.72f && nx < 0.88f;
                        fill |= ny > 0.38f && ny < 0.46f && nx > 0.06f && nx < 0.18f;
                        break;
                    case "carbine":
                        fill = ny > 0.44f && ny < 0.58f && nx > 0.18f && nx < 0.8f;
                        fill |= ny > 0.56f && ny < 0.78f && nx > 0.34f && nx < 0.46f;
                        fill |= ny > 0.32f && ny < 0.44f && nx > 0.64f && nx < 0.78f;
                        break;
                    case "launcher":
                        fill = ny > 0.4f && ny < 0.64f && nx > 0.14f && nx < 0.86f;
                        fill |= ny > 0.36f && ny < 0.68f && nx > 0.72f && nx < 0.92f;
                        fill |= ny > 0.56f && ny < 0.82f && nx > 0.3f && nx < 0.44f;
                        break;
                }

                if (fill)
                {
                    pixels[y * 32 + x] = white;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        proceduralSprites[key] = sprite;
        return sprite;
    }

    SpriteRenderer CreateAttachmentRenderer(string name, int orderOffset)
    {
        Transform existing = transform.Find(name);
        SpriteRenderer renderer = existing != null ? existing.GetComponent<SpriteRenderer>() : null;

        if (renderer == null)
        {
            GameObject attachment = new GameObject(name);
            attachment.transform.SetParent(transform, false);
            renderer = attachment.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = GetProceduralSprite("cape");
        renderer.sortingLayerID = spriteR.sortingLayerID;
        renderer.sortingOrder = spriteR.sortingOrder + orderOffset;
        return renderer;
    }

    void EnsureAttachmentRenderers()
    {
        if (spriteR == null) return;

        headRenderer = CreateAttachmentRenderer("VisualHead", 1);
        bodyRenderer = CreateAttachmentRenderer("VisualBodyPlate", 1);
        leftArmRenderer = CreateAttachmentRenderer("VisualArmL", 2);
        rightArmRenderer = CreateAttachmentRenderer("VisualArmR", 2);
        leftLegRenderer = CreateAttachmentRenderer("VisualLegL", 0);
        rightLegRenderer = CreateAttachmentRenderer("VisualLegR", 0);
        crestRenderer = CreateAttachmentRenderer("VisualCrest", 3);
        weaponRenderer = CreateAttachmentRenderer("VisualWeapon", 4);
    }

    void ConfigureAttachments(int age, int role, Color baseTint)
    {
        ConfigureHeadAttachment(age, role, baseTint);
        ConfigureBodyAttachment(age, role, baseTint);
        ConfigureLimbs(age, role, baseTint);
        ConfigureCrestAttachment(age, role, baseTint);
        ConfigureWeaponAttachment(age, role, baseTint);
        CacheAttachmentBaseTransforms();
    }

    void CacheAttachmentBaseTransforms()
    {
        if (headRenderer != null) headBasePosition = headRenderer.transform.localPosition;
        if (bodyRenderer != null) bodyBasePosition = bodyRenderer.transform.localPosition;
        if (leftArmRenderer != null) leftArmBasePosition = leftArmRenderer.transform.localPosition;
        if (rightArmRenderer != null) rightArmBasePosition = rightArmRenderer.transform.localPosition;
        if (leftLegRenderer != null) leftLegBasePosition = leftLegRenderer.transform.localPosition;
        if (rightLegRenderer != null) rightLegBasePosition = rightLegRenderer.transform.localPosition;
        if (crestRenderer != null) crestBasePosition = crestRenderer.transform.localPosition;
        if (weaponRenderer != null) weaponBasePosition = weaponRenderer.transform.localPosition;
    }

    void ConfigureHeadAttachment(int age, int role, Color baseTint)
    {
        if (headRenderer == null) return;

        Color[] skinPalette =
        {
            new Color(0.93f, 0.79f, 0.68f, 1f),
            new Color(0.83f, 0.68f, 0.56f, 1f),
            new Color(0.72f, 0.56f, 0.45f, 1f)
        };

        headRenderer.sprite = GetProceduralSprite("modern_face");
        headRenderer.transform.localPosition = new Vector3(0f, 0.225f, 0f);
        headRenderer.transform.localScale = new Vector3(0.14f + role * 0.008f, 0.15f + role * 0.008f, 1f);
        headRenderer.color = skinPalette[(age + role) % skinPalette.Length];
        headRenderer.enabled = true;
    }

    void ConfigureBodyAttachment(int age, int role, Color baseTint)
    {
        if (bodyRenderer == null) return;

        bodyRenderer.sprite = GetProceduralSprite("armor");
        Vector3 position = new Vector3(0f, 0.02f, 0f);
        Vector3 scale = new Vector3(0.2f, 0.28f, 1f);
        if (role == 1) scale = new Vector3(0.19f, 0.27f, 1f);
        if (role == 2) scale = new Vector3(0.22f, 0.3f, 1f);
        if (role == 3) scale = new Vector3(0.25f, 0.34f, 1f);

        bodyRenderer.transform.localPosition = position;
        bodyRenderer.transform.localScale = scale;
        bodyRenderer.color = Color.Lerp(baseTint, new Color(0.14f, 0.16f, 0.18f, 1f), 0.5f + age * 0.03f);
        bodyRenderer.enabled = true;
    }

    void ConfigureLimbs(int age, int role, Color baseTint)
    {
        Sprite armSprite = GetProceduralSprite("arm_modern");
        Sprite legSprite = GetProceduralSprite("leg_modern");
        Color sleeveColor = Color.Lerp(baseTint, new Color(0.2f, 0.24f, 0.28f, 1f), 0.22f);
        Color trouserColor = Color.Lerp(baseTint, new Color(0.16f, 0.18f, 0.2f, 1f), 0.34f);

        if (leftArmRenderer != null)
        {
            leftArmRenderer.sprite = armSprite;
            leftArmRenderer.transform.localPosition = new Vector3(-0.11f, 0.035f, 0f);
            leftArmRenderer.transform.localScale = new Vector3(0.07f, 0.18f + role * 0.01f, 1f);
            leftArmRenderer.color = sleeveColor;
            leftArmRenderer.enabled = true;
        }

        if (rightArmRenderer != null)
        {
            rightArmRenderer.sprite = armSprite;
            rightArmRenderer.transform.localPosition = new Vector3(0.11f, 0.035f, 0f);
            rightArmRenderer.transform.localScale = new Vector3(0.07f, 0.18f + role * 0.01f, 1f);
            rightArmRenderer.color = sleeveColor;
            rightArmRenderer.enabled = true;
        }

        if (leftLegRenderer != null)
        {
            leftLegRenderer.sprite = legSprite;
            leftLegRenderer.transform.localPosition = new Vector3(-0.05f, -0.145f, 0f);
            leftLegRenderer.transform.localScale = new Vector3(0.07f, 0.22f + age * 0.008f, 1f);
            leftLegRenderer.color = trouserColor;
            leftLegRenderer.enabled = true;
        }

        if (rightLegRenderer != null)
        {
            rightLegRenderer.sprite = legSprite;
            rightLegRenderer.transform.localPosition = new Vector3(0.05f, -0.145f, 0f);
            rightLegRenderer.transform.localScale = new Vector3(0.07f, 0.22f + age * 0.008f, 1f);
            rightLegRenderer.color = trouserColor;
            rightLegRenderer.enabled = true;
        }
    }

    void ConfigureCrestAttachment(int age, int role, Color baseTint)
    {
        if (crestRenderer == null) return;

        crestRenderer.enabled = true;
        crestRenderer.sprite = GetProceduralSprite("helmet_shell");

        Vector3 position = new Vector3(0f, 0.245f, 0f);
        Vector3 scale = new Vector3(0.18f, 0.14f, 1f);

        if (role == 1)
        {
            position = new Vector3(0.004f, 0.245f, 0f);
            scale = new Vector3(0.19f, 0.145f, 1f);
        }
        else if (role == 2)
        {
            position = new Vector3(0.006f, 0.25f, 0f);
            scale = new Vector3(0.2f, 0.15f, 1f);
        }
        else if (role == 3)
        {
            position = new Vector3(0.008f, 0.255f, 0f);
            scale = new Vector3(0.215f, 0.16f, 1f);
        }

        crestRenderer.transform.localPosition = position;
        crestRenderer.transform.localScale = scale;
        crestRenderer.color = Color.Lerp(baseTint, new Color(0.11f, 0.14f, 0.17f, 1f), 0.72f);
    }

    void ConfigureWeaponAttachment(int age, int role, Color baseTint)
    {
        if (weaponRenderer == null) return;

        weaponRenderer.enabled = true;
        float side = isPlayer ? 1f : -1f;

        switch (role)
        {
            case 0:
                weaponRenderer.sprite = GetProceduralSprite("carbine");
                weaponRenderer.transform.localPosition = new Vector3(0.15f * side, 0.02f, 0f);
                weaponRenderer.transform.localScale = new Vector3(0.24f, 0.11f, 1f);
                break;
            case 1:
                weaponRenderer.sprite = GetProceduralSprite("rifle");
                weaponRenderer.transform.localPosition = new Vector3(0.16f * side, 0.025f, 0f);
                weaponRenderer.transform.localScale = new Vector3(0.28f, 0.11f, 1f);
                break;
            case 2:
                weaponRenderer.sprite = GetProceduralSprite(age >= 2 ? "rifle" : "carbine");
                weaponRenderer.transform.localPosition = new Vector3(0.16f * side, 0.03f, 0f);
                weaponRenderer.transform.localScale = new Vector3(0.27f, 0.11f, 1f);
                break;
            default:
                weaponRenderer.sprite = GetProceduralSprite("launcher");
                weaponRenderer.transform.localPosition = new Vector3(0.17f * side, 0.04f, 0f);
                weaponRenderer.transform.localScale = new Vector3(0.31f, 0.14f, 1f);
                break;
        }

        weaponRenderer.color = Color.Lerp(baseTint, new Color(0.11f, 0.12f, 0.14f, 1f), 0.68f);
        weaponRenderer.flipX = !isPlayer;
    }

    void UpdateAnimation()
    {
        float t = Time.time * 5f + animationSeed;
        float step = Mathf.Sin(t);
        float stepAbs = Mathf.Abs(step);
        float moveBlend = is_moving ? 1f : 0.08f;
        float attackBlend = (attacking_melee || attacking_range) ? 1f : 0f;
        float bob = stepAbs * 0.018f * moveBlend + Mathf.Sin(t * 0.5f) * 0.003f;
        float lean = step * 1.2f * moveBlend;

        if (attackBlend > 0f)
        {
            float jab = Mathf.Sin(Time.time * 12f + animationSeed);
            bob += Mathf.Abs(jab) * 0.012f;
            lean += (isPlayer ? -1f : 1f) * jab * 2.2f;
        }

        Vector3 localPos = transform.localPosition;
        localPos.y = laneLocalY + bob;
        transform.localPosition = localPos;

        float side = isPlayer ? 1f : -1f;

        if (bodyRenderer != null)
        {
            bodyRenderer.transform.localPosition = bodyBasePosition + new Vector3(0f, stepAbs * 0.006f, 0f);
            bodyRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, lean);
        }

        if (headRenderer != null)
        {
            headRenderer.transform.localPosition = headBasePosition + new Vector3(0f, stepAbs * 0.004f, 0f);
            headRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, lean * 0.35f);
        }

        if (leftArmRenderer != null)
        {
            float armSwing = is_moving ? step * 7f : Mathf.Sin(t * 0.6f) * 1.5f;
            if (attackBlend > 0f) armSwing = 9f * Mathf.Sin(Time.time * 11f + animationSeed);
            leftArmRenderer.transform.localPosition = leftArmBasePosition + new Vector3(0f, stepAbs * 0.004f, 0f);
            leftArmRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, armSwing);
        }

        if (rightArmRenderer != null)
        {
            float armSwing = is_moving ? -step * 7f : -Mathf.Sin(t * 0.6f) * 1.5f;
            if (attackBlend > 0f) armSwing = -7f * Mathf.Sin(Time.time * 11f + animationSeed);
            rightArmRenderer.transform.localPosition = rightArmBasePosition + new Vector3(0f, stepAbs * 0.004f, 0f);
            rightArmRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, armSwing);
        }

        if (leftLegRenderer != null)
        {
            float legSwing = is_moving ? -step * 10f : 0f;
            leftLegRenderer.transform.localPosition = leftLegBasePosition + new Vector3(0f, Mathf.Max(0f, -step) * 0.008f, 0f);
            leftLegRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, legSwing);
        }

        if (rightLegRenderer != null)
        {
            float legSwing = is_moving ? step * 10f : 0f;
            rightLegRenderer.transform.localPosition = rightLegBasePosition + new Vector3(0f, Mathf.Max(0f, step) * 0.008f, 0f);
            rightLegRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, legSwing);
        }

        if (crestRenderer != null)
        {
            crestRenderer.transform.localPosition = crestBasePosition + new Vector3(0f, stepAbs * 0.004f, 0f);
            crestRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, lean * 0.5f);
        }

        if (weaponRenderer != null)
        {
            float swing = is_moving ? step * 4f : Mathf.Sin(Time.time * 2f + animationSeed) * 1.2f;
            if (attackBlend > 0f)
            {
                swing = 8f * Mathf.Sin(Time.time * 10f + animationSeed);
            }

            weaponRenderer.transform.localPosition = weaponBasePosition + new Vector3(0.008f * side * step, stepAbs * 0.004f, 0f);
            weaponRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, swing * side);
        }
    }

    void check_moving()
    {
        is_moving = true;
        //
        if (isPlayer)
        {
            // Дошли до базы врага
            if (gameObject.transform.localPosition.x >= 9f)
            {
                is_moving = false;
            }
            // Есть враги — проверяем дистанцию до ближайшего
            if (game_manager.enemy_troops_queue.Count > 0)
            {
                GameObject first_enemy = game_manager.enemy_troops_queue[0];
                if (first_enemy != null)
                {
                    additonal.text = "" + (first_enemy.transform.position.x - gameObject.transform.position.x) * data.COEFF + "   " + (first_enemy.transform.position.x - gameObject.transform.position.x);
                    if ((first_enemy.transform.position.x - gameObject.transform.position.x) * data.COEFF <= data.MIN_DISTANCE)
                    {
                        is_moving = false;
                    }
                }
            }
        }
        else
        {
            // Дошли до базы игрока
            if (gameObject.transform.localPosition.x <= -9f)
            {
                is_moving = false;
            }
            // Есть враги — проверяем дистанцию
            if (game_manager.player_troops_queue.Count > 0)
            {
                GameObject first_enemy = game_manager.player_troops_queue[0];
                if (first_enemy != null)
                {
                    if ((gameObject.transform.position.x - first_enemy.transform.position.x) * data.COEFF <= data.MIN_DISTANCE)
                    {
                        is_moving = false;
                    }
                }
            }
        }
    }

    void check_attacking()
    {
        attacking_melee = false;
        attacking_range = false;
        if (isPlayer)
        {
            if (game_manager.enemy_troops_queue.Count > 0)
            {
                // if there are enemies
                GameObject first_enemy = game_manager.enemy_troops_queue[0];
                
                if ((first_enemy.transform.position.x - gameObject.transform.position.x) * data.COEFF - data.MIN_DISTANCE <= troop_data.range_ranged)
                {
                    attacking_range = true;
                    attacked_gm = first_enemy;
                }
                if ((first_enemy.transform.position.x - gameObject.transform.position.x) * data.COEFF <= troop_data.range_melee)
                {
                    attacking_melee = true;
                    attacked_gm = first_enemy;
                }
                
            }
            else
            {
                //checking base attacking
                if ((9f - gameObject.transform.localPosition.x) * data.COEFF <= troop_data.range_ranged)
                {
                    attacking_range = true;
                }
                if ((9f - gameObject.transform.localPosition.x) * data.COEFF <= troop_data.range_melee)
                {
                    attacking_melee = true;
                }
            }
        }
        else
        {
            if (game_manager.player_troops_queue.Count > 0)
            {
                // if there are enemies
                GameObject first_enemy = game_manager.player_troops_queue[0];

                if ((gameObject.transform.position.x - first_enemy.transform.position.x ) * data.COEFF - data.MIN_DISTANCE <= troop_data.range_ranged)
                {
                    attacking_range = true;
                    attacked_gm = first_enemy;
                }
                if ((gameObject.transform.position.x - first_enemy.transform.position.x) * data.COEFF <= troop_data.range_melee)
                {
                    attacking_melee = true;
                    attacked_gm = first_enemy;
                }

            }
            else
            {
                //checking base attacking
                if ((gameObject.transform.localPosition.x + 9f) * data.COEFF <= troop_data.range_ranged)
                {
                    attacking_range = true;
                }
                if ((gameObject.transform.localPosition.x + 9f) * data.COEFF <= troop_data.range_melee)
                {
                    attacking_melee = true;
                }
            }
        }
    }
    
    void try_moving()
    {
        if (is_moving)
        {
            float speed = troop_data.speed / data.COEFF;
            if (!isPlayer)
            {
                speed *= -1;
            }
            gameObject.transform.position += new Vector3(speed, 0f, 0f) * Time.deltaTime;
        }
    }

    void try_attacking()
    {
        if (attacking_melee)
        {
            if (!melee_routine)
            {
                //an attacking routine is not already running 
                inst_melee = StartCoroutine(attack_melee(troop_data.melee_first_speed));
            }
        }
        else
        {
            if (attacking_range)
            {
                if (!range_routine)
                {
                    inst_range = StartCoroutine(attack_range(troop_data.ranged_first_speed));
                }
            }
           
        }
    }


    void try_dying()
    {
        if (troop_data.health <= 0)
        {

            //print("troop died");
            if (isPlayer)
            {
                if (troop_data.id == 15)
                {
                    game_manager.player_troops[3] -= 1;
                }
                else
                {
                    game_manager.player_troops[troop_data.id % 3] -= 1;
                }
                game_manager.player_troops_queue.Remove(gameObject);
                int reward = Mathf.RoundToInt(1.3f * troop_data.cost);
                
                game_manager.xp += reward / 2;
                
                

                //print("new count player" + game_manager.player_troops_queue.Count);
            }
            else
            {
                game_manager.enemy_troops[troop_data.id % 3] -= 1;
                game_manager.enemy_troops_queue.Remove(gameObject);
                int reward = Mathf.RoundToInt(1.3f * troop_data.cost);
                game_manager.money += reward;
                game_manager.xp += reward * 2;
                
                //print("new count enemies" + game_manager.enemy_troops_queue.Count);
            }
            if (prev_troop != null)
            {
                Troop prev_tr = prev_troop.GetComponent<Troop>();
                prev_tr.next_troop = next_troop;
            }
            if (next_troop != null && prev_troop != null)
            {
                Troop next_tr = next_troop.GetComponent<Troop>();
                next_tr.prev_troop = prev_troop;
            }
            Destroy(gameObject);
            
        }
    }



    void give_damage(int damage)
    {
        if (attacking_range || attacking_melee)
        {
            if (isPlayer)
            {
                //gives damage to first enemy or enemy base
                if (game_manager.enemy_troops_queue.Count > 0)
                {
                    //there is a troop alive
                    Troop enemy_script = game_manager.enemy_troops_queue[0].GetComponent<Troop>();
                    enemy_script.troop_data.health -= damage;
                    if (enemy_script.troop_data.health <= 0)
                    {
                        attacking_melee = false;
                        is_moving = true;
                        attacking_range = false;
                    }
                }
                else
                {
                    //damages enemy base
                    Base b = game_manager.enemy_base.GetComponent<Base>();
                    b.take_damage(damage);
                }
            }
            else
            {
                //same as above but for the enemy
                if (game_manager.player_troops_queue.Count > 0)
                {
                    //there is a troop alive
                    Troop enemy_script = game_manager.player_troops_queue[0].GetComponent<Troop>();
                    enemy_script.troop_data.health -= damage;
                    if (enemy_script.troop_data.health <= 0)
                    {
                        attacking_melee = false;
                        is_moving = true;
                        attacking_range = false;
                    }
                }
                else
                {
                    //damages enemy base
                    Base b = game_manager.player_base.GetComponent<Base>();
                    b.take_damage(damage);
                }
            }
        }
    }

    IEnumerator attack_melee(float cooldown)
    {
        melee_routine = true;
        yield return new WaitForSeconds(cooldown);
        if (!gameObject.activeInHierarchy) { melee_routine = false; yield break; }
        give_damage(troop_data.melee_damage);
        if (attacking_melee)
        {
            StartCoroutine(attack_melee(troop_data.melee_speed + troop_data.attack_pause / data.FPS));
        }
        else
        {
            melee_routine = false;
        }
    }

    IEnumerator attack_range(float cooldown)
    {
        range_routine = true;
        yield return new WaitForSeconds(cooldown);
        if (!gameObject.activeInHierarchy) { range_routine = false; yield break; }
        if (!attacking_melee)
        {
            give_damage(troop_data.ranged_damage);
        }
        if (attacking_range)
        {
            if (!is_moving)
            {
                StartCoroutine(attack_range(troop_data.ranged_standing_speed + troop_data.attack_pause / data.FPS));
            }
            else
            {
                StartCoroutine(attack_range(troop_data.ranged_walking_speed + troop_data.attack_pause / data.FPS));
            }
        }
        else
        {
            range_routine = false;
        }
    }

    void Start()
    {
        data = game_manager.data_object;
        max_health = troop_data.health;
        laneLocalY = transform.localPosition.y;
        animationSeed = (transform.position.x * 0.73f) + troop_data.id * 0.41f;

        if (!isPlayer && !info)
        {
            gameObject.transform.localScale = new Vector3(-gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
        }
        
        if (!info)
        {
            local_canvas.SetActive(false);
        }
        else
        {
            local_canvas.SetActive(true);
        }
        if (troop_data.id == 2 || troop_data.id == 5)
        {
            Box.size = new Vector2(0.7f, 0.5f);
        }
        if (troop_data.id == 11 || troop_data.id == 14)
        {
            Box.size = new Vector2(1.05f, 0.5f);
        }
        manage_sprites();
        ApplyVisualStyle();
        StartCoroutine(Regenerate());
    }
    // Update is called once per frame
    void Update()
    {

        if (info)
        {
            manage_texts();
        }
        check_moving();
        check_attacking();
        try_moving();
        try_attacking();
        try_dying();
        UpdateAnimation();
        UpdateDepthSorting();
    }

    /// <summary>
    /// 2.5D глубина: юниты ниже на экране — ближе к камере (крупнее, поверх)
    /// </summary>
    void UpdateDepthSorting()
    {
        // Сортировка: чем ниже Y, тем больше sortingOrder (рисуется поверх)
        if (spriteR != null)
        {
            spriteR.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100f);
        }

        if (silhouetteRenderer != null)
        {
            silhouetteRenderer.sortingOrder = spriteR.sortingOrder - 1;
        }

        if (bodyRenderer != null)
        {
            bodyRenderer.sortingOrder = spriteR.sortingOrder + 1;
        }

        if (headRenderer != null)
        {
            headRenderer.sortingOrder = spriteR.sortingOrder + 2;
        }

        if (leftArmRenderer != null)
        {
            leftArmRenderer.sortingOrder = spriteR.sortingOrder + 2;
        }

        if (rightArmRenderer != null)
        {
            rightArmRenderer.sortingOrder = spriteR.sortingOrder + 2;
        }

        if (crestRenderer != null)
        {
            crestRenderer.sortingOrder = spriteR.sortingOrder + 3;
        }

        if (leftLegRenderer != null)
        {
            leftLegRenderer.sortingOrder = spriteR.sortingOrder;
        }

        if (rightLegRenderer != null)
        {
            rightLegRenderer.sortingOrder = spriteR.sortingOrder;
        }

        if (weaponRenderer != null)
        {
            weaponRenderer.sortingOrder = spriteR.sortingOrder + 4;
        }

        // Масштаб по глубине: ниже = крупнее (ближе к камере)
        float baseScale = 1f;
        float depthFactor = Mathf.InverseLerp(-2.5f, -3.3f, transform.localPosition.y);
        float scale = Mathf.Lerp(baseScale * 1.05f, baseScale * 1.35f, depthFactor) * visualScaleMultiplier;

        float xSign = isPlayer ? 1f : -1f;
        if (!info) // В тестах модель не переворачивается
        {
            transform.localScale = new Vector3(xSign * scale, scale, 1f);
        }
    }

    IEnumerator Regenerate()
    {
        yield return new WaitForSeconds((1/data.FPS));
        if(troop_data.health < max_health && is_regenerating)
        {
            troop_data.health += 1;
        }
        StartCoroutine(Regenerate());
    }
}
