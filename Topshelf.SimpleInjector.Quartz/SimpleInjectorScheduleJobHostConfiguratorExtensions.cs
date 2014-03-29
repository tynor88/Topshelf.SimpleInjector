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
    }
}
