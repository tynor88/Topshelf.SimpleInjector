using System;
using System.Reflection;
using Quartz;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Topshelf.SimpleInjector.Quartz.Decorators.Sample
{
    internal class Program
    {
        private static readonly Container _container = new Container();

        private static void Main(string[] args)
        {
            _container.Register<IDependencyInjected, DependencyInjected>();
            _container.RegisterDecorator(typeof(IJob), typeof(LoggingDecorator));
            _container.RegisterDecorator(typeof(IJob), typeof(LifetimeScopeDecoratorFuncTFactory));

            HostFactory.Run(config =>
            {
                config.UseQuartzSimpleInjectorWithDecorators(_container, Assembly.GetExecutingAssembly());

                //Check container for errors
                _container.Verify();

                config.Service<SampleService>(s =>
                {
                    //Simple Repeatable Scheduled from TimeSpan
                    s.ScheduleQuartzJob(configurator =>
                        configurator.WithSimpleRepeatableSchedule<JobWithInjectedDependenciesDecorated>(TimeSpan.FromSeconds(1), nameof(JobWithInjectedDependenciesDecorated)));

                    // Let Topshelf use it
                    s.ConstructUsingSimpleInjector();
                    s.WhenStarted((service, control) => service.Start());
                    s.WhenStopped((service, control) => service.Stop());
                });
            });
        }

        public class SampleService
        {
            public bool Start()
            {
                Console.WriteLine("Sample Service Started.");
                return true;
            }

            public bool Stop()
            {
                return true;
            }
        }

        public class JobWithInjectedDependenciesDecorated : IJob
        {
            private readonly IDependencyInjected _dependencyInjected;

            public JobWithInjectedDependenciesDecorated(IDependencyInjected dependencyInjected)
            {
                _dependencyInjected = dependencyInjected;
            }

            public void Execute(IJobExecutionContext context)
            {
                _dependencyInjected.DoSomething(); //Another dependency
            }
        }

        public interface IDependencyInjected
        {
            void DoSomething();
        }

        public class DependencyInjected : IDependencyInjected
        {
            public void DoSomething()
            {
                Console.WriteLine("[" + typeof(DependencyInjected).Name + "] Triggered " + DateTime.Now.ToLongTimeString());
            }
        }

        public class LoggingDecorator : IJob
        {
            private readonly IJob _decoratee;

            public LoggingDecorator(IJob decoratee)
            {
                _decoratee = decoratee;
            }

            public void Execute(IJobExecutionContext context)
            {
                Console.WriteLine("See, i am decorating the Job: " + typeof(JobWithInjectedDependenciesDecorated).Name + " with a logger!");
                _decoratee.Execute(context);
            }
        }

        public class LifetimeScopeDecoratorFuncTFactory : IJob
        {
            private readonly Func<IJob> _decorateeFactory;
            private readonly Container _container;

            public LifetimeScopeDecoratorFuncTFactory(Func<IJob> decorateeFactory, Container container)
            {
                _decorateeFactory = decorateeFactory;
                _container = container;
            }

            public void Execute(IJobExecutionContext context)
            {
                using (ThreadScopedLifestyle.BeginScope(_container))
                {
                    Console.WriteLine("See, i am decorating the Job: " + typeof(JobWithInjectedDependenciesDecorated).Name + " with a Lifetime Scope!");
                    var job = _decorateeFactory.Invoke();
                    job.Execute(context);
                }
            }
        }
    }
}