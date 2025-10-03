using UnityEngine;

namespace CardSystem
{
    public interface IAreaOfEffect
    {
        public enum AoETypes
        {
            None,
            Cone,
            Circle,
            Line
        }
        public AoETypes AoEType { get; }
        public int AoERange { get; }

        public abstract void AffectTiles();
    }
}
