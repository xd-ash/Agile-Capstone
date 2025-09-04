using UnityEditor;
using UnityEngine;

[System.Serializable]
public class TileGridControlWindow : EditorWindow
{
    private SerializedObject _serializedWindow;
    private int _gridRows = 1, _gridCols = 1;
    private float _sizeScalar = 1;
    private GameObject _tilePrefab, _gridAnchorPrefab;
    /*private enum GridAnchorPoint
    {
        BottomLeft,
        BottomRight,
        TopLeft,
        TopRight
    }*/
    //private GridAnchorPoint _anchorPoint;
    private Vector3 _anchorPointPos;


    //Potential additions-
        //private float xTileSpacing;
        //private float yTileSpacing;
        //private bool _isExistingGridAdjust;
        //private bool _useRandomObstacles
            //private int _numObstacles; (maybe split for trees/rocks/bushes?)


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

        _tilePrefab = Resources.Load<GameObject>("TileGridPrefabs/TileBase");
        _gridAnchorPrefab = Resources.Load<GameObject>("TileGridPrefabs/TileGridAnchor");

    minSize = new Vector2(450, 575);//adjust? (leftover from reference)
        maxSize = new Vector2(450.01f, 575);
    }

    private void OnGUI()
    {
        //undo stuff
        _serializedWindow.Update();
        Undo.RecordObject(_serializedWindow.targetObject, nameof(TileGridControlWindow));
        //

        using (new EditorGUILayout.VerticalScope())
        {
            DrawGridOptions();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); //line seperator
            CreateGridPreview();
        }
    }

    //Create input fields for number of columns and rows, as well as potential size scaler for resizing tiles
    private void DrawGridOptions()
    {
        GUILayout.Label("Grid Options", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            _gridCols = EditorGUILayout.IntSlider(new GUIContent("Cols", "Range of 1 - 10 columns."), _gridCols, 1, 10);
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            _gridRows = EditorGUILayout.IntSlider(new GUIContent("Rows: ", "Range of 1 - 10 rows."), _gridRows, 1, 10);
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            _sizeScalar = EditorGUILayout.FloatField(new GUIContent("Size Scalar: ", "Scale value locked to range of 0 - 10"), _sizeScalar);
            _sizeScalar = Mathf.Clamp(_sizeScalar, 0, 10);
            if (_sizeScalar == 0)
            {
                GUILayout.Label("**Scale is ZERO**");
            }
            GUILayout.FlexibleSpace();
        }
    }

    //Create input fields for anchor position and grid origin corner,
    //then draw a preview of the grid setup with create grid button below
    private void CreateGridPreview()
    {
        GUILayout.Label(new GUIContent("Grid Preview: "), EditorStyles.boldLabel);
        GUILayout.Space(5);
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(new GUIContent("Anchor Position:", "Empty Parent's position."));
            GUILayout.Space(10);
            _anchorPointPos = EditorGUILayout.Vector3Field("",_anchorPointPos);
            GUILayout.FlexibleSpace();
        }
        GUILayout.Label("(Grid origin is top left (builds right and then down))");

        /* Removed grid origin option(no fucntionality yet) for now as it may not be needed
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(new GUIContent("Grid Origin:", "Starting corner for grid building."));
            _anchorPoint = (GridAnchorPoint)EditorGUILayout.EnumPopup(_anchorPoint);
            GUILayout.FlexibleSpace();
        }*/

        GUILayout.FlexibleSpace();

        for (int i = 0; i < _gridRows; i++)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                for (int j = 0; j < _gridCols; j++)
                {
                    GUILayout.Button(new GUIContent($"{i}-{j}"), GUILayout.Height(35), GUILayout.Width(35));
                }

                GUILayout.FlexibleSpace();
            }
        }

        GUILayout.FlexibleSpace();
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();

            //Potentially add in logic for if an existing grid is being changed rather than a new grid creation
            if(GUILayout.Button("Create Grid", GUILayout.MaxHeight(25), GUILayout.MaxWidth(this.position.width / 2)))
            {
                if (EditorUtility.DisplayDialog("Confirm Choice", $"Create a {_gridCols}x{_gridRows} Grid?", "Yes", "No"))
                {
                    InstantiateTileGrid();
                }
            }
            GUILayout.FlexibleSpace();
        }
    }

    private void InstantiateTileGrid()
    {
        //switch based of grid origin enum that leads to different build paths

        //based on origin enum, xspacing and/or yspacing will be +/-

        //potentially store data on parent empty abt grid size & orientation?
        //for tool use on deselecting the tile grid or closing window?

        GameObject gridAnchor = GameObject.Instantiate(_gridAnchorPrefab, _anchorPointPos, Quaternion.identity);

        for (int i = 0; i < _gridRows; i++)
        {
            for (int j = 0; j < _gridCols; j++)
            {
                GameObject tile = GameObject.Instantiate(_tilePrefab, gridAnchor.transform);
                tile.transform.localPosition = new Vector3(j * _sizeScalar, -i * _sizeScalar, 0);
                tile.transform.localScale = tile.transform.localScale * _sizeScalar;
                tile.name = $"{j}-{i}";
            }
        }
    }
}
