using System;
using System.Collections.Generic;




namespace FutharksAdventure.Core
{
    public class OccupancyMap
    {
        private readonly Dictionary<GridPosition, Entity> _occupancyDico = new Dictionary<GridPosition, Entity>();


        /// <summary>
        /// Permet de vérifier si une position est répertoriée dans la collection de référence.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>Retourne true si la position est occupée par une entité</returns>
        public bool IsOccupied(GridPosition position)
        {
            return _occupancyDico.ContainsKey(position);
        }

        public Entity GetEntity(GridPosition position)
        {
            return _occupancyDico.GetValueOrDefault(position, null);
        }

        private bool IsPlaced(Entity entity)
        {
            return _occupancyDico.TryGetValue(entity.Position, out var occupant)
                && occupant == entity;
        }

        /// <summary>
        /// Permet de positionner une nouvelle entité dans la collection de référence.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="position"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void Place(Entity entity, GridPosition position)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));
            if (IsOccupied(position))
                throw new InvalidOperationException("La position visée est déjà occupée");
            if (IsPlaced(entity))
                throw new InvalidOperationException("L'entité est déjà placée");

            _occupancyDico[position] = entity;
            entity.Position = position;
        }

        /// <summary>
        /// Permet de déplacer une entité déjà présente dans la collection.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="position"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void Move(Entity entity, GridPosition position)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));
            if (IsOccupied(position))
                throw new InvalidOperationException("La position visée est déjà occupée");
            if (!IsPlaced(entity))
                throw new InvalidOperationException("L'entité doit être placée avant d'être déplacée");

            _occupancyDico.Remove(entity.Position);
            _occupancyDico[position] = entity;

            entity.Position = position;
        }

        public void Remove(Entity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));
            if (_occupancyDico.TryGetValue(entity.Position, out var occupant) && occupant == entity)
                _occupancyDico.Remove(entity.Position);
            else if (IsOccupied(entity.Position))
                throw new InvalidOperationException("l'entité fournie ne correspond pas à celle enregistrée sur cette position");
            else throw new InvalidOperationException("Il n'y a rien a enlever à cette position");
        }
    }
}
