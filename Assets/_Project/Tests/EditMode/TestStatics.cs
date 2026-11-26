using System;
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

        // Grille 9x9 : piece de sol 7x7 de (1,1) a (7,7), centre (4,4), murs autour
        public static FloorGrid CreateRoom7x7()
        {
            var grid = new FloorGrid(9, 9);
            RoomCarver.CarveRoom(grid, new GridPosition(4, 4), 7, 7, Sides.None);
            return grid;
        }

        // Grille 5x3 : couloir praticable de (1,1) a (3,1).
        // Tout le reste est Empty, donc non praticable.
        public static FloorGrid CreateCorridor3x1()
        {
            var grid = new FloorGrid(5, 3);
            for (int x = 1; x <= 3; x++)
                grid.SetTileType(new GridPosition(x, 1), TileType.Floor);
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
            return new Enemy(maxHealth, visionRange, atkDmg);
        }
    }

    /// <summary>
    /// Petit monde de test deja assemble : grille, carte des occupants, bus,
    /// gestion des degats, selection de cible, resolver, TurnManager et joueur place.
    /// Si l'assemblage change (nouvelle dependance), on ne corrige qu'ici.
    /// </summary>
    internal class TestWorld
    {
        public FloorGrid Grid { get; }
        public OccupancyMap Occupancy { get; }
        public EventBus Bus { get; }
        public DamageHandler DamageHandler { get; }
        public TargetSelector TargetSelector { get; }
        public IntentResolver Resolver { get; }
        public TurnManager TurnManager { get; }
        public Player Player { get; }

        private TestWorld(FloorGrid grid, Player player)
        {
            Grid = grid;
            Player = player;
            Occupancy = new OccupancyMap();
            Bus = new EventBus();
            DamageHandler = new DamageHandler(Bus, Occupancy);
            TargetSelector = new TargetSelector(Grid, Occupancy);
            Resolver = new IntentResolver(Grid, Occupancy, Bus, DamageHandler, TargetSelector);
            TurnManager = new TurnManager(Player, Resolver, Bus, DamageHandler);
        }

        /// <summary>
        /// Cree le monde et place le joueur. Sans joueur fourni,
        /// utilise TestFactory.CreatePlayer() avec ses valeurs par defaut.
        /// </summary>
        public static TestWorld Create(FloorGrid grid, GridPosition playerPosition, Player player = null)
        {
            var world = new TestWorld(grid, player ?? TestFactory.CreatePlayer());
            world.Occupancy.Place(world.Player, playerPosition);
            return world;
        }

        /// <summary>Cree un ennemi et le place sur la carte.</summary>
        public Creature PlaceEnemy(GridPosition position, int maxHealth = 10, int atkDmg = 3)
        {
            var enemy = TestFactory.CreateEnemy(maxHealth: maxHealth, atkDmg: atkDmg);
            Occupancy.Place(enemy, position);
            return enemy;
        }
    }

    /// <summary>
    /// Faux generateur aleatoire : Next(maxValue) renvoie toujours la valeur choisie.
    /// Les autres methodes levent une exception pour imposer le contrat Next(maxValue).
    /// </summary>
    internal class FakeRandom : Random
    {
        private readonly int _value;

        /// <summary>Dernier maxValue recu (-1 si Next n'a jamais ete appele).</summary>
        public int LastMaxValue { get; private set; } = -1;

        public FakeRandom(int value)
        {
            _value = value;
        }

        public override int Next(int maxValue)
        {
            LastMaxValue = maxValue;
            if (_value < 0 || _value >= maxValue)
                throw new InvalidOperationException(
                    $"FakeRandom mal configure : {_value} hors de [0, {maxValue}[");
            return _value;
        }

        public override int Next() =>
            throw new NotSupportedException("Utiliser Next(maxValue)");

        public override int Next(int minValue, int maxValue) =>
            throw new NotSupportedException("Utiliser Next(maxValue)");

        public override double NextDouble() =>
            throw new NotSupportedException("Utiliser Next(maxValue)");
    }
}