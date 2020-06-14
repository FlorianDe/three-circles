using UnityEditor;
using UnityEngine;

namespace Game.Player
{
    public class TrackPlayer
    {
        public enum PlayerDirection
        {
            Clockwise = 1,
            CounterClockwise = -1
        }

        private TrackPlayerProperties _properties;

        public TrackPlayer(string name, string movementKeyLeft, string movementKeyRight,
            string movementKeySwapDirection, float speed = 10f)
        {
            Name = name;
            Direction = PlayerDirection.Clockwise;
            Speed = speed;
            _properties = new TrackPlayerProperties();

            MovementKeyLeft = movementKeyLeft;
            MovementKeyRight = movementKeyRight;
            MovementKeySwapDirection = movementKeySwapDirection;
            GameObject = (GameObject) PrefabUtility.InstantiatePrefab(Resources.Load("PlayerPrefab"));
            GameObject.name = $"{typeof(TrackPlayer).Name}_{Name}";
        }

        public string Name { get; }
        public string MovementKeyLeft { get; }
        public string MovementKeyRight { get; }
        public string MovementKeySwapDirection { get; }

        public GameObject GameObject { get; }

        public PlayerDirection Direction { get; private set; }
        public float Speed { get; }

        public void Destroy()
        {
            Object.Destroy(GameObject);
        }

        public Vector3 GetPosition()
        {
            return GameObject.transform.position;
        }

        public void SetPosition(Vector3 position)
        {
            GameObject.transform.position = position;
        }

        public Quaternion GetRotation()
        {
            return GameObject.transform.rotation;
        }

//        public void SetRotation(Vector3 rotation)
//        {
//            GameObject.transform.localRotation = Quaternion.Euler(0,0, );
//        }
//        
        public void SetRotation(Vector3 angleDeg)
        {
            if (Direction == PlayerDirection.CounterClockwise) angleDeg.z = (angleDeg.z + 180) % 360;
            GameObject.transform.localRotation = Quaternion.Euler(angleDeg);
        }

        public void SwapDirection()
        {
            Direction = Direction == PlayerDirection.Clockwise
                ? PlayerDirection.CounterClockwise
                : PlayerDirection.Clockwise;
        }
    }
}