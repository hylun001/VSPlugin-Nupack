using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AeroWizard;
using CnSharp.VisualStudio.Extensions;
using CnSharp.VisualStudio.Extensions.Projects;
using CnSharp.VisualStudio.NuPack.Properties;
using CnSharp.VisualStudio.NuPack.Util;
using EnvDTE;
using EnvDTE80;
using NuGet;
using Process = System.Diagnostics.Process;

namespace CnSharp.VisualStudio.NuPack.Packaging
{
    /// <summary>
    /// .net framework打包界面
    /// </summary>
    public partial class DeployWizard : Form
    {

        private const string NugetPackageSource = "nuget.config";
        private readonly ProjectAssemblyInfo _assemblyInfo;
        private readonly PackageProjectProperties _ppp;
        private readonly string _dir;
        private readonly NuGetConfig _nuGetConfig;
        //private readonly Package _package;
        private readonly ManifestMetadata _metadata;
        private readonly string _packageOldVersion;
        private readonly Project _project;
        private readonly ProjectNuPackConfig _projectConfig;
        private readonly string _releaseDir;
        private string _outputDir;
        private PackageMetadataControl _metadataControl;
        private NuGetDeployControl _deployControl;

        public DeployWizard()
        {
            InitializeComponent();

            _metadataControl = new PackageMetadataControl();
            _metadataControl.Dock = DockStyle.Fill;
            _metadataControl.ErrorProvider = errorProvider;
            panelPackageInfo.Controls.Add(_metadataControl);
            ActiveControl = _metadataControl;

            _deployControl = new NuGetDeployControl();
            _deployControl.Dock = DockStyle.Fill;
            wizardPageDeploy.Controls.Add(_deployControl);

            _project = Host.Instance.Dte2.GetActiveProejct();
            _dir = _project.GetDirectory();
            _releaseDir = Path.Combine(_dir, "bin", "Release");


            _nuGetConfig = ConfigHelper.ReadNuGetConfig();

            string nugetFilePath = $@"{ConfigHelper.AppDataDir}\nuget.exe";
            //初始化时，没有配置插件nuget.exe配置,保存nuget.exe到本地磁盘
            if (string.IsNullOrEmpty(_nuGetConfig.NugetPath) || !File.Exists(nugetFilePath))
            {
                _nuGetConfig.NugetPath = SaveNugetToLocal(nugetFilePath);
            }

            //更新本地nuget.config
            var nugetSource = ReadNewestNugetResource();
            if (nugetSource != null)
            {
                //初始化时，没有配置插件nuget配置，则读取nuget.config配置；如果有配置，更新本地
                if (_nuGetConfig.Sources == null || _nuGetConfig.Sources.Count <= 0)
                {
                    _nuGetConfig.Sources = _nuGetConfig.Sources ?? new System.Collections.Generic.List<NuGetSource>();
                    _nuGetConfig.Sources.Add(nugetSource);
                }
                else
                {
                    var configSource = _nuGetConfig.Sources.Find(item =>
                                                                 item.Url.Equals(nugetSource.Url, StringComparison.OrdinalIgnoreCase) &&
                                                                 item.UserName.Equals(nugetSource.UserName, StringComparison.OrdinalIgnoreCase));
                    if (configSource != null)
                    {
                        configSource.ApiKey = nugetSource.ApiKey;
                    }
                    else
                    {
                        _nuGetConfig.Sources.Insert(0, nugetSource);
                    }
                }
            }


            _projectConfig = _project.ReadNuPackConfig();

            BindTextBoxEvents();

            stepWizardControl.SelectedPageChanged += StepWizardControl_SelectedPageChanged;
            stepWizardControl.Finished += StepWizardControl_Finished;
            wizardPageMetadata.Commit += WizardPageCommit;
            wizardPageOptions.Commit += WizardPageCommit;
            chkSymbol.CheckedChanged += (sender, e) =>
            {
                if (_deployControl.ViewModel != null && string.IsNullOrWhiteSpace(_deployControl.ViewModel.SymbolServer))
                    _deployControl.ViewModel.SymbolServer = Common.SymbolServer;
            };
        }

        public DeployWizard(ManifestMetadata metadata, ProjectAssemblyInfo assemblyInfo, PackageProjectProperties ppp) : this()
        {
            _metadata = metadata;
            _assemblyInfo = assemblyInfo;
            _ppp = ppp;
            _packageOldVersion = _metadata.Version;
        }

        private void WizardPageCommit(object sender, WizardPageConfirmEventArgs e)
        {
            var wp = sender as WizardPage;
            if (Validation.HasValidationErrors(wp.Controls))
            {
                e.Cancel = true;
            }
        }

