using System;
using System.Collections.Generic;

namespace CentralTech.CTSystemsBase
{
    public interface IGenericSystem
    {
        Type Interface { get; }
        void Destroy();
    }

    public class SystemProvider
    {
        public static SystemProvider Instance = new SystemProvider();
        Dictionary<Type, IGenericSystem> _systemDictionary;

        public SystemProvider()
        {
            _systemDictionary = new Dictionary<Type, IGenericSystem>();
        }

        public void Register(IGenericSystem system)
        {
            _systemDictionary.Add(system.Interface, system);
        }

        public void Unregister<T>(T system) where T : IGenericSystem
        {
            system.Destroy();
            _systemDictionary.Remove(system.Interface);
        }

        public T GetSystem<T>() where T : IGenericSystem
        {
            Type type = typeof(T);

            if (_systemDictionary.TryGetValue(type, out IGenericSystem system))
            {
                return (T)system;
            }

            return default;
        }
    }
}

