namespace ExecomExtensions.Options
{
    partial class AddHeaderOptionsUserControl
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
            this.label1 = new System.Windows.Forms.Label();
            this.patternsLabel = new System.Windows.Forms.Label();
            this.headerTemplateTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Add header template:";
            // 
            // patternsLabel
            // 
            this.patternsLabel.AutoSize = true;
            this.patternsLabel.Location = new System.Drawing.Point(3, 185);
            this.patternsLabel.Name = "patternsLabel";
            this.patternsLabel.Size = new System.Drawing.Size(0, 13);
            this.patternsLabel.TabIndex = 2;
            // 
            // headerTemplateTextBox
            // 
            this.headerTemplateTextBox.AcceptsTab = true;
            this.headerTemplateTextBox.Location = new System.Drawing.Point(11, 32);
            this.headerTemplateTextBox.Name = "headerTemplateTextBox";
            this.headerTemplateTextBox.Size = new System.Drawing.Size(350, 150);
            this.headerTemplateTextBox.TabIndex = 3;
            this.headerTemplateTextBox.Text = "";
            this.headerTemplateTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.headerTemplateTextBox_KeyDown);
            this.headerTemplateTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.headerTemplateTextBox_KeyUp);
            // 
            // AddHeaderOptionsUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.headerTemplateTextBox);
            this.Controls.Add(this.patternsLabel);
            this.Controls.Add(this.label1);
            this.Name = "AddHeaderOptionsUserControl";
            this.Size = new System.Drawing.Size(424, 345);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label patternsLabel;
        private System.Windows.Forms.RichTextBox headerTemplateTextBox;
    }
}
