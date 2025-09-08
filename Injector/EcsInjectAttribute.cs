using System;

namespace Exerussus.EcsProtoModules.Injector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class EcsInjectAttribute : Attribute
    {
        
    }
}