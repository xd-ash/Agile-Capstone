using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomGridTool
{
    public class GridAnchorScript : MonoBehaviour
    {
        private int _gridRows, _gridCols;
        private float _sizeScalar;
        private List<GameObject> _gridTileObjects;

        public int GridRows { get { return _gridRows; }}
        public int GridCols { get { return _gridCols; }}
        public float SizeScalar { get { return _sizeScalar; }}
        public List<GameObject> GridTileObjects { get { return _gridTileObjects; }}

        public void CreateGridInfo(int rows, int cols, float scalar)
        {
            _gridRows = rows;
            _gridCols = cols;
            _sizeScalar = scalar;

            SortChildren();
        }
        public void SortChildren()
        {
            _gridTileObjects = new List<GameObject>();

            for (int i = 0; i < transform.childCount; i++)
            {
                _gridTileObjects.Add(transform.GetChild(i).gameObject);
            }
            List<GameObject> sortedList = _gridTileObjects.OrderBy(go => RowChecker(go.name)).ToList();

            for (int i = 0; i < sortedList.Count; i++)
            {
                sortedList[i].transform.SetSiblingIndex(i);
            }

            /* Potentially don't need any of this commented section. Sorting method used directly above seems to properly sort
             * (keeping just in case there is some overlooked error)
             * 
            List<GameObject> goList = new List<GameObject>();
            List<List<GameObject>> goRectList = new List<List<GameObject>>();

            if (_gridCols > 1)
            {
                for (int i = 1; i <= sortedList.Count; i++)
                {
                    goList.Add(sortedList[i - 1]);

                    if (i % _gridCols == 0 && i != 0)
                    {
                        goRectList.Add(goList);
                        goList = new List<GameObject>();
                    }
                }
                sortedList = new List<GameObject>();

                for (int i = 0; i < goRectList.Count; i++)
                {
                    goRectList[i] = goRectList[i].OrderBy(go => go.name.Length).ToList();
                    for (int j = 0; j < goRectList[i].Count; j++)
                    {
                        sortedList.Add(goRectList[i][j]);
                    }
                }
            }
            else
            {
                sortedList = sortedList.OrderBy(go => go.name.Length).ToList();
            }
            
            for (int i = 0; i < sortedList.Count; i++)
            {
                sortedList[i].transform.SetSiblingIndex(i);
            }*/
        }
        public int RowChecker(string goName)
        {
            string[] rowsAndCols = goName.Split('-');
            int row;

            if (!int.TryParse(rowsAndCols[0], out row))
            {
                Debug.Log($"GameObject ({goName}) is incorrectly labeled." +
                    $"\nGrid Heirarchy sorted incorrectly (Consider correcting the name or recreating the grid)");
                return -1;
            }
            else
            {
                return row;
            }
        }
    }
}
