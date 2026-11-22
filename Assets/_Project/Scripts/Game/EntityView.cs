using FutharksAdventure.Core;
using UnityEngine;

namespace FutharksAdventure.Game
{
    public class EntityView : MonoBehaviour
    {

        private Entity _entity;
        private EventBus _eventBus;
        private GridView _gridView;

        //--- Ctor -------------
        public void Init(Entity entity, EventBus eventBus, GridView gridView)
        {
            _entity = entity;
            _eventBus = eventBus;
            _gridView = gridView;

            _eventBus.Subscribe<EntityMoved>(OnEntityMoved);
            SetPosition();
        }

        //--- Life Cycle --------
        private void OnEnable()
        {
            _eventBus?.Subscribe<EntityMoved>(OnEntityMoved);
            SetPosition();
        }
        private void OnDisable()
        {
            _eventBus?.Unsubscribe<EntityMoved>(OnEntityMoved);
        }

        //--- Specific Methods --
        private void OnEntityMoved(EntityMoved evt)
        {
            if(evt.Entity == _entity)
                SetPosition();
        }

        private void SetPosition()
        {
            if (_entity == null || _gridView == null)
                return;

            transform.position = _gridView.CellToWorld(_entity.Position);
        }
    }
}
