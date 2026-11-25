using FutharksAdventure.Core;
using NUnit.Framework;
using System;

namespace FutharksAdventure.Tests
{
    public class TargetSelectorTests
    {
        private TestWorld _world;
        private FloorGrid _grid;
        private OccupancyMap _occupancy;
        private TargetSelector _selector;
        private Player _player;

        // Piece 7x7 de (1,1) a (7,7) : depuis le centre, distance max 3 dans chaque direction
        private static readonly GridPosition Center = new GridPosition(4, 4);

        [SetUp]
        public void SetUp()
        {
            _world = TestWorld.Create(TestFloors.CreateRoom7x7(), Center,
                TestFactory.CreatePlayer(visionRange: 6));
            _grid = _world.Grid;
            _occupancy = _world.Occupancy;
            _selector = _world.TargetSelector;
            _player = _world.Player;
        }

        private Creature PlaceEnemy(int x, int y, int maxHealth = 10) =>
            _world.PlaceEnemy(new GridPosition(x, y), maxHealth: maxHealth);

        // Monde avec un joueur a vue courte, pour tester la portee
        private TestWorld CreateShortSightedWorld(int visionRange) =>
            TestWorld.Create(TestFloors.CreateRoom7x7(), Center,
                TestFactory.CreatePlayer(visionRange: visionRange));

        // --- Eligibilite ---

        [Test]
        public void FindTarget_NoCreature_ReturnsNull()
        {
            Assert.That(_selector.FindTarget(_player), Is.Null);
        }

        [TestCase(4, 6)] // Nord
        [TestCase(6, 4)] // Est
        [TestCase(4, 2)] // Sud
        [TestCase(2, 4)] // Ouest
        public void FindTarget_AlignedEnemy_ReturnsIt(int x, int y)
        {
            var enemy = PlaceEnemy(x, y);

            Assert.That(_selector.FindTarget(_player), Is.SameAs(enemy));
        }

        [Test]
        public void FindTarget_DiagonalEnemy_ReturnsNull()
        {
            PlaceEnemy(6, 6);

            Assert.That(_selector.FindTarget(_player), Is.Null);
        }

        [Test]
        public void FindTarget_AdjacentEnemy_ReturnsNull()
        {
            PlaceEnemy(4, 5);

            Assert.That(_selector.FindTarget(_player), Is.Null);
        }

        [Test]
        public void FindTarget_AdjacentEnemyAndFartherOnOtherAxis_ReturnsFarther()
        {
            PlaceEnemy(4, 5);
            var far = PlaceEnemy(6, 4);

            Assert.That(_selector.FindTarget(_player), Is.SameAs(far));
        }

        [Test]
        public void FindTarget_EnemyAtExactlyVisionRange_ReturnsIt()
        {
            var world = CreateShortSightedWorld(visionRange: 2);
            var enemy = world.PlaceEnemy(new GridPosition(4, 6));

            Assert.That(world.TargetSelector.FindTarget(world.Player), Is.SameAs(enemy));
        }

        [Test]
        public void FindTarget_EnemyBeyondVisionRange_ReturnsNull()
        {
            var world = CreateShortSightedWorld(visionRange: 2);
            world.PlaceEnemy(new GridPosition(4, 7));

            Assert.That(world.TargetSelector.FindTarget(world.Player), Is.Null);
        }

        [Test] // (C) un mur bloque la ligne de visee
        public void FindTarget_WallBetween_ReturnsNull()
        {
            _grid.SetTileType(new GridPosition(4, 5), TileType.Wall);
            PlaceEnemy(4, 6);

            Assert.That(_selector.FindTarget(_player), Is.Null);
        }

        [Test]
        public void FindTarget_WallBesideLine_DoesNotBlock()
        {
            _grid.SetTileType(new GridPosition(5, 5), TileType.Wall);
            var enemy = PlaceEnemy(4, 6);

            Assert.That(_selector.FindTarget(_player), Is.SameAs(enemy));
        }

        [Test] // (A) une creature bloque la ligne de visee
        public void FindTarget_EnemyBehindAdjacentEnemy_ReturnsNull()
        {
            PlaceEnemy(4, 5);
            PlaceEnemy(4, 7);

            Assert.That(_selector.FindTarget(_player), Is.Null);
        }

