using System;
using System.Runtime.CompilerServices;

namespace Exerussus.EcsProtoModules.Signals
{
    public class EcsSignalApi
    {
        internal SignalHandler _signalHandler;
        
        internal void ProvideHandler(SignalHandler signalHandler)
        {
            _signalHandler = signalHandler;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push<T>(T signal) where T : struct
        {
            _signalHandler.PushInApi(signal);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push<T>() where T : struct
        {
            _signalHandler.PushInApi(new T());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subscribe<T>(Action<T> action) where T : struct
        {
            _signalHandler.SubscribeFromApi(action);
        }
    }
}