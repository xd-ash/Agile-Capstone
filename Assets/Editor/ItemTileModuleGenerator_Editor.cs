using UnityEditor;
using UnityEngine;

namespace WFC
{
    [CustomEditor(typeof(ItemTileModuleGenerator))]
    public class TileModuleGenerator_Editor : Editor
    {
        private ItemTileModuleGenerator _cur;

        private void OnEnable()
        {
            _cur = (ItemTileModuleGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate Tile Modules"))
            {
                _cur?.GenerateTileModules();
            }
        }
    }
}