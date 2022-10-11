using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class GetRelatedObjectsWithPredicateController : Controller
    {
        // GET: GetRelatedObjectsWithPredicate

        SensorApiMethods sensorApiMethods;
        public GetRelatedObjectsWithPredicateController()
        {
            sensorApiMethods = new SensorApiMethods();
        }

        public ActionResult Index()
        {
            return View();
        }
        public string Get([FromBody] string currentModel, string subjectName)
        {
            return sensorApiMethods.getRelatedObjectsWithPredicate(currentModel, subjectName);
        }
    }
}