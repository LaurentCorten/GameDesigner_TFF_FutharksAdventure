using System;
using System.Collections.Generic;



namespace FutharksAdventure.Core
{
    public class TargetSelector
    {
        private readonly FloorGrid _floorGrid;
        private readonly OccupancyMap _occupancyMap;
        private readonly Dictionary<Creature, int> _accessibleTargets = new Dictionary<Creature, int>();
        private Creature _lastTarget;

        public TargetSelector(FloorGrid floorGrid, OccupancyMap occupancyMap)
        {
            if (floorGrid is null) throw new ArgumentNullException(nameof(floorGrid));
            if (occupancyMap is null) throw new ArgumentNullException(nameof(occupancyMap));

            _floorGrid = floorGrid;
            _occupancyMap = occupancyMap;
        }

        public Creature FindTarget(Creature thrower)
        {
            if (thrower is null) throw new ArgumentNullException(nameof(thrower));

            Creature target = null;
            _accessibleTargets.Clear();

            // Vérifie les 4 directions dans l'ordre pour des candidats potentiels
            foreach (Direction direction in DirectionExtensions.AllDirections)
                CheckDirection(direction, thrower);

            // S'il y en a aucun on renvoie null
            if (_accessibleTargets.Count == 0)
                return target;

            // Des potentiels on extrait celui ou ceux les plus proches
            int distance = int.MaxValue;
            List<Creature> closestTargets = new List<Creature>();
            foreach(KeyValuePair<Creature,int> kvp in _accessibleTargets)
            {
                if (kvp.Value > distance)
                    continue;

                if(kvp.Value < distance)
                {
                    closestTargets.Clear();
                    distance = kvp.Value;
                }
                
                closestTargets.Add(kvp.Key);
            }

            // S'il n'y a qu'un seul candidat on l'envoi
            if (closestTargets.Count == 1)
                target = closestTargets[0];
            // Sinon si la liste contient la dernière cible c'est elle qu'on envoi 
            else if (closestTargets.Contains(_lastTarget))
                target = _lastTarget;
            // Sinon si les cibles on un difference de PV on envoi la plus faible ou le premier des plus faibles en ordre directionnel N/E/S/O
            else
            {
                int life = int.MaxValue;
                List<Creature> weakests = new List<Creature>();
                foreach (Creature creature in closestTargets)
                {
                    if (creature.CurrentHealth > life)
                        continue;

                    if (creature.CurrentHealth < life)
                    {
                        weakests.Clear();
                        life = creature.CurrentHealth;
                    }

                    weakests.Add(creature);
                }

                target = weakests[0];
            }

            return target;
        }

        public void RememberTarget(Creature target)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));

            _lastTarget = target;
        }

        private void CheckDirection(Direction direction, Creature thrower)
        {
            GridPosition checkedPosition = thrower.Position;
            bool keepGoing = true;

            while(keepGoing)
            {
                checkedPosition += direction.ToGridOffset();
                int distance = CheckDistance(checkedPosition, thrower.Position);
                if (distance > thrower.VisionRange)
                    return;
                keepGoing = KeepGoing(checkedPosition, thrower, distance);
            }
        }

        /// <summary>
        /// On vérifie si la ligne de visée n'est pas interrompue et que donc on continue à explorer cette direction
        /// </summary>
        /// <param name="checkedPosition"></param>
        /// <param name="thrower"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        private bool KeepGoing(GridPosition checkedPosition, Creature thrower, int distance)
        {
            // Si la cas n'est pas marchable ça coupe la ligne de visée
            if (!_floorGrid.IsWalkable(checkedPosition))
                return false;

            Entity occupant = _occupancyMap.GetEntity(checkedPosition);
            // S'il n'y a rien sur la case rien ne coup la ligne de visée
            if (occupant is null)
                return true;
            // S'il y a une créature sur la case elle coupe la ligne de visée
            if (occupant is Creature creature)
            {
                // Si cette créature n'est pas au corp à corps elle est enregistrée en cible potentielle
                if (distance > 1)
                    _accessibleTargets.Add(creature, distance);
                return false;
            }

            // TEMPORARY : En attendant qu'il y ait des entités qui ne sont pas des Créatures
            else return true;
        }

        private int CheckDistance(GridPosition targetPosition, GridPosition throwerPosition)
        {
            int distance = Math.Abs(targetPosition.X - throwerPosition.X) + Math.Abs(targetPosition.Y - throwerPosition.Y);

            return distance;
        }
    }
}
