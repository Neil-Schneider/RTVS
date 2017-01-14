﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;
using static Microsoft.Common.Core.NativeMethods;

namespace Microsoft.Common.Core.OS {
    public class Win32Process {
        public static int StartProcessAsUser(WindowsIdentity winIdentity, string applicationName, string commandLine, string workingDirectory, Win32NativeEnvironmentBlock environment, out Stream stdin, out Stream stdout, out Stream stderror) {

            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(typeof(STARTUPINFO));

            /* 
            When a process is started using CreateProcessAsUser function, the process will be started into a windowstation 
            and desktop combination based on the value of lpDesktop in the STARTUPINFO structure parameter:
            lpDesktop = "<windowsta>\<desktop>"; the system will try to start the process into that windowstation and desktop.
            lpDesktop = NULL; the system will try to use the same windowstation and desktop as the calling process if the system is associated with the interactive windowstation.
            lpDesktop = <somevalue>; the system will create a new windowstation and desktop that you cannot see.
            lpDesktop = ""; it will either create a new windowstation and desktop that you cannot see, or if one has been created by means of a prior call by using the same access token, the existing windowstation and desktop will be used.
            */
            si.lpDesktop = "";

            IntPtr stdinRead, stdinWrite, stdoutRead, stdoutWrite, stderrorRead, stderrorWrite;

            SECURITY_ATTRIBUTES sa = default(SECURITY_ATTRIBUTES);
            sa.nLength = Marshal.SizeOf(sa);
            sa.lpSecurityDescriptor = IntPtr.Zero;
            sa.bInheritHandle = true;

            if (!CreatePipe(out stdinRead, out stdinWrite, ref sa, 0)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!SetHandleInformation(stdinWrite, HANDLE_FLAGS.INHERIT, HANDLE_FLAGS.None)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!CreatePipe(out stdoutRead, out stdoutWrite, ref sa, 0)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!SetHandleInformation(stdoutRead, HANDLE_FLAGS.INHERIT, HANDLE_FLAGS.None)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!CreatePipe(out stderrorRead, out stderrorWrite, ref sa, 0)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!SetHandleInformation(stderrorRead, HANDLE_FLAGS.INHERIT, HANDLE_FLAGS.None)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            si.dwFlags = STARTF_USESTDHANDLES;
            si.hStdInput = stdinRead;
            si.hStdOutput = stdoutWrite;
            si.hStdError = stderrorWrite;

            SECURITY_ATTRIBUTES processAttr = CreateSecurityAttributes();
            SECURITY_ATTRIBUTES threadAttr = CreateSecurityAttributes();

            PROCESS_INFORMATION pi;
            if (!CreateProcessAsUser(
                winIdentity.Token, applicationName, commandLine, ref processAttr, ref threadAttr, true,
                (uint)(CREATE_PROCESS_FLAGS.CREATE_UNICODE_ENVIRONMENT | CREATE_PROCESS_FLAGS.CREATE_NO_WINDOW),
                environment.NativeEnvironmentBlock,
                workingDirectory,
                ref si,
                out pi)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            stdin = new FileStream(new SafeFileHandle(stdinWrite, true), FileAccess.Write, 0x1000, false);
            stdout = new FileStream(new SafeFileHandle(stdoutRead, true), FileAccess.Read, 0x1000, false);
            stderror = new FileStream(new SafeFileHandle(stderrorRead, true), FileAccess.Read, 0x1000, false);

            // TODO: handle cleanup for process and thread
            // TODO: cleanup security attributes
            return pi.dwProcessId;
        }

        private static SECURITY_ATTRIBUTES CreateSecurityAttributes() {
            // Grant access to Network Service.
            SecurityIdentifier networkService = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);
            DiscretionaryAcl dacl = new DiscretionaryAcl(false, false, 1);
            dacl.AddAccess(AccessControlType.Allow, networkService, -1, InheritanceFlags.None, PropagationFlags.None);
            CommonSecurityDescriptor csd = new CommonSecurityDescriptor(false, false, ControlFlags.DiscretionaryAclPresent | ControlFlags.OwnerDefaulted | ControlFlags.GroupDefaulted, null, null, null, dacl);

            byte[] buffer = new byte[csd.BinaryLength];
            csd.GetBinaryForm(buffer, 0);

            IntPtr dest = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, dest, buffer.Length);

            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.nLength = Marshal.SizeOf(sa);
            sa.lpSecurityDescriptor = dest;

            return sa;
        }

        const int MAX_PATH = 260;
    }
}
