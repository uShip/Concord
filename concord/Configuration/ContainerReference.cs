using System;
using System.Threading;
using StructureMap;

namespace concord.Configuration
{
    public static class ContainerReference
    {
        private static Lazy<IContainer> _containerBuilder =
            new Lazy<IContainer>(DefaultContainer, LazyThreadSafetyMode.ExecutionAndPublication);

        public static IContainer Container
        {
            get { return _containerBuilder.Value; }
        }

        private static Container DefaultContainer()
        {
            return new Container(x => x.AddRegistry<RunnerRegistry>());
        }

        public static void MockContainer(IContainer container)
        {
            _containerBuilder = new Lazy<IContainer>(() => container, LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }
}