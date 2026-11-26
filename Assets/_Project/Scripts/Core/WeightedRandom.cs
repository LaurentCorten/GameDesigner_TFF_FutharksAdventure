using System;
using System.Collections.Generic;

namespace FutharksAdventure.Core
{
    /// <summary>
    /// Selection aleatoire ponderee ("roulette wheel") :
    /// plus le poids d'une option est grand, plus elle a de chances d'etre tiree.
    /// </summary>
    public static class WeightedRandom
    {
        /// <summary>
        /// Tire une option selon son poids.
        /// Renvoie false (result = default) si la liste est vide ou si tous les poids valent 0.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException">Si un poids est negatif.</exception>
        public static bool TryPick<T>(IReadOnlyList<(T Item, int Weight)> options, Random random, out T result)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));
            if (random is null) throw new ArgumentNullException(nameof(random));

            // 1. Verifier les poids et calculer le total
            int total = 0;
            foreach (var option in options)
            {
                if (option.Weight < 0)
                    throw new ArgumentOutOfRangeException(nameof(options), "Un poids ne peut pas etre negatif");
                total += option.Weight;
            }

            result = default;
            if (total == 0) return false;

            // 2. Tirer un nombre entre 0 et total - 1
            int roll = random.Next(total);

            // 3. Parcourir les options : chacune "consomme" sa part du tirage
            foreach (var option in options)
            {
                if (roll < option.Weight)
                {
                    result = option.Item;
                    return true;
                }
                roll -= option.Weight;
            }

            // Impossible si le total est juste : signale un bug
            throw new InvalidOperationException("Le tirage a depasse le total des poids");
        }
    }
}