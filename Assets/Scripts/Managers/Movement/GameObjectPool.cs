using System.Collections.Generic;
using UnityEngine;

public static class GameObjectPool
{
    private static Dictionary<GameObject, Pool> _objectPools;

    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (_objectPools == null)
            _objectPools = new Dictionary<GameObject, Pool>();

        if (!_objectPools.ContainsKey(prefab))
            _objectPools[prefab] = new Pool(prefab);

        return _objectPools[prefab].Spawn(position, rotation, parent);
    }

    public static void Remove(GameObject objectToRemove)
    {
        PoolElement pe = objectToRemove.GetComponent<PoolElement>();

        if (pe == null)
            GameObject.Destroy(objectToRemove);
        else
            pe.MyPool.Despawn(objectToRemove);
    }

    private class Pool
    {
        private int _currIndex = 0;
        private Stack<GameObject> _inactiveObjects;
        private GameObject _prefab;

        public Pool(GameObject prefab)
        {
            _prefab = prefab;
            _inactiveObjects = new Stack<GameObject>();
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject obj;

            if (_inactiveObjects.Count == 0)
            {
                obj = GameObject.Instantiate(_prefab, Vector3.zero, Quaternion.identity, parent);
                obj.name = _prefab.name + "_" + _currIndex;
                _currIndex++;
                obj.AddComponent<PoolElement>().MyPool = this;
            }
            else
            {
                obj = _inactiveObjects.Pop();

                if (obj == null)
                    return Spawn(position, rotation, parent);
            }

            obj.transform.SetLocalPositionAndRotation(position, rotation);
            obj.transform.localScale = Vector3.one; //should be changed to accept a value from param
            obj.SetActive(true);

            return obj;
        }

        public void Despawn(GameObject objectToRemove)
        {
            objectToRemove.SetActive(false);
            _inactiveObjects.Push(objectToRemove);
        }
    }

    private class PoolElement : MonoBehaviour
    {
        public Pool MyPool { get; set; }
    }
}
