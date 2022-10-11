using DBMS.Controllers.DBControllers;
using DbmsApi.API;
using DbmsApi.Mongo;
/*using SensorApp;*/
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;

namespace SensorAppApi
{
    public static class SensorAPIController
    {
        #region sensorAppApi

        public static HttpResponseMessage TryCatchCoreFunctionsForVoid(Action function)
        {
            try
            {
                function();
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
                return httpResponseMessage;
            }
            catch (TimeoutException)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.RequestTimeout, ReasonPhrase = "Server timed out." };
            }
            catch (HttpRequestException e)
            {
                if (e.GetBaseException().GetType() == typeof(SocketException))
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.ServiceUnavailable, ReasonPhrase = "Unable to establish connection with server." };
                else
                    throw e;
            }
        }

        /*public static HttpResponseMessage TryCatchCoreFunctions<T>(Func<List<string>> function)
        {
            try
            {
                List<string> output = function();
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK};
                return httpResponseMessage;
            }
            catch (TimeoutException)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.RequestTimeout, ReasonPhrase = "Server timed out." };
            }
            catch (HttpRequestException e)
            {
                if (e.GetBaseException().GetType() == typeof(SocketException))
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.ServiceUnavailable, ReasonPhrase = "Unable to establish connection with server." };
                else
                    throw e;
            }
        }*/

        public static void uploadTtlApi(IGraph g, string path, List<List<String>> predicates)
        {
            HttpResponseMessage response = TryCatchCoreFunctionsForVoid(() => CoreSensorMethods.uploadTtl(g, path, predicates));

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failure");
            }
        }

        public static void loadNodesApi(MongoDbController db, IGraph g)
        {
            HttpResponseMessage response = TryCatchCoreFunctionsForVoid(() => CoreSensorMethods.loadNodes(db, g));

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failure");
            }
        }

        public static void loadRelationshipsApi(List<List<String>> predicates, IGraph g, MongoDbController db)
        {
            HttpResponseMessage response = TryCatchCoreFunctionsForVoid(() => CoreSensorMethods.loadRelationships(predicates, g, db));

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failure");
            }
        }

        public static void loadModelApi(Model loadedModel, string modelPath)
        {
            HttpResponseMessage response = TryCatchCoreFunctionsForVoid(() => CoreSensorMethods.loadModel(loadedModel, modelPath));

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failure");
            }
        }

        public static void mapTtlToBpmApi(MongoDbController db, List<List<String>> predicates, List<String> roomList, List<String> zoneList, Dictionary<String, String> roomToZoneMap, Dictionary<String, String> zoneToRoomMap, IGraph g)
        {
            HttpResponseMessage response = TryCatchCoreFunctionsForVoid(() => CoreSensorMethods.mapTtlToBpm(db, predicates, roomList, zoneList, roomToZoneMap, zoneToRoomMap, g));

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failure");
            }
        }

        public static void mapBpmToTtlApi(List<String> atbRoomList, Model loadedModel)
        {
            HttpResponseMessage response = TryCatchCoreFunctionsForVoid(() => CoreSensorMethods.mapBpmToTtl(atbRoomList, loadedModel));

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failure");
            }
        }

        public static void changeTurtleRoomsApi(string path, List<String> atbRoomList, List<String> roomList, Dictionary<String, String> roomToZoneMap)
        {
            HttpResponseMessage response = TryCatchCoreFunctionsForVoid(() => CoreSensorMethods.changeTurtleRooms(path, atbRoomList, roomList, roomToZoneMap));

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failure");
            }
        }

        public static void loadInstancesApi(Model loadedModel, List<String> validRoomList, List<String> zoneList, List<String> roomList, Dictionary<String, String> roomToZoneMap, List<deviceObject> deviceObjectList, List<List<String>> predicates, IGraph g, MongoDbController db)
        {
            HttpResponseMessage response = TryCatchCoreFunctionsForVoid(() => CoreSensorMethods.loadInstances(loadedModel, validRoomList, zoneList, roomList, roomToZoneMap, deviceObjectList, predicates, g, db));

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failure");
            }
        }

        public static void getSubjectsAndObjectsApi(MongoDbController db, List<string> subjects, List<string> objects)
        {
            HttpResponseMessage response = TryCatchCoreFunctionsForVoid(() => CoreSensorMethods.getSubjectsAndObjects(db, subjects, objects));

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failure");
            }
        }

       /* public static List<string> getDeviceRelationshipApi(MongoDbController db, string selectedSubject, string selectedObject)
        {
            HttpResponseMessage response = TryCatchCoreFunctions<List<string>>(() => CoreSensorMethods.getDeviceRelationship(db, selectedSubject, selectedObject));

            List<string> output = new List<string>();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
                return output;
            }
            else
            {
                Console.WriteLine("Failure");

                return output;
            }
        }*/


        #endregion
    }
}