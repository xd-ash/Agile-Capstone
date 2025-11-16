using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TestTEst : MonoBehaviour
{
    public UnitActionSOBase test;

    [SerializeField] private ActionBase[] actions;
    
    public void GetActionsFromSO()
    {

    }
}

[CustomEditor(typeof(TestTEst)), CanEditMultipleObjects]
public class TestEditor : Editor
{
    bool once = false;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        TestTEst test = (TestTEst)target;

        //test.CreateActionObjs();

        base.OnInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}
