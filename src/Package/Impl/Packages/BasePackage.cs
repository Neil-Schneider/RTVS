﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Shell;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Utilities;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Packages
{
    public abstract class BasePackage<TLanguageService> : VisualStudio.Shell.Package
        where TLanguageService : class, new()
    {
        private Dictionary<IVsProjectGenerator, uint> _projectFileGenerators;

        protected abstract IEnumerable<IVsEditorFactory> CreateEditorFactories();
        protected abstract IEnumerable<IVsProjectGenerator> CreateProjectFileGenerators();
        protected abstract IEnumerable<IVsProjectFactory> CreateProjectFactories();

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that relies on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            AppShell.AddRef();

            IServiceContainer container = this;
            container.AddService(typeof(TLanguageService), new TLanguageService(), true);

            foreach (var projectFactory in CreateProjectFactories())
            {
                RegisterProjectFactory(projectFactory);
            }

            foreach (var projectFileGenerator in CreateProjectFileGenerators())
            {
                RegisterProjectFileGenerator(projectFileGenerator);
            }

            foreach (var editorFactory in CreateEditorFactories())
            {
                RegisterEditorFactory(editorFactory);
            }
        }

        private void RegisterProjectFileGenerator(IVsProjectGenerator projectFileGenerator)
        {
            var registerProjectGenerators = GetService(typeof(SVsRegisterProjectTypes)) as IVsRegisterProjectGenerators;
            if (registerProjectGenerators == null)
            {
                throw new InvalidOperationException(typeof(SVsRegisterProjectTypes).FullName);
            }

            uint cookie;
            Guid riid = projectFileGenerator.GetType().GUID;
            registerProjectGenerators.RegisterProjectGenerator(ref riid, projectFileGenerator, out cookie);

            if (_projectFileGenerators == null)
            {
                _projectFileGenerators = new Dictionary<IVsProjectGenerator, uint>();
            }

            _projectFileGenerators[projectFileGenerator] = cookie;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_projectFileGenerators != null)
            {
                var projectFileGenerators = _projectFileGenerators;
                _projectFileGenerators = null;
                UnregisterProjectFileGenerators(projectFileGenerators);
            }

            IServiceContainer container = this;
            container.RemoveService(typeof(TLanguageService));

            AppShell.Release();

            base.Dispose(disposing);
        }

        private void UnregisterProjectFileGenerators(Dictionary<IVsProjectGenerator, uint> projectFileGenerators)
        {
            try
            {
                IVsRegisterProjectGenerators registerProjectGenerators = GetService(typeof (SVsRegisterProjectTypes)) as IVsRegisterProjectGenerators;
                if (registerProjectGenerators != null)
                {
                    foreach (var projectFileGenerator in projectFileGenerators)
                    {
                        try
                        {
                            registerProjectGenerators.UnregisterProjectGenerator(projectFileGenerator.Value);
                        }
                        finally
                        {
                            (projectFileGenerator.Key as IDisposable)?.Dispose();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Fail(Invariant($"Failed to dispose project file generator for package {GetType().FullName}\n{e.Message}"));
            }
        }
    }
}