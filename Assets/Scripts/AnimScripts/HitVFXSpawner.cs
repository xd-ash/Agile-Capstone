using UnityEngine;

public class HitVFXSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private ParticleSystem _bloodPrefab;
    [SerializeField] private ParticleSystem _sparkPrefab;

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

}