﻿namespace Client
{
    partial class ClientForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.routerAddress = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.targetAddress = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.message = new System.Windows.Forms.TextBox();
            this.sendButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.connectionBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.band = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(344, 25);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(167, 63);
            this.label1.TabIndex = 0;
            this.label1.Text = "Client";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelName.Location = new System.Drawing.Point(558, 25);
            this.labelName.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(387, 63);
            this.labelName.TabIndex = 1;
            this.labelName.Text = "127.0.0.1:7000";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.Location = new System.Drawing.Point(14, 44);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(404, 37);
            this.label2.TabIndex = 2;
            this.label2.Text = "Router address:     127.0.0.";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // routerAddress
            // 
            this.routerAddress.Location = new System.Drawing.Point(426, 42);
            this.routerAddress.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.routerAddress.Name = "routerAddress";
            this.routerAddress.Size = new System.Drawing.Size(62, 31);
            this.routerAddress.TabIndex = 3;
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(520, 37);
            this.connectButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(386, 44);
            this.connectButton.TabIndex = 4;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label4.Location = new System.Drawing.Point(8, 90);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(132, 37);
            this.label4.TabIndex = 6;
            this.label4.Text = "127.0.0.";
            // 
            // targetAddress
            // 
            this.targetAddress.Location = new System.Drawing.Point(152, 94);
            this.targetAddress.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.targetAddress.Name = "targetAddress";
            this.targetAddress.Size = new System.Drawing.Size(68, 31);
            this.targetAddress.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label5.Location = new System.Drawing.Point(66, 48);
            this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 31);
            this.label5.TabIndex = 8;
            this.label5.Text = "Target";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label6.Location = new System.Drawing.Point(284, 48);
            this.label6.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(124, 31);
            this.label6.TabIndex = 9;
            this.label6.Text = "Message";
            // 
            // message
            // 
            this.message.Location = new System.Drawing.Point(254, 94);
            this.message.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.message.Name = "message";
            this.message.Size = new System.Drawing.Size(276, 31);
            this.message.TabIndex = 10;
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(700, 48);
            this.sendButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(194, 81);
            this.sendButton.TabIndex = 11;
            this.sendButton.Text = "SEND";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.connectButton);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.routerAddress);
            this.groupBox1.Location = new System.Drawing.Point(32, 100);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBox1.Size = new System.Drawing.Size(918, 98);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connecting";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.connectionBox);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.band);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.sendButton);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.message);
            this.groupBox2.Controls.Add(this.targetAddress);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(32, 210);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBox2.Size = new System.Drawing.Size(918, 156);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Send a message";
            // 
            // connectionBox
            // 
            this.connectionBox.Enabled = false;
            this.connectionBox.Location = new System.Drawing.Point(548, 92);
            this.connectionBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.connectionBox.Name = "connectionBox";
            this.connectionBox.Size = new System.Drawing.Size(48, 31);
            this.connectionBox.TabIndex = 15;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(538, 48);
            this.label7.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(59, 25);
            this.label7.TabIndex = 14;
            this.label7.Text = "C_ID";
            // 
            // band
            // 
            this.band.Enabled = false;
            this.band.Location = new System.Drawing.Point(620, 94);
            this.band.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.band.Name = "band";
            this.band.Size = new System.Drawing.Size(54, 31);
            this.band.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(614, 48);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 25);
            this.label3.TabIndex = 12;
            this.label3.Text = "Band";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dataGridView1);
            this.groupBox3.Location = new System.Drawing.Point(32, 377);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBox3.Size = new System.Drawing.Size(918, 358);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "History Log";
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(16, 38);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(890, 308);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick_1);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(26, 25);
            this.button1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(230, 63);
            this.button1.TabIndex = 15;
            this.button1.Text = "Refresh";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(974, 758);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "ClientForm";
            this.Text = "Client";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox routerAddress;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox targetAddress;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox message;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TextBox band;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox connectionBox;
        private System.Windows.Forms.Label label7;
    }
}