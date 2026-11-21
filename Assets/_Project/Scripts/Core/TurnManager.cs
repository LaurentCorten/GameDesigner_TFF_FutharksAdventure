


namespace FutharksAdventure.Core
{
    public class TurnManager
    {
        private readonly Player _player;
        private readonly IntentResolver _resolver;
        private readonly EventBus _eventBus;

        public TurnManager(Player player, IntentResolver resolver, EventBus eventBus)
        {
            _player = player;
            _resolver = resolver;
            _eventBus = eventBus;
        }

        public bool SubmitMove(Direction direction)
        {
            GameAction action = _resolver.Resolve(_player, direction);
            
            if(!action.TryExecute())
                return false;
            
            _eventBus.Publish(new TurnResolved());
            return true;
        }
    }
}
