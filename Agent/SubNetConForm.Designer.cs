namespace SubnetworkController
{
    partial class SubNetConForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SubNetConForm));
            this.label2 = new System.Windows.Forms.Label();
            this.SetConnButton = new System.Windows.Forms.Button();
            this.RemoveConnButton = new System.Windows.Forms.Button();
            this.ConnDataGridView = new System.Windows.Forms.DataGridView();
            this.banwidthTrackBar = new System.Windows.Forms.TrackBar();
            this.consoleOutput = new System.Windows.Forms.TextBox();
            this.bandwidthTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ConHashComboBox = new System.Windows.Forms.ComboBox();
            this.routerListBox = new System.Windows.Forms.ListBox();
            this.clientListBox = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.routeTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.ConHashLabel = new System.Windows.Forms.Label();
            this.startFreqTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.communicationBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.ConnDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.banwidthTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.communicationBindingSource)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 25);
            this.label2.TabIndex = 3;
            this.label2.Text = "Nodes";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // SetConnButton
            // 
            this.SetConnButton.Location = new System.Drawing.Point(13, 359);
            this.SetConnButton.Name = "SetConnButton";
            this.SetConnButton.Size = new System.Drawing.Size(411, 57);
            this.SetConnButton.TabIndex = 10;
            this.SetConnButton.Text = "Set Connection";
            this.SetConnButton.UseVisualStyleBackColor = true;
            this.SetConnButton.Click += new System.EventHandler(this.SetConnButton_Click);
            // 
            // RemoveConnButton
            // 
            this.RemoveConnButton.Location = new System.Drawing.Point(13, 543);
            this.RemoveConnButton.Name = "RemoveConnButton";
            this.RemoveConnButton.Size = new System.Drawing.Size(411, 57);
            this.RemoveConnButton.TabIndex = 11;
            this.RemoveConnButton.Text = "Remove Connection";
            this.RemoveConnButton.UseVisualStyleBackColor = true;
            this.RemoveConnButton.Click += new System.EventHandler(this.RemoveConnButton_Click);
            // 
            // ConnDataGridView
            // 
            this.ConnDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.ConnDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ConnDataGridView.Location = new System.Drawing.Point(253, 12);
            this.ConnDataGridView.Name = "ConnDataGridView";
            this.ConnDataGridView.RowHeadersVisible = false;
            this.ConnDataGridView.RowTemplate.Height = 33;
            this.ConnDataGridView.Size = new System.Drawing.Size(730, 314);
            this.ConnDataGridView.TabIndex = 12;
            this.ConnDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ConnDataGridView_CellContentClick);
            // 
            // banwidthTrackBar
            // 
            this.banwidthTrackBar.LargeChange = 10;
            this.banwidthTrackBar.Location = new System.Drawing.Point(13, 105);
            this.banwidthTrackBar.Maximum = 1000;
            this.banwidthTrackBar.Minimum = 50;
            this.banwidthTrackBar.Name = "banwidthTrackBar";
            this.banwidthTrackBar.Size = new System.Drawing.Size(411, 90);
            this.banwidthTrackBar.SmallChange = 10;
            this.banwidthTrackBar.TabIndex = 13;
            this.banwidthTrackBar.TickFrequency = 10;
            this.banwidthTrackBar.Value = 50;
            this.banwidthTrackBar.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // consoleOutput
            // 
            this.consoleOutput.Location = new System.Drawing.Point(465, 355);
            this.consoleOutput.Multiline = true;
            this.consoleOutput.Name = "consoleOutput";
            this.consoleOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.consoleOutput.Size = new System.Drawing.Size(518, 604);
            this.consoleOutput.TabIndex = 14;
            // 
            // bandwidthTextBox
            // 
            this.bandwidthTextBox.Location = new System.Drawing.Point(229, 52);
            this.bandwidthTextBox.Name = "bandwidthTextBox";
            this.bandwidthTextBox.ReadOnly = true;
            this.bandwidthTextBox.Size = new System.Drawing.Size(195, 31);
            this.bandwidthTextBox.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(32, 52);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(163, 25);
            this.label5.TabIndex = 16;
            this.label5.Text = "Spectrum [GHz]";
            // 
            // ConHashComboBox
            // 
            this.ConHashComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ConHashComboBox.FormattingEnabled = true;
            this.ConHashComboBox.Items.AddRange(new object[] {
            ""});
            this.ConHashComboBox.Location = new System.Drawing.Point(13, 485);
            this.ConHashComboBox.Name = "ConHashComboBox";
            this.ConHashComboBox.Size = new System.Drawing.Size(411, 33);
            this.ConHashComboBox.TabIndex = 17;
            // 
            // routerListBox
            // 
            this.routerListBox.FormattingEnabled = true;
            this.routerListBox.ItemHeight = 25;
            this.routerListBox.Location = new System.Drawing.Point(12, 85);
            this.routerListBox.Name = "routerListBox";
            this.routerListBox.Size = new System.Drawing.Size(84, 229);
            this.routerListBox.TabIndex = 19;
            // 
            // clientListBox
            // 
            this.clientListBox.FormattingEnabled = true;
            this.clientListBox.ItemHeight = 25;
            this.clientListBox.Location = new System.Drawing.Point(122, 85);
            this.clientListBox.Name = "clientListBox";
            this.clientListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.clientListBox.Size = new System.Drawing.Size(84, 229);
            this.clientListBox.TabIndex = 20;
            this.clientListBox.SelectedIndexChanged += new System.EventHandler(this.clientListBox_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(117, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 25);
            this.label4.TabIndex = 21;
            this.label4.Text = "Clients";
            // 
            // routeTextBox
            // 
            this.routeTextBox.Location = new System.Drawing.Point(13, 249);
            this.routeTextBox.Name = "routeTextBox";
            this.routeTextBox.Size = new System.Drawing.Size(411, 31);
            this.routeTextBox.TabIndex = 25;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(32, 198);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(357, 25);
            this.label8.TabIndex = 29;
            this.label8.Text = "Route (routers IDs split with spaces)";
            // 
            // ConHashLabel
            // 
            this.ConHashLabel.AutoSize = true;
            this.ConHashLabel.Location = new System.Drawing.Point(32, 437);
            this.ConHashLabel.Name = "ConHashLabel";
            this.ConHashLabel.Size = new System.Drawing.Size(214, 25);
            this.ConHashLabel.TabIndex = 18;
            this.ConHashLabel.Text = "Connection HashKey";
            // 
            // startFreqTextBox
            // 
            this.startFreqTextBox.Location = new System.Drawing.Point(229, 298);
            this.startFreqTextBox.Name = "startFreqTextBox";
            this.startFreqTextBox.Size = new System.Drawing.Size(195, 31);
            this.startFreqTextBox.TabIndex = 32;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 298);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(194, 25);
            this.label1.TabIndex = 33;
            this.label1.Text = "Starting Frequency";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1121, 145);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(329, 86);
            this.button1.TabIndex = 34;
            this.button1.Text = "Test Send";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.routerListBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.clientListBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(226, 323);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Online";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.startFreqTextBox);
            this.groupBox2.Controls.Add(this.banwidthTrackBar);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.bandwidthTextBox);
            this.groupBox2.Controls.Add(this.RemoveConnButton);
            this.groupBox2.Controls.Add(this.ConHashComboBox);
            this.groupBox2.Controls.Add(this.SetConnButton);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.ConHashLabel);
            this.groupBox2.Controls.Add(this.routeTextBox);
            this.groupBox2.Location = new System.Drawing.Point(4, 341);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(443, 618);
            this.groupBox2.TabIndex = 36;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Manual Connecting";
            // 
            // SubNetConForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(994, 969);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.consoleOutput);
            this.Controls.Add(this.ConnDataGridView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SubNetConForm";
            this.Text = "NMS";
            this.Load += new System.EventHandler(this.Form2_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ConnDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.banwidthTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.communicationBindingSource)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button SetConnButton;
        private System.Windows.Forms.Button RemoveConnButton;
        private System.Windows.Forms.DataGridView ConnDataGridView;
        private System.Windows.Forms.BindingSource communicationBindingSource;
        private System.Windows.Forms.TrackBar banwidthTrackBar;
        private System.Windows.Forms.TextBox consoleOutput;
        private System.Windows.Forms.TextBox bandwidthTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox ConHashComboBox;
        private System.Windows.Forms.ListBox routerListBox;
        private System.Windows.Forms.ListBox clientListBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox routeTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label ConHashLabel;
        private System.Windows.Forms.TextBox startFreqTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}