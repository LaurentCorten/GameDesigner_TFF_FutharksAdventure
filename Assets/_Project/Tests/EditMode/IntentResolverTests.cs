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

        [SetUp]
        public void SetUp()
        {
            _occupancy = new OccupancyMap();
            _resolver = new IntentResolver(TestFloors.CreateRoom3x3(), _occupancy, new EventBus());
            _player = new Player(10,10);
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
    }
}