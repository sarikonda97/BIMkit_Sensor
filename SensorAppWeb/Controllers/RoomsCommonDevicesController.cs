using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class RoomsCommonDevicesController : Controller
    {
        // GET: RoomsCommonDevices

        SensorApiMethods sensorApiMethods;
        public RoomsCommonDevicesController()
        {
            sensorApiMethods = new SensorApiMethods();
        }

        public ActionResult Index()
        {
            return View();
        }

        public string Get([FromBody] string currentModel, string firstRoom, string secondRoom)
        {
            return sensorApiMethods.getRoomCommonDevices(currentModel, firstRoom, secondRoom);
        }
    }
}