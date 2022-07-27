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
    public class ValidRelationshipsController : Controller
    {
        // GET: ValidRelationships

        SensorApiMethods sensorApiMethods;
        public ValidRelationshipsController()
        {
            sensorApiMethods = new SensorApiMethods();
        }

        public ActionResult Index()
        {
            return View();
        }

        public string Get([FromBody] String subjectName, String objectName)
        {
            return sensorApiMethods.getValidRelationships(subjectName, objectName);

        }
    }
}