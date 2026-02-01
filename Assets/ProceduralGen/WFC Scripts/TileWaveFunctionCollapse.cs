using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    //Attempt at static WFC class that can be used for different modules/modulesets
    public static class TileWaveFunctionCollapse
    {
        private static int _playerCount;
        private static int _enemyCount;
        public static bool CheckCanSpawnPlayer => _playerCount < PlayerDataManager.Instance.GetCurrMapNodeData.maxPlayersAllowed;
        public static bool CheckCanSpawnEnemy => _enemyCount < PlayerDataManager.Instance.GetCurrMapNodeData.maxEnemiesAllowed;

        public static TileElement[,] WFCGenerate(TileModule[] moduleSet, Vector2Int gridSize)
        {
            //Random.InitState(NetworkManager.instance.dungeonSeed);

            TileElement[,] grid = new TileElement[gridSize.x, gridSize.y];
            List<Vector2Int> unreachedPositions = new List<Vector2Int>();

            _playerCount = 0;
            _enemyCount = 0;

            for (int y = 0; y < gridSize.y; y++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    grid[x, y] = new TileElement(moduleSet, position);

                    unreachedPositions.Add(position);
                }
            }
            int rng = Random.Range(0, unreachedPositions.Count);

            CollapseElement(grid[unreachedPositions[rng].x, unreachedPositions[rng].y], grid);
            unreachedPositions.RemoveAt(rng);

            while (unreachedPositions.Count > 0)
            {
                TileElement curElement;
                List<TileElement> lowEntropyElements = new List<TileElement>();
                int lowestEntropy = int.MaxValue;

                for (int i = 0; i < unreachedPositions.Count; i++)
                {
                    curElement = grid[unreachedPositions[i].x, unreachedPositions[i].y];
                    if (curElement.GetEntropy < lowestEntropy)
                    {
                        lowestEntropy = curElement.GetEntropy;
                        lowEntropyElements.Clear();
                        lowEntropyElements.Add(curElement);
                    }
                    else if (curElement.GetEntropy == lowestEntropy)
                        lowEntropyElements.Add(curElement);
                }
                rng = Random.Range(0, lowEntropyElements.Count);
                curElement = lowEntropyElements[rng];

                CollapseElement(curElement, grid);
                unreachedPositions.Remove(curElement.GetPosition);
            }

            return grid;
        }

        private static void CollapseElement(TileElement curElement, TileElement[,] grid)
        {
            curElement.Collapse();

            switch (curElement.GetSelectedModule.GetTileType)
            {
                case TileType.SingleEnemy:
                    _enemyCount++;
                    break;
                case TileType.SinglePlayer:
                    _playerCount++;
                    break;
            }

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if ((x == 0 && y == 0) || (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)) // setting up for neighbors, but not diags (1,1)
                        continue;

                    int curX = curElement.GetPosition.x + x;
                    int curY = curElement.GetPosition.y + y;

                    if (curX < 0 || curY < 0 || curX > grid.GetLength(0) - 1 || curY > grid.GetLength(1) - 1)
                        continue;

                    TileElement curNeighbour = grid[curX, curY];

                    if (x > 0)
                        curNeighbour.RemoveOptionsByNeighbor(curElement.GetSelectedModule.East);
                    else if (x < 0)
                        curNeighbour.RemoveOptionsByNeighbor(curElement.GetSelectedModule.West);
                    else if (y > 0)
                        curNeighbour.RemoveOptionsByNeighbor(curElement.GetSelectedModule.North);
                    else if (y < 0)
                        curNeighbour.RemoveOptionsByNeighbor(curElement.GetSelectedModule.South);
                }
            }
        }
    }
}