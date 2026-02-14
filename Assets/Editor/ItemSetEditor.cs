using UnityEngine;
using UnityEditor;

namespace WFC
{
    [CustomEditor(typeof(TileSet))]
    public class ItemTileSetEditor : Editor
    {
        private TileSet _environmentTileSet;

        private void OnEnable()
        {
            _environmentTileSet = target as TileSet;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            /*if (GUILayout.Button("Generate Neighbours"))
            {
                _environmentTileSet.SetNeighbours();
            }*/
        }
    }
}