using System;
using FutharksAdventure.Core;
using NUnit.Framework;

namespace FutharksAdventure.Tests
{
    public class OccupancyMapTests
    {
        private OccupancyMap _map;
        private Player _player;

        [SetUp]
        public void SetUp()
        {
            _map = new OccupancyMap();
            _player = TestFactory.CreatePlayer(10, 10);
        }

        [Test]
        public void IsOccupied_EmptyCell_ReturnsFalse()
        {
            Assert.That(_map.IsOccupied(new GridPosition(1, 1)), Is.False);
        }

        [Test]
        public void Place_SetsEntityPositionAndOccupiesCell()
        {
            var pos = new GridPosition(1, 1);

            _map.Place(_player, pos);

            Assert.That(_player.Position, Is.EqualTo(pos));
            Assert.That(_map.IsOccupied(pos), Is.True);
        }

        [Test]
        public void Place_OnOccupiedCell_Throws()
        {
            var pos = new GridPosition(1, 1);
            _map.Place(_player, pos);

            Assert.Throws<InvalidOperationException>(() => _map.Place(TestFactory.CreatePlayer(10, 10), pos));
        }

        [Test]
        public void Place_SameEntityTwice_Throws()
        {
            _map.Place(_player, new GridPosition(1, 1));

            Assert.Throws<InvalidOperationException>(
                () => _map.Place(_player, new GridPosition(2, 2)));
        }

        [Test]
        public void Move_UpdatesPositionAndCells()
        {
            var from = new GridPosition(1, 1);
            var to = new GridPosition(1, 2);
            _map.Place(_player, from);

            _map.Move(_player, to);

            Assert.That(_player.Position, Is.EqualTo(to));
            Assert.That(_map.IsOccupied(from), Is.False);
            Assert.That(_map.IsOccupied(to), Is.True);
        }

        [Test]
        public void Move_ToOccupiedCell_Throws()
        {
            var other = TestFactory.CreatePlayer(10, 10);
            _map.Place(_player, new GridPosition(1, 1));
            _map.Place(other, new GridPosition(1, 2));

            Assert.Throws<InvalidOperationException>(
                () => _map.Move(_player, new GridPosition(1, 2)));
        }

        [Test]
        public void Move_EntityNotPlaced_Throws()
        {
            Assert.Throws<InvalidOperationException>(
                () => _map.Move(_player, new GridPosition(1, 1)));
        }

        [Test]
        public void Place_NullEntity_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _map.Place(null, new GridPosition(1, 1)));
        }

        [Test]
        public void Move_NullEntity_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _map.Move(null, new GridPosition(1, 1)));
        }

        // --- GetEntity ---

        [Test]
        public void GetEntity_EmptyCell_ReturnsNull()
        {
            Assert.That(_map.GetEntity(new GridPosition(1, 1)), Is.Null);
        }

        [Test]
        public void GetEntity_OccupiedCell_ReturnsSameEntity()
        {
            _map.Place(_player, new GridPosition(1, 1));

            Assert.That(_map.GetEntity(new GridPosition(1, 1)), Is.SameAs(_player));
        }

        [Test]
        public void GetEntity_AfterMove_FollowsEntity()
        {
            _map.Place(_player, new GridPosition(1, 1));
            _map.Move(_player, new GridPosition(1, 2));

            Assert.That(_map.GetEntity(new GridPosition(1, 1)), Is.Null);
            Assert.That(_map.GetEntity(new GridPosition(1, 2)), Is.SameAs(_player));
        }

        // --- Remove ---

        [Test]
        public void Remove_FreesCell()
        {
            var pos = new GridPosition(1, 1);
            _map.Place(_player, pos);

            _map.Remove(_player);

            Assert.That(_map.IsOccupied(pos), Is.False);
            Assert.That(_map.GetEntity(pos), Is.Null);
        }

        [Test]
        public void Remove_LeavesOtherEntitiesUntouched()
        {
            var other = TestFactory.CreatePlayer();
            _map.Place(_player, new GridPosition(1, 1));
            _map.Place(other, new GridPosition(1, 2));

            _map.Remove(_player);

            Assert.That(_map.GetEntity(new GridPosition(1, 2)), Is.SameAs(other));
        }

        [Test]
        public void Remove_FreedCell_CanBeOccupiedAgain()
        {
            var pos = new GridPosition(1, 1);
            _map.Place(_player, pos);
            _map.Remove(_player);
            var other = TestFactory.CreatePlayer();

            Assert.That(() => _map.Place(other, pos), Throws.Nothing);
            Assert.That(_map.GetEntity(pos), Is.SameAs(other));
        }

        [Test]
        public void Remove_ThenMoveRemovedEntity_Throws()
        {
            _map.Place(_player, new GridPosition(1, 1));
            _map.Remove(_player);

            Assert.That(() => _map.Move(_player, new GridPosition(1, 2)),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Remove_NullEntity_Throws()
        {
            Assert.That(() => _map.Remove(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Remove_EntityNotPlaced_Throws()
        {
            Assert.That(() => _map.Remove(_player),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Remove_Twice_Throws()
        {
            _map.Place(_player, new GridPosition(1, 1));
            _map.Remove(_player);

            Assert.That(() => _map.Remove(_player),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test] // l'entite non placee a la position par defaut (0,0), qui est occupee par une autre
        public void Remove_UnplacedEntityOnOccupiedDefaultCell_ThrowsAndKeepsOccupant()
        {
            var occupant = TestFactory.CreatePlayer();
            _map.Place(occupant, new GridPosition(0, 0));
            var unplaced = TestFactory.CreatePlayer(); // Position par defaut (0,0)

            Assert.That(() => _map.Remove(unplaced),
                Throws.TypeOf<InvalidOperationException>());
            Assert.That(_map.GetEntity(new GridPosition(0, 0)), Is.SameAs(occupant));
        }
    }
}