        private void StepWizardControl_SelectedPageChanged(object sender, EventArgs e)
        {
            if (stepWizardControl.SelectedPage == wizardPageMetadata)
            {
                _metadataControl.Focus();
            }
            else if (stepWizardControl.SelectedPage == wizardPageOptions)
            {
                txtNugetPath.Focus();
            }
            else if (stepWizardControl.SelectedPage == wizardPageDeploy)
            {
                _deployControl.Focus();
                if (_deployControl.NuGetConfig == null)
                {
                    try
                    {
                        _deployControl.NuGetConfig = _nuGetConfig;
                        var defaultCheckedConfig = _nuGetConfig.Sources?.FirstOrDefault(item => item.Checked);
                        var deployVM = new NuGetDeployViewModel
                        {
                            SymbolServer = chkSymbol.Checked ? Common.SymbolServer : null,
                            NuGetServer = defaultCheckedConfig?.Url,
                            ApiKey = defaultCheckedConfig?.ApiKey,
                            V2Login = defaultCheckedConfig?.UserName
                        };
                        _deployControl.ViewModel = deployVM;
                    }
                    catch (System.Exception ex)
                    {
                        LogUtils.WriteLog(ex.ToString());
                    }
                }
            }
        }

        private void StepWizardControl_Finished(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }



        private void BindTextBoxEvents()
        {
            MakeTextBoxRequired(txtNugetPath);
            MakeTextBoxRequired(txtOutputDir);
            txtNugetPath.Validating += TxtNugetPath_Validating;
            txtOutputDir.Validating += TxtOutputDir_Validating;
        }

        private void MakeTextBoxRequired(TextBox box)
        {
            box.Validating += TextBoxValidating;
            box.Validated += TextBoxValidated;
        }

        private void TxtOutputDir_Validating(object sender, CancelEventArgs e)
        {
            var box = sender as TextBox;
            if (box == null)
                return;
            if (box.Text.Contains(":") && !Directory.Exists(box.Text.Trim()))
            {
                errorProvider.SetError(box, "Directory not found.");
                e.Cancel = true;
            }
        }

        private void TxtNugetPath_Validating(object sender, CancelEventArgs e)
        {
            var box = sender as TextBox;
            if (box == null)
                return;
            if (string.IsNullOrWhiteSpace(box.Text))
            {
                errorProvider.SetError(box, "*");
                e.Cancel = true;
                return;
            }
            if (!File.Exists(box.Text.Trim()))
            {
                errorProvider.SetError(box, "File not found.");
                e.Cancel = true;
            }
        }

        private void TextBoxValidated(object sender, EventArgs e)
        {
            var box = sender as TextBox;
            if (box == null)
                return;
            errorProvider.SetError(box, null);
        }

        private void TextBoxValidating(object sender, CancelEventArgs e)
        {
            var box = sender as TextBox;
            if (box == null)
                return;
            if (string.IsNullOrWhiteSpace(box.Text))
            {
                errorProvider.SetError(box, "*");
                e.Cancel = true;
            }
        }

        private void TextBoxTextChanged(object sender, EventArgs e)
        {
            var box = sender as TextBox;
            if (box != null && box.BackColor == SystemColors.Info)
                box.BackColor = SystemColors.Window;
        }

        private void DeployWizard_Load(object sender, EventArgs e)
        {
            SetBoxes();
        }


        private void SetBoxes()
        {
            var ver = _assemblyInfo?.Version ?? _ppp.AssemblyVersion;
            if (_metadata.Version.IsEmptyOrPlaceHolder())
                _metadata.Version = ver;
            if (_metadata.Title.IsEmptyOrPlaceHolder())
                _metadata.Title = _metadata.Id;
            _metadataControl.ManifestMetadata = _metadata;
            _metadataControl.AssemblyInfo = _assemblyInfo;


            txtNugetPath.Text = _nuGetConfig.NugetPath;
            txtOutputDir.Text = _projectConfig.PackageOutputDirectory;

        }

        private void btnOpenNuGetExe_Click(object sender, EventArgs e)
        {
            if (openNugetExeDialog.ShowDialog() == DialogResult.OK)
            {
                txtNugetPath.Text = openNugetExeDialog.FileName;
            }
        }

        private void btnOpenOutputDir_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                txtOutputDir.Text = folderBrowserDialog.SelectedPath;
        }

        public void SaveAndBuild()
        {
            try
            {
                if (_assemblyInfo != null)
                {
                    SaveAssemblyInfo();
                }
                else
                {
                    SavePackageInfo();
                }

                Tuple<bool, SolutionBuild2> tuple = Build();
                if (!tuple.Item1)
                {
                    //编译完Release后,将项目还原成Debug目标平台
                    tuple.Item2.SolutionConfigurations.Item("Debug").Activate();
                    return;
                }
                if (_assemblyInfo != null)
                {
                    SaveNuSpec();
                }
                EnsureOutputDir();
                Pack();
                ShowPackages();
                SyncVersionToDependency();
                SaveNuGetConfig();
                SaveProjectConfig();

                //编译完Release后,将项目还原成Debug目标平台
                tuple.Item2.SolutionConfigurations.Item("Debug").Activate();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogUtils.WriteLog(ex.ToString());
            }
        }


