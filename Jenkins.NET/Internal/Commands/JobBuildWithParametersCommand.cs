﻿using JenkinsNET.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace JenkinsNET.Internal.Commands
{
    internal class JobBuildWithParametersCommand : JenkinsHttpCommand
    {
        public JenkinsBuildResult Result {get; internal set;}

        public JobBuildWithParametersCommand(IJenkinsContext context, string jobName, IDictionary<string, string> jobParameters)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrEmpty(jobName))
                throw new ArgumentException("'jobName' cannot be empty!");

            if (jobParameters == null)
                throw new ArgumentNullException(nameof(jobParameters));

            //var _params = new Dictionary<string, string>(jobParameters) {
            //    ["delay"] = "0sec",
            //};

            //var query = new StringWriter();
            //WriteJobParameters(query, _params);

            Url = NetPath.Combine(context.BaseUrl, "job", jobName, $"buildWithParameters");
            UserName = context.UserName;
            Password = context.Password;
            Crumb = context.Crumb;
			var byteArray = Encoding.UTF8.GetBytes(HttpUtility.UrlEncode(JsonConvert.SerializeObject(jobParameters)) ?? throw new InvalidOperationException());

			OnWrite = request => {
                request.Method = "POST";
				request.ContentLength = byteArray.Length;
				request.ContentType = "application/x-www-form-urlencoded";
				var dataStream = request.GetRequestStream();
				dataStream.Write(byteArray, 0, byteArray.Length);
				dataStream.Close();
			};

            OnRead = response => {
                if (response.StatusCode != System.Net.HttpStatusCode.Created)
                    throw new JenkinsJobBuildException($"Expected HTTP status code 201 but found {(int)response.StatusCode}!");

                Result = new JenkinsBuildResult {
                    QueueItemUrl = response.GetResponseHeader("Location"),
                };
            };
        }

        private void WriteJobParameters(TextWriter writer, IDictionary<string, string> jobParameters)
        {
            var isFirst = true;
            foreach (var pair in jobParameters) {
                if (isFirst) {
                    isFirst = false;
                }
                else {
                    writer.Write('&');
                }

                var encodedName = HttpUtility.UrlEncode(pair.Key);
                var encodedValue = HttpUtility.UrlEncode(pair.Value);

                writer.Write(encodedName);
                writer.Write('=');
                writer.Write(encodedValue);
            }
        }
    }
}
