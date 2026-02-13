using UnityEditor;
using UnityEngine;

namespace WFC
{
    [CustomEditor(typeof(TileModuleGenerator))]
    public class TileModuleGenerator_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var t = (TileModuleGenerator)target;

            if (GUILayout.Button("Generate Tile Modules"))
                t?.GenerateTileModules();
        }
    }
}