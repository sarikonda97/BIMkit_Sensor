using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class GetDirectRelationshipsWithRelationshipTypeController : Controller
    {
        // GET: GetDirectRelationshipsWithRelationshipType

        SensorApiMethods sensorApiMethods;
        public GetDirectRelationshipsWithRelationshipTypeController()
        {
            sensorApiMethods = new SensorApiMethods();
        }
        public ActionResult Index()
        {
            return View();
        }

        public string Get([FromBody] string currentModel, String firstDevice)
        {
            return sensorApiMethods.getDirectRelationshipsWithRelationshipType(currentModel, firstDevice);
        }
    }
}