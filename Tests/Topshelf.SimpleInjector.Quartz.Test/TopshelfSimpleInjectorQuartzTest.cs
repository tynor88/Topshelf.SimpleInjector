using System;
using System.Reflection;
using Moq;
using NUnit.Framework;
using Quartz;
using Quartz.Impl.Matchers;
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

        [Test, RunInApplicationDomain]
        public void QuartzJobIsExecutedSuccessfullyTest()
        {
            //Arrange
            Mock<IJob> testJobMock = new Mock<IJob>();
            _container.RegisterSingleton<IJob>(() => testJobMock.Object);
            _container.Register<ISampleDependency, SampleDependency>();

            //Act
            var host = HostFactory.New(config =>
            {
                config.UseTestHost();
                config.UseQuartzSimpleInjector(_container);
                _container.Verify();
                config.Service<TestService>(s =>
                {
                    s.ScheduleQuartzJob(configurator => configurator.WithSimpleRepeatableSchedule<IJob>(TimeSpan.FromMilliseconds(1)));

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

        [Test, RunInApplicationDomain]
        public void QuartzJobListenerIsExecutedSuccessfullyTest()
        {
            //Arrange
            Mock<IJob> testJobMock = new Mock<IJob>();
            Mock<IJobListener> jobListenerMock = new Mock<IJobListener>();
            jobListenerMock.SetupGet(listener => listener.Name).Returns("jobWithListener");
            _container.RegisterSingleton<IJob>(() => testJobMock.Object);
            _container.Register<ISampleDependency, SampleDependency>();

            var jobWithListener = "jobWithListener";
            var jobKey = new JobKey(jobWithListener);

            //Act
            var host = HostFactory.New(config =>
            {
                config.UseTestHost();
                config.UseQuartzSimpleInjector(_container);
                _container.Verify();
                config.Service<TestService>(s =>
                {
                    s.ScheduleQuartzJob(configurator =>
                        configurator
                            .WithSimpleRepeatableSchedule<IJob>(TimeSpan.FromMilliseconds(1), jobWithListener)
                            .AddJobListener(() => (IJobListener)jobListenerMock.Object, KeyMatcher<JobKey>.KeyEquals(jobKey)));

                    s.ConstructUsingSimpleInjector();
                    s.WhenStarted((service, control) => service.Start());
                    s.WhenStopped((service, control) => service.Stop());
                });
            });

            var exitCode = host.Run();

            //Assert
            Assert.AreEqual(TopshelfExitCode.Ok, exitCode);
            testJobMock.Verify(job => job.Execute(It.IsAny<IJobExecutionContext>()), Times.AtLeastOnce);
            jobListenerMock.Verify(listener => listener.JobToBeExecuted(It.IsAny<IJobExecutionContext>()), Times.AtLeastOnce);
            jobListenerMock.Verify(listener => listener.JobWasExecuted(It.IsAny<IJobExecutionContext>(), It.IsAny<JobExecutionException>()), Times.AtLeastOnce);
        }

        [Test, RunInApplicationDomain]
        public void QuartzJobAsAServiceIsExecutedSuccessfullyTest()
        {
            //Arrange
            Mock<IJob> testJobMock = new Mock<IJob>();
            _container.RegisterSingleton<IJob>(() => testJobMock.Object);
            _container.Register<ISampleDependency, SampleDependency>();

            //Act
            var host = HostFactory.New(config =>
            {
                config.UseTestHost();
                config.UseQuartzSimpleInjector(_container);
                _container.Verify();
                config.ScheduleQuartzJobAsService(configurator =>
                    configurator
                    .WithJob(() => JobBuilder.Create<IJob>().Build())
                    .WithSimpleRepeatableSchedule<IJob>(TimeSpan.FromMilliseconds(1)));
            });

            var exitCode = host.Run();

            //Assert
            Assert.AreEqual(TopshelfExitCode.Ok, exitCode);
            testJobMock.Verify(job => job.Execute(It.IsAny<IJobExecutionContext>()), Times.AtLeastOnce);
        }

        [Test, RunInApplicationDomain]
        public void JobFactoryIsCorrectlyUsedForIJobCreationTest()
        {
            //Arrange
            Mock<IJob> testJobMock = new Mock<IJob>();
            _container.RegisterSingleton<IJob>(() => testJobMock.Object);
            _container.Register<ISampleDependency, SampleDependency>();
            Mock<IJobFactory> factoryMock = new Mock<IJobFactory>();

            //Act
            var host = HostFactory.New(config =>
            {
                config.UsingQuartzJobFactory<IJobFactory>(() => factoryMock.Object);
                config.UseTestHost();
                config.UseQuartzSimpleInjector(_container);
                _container.Verify();
                config.Service<TestService>(s =>
                {
                    s.ScheduleQuartzJob(configurator => configurator.WithSimpleRepeatableSchedule<IJob>(TimeSpan.FromMilliseconds(1)));

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

        [Test, RunInApplicationDomain]
        public void ExceptionIsThrownWhenTheContainerIsNullTest()
        {
            //Arrange
            Container nullContainer = null;

            //Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
                HostFactory.New(config =>
                {
                    config.UseTestHost();
                    config.UseQuartzSimpleInjector(nullContainer);
                }));

            //Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: container", exception.Message);
        }

        [Test, RunInApplicationDomain]
        public void DecoratedJobsAreCorrectlyExecutingTest()
        {
            //Arrange
            Mock<IDecoratorDependency> decoratorDependencyMock = new Mock<IDecoratorDependency>();

            _container.RegisterDecorator<IJob, TestJobDecorator>();
            _container.Register<IDecoratorDependency>(() => decoratorDependencyMock.Object);
            _container.Register<ISampleDependency, SampleDependency>();

            //Act
            var host = HostFactory.New(config =>
            {
                config.UseQuartzSimpleInjectorWithDecorators(_container, Assembly.GetExecutingAssembly());
                config.UseTestHost();
                _container.Verify();
                config.Service<TestService>(s =>
                {
                    s.ScheduleQuartzJob(configurator => configurator.WithSimpleRepeatableSchedule<TestJob>(TimeSpan.FromMilliseconds(1), nameof(TestJob)));

                    s.ConstructUsingSimpleInjector();
                    s.WhenStarted((service, control) => service.Start());
                    s.WhenStopped((service, control) => service.Stop());
                });
            });

            var exitCode = host.Run();

            //Assert
            Assert.AreEqual(TopshelfExitCode.Ok, exitCode);
            decoratorDependencyMock.Verify(dependency => dependency.DoSomething(), Times.AtLeastOnce);
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

    [Serializable]
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

    public interface IDecoratorDependency
    {
        void DoSomething();
    }

    public class TestJobDecorator : IJob
    {
        private readonly IJob _jobDecoratee;
        private readonly IDecoratorDependency _decoratorDependency;

        public TestJobDecorator(IJob jobDecoratee, IDecoratorDependency decoratorDependency)
        {
            _jobDecoratee = jobDecoratee;
            _decoratorDependency = decoratorDependency;
        }

        public void Execute(IJobExecutionContext context)
        {
            _jobDecoratee.Execute(context);
            _decoratorDependency.DoSomething();
        }
    }
}