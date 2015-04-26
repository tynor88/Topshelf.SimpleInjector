Topshelf.SimpleInjector [![Build Status](https://travis-ci.org/tynor88/Topshelf.SimpleInjector.svg?branch=master)](https://travis-ci.org/tynor88/Topshelf.SimpleInjector) [![NuGet Version](http://img.shields.io/nuget/v/Topshelf.SimpleInjector.svg?style=flat)](https://www.nuget.org/packages/Topshelf.SimpleInjector/) [![NuGet Downloads](http://img.shields.io/nuget/dt/Topshelf.SimpleInjector.svg?style=flat)](https://www.nuget.org/packages/Topshelf.SimpleInjector/)
=======================

Put your apps on the Topshelf, with the power of SimpleInjector! Topshelf.SimpleInjector provides extensions to construct your Topshelf service class from the SimpleInjector IoC container.

Install
=======================
You can find the package on [NuGet Gallery](https://www.nuget.org/packages/Topshelf.SimpleInjector/).

To install Topshelf.SimpleInjector, run the following command in the Package Manager Console:
`Install-Package Topshelf.SimpleInjector`

Quick Start
=======================
You can also find a Quick Start package on [NuGet Gallery](https://www.nuget.org/packages/Topshelf.SimpleInjector.QuickStart/). This will quickly get you up and running with your Topshelf Windows Service and SimpleInjector IoC Framework, when following these steps:

- 1) Create a new Console Application in Visual Studio
- 2) Install Topshelf.SimpleInjector.QuickStart, running the following command in the Package Manager Console:
`Install-Package Topshelf.SimpleInjector.QuickStart`
- 3) When your asked to overwrite Program.cs click Yes
- 4) Click start and you will have a running service
- 5) Change the service name and implement your logic :-)

Example Usage
=======================
The sample code below, is basically the same as the code you will get in the Quick Start package.
```csharp
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
            //This does not need to be explicitly registered
            _container.Register<SampleService>();

            //Check container for errors
            _container.Verify();

            HostFactory.Run(config =>
            {
                // Pass it to Topshelf
                config.UseSimpleInjector(_container);

                config.Service<SampleService>(s =>
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
            private readonly ISampleDependency2 _sample2;

            public SampleService(ISampleDependency sample, ISampleDependency2 sample2)
            {
                _sample = sample;
                _sample2 = sample2;
            }

            public bool Start()
            {
                Console.WriteLine("Sample Service Started.");
                Console.WriteLine("Sample Dependency: {0}", _sample);
                Console.WriteLine("Sample Dependency2: {0}", _sample2);
                return _sample != null && _sample2 != null;
            }

            public bool Stop()
            {
                return _sample != null && _sample2 != null;
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
```

References
=======================
- [Topshelf](http://topshelf-project.com)
- [SimpleInjector](https://simpleinjector.org)
- [Quartz.NET](http://www.quartz-scheduler.net)
- [Topshelf.Autofac](https://github.com/alexandrnikitin/Topshelf.Autofac)
- [Topshelf.Integrations](https://github.com/dtinteractive/Topshelf.Integrations)

Copyright & License
=======================
Copyright 2014 tynor88

Licensed under the [MIT License](https://github.com/tynor88/Topshelf.SimpleInjector/blob/master/LICENSE)
