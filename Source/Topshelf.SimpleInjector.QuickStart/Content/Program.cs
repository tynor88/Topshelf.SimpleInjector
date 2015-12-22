using SimpleInjector;

namespace Topshelf.SimpleInjector.QuickStart.Content
{
    class Program
    {
        private static void Main()
        {
            // Create a new Simple Injector container
            Container container = new Container();

            // Configure the Container
            ConfigureContainer(container);

            // Optionally verify the container's configuration to check for configuration errors.
            container.Verify();

            HostFactory.Run(config =>
            {
                // Pass it to Topshelf
                config.UseSimpleInjector(container);

                config.Service<Service>(s =>
                {
                    // Let Topshelf use it
                    s.ConstructUsingSimpleInjector();
                    s.WhenStarted((service, control) => service.Start());
                    s.WhenStopped((service, control) => service.Stop());
                    //s.WhenPaused((service, control) => service.Pause());
                });

                config.SetServiceName("My.Cool.Service");
                config.SetDisplayName("My Cool Service");
                config.SetDescription("What is my purpose?");
            });
        }

        /// <summary>
        /// Register services here
        /// </summary>
        /// <param name="container"></param>
        private static void ConfigureContainer(Container container)
        {
            //Register the service
            container.Register<Service>();

            //Register dependencies
            container.Register<IDependency, Dependency>();
        }
    }
}
