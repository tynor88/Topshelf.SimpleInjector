using NUnit.Framework;
using SimpleInjector;

namespace Topshelf.SimpleInjector.Test
{
    [TestFixture]
    public class TopshelfSimpleInjectorTest
    {
        private static Container _container;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _container = new Container();

            //Register services
            _container.Register<TestService>();
            _container.Register<ITestDependency, TestDependency>();
        }

        [Test]
        public void ExceptionIsThrownWhenUseSimpleInjectorMethodIsNotCalledTest()
        {
            var exception =
                Assert.Throws<ServiceBuilderException>(() => HostFactory.New(config =>
                {
                    config.UseTestHost();
                    //config.UseSimpleInjector(_container);
                    config.Service<TestService>(s =>
                    {
                        s.ConstructUsingSimpleInjector();
                        s.WhenStarted((service, control) => service.Start());
                        s.WhenStopped((service, control) => service.Stop());
                    });
                }));

            Assert.IsTrue(exception.Message.Contains("An exception occurred creating the service: " + typeof(TestService).Name));
        }

        [Test]
        public void ExceptionIsThrownWhenConstructUsingSimpleInjectorMethodIsNotCalledTest()
        {
            var exception =
                Assert.Throws<HostConfigurationException>(() => HostFactory.New(config =>
                {
                    config.UseTestHost();
                    config.UseSimpleInjector(_container);
                    config.Service<TestService>(s =>
                    {
                        //s.ConstructUsingSimpleInjector();
                        s.WhenStarted((service, control) => service.Start());
                        s.WhenStopped((service, control) => service.Stop());
                    });
                }));

            Assert.IsTrue(exception.Message.Contains("The service was not properly configured"));
            Assert.IsTrue(exception.Message.Contains("Factory must not be null"));
        }

        [Test]
        public void TopShelfUsesSimpleInjectorToResolveDependenciesTest()
        {
            var exitCode =
                HostFactory.Run(config =>
                {
                    config.UseTestHost();
                    config.UseSimpleInjector(_container);
                    config.Service<TestService>(s =>
                    {
                        s.ConstructUsingSimpleInjector();
                        s.WhenStarted((service, control) => service.Start());
                        s.WhenStopped((service, control) => service.Stop());
                    });
                });

            Assert.AreEqual(TopshelfExitCode.Ok, exitCode);
        }

        public class TestService
        {
            public TestService(ITestDependency testDependency) { }

            public bool Start() { return true; }

            public bool Stop() { return true; }
        }

        public interface ITestDependency { }
        public class TestDependency : ITestDependency { }
    }
}
