using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class GetRelatedObjectsController : Controller
    {
        // GET: GetRelatedObjects

        SensorApiMethods sensorApiMethods;
        public GetRelatedObjectsController()
        {
            sensorApiMethods = new SensorApiMethods();
        }

        public ActionResult Index()
        {
            return View();
        }

        public string Get([FromBody] string currentModel, string subjectName)
        {
            return sensorApiMethods.getRelatedObjects(currentModel, subjectName);
        }
    }
}