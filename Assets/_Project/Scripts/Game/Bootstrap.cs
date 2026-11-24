using FutharksAdventure.Core;
using System;
using Unity.Cinemachine;
using UnityEngine;

namespace FutharksAdventure.Game
{
    /// <summary>
    /// Single entry point of the game (Composition Root):
    /// the only place that creates objects and wires them together.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private GridView _gridView;
        [SerializeField] private EntityView _playerPrefab;
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private PlayerConfig _playerConfig;

        [Header("TEMPORARY DEBUG")]
        [SerializeField] private GameCheck _gameCheck;

        private EventBus _eventBus;
        private FutharksInputMap _inputMap;
        private FloorGrid _floorGrid;
        private Player _player;
        private OccupancyMap _occupancyMap;
        private IntentResolver _intentResolver;
        private TurnManager _turnManager;
        private InputController _inputController;

        private void Awake()
        {
            _inputMap = new FutharksInputMap();
            _player = InstantiatePlayer();

            _eventBus = new EventBus();
            _occupancyMap = new OccupancyMap();
            _floorGrid = BuildTestFloor();
            _intentResolver = new IntentResolver(_floorGrid, _occupancyMap, _eventBus);
            _turnManager = new TurnManager(_player, _intentResolver, _eventBus);
            _inputController = new InputController(_inputMap, _turnManager);

            // TEMPORARY
            if(_gameCheck != null)
                _gameCheck.Init(_eventBus, _player);
            Debug.Log($"Player has now {_player.CurrentHealth}/{_player.MaxHealth} PV, {_player.Food} pieces of food and {_player.Hatchets} hatchets.");
        }

        private void Start()
        {
            _gridView.Render(_floorGrid);

            #region TEMPORARY: Initialisation du player
            GridPosition entryGridPosition = GetEntryPosition();
            _occupancyMap.Place(_player, entryGridPosition);
            EntityView playerEntityView = Instantiate(_playerPrefab);
            playerEntityView.Init(_player, _eventBus, _gridView);
            _cinemachineCamera.Target.TrackingTarget = playerEntityView.transform; 
            #endregion

            _inputController.Enable();
        }

        private void OnDestroy()
        {
            _inputController.Disable();
            _inputMap.Dispose();
        }

        // TEMPORARY: hard-coded test floor, replaced by the procedural generator in Phase 5
        private static FloorGrid BuildTestFloor()
        {
            // Instanciate a grid big enough
            var floor = new FloorGrid(21, 11);

            // Carve the 2 rooms in it
            RoomCarver.CarveRoom(floor, new GridPosition(4, 5), 5, 5, Sides.East);
            RoomCarver.CarveRoom(floor, new GridPosition(16, 5), 7, 3, Sides.West);

            // Carve the link between them
            for (int x = 8; x < 12; x++)
            { 
                floor.SetTileType(new GridPosition(x, 4), TileType.Wall);
                floor.SetTileType(new GridPosition(x, 5), TileType.Floor);
                floor.SetTileType(new GridPosition(x, 6), TileType.Wall);
            }

            // Carve Entry and Exit
            floor.SetTileType(new GridPosition(3, 5), TileType.Entry);
            floor.SetTileType(new GridPosition(18, 5), TileType.Exit);

            return floor;
        }

        private Player InstantiatePlayer()
        {
            if (_playerConfig == null)
                throw new ArgumentNullException(nameof(_playerConfig));

            return new Player(_playerConfig.MaxHealth, _playerConfig.Food, _playerConfig.Hatchets, _playerConfig.VisionRange);
        }

        // TEMPORARY
        private GridPosition GetEntryPosition()
        {
            for (int x = 0; x < _floorGrid.Width; x++)
                for (int y = 0; y < _floorGrid.Height; y++)
                {
                    TileType tileType = _floorGrid.GetTileType(new GridPosition(x, y));
                    if(tileType == TileType.Entry)
                    {
                        return new GridPosition(x, y);
                    }
                }
            throw new InvalidOperationException("Pas de TileType.Entry dans le grid !");
        }
    }
}