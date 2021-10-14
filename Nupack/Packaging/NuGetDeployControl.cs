using System;
using System.Linq;
using System.Windows.Forms;

namespace CnSharp.VisualStudio.NuPack.Packaging
{
    public partial class NuGetDeployControl : UserControl
    {
        private NuGetDeployViewModel _viewModel;
        private NuGetConfig _nuGetConfig;
        private int TopPanelDefaultFixedHeight = 0;
        private int TopPanelAdjustHeight = 0;

        public NuGetDeployControl()
        {
            InitializeComponent();

            textBoxSymbolServer.Enabled = false;
            textBoxSymbolServer.TextChanged += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(textBoxSymbolServer.Text))
                    textBoxSymbolServer.Enabled = true;
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            TopPanelDefaultFixedHeight = pnlTop.Height;
            TopPanelAdjustHeight = pnlTop.Height - 60;
        }

        public NuGetConfig NuGetConfig
        {
            get { return _nuGetConfig; }
            set
            {
                _nuGetConfig = value;

                sourceBox.Items.Clear();
                if (_nuGetConfig == null || _nuGetConfig.Sources == null || _nuGetConfig.Sources.Count <= 0)
                {
                    this.sourceBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
                    return;
                }

                this.sourceBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                foreach (var source in _nuGetConfig.Sources)
                {
                    sourceBox.Items.Add(source.Url);
                }
            }
        }

        public NuGetDeployViewModel ViewModel
        {
            get
            {
                if (_viewModel == null)
                    _viewModel = new NuGetDeployViewModel();
                _viewModel.NuGetServer = sourceBox.Text;
                _viewModel.RememberKey = chkRemember.Checked;
                _viewModel.ApiKey = textBoxApiKey.Text;
                _viewModel.V2Login = textBoxLogin.Text;
                return _viewModel;
            }
            set
            {
                _viewModel = value;

                sourceBox.DataBindings.Clear();
                sourceBox.DataBindings.Add("Text", _viewModel, "NuGetServer", true, DataSourceUpdateMode.OnPropertyChanged);
                textBoxApiKey.DataBindings.Clear();
                textBoxApiKey.DataBindings.Add("Text", _viewModel, "ApiKey", true, DataSourceUpdateMode.OnPropertyChanged);

                if (!string.IsNullOrWhiteSpace(_viewModel.SymbolServer))
                {
                    textBoxSymbolServer.Enabled = true;
                }
                textBoxSymbolServer.DataBindings.Clear();
                textBoxSymbolServer.DataBindings.Add("Text", _viewModel, "SymbolServer", true, DataSourceUpdateMode.OnPropertyChanged);

                textBoxLogin.DataBindings.Clear();
                textBoxLogin.DataBindings.Add("Text", _viewModel, "V2Login", true, DataSourceUpdateMode.OnPropertyChanged);
                textBoxLogin.Tag = _viewModel.V2Login;
            }
        }

        private void sourceBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var url = sourceBox.Text.Trim();
            Set(url);
        }

        private void Set(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            var source = NuGetConfig.Sources.FirstOrDefault(m => m.Url == url);
            textBoxApiKey.Text = source?.ApiKey ?? string.Empty;
            chkRemember.Checked = textBoxApiKey.Text.Length > 0;
            textBoxLogin.Text = source?.UserName;
            checkBoxNugetLogin.Checked = textBoxLogin.Text.Length > 0;
            pnlTop.Height = checkBoxNugetLogin.Checked ? TopPanelDefaultFixedHeight : TopPanelAdjustHeight;
        }


        private void checkBoxNugetLogin_CheckedChanged(object sender, EventArgs e)
        {
            var check = sender as CheckBox;
            textBoxLogin.Visible = check.Checked;
            labelLogin.Visible = check.Checked;

            if (check.Checked)
            {
                pnlTop.Height = TopPanelDefaultFixedHeight;
            }
            else
            {
                pnlTop.Height = TopPanelAdjustHeight;
            }
        }

        private void lnkRemoveNugetServer_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (NuGetConfig.Sources != null && NuGetConfig.Sources.Count > 0)
            {
                DialogResult dRes = MessageBox.Show(this.FindForm(), "are you sure to remove other nuget server?", "Tips", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dRes == DialogResult.Yes)
                {
                    if (NuGetConfig.Sources.Count == 1)
                    {
                        NuGetConfig.Sources.Clear();
                    }
                    else
                    {
                        string userName = (textBoxLogin.Tag == null || string.IsNullOrEmpty(textBoxLogin.Tag.ToString())) ? textBoxLogin.Text.Trim() : textBoxLogin.Tag.ToString();
                        NuGetConfig.Sources.RemoveAll(item => item.Url != sourceBox.Text.Trim() && item.UserName != userName);
                    }
                }
            }
        }
    }

    public class NuGetDeployViewModel
    {
        public string NuGetServer { get; set; }
        public string ApiKey { get; set; }
        public bool RememberKey { get; set; }
        public string SymbolServer { get; set; }
        public string V2Login { get; set; }
    }
}
