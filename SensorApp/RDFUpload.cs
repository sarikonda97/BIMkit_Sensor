using DBMS.Controllers.DBControllers;
using DbmsApi;
using DbmsApi.API;
using DbmsApi.Mongo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

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

        protected MongoDbController db = MongoDbController.Instance;

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
                //********************* Need to remove the locations instead of adding them into the DB********************
                // Populate the set of location subclasses
                HashSet<String> locations = new HashSet<String>();
                locations.Add("Building");
                locations.Add("Floor");
                locations.Add("Room");

                // get all possible nodes inside the RDF file.
                Object deviceTypes = g.ExecuteQuery("SELECT DISTINCT ?type WHERE { ?s a ?type }");

                List<String> nodes = new List<String>();

                if (deviceTypes is SparqlResultSet)
                {
                    SparqlResultSet rset = (SparqlResultSet)deviceTypes;
                    foreach (SparqlResult r in rset)
                    {
                        String rString = r.ToString();
                        String nodeName = rString.Substring(rString.LastIndexOf('#') + 1);
                        nodes.Add(nodeName);
                    }
                }

                // Add the nodes into MongoDB as a catalog
                foreach (String node in nodes)
                {
                    MongoDeviceObject mongoDeviceObject = new MongoDeviceObject()
                    {
                        Name = node,
                        TypeId = locations.Contains(node) ? "Location" : "Equipment",
                    };
                    string coId = db.CreateDeviceObject(mongoDeviceObject);
                }

                MessageBox.Show("Successfully populated Nodes");
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private Boolean CheckListContains(List<List<String>> mainList, List<String> subList)
        {
            foreach (List<String> list in mainList)
            {
                if (list.SequenceEqual(subList))
                {
                    return true;
                }
            }
            return false;
        }

        private void loadRelationshipsButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Identify the unique parent relationships in the entire RDF graph
                List<List<String>> uniqueRelationships = new List<List<String>>();
                foreach (List<String> predicate in predicates)
                {

                    String predicatePrefix = predicate[0];
                    String predicateType = predicate[1];

                    IUriNode predicateNode = g.CreateUriNode(predicatePrefix + ":" + predicateType);
                    IEnumerable<Triple> predicateResult = g.GetTriplesWithPredicate(predicateNode);

                    // starts here

                    foreach (Triple triple in predicateResult)
                    {
                        String subjectString = triple.Subject.ToString();
                        String objectString = triple.Object.ToString();
                        String subjectName = subjectString.Substring(subjectString.LastIndexOf('#') + 1);
                        String objectName = objectString.Substring(objectString.LastIndexOf('#') + 1);

                        // This might break********************************************************************
                        IUriNode subjectNode = g.CreateUriNode("soda_hall:" + subjectName);
                        IUriNode objectNode = g.CreateUriNode("soda_hall:" + objectName);

                        IEnumerable<Triple> subjectParent = g.GetTriplesWithSubject(subjectNode);
                        IEnumerable<Triple> objectParent = g.GetTriplesWithSubject(objectNode);

                        String subjectParentName = "";
                        String objectParentName = "";

                        foreach (Triple subjectTriple in subjectParent)
                        {
                            String subjectTriplePredicateString = subjectTriple.Predicate.ToString();
                            String subjectTripleObjectString = subjectTriple.Object.ToString();
                            String subjectTriplePredicateName = subjectTriplePredicateString.Substring(subjectTriplePredicateString.LastIndexOf('#') + 1);
                            String subjectTripleObjectName = subjectTripleObjectString.Substring(subjectTripleObjectString.LastIndexOf('#') + 1);

                            if (subjectTriplePredicateName == "type")
                            {
                                subjectParentName = subjectTripleObjectName;
                            }
                        }

                        foreach (Triple objectTriple in objectParent)
                        {
                            String objectTriplePredicateString = objectTriple.Predicate.ToString();
                            String objectTripleObjectString = objectTriple.Object.ToString();
                            String objectTriplePredicateName = objectTriplePredicateString.Substring(objectTriplePredicateString.LastIndexOf('#') + 1);
                            String objectTripleObjectName = objectTripleObjectString.Substring(objectTripleObjectString.LastIndexOf('#') + 1);

                            if (objectTriplePredicateName == "type")
                            {
                                objectParentName = objectTripleObjectName;
                            }
                        }

                        List<String> temp = new List<String>();
                        temp.Add(subjectParentName);
                        temp.Add(predicateType);
                        temp.Add(objectParentName);

                        if (temp[0] != "" && temp[1] != "" && temp[2] != "" && !CheckListContains(uniqueRelationships, temp))
                        {
                            uniqueRelationships.Add(temp);
                        }
                    }
                }

                // Dump unique relationships into MongoDB as deviceRelationships document
                foreach (List<String> relationship in uniqueRelationships)
                {
                    MongoDeviceRelationships mongoRelationsObject = new MongoDeviceRelationships()
                    {
                        Subject = relationship[0],
                        Predicate = relationship[1],
                        Object = relationship[2]
                    };
                    string coId = db.CreateDeviceRelationship(mongoRelationsObject);
                }
                MessageBox.Show("Relationships populated successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void populateDropDownButton_Click(object sender, EventArgs e)
        {
            List<String> subjects = db.GetUniqueSubjects();
            List<String> objects = db.GetUniqueObjects();
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
                List<String> possiblePredicates = db.GetDeviceRelationship(selectedSubject, selectedObject);

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
                    TurtleParser ttlParser = new TurtleParser();
                    ttlParser.Load(g, path);
                    MessageBox.Show("Upload successful!");

                    // Populate a dictionary of prefixes
                    Dictionary<String, String> prefixMap = new Dictionary<String, String>();
                    prefixMap.Add("https://brickschema.org/schema/Brick", "brick");
                    prefixMap.Add("http://www.w3.org/1999/02/22-rdf-syntax-ns", "rdf");
                    prefixMap.Add("http://www.w3.org/2000/01/rdf-schema", "rdfs");

                    // Query and process possible predicate types
                    Object predicateTypes = g.ExecuteQuery("SELECT DISTINCT ?type WHERE { ?s ?type ?a}");

                    predicates = new List<List<String>>();

                    if (predicateTypes is SparqlResultSet)
                    {
                        SparqlResultSet rset = (SparqlResultSet)predicateTypes;
                        foreach (SparqlResult r in rset)
                        {
                            String rString = r.ToString();
                            int startLocation = rString.IndexOf("http");
                            int endLocation = rString.LastIndexOf('#') - rString.IndexOf("http");
                            String prefix = rString.Substring(startLocation, endLocation);
                            String predicate = rString.Substring(rString.LastIndexOf('#') + 1);

                            List<String> temp = new List<string>();
                            temp.Add(prefixMap[prefix]);
                            temp.Add(predicate);

                            predicates.Add(temp);
                        }
                    }

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
                List<String> possiblePredicates = db.GetDeviceRelationship("HVAC_Zone", "Room");

                String predicatePrefix = "brick";
                String predicateType = possiblePredicates[0];

                foreach (List<String> predicate in predicates)
                {
                    if (predicate[1] == possiblePredicates[0])
                    {
                        predicatePrefix = predicate[0];
                    }
                }

                IUriNode predicateNode = g.CreateUriNode(predicatePrefix + ":" + predicateType);
                IEnumerable<Triple> predicateResult = g.GetTriplesWithPredicate(predicateNode);

                zoneToRoomMap = new Dictionary<String, String>();
                roomToZoneMap = new Dictionary<String, String>();
                roomList = new List<String>();
                zoneList = new List<String>();

                foreach (Triple triple in predicateResult)
                {
                    String subjectString = triple.Subject.ToString();
                    String objectString = triple.Object.ToString();
                    String subjectName = subjectString.Substring(subjectString.LastIndexOf('#') + 1);
                    String objectName = objectString.Substring(objectString.LastIndexOf('#') + 1);

                    zoneToRoomMap.Add(subjectName, objectName);
                    roomToZoneMap.Add(objectName, subjectName);
                    roomList.Add(objectName);
                    zoneList.Add(subjectName);
                }

                MessageBox.Show("Turtle File rooms mapped to the BIM model successfully!");
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
                loadedModel = DBMSReadWrite.ReadModel(modelPath);
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
                Dictionary<String, String> zoneToZoneObjectIdMap = new Dictionary<String, String>();
                validRoomList = new List<String>();

                // create a new model object for each zone with the type id as zone
                foreach (string zone in zoneList)
                {
                    ModelObject modelObject = new ModelObject();
                    modelObject.TypeId = "Zone";
                    modelObject.Name = zone;
                    Guid MyUniqueId = Guid.NewGuid();
                    modelObject.Id = MyUniqueId.ToString();

                    zoneToZoneObjectIdMap.Add(zone, modelObject.Id);
                    loadedModel.ModelObjects.Add(modelObject);
                }

                // Filtering the model objects with type room
                List<ModelObject> modelRoomList = loadedModel.ModelObjects.Where(o => o.TypeId == "Room").ToList();

                int turtleRoomCount = 0;

                // Adding the zone ID's to the room model objects
                foreach (ModelObject roomModel in modelRoomList)
                {
                    //Need to rename all the rooms in Athabasca to the soda hall file names
                    roomModel.Name = roomList[turtleRoomCount];
                    validRoomList.Add(roomList[turtleRoomCount]);
                    ++turtleRoomCount;

                    // Add the new property about zones
                    roomModel.Properties.Add(new PropertyString("Zone", zoneToZoneObjectIdMap[roomToZoneMap[roomModel.Name]]));
                }

                // Filtering zone model objects just to see them
                List<ModelObject> modelZoneList = loadedModel.ModelObjects.Where(o => o.TypeId == "Zone").ToList();

                // Start Building the final deviceList
                deviceObjectList = new List<deviceObject>();

                // Build the room list
                List<MongoRoomRelationships> roomRelationshipList = new List<MongoRoomRelationships>();

                List<String> invalidDevicePredicates = new List<string>
                {
                    "isLocation",
                    "isPartOf",
                    // "hasPart",
                    //"isLocationOf",
                    "hasLocation"
                };

                List<String> currentPredicates = db.GetUniquePredicates();

                List<String> filteredPredicates = currentPredicates.Except(invalidDevicePredicates).ToList();

                List<List<String>> instanceRelationships = new List<List<String>>();

                foreach (String currentPredicate in filteredPredicates)
                {

                    String predicatePrefix = "brick";

                    foreach (List<String> predicate in predicates)
                    {
                        if (predicate[1] == currentPredicate)
                        {
                            predicatePrefix = predicate[0];
                        }
                    }

                    IUriNode predicateNode = g.CreateUriNode(predicatePrefix + ":" + currentPredicate);
                    IEnumerable<Triple> predicateResult = g.GetTriplesWithPredicate(predicateNode);

                    // for each device in the predicate result we gotta create a deviceobject and append to the list
                    foreach (Triple triple in predicateResult)
                    {
                        String predicateString = triple.Predicate.ToString();
                        String subjectString = triple.Subject.ToString();
                        String objectString = triple.Object.ToString();
                        String predicateName = predicateString.Substring(predicateString.LastIndexOf('#') + 1);
                        String subjectName = subjectString.Substring(subjectString.LastIndexOf('#') + 1);
                        String objectName = objectString.Substring(objectString.LastIndexOf('#') + 1);

                        if (currentPredicate == "hasPart")
                        {
                            MongoRoomRelationships mongoRoomRelationship = new MongoRoomRelationships();
                            mongoRoomRelationship.HVAC_Zone = subjectName;
                            mongoRoomRelationship.Room = objectName;

                            roomRelationshipList.Add(mongoRoomRelationship);
                        }
                        else
                        {
                            deviceObject newDevice = new deviceObject(); // Change the name to capital ******************
                            Guid MyUniqueId = Guid.NewGuid();
                            newDevice.id = MyUniqueId.ToString();
                            newDevice.Name = objectName;
                            // create a device type super name id   
                            // create typeid as the same parent's type id which is equipment

                            deviceObjectList.Add(newDevice);

                            deviceObject newDevice2 = new deviceObject(); // Change the name to capital ******************
                            Guid MyUniqueId2 = Guid.NewGuid();
                            newDevice2.id = MyUniqueId2.ToString();
                            newDevice2.Name = subjectName;

                            deviceObjectList.Add(newDevice2);

                            // creating a list of relationships of instances
                            List<String> curInstance = new List<String>();
                            curInstance.Add(subjectName);
                            curInstance.Add(predicateName);
                            curInstance.Add(objectName);

                            if (curInstance[0] != "" && curInstance[1] != "" && curInstance[2] != "")
                            {
                                instanceRelationships.Add(curInstance);
                            }
                        }
                    }
                }

                // lets now create a list of mongo device relationships which I need to put into the model
                List<MongoDeviceRelationships> allInstanceRelationships = new List<MongoDeviceRelationships>();

                foreach (List<String> relationship in instanceRelationships)
                {
                    MongoDeviceRelationships mongoRelationsObject = new MongoDeviceRelationships()
                    {
                        Subject = relationship[0],
                        Predicate = relationship[1],
                        Object = relationship[2]
                    };
                    allInstanceRelationships.Add(mongoRelationsObject);
                }

                loadedModel.deviceObjects = deviceObjectList.GroupBy(dev => dev.Name).Select(g => g.First()).ToList();
                loadedModel.instanceRelationships = allInstanceRelationships;
                loadedModel.roomRelationships = roomRelationshipList;

                MongoModel finalModel = preprocessAndCreateModel(loadedModel);

                string id = db.CreateModel(finalModel);
                db.AddOwnedModel("admin", id);

                MessageBox.Show("Instances loaded successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private MongoModel preprocessAndCreateModel(Model model)
        {
            MongoModel fullModel = new MongoModel(model, "admin");
            fullModel.Id = null;
            fullModel.deviceObjects = model.deviceObjects;
            fullModel.instanceRelationships = model.instanceRelationships;
            fullModel.roomRelationships = model.roomRelationships;

            // Need to do a quick check that the catalog Ids are valid. If not then they are deleted:
            List<CatalogObjectMetadata> coMetas = db.RetrieveAvailableCatalogObjects();
            foreach (CatalogObjectReference coRef in fullModel.CatalogObjects)
            {
                if (coMetas.Select(com => com.CatalogObjectId).Contains(coRef.CatalogId))
                {
                    continue;
                }
                /*if (coMetas.Select(com => com.Name).Contains(coRef.CatalogId))
                {
                    // In this case we know if came from the 3DFront dataset so do some one time fixes:
                    coRef.CatalogId = coMetas.First(i => i.Name == coRef.CatalogId).CatalogObjectId;

                    MongoCatalogObject mongoCO = db.GetCatalogObject(coRef.CatalogId);
                    MeshRep meshRep = mongoCO.MeshReps.First(m => m.LevelOfDetail == LevelOfDetail.LOD100);
                    List<Vector3D> vects = meshRep.Components.SelectMany(co => co.Vertices).ToList();
                    double top = vects.Max(v => v.z);
                    double bottom = vects.Min(v => v.z);
                    coRef.Location = new Vector3D(coRef.Location.x, coRef.Location.y, coRef.Location.z + (top - bottom) / 2.0);

                    continue;
                }*/

                coRef.CatalogId = null;
            }

            fullModel.CatalogObjects = fullModel.CatalogObjects.Where(coref => coref.CatalogId != null).ToList();

            return fullModel;
        }

        private void populateInstancesButton_Click(object sender, EventArgs e)
        {
            try
            {
                // get the list of devices from the device list
                string[] selectedModelFromDropDown = selectedModel.Split('-');
                currentModel = db.GetModel(selectedModelFromDropDown[1]);

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
                List<ModelMetadata> existingModels = db.RetrieveAllModels();

                foreach (ModelMetadata model in existingModels)
                {
                    Console.WriteLine("Hello");

                    selectModelCombobox.Items.Add(model.Name + "-" + model.ModelId);
                }
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

                foreach (String dev in relatedDevices)
                {
                    possibleRelationshipsTextbox.AppendText(dev + Environment.NewLine);
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

                recursiveRelationships(firstDevice);

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

        private void recursiveRelationships(String devName)
        {
            HashSet<String> relatedDevices = getRelatedDevices(devName);

            if (relatedDevices.Count == 0)
            {
                /*deviceRelationStatusTextBox.Text = "No";*/
                return;
            }

            if (relatedDevices.All(s => bigRelatedDevices.Contains(s)))
            {
                return;
            }
            else
            {
                bigRelatedDevices.UnionWith(relatedDevices);
                foreach (String dev in relatedDevices)
                {
                    recursiveRelationships(dev);
                }
            }
        }

        private HashSet<String> getRelatedDevices(String deviceName)
        {
            HashSet<String> relatedDevices = new HashSet<String>();

            foreach (MongoDeviceRelationships rel in currentModel.instanceRelationships)
            {
                if (rel.Subject == deviceName)
                {
                    relatedDevices.Add(rel.Object);
                }
                else if (rel.Object == deviceName)
                {
                    relatedDevices.Add(rel.Subject);
                }
            }

            return relatedDevices;
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

                foreach (MongoRoomRelationships room in currentModel.roomRelationships)
                {
                    if (room.Room == firstSelectedRoom)
                    {
                        firstHVACZone = room.HVAC_Zone;
                    }

                    if (room.Room == secondSelectedRoom)
                    {
                        secondHVACZone = room.HVAC_Zone;
                    }
                }

                bigRelatedDevices = new HashSet<String>();
                recursiveRelationships(firstHVACZone);

                HashSet<String> firstRoomRelatedDevices = bigRelatedDevices;

                bigRelatedDevices = new HashSet<String>();
                recursiveRelationships(secondHVACZone);

                HashSet<String> secondRoomRelatedDevices = bigRelatedDevices;

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
    }
}
