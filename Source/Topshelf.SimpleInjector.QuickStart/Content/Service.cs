namespace Topshelf.SimpleInjector.QuickStart.Content
{
    public class Service
    {
        private readonly IDependency _dependency;

        public Service(IDependency dependency)
        {
            _dependency = dependency;
        }

        public bool Start()
        {
            //TODO do something here when service has started

            return _dependency != null;
        }

        public bool Stop()
        {
            return _dependency != null;
        }
    }
}