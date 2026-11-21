


namespace FutharksAdventure.Core
{
    public abstract class GameAction
    {
        public abstract bool CanExecute();
        protected abstract void Execute();
        public bool TryExecute()
        {
            if (!CanExecute())
                return false;

            Execute();
            return true;
        }
    }
}