        private void SaveAssemblyInfo()
        {
            _assemblyInfo.Company = _metadata.Owners;
            //保存到程序集
            _assemblyInfo.Description = _metadata.Description;
            _assemblyInfo.Copyright = _metadata.Copyright;
            _assemblyInfo.FileVersion = _metadata.Version;
            _assemblyInfo.Title = _metadata.Id;

            _assemblyInfo.Save(true);
        }

        void SavePackageInfo()
        {
            _metadata.SyncToPackageProjectProperties(_ppp);
            _project.SavePackageProjectProperties(_ppp);
        }

        private void SyncVersionToDependency()
        {
            if (_packageOldVersion == _metadata.Version)
                return;
            NuGetExtensions.UpdateDependencyInSolution(_metadata.Id, _metadata.Version);
        }

        private void SaveNuSpec()
        {
            if (_metadata.Version.EndsWith(".*"))
            {
                var outputFileName = _project.Properties.Item("OutputFileName").Value.ToString();
                var outputFile = Path.Combine(_releaseDir, outputFileName);
                _metadata.Version = FileVersionInfo.GetVersionInfo(outputFile).FileVersion;
            }
            if (SemanticVersion.TryParse(_metadata.Version, out var ver))
                _metadata.Version = ver.ToFullString();

            _project.UpdateNuspec(_metadata);
        }

        private Tuple<bool, SolutionBuild2> Build()
        {
            var solution = (Solution2)Host.Instance.DTE.Solution;
            var solutionBuild = (SolutionBuild2)solution.SolutionBuild;
            solutionBuild.SolutionConfigurations.Item("Release").Activate();

            solutionBuild.Build(true);

            if (solutionBuild.LastBuildInfo != 0)
            {
                return Tuple.Create(false, solutionBuild);
            }

            return Tuple.Create(true, solutionBuild);
        }

        private void Pack()
        {
            var nugetExe = txtNugetPath.Text;
            //_outputDir = _outputDir.Replace("\\\\", "\\"); //this statement cause a bug of network path,see https://github.com/cnsharp/nupack/issues/20
            var script = new StringBuilder();
            script.AppendFormat(
                @"""{0}"" pack ""{1}"" -Build -Version ""{2}"" -Properties  Configuration=Release -OutputDirectory ""{3}"" ", nugetExe,
                _project.FileName, _metadata.Version, _outputDir.TrimEnd('\\'));//nuget pack path shouldn't end with slash

            if (chkForceEnglishOutput.Checked)
                script.Append(" -ForceEnglishOutput ");
            if (chkIncludeReferencedProjects.Checked)
                script.Append(" -IncludeReferencedProjects ");
            if (chkSymbol.Checked)
                script.Append(" -Symbols ");

            var deployVM = _deployControl.ViewModel;
            if (deployVM.NuGetServer.Length > 0)
            {
                script.AppendLine();
                if (!string.IsNullOrWhiteSpace(deployVM.V2Login))
                {
                    script.AppendFormat(@"""{0}"" sources Add -Name ""{1}"" -Source ""{2}"" -Username ""{3}"" -Password ""{4}""", nugetExe, deployVM.NuGetServer, deployVM.NuGetServer, deployVM.V2Login, deployVM.ApiKey);
                    script.AppendFormat(@" || ""{0}"" sources Update -Name ""{1}"" -Source ""{2}"" -Username ""{3}"" -Password ""{4}""", nugetExe, deployVM.NuGetServer, deployVM.NuGetServer, deployVM.V2Login, deployVM.ApiKey);
                    script.AppendLine();
                }

