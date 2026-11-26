using FutharksAdventure.Core;
using NUnit.Framework;
using System;

namespace FutharksAdventure.Tests
{
    public class ThrowActionTests
    {
        private TestWorld _world;
        private OccupancyMap _occupancy;
        private EventBus _bus;
        private TargetSelector _selector;
        private DamageHandler _damageHandler;
        private Player _player;

        // Piece 7x7 de (1,1) a (7,7) : depuis le centre, distance max 3 dans chaque direction
        private static readonly GridPosition Center = new GridPosition(4, 4);
        private static readonly GridPosition TwoNorth = new GridPosition(4, 6);
        private static readonly GridPosition TwoEast = new GridPosition(6, 4);

        [SetUp]
        public void SetUp()
        {
            _world = TestWorld.Create(TestFloors.CreateRoom7x7(), Center,
                TestFactory.CreatePlayer(hatchets: 3, atkDmg: 4));
            _occupancy = _world.Occupancy;
            _bus = _world.Bus;
            _selector = _world.TargetSelector;
            _damageHandler = _world.DamageHandler;
            _player = _world.Player;
        }

        private ThrowAction CreateThrow(Player thrower) =>
            new ThrowAction(_bus, _selector, _damageHandler, thrower);

        // Monde dont le joueur a un stock de hachettes donne
        private TestWorld CreateWorldWithHatchets(int hatchets) =>
            TestWorld.Create(TestFloors.CreateRoom7x7(), Center,
                TestFactory.CreatePlayer(hatchets: hatchets));

        private static ThrowAction CreateThrow(TestWorld world) =>
            new ThrowAction(world.Bus, world.TargetSelector, world.DamageHandler, world.Player);

        // --- CanExecute ---
        [Test]
        public void CanExecute_TargetAndHatchets_ReturnsTrue()
        {
            _world.PlaceEnemy(TwoNorth);

            Assert.That(CreateThrow(_player).CanExecute(), Is.True);
        }

        [Test]
        public void CanExecute_NoTarget_ReturnsFalse()
        {
            Assert.That(CreateThrow(_player).CanExecute(), Is.False);
        }

        [Test]
        public void CanExecute_NoHatchets_ReturnsFalse()
        {
            var world = CreateWorldWithHatchets(0);
            world.PlaceEnemy(TwoNorth);

            Assert.That(CreateThrow(world).CanExecute(), Is.False);
        }

        [Test]
        public void CanExecute_ChangesNothing()
        {
            var enemy = _world.PlaceEnemy(TwoNorth);

            CreateThrow(_player).CanExecute();

            Assert.That(_player.Hatchets, Is.EqualTo(3));
            Assert.That(enemy.CurrentHealth, Is.EqualTo(10));
        }

        [Test] // CanExecute ne doit pas memoriser la cible
        public void CanExecute_Alone_DoesNotRememberTarget()
        {
            _world.PlaceEnemy(TwoNorth, maxHealth: 10);                         // seule cible a distance 2
            var weak = _world.PlaceEnemy(new GridPosition(7, 4), maxHealth: 5); // distance 3
            CreateThrow(_player).CanExecute();                                  // viserait la premiere, sans lancer

            _occupancy.Move(weak, TwoEast); // egalite a distance 2

            Assert.That(_selector.FindTarget(_player), Is.SameAs(weak));
        }

        // --- Execution ---

        [Test]
        public void TryExecute_DamagesTargetWithAttackDamage()
        {
            var enemy = _world.PlaceEnemy(TwoNorth);

            bool accepted = CreateThrow(_player).TryExecute();

            Assert.That(accepted, Is.True);
            Assert.That(enemy.CurrentHealth, Is.EqualTo(10 - _player.AttackDamage));
        }

        [Test]
        public void TryExecute_ConsumesOneHatchet()
        {
            _world.PlaceEnemy(TwoNorth);

            CreateThrow(_player).TryExecute();

            Assert.That(_player.Hatchets, Is.EqualTo(2));
        }

