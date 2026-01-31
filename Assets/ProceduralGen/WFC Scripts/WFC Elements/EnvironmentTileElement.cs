using UnityEngine;
using System.Collections.Generic;
using AStarPathfinding;

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
            RemoveOptionsOnCollapse();

            int rng = Random.Range(0, _options.Count);
            _selectedModule = _options[rng];
        }

        public override void RemoveOptionsOnCollapse()
        {
            /*for (int i = _options.Count - 1; i >= 0; i--)
            {
                if (_options[i] == null)
                {
                    _options.RemoveAt(i);
                    continue;
                }

                if (MapCreator.instance == null) return;

                switch ((_options[i] as EnvironmentTileModule).GetTileType)
                {
                    case TileType.SingleEnemy:
                        if (MapCreator.instance.CheckCanSpawnEnemy) continue;
                        break;
                    case TileType.SinglePlayer:
                        if (MapCreator.instance.CheckCanSpawnPlayer) continue;
                        break;
                    case TileType.OnlyObstacles:
                        continue;
                }

                _options.RemoveAt(i);
            }*/
        }
    }
}