using System;

namespace Node
{
    partial class NodeForm
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
            try
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }
            catch (InvalidOperationException)
            {
                //todo this.Finish();
            }
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NodeForm));
            this.label1 = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.messageHistoryTable = new System.Windows.Forms.DataGridView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.consoleOutput = new System.Windows.Forms.TextBox();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tabs = new System.Windows.Forms.TabControl();
            this.generalTab = new System.Windows.Forms.TabPage();
            this.RunButton = new System.Windows.Forms.CheckBox();
            this.SendButton = new System.Windows.Forms.Button();
            this.MsgLabel = new System.Windows.Forms.Label();
            this.PortIdLabel = new System.Windows.Forms.Label();
            this.PortTextBox = new System.Windows.Forms.TextBox();
            this.MsgTextBox = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.advancedTab = new System.Windows.Forms.TabPage();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.frequencySlotsTable = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.messageHistoryTable)).BeginInit();
            this.tabs.SuspendLayout();
            this.generalTab.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.advancedTab.SuspendLayout();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.frequencySlotsTable)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 37);
            this.label1.TabIndex = 0;
            this.label1.Text = "Node ID";
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelName.Location = new System.Drawing.Point(152, 19);
            this.labelName.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(33, 37);
            this.labelName.TabIndex = 4;
            this.labelName.Text = "1";
            // 
            // messageHistoryTable
            // 
            this.messageHistoryTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.messageHistoryTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.messageHistoryTable.Location = new System.Drawing.Point(9, 33);
            this.messageHistoryTable.Margin = new System.Windows.Forms.Padding(6);
            this.messageHistoryTable.Name = "messageHistoryTable";
            this.messageHistoryTable.RowHeadersVisible = false;
            this.messageHistoryTable.Size = new System.Drawing.Size(537, 312);
            this.messageHistoryTable.TabIndex = 5;
            this.messageHistoryTable.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.connectedWiresTable_CellContentClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // consoleOutput
            // 
            this.consoleOutput.Location = new System.Drawing.Point(14, 30);
            this.consoleOutput.Multiline = true;
            this.consoleOutput.Name = "consoleOutput";
            this.consoleOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.consoleOutput.Size = new System.Drawing.Size(532, 343);
            this.consoleOutput.TabIndex = 8;
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(61, 4);
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.generalTab);
            this.tabs.Controls.Add(this.advancedTab);
            this.tabs.Location = new System.Drawing.Point(2, 0);
            this.tabs.Name = "tabs";
            this.tabs.Padding = new System.Drawing.Point(0, 0);
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(594, 963);
            this.tabs.TabIndex = 9;
            // 
            // generalTab
            // 
            this.generalTab.Controls.Add(this.RunButton);
            this.generalTab.Controls.Add(this.SendButton);
            this.generalTab.Controls.Add(this.MsgLabel);
            this.generalTab.Controls.Add(this.PortIdLabel);
            this.generalTab.Controls.Add(this.PortTextBox);
            this.generalTab.Controls.Add(this.MsgTextBox);
            this.generalTab.Controls.Add(this.groupBox2);
            this.generalTab.Controls.Add(this.groupBox1);
            this.generalTab.Controls.Add(this.label1);
            this.generalTab.Controls.Add(this.labelName);
            this.generalTab.Location = new System.Drawing.Point(4, 34);
            this.generalTab.Name = "generalTab";
            this.generalTab.Padding = new System.Windows.Forms.Padding(3);
            this.generalTab.Size = new System.Drawing.Size(586, 925);
            this.generalTab.TabIndex = 0;
            this.generalTab.Text = "General";
            this.generalTab.UseVisualStyleBackColor = true;
            // 
            // RunButton
            // 
            this.RunButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.RunButton.AutoSize = true;
            this.RunButton.Location = new System.Drawing.Point(442, 18);
            this.RunButton.Name = "RunButton";
            this.RunButton.Size = new System.Drawing.Size(110, 35);
            this.RunButton.TabIndex = 10;
            this.RunButton.Text = "DISABLE";
            this.RunButton.UseVisualStyleBackColor = true;
            this.RunButton.CheckedChanged += new System.EventHandler(this.RunButton_CheckedChanged);
            // 
            // SendButton
            // 
            this.SendButton.Location = new System.Drawing.Point(382, 824);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(190, 80);
            this.SendButton.TabIndex = 17;
            this.SendButton.Text = "Send";
            this.SendButton.UseVisualStyleBackColor = true;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // MsgLabel
            // 
            this.MsgLabel.AutoSize = true;
            this.MsgLabel.Location = new System.Drawing.Point(15, 827);
            this.MsgLabel.Name = "MsgLabel";
            this.MsgLabel.Size = new System.Drawing.Size(100, 25);
            this.MsgLabel.TabIndex = 16;
            this.MsgLabel.Text = "Message";
            // 
            // PortIdLabel
            // 
            this.PortIdLabel.AutoSize = true;
            this.PortIdLabel.Location = new System.Drawing.Point(41, 873);
            this.PortIdLabel.Name = "PortIdLabel";
            this.PortIdLabel.Size = new System.Drawing.Size(74, 25);
            this.PortIdLabel.TabIndex = 14;
            this.PortIdLabel.Text = "Target";
            // 
            // PortTextBox
            // 
            this.PortTextBox.Location = new System.Drawing.Point(121, 873);
            this.PortTextBox.Name = "PortTextBox";
            this.PortTextBox.Size = new System.Drawing.Size(64, 31);
            this.PortTextBox.TabIndex = 13;
            // 
            // MsgTextBox
            // 
            this.MsgTextBox.Location = new System.Drawing.Point(121, 824);
            this.MsgTextBox.Name = "MsgTextBox";
            this.MsgTextBox.Size = new System.Drawing.Size(255, 31);
            this.MsgTextBox.TabIndex = 11;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.messageHistoryTable);
            this.groupBox2.Location = new System.Drawing.Point(6, 59);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(566, 354);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Messages history";
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.consoleOutput);
            this.groupBox1.Location = new System.Drawing.Point(6, 419);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(566, 391);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Console output";
            // 
            // advancedTab
            // 
            this.advancedTab.Controls.Add(this.groupBox7);
            this.advancedTab.Location = new System.Drawing.Point(4, 34);
            this.advancedTab.Name = "advancedTab";
            this.advancedTab.Padding = new System.Windows.Forms.Padding(3);
            this.advancedTab.Size = new System.Drawing.Size(586, 925);
            this.advancedTab.TabIndex = 1;
            this.advancedTab.Text = "Advanced";
            this.advancedTab.UseVisualStyleBackColor = true;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.frequencySlotsTable);
            this.groupBox7.Location = new System.Drawing.Point(6, 6);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(574, 344);
            this.groupBox7.TabIndex = 6;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Frequency Slots";
            // 
            // frequencySlotsTable
            // 
            this.frequencySlotsTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.frequencySlotsTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.frequencySlotsTable.Location = new System.Drawing.Point(9, 30);
            this.frequencySlotsTable.Name = "frequencySlotsTable";
            this.frequencySlotsTable.RowHeadersVisible = false;
            this.frequencySlotsTable.RowTemplate.Height = 33;
            this.frequencySlotsTable.Size = new System.Drawing.Size(559, 300);
            this.frequencySlotsTable.TabIndex = 0;
            // 
            // NodeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(596, 969);
            this.Controls.Add(this.tabs);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.Name = "NodeForm";
            this.Text = "Node";
            this.Load += new System.EventHandler(this.RouterForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.messageHistoryTable)).EndInit();
            this.tabs.ResumeLayout(false);
            this.generalTab.ResumeLayout(false);
            this.generalTab.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.advancedTab.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.frequencySlotsTable)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.DataGridView messageHistoryTable;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.TextBox consoleOutput;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage generalTab;
        private System.Windows.Forms.TabPage advancedTab;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.DataGridView frequencySlotsTable;
        private System.Windows.Forms.Button SendButton;
        private System.Windows.Forms.Label MsgLabel;
        private System.Windows.Forms.Label PortIdLabel;
        private System.Windows.Forms.TextBox PortTextBox;
        private System.Windows.Forms.TextBox MsgTextBox;
        private System.Windows.Forms.CheckBox RunButton;
    }
}