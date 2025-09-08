using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Leopotam.EcsProto;
using UnityEngine;

namespace Exerussus.EcsProtoModules.Signals
{
    public class SignalModule : IProtoModule
    {
        public SignalModule(EcsSignalApi api)
        {
            _api = api;
            _api._signalHandler = _signalHandler = api._signalHandler ?? new SignalHandler();
        }

        private readonly EcsSignalApi _api;
        private readonly SignalHandler _signalHandler;
        
        public void Init(IProtoSystems systems)
        {
            systems.AddSystem(new RegisterSystem(_signalHandler));
        }

        public IProtoAspect[] Aspects() => null;

        public Type[] Dependencies() => null;

        internal class RegisterSystem : IProtoInitSystem, IProtoDestroySystem
        {
            public RegisterSystem(SignalHandler signal)
            {
                _signalHandler = signal;
            }
            
            internal readonly SignalHandler _signalHandler;

            public void Init(IProtoSystems systems)
            {
                Debug.Log("SignalModule.Init!");
                var stringBuilder = new StringBuilder();
                foreach (var system in systems.Systems())
                {
                    stringBuilder.Append($"{system.GetType().Name}\n");
                    TryRegisterModelPuller(system);
                    TryRegisterApiPuller(system);
#if UNITY_EDITOR || DEBUG
                    TryRegisterApiPusher(system);
                    TryRegisterModelPusher(system);
#endif
                }
                
                Debug.Log(stringBuilder);
            }
            
#if UNITY_EDITOR || DEBUG

            private void TryRegisterApiPusher(IProtoSystem system)
            {
                var type = system.GetType();

                var pullerAsModelInterfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISignalApiPusher<>))
                    .Reverse();
                    
                foreach (var itf in pullerAsModelInterfaces)
                {
                    var method = itf.GetMethod("RegisterPusher", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (method == null) throw new InvalidOperationException($"У интерфейса {itf} нет RegisterPusher");
                    method.Invoke(system, new object[] { _signalHandler });
                }
            }

            private void TryRegisterModelPusher(IProtoSystem system)
            {
                var type = system.GetType();

                var pullerAsModelInterfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISignalModelPusher<>))
                    .Reverse();
                    
                foreach (var itf in pullerAsModelInterfaces)
                {
                    var method = itf.GetMethod("RegisterPusher", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (method == null) throw new InvalidOperationException($"У интерфейса {itf} нет RegisterPusher");
                    method.Invoke(system, new object[] { _signalHandler });
                }
            }                 
#endif
            
            private void TryRegisterApiPuller(IProtoSystem system)
            {
                var type = system.GetType();

                var pullerAsModelInterfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISignalApiPuller<>))
                    .Reverse();
                    
                foreach (var itf in pullerAsModelInterfaces)
                {
                    var method = itf.GetMethod("RegisterPuller", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (method == null) throw new InvalidOperationException($"У интерфейса {itf} нет RegisterPuller");
                    method.Invoke(system, new object[] { _signalHandler });
                }
            }

            private void TryRegisterModelPuller(IProtoSystem system)
            {
                var type = system.GetType();

                var pullerAsModelInterfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISignalModelPuller<>))
                    .Reverse();
                    
                foreach (var itf in pullerAsModelInterfaces)
                {
                    var method = itf.GetMethod("RegisterPuller", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (method == null) throw new InvalidOperationException($"У интерфейса {itf} нет RegisterPuller");
                    method.Invoke(system, new object[] { _signalHandler });
                }
            }

            public void Destroy()
            {
                _signalHandler.Destroy();
            }
        }
    }
}