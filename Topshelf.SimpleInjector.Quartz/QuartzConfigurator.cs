using System;
using System.Collections.Generic;
using Quartz;

namespace Topshelf.SimpleInjector.Quartz
{
    public class QuartzConfigurator
    {
        public Func<IJobDetail> Job { get; set; }
        public IList<Func<ITrigger>> Triggers { get; set; }
        public Func<bool> JobEnabled { get; set; }

        public QuartzConfigurator()
        {
            Triggers = new List<Func<ITrigger>>();
        }

        #region Quartz Builder Framework

        public QuartzConfigurator WithJob(Func<IJobDetail> jobDetail)
        {
            Job = jobDetail;
            return this;
        }

        public QuartzConfigurator AddTrigger(Func<ITrigger> jobTrigger)
        {
            Triggers.Add(jobTrigger);
            return this;
        }

        public QuartzConfigurator EnableJobWhen(Func<bool> jobEnabled)
        {
            JobEnabled = jobEnabled;
            return this;
        }

        #endregion

        #region Simple Configuration Extensions

        public QuartzConfigurator WithCronSchedule<TJob>(string cronSchedule, string jobIdentity) where TJob : IJob
        {
            if (string.IsNullOrWhiteSpace(jobIdentity))
            {
                WithCronSchedule<TJob>(cronSchedule);
            }
            if (!string.IsNullOrWhiteSpace(cronSchedule))
            {
                Func<IJobDetail> jobDetail = () => JobBuilder
                    .Create<TJob>()
                    .WithIdentity(jobIdentity)
                    .Build();
                WithJob(jobDetail);

                Func<ITrigger> trigger = () => TriggerBuilder
                    .Create()
                    .WithCronSchedule(cronSchedule)
                    .Build();
                AddTrigger(trigger);

                return this;
            }

            throw new ArgumentException("must specify a valid cron schedule expression", "cronSchedule");
        }

        public QuartzConfigurator WithCronSchedule<TJob>(string cronSchedule) where TJob : IJob
        {
            return WithCronSchedule<TJob>(cronSchedule, typeof(TJob).ToString());
        }

        #endregion
    }
}
