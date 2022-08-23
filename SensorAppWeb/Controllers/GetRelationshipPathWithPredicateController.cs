using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class GetRelationshipPathWithPredicateController : Controller
    {
        // GET: GetRelationshipPathWithPredicate

        SensorApiMethods sensorApiMethods;
        public GetRelationshipPathWithPredicateController()
        {
            sensorApiMethods = new SensorApiMethods();
        }

        public ActionResult Index()
        {
            return View();
        }

        public string Get([FromBody] string currentModel, String firstDevice, string secondDevice)
        {
            return sensorApiMethods.getRelationshipPathWithPredicate(currentModel, firstDevice, secondDevice);
        }
    }
}