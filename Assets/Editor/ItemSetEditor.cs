using UnityEngine;
using UnityEditor;

namespace WFC
{
    [CustomEditor(typeof(EnvironmentTileSet))]
    public class ItemTileSetEditor : Editor
    {
        private EnvironmentTileSet _environmentTileSet;

        private void OnEnable()
        {
            _environmentTileSet = target as EnvironmentTileSet;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate Neighbours"))
            {
                _environmentTileSet.SetNeighbours();
            }
        }
    }
}