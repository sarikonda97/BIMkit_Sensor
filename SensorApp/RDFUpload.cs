using DBMS.Controllers.DBControllers;
using DbmsApi;
using DbmsApi.API;
using DbmsApi.Mongo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace SensorApp
{
    public partial class RDFUpload : Form
    {
        string path;
        string modelPath;
        IGraph g;
        Model loadedModel;
        String selectedSubject;
        String selectedObject;
        List<List<String>> predicates;
        Dictionary<String, String> zoneToRoomMap;
        Dictionary<String, String> roomToZoneMap;
        List<String> roomList;
        List<String> atbRoomList;
        List<String> zoneList;
        List<String> validRoomList;
        List<deviceObject> deviceObjectList;
        String selectedModel;
        String selectedInstance;
        MongoModel currentModel;
        String firstDevice;
        String secondDevice;
        HashSet<String> bigRelatedDevices;
        String firstSelectedRoom;
        String secondSelectedRoom;

        public MongoDbController db = MongoDbController.Instance;

        public RDFUpload()
        {
            InitializeComponent();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //To where your opendialog box get starting location. My initial directory location is desktop.
            openFileDialog1.InitialDirectory = "C://Desktop";
            //Your opendialog box title name.
            openFileDialog1.Title = "Select file to upload.";
            //which type file format you want to upload in database. just add them.
            openFileDialog1.Filter = "Select Valid Document(*.ttl)|*.ttl";
            //FilterIndex property represents the index of the filter currently selected in the file dialog box.
            openFileDialog1.FilterIndex = 1;
            try
            {
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (openFileDialog1.CheckFileExists)
                    {
                        path = System.IO.Path.GetFullPath(openFileDialog1.FileName);
                        fileName.Text = path;
                    }
                }
                else
                {
                    MessageBox.Show("Please Upload document.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void loadNodesButton_Click(object sender, EventArgs e)
        {
            try
            {
                CoreSensorMethods.loadNodes(db, g);
                MessageBox.Show("Successfully populated Nodes");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void loadRelationshipsButton_Click(object sender, EventArgs e)
        {
            try
            {
                CoreSensorMethods.loadRelationships(predicates, g, db);
                MessageBox.Show("Relationships populated successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void populateDropDownButton_Click(object sender, EventArgs e)
        {
            List<String> subjects = new List<string>();
            List<String> objects = new List<string>();

            CoreSensorMethods.getSubjectsAndObjects(db, subjects, objects);

            if (subjectDropDown.Items.Count == 0 && objectDropDown.Items.Count == 0)
            {
                foreach (String subject in subjects)
                {
                    subjectDropDown.Items.Add(subject);
                }
                foreach (String obj in objects)
                {
                    objectDropDown.Items.Add(obj);
                }
            }
        }

        private void subjectDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object selectedItem = subjectDropDown.SelectedItem;
            /*int selectedIndex = subjectDropDown.SelectedIndex;*/ // unnecessary for now

            if(selectedItem is String)
            {
                selectedSubject = (String)selectedItem;
            }
        }

        private void objectDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object selectedItem = objectDropDown.SelectedItem;
            /*int selectedIndex = subjectDropDown.SelectedIndex;*/ // unnecessary for now

            if (selectedItem is String)
            {
                selectedObject = (String)selectedItem;
            }
        }

        private void getValidButton_Click(object sender, EventArgs e)
        {
            if (selectedSubject != null && selectedObject != null) {
                List<String> possiblePredicates = CoreSensorMethods.getDeviceRelationship(db, selectedSubject, selectedObject);

                predicateOutput.Text = "";
                if (possiblePredicates.Count == 0)
                {
                    predicateOutput.AppendText("No Valid Relationships" + Environment.NewLine);
                }

                foreach(String predicate in possiblePredicates)
                {
                    predicateOutput.AppendText(predicate + Environment.NewLine);
                }
            } 
            else
            {
                MessageBox.Show("Please select valid subject and object.");
            }
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            try
            {
                string filename = path;
                if (filename == null)
                {
                    MessageBox.Show("Please select a valid document.");
                }
                else
                {
                    g = new Graph();
                    predicates = new List<List<String>>();
                    CoreSensorMethods.uploadTtl(g, path, predicates);
                    MessageBox.Show("Upload successful!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void browseModelFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            //To where your opendialog box get starting location. My initial directory location is desktop.
            openFileDialog.InitialDirectory = "C://Desktop";
            //Your opendialog box title name.
            openFileDialog.Title = "Select file to upload.";
            //which type file format you want to upload in database. just add them.
            openFileDialog.Filter = "Select Valid Document(*.bpm)|*.bpm";
            //FilterIndex property represents the index of the filter currently selected in the file dialog box.
            openFileDialog.FilterIndex = 1;
            try
            {
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (openFileDialog.CheckFileExists)
                    {
                        modelPath = System.IO.Path.GetFullPath(openFileDialog.FileName);
                        modelName.Text = modelPath;
                    }
                }
                else
                {
                    MessageBox.Show("Please Upload document.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void mapTurtleToModelButton_Click(object sender, EventArgs e)
        {
            try
            {
                zoneToRoomMap = new Dictionary<String, String>();
                roomToZoneMap = new Dictionary<String, String>();
                roomList = new List<String>();
                zoneList = new List<String>();
                CoreSensorMethods.mapTtlToBpm(db, predicates, roomList, zoneList, roomToZoneMap, zoneToRoomMap, g);
                MessageBox.Show("Turtle File rooms mapped to the BIM model successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void mapBpmToTtl_Click(object sender, EventArgs e)
        {
            try
            {
                atbRoomList = new List<string>();
                CoreSensorMethods.mapBpmToTtl(atbRoomList, loadedModel);
                MessageBox.Show("BPM Rooms mapped to the TTL File successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void changeTurtleRoomsButton_Click(object sender, EventArgs e)
        {
            try
            {
                CoreSensorMethods.changeTurtleRooms(path, atbRoomList, roomList, roomToZoneMap);
                MessageBox.Show("TTL Rooms have been changed to BPM Room convention and new TTL file has been generated!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void loadModelButton_Click(object sender, EventArgs e)
        {
            try
            {
                loadedModel = new Model();
                CoreSensorMethods.loadModel(loadedModel, modelPath);
                MessageBox.Show("Model loaded successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void loadInstancesButton_Click(object sender, EventArgs e)
        {
            try
            {
                CoreSensorMethods.loadInstances(loadedModel, validRoomList, zoneList, roomList, roomToZoneMap, deviceObjectList, predicates, g, db);
                MessageBox.Show("Instances loaded successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void populateInstancesButton_Click(object sender, EventArgs e)
        {
            try
            {
                // get the list of devices from the device list
                string[] selectedModelFromDropDown = selectedModel.Split('-');
                currentModel = CoreSensorMethods.getCurrentModel(db, selectedModelFromDropDown[1]);

                foreach (deviceObject dObj in currentModel.deviceObjects)
                {
                    uniqueInstancesComboBox.Items.Add(dObj.Name);
                    firstDeviceComboBox.Items.Add(dObj.Name);
                    secondDeviceComboBox.Items.Add(dObj.Name);
                }

                MessageBox.Show("Instances populated successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void getAvailableModelsBox_Click(object sender, EventArgs e)
        {
            try
            {
                // get a list of available models and populate it into the combo box
                List<ModelMetadata> existingModels = CoreSensorMethods.getAvailableModels(db);

                foreach (ModelMetadata model in existingModels)
                {
                    selectModelCombobox.Items.Add(model.Name + "-" + model.ModelId);
                }

                MessageBox.Show("Model Meta Data Loaded Successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void selectModelCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object selectedItem = selectModelCombobox.SelectedItem;

            if (selectedItem is String)
            {
                selectedModel = (String)selectedItem;
            }
        }

        private void uniqueInstancesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object selectedItem = uniqueInstancesComboBox.SelectedItem;

            if (selectedItem is String)
            {
                selectedInstance = (String)selectedItem;
            }
        }

        private void relatedDevicesButton_Click(object sender, EventArgs e)
        {
            try
            {
                HashSet<String> relatedDevices = new HashSet<String>();

                foreach (MongoDeviceRelationships rel in currentModel.instanceRelationships)
                {
                    if (rel.Subject == selectedInstance)
                    {
                        relatedDevices.Add(rel.Object);
                    }
                    else if (rel.Object == selectedInstance)
                    {
                        relatedDevices.Add(rel.Subject);
                    }
                }

                possibleRelationshipsTextbox.Text = "";
                if (relatedDevices.Count == 0)
                {
                    possibleRelationshipsTextbox.AppendText("No Related Devices" + Environment.NewLine);
                }
                else
                {
                    foreach (String dev in relatedDevices)
                    {
                        possibleRelationshipsTextbox.AppendText(dev + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void firstDeviceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object selectedItem = firstDeviceComboBox.SelectedItem;

            if (selectedItem is String)
            {
                firstDevice = (String)selectedItem;
            }
        }

        private void secondDeviceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object selectedItem = secondDeviceComboBox.SelectedItem;

            if (selectedItem is String)
            {
                secondDevice = (String)selectedItem;
            }
        }

        private void areDevicesRelatedButton_Click(object sender, EventArgs e)
        {
            try
            {
                bigRelatedDevices = new HashSet<String>();
                CoreSensorMethods.recursiveRelationships(currentModel, bigRelatedDevices, firstDevice);

                deviceRelationStatusTextBox.Text = "";
                if (bigRelatedDevices.Contains(secondDevice))
                {
                    deviceRelationStatusTextBox.Text = "Yes";
                }
                else
                {
                    deviceRelationStatusTextBox.Text = "No";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void loadRoomsButton_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (MongoRoomRelationships room in currentModel.roomRelationships)
                {
                    firstRoomComboBox.Items.Add(room.Room);
                    secondRoomComboBox.Items.Add(room.Room);
                }

                MessageBox.Show("Rooms loaded Sucessfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void getRoomRelatedDevicesButton_Click(object sender, EventArgs e)
        {
            try
            {
                String firstHVACZone = "";
                String secondHVACZone = "";
                HashSet<String> firstRoomRelatedDevices = new HashSet<string>();
                HashSet<String> secondRoomRelatedDevices = new HashSet<string>();

                CoreSensorMethods.getRoomRelatedDevicesButton(currentModel, firstHVACZone, secondHVACZone, firstSelectedRoom, secondSelectedRoom, bigRelatedDevices, firstRoomRelatedDevices, secondRoomRelatedDevices);

                roomRelatedDevicesTextBox.Text = "";

                foreach (String dev in firstRoomRelatedDevices)
                {
                    if (secondRoomRelatedDevices.Contains(dev))
                    {
                        roomRelatedDevicesTextBox.AppendText(dev + Environment.NewLine);
                    }
                }

                if (roomRelatedDevicesTextBox.Text == "")
                {
                    roomRelatedDevicesTextBox.Text = "No Common Devices";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void firstRoomComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object selectedItem = firstRoomComboBox.SelectedItem;

            if (selectedItem is String)
            {
                firstSelectedRoom = (String)selectedItem;
            }
        }

        private void secondRoomComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object selectedItem = secondRoomComboBox.SelectedItem;

            if (selectedItem is String)
            {
                secondSelectedRoom = (String)selectedItem;
            }
        }

        private void RDFUpload_Load(object sender, EventArgs e)
        {

        }

        private void relationTypeButton_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> directRelations = CoreSensorMethods.getDirectRelationship(currentModel, firstDevice);

                directRelationTextBox.Text = "";
                foreach (string rel in directRelations)
                {
                    directRelationTextBox.AppendText(rel + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void roomRelatedDevicesTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void directRelatedDevicesAndRelationships_Click(object sender, EventArgs e)
        {
            try
            {
                List<List<string>> directRelationshipWithPredicate = CoreSensorMethods.getDirectRelationshipsWithPredicate(currentModel, firstDevice);

                foreach(List<string> rel in directRelationshipWithPredicate)
                {
                    directRelationshipWithPredicateTextBox.AppendText(rel[0] + " - " + rel[1] + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void directRelationshipWithPredicateTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void getRelationshipPathButton_Click(object sender, EventArgs e)
        {
            try
            {
                bigRelatedDevices = new HashSet<String>();
                CoreSensorMethods.recursiveRelationships(currentModel, bigRelatedDevices, firstDevice);

                relationshipPathTextBox.Text = "";
                if (bigRelatedDevices.Contains(secondDevice))
                {
                    List<string> relationshipPath = CoreSensorMethods.getRelatedDevicesPath(currentModel, firstDevice, secondDevice);

                    relationshipPathTextBox.AppendText(firstDevice);
                    foreach (string rel in relationshipPath)
                    {
                        relationshipPathTextBox.AppendText(" -> " + rel);
                    }
                }
                else
                {
                    relationshipPathTextBox.AppendText("No Relationship exists");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
