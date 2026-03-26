using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WFC
{
    [CreateAssetMenu(menuName = "WFC/New Tile Module Set")]
    public class TileSet : ScriptableObject
    {
        [SerializeField] private TileModule[] _tileModules;
        [SerializeField, HideInInspector] private int _moduleWidth;
        [SerializeField, HideInInspector] private int _keyDepth;

        public TileModule[] Modules => _tileModules;
        public int GetTrueModuleWidth => _moduleWidth - 2 * _keyDepth;
        public int GetModuleKeyDepth => _keyDepth;
        public Vector2 TempGetDimesnions => new Vector2(_moduleWidth, _keyDepth);
        public void AddModules(List<TileModule> tileModules)
        {
            var tmp = new List<TileModule>();
            foreach (var module in tileModules)
                if (module != null)
                    tmp.Add(module);
            _tileModules = tmp.ToArray();
        }
        public void SetNeighbours()
        {
            _moduleWidth = _tileModules[0].GetModuleWidth;
            _keyDepth = _tileModules[0].GetKeyDepth;
            
            for (int j = 0; j < _tileModules.Length; j++)
            {
                TileModule curModule = _tileModules[j];
                List<TileModule> n = new(), e = new(), s = new(), w = new();

                for (int i = 0; i < _tileModules.Length; i++)
                {
                    TileModule moduleToCompare = _tileModules[i];

                    //if (curModule == moduleToCompare) continue;//same modules cannot be neighbors
                    
                    if (CheckKey(curModule.NorthKey, moduleToCompare.SouthKey))
                        n.Add(moduleToCompare);
                    if (CheckKey(curModule.EastKey, moduleToCompare.WestKey))
                        e.Add(moduleToCompare);
                    if (CheckKey(curModule.SouthKey, moduleToCompare.NorthKey))
                        s.Add(moduleToCompare);
                    if (CheckKey(curModule.WestKey, moduleToCompare.EastKey))
                        w.Add(moduleToCompare);
                }

                curModule.SetNeighbourArrays(n.ToArray(), e.ToArray(), s.ToArray(), w.ToArray());
            }
        }
        public bool CheckKey(TileBase[] key1, TileBase[] key2)
        {
            if (key1 == null || key2 == null)
            {
                Debug.LogError("Key is null");
                return false;
            }
            if (key1.Length != key2.Length)
            {
                Debug.LogError("Key Length not equal");
                return false;
            }

            for (int i = 0; i < key1.Length; i++)
                if (key1[i] != key2[i]) return false;
            return true;
        }
    }
}