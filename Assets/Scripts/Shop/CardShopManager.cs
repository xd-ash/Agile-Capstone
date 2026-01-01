using System.Collections.Generic;
using UnityEngine;
using CardSystem;

public class CardShopManager : MonoBehaviour
{
    private const string LOG_PREFIX = "[CardShopSpawner]";

    /*[System.Serializable]
    public struct ShopEntry
    {
        public CardSystem.CardAbilityDefinition definition;
        [Tooltip("Higher weight => more likely to be chosen")]
        public float weight;
        [Tooltip("Cost to buy this card in the shop")]
        public int cost;
    }*/

    [Header("Pool (assign in inspector)")]
    //public List<ShopEntry> pool = new List<ShopEntry>();
    public Deck pool;

    [Header("Auto Spawn Settings")]
    [Tooltip("If true, the spawner will populate the shop on scene start")]
    public bool spawnOnStart = true;
    [Tooltip("How many cards to spawn when the scene starts")]
    public int initialSpawnCount = 3;

    [Header("Spawn")]
    public Transform spawnParent; // parent for spawned card GOs (optional)
    public Vector3 localOffset = Vector3.zero;

    //[Header("Shop Settings")]
    //[Tooltip("Optional collider for the buy/drop area. If null, the object tagged 'BuyArea' will be used.")]
    //public Collider2D buyAreaCollider;
    //public string buyAreaTag = "BuyArea";

    [Header("Layout (fan settings)")]
    [Tooltip("Total horizontal span of the fan in local units")]
    public float fanWidth = 3f;
    [Tooltip("Vertical height of the fan (peak at center)")]
    public float arcHeight = 0.6f;
    [Tooltip("Max card tilt (degrees) at the edges")]
    public float maxTilt = 15f;

    [Header("Refresh Settings")]
    [Tooltip("Cost to refresh the shop (0 = free)")]
    public int refreshCost = 10;

    // runtime tracking of active spawned shop cards
    private readonly List<GameObject> activeSpawnedCards = new List<GameObject>();

    // singleton instance for easy access from other components (e.g. CardSelect when a card is bought)
    public static CardShopManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(LOG_PREFIX + " Multiple CardShopSpawner instances found. Keeping the first one.");
            return;
        }
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (spawnOnStart && initialSpawnCount > 0)
            SpawnMultiple(initialSpawnCount);
    }

    // Spawns a single card chosen from `pool` using weighted random selection.
    public void SpawnRandomCard()
    {
        var entry = PickRandomEntry();
        if (entry == null) return;

        // Create runtime Card data
        Card card = new Card(entry);
        // store shop cost on the runtime card so UI/logic can access it
        card.ShopCost = entry.GetShopCost;

        // Use same prefab path as CardManager
        GameObject prefab = Resources.Load<GameObject>("CardTestPrefab");
        if (prefab == null) return;

        Transform parent = spawnParent != null ? spawnParent : transform;
        if (parent == null) return;

        GameObject cardGO = Instantiate(prefab, parent);
        cardGO.transform.localPosition = localOffset;

        // Ensure the prefab has CardSelect and initialize it
        if (!cardGO.TryGetComponent(out CardSelect cs))
            cs = cardGO.AddComponent<CardSelect>();
        cs.OnPrefabCreation(card);

        // Enable shop behaviour on the card's CardSelect
        cs.EnableShopMode();

        // track in active list for later deletion / refresh / layout
        activeSpawnedCards.Add(cardGO);
    }

    // Convenience: spawn `count` cards (call this multiple times to populate shop)
    public void SpawnMultiple(int count)
    {
        for (int i = 0; i < count; i++)
            SpawnRandomCard();

        // arrange all active cards (new + existing)
        ArrangeSpawnedCards(activeSpawnedCards);
    }

    // Arrange spawned cards in a centered fan (local coordinates relative to parent)
    public void ArrangeSpawnedCards(List<GameObject> spawnedCards)
    {
        if (spawnedCards == null || spawnedCards.Count == 0)
            return;

        int count = spawnedCards.Count;

        // Single card -> center
        if (count == 1)
        {
            var single = spawnedCards[0];
            if (single != null)
            {
                single.transform.localPosition = localOffset;
                single.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            return;
        }

        float span = Mathf.Max(0.001f, fanWidth);
        for (int i = 0; i < count; i++)
        {
            var go = spawnedCards[i];
            if (go == null) continue;

            float t = (count == 1) ? 0.5f : (i / (float)(count - 1)); // 0..1
            // x: evenly spaced across span centered at 0
            float x = -span * 0.5f + t * span;
            // y: parabola peak at center -> gives a nice "hand" arc
            float y = -4f * arcHeight * Mathf.Pow(t - 0.5f, 2f) + arcHeight;
            // rotation z: tilt across the fan
            float tilt = Mathf.Lerp(-maxTilt, maxTilt, t);

            Vector3 localPos = new Vector3(localOffset.x + x, localOffset.y + y, localOffset.z);
            Quaternion localRot = Quaternion.Euler(0f, 0f, tilt);

            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
        }
    }

    // Called to remove a specific card GameObject from the shop (e.g. after purchase).
    // If the card belongs to the shop, it is removed from tracking and destroyed. Layout is updated.
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
            // Not in tracked list - still destroy if desired
            Destroy(cardGO);
        }
    }

    // Remove all current shop options and spawn `count` new ones (uses initialSpawnCount if count <= 0)
    public void RefreshShop(int count = -1)
    {
        // Check refresh cost first (0 or negative means free)
        if (refreshCost > 0)
        {
            if (CurrencyManager.instance == null)
            {
                Debug.LogWarning(LOG_PREFIX + " CurrencyManager not found; cannot charge refresh cost.");
                return;
            }

            // TrySpend will deduct the amount if player has enough; returns false if insufficient funds
            bool charged = CurrencyManager.instance.TrySpend(refreshCost);
            if (!charged)
            {
                Debug.Log(LOG_PREFIX + " Not enough currency to refresh shop.");
                // Optionally show a UI popup here if you have one for insufficient currency:
                // OutOfApPopup.Instance?.Show(); // or create/replace with an OutOfCurrency popup
                return;
            }
        }

        // destroy existing cards
        for (int i = activeSpawnedCards.Count - 1; i >= 0; i--)
        {
            var go = activeSpawnedCards[i];
            if (go != null) Destroy(go);
        }
        activeSpawnedCards.Clear();

        // default to initialSpawnCount if caller didn't pass a count
        int spawnCount = (count <= 0) ? initialSpawnCount : count;
        if (spawnCount > 0)
        {
            SpawnMultiple(spawnCount);
        }
    }

    // Weighted random pick from `pool` returning the full ShopEntry
    private CardAbilityDefinition PickRandomEntry()
    {
        //ShopEntry defaultEntry = default;
        if (pool == null || pool.GetDeck.Length == 0) return null;
        var poolDeck = pool.GetDeck;

        float total = 0f;
        foreach (var e in poolDeck) total += Mathf.Max(0f, e.GetShopWeight);

        if (total <= 0f) return poolDeck[0];

        float r = UnityEngine.Random.Range(0f, total);
        float acc = 0f;
        foreach (var e in poolDeck)
        {
            acc += Mathf.Max(0f, e.GetShopWeight);
            if (r <= acc) return e;
        }

        return poolDeck[poolDeck.Length - 1];
    }
}
