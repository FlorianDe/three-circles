using System.Collections.Generic;
using System.Linq;
using Game.Player;
using Game.Track;
using Menu;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(LineRenderer))]
    public class GameScript : MonoBehaviour
    {
        private const string LevelAssetsPath = "Levels";
        private readonly List<LevelDto> _levelsData = new List<LevelDto>();
        private RaceTrack _raceTrack;
        public MenuScript menuScript;

        public GameStatus Status { get; private set; }


        private void Awake()
        {
//        Application.targetFrameRate = 1000;

            foreach (var loadedAsset in Resources.LoadAll(LevelAssetsPath, typeof(TextAsset)))
                if (loadedAsset is TextAsset levelAsset)
                {
                    Debug.Log($"Trying to load level asset: {levelAsset.name}");
                    var levelDto = JsonUtility.FromJson<LevelDto>(levelAsset.text);
                    _levelsData.Add(levelDto);
                }
                else
                {
                    Debug.LogError("The level assets couldn't be load.");
                }
        }

        private void Start()
        {
            Status = GameStatus.None;
        }

        private void Update()
        {
            HandleInputs();
            UpdateElements();
            DrawElements();
        }

        public void StartGame()
        {
            var levelDto = _levelsData.Find(levelData => levelData.name == "Level 01 - Test");
            var circlePaths = levelDto?.radii
                .Select(radius => new CirclePath(radius))
                .ToList<SinglePath>();

            var players = new List<TrackPlayer>
            {
                new TrackPlayer("Player 1", "q", "w", "e"),
                new TrackPlayer("Player 2", "o", "p", "l")
            };
            _raceTrack = new RaceTrack(players, circlePaths);

            Status = GameStatus.Running;
        }

        public void HandleInputs()
        {
            if (_raceTrack?.Players != null)
                foreach (var player in _raceTrack?.Players)
                    if (Input.GetKeyDown(player.MovementKeyLeft))
                        _raceTrack.MoveLeft(player);
                    else if (Input.GetKeyDown(player.MovementKeyRight))
                        _raceTrack.MoveRight(player);
                    else if (Input.GetKeyDown(player.MovementKeySwapDirection)) player.SwapDirection();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _raceTrack?.Destroy();
                _raceTrack = null;
                Status = GameStatus.None;
                menuScript.Show();
            }
        }

        public void UpdateElements()
        {
            _raceTrack?.UpdatePlayerPositions();
        }

        public void DrawElements()
        {
            _raceTrack?.Draw();
        }
    }
}