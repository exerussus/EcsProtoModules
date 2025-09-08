using System;

namespace Exerussus.EcsProtoModules.Signals
{
    public class ActionWrapper<T> : IDisposable where T : struct
    {
        public Action<T> Actions;

        public void Invoke(T signal)
        {
            Actions?.Invoke(signal);
        }

        public void Dispose()
        {
            Actions = null;
        }
    }
}