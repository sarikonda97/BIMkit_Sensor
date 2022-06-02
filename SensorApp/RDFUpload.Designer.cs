
namespace SensorApp
{
    partial class RDFUpload
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
            this.browseButton = new System.Windows.Forms.Button();
            this.fileName = new System.Windows.Forms.TextBox();
            this.uploadButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.loadNodesButton = new System.Windows.Forms.Button();
            this.loadRelationshipsButton = new System.Windows.Forms.Button();
            this.subjectDropDown = new System.Windows.Forms.ComboBox();
            this.objectDropDown = new System.Windows.Forms.ComboBox();
            this.predicateOutput = new System.Windows.Forms.RichTextBox();
            this.getValidButton = new System.Windows.Forms.Button();
            this.populateDropDownButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(695, 24);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(75, 23);
            this.browseButton.TabIndex = 0;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // fileName
            // 
            this.fileName.Location = new System.Drawing.Point(26, 27);
            this.fileName.Name = "fileName";
            this.fileName.Size = new System.Drawing.Size(639, 20);
            this.fileName.TabIndex = 1;
            // 
            // uploadButton
            // 
            this.uploadButton.Location = new System.Drawing.Point(349, 53);
            this.uploadButton.Name = "uploadButton";
            this.uploadButton.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.uploadButton.Size = new System.Drawing.Size(75, 23);
            this.uploadButton.TabIndex = 3;
            this.uploadButton.Text = "Upload";
            this.uploadButton.UseVisualStyleBackColor = true;
            this.uploadButton.Click += new System.EventHandler(this.uploadButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // loadNodesButton
            // 
            this.loadNodesButton.Location = new System.Drawing.Point(26, 97);
            this.loadNodesButton.Name = "loadNodesButton";
            this.loadNodesButton.Size = new System.Drawing.Size(75, 42);
            this.loadNodesButton.TabIndex = 4;
            this.loadNodesButton.Text = "Load Nodes";
            this.loadNodesButton.UseVisualStyleBackColor = true;
            this.loadNodesButton.Click += new System.EventHandler(this.loadNodesButton_Click);
            // 
            // loadRelationshipsButton
            // 
            this.loadRelationshipsButton.Location = new System.Drawing.Point(143, 97);
            this.loadRelationshipsButton.Name = "loadRelationshipsButton";
            this.loadRelationshipsButton.Size = new System.Drawing.Size(88, 43);
            this.loadRelationshipsButton.TabIndex = 5;
            this.loadRelationshipsButton.Text = "Load Relationships";
            this.loadRelationshipsButton.UseVisualStyleBackColor = true;
            this.loadRelationshipsButton.Click += new System.EventHandler(this.loadRelationshipsButton_Click);
            // 
            // subjectDropDown
            // 
            this.subjectDropDown.FormattingEnabled = true;
            this.subjectDropDown.Location = new System.Drawing.Point(397, 96);
            this.subjectDropDown.Name = "subjectDropDown";
            this.subjectDropDown.Size = new System.Drawing.Size(121, 21);
            this.subjectDropDown.TabIndex = 6;
            this.subjectDropDown.SelectedIndexChanged += new System.EventHandler(this.subjectDropDown_SelectedIndexChanged);
            // 
            // objectDropDown
            // 
            this.objectDropDown.FormattingEnabled = true;
            this.objectDropDown.Location = new System.Drawing.Point(397, 123);
            this.objectDropDown.Name = "objectDropDown";
            this.objectDropDown.Size = new System.Drawing.Size(121, 21);
            this.objectDropDown.TabIndex = 7;
            this.objectDropDown.SelectedIndexChanged += new System.EventHandler(this.objectDropDown_SelectedIndexChanged);
            // 
            // predicateOutput
            // 
            this.predicateOutput.Location = new System.Drawing.Point(670, 71);
            this.predicateOutput.Name = "predicateOutput";
            this.predicateOutput.Size = new System.Drawing.Size(100, 96);
            this.predicateOutput.TabIndex = 8;
            this.predicateOutput.Text = "";
            this.predicateOutput.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // getValidButton
            // 
            this.getValidButton.Location = new System.Drawing.Point(559, 96);
            this.getValidButton.Name = "getValidButton";
            this.getValidButton.Size = new System.Drawing.Size(91, 42);
            this.getValidButton.TabIndex = 9;
            this.getValidButton.Text = "Populate Valid Relationships";
            this.getValidButton.UseVisualStyleBackColor = true;
            this.getValidButton.Click += new System.EventHandler(this.getValidButton_Click);
            // 
            // populateDropDownButton
            // 
            this.populateDropDownButton.Location = new System.Drawing.Point(272, 97);
            this.populateDropDownButton.Name = "populateDropDownButton";
            this.populateDropDownButton.Size = new System.Drawing.Size(102, 41);
            this.populateDropDownButton.TabIndex = 10;
            this.populateDropDownButton.Text = "Populate Subjects and Objects";
            this.populateDropDownButton.UseVisualStyleBackColor = true;
            this.populateDropDownButton.Click += new System.EventHandler(this.populateDropDownButton_Click);
            // 
            // RDFUpload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 190);
            this.Controls.Add(this.populateDropDownButton);
            this.Controls.Add(this.getValidButton);
            this.Controls.Add(this.predicateOutput);
            this.Controls.Add(this.objectDropDown);
            this.Controls.Add(this.subjectDropDown);
            this.Controls.Add(this.loadRelationshipsButton);
            this.Controls.Add(this.loadNodesButton);
            this.Controls.Add(this.uploadButton);
            this.Controls.Add(this.fileName);
            this.Controls.Add(this.browseButton);
            this.Name = "RDFUpload";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox fileName;
        private System.Windows.Forms.Button uploadButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button loadNodesButton;
        private System.Windows.Forms.Button loadRelationshipsButton;
        private System.Windows.Forms.ComboBox subjectDropDown;
        private System.Windows.Forms.ComboBox objectDropDown;
        private System.Windows.Forms.RichTextBox predicateOutput;
        private System.Windows.Forms.Button getValidButton;
        private System.Windows.Forms.Button populateDropDownButton;
    }
}

