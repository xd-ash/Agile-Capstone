using System.Collections.Generic;
using UnityEngine;

public abstract class NodeMapNode : MonoBehaviour
{
    [SerializeField] protected List<NodeMapNode> _prev = new();
    [SerializeField] protected List<NodeMapNode> _next = new();
    private LineRenderer _lineRenderer;

    public NodeMapNode[] GetPrevNodes => _prev.ToArray();
    public NodeMapNode[] GetNextNodes => _next.ToArray();

    public virtual void InitNode(List<NodeMapNode> prev, List<NodeMapNode> next)
    {
        _prev = prev;
        _next = next;

        _lineRenderer = gameObject.GetComponentInChildren<LineRenderer>();
        var positions = GetLineRendererPositions();
        _lineRenderer.positionCount = positions.Length;
        _lineRenderer.SetPositions(positions);
    }
    protected virtual Vector3[] GetLineRendererPositions()
    {
        List<Vector3> positions = new();

        positions.Add(transform.position);

        foreach(var node in _prev)
        {
            positions.Add(node.transform.position);
            positions.Add(transform.position);
        }
        foreach(var node in _next)
        {
            positions.Add(node.transform.position);
            positions.Add(transform.position);
        }

        return positions.ToArray();
    }

    //public abstract void OnClick();
}