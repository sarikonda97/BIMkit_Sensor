using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class GetBuildingModelsController : Controller
    {
        // GET: GetBuildingModels

        SensorApiMethods sensorApiMethods;
        public GetBuildingModelsController()
        {
            sensorApiMethods = new SensorApiMethods();
        }

        public ActionResult Index()
        {
            return View();
        }

        public string Get()
        {
            return sensorApiMethods.getAvailableModels();
        }
    }
}