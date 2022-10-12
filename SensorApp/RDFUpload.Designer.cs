
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
            this.loadNodesButton = new System.Windows.Forms.Button();
            this.loadRelationshipsButton = new System.Windows.Forms.Button();
            this.subjectDropDown = new System.Windows.Forms.ComboBox();
            this.objectDropDown = new System.Windows.Forms.ComboBox();
            this.predicateOutput = new System.Windows.Forms.RichTextBox();
            this.getValidButton = new System.Windows.Forms.Button();
            this.populateDropDownButton = new System.Windows.Forms.Button();
            this.loadInstancesButton = new System.Windows.Forms.Button();
            this.browseModelFile = new System.Windows.Forms.Button();
            this.modelName = new System.Windows.Forms.TextBox();
            this.loadModelButton = new System.Windows.Forms.Button();
            this.mapTurtleToModelButton = new System.Windows.Forms.Button();
            this.uniqueInstancesComboBox = new System.Windows.Forms.ComboBox();
            this.relatedDevicesButton = new System.Windows.Forms.Button();
            this.populateInstancesButton = new System.Windows.Forms.Button();
            this.possibleRelationshipsTextbox = new System.Windows.Forms.RichTextBox();
            this.getAvailableModelsBox = new System.Windows.Forms.Button();
            this.selectModelCombobox = new System.Windows.Forms.ComboBox();
            this.firstDeviceComboBox = new System.Windows.Forms.ComboBox();
            this.secondDeviceComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.areDevicesRelatedButton = new System.Windows.Forms.Button();
            this.deviceRelationStatusTextBox = new System.Windows.Forms.RichTextBox();
            this.loadRoomsButton = new System.Windows.Forms.Button();
            this.firstRoomComboBox = new System.Windows.Forms.ComboBox();
            this.secondRoomComboBox = new System.Windows.Forms.ComboBox();
            this.getRoomRelatedDevicesButton = new System.Windows.Forms.Button();
            this.roomRelatedDevicesTextBox = new System.Windows.Forms.RichTextBox();
            this.mapBpmToTtl = new System.Windows.Forms.Button();
            this.changeTurtleRoomsButton = new System.Windows.Forms.Button();
            this.directRelationshipButton = new System.Windows.Forms.Button();
            this.directRelationTextBox = new System.Windows.Forms.RichTextBox();
            this.directRelatedDevicesAndRelationships = new System.Windows.Forms.Button();
            this.directRelationshipWithPredicateTextBox = new System.Windows.Forms.RichTextBox();
            this.getRelationshipPathButton = new System.Windows.Forms.Button();
            this.relationshipPathTextBox = new System.Windows.Forms.RichTextBox();
            this.roomRelatedDevicesButton = new System.Windows.Forms.Button();
            this.roomRelatedDevTextBox = new System.Windows.Forms.RichTextBox();
            this.getRelPathWithRelButton = new System.Windows.Forms.Button();
            this.relPathWithRelTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(695, 12);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(75, 41);
            this.browseButton.TabIndex = 0;
            this.browseButton.Text = "Browse Turtle File";
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
            this.uploadButton.Size = new System.Drawing.Size(87, 23);
            this.uploadButton.TabIndex = 3;
            this.uploadButton.Text = "Load Brick File";
            this.uploadButton.UseVisualStyleBackColor = true;
            this.uploadButton.Click += new System.EventHandler(this.uploadButton_Click);
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
            // loadInstancesButton
            // 
            this.loadInstancesButton.Location = new System.Drawing.Point(119, 305);
            this.loadInstancesButton.Name = "loadInstancesButton";
            this.loadInstancesButton.Size = new System.Drawing.Size(86, 43);
            this.loadInstancesButton.TabIndex = 11;
            this.loadInstancesButton.Text = "Load Instances";
            this.loadInstancesButton.UseVisualStyleBackColor = true;
            this.loadInstancesButton.Click += new System.EventHandler(this.loadInstancesButton_Click);
            // 
            // browseModelFile
            // 
            this.browseModelFile.Location = new System.Drawing.Point(695, 201);
            this.browseModelFile.Name = "browseModelFile";
            this.browseModelFile.Size = new System.Drawing.Size(75, 36);
            this.browseModelFile.TabIndex = 12;
            this.browseModelFile.Text = "Browse Model";
            this.browseModelFile.UseVisualStyleBackColor = true;
            this.browseModelFile.Click += new System.EventHandler(this.browseModelFile_Click);
            // 
            // modelName
            // 
            this.modelName.Location = new System.Drawing.Point(26, 210);
            this.modelName.Name = "modelName";
            this.modelName.Size = new System.Drawing.Size(639, 20);
            this.modelName.TabIndex = 13;
            // 
            // loadModelButton
            // 
            this.loadModelButton.Location = new System.Drawing.Point(349, 236);
            this.loadModelButton.Name = "loadModelButton";
            this.loadModelButton.Size = new System.Drawing.Size(99, 23);
            this.loadModelButton.TabIndex = 14;
            this.loadModelButton.Text = "Load Model";
            this.loadModelButton.UseVisualStyleBackColor = true;
            this.loadModelButton.Click += new System.EventHandler(this.loadModelButton_Click);
            // 
            // mapTurtleToModelButton
            // 
            this.mapTurtleToModelButton.Location = new System.Drawing.Point(29, 250);
            this.mapTurtleToModelButton.Name = "mapTurtleToModelButton";
            this.mapTurtleToModelButton.Size = new System.Drawing.Size(84, 47);
            this.mapTurtleToModelButton.TabIndex = 15;
            this.mapTurtleToModelButton.Text = "Populate Room Relationships";
            this.mapTurtleToModelButton.UseVisualStyleBackColor = true;
            this.mapTurtleToModelButton.Click += new System.EventHandler(this.mapTurtleToModelButton_Click);
            // 
            // uniqueInstancesComboBox
            // 
            this.uniqueInstancesComboBox.FormattingEnabled = true;
            this.uniqueInstancesComboBox.Location = new System.Drawing.Point(502, 291);
            this.uniqueInstancesComboBox.Name = "uniqueInstancesComboBox";
            this.uniqueInstancesComboBox.Size = new System.Drawing.Size(162, 21);
            this.uniqueInstancesComboBox.TabIndex = 16;
            this.uniqueInstancesComboBox.SelectedIndexChanged += new System.EventHandler(this.uniqueInstancesComboBox_SelectedIndexChanged);
            // 
            // relatedDevicesButton
            // 
            this.relatedDevicesButton.Location = new System.Drawing.Point(518, 318);
            this.relatedDevicesButton.Name = "relatedDevicesButton";
            this.relatedDevicesButton.Size = new System.Drawing.Size(121, 23);
            this.relatedDevicesButton.TabIndex = 17;
            this.relatedDevicesButton.Text = "Get Related Devices";
            this.relatedDevicesButton.UseVisualStyleBackColor = true;
            this.relatedDevicesButton.Click += new System.EventHandler(this.relatedDevicesButton_Click);
            // 
            // populateInstancesButton
            // 
            this.populateInstancesButton.Location = new System.Drawing.Point(421, 291);
            this.populateInstancesButton.Name = "populateInstancesButton";
            this.populateInstancesButton.Size = new System.Drawing.Size(75, 43);
            this.populateInstancesButton.TabIndex = 18;
            this.populateInstancesButton.Text = "Populate Instances";
            this.populateInstancesButton.UseVisualStyleBackColor = true;
            this.populateInstancesButton.Click += new System.EventHandler(this.populateInstancesButton_Click);
            // 
            // possibleRelationshipsTextbox
            // 
            this.possibleRelationshipsTextbox.Location = new System.Drawing.Point(670, 271);
            this.possibleRelationshipsTextbox.Name = "possibleRelationshipsTextbox";
            this.possibleRelationshipsTextbox.Size = new System.Drawing.Size(100, 96);
            this.possibleRelationshipsTextbox.TabIndex = 19;
            this.possibleRelationshipsTextbox.Text = "";
            // 
            // getAvailableModelsBox
            // 
            this.getAvailableModelsBox.Location = new System.Drawing.Point(211, 287);
            this.getAvailableModelsBox.Name = "getAvailableModelsBox";
            this.getAvailableModelsBox.Size = new System.Drawing.Size(75, 50);
            this.getAvailableModelsBox.TabIndex = 20;
            this.getAvailableModelsBox.Text = "Get Available Models";
            this.getAvailableModelsBox.UseVisualStyleBackColor = true;
            this.getAvailableModelsBox.Click += new System.EventHandler(this.getAvailableModelsBox_Click);
            // 
            // selectModelCombobox
            // 
            this.selectModelCombobox.FormattingEnabled = true;
            this.selectModelCombobox.Location = new System.Drawing.Point(292, 303);
            this.selectModelCombobox.Name = "selectModelCombobox";
            this.selectModelCombobox.Size = new System.Drawing.Size(121, 21);
            this.selectModelCombobox.TabIndex = 21;
            this.selectModelCombobox.SelectedIndexChanged += new System.EventHandler(this.selectModelCombobox_SelectedIndexChanged);
            // 
            // firstDeviceComboBox
            // 
            this.firstDeviceComboBox.FormattingEnabled = true;
            this.firstDeviceComboBox.Location = new System.Drawing.Point(26, 389);
            this.firstDeviceComboBox.Name = "firstDeviceComboBox";
            this.firstDeviceComboBox.Size = new System.Drawing.Size(121, 21);
            this.firstDeviceComboBox.TabIndex = 22;
            this.firstDeviceComboBox.SelectedIndexChanged += new System.EventHandler(this.firstDeviceComboBox_SelectedIndexChanged);
            // 
            // secondDeviceComboBox
            // 
            this.secondDeviceComboBox.FormattingEnabled = true;
            this.secondDeviceComboBox.Location = new System.Drawing.Point(26, 440);
            this.secondDeviceComboBox.Name = "secondDeviceComboBox";
            this.secondDeviceComboBox.Size = new System.Drawing.Size(121, 21);
            this.secondDeviceComboBox.TabIndex = 23;
            this.secondDeviceComboBox.SelectedIndexChanged += new System.EventHandler(this.secondDeviceComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 370);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 24;
            this.label1.Text = "First Device";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 421);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 25;
            this.label2.Text = "Second Device";
            // 
            // areDevicesRelatedButton
            // 
            this.areDevicesRelatedButton.Location = new System.Drawing.Point(175, 381);
            this.areDevicesRelatedButton.Name = "areDevicesRelatedButton";
            this.areDevicesRelatedButton.Size = new System.Drawing.Size(75, 35);
            this.areDevicesRelatedButton.TabIndex = 26;
            this.areDevicesRelatedButton.Text = "Are They Related ?";
            this.areDevicesRelatedButton.UseVisualStyleBackColor = true;
            this.areDevicesRelatedButton.Click += new System.EventHandler(this.areDevicesRelatedButton_Click);
            // 
            // deviceRelationStatusTextBox
            // 
            this.deviceRelationStatusTextBox.Location = new System.Drawing.Point(163, 426);
            this.deviceRelationStatusTextBox.Name = "deviceRelationStatusTextBox";
            this.deviceRelationStatusTextBox.Size = new System.Drawing.Size(100, 35);
            this.deviceRelationStatusTextBox.TabIndex = 27;
            this.deviceRelationStatusTextBox.Text = "";
            // 
            // loadRoomsButton
            // 
            this.loadRoomsButton.Location = new System.Drawing.Point(292, 365);
            this.loadRoomsButton.Name = "loadRoomsButton";
            this.loadRoomsButton.Size = new System.Drawing.Size(75, 23);
            this.loadRoomsButton.TabIndex = 28;
            this.loadRoomsButton.Text = "Load Rooms";
            this.loadRoomsButton.UseVisualStyleBackColor = true;
            this.loadRoomsButton.Click += new System.EventHandler(this.loadRoomsButton_Click);
            // 
            // firstRoomComboBox
            // 
            this.firstRoomComboBox.FormattingEnabled = true;
            this.firstRoomComboBox.Location = new System.Drawing.Point(272, 399);
            this.firstRoomComboBox.Name = "firstRoomComboBox";
            this.firstRoomComboBox.Size = new System.Drawing.Size(121, 21);
            this.firstRoomComboBox.TabIndex = 29;
            this.firstRoomComboBox.SelectedIndexChanged += new System.EventHandler(this.firstRoomComboBox_SelectedIndexChanged);
            // 
            // secondRoomComboBox
            // 
            this.secondRoomComboBox.FormattingEnabled = true;
            this.secondRoomComboBox.Location = new System.Drawing.Point(272, 439);
            this.secondRoomComboBox.Name = "secondRoomComboBox";
            this.secondRoomComboBox.Size = new System.Drawing.Size(121, 21);
            this.secondRoomComboBox.TabIndex = 30;
            this.secondRoomComboBox.SelectedIndexChanged += new System.EventHandler(this.secondRoomComboBox_SelectedIndexChanged);
            // 
            // getRoomRelatedDevicesButton
            // 
            this.getRoomRelatedDevicesButton.Location = new System.Drawing.Point(583, 392);
            this.getRoomRelatedDevicesButton.Name = "getRoomRelatedDevicesButton";
            this.getRoomRelatedDevicesButton.Size = new System.Drawing.Size(75, 71);
            this.getRoomRelatedDevicesButton.TabIndex = 31;
            this.getRoomRelatedDevicesButton.Text = "Identify Related Devices";
            this.getRoomRelatedDevicesButton.UseVisualStyleBackColor = true;
            this.getRoomRelatedDevicesButton.Click += new System.EventHandler(this.getRoomRelatedDevicesButton_Click);
            // 
            // roomRelatedDevicesTextBox
            // 
            this.roomRelatedDevicesTextBox.Location = new System.Drawing.Point(670, 379);
            this.roomRelatedDevicesTextBox.Name = "roomRelatedDevicesTextBox";
            this.roomRelatedDevicesTextBox.Size = new System.Drawing.Size(100, 96);
            this.roomRelatedDevicesTextBox.TabIndex = 32;
            this.roomRelatedDevicesTextBox.Text = "";
            this.roomRelatedDevicesTextBox.TextChanged += new System.EventHandler(this.roomRelatedDevicesTextBox_TextChanged);
            // 
            // mapBpmToTtl
            // 
            this.mapBpmToTtl.Location = new System.Drawing.Point(29, 303);
            this.mapBpmToTtl.Name = "mapBpmToTtl";
            this.mapBpmToTtl.Size = new System.Drawing.Size(84, 47);
            this.mapBpmToTtl.TabIndex = 33;
            this.mapBpmToTtl.Text = "Populate Room Device Relations";
            this.mapBpmToTtl.UseVisualStyleBackColor = true;
            this.mapBpmToTtl.Click += new System.EventHandler(this.mapBpmToTtl_Click);
            // 
            // changeTurtleRoomsButton
            // 
            this.changeTurtleRoomsButton.Location = new System.Drawing.Point(119, 250);
            this.changeTurtleRoomsButton.Name = "changeTurtleRoomsButton";
            this.changeTurtleRoomsButton.Size = new System.Drawing.Size(86, 43);
            this.changeTurtleRoomsButton.TabIndex = 34;
            this.changeTurtleRoomsButton.Text = "Generate New Turtle File";
            this.changeTurtleRoomsButton.UseVisualStyleBackColor = true;
            this.changeTurtleRoomsButton.Click += new System.EventHandler(this.changeTurtleRoomsButton_Click);
            // 
            // directRelationshipButton
            // 
            this.directRelationshipButton.Location = new System.Drawing.Point(29, 491);
            this.directRelationshipButton.Name = "directRelationshipButton";
            this.directRelationshipButton.Size = new System.Drawing.Size(118, 47);
            this.directRelationshipButton.TabIndex = 35;
            this.directRelationshipButton.Text = "Get Direct Relationships of First Device";
            this.directRelationshipButton.UseVisualStyleBackColor = true;
            this.directRelationshipButton.Click += new System.EventHandler(this.relationTypeButton_Click);
            // 
            // directRelationTextBox
            // 
            this.directRelationTextBox.Location = new System.Drawing.Point(163, 491);
            this.directRelationTextBox.Name = "directRelationTextBox";
            this.directRelationTextBox.Size = new System.Drawing.Size(100, 47);
            this.directRelationTextBox.TabIndex = 36;
            this.directRelationTextBox.Text = "";
            // 
            // directRelatedDevicesAndRelationships
            // 
            this.directRelatedDevicesAndRelationships.Location = new System.Drawing.Point(273, 491);
            this.directRelatedDevicesAndRelationships.Name = "directRelatedDevicesAndRelationships";
            this.directRelatedDevicesAndRelationships.Size = new System.Drawing.Size(131, 47);
            this.directRelatedDevicesAndRelationships.TabIndex = 37;
            this.directRelatedDevicesAndRelationships.Text = "Get Direct Relationships of First Device with Relationship Type";
            this.directRelatedDevicesAndRelationships.UseVisualStyleBackColor = true;
            this.directRelatedDevicesAndRelationships.Click += new System.EventHandler(this.directRelatedDevicesAndRelationships_Click);
            // 
            // directRelationshipWithPredicateTextBox
            // 
            this.directRelationshipWithPredicateTextBox.Location = new System.Drawing.Point(410, 491);
            this.directRelationshipWithPredicateTextBox.Name = "directRelationshipWithPredicateTextBox";
            this.directRelationshipWithPredicateTextBox.Size = new System.Drawing.Size(86, 47);
            this.directRelationshipWithPredicateTextBox.TabIndex = 38;
            this.directRelationshipWithPredicateTextBox.Text = "";
            this.directRelationshipWithPredicateTextBox.TextChanged += new System.EventHandler(this.directRelationshipWithPredicateTextBox_TextChanged);
            // 
            // getRelationshipPathButton
            // 
            this.getRelationshipPathButton.Location = new System.Drawing.Point(502, 491);
            this.getRelationshipPathButton.Name = "getRelationshipPathButton";
            this.getRelationshipPathButton.Size = new System.Drawing.Size(74, 47);
            this.getRelationshipPathButton.TabIndex = 39;
            this.getRelationshipPathButton.Text = "Get Realtionship Path";
            this.getRelationshipPathButton.UseVisualStyleBackColor = true;
            this.getRelationshipPathButton.Click += new System.EventHandler(this.getRelationshipPathButton_Click);
            // 
            // relationshipPathTextBox
            // 
            this.relationshipPathTextBox.Location = new System.Drawing.Point(583, 491);
            this.relationshipPathTextBox.Name = "relationshipPathTextBox";
            this.relationshipPathTextBox.Size = new System.Drawing.Size(185, 56);
            this.relationshipPathTextBox.TabIndex = 40;
            this.relationshipPathTextBox.Text = "";
            // 
            // roomRelatedDevicesButton
            // 
            this.roomRelatedDevicesButton.Location = new System.Drawing.Point(399, 384);
            this.roomRelatedDevicesButton.Name = "roomRelatedDevicesButton";
            this.roomRelatedDevicesButton.Size = new System.Drawing.Size(65, 79);
            this.roomRelatedDevicesButton.TabIndex = 41;
            this.roomRelatedDevicesButton.Text = "Identify Related Devices for a Room";
            this.roomRelatedDevicesButton.UseVisualStyleBackColor = true;
            this.roomRelatedDevicesButton.Click += new System.EventHandler(this.roomRelatedDevicesButton_Click);
            // 
            // roomRelatedDevTextBox
            // 
            this.roomRelatedDevTextBox.Location = new System.Drawing.Point(476, 379);
            this.roomRelatedDevTextBox.Name = "roomRelatedDevTextBox";
            this.roomRelatedDevTextBox.Size = new System.Drawing.Size(100, 96);
            this.roomRelatedDevTextBox.TabIndex = 42;
            this.roomRelatedDevTextBox.Text = "";
            // 
            // getRelPathWithRelButton
            // 
            this.getRelPathWithRelButton.Location = new System.Drawing.Point(29, 553);
            this.getRelPathWithRelButton.Name = "getRelPathWithRelButton";
            this.getRelPathWithRelButton.Size = new System.Drawing.Size(118, 60);
            this.getRelPathWithRelButton.TabIndex = 43;
            this.getRelPathWithRelButton.Text = "Get Relationship Path with Predicate";
            this.getRelPathWithRelButton.UseVisualStyleBackColor = true;
            this.getRelPathWithRelButton.Click += new System.EventHandler(this.getRelPathWithRelButton_Click);
            // 
            // relPathWithRelTextBox
            // 
            this.relPathWithRelTextBox.Location = new System.Drawing.Point(163, 553);
            this.relPathWithRelTextBox.Name = "relPathWithRelTextBox";
            this.relPathWithRelTextBox.Size = new System.Drawing.Size(250, 66);
            this.relPathWithRelTextBox.TabIndex = 44;
            this.relPathWithRelTextBox.Text = "";
            // 
            // RDFUpload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(780, 636);
            this.Controls.Add(this.relPathWithRelTextBox);
            this.Controls.Add(this.getRelPathWithRelButton);
            this.Controls.Add(this.roomRelatedDevTextBox);
            this.Controls.Add(this.roomRelatedDevicesButton);
            this.Controls.Add(this.relationshipPathTextBox);
            this.Controls.Add(this.getRelationshipPathButton);
            this.Controls.Add(this.directRelationshipWithPredicateTextBox);
            this.Controls.Add(this.directRelatedDevicesAndRelationships);
            this.Controls.Add(this.directRelationTextBox);
            this.Controls.Add(this.directRelationshipButton);
            this.Controls.Add(this.changeTurtleRoomsButton);
            this.Controls.Add(this.mapBpmToTtl);
            this.Controls.Add(this.roomRelatedDevicesTextBox);
            this.Controls.Add(this.getRoomRelatedDevicesButton);
            this.Controls.Add(this.secondRoomComboBox);
            this.Controls.Add(this.firstRoomComboBox);
            this.Controls.Add(this.loadRoomsButton);
            this.Controls.Add(this.deviceRelationStatusTextBox);
            this.Controls.Add(this.areDevicesRelatedButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.secondDeviceComboBox);
            this.Controls.Add(this.firstDeviceComboBox);
            this.Controls.Add(this.selectModelCombobox);
            this.Controls.Add(this.getAvailableModelsBox);
            this.Controls.Add(this.possibleRelationshipsTextbox);
            this.Controls.Add(this.populateInstancesButton);
            this.Controls.Add(this.relatedDevicesButton);
            this.Controls.Add(this.uniqueInstancesComboBox);
            this.Controls.Add(this.mapTurtleToModelButton);
            this.Controls.Add(this.loadModelButton);
            this.Controls.Add(this.modelName);
            this.Controls.Add(this.browseModelFile);
            this.Controls.Add(this.loadInstancesButton);
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
            this.Text = "Sensor App";
            this.Load += new System.EventHandler(this.RDFUpload_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox fileName;
        private System.Windows.Forms.Button uploadButton;
        private System.Windows.Forms.Button loadNodesButton;
        private System.Windows.Forms.Button loadRelationshipsButton;
        private System.Windows.Forms.ComboBox subjectDropDown;
        private System.Windows.Forms.ComboBox objectDropDown;
        private System.Windows.Forms.RichTextBox predicateOutput;
        private System.Windows.Forms.Button getValidButton;
        private System.Windows.Forms.Button populateDropDownButton;
        private System.Windows.Forms.Button loadInstancesButton;
        private System.Windows.Forms.Button browseModelFile;
        private System.Windows.Forms.TextBox modelName;
        private System.Windows.Forms.Button loadModelButton;
        private System.Windows.Forms.Button mapTurtleToModelButton;
        private System.Windows.Forms.ComboBox uniqueInstancesComboBox;
        private System.Windows.Forms.Button relatedDevicesButton;
        private System.Windows.Forms.Button populateInstancesButton;
        private System.Windows.Forms.RichTextBox possibleRelationshipsTextbox;
        private System.Windows.Forms.Button getAvailableModelsBox;
        private System.Windows.Forms.ComboBox selectModelCombobox;
        private System.Windows.Forms.ComboBox firstDeviceComboBox;
        private System.Windows.Forms.ComboBox secondDeviceComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button areDevicesRelatedButton;
        private System.Windows.Forms.RichTextBox deviceRelationStatusTextBox;
        private System.Windows.Forms.Button loadRoomsButton;
        private System.Windows.Forms.ComboBox firstRoomComboBox;
        private System.Windows.Forms.ComboBox secondRoomComboBox;
        private System.Windows.Forms.Button getRoomRelatedDevicesButton;
        private System.Windows.Forms.RichTextBox roomRelatedDevicesTextBox;
        private System.Windows.Forms.Button mapBpmToTtl;
        private System.Windows.Forms.Button changeTurtleRoomsButton;
        private System.Windows.Forms.Button directRelationshipButton;
        private System.Windows.Forms.RichTextBox directRelationTextBox;
        private System.Windows.Forms.Button directRelatedDevicesAndRelationships;
        private System.Windows.Forms.RichTextBox directRelationshipWithPredicateTextBox;
        private System.Windows.Forms.Button getRelationshipPathButton;
        private System.Windows.Forms.RichTextBox relationshipPathTextBox;
        private System.Windows.Forms.Button roomRelatedDevicesButton;
        private System.Windows.Forms.RichTextBox roomRelatedDevTextBox;
        private System.Windows.Forms.Button getRelPathWithRelButton;
        private System.Windows.Forms.RichTextBox relPathWithRelTextBox;
    }
}

