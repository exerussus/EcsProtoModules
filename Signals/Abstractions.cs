using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Exerussus.EcsProtoModules.Signals
{
    public interface ISignalApiPusher<TSignal> where TSignal : struct
    {
        public abstract ApiPushHandle ApiPushHandle { get; set; }
        
#if UNITY_EDITOR || DEBUG
        [Preserve]
        internal void RegisterPusher(SignalHandler signalHandler)
        {
            Debug.Log($"RegisterPusher of type {GetType().Name} with signal {typeof(TSignal).Name}");
            if (ApiPushHandle.Id == 0 || ApiPushHandle.SignalHandler == null)
            {
                signalHandler.FreeSystemId++;
                ApiPushHandle = new ApiPushHandle(signalHandler.FreeSystemId, signalHandler);
            }
            signalHandler.RegisterPusher<TSignal>(ApiPushHandle.Id, GetType());
        }
#endif
    }
    
    public interface ISignalApiPuller<TSignal> where TSignal : struct
    {
        public abstract void OnApiSignal(TSignal signal);
        
        [Preserve]
        internal void RegisterPuller(SignalHandler signalHandler)
        {
            Debug.Log($"RegisterPuller of type {GetType().Name} with signal {typeof(TSignal).Name}");
            Action<TSignal> pull = OnApiSignal;
#if UNITY_EDITOR || DEBUG
            signalHandler.SubscribeFromApi(pull);
#else
            signalHandler.SubscribeFromApi(pull);
#endif
        }
    }
    
    public interface ISignalModelPusher<TSignal> where TSignal : struct
    {
        public abstract ModelPushHandle ModelPushHandle { get; set; }
        
#if UNITY_EDITOR || DEBUG
        [Preserve]
        internal void RegisterPusher(SignalHandler signalHandler)
        {
            Debug.Log($"RegisterPusher of type {GetType().Name} with signal {typeof(TSignal).Name}");
            if (ModelPushHandle.Id == 0 || ModelPushHandle.SignalHandler == null)
            {
                signalHandler.FreeSystemId++;
                ModelPushHandle = new ModelPushHandle(signalHandler.FreeSystemId, signalHandler);
            }
            signalHandler.RegisterPusher<TSignal>(ModelPushHandle.Id, GetType());
        }
#endif
    }
    
    public interface ISignalModelPuller<TSignal> where TSignal : struct
    {
        public abstract void OnModelSignal(TSignal signal);
        
        [Preserve]
        internal void RegisterPuller(SignalHandler signalHandler)
        {
            Debug.Log($"RegisterPuller of type {GetType().Name} with signal {typeof(TSignal).Name}");
            Action<TSignal> pull = OnModelSignal;
            
            #if UNITY_EDITOR || DEBUG
            signalHandler.SubscribeAsModel(GetType(), pull);
            #else
            signalHandler.SubscribeAsModel(pull);
            #endif
        }
    }
}