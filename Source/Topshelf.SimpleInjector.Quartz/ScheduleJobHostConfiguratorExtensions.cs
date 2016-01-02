using System;
using Quartz.Spi;
using Topshelf.HostConfigurators;

namespace Topshelf.SimpleInjector.Quartz
{
    public static class ScheduleJobHostConfiguratorExtensions
    {
        /// <summary>
        /// Provide your own IJobFactory implementation for SimpleInjector to resolve IJob instances
        /// </summary>
        /// <typeparam name="TJobFactory">The JobFactory type</typeparam>
        /// <param name="configurator">The hostconfigurator</param>
        /// <param name="jobFactory">The Func to create a new TJobFactory</param>
        /// <returns>The hostconfigurator</returns>
        public static HostConfigurator UsingQuartzJobFactory<TJobFactory>(this HostConfigurator configurator, Func<TJobFactory> jobFactory)
            where TJobFactory : IJobFactory
        {
            ScheduleJobServiceConfiguratorExtensions.JobFactory = jobFactory();
            return configurator;
        }

        /// <summary>
        /// Schedule a Quartz Job as a Service
        /// </summary>
        /// <param name="configurator">The HostConfigurator</param>
        /// <param name="jobConfigurator">The jobConfigurator Action</param>
        /// <returns>HostConfigurator</returns>
        public static HostConfigurator ScheduleQuartzJobAsService(this HostConfigurator configurator, Action<QuartzConfigurator> jobConfigurator)
        {
            configurator.Service<NullService>(s => s
                .ScheduleQuartzJob(jobConfigurator)
                .WhenStarted(p => p.Start())
                .WhenStopped(p => p.Stop())
                .ConstructUsing(settings => new NullService())
                );

            return configurator;
        }
    }
}