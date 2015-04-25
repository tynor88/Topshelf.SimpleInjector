using System;
using System.Globalization;
using Quartz;
using Quartz.Spi;
using SimpleInjector;

namespace Topshelf.SimpleInjector.Quartz.Factory
{
    /// <summary>
    /// Factory to make it possible to inject dependencies to Quartz jobs (IJob implementations) with SimpleInjector
    /// </summary>
    public class SimpleInjectorJobFactory : IJobFactory
    {
        #region Private Fields

        private readonly Container _container;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiate a new SimpleInjectorJobFactory which is able to produce IJob implementations
        /// </summary>
        /// <param name="container">The SimpleInjector Container to resolve IJob implementations from</param>
        public SimpleInjectorJobFactory(Container container)
        {
            _container = container;
        }

        #endregion

        #region IJobFactory Members

        /// <summary>
        /// Called by the scheduler at the time of the trigger firing, in order to produce a Quartz.IJob instance on which to call Execute.
        /// </summary>
        /// <param name="bundle">The TriggerFiredBundle from which the Quartz.IJobDetail and other info relating to the trigger firing can be obtained.</param>
        /// <param name="scheduler">a handle to the scheduler that is about to execute the job</param>
        /// <returns>the newly instantiated Job</returns>
        /// <remarks>It should be extremely rare for this method to throw an exception - 
        /// basically only the the case where there is no way at all to instantiate and prepare the Job for execution. 
        /// When the exception is thrown, the Scheduler will move all triggers associated with the Job into the Quartz.TriggerState.Error state, 
        /// which will require human intervention (e.g. an application restart after fixing whatever configuration problem led to the issue 
        /// with instantiating the Job.</remarks>
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            IJobDetail jobDetail = bundle.JobDetail;
            Type jobType = jobDetail.JobType;

            try
            {
                // Return job registered in container
                return (IJob)_container.GetInstance(jobType);
            }
            catch (Exception ex)
            {
                throw new SchedulerConfigException(string.Format(CultureInfo.InvariantCulture,
                    "Failed to instantiate Job '{0}' of type '{1}'",
                    bundle.JobDetail.Key, bundle.JobDetail.JobType), ex);
            }
        }

        /// <summary>
        /// Allows the the job factory to destroy/cleanup the job if needed.
        /// </summary>
        /// <param name="job">The job to cleanup</param>
        public void ReturnJob(IJob job)
        {
        }

        #endregion
    }
}