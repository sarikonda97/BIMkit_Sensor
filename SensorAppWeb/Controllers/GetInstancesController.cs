using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class GetInstancesController : Controller
    {
        // GET: GetInstances

        SensorApiMethods sensorApiMethods;
        public GetInstancesController()
        {
            sensorApiMethods = new SensorApiMethods();
        }

        public ActionResult Index()
        {
            return View();
        }

        public string Get([FromBody] string currentModel)
        {
            return sensorApiMethods.getInstancesFromModel(currentModel);
        }
    }
}