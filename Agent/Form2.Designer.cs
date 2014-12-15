namespace Agent
{
    partial class Form2
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
            this.clientATextBox = new System.Windows.Forms.TextBox();
            this.routeTextBox = new System.Windows.Forms.TextBox();
            this.clientBTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.ConHashLabel = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.startFreqTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.communicationBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.ConnDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.banwidthTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.communicationBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 2);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 25);
            this.label2.TabIndex = 3;
            this.label2.Text = "Routers";
            // 
            // SetConnButton
            // 
            this.SetConnButton.Location = new System.Drawing.Point(586, 319);
            this.SetConnButton.Name = "SetConnButton";
            this.SetConnButton.Size = new System.Drawing.Size(329, 90);
            this.SetConnButton.TabIndex = 10;
            this.SetConnButton.Text = "Set Connection";
            this.SetConnButton.UseVisualStyleBackColor = true;
            this.SetConnButton.Click += new System.EventHandler(this.SetConnButton_Click);
            // 
            // RemoveConnButton
            // 
            this.RemoveConnButton.Location = new System.Drawing.Point(487, 540);
            this.RemoveConnButton.Name = "RemoveConnButton";
            this.RemoveConnButton.Size = new System.Drawing.Size(428, 51);
            this.RemoveConnButton.TabIndex = 11;
            this.RemoveConnButton.Text = "Remove Connection";
            this.RemoveConnButton.UseVisualStyleBackColor = true;
            this.RemoveConnButton.Click += new System.EventHandler(this.RemoveConnButton_Click);
            // 
            // ConnDataGridView
            // 
            this.ConnDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ConnDataGridView.Location = new System.Drawing.Point(153, 49);
            this.ConnDataGridView.Name = "ConnDataGridView";
            this.ConnDataGridView.RowTemplate.Height = 33;
            this.ConnDataGridView.Size = new System.Drawing.Size(762, 229);
            this.ConnDataGridView.TabIndex = 12;
            this.ConnDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ConnDataGridView_CellContentClick);
            // 
            // banwidthTrackBar
            // 
            this.banwidthTrackBar.LargeChange = 10;
            this.banwidthTrackBar.Location = new System.Drawing.Point(150, 319);
            this.banwidthTrackBar.Maximum = 1000;
            this.banwidthTrackBar.Minimum = 10;
            this.banwidthTrackBar.Name = "banwidthTrackBar";
            this.banwidthTrackBar.Size = new System.Drawing.Size(430, 90);
            this.banwidthTrackBar.SmallChange = 10;
            this.banwidthTrackBar.TabIndex = 13;
            this.banwidthTrackBar.TickFrequency = 10;
            this.banwidthTrackBar.Value = 10;
            this.banwidthTrackBar.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // consoleOutput
            // 
            this.consoleOutput.Location = new System.Drawing.Point(12, 665);
            this.consoleOutput.Multiline = true;
            this.consoleOutput.Name = "consoleOutput";
            this.consoleOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.consoleOutput.Size = new System.Drawing.Size(903, 240);
            this.consoleOutput.TabIndex = 14;
            // 
            // bandwidthTextBox
            // 
            this.bandwidthTextBox.Location = new System.Drawing.Point(318, 378);
            this.bandwidthTextBox.Name = "bandwidthTextBox";
            this.bandwidthTextBox.ReadOnly = true;
            this.bandwidthTextBox.Size = new System.Drawing.Size(100, 31);
            this.bandwidthTextBox.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(287, 291);
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
            this.ConHashComboBox.Location = new System.Drawing.Point(166, 550);
            this.ConHashComboBox.Name = "ConHashComboBox";
            this.ConHashComboBox.Size = new System.Drawing.Size(300, 33);
            this.ConHashComboBox.TabIndex = 17;
            // 
            // routerListBox
            // 
            this.routerListBox.FormattingEnabled = true;
            this.routerListBox.ItemHeight = 25;
            this.routerListBox.Location = new System.Drawing.Point(12, 30);
            this.routerListBox.Name = "routerListBox";
            this.routerListBox.Size = new System.Drawing.Size(120, 229);
            this.routerListBox.TabIndex = 19;
            // 
            // clientListBox
            // 
            this.clientListBox.FormattingEnabled = true;
            this.clientListBox.ItemHeight = 25;
            this.clientListBox.Location = new System.Drawing.Point(12, 291);
            this.clientListBox.Name = "clientListBox";
            this.clientListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.clientListBox.Size = new System.Drawing.Size(120, 279);
            this.clientListBox.TabIndex = 20;
            this.clientListBox.SelectedIndexChanged += new System.EventHandler(this.clientListBox_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(33, 262);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 25);
            this.label4.TabIndex = 21;
            this.label4.Text = "Clients";
            // 
            // clientATextBox
            // 
            this.clientATextBox.Location = new System.Drawing.Point(633, 473);
            this.clientATextBox.Name = "clientATextBox";
            this.clientATextBox.Size = new System.Drawing.Size(60, 31);
            this.clientATextBox.TabIndex = 24;
            // 
            // routeTextBox
            // 
            this.routeTextBox.Location = new System.Drawing.Point(157, 474);
            this.routeTextBox.Name = "routeTextBox";
            this.routeTextBox.Size = new System.Drawing.Size(374, 31);
            this.routeTextBox.TabIndex = 25;
            // 
            // clientBTextBox
            // 
            this.clientBTextBox.Location = new System.Drawing.Point(804, 474);
            this.clientBTextBox.Name = "clientBTextBox";
            this.clientBTextBox.Size = new System.Drawing.Size(100, 31);
            this.clientBTextBox.TabIndex = 26;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(711, 429);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(193, 25);
            this.label6.TabIndex = 27;
            this.label6.Text = "Client B ip address";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(515, 429);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(193, 25);
            this.label7.TabIndex = 28;
            this.label7.Text = "Client A ip address";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(152, 429);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(357, 25);
            this.label8.TabIndex = 29;
            this.label8.Text = "Route (routers IDs split with spaces)";
            // 
            // ConHashLabel
            // 
            this.ConHashLabel.AutoSize = true;
            this.ConHashLabel.Location = new System.Drawing.Point(161, 522);
            this.ConHashLabel.Name = "ConHashLabel";
            this.ConHashLabel.Size = new System.Drawing.Size(214, 25);
            this.ConHashLabel.TabIndex = 18;
            this.ConHashLabel.Text = "Connection HashKey";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(711, 477);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(90, 25);
            this.label9.TabIndex = 30;
            this.label9.Text = "127.0.0.";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(537, 477);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(90, 25);
            this.label10.TabIndex = 31;
            this.label10.Text = "127.0.0.";
            // 
            // startFreqTextBox
            // 
            this.startFreqTextBox.Location = new System.Drawing.Point(344, 608);
            this.startFreqTextBox.Name = "startFreqTextBox";
            this.startFreqTextBox.Size = new System.Drawing.Size(100, 31);
            this.startFreqTextBox.TabIndex = 32;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(124, 614);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(194, 25);
            this.label1.TabIndex = 33;
            this.label1.Text = "Starting Frequency";
            // 
            // communicationBindingSource
            // 
            this.communicationBindingSource.DataSource = typeof(Agent.Communication);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(923, 942);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.startFreqTextBox);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.clientBTextBox);
            this.Controls.Add(this.routeTextBox);
            this.Controls.Add(this.clientATextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.clientListBox);
            this.Controls.Add(this.routerListBox);
            this.Controls.Add(this.ConHashLabel);
            this.Controls.Add(this.ConHashComboBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.bandwidthTextBox);
            this.Controls.Add(this.consoleOutput);
            this.Controls.Add(this.banwidthTrackBar);
            this.Controls.Add(this.ConnDataGridView);
            this.Controls.Add(this.RemoveConnButton);
            this.Controls.Add(this.SetConnButton);
            this.Controls.Add(this.label2);
            this.Name = "Form2";
            this.Text = "NMS";
            this.Load += new System.EventHandler(this.Form2_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ConnDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.banwidthTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.communicationBindingSource)).EndInit();
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
        private System.Windows.Forms.TextBox clientATextBox;
        private System.Windows.Forms.TextBox routeTextBox;
        private System.Windows.Forms.TextBox clientBTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label ConHashLabel;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox startFreqTextBox;
        private System.Windows.Forms.Label label1;
    }
}