using UnityEngine;
using System.Collections.Generic;
using System;

public class NodeMapCreator : MonoBehaviour
{
    [SerializeField] private Vector2Int _nodeDepthRange;
    [SerializeField] private Vector2Int _numNodeRange;

    private int _maxNodeDepth;
    private Dictionary<int, List<Node>> _nodesInDepths = new();
    public GameObject prefab;

    private void Start()
    {
        _maxNodeDepth = UnityEngine.Random.Range(_nodeDepthRange.x, _nodeDepthRange.y);

        CreateNodes();
        LinkNodes();
        InstantiateNodes();
    }

    private void CreateNodes()
    {
        for (int i = 0; i < _maxNodeDepth; i++)
        {
            if (i == 0 || i == _maxNodeDepth - 1)
                _nodesInDepths.Add(i, new() { new() });
            else
            {
                int numNodes = UnityEngine.Random.Range(_numNodeRange.x, _numNodeRange.y);
                List<Node> tempNodes = new();
                for (int j = 0; j < numNodes; j++)
                    tempNodes.Add(new());
                _nodesInDepths.Add(i, new(tempNodes));
            }
        }

    }

    private void LinkNodes()
    {
        for (int i = 0; i < _maxNodeDepth; i++)
        {
            List<Node> iMinusNodes = new();
            List<Node> iNodes = new();
            List<Node> iPlusNodes = new();

            if (i != 0)
                iMinusNodes = _nodesInDepths[i - 1];

            iNodes = _nodesInDepths[i];

            if (i != _maxNodeDepth - 1)
                iPlusNodes = _nodesInDepths[i + 1];

            foreach (var node in iNodes)
            {
                if (i != 0 && node.inputs.Count == 0)
                {
                    int rng = UnityEngine.Random.Range(0, iMinusNodes.Count);
                    node.inputs.Add(iMinusNodes[rng]);
                }

                if (i != _maxNodeDepth - 1 && iPlusNodes.Count > 0)
                {
                    int rng = UnityEngine.Random.Range(0, iPlusNodes.Count);
                    node.outputs.Add(iPlusNodes[rng]);
                }
            }
        }
    }
    private void InstantiateNodes()
    {
        foreach (var kvp in _nodesInDepths)
        {
            GameObject parent = new($"Depth {kvp.Key} Parent");
            parent.transform.parent = transform;
            parent.transform.localPosition = Vector3.zero;

            for (int i = 0; i < kvp.Value.Count; i++)
            {
                //GameObject nodeGo = new("node");
                GameObject nodeGo = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                nodeGo.transform.parent = parent.transform;
                nodeGo.transform.localPosition = new Vector3(kvp.Key, -kvp.Value.Count / 2f + i, 0);
                kvp.Value[i].nodeTrans = nodeGo.transform;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        foreach (var kvp in _nodesInDepths)
            foreach (var node in kvp.Value)
                foreach (var output in node.outputs)
                    Gizmos.DrawLine(node.nodeTrans.position, output.nodeTrans.position);
    }
    public class Node
    {
        public int depth;
        public Transform nodeTrans;
        public List<Node> inputs = new();
        public List<Node> outputs = new();
    }
}