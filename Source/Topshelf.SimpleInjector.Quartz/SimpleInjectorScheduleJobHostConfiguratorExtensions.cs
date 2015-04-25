using System.Reflection;
using SimpleInjector;
using Topshelf.HostConfigurators;

namespace Topshelf.SimpleInjector.Quartz
{
    public static class SimpleInjectorScheduleJobHostConfiguratorExtensions
    {
        public static HostConfigurator UseQuartzSimpleInjector(this HostConfigurator configurator, Container container, params Assembly[] jobAssemblies)
        {
            // Pass it to Topshelf
            configurator.UseSimpleInjector(container);

            SimpleInjectorScheduleJobServiceConfiguratorExtensions.SetupQuartzSimpleInjector(jobAssemblies);

            return configurator;
        }
    }
}
