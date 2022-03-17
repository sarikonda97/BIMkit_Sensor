
namespace MCGDApp
{
    partial class GenerativeDesignSettingsForm
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
            this.checkBoxShowRoute = new System.Windows.Forms.CheckBox();
            this.textBoxMoves = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxRate = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxMovement = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxIterations = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonDone = new System.Windows.Forms.Button();
            this.checkBoxFixedItterations = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkBoxShowRoute
            // 
            this.checkBoxShowRoute.AutoSize = true;
            this.checkBoxShowRoute.Location = new System.Drawing.Point(15, 139);
            this.checkBoxShowRoute.Name = "checkBoxShowRoute";
            this.checkBoxShowRoute.Size = new System.Drawing.Size(85, 17);
            this.checkBoxShowRoute.TabIndex = 30;
            this.checkBoxShowRoute.Text = "Show Route";
            this.checkBoxShowRoute.UseVisualStyleBackColor = true;
            // 
            // textBoxMoves
            // 
            this.textBoxMoves.Location = new System.Drawing.Point(71, 90);
            this.textBoxMoves.Name = "textBoxMoves";
            this.textBoxMoves.Size = new System.Drawing.Size(38, 20);
            this.textBoxMoves.TabIndex = 29;
            this.textBoxMoves.Text = "4";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(26, 93);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(39, 13);
            this.label7.TabIndex = 28;
            this.label7.Text = "Moves";
            // 
            // textBoxRate
            // 
            this.textBoxRate.Location = new System.Drawing.Point(71, 64);
            this.textBoxRate.Name = "textBoxRate";
            this.textBoxRate.Size = new System.Drawing.Size(38, 20);
            this.textBoxRate.TabIndex = 27;
            this.textBoxRate.Text = "0.5";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(35, 67);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 13);
            this.label6.TabIndex = 26;
            this.label6.Text = "Rate";
            // 
            // textBoxMovement
            // 
            this.textBoxMovement.Location = new System.Drawing.Point(71, 38);
            this.textBoxMovement.Name = "textBoxMovement";
            this.textBoxMovement.Size = new System.Drawing.Size(38, 20);
            this.textBoxMovement.TabIndex = 25;
            this.textBoxMovement.Text = "20";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 41);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 13);
            this.label5.TabIndex = 24;
            this.label5.Text = "Movement";
            // 
            // textBoxIterations
            // 
            this.textBoxIterations.Location = new System.Drawing.Point(71, 12);
            this.textBoxIterations.Name = "textBoxIterations";
            this.textBoxIterations.Size = new System.Drawing.Size(38, 20);
            this.textBoxIterations.TabIndex = 23;
            this.textBoxIterations.Text = "100";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "Itterations";
            // 
            // buttonDone
            // 
            this.buttonDone.Location = new System.Drawing.Point(15, 162);
            this.buttonDone.Name = "buttonDone";
            this.buttonDone.Size = new System.Drawing.Size(94, 23);
            this.buttonDone.TabIndex = 31;
            this.buttonDone.Text = "Done";
            this.buttonDone.UseVisualStyleBackColor = true;
            this.buttonDone.Click += new System.EventHandler(this.buttonDone_Click);
            // 
            // checkBoxFixedItterations
            // 
            this.checkBoxFixedItterations.AutoSize = true;
            this.checkBoxFixedItterations.Location = new System.Drawing.Point(15, 116);
            this.checkBoxFixedItterations.Name = "checkBoxFixedItterations";
            this.checkBoxFixedItterations.Size = new System.Drawing.Size(100, 17);
            this.checkBoxFixedItterations.TabIndex = 32;
            this.checkBoxFixedItterations.Text = "Fixed Itterations";
            this.checkBoxFixedItterations.UseVisualStyleBackColor = true;
            // 
            // GenerativeDesignSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(125, 200);
            this.Controls.Add(this.checkBoxFixedItterations);
            this.Controls.Add(this.buttonDone);
            this.Controls.Add(this.checkBoxShowRoute);
            this.Controls.Add(this.textBoxMoves);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxRate);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBoxMovement);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxIterations);
            this.Controls.Add(this.label4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GenerativeDesignSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GD Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxShowRoute;
        private System.Windows.Forms.TextBox textBoxMoves;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxRate;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxMovement;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxIterations;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonDone;
        private System.Windows.Forms.CheckBox checkBoxFixedItterations;
    }
}