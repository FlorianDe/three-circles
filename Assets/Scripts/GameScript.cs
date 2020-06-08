using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


[RequireComponent(typeof(LineRenderer))]
public class GameScript : MonoBehaviour
{
    public MenuScript menuScript;

    private const String LevelAssetsPath = "Levels";

    public GameStatus Status { get; private set; }
    private RaceTrack _raceTrack;
    private readonly List<LevelDto> _levelsData = new List<LevelDto>();


    public enum GameStatus
    {
        None,
        Running,
        Paused,
        Finished
    }

    private void Awake()
    {
//        Application.targetFrameRate = 1000;

        foreach (var loadedAsset in Resources.LoadAll(LevelAssetsPath, typeof(TextAsset)))
        {
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
    }

    void Start()
    {
        Status = GameStatus.None;
    }

    void Update()
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

        var player1 = new Player("Player 1", "q", "w", "e");
        var player2 = new Player("Player 2", "o", "p", "l");
        
        _raceTrack = new RaceTrack(new List<Player>{player1, player2}, circlePaths);

        Status = GameStatus.Running;
    }

    public void HandleInputs()
    {
        if (_raceTrack?.Players != null){
            foreach (var player in _raceTrack?.Players)
            {
                if (Input.GetKeyDown(player.MovementKeyLeft))
                {
                    _raceTrack.MoveLeft(player);
                }
                else if (Input.GetKeyDown(player.MovementKeyRight))
                {
                    _raceTrack.MoveRight(player);
                }
                else if (Input.GetKeyDown(player.MovementKeySwapDirection))
                {
                    player.SwapDirection();
                }
            }
        }

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

[Serializable]
class LevelDto
{
    public String name;
    public List<float> radii;
}


class RaceTrack
{
    public List<Player> Players { get; private set; }
    private LinkedList<SinglePath> _paths;
    private Dictionary<Player, SinglePath> _playerPathMapping = new Dictionary<Player, SinglePath>();

    public RaceTrack(List<Player> players, List<SinglePath> paths)
    {
        Players = players;
        _paths = new LinkedList<SinglePath>(paths);

        AssignPlayersToFirstLane(Players);
    }

    public void AddPlayers(List<Player> players)
    {
        AssignPlayersToFirstLane(players);
    }

    public void AddPath(SinglePath path)
    {
        _paths.AddLast(path);
    }

    private void AssignPlayersToFirstLane(List<Player> players)
    {
        var playerLane = _paths.First.Value;
        if (playerLane != null)
        {
            foreach (var player in players)
            {
                _playerPathMapping[player] = playerLane;
                player.SetPosition(playerLane.GetInitialPosition());
            }
        }
    }

    public void Draw()
    {
        foreach (var path in _paths)
        {
            path.Draw();
        }
    }

    public void MoveLeft(Player player)
    {
        Move(player, currentTrack => currentTrack?.Previous);
    }

    public void MoveRight(Player player)
    {
        Move(player, currentTrack => currentTrack?.Next);
    }

    private void Move(Player player, Func<LinkedListNode<SinglePath>, LinkedListNode<SinglePath>> movement)
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
        {
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
    }

    public void Destroy()
    {
        foreach (var player in Players)
        {
            player.Destroy();
        }

        Players = null;

        foreach (var path in _paths)
        {
            path.Destroy();
        }

        _paths = null;
    }
}

public class Player
{
    public enum PlayerDirection
    {
        Clockwise = 1,
        CounterClockwise = -1
    }


    public string Name { get; }
    public string MovementKeyLeft { get; }
    public string MovementKeyRight { get; }
    public string MovementKeySwapDirection { get; }

    public GameObject GameObject { get; }

    public PlayerDirection Direction { get; private set; }
    public float Speed { get; }
    private PlayerProperties _properties;

    public Player(string name, string movementKeyLeft, string movementKeyRight, string movementKeySwapDirection)
    {
        Name = name;
        Direction = PlayerDirection.Clockwise;
        Speed = 10.0f;
        _properties = new PlayerProperties();

        MovementKeyLeft = movementKeyLeft;
        MovementKeyRight = movementKeyRight;
        MovementKeySwapDirection = movementKeySwapDirection;
        GameObject = (GameObject) PrefabUtility.InstantiatePrefab(Resources.Load("PlayerPrefab"));
        GameObject.name = $"{typeof(Player).Name}_{Name}";
    }

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

    public void SetRotation(Vector3 rotation)
    {
        GameObject.transform.eulerAngles = rotation;
    }

    public void SwapDirection()
    {
        Direction = (Direction == PlayerDirection.Clockwise)
            ? PlayerDirection.CounterClockwise
            : PlayerDirection.Clockwise;
    }
}

class PlayerProperties
{
}

abstract class SinglePath
{
    protected GameObject GameObject;
    public abstract void Draw();
    public abstract Vector3 GetInitialPosition();
    public abstract Vector3 NextPosition(Player player);

    public abstract Vector3 GetLaneSwapPosition(Player player, SinglePath singlePath);

    public void Destroy()
    {
        Object.Destroy(GameObject);
    }

    public abstract Vector3 NextRotation(Player player);
}

class CirclePath : SinglePath
{
    public float ThetaScale = 0.01f;
    public float Radius;


    private int _size;
    private readonly LineRenderer _lineRenderer;
    private float _theta;

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
        _size = (int) ((1f / ThetaScale) + 1f);
        _lineRenderer.positionCount = _size;
        for (int i = 0; i < _size; i++)
        {
            _theta += (2.0f * Mathf.PI * ThetaScale);
            float x = Radius * Mathf.Cos(_theta);
            float y = Radius * Mathf.Sin(_theta);
            _lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    public override Vector3 GetInitialPosition()
    {
        //Outsource to config/maybe angle->pos
        return new Vector3(Radius, 0, 0);
    }

    public override Vector3 NextPosition(Player player)
    {
        var circumference = Mathf.PI * Radius * 2;
        var angleTransform = (player.Speed / circumference);
        return Quaternion.AngleAxis((int)player.Direction * angleTransform, Vector3.forward) * player.GameObject.transform.position;
    }
    
    public override Vector3 NextRotation(Player player)
    {
        return Quaternion.AngleAxis(Mathf.Atan2(player.GetPosition().y, player.GetPosition().x)*Mathf.Rad2Deg, Vector3.forward) * Vector3.right * Mathf.Rad2Deg;
    }

    public override Vector3 GetLaneSwapPosition(Player player, SinglePath singlePath)
    {
        return Vector3.Normalize(player.GetPosition()) * Radius;
    }
}