using UnityEngine;
public enum LoopingVFXType
{
    Fire,
    Bleed
}

public class HitVFXSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private ParticleSystem _bloodPrefab;
    [SerializeField] private ParticleSystem _sparkPrefab;
    
    [Header("Looping VFX Prefabs")]
    [SerializeField] private ParticleSystem _firePrefab;
    [SerializeField] private ParticleSystem _bleedPrefab; 

    [Header("Settings")]
    [SerializeField] private float _destroyDelay = 2f; 
    [SerializeField] private float _spawnHeightOffset = 0.5f;

    public void SpawnBlood()
    {
        SpawnEffect(_bloodPrefab);
    }

    public void SpawnSpark()
    {
        SpawnEffect(_sparkPrefab);
    }

    private void SpawnEffect(ParticleSystem prefab)
    {
        if (prefab == null)
        {
            return;
        }

        Vector3 spawnPos = transform.position + new Vector3(0f, _spawnHeightOffset, 0f);
        ParticleSystem instance = Instantiate(prefab, spawnPos, Quaternion.identity);
        instance.Play();
        Destroy(instance.gameObject, _destroyDelay);
    }

    public ParticleSystem SpawnLoopingEffect(LoopingVFXType type)
    {
        ParticleSystem prefab = type switch
        {
            LoopingVFXType.Fire => _firePrefab,
            LoopingVFXType.Bleed => _bleedPrefab,
            _ => null
        };

        if (prefab == null) return null;

        // [LOOP_VFX] Parent to unit so it follows movement, offset upward
        Vector3 offset = new Vector3(0f, _spawnHeightOffset, 0f);
        ParticleSystem instance = Instantiate(prefab, transform.position + offset, Quaternion.identity, transform);
        instance.transform.localPosition = new Vector3(0f, _spawnHeightOffset, 0f);
        instance.Play();
        return instance;
    }

// [LOOP_VFX] Stops and destroys a looping particle instance
    public static void StopLoopingEffect(ParticleSystem instance)
    {
        if (instance == null) return;
        instance.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        Destroy(instance.gameObject, 1f); // [LOOP_VFX] brief delay so remaining particles fade out
    }
}