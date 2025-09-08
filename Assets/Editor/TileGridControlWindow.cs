using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CustomGridTool
{
    [System.Serializable]
    public class TileGridControlWindow : EditorWindow
    {
        private SerializedObject _serializedWindow;

        private int _gridRows = 5, maxRows = 15,
                    _gridCols = 5, maxCols = 15;
        private float _sizeScalar = 1;
        private GameObject _tilePrefab, _gridAnchorPrefab, _selectedGrid;
        private Vector2 _scrollPos;
        private Vector3 _anchorPointPos;
        private string _newGridName = "TileGridAnchor";

        [MenuItem("Window/Custom Tile Grid Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<TileGridControlWindow>();
            window.titleContent = new GUIContent("Custom Tile Grid Tool");
            window.Show();
        }

        private void OnEnable()
        {
            _serializedWindow = new SerializedObject(this);

            //Load prefabs to use in grid creation
            _tilePrefab = Resources.Load<GameObject>("TileGridPrefabs/TileBase");
            _gridAnchorPrefab = Resources.Load<GameObject>("TileGridPrefabs/TileGridAnchor");

            //set tool size
            minSize = new Vector2(450, 575);
            maxSize = new Vector2(450.01f, 575);
        }

        private void OnGUI()
        {
            //undo stuff
            _serializedWindow.Update();
            Undo.RecordObject(_serializedWindow.targetObject, nameof(TileGridControlWindow));
            //

            //Grab existing grid info if current selection is a grid
            //else clear _selectedGrid
            var select = Selection.activeGameObject;
            if (select != null)
            {
                if (select.GetComponent<GridAnchorScript>() && _selectedGrid != select)
                {
                    GetExistingGridInfo();
                }
                else if (!select.GetComponent<GridAnchorScript>())
                {
                    _selectedGrid = null;
                }
            }
            else
            {
                _selectedGrid = null;
            }

            //Draw each tool section 
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawListOfSceneGrids();
                    DrawGridOptions();
                }
                GUILayout.Space(-5);
                CreateGridPreview();
            }
        }

        //Create input fields for number of columns and rows, anchor position,
        //as well as potential size scaler for resizing tiles
        private void DrawGridOptions()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Height(maxSize.y / 4)))
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    GUILayout.Label("Grid Options");
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(new GUIContent("Columns:", $"Range of 1 - {maxCols} columns."));
                    GUILayout.Space(5);
                    _gridCols = EditorGUILayout.IntSlider(_gridCols, 1, maxCols, GUILayout.Width(maxSize.x / 2));
                    GUILayout.FlexibleSpace();
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(new GUIContent("Rows:     ", $"Range of 1 - {maxRows} rows."));
                    GUILayout.Space(7f);
                    _gridRows = EditorGUILayout.IntSlider(_gridRows, 1, maxRows, GUILayout.Width(maxSize.x / 2));
                    GUILayout.FlexibleSpace();
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(new GUIContent("Size Scalar", "Scale multiplier for tiles (range 1 - 10)"));
                    GUILayout.Space(5);
                    _sizeScalar = EditorGUILayout.FloatField(_sizeScalar, GUILayout.Width(maxSize.x / 6));
                    _sizeScalar = Mathf.Clamp(_sizeScalar, 0, 10);
                    if (_sizeScalar == 0)
                    {
                        GUILayout.Label("**Scale is ZERO**");
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Space(5);

                using (new EditorGUILayout.HorizontalScope())
                {
                    _anchorPointPos = EditorGUILayout.Vector3Field(new GUIContent("Anchor Position:", "Empty Parent's position"), _anchorPointPos);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Label("(Grid origin is top left (builds right and then down))");
                GUILayout.FlexibleSpace();
            }
        }

        //Create list of grid anchors and "highlight" current selected grid if any
        public void DrawListOfSceneGrids()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Height(maxSize.y / 4)))
            {
                List<GameObject> grids = new List<GameObject>();

                foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (obj.GetComponent<GridAnchorScript>())
                    {
                        grids.Add(obj);
                    }
                }

                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.Width(maxSize.x / 3)))
                {
                    GUILayout.Label("Grids in Scene");
                }

                using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(_scrollPos))
                {
                    _scrollPos = scrollViewScope.scrollPosition;
                    if (grids.Count > 0)
                    {
                        foreach (GameObject grid in grids)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                if (_selectedGrid == grid)
                                {
                                    GUILayout.Label(">>");
                                }
                                else
                                {
                                    GUILayout.Label("-");
                                }
                                GUILayout.Space(-5);

                                string truncatedName = grid.name.Length > 12 ? grid.name.Substring(0, 11) + "..." : grid.name;
                                if (GUILayout.Button(new GUIContent(truncatedName, grid.name), GUILayout.Width(maxSize.x / 5)))
                                {
                                    if (grid != Selection.activeGameObject)
                                    {
                                        Selection.activeGameObject = grid;
                                    }
                                    else
                                    {
                                        EditorGUIUtility.PingObject(grid);
                                    }
                                }
                                GUILayout.FlexibleSpace();
                            }
                        }
                        GUILayout.FlexibleSpace();
                        if (_selectedGrid != null)
                        {
                            if (GUILayout.Button("Create New Grid"))
                            {
                                ToolValReset();
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label("None");
                    }
                }
            }
        }

        //Create input fields for anchor position and grid origin corner,
        //then draw a preview of the grid setup with create grid button below
        private void CreateGridPreview()
        {
            if (ChildCountChecker())
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); //line seperator
                GUILayout.Space(-5);
                if (_selectedGrid != null)
                {
                    GUILayout.Space(-4);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label($"\"{_selectedGrid.name}\"", EditorStyles.boldLabel);
                        GUILayout.Space(-5);
                        GUILayout.Label($"Preview ({_gridCols}x{_gridRows}):");
                        GUILayout.FlexibleSpace();
                    }
                }
                else
                {
                    GUILayout.Space(-3);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Grid Name: ");
                        _newGridName = GUILayout.TextField(_newGridName, GUILayout.Width(maxSize.x / 4));
                        GUILayout.Space(-5);
                        GUILayout.Label($"({_gridCols}x{_gridRows})");
                        GUILayout.FlexibleSpace();
                    }
                }
                if (_gridRows > 10 || _gridCols > 10)
                {
                    GUILayout.Space(-10);
                    GUILayout.Label("*Hover over tiles to view names", EditorStyles.miniBoldLabel);
                }

                GUILayout.FlexibleSpace();

                for (int i = 0; i < _gridRows; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        for (int j = 0; j < _gridCols; j++)
                        {
                            GUILayout.Button(new GUIContent(GridPreviewButtonName(false, i, j), GridPreviewButtonName(true, i, j))
                                , GUILayout.Height(GridPreviewSizeCalc()), GUILayout.Width(GridPreviewSizeCalc()));
                        }

                        GUILayout.FlexibleSpace();
                    }
                }

                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button($"{(_selectedGrid != null ? "Adjust" : "Create")} Grid", GUILayout.MaxHeight(25), GUILayout.MaxWidth(this.position.width / 2)))
                    {
                        if (EditorUtility.DisplayDialog("Confirm Choice", $"{(_selectedGrid != null ? "Adjust the" : "Create a")} {_gridCols}x{_gridRows} Grid?", "Yes", "No"))
                        {
                            if (_selectedGrid != null)
                            {
                                AdjustExistingTileGrid();
                            }
                            else
                            {
                                InstantiateTileGrid();
                            }
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Space(5);
            }
            else
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"{_selectedGrid.name} has incorrect number of tile children.\n\nExpected Children: {_gridCols * _gridRows}\nCurrent Children: {_selectedGrid.transform.childCount}" +
                                        $"\n\nConsider recreating grid or manually filling gaps with the tile Prefab.");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.FlexibleSpace();  
            }
        }

        //calulate button sizes for the grid preview section
        private float GridPreviewSizeCalc()
        {
            int greaterTileLength = _gridRows > _gridCols ? _gridRows : _gridCols;

            if (greaterTileLength > 10)
            {
                return 325 / greaterTileLength; //325 value obtained from trial & error 
            }
            else
            {
                return 35;//35 as the default length & width
            }

        }

        //Determine whether or not to show button names based on number of rows and/or columns
        //and enable tooltips when button names are disabled
        private string GridPreviewButtonName(bool isTooltip, int i, int j)
        {
            switch (isTooltip)
            {
                case true:
                    if (_gridRows > 10 || _gridCols > 10)
                    {
                        return $"{i}-{j}";
                    }
                    else
                    {
                        return string.Empty;
                    }
                case false:
                    if (_gridRows > 10 || _gridCols > 10)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return $"{i}-{j}";
                    }
            }
        }

        //Create grid of tile prefabs with an empty parent (grid anchor) that has
        //user entered columns and rows.
        private void InstantiateTileGrid()
        {
            GameObject gridAnchor = GameObject.Instantiate(_gridAnchorPrefab, _anchorPointPos, Quaternion.identity);
            gridAnchor.name = _newGridName;

            if (!gridAnchor.GetComponent<GridAnchorScript>())
            {
                gridAnchor.AddComponent<GridAnchorScript>();
            }

            for (int i = 0; i < _gridRows; i++)
            {
                for (int j = 0; j < _gridCols; j++)
                {
                    GameObject tile = GameObject.Instantiate(_tilePrefab, gridAnchor.transform);
                    tile.transform.localPosition = new Vector3(j * _sizeScalar, -i * _sizeScalar, 0);
                    tile.transform.localScale = tile.transform.localScale * _sizeScalar;
                    tile.name = $"{i}-{j}";
                }
            }

            gridAnchor.GetComponent<GridAnchorScript>().CreateGridInfo(_gridRows, _gridCols, _sizeScalar);
            Selection.activeObject = gridAnchor;
        }

        //Adjust existing grid of tile prefabs with an empty parent (grid anchor) that has
        //user entered columns and rows.
        private void AdjustExistingTileGrid()
        {
            var gridAnchor = _selectedGrid.GetComponent<GridAnchorScript>();

            for (int i = 0; i < gridAnchor.GridRows ; i++)
            {
                for (int j = 0; j < gridAnchor.GridCols; j++)
                {
                    if (i >= _gridRows ||  j >= _gridCols)
                    {
                        DestroyImmediate(_selectedGrid.transform.Find($"{i}-{j}").gameObject);
                    }
                }
            }

            for (int i = 0; i < _gridRows; i++)
            {
                for (int j = 0; j < _gridCols; j++)
                {
                    if (i > gridAnchor.GridRows - 1 || j > gridAnchor.GridCols - 1)
                    {
                        GameObject tile = GameObject.Instantiate(_tilePrefab, gridAnchor.transform);
                        tile.transform.localPosition = new Vector3(j * _sizeScalar, -i * _sizeScalar, 0);
                        tile.transform.localScale = tile.transform.localScale * _sizeScalar;
                        tile.name = $"{i}-{j}";
                    }
                }
            }

            gridAnchor.CreateGridInfo(_gridRows, _gridCols, _sizeScalar);
        }

        //grab grid information from grid anchor script
        private void GetExistingGridInfo()
        {
            _selectedGrid = Selection.activeGameObject;
            var grid = _selectedGrid.GetComponent<GridAnchorScript>();

            _gridRows = grid.GridRows;
            _gridCols = grid.GridCols;
            _sizeScalar = grid.SizeScalar;
        }

        //Check whether of not grid contains the expected amount of tiles.
        //(should update this function depending on how tiles evolve over time)
        private bool ChildCountChecker()
        {
            if (_selectedGrid != null)
            {
                var anchorScript = _selectedGrid.GetComponent<GridAnchorScript>();
                List<Transform> tileChildList = new List<Transform>();

                for (int i = 0; i < _selectedGrid.transform.childCount; i++)
                {
                    Transform child = _selectedGrid.transform.GetChild(i);

                    if (child.GetComponent<TileHoverScript>())
                    {
                        tileChildList.Add(child);
                    }
                }

                if (tileChildList.Count != (anchorScript.GridCols * anchorScript.GridRows))
                {
                    return false;
                }
            }
            return true;
        }
        
        //reset tool values with bool param option to clear selection
        private void ToolValReset(bool resetSelect = true)
        {
            if (resetSelect)
            {
                Selection.activeGameObject = null;
            }
            _gridRows = 5;
            _gridCols = 5;
            _sizeScalar = 1;
            _newGridName = "TileGridAnchor";
        }
    }
}
