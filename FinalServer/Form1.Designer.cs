namespace Cloud
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            //this.serverBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.consoleOutput = new System.Windows.Forms.TextBox();
            //((System.ComponentModel.ISupportInitialize)(this.serverBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(366, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 63);
            this.label1.TabIndex = 0;
            this.label1.Text = "Cloud";
            // 
            // serverBindingSource
            // 
            //this.serverBindingSource.DataSource = typeof(Cloud.Cloud);
            // 
            // consoleOutput
            // 
            this.consoleOutput.Location = new System.Drawing.Point(13, 84);
            this.consoleOutput.Multiline = true;
            this.consoleOutput.Name = "consoleOutput";
            this.consoleOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.consoleOutput.Size = new System.Drawing.Size(937, 752);
            this.consoleOutput.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(962, 848);
            this.Controls.Add(this.consoleOutput);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "Form1";
            this.Text = "Cloud";
            //this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            //((System.ComponentModel.ISupportInitialize)(this.serverBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.BindingSource serverBindingSource;
        private System.Windows.Forms.TextBox consoleOutput;
    }
}