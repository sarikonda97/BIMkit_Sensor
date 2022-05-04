
namespace MCGDApp
{
    partial class RuleGeneratorSettingsForm
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
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.comboBoxType1 = new System.Windows.Forms.ComboBox();
            this.comboBoxEC1 = new System.Windows.Forms.ComboBox();
            this.comboBoxEC2 = new System.Windows.Forms.ComboBox();
            this.comboBoxType2 = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGenerate.Location = new System.Drawing.Point(352, 12);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(116, 48);
            this.buttonGenerate.TabIndex = 0;
            this.buttonGenerate.Text = "Generate";
            this.buttonGenerate.UseVisualStyleBackColor = true;
            this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
            // 
            // comboBoxType1
            // 
            this.comboBoxType1.FormattingEnabled = true;
            this.comboBoxType1.Location = new System.Drawing.Point(156, 12);
            this.comboBoxType1.Name = "comboBoxType1";
            this.comboBoxType1.Size = new System.Drawing.Size(190, 21);
            this.comboBoxType1.TabIndex = 1;
            // 
            // comboBoxEC1
            // 
            this.comboBoxEC1.FormattingEnabled = true;
            this.comboBoxEC1.Location = new System.Drawing.Point(12, 12);
            this.comboBoxEC1.Name = "comboBoxEC1";
            this.comboBoxEC1.Size = new System.Drawing.Size(138, 21);
            this.comboBoxEC1.TabIndex = 2;
            // 
            // comboBoxEC2
            // 
            this.comboBoxEC2.FormattingEnabled = true;
            this.comboBoxEC2.Location = new System.Drawing.Point(12, 39);
            this.comboBoxEC2.Name = "comboBoxEC2";
            this.comboBoxEC2.Size = new System.Drawing.Size(138, 21);
            this.comboBoxEC2.TabIndex = 4;
            // 
            // comboBoxType2
            // 
            this.comboBoxType2.FormattingEnabled = true;
            this.comboBoxType2.Location = new System.Drawing.Point(156, 39);
            this.comboBoxType2.Name = "comboBoxType2";
            this.comboBoxType2.Size = new System.Drawing.Size(190, 21);
            this.comboBoxType2.TabIndex = 3;
            // 
            // RuleGeneratorSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 72);
            this.Controls.Add(this.comboBoxEC2);
            this.Controls.Add(this.comboBoxType2);
            this.Controls.Add(this.comboBoxEC1);
            this.Controls.Add(this.comboBoxType1);
            this.Controls.Add(this.buttonGenerate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RuleGeneratorSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "RuleGeneratorSettingsForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.ComboBox comboBoxType1;
        private System.Windows.Forms.ComboBox comboBoxEC1;
        private System.Windows.Forms.ComboBox comboBoxEC2;
        private System.Windows.Forms.ComboBox comboBoxType2;
    }
}