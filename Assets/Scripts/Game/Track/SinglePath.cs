using Game.Player;
using UnityEngine;

namespace Game.Track
{
    internal abstract class SinglePath
    {
        protected GameObject GameObject;
        public abstract void Draw();
        public abstract Vector3 GetInitialPosition();
        public abstract Vector3 NextPosition(TrackPlayer player);

        public abstract Vector3 GetLaneSwapPosition(TrackPlayer player, SinglePath singlePath);

        public void Destroy()
        {
            Object.Destroy(GameObject);
        }

        public abstract Vector3 NextRotation(TrackPlayer player);
    }
}