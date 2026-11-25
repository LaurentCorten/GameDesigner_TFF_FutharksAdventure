using FutharksAdventure.Core;
using NUnit.Framework;
using System;

namespace FutharksAdventure.Tests
{
    public class MoveActionTests
    {
        private FloorGrid _grid;
        private OccupancyMap _occupancy;
        private EventBus _bus;
        private Player _player;

        private static readonly GridPosition Center = new GridPosition(2, 2);
        private static readonly GridPosition NorthOfCenter = new GridPosition(2, 3);

        [SetUp]
        public void SetUp()
        {
            _grid = TestFloors.CreateRoom3x3();
            _occupancy = new OccupancyMap();
            _bus = new EventBus();
            _player = TestFactory.CreatePlayer(10, 10);
            _occupancy.Place(_player, Center);
        }

        private MoveAction CreateMove(Direction direction) =>
            new MoveAction(_player, _player.Position, _player.Position + direction.ToGridOffset(), _grid, _occupancy, _bus);

        [Test]
        public void CanExecute_TowardFloor_ReturnsTrue()
        {
            Assert.That(CreateMove(Direction.North).CanExecute(), Is.True);
        }

        [Test]
        public void CanExecute_TowardWall_ReturnsFalse()
        {
            _occupancy.Move(_player, NorthOfCenter); // (2,4) est un mur

            Assert.That(CreateMove(Direction.North).CanExecute(), Is.False);
        }

        [Test]
        public void CanExecute_TowardOccupiedCell_ReturnsFalse()
        {
            _occupancy.Place(TestFactory.CreatePlayer(10, 10), NorthOfCenter);

            Assert.That(CreateMove(Direction.North).CanExecute(), Is.False);
        }

        [Test]
        public void CanExecute_DoesNotMoveEntity()
        {
            CreateMove(Direction.North).CanExecute();

            Assert.That(_player.Position, Is.EqualTo(Center));
        }

        [Test]
        public void TryExecute_TowardFloor_ReturnsTrueAndMovesEntity()
        {
            bool accepted = CreateMove(Direction.North).TryExecute();

            Assert.That(accepted, Is.True);
            Assert.That(_player.Position, Is.EqualTo(NorthOfCenter));
        }

        [Test]
        public void TryExecute_TowardWall_ReturnsFalseAndDoesNotMove()
        {
            _occupancy.Move(_player, NorthOfCenter);

            bool accepted = CreateMove(Direction.North).TryExecute();

            Assert.That(accepted, Is.False);
            Assert.That(_player.Position, Is.EqualTo(NorthOfCenter));
        }

        [Test]
        public void TryExecute_Success_PublishesEntityMoved()
        {
            EntityMoved? received = null;
            _bus.Subscribe<EntityMoved>(e => received = e);

            CreateMove(Direction.North).TryExecute();

            Assert.That(received.HasValue, Is.True);
            Assert.That(received.Value.Entity, Is.SameAs(_player));
            Assert.That(received.Value.From, Is.EqualTo(Center));
            Assert.That(received.Value.To, Is.EqualTo(NorthOfCenter));
        }

        [Test]
        public void TryExecute_Refused_PublishesNothing()
        {
            _occupancy.Move(_player, NorthOfCenter);
            bool published = false;
            _bus.Subscribe<EntityMoved>(_ => published = true);

            CreateMove(Direction.North).TryExecute();

            Assert.That(published, Is.False);
        }

        [Test]
        public void TryExecute_AfterEntityMovedElsewhere_Throws()
        {
            var move = CreateMove(Direction.North);
            _occupancy.Move(_player, new GridPosition(1, 2));

            Assert.Throws<InvalidOperationException>(() => move.TryExecute());
        }

        [Test] // le trou decrit plus haut
        public void TryExecute_AfterEntityMovedElsewhere_EvenIfTargetOccupied_Throws()
        {
            var move = CreateMove(Direction.North);             // cible (2,3)
            _occupancy.Move(_player, new GridPosition(1, 2));
            _occupancy.Place(TestFactory.CreatePlayer(10, 10), NorthOfCenter);      // la cible est maintenant occupee

            Assert.Throws<InvalidOperationException>(() => move.TryExecute());
        }

        [Test]
        public void TryExecute_EntityNotPlaced_Throws()
        {
            var grid = new FloorGrid(1, 2);
            grid.SetTileType(new GridPosition(0, 1), TileType.Floor);
            var unplaced = TestFactory.CreatePlayer(10, 10); // Position par defaut (0,0), absente de la carte

            var move = new MoveAction(unplaced, unplaced.Position, unplaced.Position + Direction.North.ToGridOffset(), grid, new OccupancyMap(), _bus);

            Assert.Throws<InvalidOperationException>(() => move.TryExecute());
        }

        [Test]
        public void CanExecute_TowardOutsideGrid_ReturnsFalse()
        {
            var grid = new FloorGrid(1, 1);
            grid.SetTileType(new GridPosition(0, 0), TileType.Floor);
            var occupancy = new OccupancyMap();
            var player = TestFactory.CreatePlayer(10, 10);
            occupancy.Place(player, new GridPosition(0, 0));

            var move = new MoveAction(player, player.Position, player.Position+Direction.West.ToGridOffset(), grid, occupancy, _bus);

            Assert.That(move.CanExecute(), Is.False);
        }

        [Test]
        public void Constructor_NullEntity_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MoveAction(null, new GridPosition(0,0), new GridPosition(0,1), _grid, _occupancy, _bus));
        }

        [Test]
        public void Constructor_NullGrid_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MoveAction(_player, _player.Position, _player.Position + Direction.North.ToGridOffset(), null, _occupancy, _bus));
        }

        [Test]
        public void Constructor_NullOccupancyMap_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MoveAction(_player, _player.Position, _player.Position + Direction.North.ToGridOffset(), _grid, null, _bus));
        }

        [Test]
        public void Constructor_NullEventBus_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MoveAction(_player, _player.Position, _player.Position + Direction.North.ToGridOffset(), _grid, _occupancy, null));
        }
    }
}