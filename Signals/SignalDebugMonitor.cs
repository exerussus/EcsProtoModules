#if UNITY_EDITOR || DEBUG
using System;
using System.Text;
using Leopotam.EcsProto;
using UnityEngine;

namespace Exerussus.EcsProtoModules.Signals.Editor
{
    public class SignalDebugMonitor : MonoBehaviour
    {
        [TextArea(50, 130)]
        public string message;
        
        public class System : IProtoInitSystem, IProtoRunSystem
        {
            public System(SignalDebugMonitor signalDebugMonitor)
            {
                _signalDebugMonitor = signalDebugMonitor;
            }

            private readonly SignalDebugMonitor _signalDebugMonitor;
            private SignalHandler _sh;
            private float _nextTimeUpdate;
            private const float UpdateInterval = 1f;
            private bool _isInit;
            
            public void Init(IProtoSystems systems)
            {
                foreach (var system in systems.Systems())
                {
                    if (system is SignalModule.RegisterSystem registerSystem)
                    {
                        _sh = registerSystem._signalHandler;
                        _isInit = true;
                        return;
                    }
                }

                throw new Exception($"Не удалось найти систему {nameof(SignalModule.RegisterSystem)}");
            }
            
            public void Run()
            {
                if (!_isInit) return;
                if (_nextTimeUpdate > Time.time) return;
                _nextTimeUpdate = Time.time + UpdateInterval;

                var message = new StringBuilder();

                message.AppendLine($"Last time update : {(int)Time.time}\n");

                CheckPushers(message);
                CheckPullers(message);
                
                _signalDebugMonitor.message = message.ToString();
            }

            private void CheckPushers(StringBuilder sb)
            {
                foreach (var (signalType, pushers) in _sh._registeredPushers)
                {
                    if (!_sh._registeredPullersTypes.TryGetValue(signalType, out var pullers) || pullers.Count == 0)
                    {
                        foreach (var pusherId in pushers)
                        {
                            sb.AppendLine($"Pusher of type {_sh._registeredPushersTypes[pusherId]} has no pullers for signal {signalType}.");
                        }
                    }
                }
            }

            private void CheckPullers(StringBuilder sb)
            {
                foreach (var (signalType, pullers) in _sh._registeredPullersTypes)
                {
                    if (!_sh._registeredPushers.TryGetValue(signalType, out var pushers) || pushers.Count == 0)
                    {
                        foreach (var pullerType in pullers)
                        {
                            sb.AppendLine($"Puller of type {pullerType} has no pusher for signal {signalType}.");
                        }
                    }
                }
            }
            
        }
    }
}
#endif