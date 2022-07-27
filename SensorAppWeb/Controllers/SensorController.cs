using Newtonsoft.Json;
using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class SensorController : Controller
    {
        SensorApiMethods sensorApiMethods;

        public SensorController()
        {
            sensorApiMethods = new SensorApiMethods();
        }

        // GET: Sensor
        public ActionResult Index()
        {
            return View();
        }

        public string Get()
        {
            var temp = new
            {
                response = "hello world"
            };

            string jsonData = JsonConvert.SerializeObject(temp);

            return jsonData;
        }

        public string Post([FromBody] String ttlPath, String modelPath)
        {
            string status = sensorApiMethods.LoadFiles(ttlPath, modelPath);

            var statusObject = new
            {
                response = status
            };

            return JsonConvert.SerializeObject(statusObject);
        }
    }
}