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
    }
}
