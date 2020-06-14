using System;
using System.Collections.Generic;
using Game.Player;

namespace Game.Track
{
    internal class RaceTrack
    {
        private LinkedList<SinglePath> _paths;

        private readonly Dictionary<TrackPlayer, SinglePath> _playerPathMapping =
            new Dictionary<TrackPlayer, SinglePath>();

        public RaceTrack(List<TrackPlayer> players, List<SinglePath> paths)
        {
            Players = players;
            _paths = new LinkedList<SinglePath>(paths);

            AssignPlayersToFirstLane(Players);
        }

        public List<TrackPlayer> Players { get; private set; }

        public void AddPlayers(List<TrackPlayer> players)
        {
            AssignPlayersToFirstLane(players);
        }

        public void AddPath(SinglePath path)
        {
            _paths.AddLast(path);
        }

        private void AssignPlayersToFirstLane(List<TrackPlayer> players)
        {
            var playerLane = _paths.First.Value;
            if (playerLane != null)
                foreach (var player in players)
                {
                    _playerPathMapping[player] = playerLane;
                    player.SetPosition(playerLane.GetInitialPosition());
                }
        }

        public void Draw()
        {
            foreach (var path in _paths) path.Draw();
        }

        public void MoveLeft(TrackPlayer player)
        {
            Move(player, currentTrack => currentTrack?.Previous);
        }

        public void MoveRight(TrackPlayer player)
        {
            Move(player, currentTrack => currentTrack?.Next);
        }

        private void Move(TrackPlayer player, Func<LinkedListNode<SinglePath>, LinkedListNode<SinglePath>> movement)
        {
            SinglePath pathForPlayerOnTrack;
            if (_playerPathMapping.TryGetValue(player, out pathForPlayerOnTrack))
            {
                var currentTrack = _paths.Find(pathForPlayerOnTrack);
                if (currentTrack != null)
                {
                    var nextTrack = movement.Invoke(currentTrack);
                    if (nextTrack != null)
                    {
                        _playerPathMapping[player] = nextTrack.Value;
                        player.SetPosition(nextTrack.Value.GetLaneSwapPosition(player, currentTrack.Value));
                    }
                }
            }
        }


        public void UpdatePlayerPositions()
        {
            if (Players != null)
                foreach (var player in Players)
                {
                    SinglePath currentPath;
                    _playerPathMapping.TryGetValue(player, out currentPath);
                    if (currentPath != null)
                    {
                        var nextPosition = currentPath.NextPosition(player);
                        var nextRotation = currentPath.NextRotation(player);
                        player.SetPosition(nextPosition);
                        player.SetRotation(nextRotation);
                    }
                }
        }

        public void Destroy()
        {
            foreach (var player in Players) player.Destroy();

            Players = null;

            foreach (var path in _paths) path.Destroy();

            _paths = null;
        }
    }
}