                script.AppendFormat("\"{0}\" push \"{1}{4}.{5}.nupkg\" -source \"{2}\" \"{3}\"", nugetExe, _outputDir, deployVM.NuGetServer, deployVM.ApiKey,
                    _metadata.Id, _metadata.Version);
            }

            if (chkSymbol.Checked && !string.IsNullOrWhiteSpace(deployVM.SymbolServer))
            {
                script.AppendLine();
                script.AppendFormat("\"{0}\" SetApiKey \"{1}\"", nugetExe, deployVM.ApiKey);
                script.AppendLine();
                script.AppendFormat("\"{0}\" push \"{1}{2}.{3}.symbols.nupkg\" -source \"{4}\"", nugetExe, _outputDir, _metadata.Id, _metadata.Version, deployVM.SymbolServer);
            }

            CmdUtil.RunCmd(script.ToString());
        }

        private void ShowPackages()
        {
            var outputDir = new DirectoryInfo(_outputDir);
            if (!outputDir.Exists)
                return;
            var files = outputDir.GetFiles("*.nupkg");
            if (chkOpenDir.Checked && files.Length > 0)
                Process.Start(_outputDir);
        }


        private void EnsureOutputDir()
        {
            _outputDir = txtOutputDir.Text.Trim().Replace("/", "\\");
            var relativePath = false;
            if (_outputDir.Length == 0 || !Directory.Exists(_outputDir))
            {
                if (_outputDir.Contains(":\\"))
                {
                    try
                    {
                        Directory.CreateDirectory(_outputDir);
                    }
                    catch
                    {
                        _outputDir = _projectConfig.PackageOutputDirectory;
                        relativePath = true;
                    }
                }
                else
                {
                    if (_outputDir.Length == 0)
                        _outputDir = _projectConfig.PackageOutputDirectory;
                    relativePath = true;
                }
            }
            if (relativePath)
            {
                _outputDir = Path.Combine(_dir, _outputDir.TrimStart('\\'));
                if (!Directory.Exists(_outputDir))
                    Directory.CreateDirectory(_outputDir);
            }
            if (!_outputDir.EndsWith("\\"))
                _outputDir += "\\";
        }


        /// <summary>
        /// 将nuget.exe保存到本地
        /// </summary>
        private string SaveNugetToLocal(string nugetFilePath)
        {
            FileStream pFileStream = null;
            try
            {
                pFileStream = new FileStream(nugetFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                pFileStream.Write(Resources.nuget, 0, Resources.nuget.Length);
            }
            catch (System.Exception ex)
            {
                LogUtils.WriteLog(ex.ToString());
            }
            finally
            {
                if (pFileStream != null)
                    pFileStream.Close();
            }

            return nugetFilePath;
        }
        /// <summary>
        /// 搜索解决方案下的nuget.config文件
        /// </summary>
        /// <param name="solutionProjects"></param>
        /// <returns></returns>
        private string SearchNugetConfigFilePath(IEnumerable<Project> projects)
        {
            if (projects == null)
            {
                return string.Empty;
            }

            string directory = string.Empty, currentFilePath = string.Empty;
            foreach (var project in projects)
            {
                directory = project.GetDirectory();
                currentFilePath = $"{directory}/{NugetPackageSource}";
                if (File.Exists(currentFilePath))
                {
                    return currentFilePath;
                }
                else
                {
                    //找上级节点是否存在nuget.config文件
                    var parentDirectory = Directory.GetParent(directory.TrimEnd('\\').TrimEnd('/'));
                    if (parentDirectory != null)
                    {
                        currentFilePath = $"{parentDirectory.FullName}/{NugetPackageSource}";
                        if (File.Exists(currentFilePath))
                        {
                            return currentFilePath;
                        }

                        var directories = Directory.GetDirectories(parentDirectory.FullName);
                        foreach (var dir in directories)
                        {
                            currentFilePath = $"{dir}/{NugetPackageSource}";
                            if (File.Exists(currentFilePath))
                            {
                                return currentFilePath;
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }


        private void SaveNuGetConfig()
        {
            var deployVM = _deployControl.ViewModel;
            if (!string.IsNullOrWhiteSpace(deployVM.NuGetServer))
            {
                _nuGetConfig.AddOrUpdateSource(new NuGetSource
                {
                    Url = deployVM.NuGetServer,
                    ApiKey = deployVM.RememberKey ? deployVM.ApiKey : null,
                    UserName = deployVM.V2Login,
                    Checked = true,
                });
            }
            _nuGetConfig.Save();
        }

        private void SaveProjectConfig()
        {
            if (txtOutputDir.Text == _projectConfig.PackageOutputDirectory)
                return;
            _projectConfig.PackageOutputDirectory = txtOutputDir.Text;
            _projectConfig.Save();
        }


        /// <summary>
        /// 读取最新的nuget.config配置
        /// </summary>
        /// <returns></returns>
        private NuGetSource ReadNewestNugetResource()
        {
            //vs2019正式版
            string nugetConfigFilePath = SearchNugetConfigFilePath(Host.Instance.Dte2.GetSolutionProjects());
            if (string.IsNullOrEmpty(nugetConfigFilePath))
            {
                //vs2019社区版
                string fullName = Host.Instance.Solution.FullName;
                if (!string.IsNullOrEmpty(fullName))
                {
                    string solutionPath = Path.GetDirectoryName(fullName);
                    nugetConfigFilePath = $"{solutionPath}/{NugetPackageSource}";
                }
            }

            LogUtils.WriteLog($"nuget Config File Path:{nugetConfigFilePath}");
            return ConfigHelper.GetDefaultNugetConfig(nugetConfigFilePath);
        }

    }
}