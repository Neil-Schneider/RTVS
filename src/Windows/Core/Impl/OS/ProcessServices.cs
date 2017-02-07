﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Common.Core.OS;

namespace Microsoft.Windows.Core.OS {
    public sealed class ProcessServices : IProcessServices {
        public Process Start(ProcessStartInfo psi) => Process.Start(psi);

        public Process Start(string path) => Process.Start(path);

        public string MessageFromExitCode(int processExitCode) => ErrorCodeConverter.MessageFromErrorCode(processExitCode);
    }
}
