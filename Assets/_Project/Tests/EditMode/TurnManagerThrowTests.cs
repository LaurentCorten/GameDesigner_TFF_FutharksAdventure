using FutharksAdventure.Core;
using NUnit.Framework;
using System.Collections.Generic;

namespace FutharksAdventure.Tests
{
    public class TurnManagerThrowTests
    {
        private TestWorld _world;
        private EventBus _bus;
        private Player _player;
        private TurnManager _turnManager;

        private static readonly GridPosition Center = new GridPosition(4, 4);
        private static readonly GridPosition TwoNorth = new GridPosition(4, 6);

        [SetUp]
        public void SetUp()
        {
            _world = TestWorld.Create(TestFloors.CreateRoom7x7(), Center,
                TestFactory.CreatePlayer(food: 10, hatchets: 3));
            _bus = _world.Bus;
            _player = _world.Player;
            _turnManager = _world.TurnManager;
        }

        [Test]
        public void SubmitThrow_WithTarget_ReturnsTrueAndConsumesTurn()
        {
            _world.PlaceEnemy(TwoNorth);
            bool resolved = false;
            _bus.Subscribe<TurnResolved>(_ => resolved = true);

            bool accepted = _turnManager.SubmitThrow();

            Assert.That(accepted, Is.True);
            Assert.That(resolved, Is.True);
            Assert.That(_player.Food, Is.EqualTo(9));
            Assert.That(_player.Hatchets, Is.EqualTo(2));
        }

        [Test]
        public void SubmitThrow_WithTarget_PublishesHealthChangedBeforeTurnResolved()
        {
            var enemy = _world.PlaceEnemy(TwoNorth);
            var log = new List<string>();
            _bus.Subscribe<HealthChanged>(e => { if (e.Creature == enemy) log.Add("enemyHit"); });
            _bus.Subscribe<TurnResolved>(_ => log.Add("turn"));

            _turnManager.SubmitThrow();

            Assert.That(log, Is.EqualTo(new[] { "enemyHit", "turn" }));
        }

        [Test]
        public void SubmitThrow_NoTarget_ReturnsFalseNoTurnNoCost()
        {
            bool resolved = false;
            _bus.Subscribe<TurnResolved>(_ => resolved = true);

            bool accepted = _turnManager.SubmitThrow();

            Assert.That(accepted, Is.False);
            Assert.That(resolved, Is.False);
            Assert.That(_player.Food, Is.EqualTo(10));
            Assert.That(_player.Hatchets, Is.EqualTo(3));
        }

        [Test]
        public void SubmitThrow_NoHatchets_ReturnsFalseNoTurnNoCost()
        {
            var world = TestWorld.Create(TestFloors.CreateRoom7x7(), Center,
                TestFactory.CreatePlayer(food: 10, hatchets: 0));
            world.PlaceEnemy(TwoNorth);
            bool resolved = false;
            world.Bus.Subscribe<TurnResolved>(_ => resolved = true);

            bool accepted = world.TurnManager.SubmitThrow();

            Assert.That(accepted, Is.False);
            Assert.That(resolved, Is.False);
            Assert.That(world.Player.Food, Is.EqualTo(10));
        }

        [Test]
        public void SubmitThrow_PlayerDead_ReturnsFalseAndKeepsHatchets()
        {
            _world.PlaceEnemy(TwoNorth);
            _player.TakeDamage(_player.MaxHealth);

            bool accepted = _turnManager.SubmitThrow();

            Assert.That(accepted, Is.False);
            Assert.That(_player.Hatchets, Is.EqualTo(3));
        }
    }
}