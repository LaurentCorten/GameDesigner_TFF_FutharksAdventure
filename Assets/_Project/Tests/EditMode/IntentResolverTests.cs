using FutharksAdventure.Core;
using NUnit.Framework;
using System;

namespace FutharksAdventure.Tests
{
    public class IntentResolverTests
    {
        private OccupancyMap _occupancy;
        private IntentResolver _resolver;
        private Player _player;
        private static readonly GridPosition Center = new GridPosition(2, 2);
        private static readonly GridPosition East = new GridPosition(3, 2);

        [SetUp]
        public void SetUp()
        {
            _occupancy = new OccupancyMap();
            _resolver = new IntentResolver(TestFloors.CreateRoom3x3(), _occupancy, new EventBus());
            _player = TestFactory.CreatePlayer(10, 10);
            _occupancy.Place(_player, new GridPosition(2, 2));
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
            Assert.Throws<ArgumentNullException>(() => new IntentResolver(null, _occupancy, new EventBus()));
        }

        [Test]
        public void Constructor_NullOccupancyMap_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new IntentResolver(TestFloors.CreateRoom3x3(), null, new EventBus()));
        }

        [Test]
        public void Constructor_NullEventBus_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new IntentResolver(TestFloors.CreateRoom3x3(), _occupancy, null));
        }

        [Test] // passera tout seul une fois le constructeur de MoveAction protege
        public void Resolve_NullEntity_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _resolver.Resolve(null, Direction.North));
        }
        [Test]
        public void Resolve_TowardCreature_ReturnsAttackAction()
        {
            _occupancy.Place(TestFactory.CreateEnemy(), East);

            Assert.That(_resolver.Resolve(_player, Direction.East), Is.InstanceOf<AttackAction>());
        }

        [Test]
        public void Resolve_TowardCreature_ExecutedAction_DamagesOccupantWithoutMoving()
        {
            var enemy = TestFactory.CreateEnemy(maxHealth: 10);
            _occupancy.Place(enemy, East);

            _resolver.Resolve(_player, Direction.East).TryExecute();

            Assert.That(enemy.CurrentHealth, Is.EqualTo(10 - _player.AttackDamage));
            Assert.That(_player.Position, Is.EqualTo(Center));
            Assert.That(enemy.Position, Is.EqualTo(East));
        }

        [Test]
        public void Resolve_Alone_ChangesNothing()
        {
            var enemy = TestFactory.CreateEnemy(maxHealth: 10);
            _occupancy.Place(enemy, East);

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