        [Test]
        public void FindTarget_NeverReturnsThrower()
        {
            Assert.That(_selector.FindTarget(_player), Is.Not.SameAs(_player));
        }

        // --- Priorites ---

        [Test]
        public void FindTarget_ClosestWins()
        {
            var close = PlaceEnemy(4, 6); // distance 2
            PlaceEnemy(7, 4);             // distance 3

            Assert.That(_selector.FindTarget(_player), Is.SameAs(close));
        }

        [Test]
        public void FindTarget_TieWithoutPrevious_LowestHealthWins()
        {
            PlaceEnemy(4, 6, maxHealth: 10);
            var weak = PlaceEnemy(6, 4, maxHealth: 5);

            Assert.That(_selector.FindTarget(_player), Is.SameAs(weak));
        }

        [Test]
        public void FindTarget_TieUsesCurrentHealth_NotMaxHealth()
        {
            var hurt = PlaceEnemy(4, 6, maxHealth: 10);
            PlaceEnemy(6, 4, maxHealth: 8);
            hurt.TakeDamage(5); // 5 PV restants

            Assert.That(_selector.FindTarget(_player), Is.SameAs(hurt));
        }

        [Test]
        public void FindTarget_TieWithPreviousAmongTied_PreviousWins()
        {
            var previous = PlaceEnemy(4, 6, maxHealth: 10);
            PlaceEnemy(6, 4, maxHealth: 5);
            _selector.RememberTarget(previous);

            Assert.That(_selector.FindTarget(_player), Is.SameAs(previous));
        }

        [Test]
        public void FindTarget_PreviousFartherThanOther_ClosestStillWins()
        {
            var previous = PlaceEnemy(7, 4); // distance 3
            var close = PlaceEnemy(4, 6);    // distance 2
            _selector.RememberTarget(previous);

            Assert.That(_selector.FindTarget(_player), Is.SameAs(close));
        }

        [Test]
        public void FindTarget_PreviousNoLongerAligned_IsIgnored()
        {
            var previous = PlaceEnemy(4, 6, maxHealth: 10);
            _selector.RememberTarget(previous);
            _occupancy.Move(previous, new GridPosition(5, 6)); // n'est plus aligne
            var other = PlaceEnemy(6, 4, maxHealth: 10);

            Assert.That(_selector.FindTarget(_player), Is.SameAs(other));
        }

        [Test]
        public void FindTarget_PreviousDeadAndRemoved_FallsBackToLowestHealth()
        {
            var previous = PlaceEnemy(4, 6, maxHealth: 3);
            _selector.RememberTarget(previous);
            _world.DamageHandler.ApplyDamage(previous, 3); // mort et retire de la carte
            PlaceEnemy(6, 4, maxHealth: 10);
            var weak = PlaceEnemy(4, 2, maxHealth: 5);

            Assert.That(_selector.FindTarget(_player), Is.SameAs(weak));
        }

        [Test] // (B) egalite totale : ordre de l'enum Direction (N, E, S, O)
        public void FindTarget_FullTie_FirstInDirectionOrder()
        {
            PlaceEnemy(2, 4); // Ouest
            var north = PlaceEnemy(4, 6);
            PlaceEnemy(6, 4); // Est

            Assert.That(_selector.FindTarget(_player), Is.SameAs(north));
        }

        // --- Memoire ---

        [Test] // chercher une cible ne la memorise pas
        public void FindTarget_Alone_DoesNotRemember()
        {
            PlaceEnemy(4, 6, maxHealth: 10);           // seule cible a distance 2
            var weak = PlaceEnemy(7, 4, maxHealth: 5); // distance 3
            _selector.FindTarget(_player);             // trouve la premiere, sans la memoriser

            _occupancy.Move(weak, new GridPosition(6, 4)); // egalite a distance 2

            Assert.That(_selector.FindTarget(_player), Is.SameAs(weak));
        }

        // --- Gardes ---

        [Test]
        public void FindTarget_NullThrower_Throws()
        {
            Assert.That(() => _selector.FindTarget(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Constructor_NullGrid_Throws()
        {
            Assert.That(() => new TargetSelector(null, _occupancy), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Constructor_NullOccupancyMap_Throws()
        {
            Assert.That(() => new TargetSelector(_grid, null), Throws.TypeOf<ArgumentNullException>());
        }
    }
}