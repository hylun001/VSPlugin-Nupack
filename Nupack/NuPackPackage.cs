﻿using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CnSharp.VisualStudio.Extensions;
using CnSharp.VisualStudio.Extensions.Commands;
using CnSharp.VisualStudio.NuPack.Commands;
using CnSharp.VisualStudio.NuPack.Extensions;
using CnSharp.VisualStudio.NuPack.Util;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace CnSharp.VisualStudio.NuPack
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#1110", "#1112", "1.0", IconResourceID = 1400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class NuPackPackage : AsyncPackage
    {
        /// <summary>
        /// NuPackPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "cfa941e7-1101-459a-9777-496681f602d0";



        /// <summary>
        /// Initializes a new instance of the <see cref="NuPackPackage"/> class.
        /// </summary>
        public NuPackPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
            RedirectAssembly("VsSharp", new Version("1.3.2.1"), "31e1bdd79b8e5ae1");
            RedirectAssembly("Microsoft.VisualStudio.Threading", new Version("15.6.0.0"), "b03f5f7f11d50a3a");
        }

        #region Package Members

        #region Overrides of AsyncPackage

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var dte = GetGlobalService(typeof(DTE)) as DTE2;
            Host.Instance.DTE = dte;

            bool isSolutionLoaded = await IsSolutionLoadedAsync();

            if (isSolutionLoaded)
            {
                HandleOpenSolution();
            }

            // Listen for subsequent solution events
            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnAfterOpenSolution += HandleOpenSolution;


            dte.Events.SolutionEvents.ProjectAdded += p =>
            {
                if (string.IsNullOrWhiteSpace(p.FileName) ||
                    !Common.SupportedProjectTypes.Any(t => p.FileName.EndsWith(t, StringComparison.OrdinalIgnoreCase)))
                    return;
                var sln = Host.Instance.Solution2;
                SolutionDataCache.Instance.TryGetValue(sln.FileName, out var sp);
                sp?.AddProject(p);
            };
            dte.Events.SolutionEvents.ProjectRemoved += p =>
            {
                var sln = Host.Instance.Solution2;
                SolutionDataCache.Instance.TryGetValue(sln.FileName, out var sp);
                sp?.RemoveProject(p);
            };

            //var commandService =  await this.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

            AddNuSpecCommand.Initialize(this);
            NuGetDeployCommand.Initialize(this);
            //AssemblyInfoEditCommand.Initialize(this);
            AddDirectoryBuildPropsCommand.Initialize(this);
        }

        #endregion

        private async Task<bool> IsSolutionLoadedAsync()
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            var solService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;

            ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));

            return value is bool isSolOpen && isSolOpen;
        }

        private void HandleOpenSolution(object sender = null, EventArgs e = null)
        {
            var sln = Host.Instance.Solution2;
            var projects = Host.Instance.DTE.GetSolutionProjects()
                    .Where(
                        p =>
                            !string.IsNullOrWhiteSpace(p.FileName) &&
                            Common.SupportedProjectTypes.Any(
                                t => p.FileName.EndsWith(t, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            var sp = new SolutionProperties
            {
                Projects = projects
            };
            SolutionDataCache.Instance.AddOrUpdate(sln.FileName, sp, (k, v) =>
            {
                v = sp;
                return v;
            });
        }


        public static void RedirectAssembly(string shortName, Version targetVersion, string publicKeyToken)
        {
            ResolveEventHandler handler = null;

            handler = (sender, args) =>
            {
                // Use latest strong name & version when trying to load SDK assemblies
                var requestedAssembly = new AssemblyName(args.Name);
                if (requestedAssembly.Name != shortName)
                    return null;

                Debug.WriteLine("Redirecting assembly load of " + args.Name
                              + ",\tloaded by " + (args.RequestingAssembly == null ? "(unknown)" : args.RequestingAssembly.FullName));

                requestedAssembly.Version = targetVersion;
                requestedAssembly.SetPublicKeyToken(new AssemblyName(shortName + ", PublicKeyToken=" + publicKeyToken).GetPublicKeyToken());
                requestedAssembly.CultureInfo = CultureInfo.InvariantCulture;

                AppDomain.CurrentDomain.AssemblyResolve -= handler;

                return Assembly.Load(requestedAssembly);
            };
            AppDomain.CurrentDomain.AssemblyResolve += handler;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogUtils.WriteLog((e.ExceptionObject as Exception).ToString());
        }

        #region obsolete
        //private CommandConfig _config;
        //private static Guid Id = new Guid("{738D452D-862C-4BB5-A035-CA8E07403138}");

        //private  void LoadCustomCommands()
        //{
        //    if (_config != null || Host.Plugins.Any(m => m.Id == Id))
        //        return;
        //    _config = new CommandConfig
        //    {
        //        Menus =
        //        {
        //           new AssemblyInfoMenu(),
        //           new AddDirectoryBuildPropsMenu()
        //        }
        //    };
        //    var plugin = new Plugin
        //    {
        //        Id = Id,
        //        CommandConfig = _config,
        //        Assembly = Assembly.GetExecutingAssembly(),
        //        Location =
        //            Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", ""),
        //        //ResourceManager = new ResourceManager(config.ResourceManager, Assembly.GetExecutingAssembly())
        //    };


        //    var manager = new CommandManager(plugin);
        //    manager.Load();
        //}
        #endregion
        #endregion
    }
}
