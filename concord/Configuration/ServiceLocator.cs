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
        }

        public static ServiceLocator Instance
        {
            get { return LazyInstance.Value; }
        }

        public T Get<T>()
        {
            return ContainerReference.Container.GetInstance<T>();
        }

        public void Inject<T, TInstance>()
            where T : class
            where TInstance : T
        {
            var container = ContainerReference.Container;
            //var instance = container.GetInstance<TInstance>();
            //container.Inject<T>(instance);
            container.EjectAllInstancesOf<T>();
            container.Configure(x => x.For<T>().Use<TInstance>());
        }
    }
}