        [Test]
        public void TryExecute_PublishesHatchetsChanged()
        {
            _world.PlaceEnemy(TwoNorth);
            HatchetsChanged? received = null;
            _bus.Subscribe<HatchetsChanged>(e => received = e);

            CreateThrow(_player).TryExecute();

            Assert.That(received.HasValue, Is.True);
            Assert.That(received.Value.Current, Is.EqualTo(2));
        }

        [Test]
        public void TryExecute_PublishesHealthChangedForTarget()
        {
            var enemy = _world.PlaceEnemy(TwoNorth);
            HealthChanged? received = null;
            _bus.Subscribe<HealthChanged>(e => received = e);

            CreateThrow(_player).TryExecute();

            Assert.That(received.HasValue, Is.True);
            Assert.That(received.Value.Creature, Is.SameAs(enemy));
            Assert.That(received.Value.Amount, Is.EqualTo(-_player.AttackDamage));
        }

        [Test]
        public void TryExecute_KillsEnemy_RemovesItFromMap()
        {
            var weak = _world.PlaceEnemy(TwoNorth, maxHealth: 4);

            CreateThrow(_player).TryExecute();

            Assert.That(weak.IsDead, Is.True);
            Assert.That(_occupancy.GetEntity(TwoNorth), Is.Null);
        }

        [Test]
        public void TryExecute_NobodyMoves()
        {
            var enemy = _world.PlaceEnemy(TwoNorth);

            CreateThrow(_player).TryExecute();

            Assert.That(_player.Position, Is.EqualTo(Center));
            Assert.That(enemy.Position, Is.EqualTo(TwoNorth));
        }

        [Test]
        public void TryExecute_RemembersTarget()
        {
            var target = _world.PlaceEnemy(TwoNorth, maxHealth: 10);
            CreateThrow(_player).TryExecute();          // target : 6 PV restants
            _world.PlaceEnemy(TwoEast, maxHealth: 2);   // egalite de distance, moins de PV

            Assert.That(_selector.FindTarget(_player), Is.SameAs(target));
        }

        [Test]
        public void TryExecute_LastHatchet_ThenRefused()
        {
            var world = CreateWorldWithHatchets(1);
            world.PlaceEnemy(TwoNorth, maxHealth: 20);

            bool first = CreateThrow(world).TryExecute();
            bool second = CreateThrow(world).TryExecute();

            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
            Assert.That(world.Player.Hatchets, Is.EqualTo(0));
        }

        // --- Refus ---

        [Test]
        public void TryExecute_NoTarget_ReturnsFalseKeepsHatchetsPublishesNothing()
        {
            bool published = false;
            _bus.Subscribe<HatchetsChanged>(_ => published = true);
            _bus.Subscribe<HealthChanged>(_ => published = true);

            bool accepted = CreateThrow(_player).TryExecute();

            Assert.That(accepted, Is.False);
            Assert.That(_player.Hatchets, Is.EqualTo(3));
            Assert.That(published, Is.False);
        }

        [Test]
        public void TryExecute_NoHatchets_ReturnsFalseTargetUntouchedPublishesNothing()
        {
            var world = CreateWorldWithHatchets(0);
            var enemy = world.PlaceEnemy(TwoNorth);
            bool published = false;
            world.Bus.Subscribe<HatchetsChanged>(_ => published = true);
            world.Bus.Subscribe<HealthChanged>(_ => published = true);

            bool accepted = CreateThrow(world).TryExecute();

            Assert.That(accepted, Is.False);
            Assert.That(world.Player.Hatchets, Is.EqualTo(0));
            Assert.That(enemy.CurrentHealth, Is.EqualTo(10));
            Assert.That(published, Is.False);
        }

        // --- Constructeur ---

        [Test]
        public void Constructor_NullEventBus_Throws()
        {
            Assert.That(() => new ThrowAction(null, _selector, _damageHandler, _player),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Constructor_NullSelector_Throws()
        {
            Assert.That(() => new ThrowAction(_bus, null, _damageHandler, _player),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Constructor_NullDamageHandler_Throws()
        {
            Assert.That(() => new ThrowAction(_bus, _selector, null, _player),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Constructor_NullThrower_Throws()
        {
            Assert.That(() => new ThrowAction(_bus, _selector, _damageHandler, null),
                Throws.TypeOf<ArgumentNullException>());
        }
    }
}