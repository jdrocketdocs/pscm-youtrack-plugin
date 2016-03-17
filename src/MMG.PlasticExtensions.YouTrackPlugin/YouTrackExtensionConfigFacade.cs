﻿// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackExtensionConfigFacade.cs
// Last Modified: 03/17/2016 11:02 AM
// Modified By: Green, Brett (greenb1)
// *************************************************

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    using System;
    using System.Collections.Generic;
    using Codice.Client.IssueTracker;
    using Codice.Utils;
    using log4net;

    public class YouTrackExtensionConfigFacade : IYouTrackExtensionConfigFacade
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");
        private readonly IssueTrackerConfiguration _config;
        private readonly Uri _hostUri;


        internal YouTrackExtensionConfigFacade()
        {
            BranchPrefix = "yt_";
            _hostUri = new Uri("http://issues.domain.com");
            UserID = "";
            Password = "";
            ShowIssueStateInBranchTitle = false;
            IgnoreIssueStateForBranchTitle = "Completed";
            UsernameMapping = "";

            IsDefaultInit = true;
            _log.Debug("YouTrackExtensionConfigFacade: empty ctor called");
        }

        public YouTrackExtensionConfigFacade(IssueTrackerConfiguration pConfig)
        {
            _config = pConfig;

            BranchPrefix = getValidParameterValue(ConfigParameterNames.BranchPrefix);
            var hostValue = getValidParameterValue(ConfigParameterNames.Host);
            if (!Uri.TryCreate(hostValue, UriKind.Absolute, out _hostUri))
                throw new ApplicationException(string.Format("Unable to parse host URL '{0}'.", hostValue));

            UserID = getValidParameterValue(ConfigParameterNames.UserID);
            Password = getValidParameterValue(ConfigParameterNames.Password);
            ShowIssueStateInBranchTitle = bool.Parse(getValidParameterValue(ConfigParameterNames.ShowIssueStateInBranchTitle, "false"));
            IgnoreIssueStateForBranchTitle = getValidParameterValue(ConfigParameterNames.ClosedIssueStates, "Completed");
            UsernameMapping = getValidParameterValue(ConfigParameterNames.UsernameMapping);

            IsDefaultInit = false;
            _log.Debug("YouTrackExtensionConfigFacade: ctor called");
        }

        public string BranchPrefix { get; private set; }

        public Uri Host
        {
            get { return _hostUri; }
        }

        public string UsernameMapping { get; private set; }

        public string UserID { get; private set; }

        public string Password { get; private set; }

        public bool UseSSL
        {
            get { return _hostUri.Scheme == "https"; }
        }

        public bool ShowIssueStateInBranchTitle { get; private set; }

        internal bool IsDefaultInit { get; private set; }

        /// <summary>
        /// Issue state(s) to not display in branch title when ShowIssueStateInBranchTitle = true.
        /// </summary>
        /// <remarks>Use commas to separate multiple states.</remarks>
        public string IgnoreIssueStateForBranchTitle { get; private set; }

        public ExtensionWorkingMode WorkingMode
        {
            get
            {
                if (_config == null)
                    return ExtensionWorkingMode.TaskOnBranch;

                return _config.WorkingMode == ExtensionWorkingMode.None
                    ? ExtensionWorkingMode.TaskOnBranch
                    : _config.WorkingMode;
            }
        }

        public List<IssueTrackerConfigurationParameter> GetYouTrackParameters()
        {
            var parameters = new List<IssueTrackerConfigurationParameter>();

            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.BranchPrefix,
                    Value = BranchPrefix,
                    Type = IssueTrackerConfigurationParameterType.BranchPrefix,
                    IsGlobal = true
                });
            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.Host,
                    Value = Host.ToString(),
                    Type = IssueTrackerConfigurationParameterType.Host,
                    IsGlobal = true
                });
            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.UserID,
                    Value = UserID,
                    Type = IssueTrackerConfigurationParameterType.User,
                    IsGlobal = false
                });
            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.UsernameMapping,
                    Value = UsernameMapping,
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = true
                });
            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.Password,
                    Value = Password,
                    Type = IssueTrackerConfigurationParameterType.Password,
                    IsGlobal = false
                });
            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.ShowIssueStateInBranchTitle,
                    Value = ShowIssueStateInBranchTitle.ToString(),
                    Type = IssueTrackerConfigurationParameterType.Boolean,
                    IsGlobal = false
                });
            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.ClosedIssueStates,
                    Value = IgnoreIssueStateForBranchTitle,
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = false
                });

            return parameters;
        }

        public string GetDecryptedPassword()
        {
            if (_config == null)
                throw new ApplicationException("The configuration has not yet been initialized!");

            if (string.IsNullOrEmpty(Password))
                throw new ApplicationException("Password value can not be empty!");

            var decryptedPassword = CryptoServices.GetDecryptedPassword(Password);
            return decryptedPassword;
        }

        private string getValidParameterValue(string pParamName, string pDefaultValue = "")
        {
            if (_config == null)
                throw new ApplicationException("The configuration has not yet been initialized!");

            var configValue = _config.GetValue(pParamName);

            return string.IsNullOrEmpty(configValue)
                ? pDefaultValue
                : configValue;
        }
    }
}