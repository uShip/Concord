using System;

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
    }
}