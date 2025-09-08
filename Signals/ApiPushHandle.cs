using System.Runtime.CompilerServices;

namespace Exerussus.EcsProtoModules.Signals
{
    public struct ApiPushHandle
    {
        internal ApiPushHandle(int id, SignalHandler signalHandler)
        {
            Id = id;
            SignalHandler = signalHandler;
        }

        internal readonly int Id;
        internal readonly SignalHandler SignalHandler;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push<T>(T signal) where T : struct
        {
            SignalHandler.PushFromApi(Id, signal);
        }
    }
}