using System;
using System.Linq;
using Quartz;
using Quartz.Collection;
using Quartz.Spi;
using Topshelf.Logging;
using Topshelf.ServiceConfigurators;

namespace Topshelf.SimpleInjector.Quartz
{
    public static class ScheduleJobServiceConfiguratorExtensions
    {
        private static IScheduler Scheduler;
        internal static IJobFactory JobFactory;

        internal static Func<IScheduler> SchedulerFactory { get; set; }

        private static IScheduler GetScheduler()
        {
            var scheduler = SchedulerFactory();

            if (JobFactory != null)
                scheduler.JobFactory = JobFactory;

            return scheduler;
        }

        public static ServiceConfigurator<T> UsingQuartzJobFactory<T, TJobFactory>(this ServiceConfigurator<T> configurator, Func<TJobFactory> jobFactory)
            where T : class
            where TJobFactory : IJobFactory
        {
            JobFactory = jobFactory();
            return configurator;
        }

        public static ServiceConfigurator<T> UsingQuartzJobFactory<T, TJobFactory>(this ServiceConfigurator<T> configurator)
            where T : class
            where TJobFactory : IJobFactory, new()
        {
            return UsingQuartzJobFactory(configurator, () => new TJobFactory());
        }

        public static ServiceConfigurator<T> ScheduleQuartzJob<T>(this ServiceConfigurator<T> configurator, Action<QuartzConfigurator> jobConfigurator) where T : class
        {
            ConfigureJob<T>(configurator, jobConfigurator);
            return configurator;
        }

        private static void ConfigureJob<T>(ServiceConfigurator<T> configurator, Action<QuartzConfigurator> jobConfigurator) where T : class
        {
            var log = HostLogger.Get(typeof(ScheduleJobServiceConfiguratorExtensions));

            var jobConfig = new QuartzConfigurator();
            jobConfigurator(jobConfig);

            if (jobConfig.JobEnabled == null || jobConfig.JobEnabled() || jobConfig.Job == null || jobConfig.Triggers == null)
            {
                var jobDetail = jobConfig.Job();
                var jobTriggers = jobConfig.Triggers.Select(triggerFactory => triggerFactory()).Where(trigger => trigger != null);

                configurator.BeforeStartingService(() =>
                {
                    log.Debug("[Topshelf.Quartz] Scheduler starting up...");
                    if (Scheduler == null)
                        Scheduler = GetScheduler();

                    if (Scheduler != null)
                    {
                        if (jobDetail != null && jobTriggers.Any())
                        {
                            var triggersForJob = new HashSet<ITrigger>(jobTriggers);
                            Scheduler.ScheduleJob(jobDetail, triggersForJob, false);
                            log.Info($"[Topshelf.Quartz] Scheduled Job: {jobDetail.Key}");

                            foreach (var trigger in triggersForJob)
                            {
                                log.Info($"[Topshelf.Quartz] Job Schedule: {trigger} - Next Fire Time (local): {(trigger.GetNextFireTimeUtc().HasValue ? trigger.GetNextFireTimeUtc().Value.ToLocalTime().ToString() : "none")}");
                            }

                            if (!Scheduler.IsStarted)
                            {
                                Scheduler.Start();
                                log.Info("[Topshelf.Quartz] Scheduler started...");
                            }
                        }

                        foreach (Tuple<Func<IJobListener>, IMatcher<JobKey>[]> jobListenerTuple in jobConfig.JobListeners)
                        {
                            IJobListener jobListener = jobListenerTuple.Item1();
                            IMatcher<JobKey>[] keyEquals = jobListenerTuple.Item2;

                            if (jobListener != null)
                            {
                                Scheduler.ListenerManager.AddJobListener(jobListener, keyEquals);
                            }
                        }
                    }

                });

                configurator.BeforeStoppingService(() =>
                {
                    log.Debug("[Topshelf.Quartz] Scheduler shutting down...");
                    Scheduler.Shutdown(true);
                    log.Info("[Topshelf.Quartz] Scheduler shut down...");
                });
            }
        }
    }
}