using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class GetRelatedDevicesController : Controller
    {
        // GET: GetRelatedDevices

        SensorApiMethods sensorApiMethods;
        public GetRelatedDevicesController()
        {
            sensorApiMethods = new SensorApiMethods();
        }

        public ActionResult Index()
        {
            return View();
        }

        public string Get([FromBody] string currentModel, String deviceName)
        {
            return sensorApiMethods.getRelatedDevices(currentModel, deviceName);

        }
    }
}