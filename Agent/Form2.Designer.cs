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
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.RouterComboBox = new System.Windows.Forms.ComboBox();
            this.ModComboBox = new System.Windows.Forms.ComboBox();
            this.WireComboBox = new System.Windows.Forms.ComboBox();
            this.BitrateComboBox = new System.Windows.Forms.ComboBox();
            this.SetConnButton = new System.Windows.Forms.Button();
            this.RemoveConnButton = new System.Windows.Forms.Button();
            this.ConnDataGridView = new System.Windows.Forms.DataGridView();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.communicationBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.consoleOutput = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.ConnDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.communicationBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(769, 174);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 33);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(769, 227);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(105, 62);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(47, 291);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 25);
            this.label1.TabIndex = 2;
            this.label1.Text = "WireID";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(84, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 25);
            this.label2.TabIndex = 3;
            this.label2.Text = "Router";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(45, 356);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(118, 25);
            this.label3.TabIndex = 4;
            this.label3.Text = "Modulation";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(47, 420);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 25);
            this.label4.TabIndex = 5;
            this.label4.Text = "Bitrate";
            // 
            // RouterComboBox
            // 
            this.RouterComboBox.Location = new System.Drawing.Point(89, 62);
            this.RouterComboBox.Name = "RouterComboBox";
            this.RouterComboBox.Size = new System.Drawing.Size(121, 33);
            this.RouterComboBox.TabIndex = 6;
            this.RouterComboBox.SelectedValueChanged += new System.EventHandler(this.RouterComboBox_SelectedValueChanged);
            // 
            // ModComboBox
            // 
            this.ModComboBox.FormattingEnabled = true;
            this.ModComboBox.Items.AddRange(new object[] {
            "QPSK",
            "SixteenQAM"});
            this.ModComboBox.Location = new System.Drawing.Point(50, 384);
            this.ModComboBox.Name = "ModComboBox";
            this.ModComboBox.Size = new System.Drawing.Size(121, 33);
            this.ModComboBox.TabIndex = 7;
            // 
            // WireComboBox
            // 
            this.WireComboBox.FormattingEnabled = true;
            this.WireComboBox.Location = new System.Drawing.Point(50, 320);
            this.WireComboBox.Name = "WireComboBox";
            this.WireComboBox.Size = new System.Drawing.Size(121, 33);
            this.WireComboBox.TabIndex = 8;
            // 
            // BitrateComboBox
            // 
            this.BitrateComboBox.FormattingEnabled = true;
            this.BitrateComboBox.Location = new System.Drawing.Point(52, 448);
            this.BitrateComboBox.Name = "BitrateComboBox";
            this.BitrateComboBox.Size = new System.Drawing.Size(121, 33);
            this.BitrateComboBox.TabIndex = 9;
            // 
            // SetConnButton
            // 
            this.SetConnButton.Location = new System.Drawing.Point(50, 510);
            this.SetConnButton.Name = "SetConnButton";
            this.SetConnButton.Size = new System.Drawing.Size(221, 51);
            this.SetConnButton.TabIndex = 10;
            this.SetConnButton.Text = "Set Konekszyn";
            this.SetConnButton.UseVisualStyleBackColor = true;
            // 
            // RemoveConnButton
            // 
            this.RemoveConnButton.Location = new System.Drawing.Point(340, 278);
            this.RemoveConnButton.Name = "RemoveConnButton";
            this.RemoveConnButton.Size = new System.Drawing.Size(221, 51);
            this.RemoveConnButton.TabIndex = 11;
            this.RemoveConnButton.Text = "Remove Konekszyn";
            this.RemoveConnButton.UseVisualStyleBackColor = true;
            this.RemoveConnButton.Click += new System.EventHandler(this.RemoveConnButton_Click);
            // 
            // ConnDataGridView
            // 
            this.ConnDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ConnDataGridView.Location = new System.Drawing.Point(43, 116);
            this.ConnDataGridView.Name = "ConnDataGridView";
            this.ConnDataGridView.RowTemplate.Height = 33;
            this.ConnDataGridView.Size = new System.Drawing.Size(518, 150);
            this.ConnDataGridView.TabIndex = 12;
            this.ConnDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ConnDataGridView_CellContentClick);
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(200, 384);
            this.trackBar1.Maximum = 1000;
            this.trackBar1.Minimum = 10;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(430, 90);
            this.trackBar1.SmallChange = 10;
            this.trackBar1.TabIndex = 13;
            this.trackBar1.TickFrequency = 10;
            this.trackBar1.Value = 10;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // communicationBindingSource
            // 
            this.communicationBindingSource.DataSource = typeof(Agent.Communication);
            // 
            // consoleOutput
            // 
            this.consoleOutput.Location = new System.Drawing.Point(917, 210);
            this.consoleOutput.Multiline = true;
            this.consoleOutput.Name = "consoleOutput";
            this.consoleOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.consoleOutput.Size = new System.Drawing.Size(914, 528);
            this.consoleOutput.TabIndex = 14;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1873, 838);
            this.Controls.Add(this.consoleOutput);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.ConnDataGridView);
            this.Controls.Add(this.RemoveConnButton);
            this.Controls.Add(this.SetConnButton);
            this.Controls.Add(this.BitrateComboBox);
            this.Controls.Add(this.WireComboBox);
            this.Controls.Add(this.ModComboBox);
            this.Controls.Add(this.RouterComboBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBox1);
            this.Name = "Form2";
            this.Text = "Form2";
            this.Load += new System.EventHandler(this.Form2_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ConnDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.communicationBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox RouterComboBox;
        private System.Windows.Forms.ComboBox ModComboBox;
        private System.Windows.Forms.ComboBox WireComboBox;
        private System.Windows.Forms.ComboBox BitrateComboBox;
        private System.Windows.Forms.Button SetConnButton;
        private System.Windows.Forms.Button RemoveConnButton;
        private System.Windows.Forms.DataGridView ConnDataGridView;
        private System.Windows.Forms.BindingSource communicationBindingSource;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.TextBox consoleOutput;
    }
}