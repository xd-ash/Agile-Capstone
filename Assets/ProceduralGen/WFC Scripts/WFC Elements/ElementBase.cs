using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    public abstract class ElementBase
    {
        protected List<IModule> _options;
        protected IModule _selectedModule;
        protected Vector2Int _position;
        protected bool _isEdge;

        public virtual List<IModule> GetOptions { get => _options; }
        public virtual IModule GetSelectedModule { get => _selectedModule; }
        public virtual Vector2Int GetPosition { get => _position; }
        public virtual bool GetEdgeBool { get => _isEdge; }
        public virtual int GetEntropy { get => _options.Count; }

        public abstract void Collapse();
        public virtual void RemoveOptions(IModule[] legalNeighbors)
        {
            List<IModule> temp = new List<IModule>(legalNeighbors);

            for (int i = _options.Count - 1; i >= 0; i--)
            {
                if (!temp.Contains(_options[i]))
                    _options.RemoveAt(i);
            }
        }
        public virtual void RemoveOptionsFromPosition()
        {
            throw new System.NotImplementedException();
        }
    }
}