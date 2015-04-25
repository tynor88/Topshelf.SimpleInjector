using System;
using System.Reflection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using SimpleInjector;
using Topshelf.Logging;
using Topshelf.ServiceConfigurators;

namespace Topshelf.SimpleInjector.Quartz
{
    public static class SimpleInjectorScheduleJobServiceConfiguratorExtensions
    {
        public static ServiceConfigurator<T> UseQuartzSimpleInjector<T>(this ServiceConfigurator<T> configurator, params Assembly[] jobAssemblies)
            where T : class
        {
            SetupQuartzSimpleInjector(jobAssemblies);

            return configurator;
        }

        internal static void SetupQuartzSimpleInjector(params Assembly[] jobAssemblies)
        {
            var log = HostLogger.Get(typeof(SimpleInjectorScheduleJobServiceConfiguratorExtensions));

            Container container = SimpleInjectorHostBuilderConfigurator.Container;

            if (container == null)
                throw new Exception("You must call UseSimpleInjector() to use the Topshelf SimpleInjector Quartz integration.");

            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();

            container.RegisterSingle<IJobFactory>(new SimpleInjectorJobFactory(container, jobAssemblies));
            container.RegisterSingle<ISchedulerFactory>(schedulerFactory);
            container.RegisterSingle<IScheduler>(() =>
            {
                IScheduler scheduler = schedulerFactory.GetScheduler();
                scheduler.JobFactory = container.GetInstance<IJobFactory>();
                return scheduler;
            });

            Func<IScheduler> schedulerFunc = () => container.GetInstance<IScheduler>();

            ScheduleJobServiceConfiguratorExtensions.SchedulerFactory = schedulerFunc;

            log.Info("[Topshelf.SimpleInjector.Quartz] Quartz configured to construct jobs with SimpleInjector.");
        }
    }
}
