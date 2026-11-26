using FutharksAdventure.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FutharksAdventure.Tests
{
    public class DamageHandlerTests
    {
        private EventBus _bus;
        private OccupancyMap _occupancy;
        private DamageHandler _damageHandler;
        private Player _player;
        private Creature _enemy;

        private static readonly GridPosition PlayerCell = new GridPosition(2, 2);
        private static readonly GridPosition EnemyCell = new GridPosition(2, 3);

        [SetUp]
        public void SetUp()
        {
            var world = TestWorld.Create(TestFloors.CreateRoom3x3(), PlayerCell,
                TestFactory.CreatePlayer(maxHealth: 10));
            _bus = world.Bus;
            _occupancy = world.Occupancy;
            _damageHandler = world.DamageHandler;
            _player = world.Player;
            _enemy = world.PlaceEnemy(EnemyCell, maxHealth: 10);
        }

        // --- Survie ---

        [Test]
        public void ApplyDamage_ReducesHealth()
        {
            _damageHandler.ApplyDamage(_enemy, 3);

            Assert.That(_enemy.CurrentHealth, Is.EqualTo(7));
        }

        [Test]
        public void ApplyDamage_PublishesHealthChanged()
        {
            HealthChanged? received = null;
            _bus.Subscribe<HealthChanged>(e => received = e);

            _damageHandler.ApplyDamage(_enemy, 3);

            Assert.That(received.HasValue, Is.True);
            Assert.That(received.Value.Creature, Is.SameAs(_enemy));
            Assert.That(received.Value.Amount, Is.EqualTo(-3));
        }

        [Test]
        public void ApplyDamage_Overkill_HealthChangedCarriesFullAmount()
        {
            HealthChanged? received = null;
            _bus.Subscribe<HealthChanged>(e => received = e);

            _damageHandler.ApplyDamage(_enemy, 25);

            Assert.That(received.Value.Amount, Is.EqualTo(-25));
            Assert.That(_enemy.CurrentHealth, Is.EqualTo(0));
        }

        [Test] // coup valide sans degats : retour visuel necessaire
        public void ApplyDamage_Zero_PublishesHealthChangedAndChangesNothing()
        {
            HealthChanged? received = null;
            _bus.Subscribe<HealthChanged>(e => received = e);

            _damageHandler.ApplyDamage(_enemy, 0);

            Assert.That(received.HasValue, Is.True);
            Assert.That(received.Value.Creature, Is.SameAs(_enemy));
            Assert.That(received.Value.Amount, Is.EqualTo(0));
            Assert.That(_enemy.CurrentHealth, Is.EqualTo(10));
            Assert.That(_enemy.IsDead, Is.False);
        }

        [Test]
        public void ApplyDamage_Survives_StaysOnMapAndNoPlayerDied()
        {
            bool died = false;
            _bus.Subscribe<PlayerDied>(_ => died = true);

            _damageHandler.ApplyDamage(_player, 3);
            _damageHandler.ApplyDamage(_enemy, 3);

            Assert.That(died, Is.False);
            Assert.That(_occupancy.GetEntity(PlayerCell), Is.SameAs(_player));
            Assert.That(_occupancy.GetEntity(EnemyCell), Is.SameAs(_enemy));
        }

        // --- Mort d'un non-joueur ---

        [Test]
        public void ApplyDamage_KillsEnemy_RemovesItFromMap()
        {
            _damageHandler.ApplyDamage(_enemy, 10);

            Assert.That(_enemy.IsDead, Is.True);
            Assert.That(_occupancy.IsOccupied(EnemyCell), Is.False);
        }

        [Test]
        public void ApplyDamage_KillsEnemy_NoPlayerDiedAndPlayerStays()
        {
            bool died = false;
            _bus.Subscribe<PlayerDied>(_ => died = true);

            _damageHandler.ApplyDamage(_enemy, 10);

            Assert.That(died, Is.False);
            Assert.That(_occupancy.GetEntity(PlayerCell), Is.SameAs(_player));
        }

        // --- Mort du joueur ---

        [Test]
        public void ApplyDamage_KillsPlayer_PublishesPlayerDiedOnce()
        {
            int deaths = 0;
            _bus.Subscribe<PlayerDied>(_ => deaths++);

            _damageHandler.ApplyDamage(_player, 10);

            Assert.That(_player.IsDead, Is.True);
            Assert.That(deaths, Is.EqualTo(1));
        }

        [Test]
        public void ApplyDamage_KillsPlayer_PlayerStaysOnMap()
        {
            _damageHandler.ApplyDamage(_player, 10);

            Assert.That(_occupancy.GetEntity(PlayerCell), Is.SameAs(_player));
        }

        [Test]
        public void ApplyDamage_KillsPlayer_HealthChangedBeforePlayerDied()
        {
            var log = new List<string>();
            _bus.Subscribe<HealthChanged>(_ => log.Add("health"));
            _bus.Subscribe<PlayerDied>(_ => log.Add("died"));

            _damageHandler.ApplyDamage(_player, 10);

            Assert.That(log, Is.EqualTo(new[] { "health", "died" }));
        }

        // --- Bugs et gardes ---

        [Test] // cible deja morte = bug
        public void ApplyDamage_AlreadyDead_ThrowsAndPublishesNothing()
        {
            _damageHandler.ApplyDamage(_player, 10);
            bool published = false;
            _bus.Subscribe<HealthChanged>(_ => published = true);
            _bus.Subscribe<PlayerDied>(_ => published = true);

            Assert.That(() => _damageHandler.ApplyDamage(_player, 1), Throws.TypeOf<InvalidOperationException>());
            Assert.That(published, Is.False);
        }

        [Test]
        public void ApplyDamage_NegativeAmount_ThrowsAndPublishesNothing()
        {
            bool published = false;
            _bus.Subscribe<HealthChanged>(_ => published = true);

            Assert.That(() => _damageHandler.ApplyDamage(_enemy, -1), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(published, Is.False);
            Assert.That(_enemy.CurrentHealth, Is.EqualTo(10));
        }

        [Test]
        public void ApplyDamage_NullTarget_Throws()
        {
            Assert.That(() => _damageHandler.ApplyDamage(null, 1), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Constructor_NullEventBus_Throws()
        {
            Assert.That(() => new DamageHandler(null, _occupancy), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Constructor_NullOccupancyMap_Throws()
        {
            Assert.That(() => new DamageHandler(_bus, null), Throws.TypeOf<ArgumentNullException>());
        }
    }
}