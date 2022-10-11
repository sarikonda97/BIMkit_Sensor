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
    public class GetKnownRelationshipTypesController : Controller
    {
        // GET: GetKnownRelationshipTypes

        SensorApiMethods sensorApiMethods;
        public GetKnownRelationshipTypesController()
        {
            sensorApiMethods = new SensorApiMethods();
        }

        public ActionResult Index()
        {
            return View();
        }

        public string Get([FromBody] String subjectDeviceType, String objectDeviceType)
        {
            return sensorApiMethods.getValidRelationships(subjectDeviceType, objectDeviceType);

        }
    }
}