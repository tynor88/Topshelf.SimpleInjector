using System.Reflection;
using SimpleInjector;
using Topshelf.HostConfigurators;

namespace Topshelf.SimpleInjector.Quartz
{
    public static class SimpleInjectorScheduleJobHostConfiguratorExtensions
    {
        public static HostConfigurator UseQuartzSimpleInjector(this HostConfigurator configurator, Container container)
        {
            // Pass it to Topshelf
            configurator.UseSimpleInjector(container);

            SimpleInjectorScheduleJobServiceConfiguratorExtensions.SetupQuartzSimpleInjector();

            return configurator;
        }

        /// <summary>
        /// Configure Quartz with Simple Injector to support decorators for IJob implementations
        /// </summary>
        /// <param name="configurator">The HostConfigurator</param>
        /// <param name="container">The Simple Injector container</param>
        /// <param name="jobAssemblies">The assemblies where the jobs (IJob implementations / decorators) are</param>
        /// <returns></returns>
        public static HostConfigurator UseQuartzSimpleInjectorWithDecorators(this HostConfigurator configurator, Container container, params Assembly[] jobAssemblies)
        {
            // Pass it to Topshelf
            configurator.UseSimpleInjector(container);

            SimpleInjectorScheduleJobServiceConfiguratorExtensions.SetupQuartzSimpleInjector(jobAssemblies);

            return configurator;
        }
    }
}
