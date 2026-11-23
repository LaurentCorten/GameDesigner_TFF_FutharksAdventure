using System.Collections.Generic;
using FutharksAdventure.Core;
using NUnit.Framework;

namespace FutharksAdventure.Tests
{
    public class TurnManagerTurnCostTests
    {
        private static readonly GridPosition Start = new GridPosition(2, 1);

        private EventBus _bus;
        private Player _player;
        private TurnManager _turnManager;
        private List<string> _events;

        // Couloir praticable de (1,1) a (3,1), le joueur part de (2,1).
        // Tout le reste est Empty, donc non praticable.
        private void Setup(int maxHealth, int food)
        {
            var grid = new FloorGrid(5, 3);
            for (int x = 1; x <= 3; x++)
                grid.SetTileType(new GridPosition(x, 1), TileType.Floor);

            var occupancy = new OccupancyMap();
            _bus = new EventBus();
            _player = new Player(maxHealth, food);
            occupancy.Place(_player, Start);

            var resolver = new IntentResolver(grid, occupancy, _bus);
            _turnManager = new TurnManager(_player, resolver, _bus);

            _events = new List<string>();
            _bus.Subscribe<EntityMoved>(_ => _events.Add(nameof(EntityMoved)));
            _bus.Subscribe<FoodChanged>(_ => _events.Add(nameof(FoodChanged)));
            _bus.Subscribe<HealthChanged>(_ => _events.Add(nameof(HealthChanged)));
            _bus.Subscribe<PlayerDied>(_ => _events.Add(nameof(PlayerDied)));
            _bus.Subscribe<TurnResolved>(_ => _events.Add(nameof(TurnResolved)));
        }

        // Aller-retour dans le couloir : chaque appel est un deplacement valide.
        private bool PlayValidTurn(int turnIndex)
        {
            Direction direction = turnIndex % 2 == 0 ? Direction.East : Direction.West;
            return _turnManager.SubmitMove(direction);
        }

        // --- Nourriture ---

        [Test]
        public void AcceptedMove_ConsumesOneFood()
        {
            Setup(maxHealth: 10, food: 5);

            PlayValidTurn(0);

            Assert.That(_player.Food, Is.EqualTo(4));
            Assert.That(_player.CurrentHealth, Is.EqualTo(10));
        }

        [Test]
        public void AcceptedMove_PublishesFoodChangedWithNewValue()
        {
            Setup(maxHealth: 10, food: 5);
            int received = -1;
            _bus.Subscribe<FoodChanged>(evt => received = evt.Current);

            PlayValidTurn(0);

            Assert.That(received, Is.EqualTo(4));
        }

        [Test]
        public void RefusedMove_CostsNothingAndPublishesNothing()
        {
            Setup(maxHealth: 10, food: 5);

            bool accepted = _turnManager.SubmitMove(Direction.North); // case Empty

            Assert.That(accepted, Is.False);
            Assert.That(_player.Food, Is.EqualTo(5));
            Assert.That(_player.CurrentHealth, Is.EqualTo(10));
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void LastFood_IsEaten_WithoutLosingHealth()
        {
            Setup(maxHealth: 10, food: 1);

            PlayValidTurn(0);

            Assert.That(_player.Food, Is.EqualTo(0));
            Assert.That(_player.CurrentHealth, Is.EqualTo(10));
            Assert.That(_events, Does.Not.Contain(nameof(HealthChanged)));
        }

        // --- Famine ---

        [Test]
        public void Starving_LosesOneHealthPerTurn()
        {
            Setup(maxHealth: 10, food: 0);

            PlayValidTurn(0);
            PlayValidTurn(1);

            Assert.That(_player.CurrentHealth, Is.EqualTo(8));
            Assert.That(_player.Food, Is.EqualTo(0));
        }

        [Test]
        public void Starving_PublishesHealthChanged_NotFoodChanged()
        {
            Setup(maxHealth: 10, food: 0);
            HealthChanged received = default;
            _bus.Subscribe<HealthChanged>(evt => received = evt);

            PlayValidTurn(0);

            Assert.That(received.Creature, Is.SameAs(_player));
            Assert.That(received.Current, Is.EqualTo(9));
            Assert.That(received.Max, Is.EqualTo(10));
            Assert.That(_events, Does.Not.Contain(nameof(FoodChanged)));
        }

        [Test]
        public void FoodThenStarvation_FullSequence()
        {
            Setup(maxHealth: 3, food: 2);

            for (int i = 0; i < 4; i++)
                PlayValidTurn(i);

            // 2 tours payes en nourriture, puis 2 tours payes en PV
            Assert.That(_player.Food, Is.EqualTo(0));
            Assert.That(_player.CurrentHealth, Is.EqualTo(1));
            Assert.That(_player.IsDead, Is.False);
        }

        // --- Mort par famine ---

        [Test]
        public void StarvationDeath_PublishesEventsInOrder()
        {
            Setup(maxHealth: 1, food: 0);

            bool accepted = PlayValidTurn(0);

            Assert.That(accepted, Is.True);
            Assert.That(_player.IsDead, Is.True);
            Assert.That(_events, Is.EqualTo(new[]
            {
                nameof(EntityMoved),
                nameof(HealthChanged),
                nameof(PlayerDied),
                nameof(TurnResolved)
            }));
        }

        [Test]
        public void AfterDeath_SubmitMove_IsRefusedSilently()
        {
            Setup(maxHealth: 1, food: 0);
            PlayValidTurn(0);
            GridPosition positionAtDeath = _player.Position;
            _events.Clear();

            bool accepted = PlayValidTurn(1);

            Assert.That(accepted, Is.False);
            Assert.That(_player.Position, Is.EqualTo(positionAtDeath));
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void PlayerDied_IsPublishedOnlyOnce()
        {
            Setup(maxHealth: 1, food: 0);
            int deathCount = 0;
            _bus.Subscribe<PlayerDied>(_ => deathCount++);

            PlayValidTurn(0);
            PlayValidTurn(1);
            PlayValidTurn(2);

            Assert.That(deathCount, Is.EqualTo(1));
        }
    }
}