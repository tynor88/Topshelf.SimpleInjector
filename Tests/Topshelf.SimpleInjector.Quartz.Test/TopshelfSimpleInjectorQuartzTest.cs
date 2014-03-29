using System;
using Moq;
using NUnit.Framework;
using Quartz;
using Quartz.Spi;
using SimpleInjector;

namespace Topshelf.SimpleInjector.Quartz.Test
{
    [TestFixture]
    public class TopshelfSimpleInjectorQuartzTest
    {
        private static Container _container;

        [SetUp]
        public void SetUp()
        {
            _container = new Container();
        }

        [Test]
        public void QuartzJobIsExecutedSuccessfullyTest()
        {
            //Arrange
            Mock<IJob> testJobMock = new Mock<IJob>();
            _container.RegisterSingle<IJob>(() => testJobMock.Object);
            _container.Register<ISampleDependency, SampleDependency>();

            //Act
            var host =
                HostFactory.New(config =>
                {
                    config.UseTestHost();
                    config.UseQuartzSimpleInjector(_container);
                    _container.Verify();
                    config.Service<TestService>(s =>
                    {
                        s.ScheduleQuartzJob(
                            configurator => configurator.WithSimpleRepeatableSchedule<IJob>(TimeSpan.FromMilliseconds(1)));

                        s.ConstructUsingSimpleInjector();
                        s.WhenStarted((service, control) => service.Start());
                        s.WhenStopped((service, control) => service.Stop());
                    });
                });

            var exitCode = host.Run();

            //Assert
            Assert.AreEqual(TopshelfExitCode.Ok, exitCode);
            testJobMock.Verify(job => job.Execute(It.IsAny<IJobExecutionContext>()), Times.AtLeastOnce);
        }

        [Test]
        public void JobFactoryIsCorrectlyUsedForIJobCreationTest()
        {
            //Arrange
            Mock<IJob> testJobMock = new Mock<IJob>();
            _container.RegisterSingle<IJob>(() => testJobMock.Object);
            _container.Register<ISampleDependency, SampleDependency>();
            Mock<IJobFactory> factoryMock = new Mock<IJobFactory>();

            //Act
            var host =
                HostFactory.New(config =>
                {
                    config.UsingQuartzJobFactory<IJobFactory>(() => factoryMock.Object);
                    config.UseTestHost();
                    config.UseQuartzSimpleInjector(_container);
                    _container.Verify();
                    config.Service<TestService>(s =>
                    {
                        s.ScheduleQuartzJob(
                            configurator =>
                                configurator.WithSimpleRepeatableSchedule<IJob>(TimeSpan.FromMilliseconds(1)));

                        s.ConstructUsingSimpleInjector();
                        s.WhenStarted((service, control) => service.Start());
                        s.WhenStopped((service, control) => service.Stop());
                    });
                });

            var exitCode = host.Run();

            //Assert
            Assert.AreEqual(TopshelfExitCode.Ok, exitCode);
            factoryMock.Verify(factory => factory.NewJob(It.IsAny<TriggerFiredBundle>(), It.IsAny<IScheduler>()), Times.AtLeastOnce);
        }
    }

    public class TestService
    {
        private readonly ISampleDependency _sample;

        public TestService(ISampleDependency sample)
        {
            _sample = sample;
        }

        public bool Start()
        {
            Console.WriteLine("Sample Service Started.");
            Console.WriteLine("Sample Dependency: {0}", _sample);
            return _sample != null;
        }

        public bool Stop()
        {
            return _sample != null;
        }
    }

    public interface ISampleDependency
    {
    }

    public class SampleDependency : ISampleDependency
    {
    }

    public class TestJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Executing...");
        }
    }
}
