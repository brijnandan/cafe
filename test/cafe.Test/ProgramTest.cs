﻿using FluentAssertions;
using Xunit;

namespace cafe.Test
{
    public class ProgramTest
    {
        [Fact]
        public void LogConfigurationFile_ShouldBeClientForNoArguments()
        {
            AssertLogConfigurationFileIs("nlog-client.config");
        }

        [Fact]
        public void LogConfigurationFile_ShouldBeClientForNonServerArguments()
        {
            AssertLogConfigurationFileIs("nlog-client.config", "chef", "run");
        }

        [Fact]
        public void LogConfigurationFile_ShouldBeServerForServerArgument()
        {
            AssertLogConfigurationFileIs("nlog-server.config", "server");
        }
        [Fact]
        public void LogConfigurationFile_ShouldBeServerForServerRunAsServiceArguments()
        {
            AssertLogConfigurationFileIs(Program.ServerLoggingConfigurationFile, "server", "--run-as-service");
        }

        private void AssertLogConfigurationFileIs(string expectedConfigFile, params string[] args)
        {
            Program.LoggingConfigurationFileFor(args).Should().Be(expectedConfigFile);
        }
    }
}