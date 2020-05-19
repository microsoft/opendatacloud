using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Msr.Odr.Services.Batch;
using FluentValidation;

namespace Msr.Odr.Services.Configuration
{
    public class BatchConfiguration
    {
        public string Key { get; set; }
        public string Account { get; set; }
        public string Url { get; set; }
        public string StorageName { get; set; }
        public string StorageKey { get; set; }

        public string JobId => GetJobId();
        public string TaskId => GetTaskId();
        public string TaskPath => GetTaskPath();

#if DEBUG

        private readonly string _jobId = BatchConstants.DatasetJobId;
        private readonly string _taskId = $"local-test-{Guid.NewGuid():N}";
        private readonly string _taskPath = System.IO.Directory.GetCurrentDirectory();

        private string GetJobId()
        {
            return _jobId;
        }

        private string GetTaskId()
        {
            return _taskId;
        }

        private string GetTaskPath()
        {
            return _taskPath;
        }

#else

        private string GetJobId()
        {
            return VerifyEnvironmentValue("AZ_BATCH_JOB_ID");
        }

        private string GetTaskId()
        {
            return VerifyEnvironmentValue("AZ_BATCH_TASK_ID");
        }

        private string GetTaskPath()
        {
            return VerifyEnvironmentValue("AZ_BATCH_TASK_DIR");
        }

        private string VerifyEnvironmentValue(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var value = Environment.GetEnvironmentVariable(name);

            // TODO: configuration access the properties, so this exception is thrown in the web app.
            //if (string.IsNullOrWhiteSpace(value))
            //{
            //    throw new EnvironmentValueNotFoundException(name);
            //}

            return value;
        }

#endif

        public static void Validate(BatchConfiguration configuration)
        {
            var validator = new BatchConfigurationValidator();
            validator.ValidateAndThrow(configuration);
        }
    }
}
