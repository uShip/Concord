using System;
using StructureMap;

namespace concord.Configuration
{
    public class ServiceLocator
    {
        private static readonly Lazy<ServiceLocator> LazyInstance
            = new Lazy<ServiceLocator>(() => new ServiceLocator());

        private ServiceLocator()
        {
            ObjectFactory.Configure(x => x.AddRegistry<RunnerRegistry>());
        }

        public static ServiceLocator Instance
        {
            get { return LazyInstance.Value; }
        }

        public T Get<T>()
        {
            return ObjectFactory.GetInstance<T>();
        }
    }
}