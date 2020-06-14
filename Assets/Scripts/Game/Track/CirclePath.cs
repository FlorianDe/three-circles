using System;
using Game.Player;
using UnityEngine;

namespace Game.Track
{
    internal class CirclePath : SinglePath
    {
        private readonly LineRenderer _lineRenderer;


        private int _size;
        private float _theta;
        public float Radius;
        public float ThetaScale = 0.01f;

        public CirclePath(float radius = 3f)
        {
            Radius = radius;

            GameObject = new GameObject($"{typeof(CirclePath).Name}_{Guid.NewGuid()}");
            _lineRenderer = GameObject.AddComponent<LineRenderer>();
            _lineRenderer.material = Resources.Load("LineRenderer_White_Hill_Material", typeof(Material)) as Material;
        }

        public override void Draw()
        {
            _theta = 0f;
            _size = (int) (1f / ThetaScale + 1f);
            _lineRenderer.positionCount = _size;
            for (var i = 0; i < _size; i++)
            {
                _theta += 2.0f * Mathf.PI * ThetaScale;
                var x = Radius * Mathf.Cos(_theta);
                var y = Radius * Mathf.Sin(_theta);
                _lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            }
        }

        public override Vector3 GetInitialPosition()
        {
            //Outsource to config/maybe angle->pos
            return new Vector3(Radius, 0, 0);
        }

        public override Vector3 NextPosition(TrackPlayer player)
        {
            var circumference = Mathf.PI * Radius * 2;
            var angleTransform = player.Speed / circumference;
            return Quaternion.AngleAxis((int) player.Direction * angleTransform, Vector3.forward) *
                   player.GameObject.transform.position;
        }

        public override Vector3 NextRotation(TrackPlayer player)
        {
            var circleCenter = GameObject.transform.position;
            var playerPos = player.GetPosition();
            var atan2 = Mathf.Atan2(playerPos.y - circleCenter.y, playerPos.x - circleCenter.x);
            var deg = atan2 * Mathf.Rad2Deg;


            var eulerRotVector3 = new Vector3(0, 0, deg);
            Debug.Log($"Player Pos: {playerPos} eulerRotVector3: {eulerRotVector3}");
            return eulerRotVector3;
        }

        public override Vector3 GetLaneSwapPosition(TrackPlayer player, SinglePath singlePath)
        {
            return Vector3.Normalize(player.GetPosition()) * Radius;
        }
    }
}