
namespace FutharksAdventure.Core
{
    public class RefusedAction : GameAction
    {
        public override bool CanExecute()
        {
            return false;
        }

        protected override void Execute()
        {
            // NOOP
        }
    }
}
