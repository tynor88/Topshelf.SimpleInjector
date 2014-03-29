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
            _container.Register<IJob, SampleJob>();
            //This does not need to be explicitly registered
            _container.Register<SampleService>();

            HostFactory.Run(config =>
            {
                config.UseQuartzSimpleInjector(_container);

                //Check container for errors
                _container.Verify();

                config.Service<SampleService>(s =>
                {
                    s.ScheduleQuartzJob(
                        configurator =>
                            configurator.WithJob(() => JobBuilder.Create<IJob>().WithIdentity("SampleJob").Build())
                                .AddTrigger(() =>
                                    TriggerBuilder.Create()
                                        .WithSimpleSchedule(
                                            builder => builder.WithIntervalInSeconds(5).RepeatForever()).Build()));

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
            void Hello();
        }

        public class SampleDependency : ISampleDependency
        {
            public void Hello()
            {
                Console.WriteLine("Hello");
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
                _sampleDependency.Hello();
            }
        }
    }
}
