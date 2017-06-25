﻿using JenkinsNET.Internal;
using System;
using System.IO;

namespace JenkinsNET.Commands
{
    internal class ArtifactGetCommand : JenkinsHttpCommand
    {
        public MemoryStream Result {get; private set;}


        public ArtifactGetCommand(IJenkinsContext context, string jobName, string buildNumber, string filename)
        {
            if (string.IsNullOrEmpty(jobName))
                throw new ArgumentException("'jobName' cannot be empty!");

            if (string.IsNullOrEmpty(buildNumber))
                throw new ArgumentException("'buildNumber' cannot be empty!");

            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("'filename' cannot be empty!");

            var urlFilename = filename.Replace('\\', '/');
            Url = NetPath.Combine(context.BaseUrl, "job", jobName, buildNumber, "artifact", urlFilename);
            UserName = context.UserName;
            Password = context.Password;

            OnWrite = request => {
                request.Method = "POST";
            };

            OnRead = response => {
                using (var stream = response.GetResponseStream()) {
                    if (stream == null) return;

                    Result = new MemoryStream();
                    stream.CopyTo(Result);
                }
            };
        }
    }
}