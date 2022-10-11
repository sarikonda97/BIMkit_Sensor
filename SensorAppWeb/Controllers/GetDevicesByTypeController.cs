using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class GetDevicesByTypeController : Controller
    {
        // GET: GetDevicesByType

        SensorApiMethods sensorApiMethods;
        public GetDevicesByTypeController()
        {
            sensorApiMethods = new SensorApiMethods();
        }

        public ActionResult Index()
        {
            return View();
        }

        public string Get([FromBody] String currentModel, string deviceType)
        {
            return sensorApiMethods.GetDevicesByType(currentModel, deviceType);
        }
    }
}