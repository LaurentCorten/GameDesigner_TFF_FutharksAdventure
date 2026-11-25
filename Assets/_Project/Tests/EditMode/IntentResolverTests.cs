using FutharksAdventure.Core;
using NUnit.Framework;
using System;

namespace FutharksAdventure.Tests
{
    public class IntentResolverTests
    {
        private TestWorld _world;
        private FloorGrid _grid;
        private OccupancyMap _occupancy;
        private EventBus _bus;
        private DamageHandler _damageHandler;
        private IntentResolver _resolver;
        private Player _player;

        private static readonly GridPosition Center = new GridPosition(2, 2);
        private static readonly GridPosition East = new GridPosition(3, 2);

        [SetUp]
        public void SetUp()
        {
            _world = TestWorld.Create(TestFloors.CreateRoom3x3(), Center);
            _grid = _world.Grid;
            _occupancy = _world.Occupancy;
            _bus = _world.Bus;
            _damageHandler = _world.DamageHandler;
            _resolver = _world.Resolver;
            _player = _world.Player;
        }

        [Test]
        public void Resolve_Direction_ReturnsMoveAction()
        {
            Assert.That(_resolver.Resolve(_player, Direction.East), Is.InstanceOf<MoveAction>());
        }

        [Test]
        public void Resolve_ReturnedAction_MovesGivenEntityInGivenDirection()
        {
            _resolver.Resolve(_player, Direction.East).TryExecute();

            Assert.That(_player.Position, Is.EqualTo(new GridPosition(3, 2)));
        }

        [Test]
        public void Constructor_NullGrid_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new IntentResolver(null, _occupancy, _bus, _damageHandler));
        }

        [Test]
        public void Constructor_NullOccupancyMap_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new IntentResolver(_grid, null, _bus, _damageHandler));
        }

        [Test]
        public void Constructor_NullEventBus_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new IntentResolver(_grid, _occupancy, null, _damageHandler));
        }

        [Test]
        public void Constructor_NullDamageHandler_Throws()
        {
            Assert.That(() => new IntentResolver(_grid, _occupancy, _bus, null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Resolve_NullEntity_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _resolver.Resolve(null, Direction.North));
        }

        [Test]
        public void Resolve_TowardCreature_ReturnsAttackAction()
        {
            _world.PlaceEnemy(East);

            Assert.That(_resolver.Resolve(_player, Direction.East), Is.InstanceOf<AttackAction>());
        }

        [Test]
        public void Resolve_TowardCreature_ExecutedAction_DamagesOccupantWithoutMoving()
        {
            var enemy = _world.PlaceEnemy(East, maxHealth: 10);

            _resolver.Resolve(_player, Direction.East).TryExecute();

            Assert.That(enemy.CurrentHealth, Is.EqualTo(10 - _player.AttackDamage));
            Assert.That(_player.Position, Is.EqualTo(Center));
            Assert.That(enemy.Position, Is.EqualTo(East));
        }

        [Test]
        public void Resolve_Alone_ChangesNothing()
        {
            var enemy = _world.PlaceEnemy(East, maxHealth: 10);

            _resolver.Resolve(_player, Direction.East);

            Assert.That(enemy.CurrentHealth, Is.EqualTo(10));
            Assert.That(_player.Position, Is.EqualTo(Center));
        }

        [Test]
        public void Resolve_TowardWall_ReturnsActionThatRefuses()
        {
            _occupancy.Move(_player, East); // (4,2) est un mur

            GameAction action = _resolver.Resolve(_player, Direction.East);

            Assert.That(action, Is.Not.Null);
            Assert.That(action.CanExecute(), Is.False);
        }

        [TestCase(Direction.North)]
        [TestCase(Direction.East)]
        [TestCase(Direction.South)]
        [TestCase(Direction.West)]
        public void Resolve_EveryDefinedDirection_ReturnsAnAction(Direction direction)
        {
            Assert.That(_resolver.Resolve(_player, direction), Is.Not.Null);
        }

        [Test] // remplace l'ancien MoveActionTests.Constructor_UndefinedDirection_Throws
        public void Resolve_UndefinedDirection_ThrowsAndChangesNothing()
        {
            Assert.That(() => _resolver.Resolve(_player, (Direction)99),
                Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(_player.Position, Is.EqualTo(Center));
            Assert.That(_occupancy.GetEntity(Center), Is.SameAs(_player));
        }
    }
}