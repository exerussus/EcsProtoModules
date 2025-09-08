using System;
using System.Collections.Generic;
using UnityEngine;

namespace Exerussus.EcsProtoModules.Signals
{
    internal class SignalHandler
    {
        #region Fields

#if UNITY_EDITOR || DEBUG
        // <pusherId, pusherType>
        internal readonly Dictionary<int, Type> _registeredPushersTypes = new ();
        // <signalType, HashSet<SystemType>>
        internal readonly Dictionary<Type, HashSet<Type>> _registeredPullersTypes = new ();
        // <signalType, HashSet<pusherId>>
        internal readonly Dictionary<Type, HashSet<int>> _registeredPushers = new ();
#endif
        
        internal readonly Dictionary<Type, object> _apiInSubscribers = new ();
        internal readonly Dictionary<Type, object> _apiFromSubscribers = new ();
        internal readonly Dictionary<Type, object> _modelSubscribers = new ();
        internal readonly Dictionary<(int filterId, Type signalType), object> _modelSubscribersWithFilter = new ();
        internal int FreeSystemId;

        #endregion

        #region API Methods

        /// <summary> Пуш извне к подписчикам модели. </summary>
        public void PushInApi<T>(T signal) where T : struct
        {
            var type = typeof(T);
            if (!_apiInSubscribers.TryGetValue(type, out var raw)) return;
            var actions = (ActionWrapper<T>)raw;
            actions.Invoke(signal);
        }
        
        /// <summary> Регистрация внешних подписчиков на сигналы внутри модели. </summary>
        public void SubscribeInApi<T>(Action<T> action) where T : struct
        {
            var type = typeof(T);
            ActionWrapper<T> wrapper;
            
            if (_apiFromSubscribers.TryGetValue(type, out var raw))
            {
                wrapper = (ActionWrapper<T>)raw;
            }
            else
            {
                wrapper = new ActionWrapper<T>();
                _apiFromSubscribers[type] = wrapper;
            }
            
            wrapper.Actions += action;
        }
        
        /// <summary> Пуш внутри модели в сторону API. </summary>
        public void PushFromApi<T>(int id, T signal) where T : struct
        {
            var type = typeof(T);
            if (!_apiFromSubscribers.TryGetValue(type, out var raw)) return;
            var actions = (ActionWrapper<T>)raw;
            actions.Invoke(signal);
        }
        
        /// <summary> Модель подписывается на сигналы извне. </summary>
        public void SubscribeFromApi<T>(Action<T> action) where T : struct
        {
            var type = typeof(T);
            ActionWrapper<T> wrapper;
            
            if (_apiInSubscribers.TryGetValue(type, out var raw))
            {
                wrapper = (ActionWrapper<T>)raw;
            }
            else
            {
                wrapper = new ActionWrapper<T>();
                _apiInSubscribers[type] = wrapper;
            }
            
            wrapper.Actions += action;
        }
        
        #endregion
        
#if UNITY_EDITOR || DEBUG
        public void RegisterPusher<T>(int id, Type pusherType) where T : struct
        {
            var type = typeof(T);
            _registeredPushersTypes[id] = pusherType;
            if (!_registeredPushers.TryGetValue(type, out var pushers))
            {
                pushers = new HashSet<int>();
                _registeredPushers[type] = pushers;
            }
            pushers.Add(id);
        }
#endif
        
        public void PushAsModel<T>(int id, T signal) where T : struct
        {
            var type = typeof(T);
            
#if UNITY_EDITOR || DEBUG
            if (id == 0 || !_registeredPushersTypes.TryGetValue(id, out var pusherType))
            {
                LogErrorHelper($"Invalid push handle on signal {type}.");
                return;
            }
            
            if (!_registeredPushers.TryGetValue(type, out var pushers) || !pushers.Contains(id))
            {
                LogErrorHelper($"Signal {type} is not registered for pusher {pusherType.Name}");
                return;
            }
#endif
            
            if (!_modelSubscribers.TryGetValue(type, out var raw)) return;
            var actions = (ActionWrapper<T>)raw;
            actions.Invoke(signal);
        }
        
