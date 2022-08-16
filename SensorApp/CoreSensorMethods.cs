using DBMS.Controllers.DBControllers;
using DbmsApi;
using DbmsApi.API;
using DbmsApi.Mongo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace SensorApp
{
    public static class CoreSensorMethods
    {
        public static void uploadTtl(IGraph g, string path, List<List<String>> predicates)
        {
            TurtleParser ttlParser = new TurtleParser();
            ttlParser.Load(g, path);

            // Populate a dictionary of prefixes
            Dictionary<String, String> prefixMap = new Dictionary<String, String>();
            prefixMap.Add("https://brickschema.org/schema/Brick", "brick");
            prefixMap.Add("http://www.w3.org/1999/02/22-rdf-syntax-ns", "rdf");
            prefixMap.Add("http://www.w3.org/2000/01/rdf-schema", "rdfs");

            // Query and process possible predicate types
            Object predicateTypes = g.ExecuteQuery("SELECT DISTINCT ?type WHERE { ?s ?type ?a}");

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

        public static void loadNodes(MongoDbController db, IGraph g)
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

        }
        
        public static void loadRelationships(List<List<String>> predicates, IGraph g, MongoDbController db)
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

                    // hard coded value to soda_hall. Will break if used with other namespace files.
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
        }

        public static void loadModel(Model loadedModel, string modelPath)
        {
            loadedModel = DBMSReadWrite.ReadModel(modelPath);
        }

        public static void mapTtlToBpm(MongoDbController db, List<List<String>> predicates, List<String> roomList, List<String> zoneList, Dictionary<String, String> roomToZoneMap, Dictionary<String, String> zoneToRoomMap, IGraph g)
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

            foreach (Triple triple in predicateResult)
            {
                String subjectString = triple.Subject.ToString();
                String objectString = triple.Object.ToString();
                String subjectName = subjectString.Substring(subjectString.LastIndexOf('#') + 1);
                String objectName = objectString.Substring(objectString.LastIndexOf('#') + 1);

                /*if (!zoneToRoomMap.ContainsKey(subjectName))
                {
                    zoneToRoomMap.Add(subjectName, objectName);
                }*/
                zoneToRoomMap[subjectName] = objectName;
                /*
                if(!roomToZoneMap.ContainsKey(objectName))
                {
                    roomToZoneMap.Add(objectName, subjectName);
                }*/
                roomToZoneMap[objectName] = subjectName;
                
                roomList.Add(objectName);
                zoneList.Add(subjectName);
            }
        }

        public static void mapBpmToTtl(List<String> atbRoomList, Model loadedModel)
        {
            atbRoomList = new List<String>();
            List<ModelObject> modelRoomList = loadedModel.ModelObjects.Where(o => o.TypeId == "Room").ToList();

            foreach (ModelObject roomModel in modelRoomList)
            {
                atbRoomList.Add(roomModel.Name.Replace(" ", "_").Replace(":", ""));
            }
        }

        public static void changeTurtleRooms(string turtlePath, List<String> atbRoomList, List<String> roomList, Dictionary<String, String> roomToZoneMap)
        {
            //    String ttlFile = File.ReadAllText("C:\\Users\\csydora\\Desktop\\soda_hall.ttl");
            String ttlFile = turtlePath;
            int ttlRoomCount = 0;
            foreach (String atbRoom in atbRoomList)
            {
                String zoneName = "hvac_zone_" + atbRoom;

                ttlFile = ttlFile.Replace(roomList[ttlRoomCount], atbRoom);
                ttlFile = ttlFile.Replace(roomToZoneMap[roomList[ttlRoomCount]], zoneName);
                ++ttlRoomCount;
            }

            File.WriteAllText("C:\\Users\\csydora\\Desktop\\BIMkit\\SensorApp\\test.ttl", ttlFile);
        }

        public static void loadInstances(Model loadedModel, List<String> validRoomList, List<String> zoneList, List<String> roomList, Dictionary<String, String> roomToZoneMap, List<deviceObject> deviceObjectList, List<List<String>> predicates, IGraph g, MongoDbController db)
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

                if (!zoneToZoneObjectIdMap.ContainsKey(zone))
                {
                    zoneToZoneObjectIdMap.Add(zone, modelObject.Id);
                }
                
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

            loadedModel.deviceObjects = deviceObjectList.GroupBy(dev => dev.Name).Select(gh => gh.First()).ToList(); // gh was initially g
            loadedModel.instanceRelationships = allInstanceRelationships;
            loadedModel.roomRelationships = roomRelationshipList;

            MongoModel finalModel = preprocessAndCreateModel(loadedModel, db);

            string id = db.CreateModel(finalModel);
            db.AddOwnedModel("admin", id);
        }

        public static void getSubjectsAndObjects(MongoDbController db, List<string> subjects, List<string> objects)
        {
            subjects.AddRange(db.GetUniqueSubjects());
            objects.AddRange(db.GetUniqueObjects());
        }

        public static List<string> getDeviceRelationship(MongoDbController db, string selectedSubject, string selectedObject)
        {
            return db.GetDeviceRelationship(selectedSubject, selectedObject);
        }

        public static List<ModelMetadata> getAvailableModels(MongoDbController db)
        {
            return db.RetrieveAllModels();
        }

        public static MongoModel getCurrentModel(MongoDbController db, string selectedModel)
        {
            return db.GetModel(selectedModel);
        }

        public static void recursiveRelationships(MongoModel currentModel, HashSet<String> bigRelatedDevices, string devName)
        {
            HashSet<String> relatedDevices = getRelatedDevices(currentModel, devName);

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
                    recursiveRelationships(currentModel, bigRelatedDevices, dev);
                }
            }
        }

        public static void getRoomRelatedDevicesButton(MongoModel currentModel, string firstHVACZone, string secondHVACZone, string firstRoom, string secondRoom, HashSet<String> bigRelatedDevices, HashSet<String> firstRoomRelatedDevices, HashSet<String> secondRoomRelatedDevices)
        {
            foreach (MongoRoomRelationships room in currentModel.roomRelationships)
            {
                if (room.Room == firstRoom)
                {
                    firstHVACZone = room.HVAC_Zone;
                }

                if (room.Room == secondRoom)
                {
                    secondHVACZone = room.HVAC_Zone;
                }
            }

            bigRelatedDevices = new HashSet<String>();
            CoreSensorMethods.recursiveRelationships(currentModel, bigRelatedDevices, firstHVACZone);

            firstRoomRelatedDevices.UnionWith(bigRelatedDevices);

            bigRelatedDevices = new HashSet<String>();
            CoreSensorMethods.recursiveRelationships(currentModel, bigRelatedDevices, secondHVACZone);

            secondRoomRelatedDevices.UnionWith(bigRelatedDevices);
        }

        public static List<string> getDirectRelationship(MongoModel currentModel, string devName)
        {
            HashSet<String> relatedDevices = getRelatedDevices(currentModel, devName);
            List<string> output = new List<string>();

            if (relatedDevices.Count == 0)
            {
                output.Add("No Related Devices");
            }
            else
            {
                output.AddRange(relatedDevices.ToList());
            }

            return output;
        }

        public static List<List<string>> getDirectRelationshipsWithPredicate(MongoModel currentModel, string devName)
        {
            Dictionary<string, string> relatedDevicesWithPredicate = getRelatedDevicesWithRelationship(currentModel, devName);
            List<List<string>> output = new List<List<string>>();

            if (relatedDevicesWithPredicate.Count == 0)
            {
                List<string> temp = new List<string>();
                temp.Add("No Related Devices");
            }
            else
            {
                List<string> keys = relatedDevicesWithPredicate.Keys.ToList();
                List<string> values = relatedDevicesWithPredicate.Values.ToList();


                for (int i=0; i<keys.Count; i++)
                {
                    List<string> temp = new List<string>();
                    temp.Add(keys[i]);
                    temp.Add(values[i]);
                    output.Add(temp);
                }
            }

            return output;
        }

        public static List<string> getRelatedDevicesPath(MongoModel currentModel, string firstDevice, string secondDevice)
        {
            List<List<string>> deviceRelations = new List<List<string>>();
            deviceRelations.Add(new List<string> { firstDevice });

            while(true)
            {
                List<List<string>> temp = new List<List<string>>();
                foreach (List<string> dev in deviceRelations)
                {
                    string currentDevice = dev[dev.Count() - 1];
                    HashSet<string> relatedDevicesSet = getRelatedDevices(currentModel, currentDevice);
                    List<string> relatedDevicesList = relatedDevicesSet.ToList();

                    for (int i = 0; i < relatedDevicesList.Count; i++)
                    {
                        if (relatedDevicesList[i] == secondDevice)
                        {
                            dev.Add(secondDevice);
                            return dev.GetRange(1, dev.Count-1);
                        }
                        else
                        {
                            List<string> tempDev = new List<string>(dev);
                            tempDev.Add(relatedDevicesList[i]);
                            temp.Add(tempDev);
                        }

                    }
                }
                deviceRelations = new List<List<string>>(temp);
            }
        }

        private static HashSet<String> getRelatedDevices(MongoModel currentModel, String deviceName)
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

        private static Dictionary<string, string> getRelatedDevicesWithRelationship(MongoModel currentModel, String deviceName)
        {
            Dictionary<string, string> relatedDevices = new Dictionary<string, string>();

            foreach (MongoDeviceRelationships rel in currentModel.instanceRelationships)
            {
                if (rel.Subject == deviceName && !relatedDevices.ContainsKey(rel.Object))
                {
                    relatedDevices.Add(rel.Object, rel.Predicate);
                }
                else if (rel.Object == deviceName && !relatedDevices.ContainsKey(rel.Subject))
                {
                    relatedDevices.Add(rel.Subject, rel.Predicate);
                }
            }

            return relatedDevices;
        }

        private static Boolean CheckListContains(List<List<String>> mainList, List<String> subList)
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

        private static MongoModel preprocessAndCreateModel(Model model, MongoDbController db)
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
    }
}
