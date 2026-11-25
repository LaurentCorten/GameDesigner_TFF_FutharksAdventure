using FutharksAdventure.Core;

namespace FutharksAdventure.Tests
{
    internal static class TestFloors
    {
        // Grille 5x5 : piece de sol 3x3 de (1,1) a (3,3), murs autour
        public static FloorGrid CreateRoom3x3()
        {
            var grid = new FloorGrid(5, 5);
            RoomCarver.CarveRoom(grid, new GridPosition(2, 2), 3, 3, Sides.None);
            return grid;
        }
    }

    /// <summary>
    /// Fabrique d'objets pour les tests : si un constructeur change,
    /// on ne corrige qu'ici.
    /// </summary>
    internal static class TestFactory
    {
        public static Player CreatePlayer(
            int maxHealth = 10,
            int food = 10,
            int hatchets = 3,
            int visionRange = 6,
            int atkDmg = 4)

        {   
            return new Player(maxHealth, food, hatchets, visionRange, atkDmg);
        }

        public static Creature CreateEnemy(
            int maxHealth = 10,
            int visionRange = 6,
            int atkDmg = 3)

        {
            return new Enemy(maxHealth, visionRange, atkDmg); // option A
        }
    }


}