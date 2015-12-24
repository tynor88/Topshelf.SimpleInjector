using System;
using Quartz;
using Quartz.Impl.Matchers;
using SimpleInjector;
using Topshelf.SimpleInjector.Quartz;

namespace Topshelf.SimpleInjector.QuartzAsService.Sample
{
    internal class Program
    {
        private static readonly Container _container = new Container();

        private static void Main(string[] args)
        {
            try
            {
                //Register services
                _container.Register<IJob, WithInjectedDependenciesJob>();
                _container.Register<IDependencyInjected, DependencyInjected>();

                var jobWithListener = "jobWithListener";
                var jobKey = new JobKey(jobWithListener);

                HostFactory.Run(config =>
                {
                    config.UseQuartzSimpleInjector(_container);

                    //Check container for errors
                    _container.Verify();

                    config.ScheduleQuartzJobAsService(configurator =>
                        configurator.WithJob(
                            () =>
                                JobBuilder.Create<WithInjectedDependenciesJob>()
                                    .WithIdentity(jobKey)
                                    .Build())
                            .AddTrigger(
                                () =>
                                    TriggerBuilder.Create()
                                        .WithIdentity(jobWithListener + ".trigger")
                                        .WithSimpleSchedule(
                                            builder => builder.WithIntervalInSeconds(1).RepeatForever()).Build())
                            .AddJobListener(() => new RecurringJobListener(), KeyMatcher<JobKey>.KeyEquals(jobKey)));
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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

        public class RecurringJobListener : IJobListener
        {
            public void JobExecutionVetoed(IJobExecutionContext context)
            {
            }

            public void JobToBeExecuted(IJobExecutionContext context)
            {
                Console.WriteLine("[" + typeof(RecurringJobListener).Name + "] To be executed");
            }

            public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
            {
                Console.WriteLine("[" + typeof(RecurringJobListener).Name + "] Was executed");
            }

            public string Name => typeof(RecurringJobListener).Name;
        }
    }
}