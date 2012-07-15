namespace Newsbin.Remote.TestClient
{
    partial class Main
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.m_Password = new System.Windows.Forms.TextBox();
            this.m_Port = new System.Windows.Forms.TextBox();
            this.m_IPAddress = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.bDisconnect = new System.Windows.Forms.Button();
            this.bConnect = new System.Windows.Forms.Button();
            this.lArgument2 = new System.Windows.Forms.Label();
            this.tbArgument2 = new System.Windows.Forms.TextBox();
            this.tbArgument1 = new System.Windows.Forms.TextBox();
            this.lArgument1 = new System.Windows.Forms.Label();
            this.bExuteCommand = new System.Windows.Forms.Button();
            this.lbCommands = new System.Windows.Forms.ListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lbResponse = new System.Windows.Forms.ListBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lbLog = new System.Windows.Forms.ListBox();
            this.lArgument3 = new System.Windows.Forms.Label();
            this.tbArgument3 = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.m_Password);
            this.groupBox1.Controls.Add(this.m_Port);
            this.groupBox1.Controls.Add(this.m_IPAddress);
            this.groupBox1.Location = new System.Drawing.Point(16, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(635, 50);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(410, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Password";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(339, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Port";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Newsbin address";
            // 
            // m_Password
            // 
            this.m_Password.Location = new System.Drawing.Point(469, 16);
            this.m_Password.Name = "m_Password";
            this.m_Password.Size = new System.Drawing.Size(138, 20);
            this.m_Password.TabIndex = 7;
            this.m_Password.Text = Newsbin.Remote.TestClient.Properties.Settings.Default.Password;
            // 
            // m_Port
            // 
            this.m_Port.Location = new System.Drawing.Point(371, 19);
            this.m_Port.Name = "m_Port";
            this.m_Port.Size = new System.Drawing.Size(33, 20);
            this.m_Port.TabIndex = 1;
            this.m_Port.Text = Newsbin.Remote.TestClient.Properties.Settings.Default.Port;
            // 
            // m_IPAddress
            // 
            this.m_IPAddress.Location = new System.Drawing.Point(100, 19);
            this.m_IPAddress.Name = "m_IPAddress";
            this.m_IPAddress.Size = new System.Drawing.Size(233, 20);
            this.m_IPAddress.TabIndex = 0;
            this.m_IPAddress.Text = Newsbin.Remote.TestClient.Properties.Settings.Default.IPAddress;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tbArgument3);
            this.groupBox2.Controls.Add(this.lArgument3);
            this.groupBox2.Controls.Add(this.bDisconnect);
            this.groupBox2.Controls.Add(this.bConnect);
            this.groupBox2.Controls.Add(this.lArgument2);
            this.groupBox2.Controls.Add(this.tbArgument2);
            this.groupBox2.Controls.Add(this.tbArgument1);
            this.groupBox2.Controls.Add(this.lArgument1);
            this.groupBox2.Controls.Add(this.bExuteCommand);
            this.groupBox2.Controls.Add(this.lbCommands);
            this.groupBox2.Location = new System.Drawing.Point(16, 66);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(374, 281);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Command";
            // 
            // bDisconnect
            // 
            this.bDisconnect.Enabled = false;
            this.bDisconnect.Location = new System.Drawing.Point(272, 16);
            this.bDisconnect.Name = "bDisconnect";
            this.bDisconnect.Size = new System.Drawing.Size(93, 23);
            this.bDisconnect.TabIndex = 7;
            this.bDisconnect.Text = "Disconnect";
            this.bDisconnect.UseVisualStyleBackColor = true;
            this.bDisconnect.Click += new System.EventHandler(this.bDisconnect_Click);
            // 
            // bConnect
            // 
            this.bConnect.Location = new System.Drawing.Point(177, 16);
            this.bConnect.Name = "bConnect";
            this.bConnect.Size = new System.Drawing.Size(89, 23);
            this.bConnect.TabIndex = 6;
            this.bConnect.Text = "Connect";
            this.bConnect.UseVisualStyleBackColor = true;
            this.bConnect.Click += new System.EventHandler(this.bConnect_Click);
            // 
            // lArgument2
            // 
            this.lArgument2.AutoSize = true;
            this.lArgument2.Location = new System.Drawing.Point(174, 87);
            this.lArgument2.Name = "lArgument2";
            this.lArgument2.Size = new System.Drawing.Size(35, 13);
            this.lArgument2.TabIndex = 5;
            this.lArgument2.Text = "label4";
            this.lArgument2.Visible = false;
            // 
            // tbArgument2
            // 
            this.tbArgument2.Location = new System.Drawing.Point(177, 103);
            this.tbArgument2.Name = "tbArgument2";
            this.tbArgument2.Size = new System.Drawing.Size(188, 20);
            this.tbArgument2.TabIndex = 4;
            this.tbArgument2.Visible = false;
            // 
            // tbArgument1
            // 
            this.tbArgument1.Location = new System.Drawing.Point(177, 64);
            this.tbArgument1.Name = "tbArgument1";
            this.tbArgument1.Size = new System.Drawing.Size(188, 20);
            this.tbArgument1.TabIndex = 3;
            this.tbArgument1.Visible = false;
            // 
            // lArgument1
            // 
            this.lArgument1.AutoSize = true;
            this.lArgument1.Location = new System.Drawing.Point(174, 48);
            this.lArgument1.Name = "lArgument1";
            this.lArgument1.Size = new System.Drawing.Size(35, 13);
            this.lArgument1.TabIndex = 2;
            this.lArgument1.Text = "label4";
            this.lArgument1.Visible = false;
            // 
            // bExuteCommand
            // 
            this.bExuteCommand.Enabled = false;
            this.bExuteCommand.Location = new System.Drawing.Point(245, 247);
            this.bExuteCommand.Name = "bExuteCommand";
            this.bExuteCommand.Size = new System.Drawing.Size(123, 23);
            this.bExuteCommand.TabIndex = 1;
            this.bExuteCommand.Text = "Execute command";
            this.bExuteCommand.UseVisualStyleBackColor = true;
            this.bExuteCommand.Click += new System.EventHandler(this.bExuteCommand_Click);
            // 
            // lbCommands
            // 
            this.lbCommands.FormattingEnabled = true;
            this.lbCommands.Location = new System.Drawing.Point(9, 45);
            this.lbCommands.Name = "lbCommands";
            this.lbCommands.Size = new System.Drawing.Size(159, 225);
            this.lbCommands.TabIndex = 0;
            this.lbCommands.SelectedIndexChanged += new System.EventHandler(this.lbCommands_SelectedIndexChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.lbResponse);
            this.groupBox3.Location = new System.Drawing.Point(396, 66);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(255, 281);
            this.groupBox3.TabIndex = 16;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Response";
            // 
            // lbResponse
            // 
            this.lbResponse.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbResponse.FormattingEnabled = true;
            this.lbResponse.HorizontalScrollbar = true;
            this.lbResponse.Location = new System.Drawing.Point(6, 16);
            this.lbResponse.Name = "lbResponse";
            this.lbResponse.Size = new System.Drawing.Size(243, 251);
            this.lbResponse.TabIndex = 0;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.lbLog);
            this.groupBox4.Location = new System.Drawing.Point(16, 353);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(635, 173);
            this.groupBox4.TabIndex = 17;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Log";
            // 
            // lbLog
            // 
            this.lbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbLog.FormattingEnabled = true;
            this.lbLog.HorizontalScrollbar = true;
            this.lbLog.Location = new System.Drawing.Point(9, 15);
            this.lbLog.Name = "lbLog";
            this.lbLog.Size = new System.Drawing.Size(620, 147);
            this.lbLog.TabIndex = 0;
            // 
            // lArgument3
            // 
            this.lArgument3.AutoSize = true;
            this.lArgument3.Location = new System.Drawing.Point(177, 130);
            this.lArgument3.Name = "lArgument3";
            this.lArgument3.Size = new System.Drawing.Size(35, 13);
            this.lArgument3.TabIndex = 8;
            this.lArgument3.Text = "label4";
            this.lArgument3.Visible = false;
            // 
            // tbArgument3
            // 
            this.tbArgument3.Location = new System.Drawing.Point(180, 147);
            this.tbArgument3.Name = "tbArgument3";
            this.tbArgument3.Size = new System.Drawing.Size(185, 20);
            this.tbArgument3.TabIndex = 9;
            this.tbArgument3.Visible = false;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(665, 538);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Main";
            this.Text = "Newsbin Remote Client Library Tester";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox m_Port;
        private System.Windows.Forms.TextBox m_IPAddress;
        private System.Windows.Forms.TextBox m_Password;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox lbCommands;
        private System.Windows.Forms.Button bExuteCommand;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ListBox lbResponse;
        private System.Windows.Forms.Label lArgument1;
        private System.Windows.Forms.TextBox tbArgument1;
        private System.Windows.Forms.TextBox tbArgument2;
        private System.Windows.Forms.Label lArgument2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ListBox lbLog;
        private System.Windows.Forms.Button bConnect;
        private System.Windows.Forms.Button bDisconnect;
        private System.Windows.Forms.Label lArgument3;
        private System.Windows.Forms.TextBox tbArgument3;
    }
}

