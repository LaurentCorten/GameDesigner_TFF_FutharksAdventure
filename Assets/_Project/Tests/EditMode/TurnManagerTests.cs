using FutharksAdventure.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FutharksAdventure.Tests
{
    public class TurnManagerTests
    {
        private EventBus _bus;
        private OccupancyMap _occupancy;
        private Player _player;
        private TurnManager _turnManager;
        private IntentResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            var grid = TestFloors.CreateRoom3x3();
            _bus = new EventBus();
            _occupancy = new OccupancyMap();
            _player = TestFactory.CreatePlayer(10,10);
            _occupancy.Place(_player, new GridPosition(2, 3)); // contre le mur Nord

            _resolver = new IntentResolver(grid, _occupancy, _bus);
            _turnManager = new TurnManager(_player, _resolver, _bus);
        }

        [Test]
        public void SubmitMove_Valid_ReturnsTrueAndMovesPlayer()
        {
            bool accepted = _turnManager.SubmitMove(Direction.South);

            Assert.That(accepted, Is.True);
            Assert.That(_player.Position, Is.EqualTo(new GridPosition(2, 2)));
        }

        [Test]
        public void SubmitMove_Valid_PublishesTurnResolved()
        {
            bool resolved = false;
            _bus.Subscribe<TurnResolved>(_ => resolved = true);

            _turnManager.SubmitMove(Direction.South);

            Assert.That(resolved, Is.True);
        }

        [Test]
        public void SubmitMove_IntoWall_ReturnsFalseAndPublishesNoTurn()
        {
            bool resolved = false;
            _bus.Subscribe<TurnResolved>(_ => resolved = true);

            bool accepted = _turnManager.SubmitMove(Direction.North);

            Assert.That(accepted, Is.False);
            Assert.That(resolved, Is.False);
            Assert.That(_player.Position, Is.EqualTo(new GridPosition(2, 3)));
        }

        [Test]
        public void SubmitMove_Valid_PublishesEntityMovedBeforeTurnResolved()
        {
            var log = new List<string>();
            _bus.Subscribe<EntityMoved>(_ => log.Add("moved"));
            _bus.Subscribe<TurnResolved>(_ => log.Add("turn"));

            _turnManager.SubmitMove(Direction.South);

            Assert.That(log, Is.EqualTo(new[] { "moved", "turn" }));
        }

        [Test]
        public void Constructor_NullPlayer_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new TurnManager(null, _resolver, _bus));
        }

        [Test]
        public void Constructor_NullResolver_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new TurnManager(_player, null, _bus));
        }

        [Test]
        public void Constructor_NullEventBus_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new TurnManager(_player, _resolver, null));
        }

        private static readonly GridPosition SouthOfPlayer = new GridPosition(2, 2);

        [Test]
        public void SubmitMove_TowardEnemy_ReturnsTrueAndPlayerStays()
        {
            _occupancy.Place(TestFactory.CreateEnemy(), SouthOfPlayer);

            bool accepted = _turnManager.SubmitMove(Direction.South);

            Assert.That(accepted, Is.True);
            Assert.That(_player.Position, Is.EqualTo(new GridPosition(2, 3)));
        }

        [Test]
        public void SubmitMove_TowardEnemy_ConsumesATurn()
        {
            _occupancy.Place(TestFactory.CreateEnemy(), SouthOfPlayer);
            int foodBefore = _player.Food;
            bool resolved = false;
            _bus.Subscribe<TurnResolved>(_ => resolved = true);

            _turnManager.SubmitMove(Direction.South);

            Assert.That(_player.Food, Is.EqualTo(foodBefore - 1));
            Assert.That(resolved, Is.True);
        }

        [Test]
        public void SubmitMove_TowardEnemy_PublishesEnemyHealthChangedBeforeTurnResolved()
        {
            var enemy = TestFactory.CreateEnemy();
            _occupancy.Place(enemy, SouthOfPlayer);
            var log = new List<string>();
            _bus.Subscribe<HealthChanged>(e => { if (e.Creature == enemy) log.Add("enemyHit"); });
            _bus.Subscribe<TurnResolved>(_ => log.Add("turn"));

            _turnManager.SubmitMove(Direction.South);

            Assert.That(log, Is.EqualTo(new[] { "enemyHit", "turn" }));
        }

        [Test]
        public void SubmitMove_AfterKillingEnemy_PlayerCanWalkOnItsCell()
        {
            _occupancy.Place(TestFactory.CreateEnemy(maxHealth: _player.AttackDamage), SouthOfPlayer);

            _turnManager.SubmitMove(Direction.South); // tue l'ennemi
            bool moved = _turnManager.SubmitMove(Direction.South);

            Assert.That(moved, Is.True);
            Assert.That(_player.Position, Is.EqualTo(SouthOfPlayer));
        }
    }
}