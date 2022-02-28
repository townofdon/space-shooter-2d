using System.Collections.Generic;
using UnityEngine;

namespace FSM
{
    public class FiniteStateMachine : MonoBehaviour
    {
        [SerializeField] BaseState _initialState;
        Dictionary<System.Type, Component> _cachedComponents;

        BaseState currentState;

        public void SetState(BaseState state) {
            if (state == currentState) return;
            if (currentState != null) currentState.End(this);
            currentState = state;
            if (currentState != null) currentState.Begin(this);
        }

        void Awake()
        {
            _cachedComponents = new Dictionary<System.Type, Component>();
            currentState = _initialState;
            if (currentState != null) currentState.Begin(this);
        }

        void Update()
        {
            if (currentState != null) currentState.Execute(this);
        }

        // overridden GetComponent caches component in a dictionary
        public new T GetComponent<T>() where T : Component
        {
            if(_cachedComponents.ContainsKey(typeof(T)))
                return _cachedComponents[typeof(T)] as T;

            var component = base.GetComponent<T>();
            if(component != null)
            {
                _cachedComponents.Add(typeof(T), component);
            }
            return component;
        }
    }
}
