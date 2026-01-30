using UnityEngine;
using System.Collections.Generic;

namespace WFC
{
    public class EnvironmentTileElement : ElementBase
    {
        public EnvironmentTileElement(IModule[] options, Vector2Int position)
        {
            _options = new List<IModule>(options);
            _position = position;
        }

        public override void Collapse()
        {
            RemoveOptionsFromPosition();

            int rng = Random.Range(0, _options.Count);
            _selectedModule = _options[rng];
        }

        public override void RemoveOptionsFromPosition()
        {

        }
    }
}