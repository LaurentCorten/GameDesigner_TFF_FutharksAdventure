using System;



namespace FutharksAdventure.Core
{
    public class ThrowAction : GameAction
    {
        private readonly EventBus _eventBus;
        private readonly TargetSelector _targetSelector;
        private readonly DamageHandler _damageHandler;
        private readonly Player _player;
        private Creature _target;

        public ThrowAction(EventBus eventBus, TargetSelector targetSelector, DamageHandler damageHandler, Player player)
        {
            if (eventBus is null) throw new ArgumentNullException(nameof(eventBus));
            if (targetSelector is null) throw new ArgumentNullException(nameof(targetSelector));
            if (damageHandler is null) throw new ArgumentNullException(nameof(damageHandler));
            if (player is null) throw new ArgumentNullException(nameof(player));

            _eventBus = eventBus;
            _targetSelector = targetSelector;
            _damageHandler = damageHandler;
            _player = player;
        }

        public override bool CanExecute()
        {
            if (_player.Hatchets <= 0) return false;
            
            _target = _targetSelector.FindTarget(_player);
            if (_target is null) return false;

            return true;
        }

        protected override void Execute()
        {
            if (_target is null) throw new InvalidOperationException("Target ne devrait pas pouvoir être null à cet endroit");

            _player.ConsumeHatchet();
            _damageHandler.ApplyDamage(_target, _player.AttackDamage);
            _targetSelector.RememberTarget(_target);
            _eventBus.Publish(new HatchetsChanged(_player.Hatchets));
            
        }
    }
}
