namespace NewsbinRemote
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
            this.button1 = new System.Windows.Forms.Button();
            this.StatusList = new System.Windows.Forms.ListView();
            this.Status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.m_ListFiles = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.m_MaxAge = new System.Windows.Forms.TextBox();
            this.m_MinAge = new System.Windows.Forms.TextBox();
            this.m_GroupText = new System.Windows.Forms.TextBox();
            this.m_Password = new System.Windows.Forms.TextBox();
            this.m_Key = new System.Windows.Forms.TextBox();
            this.m_Port = new System.Windows.Forms.TextBox();
            this.m_IPAddress = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(11, 527);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(78, 29);
            this.button1.TabIndex = 0;
            this.button1.Text = "Connect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnConnect);
            // 
            // StatusList
            // 
            this.StatusList.AllowColumnReorder = true;
            this.StatusList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Status});
            this.StatusList.FullRowSelect = true;
            this.StatusList.GridLines = true;
            this.StatusList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.StatusList.HideSelection = false;
            this.StatusList.Location = new System.Drawing.Point(12, 131);
            this.StatusList.Name = "StatusList";
            this.StatusList.Size = new System.Drawing.Size(879, 377);
            this.StatusList.TabIndex = 1;
            this.StatusList.UseCompatibleStateImageBehavior = false;
            this.StatusList.View = System.Windows.Forms.View.Details;
            // 
            // Status
            // 
            this.Status.Text = "Status";
            this.Status.Width = 872;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.m_MaxAge);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.m_MinAge);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.m_GroupText);
            this.groupBox1.Controls.Add(this.m_Password);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.m_Key);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.m_Port);
            this.groupBox1.Controls.Add(this.m_IPAddress);
            this.groupBox1.Location = new System.Drawing.Point(16, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(874, 115);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(634, 50);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(64, 16);
            this.label7.TabIndex = 13;
            this.label7.Text = "Max Age:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(437, 47);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 16);
            this.label6.TabIndex = 11;
            this.label6.Text = "Min Age:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(433, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 16);
            this.label5.TabIndex = 9;
            this.label5.Text = "Groups:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(447, 81);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 16);
            this.label4.TabIndex = 6;
            this.label4.Text = "Password:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(15, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Key:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Port:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "IP Address:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(14, 573);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 28);
            this.button2.TabIndex = 3;
            this.button2.Text = "Add Group";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.OnAddGroup);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(176, 573);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 28);
            this.button3.TabIndex = 4;
            this.button3.Text = "Load Group";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.OnLoadGroup);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(479, 528);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 28);
            this.button4.TabIndex = 5;
            this.button4.Text = "Upload NZB";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.OnUploadNZB);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(95, 573);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 28);
            this.button6.TabIndex = 7;
            this.button6.Text = "ClearGroups";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.OnClearGroups);
            // 
            // m_ListFiles
            // 
            this.m_ListFiles.Location = new System.Drawing.Point(16, 619);
            this.m_ListFiles.Name = "m_ListFiles";
            this.m_ListFiles.Size = new System.Drawing.Size(75, 28);
            this.m_ListFiles.TabIndex = 8;
            this.m_ListFiles.Text = "List Files";
            this.m_ListFiles.UseVisualStyleBackColor = true;
            this.m_ListFiles.Click += new System.EventHandler(this.OnListFiles);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(97, 619);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 28);
            this.button7.TabIndex = 9;
            this.button7.Text = "Upload Files";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.OnUploadFiles);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(261, 573);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(97, 28);
            this.button8.TabIndex = 10;
            this.button8.Text = "Download Files";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.OnDownloadFiles);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(364, 573);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(96, 28);
            this.button9.TabIndex = 11;
            this.button9.Text = "List Downloads";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.OnListDownloads);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(560, 528);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(102, 28);
            this.button10.TabIndex = 12;
            this.button10.Text = "Autoload NZB";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.OnAutoloadNZB);
            // 
            // m_MaxAge
            // 
            this.m_MaxAge.Location = new System.Drawing.Point(702, 47);
            this.m_MaxAge.Name = "m_MaxAge";
            this.m_MaxAge.Size = new System.Drawing.Size(104, 20);
            this.m_MaxAge.TabIndex = 12;
            this.m_MaxAge.Text = global::NewsbinRemote.Properties.Settings.Default.MaxAge;
            // 
            // m_MinAge
            // 
            this.m_MinAge.Location = new System.Drawing.Point(513, 46);
            this.m_MinAge.Name = "m_MinAge";
            this.m_MinAge.Size = new System.Drawing.Size(92, 20);
            this.m_MinAge.TabIndex = 10;
            this.m_MinAge.Text = global::NewsbinRemote.Properties.Settings.Default.MinAge;
            // 
            // m_GroupText
            // 
            this.m_GroupText.Location = new System.Drawing.Point(509, 18);
            this.m_GroupText.Name = "m_GroupText";
            this.m_GroupText.Size = new System.Drawing.Size(298, 20);
            this.m_GroupText.TabIndex = 8;
            this.m_GroupText.Text = global::NewsbinRemote.Properties.Settings.Default.Groups;
            // 
            // m_Password
            // 
            this.m_Password.Location = new System.Drawing.Point(529, 81);
            this.m_Password.Name = "m_Password";
            this.m_Password.Size = new System.Drawing.Size(138, 20);
            this.m_Password.TabIndex = 7;
            this.m_Password.Text = global::NewsbinRemote.Properties.Settings.Default.Password;
            // 
            // m_Key
            // 
            this.m_Key.Location = new System.Drawing.Point(109, 81);
            this.m_Key.Name = "m_Key";
            this.m_Key.Size = new System.Drawing.Size(323, 20);
            this.m_Key.TabIndex = 5;
            this.m_Key.Text = global::NewsbinRemote.Properties.Settings.Default.Key;
            // 
            // m_Port
            // 
            this.m_Port.Location = new System.Drawing.Point(109, 50);
            this.m_Port.Name = "m_Port";
            this.m_Port.Size = new System.Drawing.Size(156, 20);
            this.m_Port.TabIndex = 1;
            this.m_Port.Text = global::NewsbinRemote.Properties.Settings.Default.Port;
            // 
            // m_IPAddress
            // 
            this.m_IPAddress.Location = new System.Drawing.Point(109, 19);
            this.m_IPAddress.Name = "m_IPAddress";
            this.m_IPAddress.Size = new System.Drawing.Size(233, 20);
            this.m_IPAddress.TabIndex = 0;
            this.m_IPAddress.Text = global::NewsbinRemote.Properties.Settings.Default.IPAddress;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(922, 680);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.m_ListFiles);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.StatusList);
            this.Controls.Add(this.button1);
            this.Name = "Main";
            //this.Text = "Newsbin Remote ";
            this.Text = System.Windows.Forms.Application.ProductName;
            this.Text += " ";
            this.Text += System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Load += new System.EventHandler(this.Main_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListView StatusList;
        private System.Windows.Forms.ColumnHeader Status;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox m_Port;
        private System.Windows.Forms.TextBox m_IPAddress;
        private System.Windows.Forms.TextBox m_Key;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox m_Password;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox m_GroupText;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox m_MaxAge;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox m_MinAge;
        private System.Windows.Forms.Button m_ListFiles;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
		private System.Windows.Forms.Button button10;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    }
}