        public void PushAsModelWithFilter<T>(int id, int filterId, T signal) where T : struct
        {
            var type = typeof(T);
            
#if UNITY_EDITOR || DEBUG
            if (id == 0 || !_registeredPushersTypes.TryGetValue(id, out var pusherType))
            {
                LogErrorHelper($"Invalid push handle on signal {type}.");
                return;
            }
            
            if (!_registeredPushers.TryGetValue(type, out var pushers) || !pushers.Contains(id))
            {
                LogErrorHelper($"Signal {type} is not registered for pusher {pusherType.Name}");
                return;
            }
#endif
            
            if (!_modelSubscribersWithFilter.TryGetValue((filterId, type), out var raw)) return;
            var actions = (ActionWrapper<T>)raw;
            actions.Invoke(signal);
        }

#if UNITY_EDITOR || DEBUG
        public void SubscribeAsModel<T>(Type systemType, Action<T> action) where T : struct
        {
            AddSystemToPullers(systemType, typeof(T));
            SubscribeAsModel(action);
        }
#endif

        public void SubscribeAsModel<T>(Action<T> action) where T : struct
        {
            var type = typeof(T);
            ActionWrapper<T> wrapper;
            
            if (_modelSubscribers.TryGetValue(type, out var raw))
            {
                wrapper = (ActionWrapper<T>)raw;
            }
            else
            {
                wrapper = new ActionWrapper<T>();
                _modelSubscribers[type] = wrapper;
            }
            
            wrapper.Actions += action;
        }

#if UNITY_EDITOR || DEBUG
        public void SubscribeAsModelWithFilter<T>(Type systemType, int filterId, Action<T> action) where T : struct
        {
            AddSystemToPullers(systemType, typeof(T));
            SubscribeAsModelWithFilter(filterId, action);
        }
#endif
        
        public void SubscribeAsModelWithFilter<T>(int filterId, Action<T> action) where T : struct
        {
            var type = typeof(T);
            
            ActionWrapper<T> wrapper;
            
            if (_modelSubscribersWithFilter.TryGetValue((filterId, type), out var raw))
            {
                wrapper = (ActionWrapper<T>)raw;
            }
            else
            {
                wrapper = new ActionWrapper<T>();
                _modelSubscribersWithFilter[(filterId, type)] = wrapper;
            }
            
            wrapper.Actions += action;
        }

#if UNITY_EDITOR || DEBUG
        private void AddSystemToPullers(Type systemType, Type signalType)
        {
            if (!_registeredPullersTypes.TryGetValue(signalType, out var pullers))
            {
                pullers = new HashSet<Type>();
                _registeredPullersTypes[signalType] = pullers;
            }
            
            pullers.Add(systemType);
        }
#endif

        public void Destroy()
        {
            foreach (var sub in _apiInSubscribers.Values) if (sub is IDisposable disposable) disposable.Dispose();
            foreach (var sub in _modelSubscribers.Values) if (sub is IDisposable disposable) disposable.Dispose();
            foreach (var sub in _modelSubscribersWithFilter.Values) if (sub is IDisposable disposable) disposable.Dispose();
            
            _apiInSubscribers.Clear();
            _modelSubscribers.Clear();
            _modelSubscribersWithFilter.Clear();
            
#if UNITY_EDITOR || DEBUG
            _registeredPushersTypes.Clear();
            _registeredPullersTypes.Clear();
            _registeredPushers.Clear();
#endif
        }
        
#if UNITY_EDITOR || DEBUG
        private void LogErrorHelper(string message)
        {
            Debug.LogError($"EcsProto.SignalModule ERROR | {message}");
        }
#endif
    }
}