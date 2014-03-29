using System;
using Quartz.Spi;
using Topshelf.HostConfigurators;

namespace Topshelf.SimpleInjector.Quartz
{
    public static class ScheduleJobHostConfiguratorExtensions
    {
        public static HostConfigurator UsingQuartzJobFactory<TJobFactory>(this HostConfigurator configurator, Func<TJobFactory> jobFactory)
            where TJobFactory : IJobFactory
        {
            ScheduleJobServiceConfiguratorExtensions.JobFactory = jobFactory();
            return configurator;
        }

        public static HostConfigurator UsingQuartzJobFactory<TJobFactory>(this HostConfigurator configurator)
            where TJobFactory : IJobFactory, new()
        {
            return UsingQuartzJobFactory(configurator, () => new TJobFactory());
        }

        //public static HostConfigurator ScheduleQuartzJobAsService(this HostConfigurator configurator, Action<QuartzConfigurator> jobConfigurator)
        //{
        //    configurator.Service<NullService>(s => s
        //                                            .ScheduleQuartzJob(jobConfigurator)
        //                                            .WhenStarted(p => p.Start())
        //                                            .WhenStopped(p => p.Stop())
        //                                            .ConstructUsing(settings => new NullService())
        //                                            );

        //    return configurator;
        //}
    }
}
