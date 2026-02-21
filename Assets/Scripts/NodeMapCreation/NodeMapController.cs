using System.Collections.Generic;
using UnityEngine;

public class NodeMapController : MonoBehaviour
{
    private NodeMapCreator _nodeMapCreator;
    private Dictionary<int, List<NodeMapNode>> _nodeMap = new();

    private int _randomSeed; //replace w/ playerdata one

    private void Start()
    {
        _nodeMapCreator = GetComponent<NodeMapCreator>();

        _randomSeed = UnityEngine.Random.Range(0, int.MaxValue);

        _nodeMap = _nodeMapCreator.GenerateFullNodeMap(_randomSeed);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _randomSeed = UnityEngine.Random.Range(0, int.MaxValue);

            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

            _nodeMap = _nodeMapCreator.GenerateFullNodeMap(_randomSeed);
        }
    }
    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.khaki;

        foreach (var kvp in _nodeMap)
            foreach (var node in kvp.Value)
                foreach (var prevNode in node.GetPrevNodes)
                    Gizmos.DrawLine(node.transform.position, prevNode.transform.position);
    }*/
}
