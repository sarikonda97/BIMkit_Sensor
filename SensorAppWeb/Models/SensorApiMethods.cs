using SensorApp;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DbmsApi.API;
using DbmsApi.Mongo;
using VDS.RDF;
using DBMS.Controllers.DBControllers;
using Newtonsoft.Json;

namespace SensorAppWeb.Models
{
    public class SensorApiMethods
    {
        // string path;
        // string modelPath;
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

        protected MongoDbController db = MongoDbController.Instance;


        public string LoadFiles(string ttlPath, string modelPath)
        {
            try
            {
                g = new Graph();
                predicates = new List<List<String>>();
                loadedModel = new Model();
                atbRoomList = new List<string>();
                zoneToRoomMap = new Dictionary<String, String>();
                roomToZoneMap = new Dictionary<String, String>();
                roomList = new List<String>();
                zoneList = new List<String>();
                CoreSensorMethods.uploadTtl(g, ttlPath, predicates);
                CoreSensorMethods.loadNodes(db, g);
                CoreSensorMethods.loadRelationships(predicates, g, db);
                CoreSensorMethods.loadModel(loadedModel, modelPath);
                CoreSensorMethods.mapTtlToBpm(db, predicates, roomList, zoneList, roomToZoneMap, zoneToRoomMap, g);
                CoreSensorMethods.mapBpmToTtl(atbRoomList, loadedModel);
                //    CoreSensorMethods.changeTurtleRooms(ttlPath, atbRoomList, roomList, roomToZoneMap); // not including change turtle rooms as it is a one time operation 
                CoreSensorMethods.loadInstances(loadedModel, validRoomList, zoneList, roomList, roomToZoneMap, deviceObjectList, predicates, g, db);

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            
            return "Successfully Loaded the Brick and BIM file!";
        }

        public string getSubjectsAndObjects()
        {
            List<String> subjects = new List<string>();
            List<String> objects = new List<string>();

            CoreSensorMethods.getSubjectsAndObjects(db, subjects, objects);

            var returnObject = new
            {
                subjects = subjects.ToList(),
                objects = objects.ToList()
            };

            return JsonConvert.SerializeObject(returnObject);
        }

        public string getValidRelationships(string subjectName, string objectName)
        {
            List<string> possiblePredicates = CoreSensorMethods.getDeviceRelationship(db, subjectName, objectName);

            List<string> output = new List<string>();

            if (possiblePredicates.Count == 0)
            {
                output.Add("No Valid Relationships");
            } 
            else
            {
                output.AddRange(possiblePredicates);
            }

            var returnObject = new
            {
                relations = output.ToList()
            };

            return JsonConvert.SerializeObject(returnObject);
        }

        public string getAvailableModels()
        {
            List<ModelMetadata> existingModels = CoreSensorMethods.getAvailableModels(db);
            List<string> output = new List<string>();

            foreach (ModelMetadata model in existingModels)
            {
                output.Add(model.Name + "-" + model.ModelId);
            }

            var returnObject = new
            {
                models = output.ToList()
            };

            return JsonConvert.SerializeObject(returnObject);
        }

        public string getInstancesFromModel(string currentModelName)
        {
            List<string> output = new List<String>();

            MongoModel currentModel = CoreSensorMethods.getCurrentModel(db, currentModelName);

            foreach (deviceObject dObj in currentModel.deviceObjects)
            {
                output.Add(dObj.Name);
            }

            var returnObject = new
            {
                instances = output.ToList()
            };

            return JsonConvert.SerializeObject(returnObject);
        }

        public string areDevicesRelated(string currentModelName, string firstDevice, string secondDevice)
        {
            MongoModel currentModel = CoreSensorMethods.getCurrentModel(db, currentModelName);
            bigRelatedDevices = new HashSet<String>();
            CoreSensorMethods.recursiveRelationships(currentModel, bigRelatedDevices, firstDevice);

            string output;

            if (bigRelatedDevices.Contains(secondDevice))
            {
                output = "Yes";
            }
            else
            {
                output = "No";
            }

            var returnObject = new
            {
                related = output
            };

            return JsonConvert.SerializeObject(returnObject);
        }

        public string getRelatedDevices(string currentModelName, string deviceName)
        {
            MongoModel currentModel = CoreSensorMethods.getCurrentModel(db, currentModelName);

            HashSet<String> relatedDevices = new HashSet<String>();
            List<string> output = new List<string>();

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

            if (relatedDevices.Count == 0)
            {
                output.Add("No Related Devices");
            }
            else
            {
                foreach (String dev in relatedDevices)
                {
                    output.Add(dev);
                }
            }

            var returnObject = new
            {
                relatedDevices = output
            };

            return JsonConvert.SerializeObject(returnObject);
        }

        public string getRooms(string currentModelName)
        {
            MongoModel currentModel = CoreSensorMethods.getCurrentModel(db, currentModelName);
            List<string> output = new List<string>();

            foreach (MongoRoomRelationships room in currentModel.roomRelationships)
            {
                output.Add(room.Room);
            }

            var returnObject = new
            {
                rooms = output
            };

            return JsonConvert.SerializeObject(returnObject);
        }

        public string getRoomCommonDevices(string currentModelName, string firstRoom, string secondRoom)
        {
            MongoModel currentModel = CoreSensorMethods.getCurrentModel(db, currentModelName);
            String firstHVACZone = "";
            String secondHVACZone = "";
            HashSet<String> firstRoomRelatedDevices = new HashSet<string>();
            HashSet<String> secondRoomRelatedDevices = new HashSet<string>();
            bigRelatedDevices = new HashSet<String>();
            List<string> output = new List<string>();

            CoreSensorMethods.getRoomRelatedDevicesButton(currentModel, firstHVACZone, secondHVACZone, firstRoom, secondRoom, bigRelatedDevices, firstRoomRelatedDevices, secondRoomRelatedDevices);


            foreach (String dev in firstRoomRelatedDevices)
            {
                if (secondRoomRelatedDevices.Contains(dev))
                {
                    output.Add(dev);
                }
            }

            if (output.Count == 0)
            {
                output.Add("No Common Devices");
            }

            var returnObject = new
            {
                rooms = output
            };

            return JsonConvert.SerializeObject(returnObject);
        }
    }
}