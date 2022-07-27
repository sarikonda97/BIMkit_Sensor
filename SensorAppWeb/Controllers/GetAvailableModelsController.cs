using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class GetAvailableModelsController : Controller
    {
        // GET: GetAvailableModels

        SensorApiMethods sensorApiMethods;
        public GetAvailableModelsController()
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