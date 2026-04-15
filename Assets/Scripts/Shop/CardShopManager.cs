using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CardSystem;

public class CardShopManager : MonoBehaviour
{
    private const string LOG_PREFIX = "[CardShopSpawner]";

    [Header("Pool (assign in inspector)")]
    [SerializeField] private List<CardAbilityDefinition> _pool;

    [Header("Auto Spawn Settings")]
    [SerializeField] private bool _spawnOnStart = true;
    [SerializeField] private int _initialSpawnCount = 5;

    [Header("Spawn")]
    [SerializeField] private Transform _spawnParent;
    [SerializeField] private Vector3 _localOffset = Vector3.zero;

    [Header("Layout (fan settings)")]
    [SerializeField] private float _fanWidth = 15f;
    [SerializeField] private float _arcHeight = 0f;
    [SerializeField] private float _maxTilt = 0f;

    [Header("Refresh Settings")]
    [SerializeField] private int _refreshCost = 10;

    private readonly List<GameObject> activeSpawnedCards = new List<GameObject>();

    public static CardShopManager Instance { get; private set; }

    private void Awake()
    {
        _pool = Resources.Load<CardAndPackLibrary>("Libraries/CardAndPackLibrary").GetCardsInProject;

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(LOG_PREFIX + " Multiple CardShopSpawner instances found. Keeping the first one.");
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (_spawnOnStart && _initialSpawnCount > 0)
            SpawnMultiple(_initialSpawnCount);
    }

    public void SpawnMultiple(int count)
    {
        Random.InitState(PlayerDataManager.Instance.GetGeneralSeed);

        for (int i = 0; i < count; i++)
            SpawnRandomCard();

        ArrangeSpawnedCards(activeSpawnedCards);
    }

    public void SpawnRandomCard()
    {
        var entry = PickRandomEntry();
        if (entry == null) return;

        GameObject prefab = Resources.Load<GameObject>("NewCardPrefab");
        if (prefab == null) return;

        Transform parent = _spawnParent != null ? _spawnParent : transform;

        GameObject cardGO = Instantiate(prefab, parent);
        cardGO.transform.localPosition = Vector3.zero;

        Card card = new Card(entry, entry.GetBaseCardRarity, cardGO.transform);
        CardPrefabSetterUpper.SetupCardPrefab(card, CardState.Shop);

        if (!cardGO.TryGetComponent(out CardSelect cs))
            cs = cardGO.AddComponent<CardSelect>();

        if (!cardGO.TryGetComponent(out CardFunctionScript cfs))
            cfs = cardGO.AddComponent<CardFunctionScript>();

     
        CreatePriceText(entry, cardGO);

        activeSpawnedCards.Add(cardGO);
    }


    private void CreatePriceText(CardAbilityDefinition entry, GameObject cardGO)
    {
        if (entry == null || cardGO == null) return;

        GameObject textGO = new GameObject("PriceText");
        textGO.transform.SetParent(cardGO.transform, false);

        // MUST be UI component (NOT world TMP)
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();

        tmp.text = $"${entry.GetShopCost}";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24; // UI size (NOT 5 or 6)
        tmp.color = Color.yellow;

        RectTransform rt = tmp.rectTransform;

        // Force it BELOW center of card
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 1f);

        rt.anchoredPosition = new Vector2(0f, 40f);

        // IMPORTANT: prevent layout groups from overriding it
        rt.localScale = Vector3.one;
    }


    public void ArrangeSpawnedCards(List<GameObject> spawnedCards)
    {
        if (spawnedCards == null || spawnedCards.Count == 0)
            return;

        int count = spawnedCards.Count;

        if (count == 1)
        {
            var single = spawnedCards[0];
            if (single != null)
            {
                single.transform.localPosition = _localOffset;
                single.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            return;
        }

        float span = Mathf.Max(0.001f, _fanWidth);

        for (int i = 0; i < count; i++)
        {
            var go = spawnedCards[i];
            if (go == null) continue;

            float t = i / (float)(count - 1);

            float x = -span * 0.5f + t * span;
            float y = -4f * _arcHeight * Mathf.Pow(t - 0.5f, 2f) + _arcHeight;
            float tilt = Mathf.Lerp(-_maxTilt, _maxTilt, t);

            go.transform.localPosition = new Vector3(x, y, 0f);
            go.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
        }
    }

    public void DeleteCard(GameObject cardGO)
    {
        if (cardGO == null) return;

        if (activeSpawnedCards.Remove(cardGO))
        {
            Destroy(cardGO);
            ArrangeSpawnedCards(activeSpawnedCards);
        }
        else
        {
            Destroy(cardGO);
        }
    }

    public void RefreshShop(int count = -1)
    {
        if (_refreshCost > 0)
        {
            if (CurrencyManager.Instance == null)
            {
                Debug.LogWarning(LOG_PREFIX + " CurrencyManager not found; cannot charge refresh cost.");
                return;
            }

            bool charged = CurrencyManager.Instance.TrySpend(_refreshCost);
            if (!charged)
            {
                Debug.Log(LOG_PREFIX + " Not enough currency to refresh shop.");
                return;
            }
        }

        PlayerDataManager.Instance.GenerateGeneralSeed();

        for (int i = activeSpawnedCards.Count - 1; i >= 0; i--)
        {
            var go = activeSpawnedCards[i];
            if (go != null) Destroy(go);
        }

        activeSpawnedCards.Clear();

        int spawnCount = (count <= 0) ? _initialSpawnCount : count;

        if (spawnCount > 0)
            SpawnMultiple(spawnCount);
    }

    private CardAbilityDefinition PickRandomEntry()
    {
        if (_pool == null || _pool.Count == 0) return null;

        float total = 0f;
        foreach (var e in _pool)
            total += Mathf.Max(0f, e.GetShopWeight);

        if (total <= 0f) return _pool[0];

        float r = UnityEngine.Random.Range(0f, total);
        float acc = 0f;

        foreach (var e in _pool)
        {
            acc += Mathf.Max(0f, e.GetShopWeight);
            if (r <= acc) return e;
        }

        return _pool[_pool.Count - 1];
    }
}