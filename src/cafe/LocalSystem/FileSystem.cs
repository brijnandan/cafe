﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace cafe.LocalSystem
{
    public class FileSystem : IFileSystem
    {
        private readonly IEnvironment _environment;
        private readonly IFileSystemCommands _commands;

        private static readonly Logger Logger = LogManager.GetLogger(typeof(FileSystem).FullName);

        public FileSystem(IEnvironment environment, IFileSystemCommands commands)
        {
            _environment = environment;
            _commands = commands;
        }

        public void EnsureDirectoryExists(string directory)
        {
            if (!_commands.DirectoryExists(directory))
            {
                Logger.Info($"Creating staging directory {directory}");
                _commands.CreateDirectory(directory);
            }
            else
            {
                Logger.Debug($"Directory {directory} already exists, so it does not need to be created");
            }
        }


        public string FindInstallationDirectoryInPathContaining(string executable, string defaultPath)
        {
            var paths = new List<string>() { defaultPath };
            var environmentPath = _environment.GetEnvironmentVariable("PATH");
            paths.AddRange(environmentPath.Split(';'));
            var batchFilePath = paths
                .Select(x => Path.Combine(x, executable))
                .FirstOrDefault(_commands.FileExists);
            if (batchFilePath == null)
            {
                Logger.Warn($"Could not find {executable} in the path {environmentPath}");
                return null;
            }
            var binDirectory = Directory.GetParent(batchFilePath);
            return binDirectory.FullName;
        }

        public bool FileExists(string destination)
        {
            return _commands.FileExists(destination);
        }
    }
}