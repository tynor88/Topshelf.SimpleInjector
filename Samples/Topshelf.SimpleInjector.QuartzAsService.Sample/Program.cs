using System;
using Quartz;
using SimpleInjector;
using Topshelf.SimpleInjector.Quartz;

namespace Topshelf.SimpleInjector.QuartzAsService.Sample
{
    internal class Program
    {
        private static readonly Container _container = new Container();

        private static void Main(string[] args)
        {
            //Register services
            _container.Register<IJob, WithInjectedDependenciesJob>();
            _container.Register<IDependencyInjected, DependencyInjected>();

            HostFactory.Run(config =>
            {
                config.UseQuartzSimpleInjector(_container);

                //Check container for errors
                _container.Verify();

                config.ScheduleQuartzJobAsService(configurator =>
                    configurator.WithJob(
                        () =>
                            JobBuilder.Create<WithInjectedDependenciesJob>()
                                .WithIdentity("WithInjectedDependenciesJob")
                                .Build())
                        .AddTrigger(
                            () =>
                                TriggerBuilder.Create()
                                    .WithSimpleSchedule(
                                        builder =>
                                            builder.WithIntervalInSeconds(1).RepeatForever()).Build()));
            });
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
    }
}
