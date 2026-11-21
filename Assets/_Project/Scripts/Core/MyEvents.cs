


namespace FutharksAdventure.Core
{
    public readonly struct EntityMoved
    {
        public Entity Entity { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }

        public EntityMoved(Entity entity, GridPosition from, GridPosition to)
        {
            Entity = entity;
            From = from;
            To = to;
        }
    }

    public readonly struct TurnResolved { };

}
