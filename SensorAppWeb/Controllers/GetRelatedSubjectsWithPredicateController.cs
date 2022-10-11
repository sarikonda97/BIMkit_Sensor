using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class GetRelatedSubjectsWithPredicateController : Controller
    {
        // GET: GetRelatedSubjectsWithPredicate

        SensorApiMethods sensorApiMethods;
        public GetRelatedSubjectsWithPredicateController()
        {
            sensorApiMethods = new SensorApiMethods();
        }

        public ActionResult Index()
        {
            return View();
        }
        public string Get([FromBody] string currentModel, string objectName)
        {
            return sensorApiMethods.getRelatedSubjectsWithPredicate(currentModel, objectName);
        }
    }
}