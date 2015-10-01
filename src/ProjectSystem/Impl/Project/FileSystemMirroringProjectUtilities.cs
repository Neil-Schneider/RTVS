﻿using System.IO;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project
{
    public static class FileSystemMirroringProjectUtilities
    {
        public static string GetProjectDirectory(this UnconfiguredProject unconfiguredProject)
        {
            return PathHelper.EnsureTrailingSlash(Path.GetDirectoryName(unconfiguredProject.FullPath));
        }

        public static string GetInMemoryTargetsFileFullPath(this UnconfiguredProject unconfiguredProject)
        {
            var projectPath = unconfiguredProject.FullPath;
            return Path.Combine(Path.GetDirectoryName(projectPath), GetInMemoryTargetsFileName(projectPath));
        }

        public static string GetInMemoryTargetsFileName(string cpsProjFileName)
        {
            return Path.GetFileNameWithoutExtension(cpsProjFileName) + ".InMemory.Targets";
        }
    }
}
