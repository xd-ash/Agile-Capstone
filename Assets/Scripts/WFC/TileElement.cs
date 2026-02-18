using UnityEngine;
using System.Collections.Generic;

namespace WFC
{
    public class TileElement
    {
        protected List<TileModule> _options;
        protected TileModule _selectedModule;
        protected Vector2Int _position;
        protected bool _isEdge;

        public virtual List<TileModule> GetOptions => _options;
        public virtual TileModule GetSelectedModule => _selectedModule;
        public virtual Vector2Int GetPosition => _position;
        public virtual bool GetEdgeBool => _isEdge;
        public virtual int GetEntropy => _options.Count;

        public TileElement(TileModule[] options, Vector2Int position)
        {
            _options = new List<TileModule>(options);
            _position = position;
        }

        public void Collapse()
        {
            RemoveOptionsOnCollapse();

            int rng = Random.Range(0, _options.Count);
            
            //Debug.Log($"optionsCount:{_options.Count}");
            _selectedModule = _options[rng];
        }
        public virtual void RemoveOptionsByNeighbor(TileModule[] legalNeighbors)
        {
            List<TileModule> temp = new List<TileModule>(legalNeighbors);

            for (int i = _options.Count - 1; i >= 0; i--)
            {
                if (!temp.Contains(_options[i]))
                    _options.RemoveAt(i); 
            }
        }
        public void RemoveOptionsOnCollapse()
        {
            for (int i = _options.Count - 1; i >= 0; i--)
            {
                if (_options[i] == null)
                {
                    _options.RemoveAt(i);
                    continue;
                }

                if (MapCreator.Instance == null) continue;
            }
        }
    }
}