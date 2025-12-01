using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CardSystem
{
    //Concrete filter strategy to filter based on gameobject layer
    [CreateNodeMenu("Filter/Layer Filter")]
    public class LayerFilter : FilterStrategy
    {
        public enum TargetLayers
        {
            None = 0,
            OpposingTeam = 2,
            SameTeam = 4,
        }
        [SerializeField] private TargetLayers _targetLayers;

        private LayerMask _layermask;
        
        public override IEnumerable<GameObject> Filter(IEnumerable<GameObject> objectsToFilter, Unit unit)
        {
            GrabLayerMask(unit.team);

            foreach (var obj in objectsToFilter)
                if (obj != null && ((_layermask & (1 << obj.layer)) != 0)) // Bitwise operations, not sure if done correctly
                    yield return obj;
        }

        private void GrabLayerMask(Team unitTeam)
        {
            string[] enumStrings = _targetLayers.ToString().Split(", ");
            List<string> layerStrings = new List<string>();

            if (enumStrings.Contains(TargetLayers.SameTeam.ToString()) || enumStrings.Contains("Everything"))
                layerStrings.Add(unitTeam == Team.Friendly ? "Player" : "Enemy");
            if (enumStrings.Contains(TargetLayers.OpposingTeam.ToString()) || enumStrings.Contains("Everything"))
                layerStrings.Add(unitTeam == Team.Friendly ? "Enemy" : "Player");

            _layermask = LayerMask.GetMask(layerStrings.ToArray());
        }
    }
}