using System;
using System.Collections.Generic;

namespace FutharksAdventure.Core
{
    /// <summary>
    /// Calcul de distances sur la grille par parcours en largeur (BFS).
    /// Seuls les murs (et les cases non praticables) bloquent : les occupants sont ignores.
    /// </summary>
    public static class Pathfinder
    {
        /// <summary>
        /// Calcule la distance (en pas) de chaque case praticable jusqu'à la cible.
        /// Complexité : chaque case est visitée une seule fois (O(largeur x hauteur)).
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Si la cible n'est pas praticable.</exception>
        public static DistanceMap ComputeDistances(FloorGrid grid, GridPosition target)
        {
            if (grid is null) throw new ArgumentNullException(nameof(grid));
            if (!grid.IsWalkable(target))
                throw new ArgumentException("La cible doit être sur une case praticable", nameof(target));

            // 1. Toutes les cases commencent "non atteintes"
            int[,] distances = new int[grid.Width, grid.Height];
            for (int x = 0; x < grid.Width; x++)
                for (int y = 0; y < grid.Height; y++)
                    distances[x, y] = DistanceMap.UNREACHABLE;

            // 2. On part de la cible (distance 0) : c'est la premiere case de la file
            var queue = new Queue<GridPosition>();
            distances[target.X, target.Y] = 0;
            queue.Enqueue(target);

            // 3. On sort les cases dans l'ordre d'arrivee (premier entre, premier sorti) :
            while (queue.Count > 0)
            {
                GridPosition current = queue.Dequeue();
                int nextDistance = distances[current.X, current.Y] + 1;

                for (int i = 0; i < DirectionExtensions.AllDirections.Count; i++)
                {
                    Direction direction = DirectionExtensions.AllDirections[i];

                    GridPosition neighbour = current + direction.ToGridOffset();

                    // Mur, vide ou hors grille : IsWalkable renvoie false
                    if (!grid.IsWalkable(neighbour)) continue;

                    // Deja atteinte : on l'a forcement atteinte par un chemin aussi court ou plus court
                    if (distances[neighbour.X, neighbour.Y] != DistanceMap.UNREACHABLE) continue;

                    distances[neighbour.X, neighbour.Y] = nextDistance;
                    queue.Enqueue(neighbour);
                }
            }

            return new DistanceMap(distances);
        }
    }
}