using System;
using Quartz;
using SimpleInjector;

namespace Topshelf.SimpleInjector.Quartz.Sample
{
    class Program
    {
        private static readonly Container _container = new Container();

        static void Main(string[] args)
        {
            //Register services
            _container.Register<ISampleDependency, SampleDependency>();
            //This does not need to be explicitly registered
            _container.Register<SampleService>();
            _container.Register<IDependencyInjected, DependencyInjected>();

            HostFactory.Run(config =>
            {
                config.UseQuartzSimpleInjector(_container);

                //Check container for errors
                _container.Verify();

                config.Service<SampleService>(s =>
                {
                    s.ScheduleQuartzJob(
                        configurator =>
                            configurator.WithJob(
                                () => JobBuilder.Create<WithInjectedDependenciesJob>().WithIdentity("WithInjectedDependenciesJob").Build())
                                .AddTrigger(() =>
                                    TriggerBuilder.Create()
                                        .WithSimpleSchedule(
                                            builder => builder.WithIntervalInSeconds(1).RepeatForever()).Build()));

                    s.ScheduleQuartzJob(configurator =>
                        configurator.WithCronSchedule<CronScheduledJob>("0/1 * * * * ?", "CronScheduledJob"));

                    s.ScheduleQuartzJob(configurator =>
                        configurator.WithSimpleRepeatableSchedule<SimpleRepeatableScheduledJob>(
                            TimeSpan.FromSeconds(1), "SimpleRepeatableScheduledJob"));

                    // Let Topshelf use it
                    s.ConstructUsingSimpleInjector();
                    s.WhenStarted((service, control) => service.Start());
                    s.WhenStopped((service, control) => service.Stop());
                });
            });
        }

        public class SampleService
        {
            private readonly ISampleDependency _sample;

            public SampleService(ISampleDependency sample)
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
            void TestInjected();
        }

        public class SampleDependency : ISampleDependency
        {
            public void TestInjected()
            {
                Console.WriteLine("[" + typeof(SampleDependency).Name + "] Triggered " + DateTime.Now.ToLongTimeString());
            }
        }

        public class SampleJob : IJob
        {
            private readonly ISampleDependency _sampleDependency;

            public SampleJob(ISampleDependency sampleDependency)
            {
                _sampleDependency = sampleDependency;
            }

            public void Execute(IJobExecutionContext context)
            {
                _sampleDependency.TestInjected();
            }
        }

        public class WithInjectedDependenciesJob : IJob
        {
            private readonly IDependencyInjected _dependencyInjected;

            public WithInjectedDependenciesJob(IDependencyInjected dependencyInjected)
            {
                _dependencyInjected = dependencyInjected;
            }

            public void Execute(IJobExecutionContext context)
            {
                _dependencyInjected.Execute();
            }
        }

        public interface IDependencyInjected
        {
            void Execute();
        }

        public class DependencyInjected : IDependencyInjected
        {
            public void Execute()
            {
                Console.WriteLine("[" + typeof(DependencyInjected).Name + "] Triggered " + DateTime.Now.ToLongTimeString());
            }
        }

        public class SimpleRepeatableScheduledJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                Console.WriteLine("[" + typeof(SimpleRepeatableScheduledJob).Name + "] Triggered " + DateTime.Now.ToLongTimeString());
            }
        }

        public class CronScheduledJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                Console.WriteLine("[" + typeof(CronScheduledJob).Name + "] Triggered " + DateTime.Now.ToLongTimeString());
            }
        }
    }
}
