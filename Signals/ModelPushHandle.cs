using System.Runtime.CompilerServices;

namespace Exerussus.EcsProtoModules.Signals
{
    public readonly struct ModelPushHandle
    {
        internal ModelPushHandle(int id, SignalHandler signalHandler)
        {
            Id = id;
            SignalHandler = signalHandler;
        }

        internal readonly int Id;
        internal readonly SignalHandler SignalHandler;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push<T>(T signal) where T : struct
        {
            SignalHandler.PushAsModel(Id, signal);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push<T>() where T : struct
        {
            SignalHandler.PushAsModel(Id, new T());
        }
    }
}