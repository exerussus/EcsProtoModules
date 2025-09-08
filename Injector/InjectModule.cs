using System;
using System.Reflection;
using System.Text;
using Leopotam.EcsProto;

namespace Exerussus.EcsProtoModules.Injector
{
    public class InjectModule : IProtoModule
    {
        public InjectModule(params object[] references)
        {
            _diContainer = new DIContainer().Add(references);
        }
        
        public InjectModule(params UnityEngine.Object[] references)
        {
            var objects = new object[references.Length];
            for (var index = 0; index < references.Length; index++)
            {
                var obj = references[index];
                objects[index] = obj;
            }
            _diContainer = new DIContainer().Add(objects);
        }

        private readonly DIContainer _diContainer;
        private static readonly Type DiAttrType = typeof(EcsInjectAttribute);
        
        public void Init(IProtoSystems systems)
        {
            systems.AddSystem(new InjectSystem(_diContainer));
        }

        public IProtoAspect[] Aspects() => null;

        public Type[] Dependencies() => null;

        sealed class InjectSystem : IProtoInitSystem
        {
            public InjectSystem(DIContainer diContainer)
            {
                _diContainer = diContainer;
            }

            private readonly DIContainer _diContainer;

            public void Init(IProtoSystems systems)
            {
                foreach (var service in systems.Services().Values) if (service is IEcsInjectable) TryInjectFields(service);
                foreach (var system in systems.Systems()) if (system is IEcsInjectable) TryInjectFields(system);
            }

            public void TryInjectFields(object target)
            {
                foreach (var fi in target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (fi.IsStatic) continue;

                    if (Attribute.IsDefined(fi, DiAttrType))
                    {
                        if (_diContainer.TryGet(fi.FieldType, out var injectObj))
                        {
                            fi.SetValue(target, injectObj);
                        }
                        else
                        {
#if DEBUG
                            throw new Exception(
                                $"Ошибка инъекции данных в \"{CleanTypeName(target.GetType())}\" - тип поля \"{fi.Name}\" отсутствует в контейнере зависимостей.");
#endif
                        }
                    }
                }
            }
        
#if DEBUG || UNITY_EDITOR
            private static string CleanTypeName(Type type)
            {
                string name;
                if (!type.IsGenericType) name = type.Name;
                else
                {
                    var constraints = new StringBuilder();
                    foreach (var constraint in type.GetGenericArguments())
                    {
                        if (constraints.Length > 0) constraints.Append(", ");
                    
                        constraints.Append(CleanTypeName(constraint));
                    }

                    var genericIndex = type.Name.LastIndexOf("`", StringComparison.Ordinal);
                    var typeName = genericIndex == -1
                        ? type.Name
                        : type.Name.Substring(0, genericIndex);
                    name = $"{typeName}<{constraints}>";
                }

                return name;
            }
#endif
        }
    }
}