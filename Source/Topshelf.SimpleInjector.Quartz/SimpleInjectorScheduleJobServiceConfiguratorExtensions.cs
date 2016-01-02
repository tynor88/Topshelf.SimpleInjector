using System;
using System.Linq;
using System.Reflection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using SimpleInjector;
using Topshelf.Logging;
using Topshelf.SimpleInjector.Quartz.Factory;

namespace Topshelf.SimpleInjector.Quartz
{
    internal static class SimpleInjectorScheduleJobServiceConfiguratorExtensions
    {
        internal static void SetupQuartzSimpleInjector()
        {
            RegisterQuartzInSimpleInjector();
        }

        internal static void SetupQuartzSimpleInjector(params Assembly[] jobAssemblies)
        {
            RegisterQuartzInSimpleInjector(jobAssemblies);
        }

        private static void RegisterQuartzInSimpleInjector(Assembly[] jobAssemblies = null)
        {
            var log = HostLogger.Get(typeof(SimpleInjectorScheduleJobServiceConfiguratorExtensions));

            Container container = SimpleInjectorHostBuilderConfigurator.Container;

            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();

            if (jobAssemblies == null)
            {
                container.RegisterSingleton<IJobFactory>(new SimpleInjectorJobFactory(container));
            }
            else
            {
                container.RegisterSingleton<IJobFactory>(new SimpleInjectorDecoratorJobFactory(container, jobAssemblies));
            }

            container.RegisterSingleton<ISchedulerFactory>(schedulerFactory);

            if (!Environment.GetCommandLineArgs().Any(x => x.ToLower() == "install" || x.ToLower() == "uninstall"))
            {
                container.RegisterSingleton<IScheduler>(() =>
                {
                    IScheduler scheduler = schedulerFactory.GetScheduler();
                    scheduler.JobFactory = container.GetInstance<IJobFactory>();
                    return scheduler;
                });
            }

            Func<IScheduler> schedulerFunc = () => container.GetInstance<IScheduler>();

            ScheduleJobServiceConfiguratorExtensions.SchedulerFactory = schedulerFunc;

            log.Info("[Topshelf.SimpleInjector.Quartz] Quartz configured to construct jobs with SimpleInjector.");
        }
    }
}