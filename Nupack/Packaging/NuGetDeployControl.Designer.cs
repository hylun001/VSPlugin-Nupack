namespace CnSharp.VisualStudio.NuPack.Packaging
{
    partial class NuGetDeployControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxSymbolServer = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.labelLogin = new System.Windows.Forms.Label();
            this.textBoxLogin = new System.Windows.Forms.TextBox();
            this.checkBoxNugetLogin = new System.Windows.Forms.CheckBox();
            this.chkRemember = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.sourceBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxApiKey = new System.Windows.Forms.TextBox();
            this.lnkRemoveNugetServer = new System.Windows.Forms.LinkLabel();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.pnlBottom.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxSymbolServer
            // 
            this.textBoxSymbolServer.Location = new System.Drawing.Point(127, 70);
            this.textBoxSymbolServer.Name = "textBoxSymbolServer";
            this.textBoxSymbolServer.Size = new System.Drawing.Size(412, 21);
            this.textBoxSymbolServer.TabIndex = 33;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(26, 73);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 39;
            this.label5.Text = "Symbol Server:";
            // 
            // labelLogin
            // 
            this.labelLogin.AutoSize = true;
            this.labelLogin.Location = new System.Drawing.Point(26, 96);
            this.labelLogin.Name = "labelLogin";
            this.labelLogin.Size = new System.Drawing.Size(41, 12);
            this.labelLogin.TabIndex = 38;
            this.labelLogin.Text = "Login:";
            this.labelLogin.Visible = false;
            // 
            // textBoxLogin
            // 
            this.textBoxLogin.Location = new System.Drawing.Point(127, 93);
            this.textBoxLogin.Name = "textBoxLogin";
            this.textBoxLogin.Size = new System.Drawing.Size(412, 21);
            this.textBoxLogin.TabIndex = 35;
            this.textBoxLogin.Visible = false;
            // 
            // checkBoxNugetLogin
            // 
            this.checkBoxNugetLogin.AutoSize = true;
            this.checkBoxNugetLogin.Location = new System.Drawing.Point(127, 54);
            this.checkBoxNugetLogin.Name = "checkBoxNugetLogin";
            this.checkBoxNugetLogin.Size = new System.Drawing.Size(132, 16);
            this.checkBoxNugetLogin.TabIndex = 37;
            this.checkBoxNugetLogin.Text = "Use NuGet V2 login";
            this.checkBoxNugetLogin.UseVisualStyleBackColor = true;
            this.checkBoxNugetLogin.CheckedChanged += new System.EventHandler(this.checkBoxNugetLogin_CheckedChanged);
            // 
            // chkRemember
            // 
            this.chkRemember.AutoSize = true;
            this.chkRemember.Checked = true;
            this.chkRemember.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRemember.Location = new System.Drawing.Point(556, 23);
            this.chkRemember.Name = "chkRemember";
            this.chkRemember.Size = new System.Drawing.Size(90, 16);
            this.chkRemember.TabIndex = 32;
            this.chkRemember.Text = "Remember it";
            this.chkRemember.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 12);
            this.label1.TabIndex = 34;
            this.label1.Text = "NuGet Server:";
            // 
            // sourceBox
            // 
            this.sourceBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sourceBox.FormattingEnabled = true;
            this.sourceBox.ItemHeight = 12;
            this.sourceBox.Location = new System.Drawing.Point(127, 12);
            this.sourceBox.Name = "sourceBox";
            this.sourceBox.Size = new System.Drawing.Size(412, 20);
            this.sourceBox.TabIndex = 30;
            this.sourceBox.SelectedIndexChanged += new System.EventHandler(this.sourceBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 36;
            this.label2.Text = "API Key:";
            // 
            // textBoxApiKey
            // 
            this.textBoxApiKey.Location = new System.Drawing.Point(127, 18);
            this.textBoxApiKey.Name = "textBoxApiKey";
            this.textBoxApiKey.PasswordChar = '*';
            this.textBoxApiKey.Size = new System.Drawing.Size(412, 21);
            this.textBoxApiKey.TabIndex = 31;
            // 
            // lnkRemoveNugetServer
            // 
            this.lnkRemoveNugetServer.AutoSize = true;
            this.lnkRemoveNugetServer.Location = new System.Drawing.Point(259, 55);
            this.lnkRemoveNugetServer.Name = "lnkRemoveNugetServer";
            this.lnkRemoveNugetServer.Size = new System.Drawing.Size(155, 12);
            this.lnkRemoveNugetServer.TabIndex = 40;
            this.lnkRemoveNugetServer.TabStop = true;
            this.lnkRemoveNugetServer.Text = "remove other nuget server";
            this.lnkRemoveNugetServer.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRemoveNugetServer_LinkClicked);
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.label2);
            this.pnlBottom.Controls.Add(this.textBoxApiKey);
            this.pnlBottom.Controls.Add(this.textBoxSymbolServer);
            this.pnlBottom.Controls.Add(this.chkRemember);
            this.pnlBottom.Controls.Add(this.label5);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBottom.Location = new System.Drawing.Point(0, 120);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(675, 119);
            this.pnlBottom.TabIndex = 41;
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.sourceBox);
            this.pnlTop.Controls.Add(this.label1);
            this.pnlTop.Controls.Add(this.lnkRemoveNugetServer);
            this.pnlTop.Controls.Add(this.checkBoxNugetLogin);
            this.pnlTop.Controls.Add(this.labelLogin);
            this.pnlTop.Controls.Add(this.textBoxLogin);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(675, 120);
            this.pnlTop.TabIndex = 42;
            // 
            // NuGetDeployControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlTop);
            this.Name = "NuGetDeployControl";
            this.Size = new System.Drawing.Size(675, 239);
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxSymbolServer;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label labelLogin;
        private System.Windows.Forms.TextBox textBoxLogin;
        private System.Windows.Forms.CheckBox checkBoxNugetLogin;
        private System.Windows.Forms.CheckBox chkRemember;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox sourceBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxApiKey;
        private System.Windows.Forms.LinkLabel lnkRemoveNugetServer;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Panel pnlTop;
    }
}
