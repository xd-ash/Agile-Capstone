using System.Collections.Generic;
using UnityEngine;

public class NewNodeCreation : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem.MinMaxCurve possibleNodesPerTier = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(), new AnimationCurve());
    [SerializeField] private int _numberOfTiers = 10;

    private Dictionary<int, List<Node>> _nodeTiers = new Dictionary<int, List<Node>>();

    private void Start()
    {
        for (int i = 0; i < _numberOfTiers; i++)
        {
            List<Node> nodes = new List<Node>();

            int rng = (int)possibleNodesPerTier.Evaluate((float)i / _numberOfTiers, Random.Range(0f, 1f));

            for (int y = 0; y < rng; y++)
            {
                Debug.Log("Tier " + i +": " + rng);
            }
        }
    }

    public class Node
    {
        public List<Node> prev = new();
        public List<Node> next = new();
        public List<Node> possiblePrev = new();
        public List<Node> possibleNext = new();
    }
}
