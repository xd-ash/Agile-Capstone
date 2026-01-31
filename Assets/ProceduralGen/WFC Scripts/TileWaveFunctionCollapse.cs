using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    //Attempt at static WFC class that can be used for different modules/modulesets
    public static class TileWaveFunctionCollapse
    {
        public static ElementBase[,] WFCGenerate(IModule[] moduleSet, Vector2Int gridSize/*, int maxPlayers, int maxEnemies*/)
        {
            //Random.InitState(NetworkManager.instance.dungeonSeed);

            EnvironmentTileElement[,] grid = new EnvironmentTileElement[gridSize.x, gridSize.y];
            List<Vector2Int> unreachedPositions = new List<Vector2Int>();

            int tempPlayerCount = 0;
            int tempEnemyCount = 0;

            for (int y = 0; y < gridSize.y; y++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    grid[x, y] = new EnvironmentTileElement(moduleSet, position);

                    unreachedPositions.Add(position);
                }
            }
            int rng = Random.Range(0, unreachedPositions.Count);

            CollapseElement(grid[unreachedPositions[rng].x, unreachedPositions[rng].y], grid);
            unreachedPositions.RemoveAt(rng);

            while (unreachedPositions.Count > 0)
            {
                EnvironmentTileElement curElement;
                List<EnvironmentTileElement> lowEntropyElements = new List<EnvironmentTileElement>();
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

                /*switch ((curElement.GetSelectedModule as EnvironmentTileModule).GetTileType)
                {
                    case TileType.SingleEnemy:
                        temp
                        break;
                    case TileType.SinglePlayer:
                        break;
                }*/
            }

            return grid;
        }

        private static void CollapseElement(EnvironmentTileElement curElement, EnvironmentTileElement[,] grid)
        {
            curElement.Collapse();


            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if ((x == 0 && y == 0) || (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)) // setting up for neighbors, but not diags (1,1)
                    {
                        continue;
                    }

                    int curX = curElement.GetPosition.x + x;
                    int curY = curElement.GetPosition.y + y;

                    if (curX < 0 || curY < 0 || curX > grid.GetLength(0) - 1 || curY > grid.GetLength(1) - 1)
                        continue;

                    EnvironmentTileElement curNeighbour = grid[curX, curY];

                    if (x > 0)
                        curNeighbour.RemoveOptions(curElement.GetSelectedModule.East);
                    else if (x < 0)
                        curNeighbour.RemoveOptions(curElement.GetSelectedModule.West);
                    else if (y > 0)
                        curNeighbour.RemoveOptions(curElement.GetSelectedModule.North);
                    else if (y < 0)
                        curNeighbour.RemoveOptions(curElement.GetSelectedModule.South);
                }
            }
        }
    }
}