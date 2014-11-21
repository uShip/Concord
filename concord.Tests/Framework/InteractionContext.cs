using System;
using System.Linq.Expressions;
using concord.Configuration;
using NUnit.Framework;
using Rhino.Mocks;
using StructureMap;
using StructureMap.AutoMocking;

namespace concord.Tests.Framework
{
    public class InteractionContext<TClassToTest>
        where TClassToTest : class
    {
        private readonly MockMode _mode;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionContext{T}"/> class.
        /// </summary>
        public InteractionContext()
            : this(MockMode.AAA)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionContext{T}"/> class and instructs
        /// the underlying <see cref="RhinoAutoMocker{TARGETCLASS}"/> to use the specified mocking mode.
        /// </summary>
        /// <param name="mode"></param>
        public InteractionContext(MockMode mode)
        {
            _mode = mode;
        }

        /// <summary>
        /// Gets the underlying <see cref="RhinoAutoMocker{TARGETCLASS}"/>.
        /// </summary>
        public RhinoAutoMocker<TClassToTest> Services { get; private set; }

        /// <summary>
        /// Gets the <see cref="IContainer"/> controlling the context.
        /// </summary>
        public IContainer Container
        {
            get { return Services.Container; }
        }

        /// <summary>
        /// Gets the instance of the class being tested.
        /// </summary>
        public virtual TClassToTest ClassUnderTest
        {
            get { return Services.ClassUnderTest; }
        }

        /// <summary>
        /// Returns the current mocked instance for the specified <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of mocked instance to retrieve.</typeparam>
        /// <returns></returns>
        public TService MockFor<TService>()
            where TService : class
        {
            return Services.Get<TService>();
        }

        /// <summary>
        /// Verifies all expectations for the specified <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of mocked instance to verify.</typeparam>
        public void VerifyCallsFor<TService>()
            where TService : class
        {
            MockFor<TService>().VerifyAllExpectations();
        }

        /// <summary>
        /// Called by the NUnit framework to perform setup tasks.
        /// Context-specific setup tasks should be implemented by overriding the <see cref="BeforeEach"/> method.
        /// </summary>
        [SetUp]
        public virtual void SetUp()
        {
            Services = new RhinoAutoMocker<TClassToTest>(_mode);
            ContainerReference.MockContainer(Container);
            BeforeEach();
        }

        /// <summary>
        /// Called after setup tasks are performed in the <see cref="SetUp"/> method.
        /// </summary>
        protected virtual void BeforeEach()
        {
        }

        public void UseInstanceFor<T>(Expression<Func<T>> funcToGetConcreteClassToUseAsAMock) where T : class
        {
            if (Services.Get<T>().IsMocked())
                Services.Get<T>().ClearBehavior();
            ObjectFactory.Configure(x => x.For<T>().Use(funcToGetConcreteClassToUseAsAMock));
            Services.Container.Configure(x => x.For<T>().Use(funcToGetConcreteClassToUseAsAMock));
        }

        public void UseInstanceFor<T>(T concreteClassToUseAsAMock) where T : class
        {
            if (Services.Get<T>().IsMocked())
                Services.Get<T>().ClearBehavior();
            Services.Inject<T>(concreteClassToUseAsAMock);
        }

        public void UseInstanceFor<T, TInstance>()
            where T : class
            where TInstance : T
        {
            UseInstanceFor<T>(() => Services.Container.GetInstance<TInstance>());
        }

        public void UseConcreteClassFor<T>() where T : class
        {
            Services.UseConcreteClassFor<T>();
        }

        public void ClearBehaviorFor<T>() where T : class
        {
            Services.Get<T>().ClearBehavior();
        }

        public void PartialMockTheClassUnderTest()
        {
            try
            {
                Services.PartialMockTheClassUnderTest();
            }
            // ReSharper disable once RedundantCatchClause
            catch (Exception)
            {
                // here for debugging purposes, put a breakpoint here
                throw;
            }

        }

        [TestFixtureTearDown]
        public void TearDownAndResetObjectFactory()
        {
            ObjectFactory.Initialize(x => { });
        }

        protected void Configure(Action<ConfigurationExpression> configure)
        {
            ObjectFactory.Configure(configure);
            Services.Container.Configure(configure);
        }
    }
}
