using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class DeviceRelationValidatorController : Controller
    {
        // GET: DeviceRelationValidator

        SensorApiMethods sensorApiMethods;
        public DeviceRelationValidatorController()
        {
            sensorApiMethods = new SensorApiMethods();
        }

        public ActionResult Index()
        {
            return View();
        }

        public string Get([FromBody] string currentModel, String firstDevice, String secondDevice)
        {
            return sensorApiMethods.areDevicesRelated(currentModel, firstDevice, secondDevice);
        }
    }
}