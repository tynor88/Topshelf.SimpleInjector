using System;
using Quartz;
using Quartz.Spi;
using SimpleInjector;

namespace Topshelf.SimpleInjector.Quartz
{
    /// <summary>
    /// Factory to make it possible to inject quartz jobs with SimpleInjector
    /// </summary>
    public class SimpleInjectorJobFactory : IJobFactory
    {
        #region Private Fields

        private readonly Container _container;

        #endregion

        public SimpleInjectorJobFactory(Container container)
        {
            _container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            IJobDetail jobDetail = bundle.JobDetail;
            Type jobType = jobDetail.JobType;

            try
            {
                // Return job registrated in container
                return (IJob)_container.GetInstance(jobType);
            }
            catch (Exception ex)
            {
                throw new SchedulerException("Problem instantiating class", ex);
            }
        }

        public void ReturnJob(IJob job)
        {
        }
    }
}
