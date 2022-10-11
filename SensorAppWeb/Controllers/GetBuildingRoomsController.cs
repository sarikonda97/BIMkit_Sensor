using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class GetBuildingRoomsController : Controller
    {
        // GET: GetBuildingRooms

        SensorApiMethods sensorApiMethods;
        public GetBuildingRoomsController()
        {
            sensorApiMethods = new SensorApiMethods();
        }

        public ActionResult Index()
        {
            return View();
        }

        public string Get([FromBody] String currentModel)
        {
            return sensorApiMethods.getRooms(currentModel);
        }
    }
}