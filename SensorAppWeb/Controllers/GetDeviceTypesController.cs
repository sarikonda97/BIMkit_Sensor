using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class GetDeviceTypesController : Controller
    {
        // GET: GetDeviceTypes

        SensorApiMethods sensorApiMethods;
        public GetDeviceTypesController()
        {
            sensorApiMethods = new SensorApiMethods();
        }

        public ActionResult Index()
        {
            return View();
        }

        public string Get([FromBody] String currentModel)
        {
            return sensorApiMethods.GetDeviceTypes(currentModel);
        }
    }
}