using FutharksAdventure.Core;
using NUnit.Framework;

namespace FutharksAdventure.Tests
{
    public class DistanceMapTests
    {
        private static GridPosition P(int x, int y) => new GridPosition(x, y);

        // Piece 7x7 ouverte, cible au centre (4,4)
        private static DistanceMap OpenRoomMap() =>
            Pathfinder.ComputeDistances(TestFloors.CreateRoom7x7(), P(4, 4));

        // --- Directions qui rapprochent ---

        [Test]
        public void GetApproachingDirections_StraightLine_SingleDirection()
        {
            var directions = OpenRoomMap().GetApproachingDirections(P(4, 6));

            Assert.That(directions, Is.EquivalentTo(new[] { Direction.South }));
        }

        [Test]
        public void GetApproachingDirections_Diagonal_TwoDirections()
        {
            var directions = OpenRoomMap().GetApproachingDirections(P(6, 6));

            Assert.That(directions, Is.EquivalentTo(new[] { Direction.South, Direction.West }));
        }

        [Test]
        public void GetApproachingDirections_Corridor_TowardTarget()
        {
            var map = Pathfinder.ComputeDistances(TestFloors.CreateCorridor3x1(), P(1, 1));

            Assert.That(map.GetApproachingDirections(P(3, 1)), Is.EquivalentTo(new[] { Direction.West }));
        }

        [Test]
        public void GetApproachingDirections_AroundWall_FollowsPathNotStraightLine()
        {
            // Depuis (2,1), cible (6,1) derriere le mur : distance 16.
            // Nord (2,2) et Est (3,1) sont a 15 ; Ouest (1,1) est a 17 ; Sud est un mur.
            var map = Pathfinder.ComputeDistances(TestFloors.CreateRoom7x7SplitByWall(), P(6, 1));

            Assert.That(map.GetApproachingDirections(P(2, 1)),
                Is.EquivalentTo(new[] { Direction.North, Direction.East }));
        }

        // --- Aucune direction ---

        [Test]
        public void GetApproachingDirections_OnTarget_IsEmpty()
        {
            Assert.That(OpenRoomMap().GetApproachingDirections(P(4, 4)), Is.Empty);
        }

        [Test]
        public void GetApproachingDirections_FromWall_IsEmpty()
        {
            Assert.That(OpenRoomMap().GetApproachingDirections(P(0, 0)), Is.Empty);
        }

        [Test]
        public void GetApproachingDirections_FromUnconnectedRoom_IsEmpty()
        {
            var map = Pathfinder.ComputeDistances(TestFloors.CreateTwoUnconnectedRooms(), P(2, 2));

            Assert.That(map.GetApproachingDirections(P(6, 2)), Is.Empty);
        }

        [TestCase(-1, 0)]
        [TestCase(100, 100)]
        public void GetApproachingDirections_OutsideGrid_IsEmpty(int x, int y)
        {
            Assert.That(OpenRoomMap().GetApproachingDirections(P(x, y)), Is.Empty);
        }
    }
}