using System;

namespace Router
{
    partial class RouterForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.connectedWiresTable = new System.Windows.Forms.DataGridView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.consoleOutput = new System.Windows.Forms.TextBox();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tabs = new System.Windows.Forms.TabControl();
            this.generalTab = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.advancedTab = new System.Windows.Forms.TabPage();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.frequencySlotsTable = new System.Windows.Forms.DataGridView();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.fromClientTable = new System.Windows.Forms.DataGridView();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.messagesTable = new System.Windows.Forms.DataGridView();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.toClientTable = new System.Windows.Forms.DataGridView();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.clientTable = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.connectedWiresTable)).BeginInit();
            this.tabs.SuspendLayout();
            this.generalTab.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.advancedTab.SuspendLayout();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.frequencySlotsTable)).BeginInit();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fromClientTable)).BeginInit();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.messagesTable)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.toClientTable)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.clientTable)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(290, 63);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP Address";
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelName.Location = new System.Drawing.Point(417, 19);
            this.labelName.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(387, 63);
            this.labelName.TabIndex = 4;
            this.labelName.Text = "127.0.0.1:8000";
            // 
            // connectedWiresTable
            // 
            this.connectedWiresTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.connectedWiresTable.Location = new System.Drawing.Point(9, 33);
            this.connectedWiresTable.Margin = new System.Windows.Forms.Padding(6);
            this.connectedWiresTable.Name = "connectedWiresTable";
            this.connectedWiresTable.Size = new System.Drawing.Size(780, 244);
            this.connectedWiresTable.TabIndex = 5;
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
            this.consoleOutput.Size = new System.Drawing.Size(778, 487);
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
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(958, 1538);
            this.tabs.TabIndex = 9;
            // 
            // generalTab
            // 
            this.generalTab.Controls.Add(this.groupBox2);
            this.generalTab.Controls.Add(this.groupBox1);
            this.generalTab.Controls.Add(this.label1);
            this.generalTab.Controls.Add(this.labelName);
            this.generalTab.Location = new System.Drawing.Point(4, 34);
            this.generalTab.Name = "generalTab";
            this.generalTab.Padding = new System.Windows.Forms.Padding(3);
            this.generalTab.Size = new System.Drawing.Size(950, 1500);
            this.generalTab.TabIndex = 0;
            this.generalTab.Text = "General";
            this.generalTab.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.connectedWiresTable);
            this.groupBox2.Location = new System.Drawing.Point(6, 85);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(798, 286);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Connected wires";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.consoleOutput);
            this.groupBox1.Location = new System.Drawing.Point(3, 377);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(801, 523);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Console output";
            // 
            // advancedTab
            // 
            this.advancedTab.Controls.Add(this.groupBox7);
            this.advancedTab.Controls.Add(this.groupBox6);
            this.advancedTab.Controls.Add(this.groupBox5);
            this.advancedTab.Controls.Add(this.groupBox4);
            this.advancedTab.Controls.Add(this.groupBox3);
            this.advancedTab.Location = new System.Drawing.Point(4, 34);
            this.advancedTab.Name = "advancedTab";
            this.advancedTab.Padding = new System.Windows.Forms.Padding(3);
            this.advancedTab.Size = new System.Drawing.Size(950, 1500);
            this.advancedTab.TabIndex = 1;
            this.advancedTab.Text = "Advanced";
            this.advancedTab.UseVisualStyleBackColor = true;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.frequencySlotsTable);
            this.groupBox7.Location = new System.Drawing.Point(6, 1133);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(922, 344);
            this.groupBox7.TabIndex = 6;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Frequency Slots";
            // 
            // frequencySlotsTable
            // 
            this.frequencySlotsTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.frequencySlotsTable.Location = new System.Drawing.Point(9, 30);
            this.frequencySlotsTable.Name = "frequencySlotsTable";
            this.frequencySlotsTable.RowTemplate.Height = 33;
            this.frequencySlotsTable.Size = new System.Drawing.Size(903, 300);
            this.frequencySlotsTable.TabIndex = 0;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.fromClientTable);
            this.groupBox6.Location = new System.Drawing.Point(6, 566);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(922, 274);
            this.groupBox6.TabIndex = 5;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "From Client";
            // 
            // fromClientTable
            // 
            this.fromClientTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.fromClientTable.Location = new System.Drawing.Point(6, 30);
            this.fromClientTable.Name = "fromClientTable";
            this.fromClientTable.RowTemplate.Height = 33;
            this.fromClientTable.Size = new System.Drawing.Size(906, 238);
            this.fromClientTable.TabIndex = 1;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.messagesTable);
            this.groupBox5.Location = new System.Drawing.Point(6, 853);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(922, 274);
            this.groupBox5.TabIndex = 4;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Waiting messages";
            // 
            // messagesTable
            // 
            this.messagesTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.messagesTable.Location = new System.Drawing.Point(6, 30);
            this.messagesTable.Name = "messagesTable";
            this.messagesTable.RowTemplate.Height = 33;
            this.messagesTable.Size = new System.Drawing.Size(906, 238);
            this.messagesTable.TabIndex = 1;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.toClientTable);
            this.groupBox4.Location = new System.Drawing.Point(6, 286);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(922, 274);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "To Client";
            // 
            // toClientTable
            // 
            this.toClientTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.toClientTable.Location = new System.Drawing.Point(9, 30);
            this.toClientTable.Name = "toClientTable";
            this.toClientTable.RowTemplate.Height = 33;
            this.toClientTable.Size = new System.Drawing.Size(903, 238);
            this.toClientTable.TabIndex = 1;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.clientTable);
            this.groupBox3.Location = new System.Drawing.Point(6, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(922, 274);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Client list";
            // 
            // clientTable
            // 
            this.clientTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.clientTable.Location = new System.Drawing.Point(6, 30);
            this.clientTable.Name = "clientTable";
            this.clientTable.RowTemplate.Height = 33;
            this.clientTable.Size = new System.Drawing.Size(906, 238);
            this.clientTable.TabIndex = 0;
            // 
            // RouterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 1538);
            this.Controls.Add(this.tabs);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "RouterForm";
            this.Text = "Router";
            ((System.ComponentModel.ISupportInitialize)(this.connectedWiresTable)).EndInit();
            this.tabs.ResumeLayout(false);
            this.generalTab.ResumeLayout(false);
            this.generalTab.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.advancedTab.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.frequencySlotsTable)).EndInit();
            this.groupBox6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fromClientTable)).EndInit();
            this.groupBox5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.messagesTable)).EndInit();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.toClientTable)).EndInit();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.clientTable)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.DataGridView connectedWiresTable;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.TextBox consoleOutput;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage generalTab;
        private System.Windows.Forms.TabPage advancedTab;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.DataGridView frequencySlotsTable;
        private System.Windows.Forms.DataGridView fromClientTable;
        private System.Windows.Forms.DataGridView messagesTable;
        private System.Windows.Forms.DataGridView toClientTable;
        private System.Windows.Forms.DataGridView clientTable;
    }
}