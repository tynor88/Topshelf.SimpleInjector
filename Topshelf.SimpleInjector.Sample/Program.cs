using System;
using SimpleInjector;

namespace Topshelf.SimpleInjector.Sample
{
    class Program
    {
        private static readonly Container _container = new Container();
        static void Main(string[] args)
        {
            //Register services
            _container.Register<ISampleDependency, SampleDependency>();
            _container.Register<ISampleDependency2, SampleDependency2>();
            _container.Register<SampleService>(); //This does not need to be explicitly registered

            //Check container for errors
            _container.Verify();

            HostFactory.Run(c =>
            {
                // Pass it to Topshelf
                c.UseSimpleInjector(_container);

                c.Service<SampleService>(s =>
                {
                    // Let Topshelf use it
                    s.ConstructUsingSimpleInjector();
                    s.WhenStarted((service, control) => service.Start());
                    s.WhenStopped((service, control) => service.Stop());
                });
            });
        }

        public class SampleService
        {
            private readonly ISampleDependency _sample;

            public SampleService(ISampleDependency sample, ISampleDependency2 sampleDependecy2)
            {
                _sample = sample;
            }

            public bool Start()
            {
                Console.WriteLine("Sample Service Started.");
                Console.WriteLine("Sample Dependency: {0}", _sample);
                return _sample != null;
            }

            public bool Stop()
            {
                return _sample != null;
            }
        }

        public interface ISampleDependency
        {
        }

        public class SampleDependency : ISampleDependency
        {
        }

        public interface ISampleDependency2
        {
        }

        public class SampleDependency2 : ISampleDependency2
        {
        }
    }
}
