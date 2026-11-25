using System;



namespace FutharksAdventure.Core
{
    public class AttackAction : GameAction
    {
        private readonly DamageHandler _damageHandler;
        private readonly Creature _attackingCreature;
        private readonly Creature _targetCreature;
        private readonly GridPosition _fromPosition;
        private readonly GridPosition _toPosition;

        public AttackAction(DamageHandler damageHandler, Creature attackingCreature, Creature targetCreature, GridPosition fromPosition, GridPosition toPosition)
        {
            if (attackingCreature is null) throw new ArgumentNullException(nameof(attackingCreature));
            if (targetCreature is null) throw new ArgumentNullException(nameof(targetCreature));
            if (damageHandler is null) throw new ArgumentNullException(nameof(damageHandler));

            _damageHandler = damageHandler;
            _attackingCreature = attackingCreature;
            _targetCreature = targetCreature;
            _fromPosition = fromPosition;
            _toPosition = toPosition;
        }

        public override bool CanExecute()
        {
            if (_attackingCreature.Position != _fromPosition)
                throw new InvalidOperationException("La position initiale n'est plus valide");
            if (_targetCreature.Position != _toPosition)
                throw new InvalidOperationException("La position target n'est plus valide");
            if (_targetCreature.IsDead)
                return false;

            return true;
        }

        protected override void Execute()
        {
            if (_targetCreature.IsDead)
                throw new InvalidOperationException("La cible est déjà morte, l'attaque n'aurait pas du pouvoir être relancée !");
            
            int damage = _attackingCreature.AttackDamage; //TEMPORARY -> sera modulé par la def plus tard
            _damageHandler.ApplyDamage(_targetCreature, damage);
        }
    }
}
