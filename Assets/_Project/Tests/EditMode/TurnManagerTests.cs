using FutharksAdventure.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FutharksAdventure.Tests
{
    public class TurnManagerTests
    {
        private TestWorld _world;
        private EventBus _bus;
        private Player _player;
        private TurnManager _turnManager;
        private IntentResolver _resolver;
        private DamageHandler _damageHandler;

        private static readonly GridPosition Start = new GridPosition(2, 3); // contre le mur Nord
        private static readonly GridPosition SouthOfPlayer = new GridPosition(2, 2);

        [SetUp]
        public void SetUp()
        {
            _world = TestWorld.Create(TestFloors.CreateRoom3x3(), Start);
            _bus = _world.Bus;
            _player = _world.Player;
            _resolver = _world.Resolver;
            _damageHandler = _world.DamageHandler;
            _turnManager = _world.TurnManager;
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
            Assert.That(_player.Position, Is.EqualTo(Start));
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
            Assert.Throws<ArgumentNullException>(() => new TurnManager(null, _resolver, _bus, _damageHandler));
        }

        [Test]
        public void Constructor_NullResolver_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new TurnManager(_player, null, _bus, _damageHandler));
        }

        [Test]
        public void Constructor_NullEventBus_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new TurnManager(_player, _resolver, null, _damageHandler));
        }

        [Test]
        public void Constructor_NullDamageHandler_Throws()
        {
            Assert.That(() => new TurnManager(_player, _resolver, _bus, null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void SubmitMove_TowardEnemy_ReturnsTrueAndPlayerStays()
        {
            _world.PlaceEnemy(SouthOfPlayer);

            bool accepted = _turnManager.SubmitMove(Direction.South);

            Assert.That(accepted, Is.True);
            Assert.That(_player.Position, Is.EqualTo(Start));
        }

        [Test]
        public void SubmitMove_TowardEnemy_ConsumesATurn()
        {
            _world.PlaceEnemy(SouthOfPlayer);
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
            var enemy = _world.PlaceEnemy(SouthOfPlayer);
            var log = new List<string>();
            _bus.Subscribe<HealthChanged>(e => { if (e.Creature == enemy) log.Add("enemyHit"); });
            _bus.Subscribe<TurnResolved>(_ => log.Add("turn"));

            _turnManager.SubmitMove(Direction.South);

            Assert.That(log, Is.EqualTo(new[] { "enemyHit", "turn" }));
        }

        [Test]
        public void SubmitMove_AfterKillingEnemy_PlayerCanWalkOnItsCell()
        {
            _world.PlaceEnemy(SouthOfPlayer, maxHealth: _player.AttackDamage);

            _turnManager.SubmitMove(Direction.South); // tue l'ennemi
            bool moved = _turnManager.SubmitMove(Direction.South);

            Assert.That(moved, Is.True);
            Assert.That(_player.Position, Is.EqualTo(SouthOfPlayer));
        }
    }
}