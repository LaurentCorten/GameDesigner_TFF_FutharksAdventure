using FutharksAdventure.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FutharksAdventure.Tests
{
    public class AttackActionTests
    {
        private OccupancyMap _occupancy;
        private EventBus _bus;
        private Player _player;
        private Creature _enemy;

        private static readonly GridPosition PlayerCell = new GridPosition(2, 2);
        private static readonly GridPosition NorthOfPlayer = new GridPosition(2, 3);
        private static readonly GridPosition EastOfPlayer = new GridPosition(3, 2);
        private static readonly GridPosition WestOfPlayer = new GridPosition(1, 2);

        [SetUp]
        public void SetUp()
        {
            _occupancy = new OccupancyMap();
            _bus = new EventBus();
            _player = TestFactory.CreatePlayer(maxHealth: 10, atkDmg: 4);
            _enemy = TestFactory.CreateEnemy(maxHealth: 10, atkDmg: 3);
            _occupancy.Place(_player, PlayerCell);
            _occupancy.Place(_enemy, NorthOfPlayer);
        }

        private AttackAction CreateAttack(Creature attacker, Creature target) =>
            new AttackAction(_bus, _occupancy, attacker, target, attacker.Position, target.Position);

        private Creature PlaceWeakEnemy(GridPosition cell)
        {
            var weak = TestFactory.CreateEnemy(maxHealth: 4); // meurt en un coup du joueur (4 degats)
            _occupancy.Place(weak, cell);
            return weak;
        }

        // --- CanExecute ---

        [Test]
        public void CanExecute_AdjacentLivingTarget_ReturnsTrue()
        {
            Assert.That(CreateAttack(_player, _enemy).CanExecute(), Is.True);
        }

        [Test]
        public void CanExecute_DoesNotDamageTarget()
        {
            CreateAttack(_player, _enemy).CanExecute();

            Assert.That(_enemy.CurrentHealth, Is.EqualTo(10));
        }

        // --- Execution, cible survivante ---

        [Test]
        public void TryExecute_ReturnsTrueAndRemovesAttackDamageFromTarget()
        {
            bool accepted = CreateAttack(_player, _enemy).TryExecute();

            Assert.That(accepted, Is.True);
            Assert.That(_enemy.CurrentHealth, Is.EqualTo(10 - _player.AttackDamage));
        }

        [Test]
        public void TryExecute_DoesNotDamageAttacker()
        {
            CreateAttack(_player, _enemy).TryExecute();

            Assert.That(_player.CurrentHealth, Is.EqualTo(10));
        }

        [Test]
        public void TryExecute_NobodyMoves()
        {
            CreateAttack(_player, _enemy).TryExecute();

            Assert.That(_player.Position, Is.EqualTo(PlayerCell));
            Assert.That(_enemy.Position, Is.EqualTo(NorthOfPlayer));
            Assert.That(_occupancy.GetEntity(PlayerCell), Is.SameAs(_player));
            Assert.That(_occupancy.GetEntity(NorthOfPlayer), Is.SameAs(_enemy));
        }

        [Test]
        public void TryExecute_PublishesHealthChangedForTarget()
        {
            HealthChanged? received = null;
            _bus.Subscribe<HealthChanged>(e => received = e);

            CreateAttack(_player, _enemy).TryExecute();

            Assert.That(received.HasValue, Is.True);
            Assert.That(received.Value.Creature, Is.SameAs(_enemy));
            Assert.That(received.Value.Amount, Is.EqualTo(-_player.AttackDamage));
        }

        [Test]
        public void TryExecute_TargetSurvives_PublishesNoPlayerDied()
        {
            bool died = false;
            _bus.Subscribe<PlayerDied>(_ => died = true);

            CreateAttack(_enemy, _player).TryExecute();

            Assert.That(died, Is.False);
            Assert.That(_player.IsDead, Is.False);
        }

        // --- Execution, cible tuee (ennemi) ---

        [Test]
        public void TryExecute_KillsEnemy_RemovesItFromMap()
        {
            var weak = PlaceWeakEnemy(EastOfPlayer);

            CreateAttack(_player, weak).TryExecute();

            Assert.That(weak.IsDead, Is.True);
            Assert.That(_occupancy.IsOccupied(EastOfPlayer), Is.False);
            Assert.That(_occupancy.GetEntity(EastOfPlayer), Is.Null);
        }

        [Test]
        public void TryExecute_KillsEnemy_PublishesNoPlayerDied()
        {
            var weak = PlaceWeakEnemy(EastOfPlayer);
            bool died = false;
            _bus.Subscribe<PlayerDied>(_ => died = true);

            CreateAttack(_player, weak).TryExecute();

            Assert.That(died, Is.False);
        }

        [Test]
        public void TryExecute_KillsEnemy_OtherEntitiesStayOnMap()
        {
            var weak = PlaceWeakEnemy(EastOfPlayer);

            CreateAttack(_player, weak).TryExecute();

            Assert.That(_occupancy.GetEntity(PlayerCell), Is.SameAs(_player));
            Assert.That(_occupancy.GetEntity(NorthOfPlayer), Is.SameAs(_enemy));
        }

        // --- Execution, cible tuee (joueur) ---

        [Test]
        public void TryExecute_KillsPlayer_PublishesPlayerDiedOnce()
        {
            var killer = TestFactory.CreateEnemy(atkDmg: 10);
            _occupancy.Place(killer, WestOfPlayer);
            int deaths = 0;
            _bus.Subscribe<PlayerDied>(_ => deaths++);

            CreateAttack(killer, _player).TryExecute();

            Assert.That(_player.IsDead, Is.True);
            Assert.That(deaths, Is.EqualTo(1));
        }

        [Test]
        public void TryExecute_KillsPlayer_PlayerStaysOnMap()
        {
            var killer = TestFactory.CreateEnemy(atkDmg: 10);
            _occupancy.Place(killer, WestOfPlayer);

            CreateAttack(killer, _player).TryExecute();

            Assert.That(_occupancy.GetEntity(PlayerCell), Is.SameAs(_player));
        }

        [Test]
        public void TryExecute_KillsPlayer_PublishesHealthChangedBeforePlayerDied()
        {
            var killer = TestFactory.CreateEnemy(atkDmg: 10);
            _occupancy.Place(killer, WestOfPlayer);
            var log = new List<string>();
            _bus.Subscribe<HealthChanged>(_ => log.Add("health"));
            _bus.Subscribe<PlayerDied>(_ => log.Add("died"));

            CreateAttack(killer, _player).TryExecute();

            Assert.That(log, Is.EqualTo(new[] { "health", "died" }));
        }

        // --- Actions perimees (bug = exception) ---

        [Test]
        public void TryExecute_AttackerMovedSinceCreation_Throws()
        {
            var attack = CreateAttack(_player, _enemy);
            _occupancy.Move(_player, EastOfPlayer);

            Assert.That(() => attack.TryExecute(), Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void TryExecute_TargetMovedSinceCreation_Throws()
        {
            var attack = CreateAttack(_player, _enemy);
            _occupancy.Move(_enemy, new GridPosition(3, 3));

            Assert.That(() => attack.TryExecute(), Throws.TypeOf<InvalidOperationException>());
        }

        [Test] // cible deja morte = action perimee
        public void TryExecute_TargetAlreadyDead_ReturnsFalseAndPublishesNothing()
        {
            var weak = PlaceWeakEnemy(EastOfPlayer);
            var first = CreateAttack(_player, weak);
            var second = CreateAttack(_player, weak);
            first.TryExecute();
            bool published = false;
            _bus.Subscribe<HealthChanged>(_ => published = true);

            bool accepted = second.TryExecute();

            Assert.That(second.CanExecute(), Is.False);
            Assert.That(accepted, Is.False);
            Assert.That(published, Is.False);
        }

        [Test] // deux attaques sur le joueur dans le meme tour
        public void TryExecute_TwoAttacksOnPlayer_PlayerDiedPublishedOnce()
        {
            var killer1 = TestFactory.CreateEnemy(atkDmg: 10);
            var killer2 = TestFactory.CreateEnemy(atkDmg: 10);
            _occupancy.Place(killer1, WestOfPlayer);
            _occupancy.Place(killer2, EastOfPlayer);
            var attack1 = CreateAttack(killer1, _player);
            var attack2 = CreateAttack(killer2, _player);
            int deaths = 0;
            _bus.Subscribe<PlayerDied>(_ => deaths++);

            attack1.TryExecute();
            bool secondAccepted = attack2.TryExecute();

            Assert.That(secondAccepted, Is.False);
            Assert.That(deaths, Is.EqualTo(1));
        }

        // --- OverKill ---
        [Test]
        public void TryExecute_Overkill_HealthChangedCarriesFullDamage()
        {
            var fragile = TestFactory.CreateEnemy(maxHealth: 2);
            _occupancy.Place(fragile, EastOfPlayer);
            HealthChanged? received = null;
            _bus.Subscribe<HealthChanged>(e => received = e);

            CreateAttack(_player, fragile).TryExecute(); // 4 degats sur 2 PV

            Assert.That(received.Value.Amount, Is.EqualTo(-_player.AttackDamage));
            Assert.That(fragile.CurrentHealth, Is.EqualTo(0));
        }


        // --- Constructeur ---

        [Test]
        public void Constructor_NullEventBus_Throws()
        {
            Assert.That(() => new AttackAction(null, _occupancy, _player, _enemy, PlayerCell, NorthOfPlayer),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Constructor_NullOccupancyMap_Throws()
        {
            Assert.That(() => new AttackAction(_bus, null, _player, _enemy, PlayerCell, NorthOfPlayer),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Constructor_NullAttacker_Throws()
        {
            Assert.That(() => new AttackAction(_bus, _occupancy, null, _enemy, PlayerCell, NorthOfPlayer),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Constructor_NullTarget_Throws()
        {
            Assert.That(() => new AttackAction(_bus, _occupancy, _player, null, PlayerCell, NorthOfPlayer),
                Throws.TypeOf<ArgumentNullException>());
        }
    }
}