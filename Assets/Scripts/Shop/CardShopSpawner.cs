using System.Collections.Generic;
using UnityEngine;
using CardSystem;

public class CardShopSpawner : MonoBehaviour
{
    private const string LOG_PREFIX = "[CardShopSpawner]";

    [System.Serializable]
    public struct ShopEntry
    {
        public CardSystem.CardAbilityDefinition definition;
        [Tooltip("Higher weight => more likely to be chosen")]
        public float weight;
        [Tooltip("Cost to buy this card in the shop")]
        public int cost;
    }

    [Header("Pool (assign in inspector)")]
    public List<ShopEntry> pool = new List<ShopEntry>();

    [Header("Auto Spawn Settings")]
    [Tooltip("If true, the spawner will populate the shop on scene start")]
    public bool spawnOnStart = true;
    [Tooltip("How many cards to spawn when the scene starts")]
    public int initialSpawnCount = 3;

    [Header("Spawn")]
    public Transform spawnParent; // parent for spawned card GOs (optional)
    public Vector3 localOffset = Vector3.zero;

    [Header("Shop Settings")]
    [Tooltip("Optional collider for the buy/drop area. If null, the object tagged 'BuyArea' will be used.")]
    public Collider2D buyAreaCollider;
    public string buyAreaTag = "BuyArea";

    [Header("Layout (fan settings)")]
    [Tooltip("Total horizontal span of the fan in local units")]
    public float fanWidth = 3f;
    [Tooltip("Vertical height of the fan (peak at center)")]
    public float arcHeight = 0.6f;
    [Tooltip("Max card tilt (degrees) at the edges")]
    public float maxTilt = 15f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (spawnOnStart && initialSpawnCount > 0)
        {
            SpawnMultiple(initialSpawnCount);
        }
    }

    // Spawns a single card chosen from `pool` using weighted random selection.
    public GameObject SpawnRandomCard()
    {
        var entry = PickRandomEntry();
        if (entry.definition == null)
        {
            return null;
        }

        // Create runtime Card data
        Card card = new Card(entry.definition);
        // store shop cost on the runtime card so UI/logic can access it
        card.ShopCost = entry.cost;

        // Use same prefab path as CardManager
        GameObject prefab = Resources.Load<GameObject>("CardTestPrefab");
        if (prefab == null)
        {
            return null;
        }

        Transform parent = spawnParent != null ? spawnParent : transform;
        GameObject cardGO = Instantiate(prefab, parent);
        cardGO.transform.localPosition = localOffset;

        // Ensure the prefab has CardSelect and initialize it
        var cs = cardGO.GetComponent<CardSelect>();
        if (cs == null) cs = cardGO.AddComponent<CardSelect>();
        cs.OnPrefabCreation(card);

        // Enable shop behaviour (cost + buy-area) on the card's CardSelect
        cs.EnableShopMode(entry.cost, buyAreaCollider, buyAreaTag);

        // Track runtime transform on the Card data
        card.CardTransform = cardGO.transform;

        return cardGO;
    }

    // Convenience: spawn `count` cards (call this multiple times to populate shop)
    public void SpawnMultiple(int count)
    {
        var spawned = new List<GameObject>(count);

        for (int i = 0; i < count; i++)
        {
            var go = SpawnRandomCard();
            if (go != null) spawned.Add(go);
        }

        ArrangeSpawnedCards(spawned);
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

    // Weighted random pick from `pool` returning the full ShopEntry
    private ShopEntry PickRandomEntry()
    {
        ShopEntry defaultEntry = default;
        if (pool == null || pool.Count == 0) return defaultEntry;

        float total = 0f;
        foreach (var e in pool) total += Mathf.Max(0f, e.weight);

        if (total <= 0f) return pool[0];

        float r = UnityEngine.Random.Range(0f, total);
        float acc = 0f;
        foreach (var e in pool)
        {
            acc += Mathf.Max(0f, e.weight);
            if (r <= acc) return e;
        }

        return pool[pool.Count - 1];
    }
}
