using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class GetRoomDevicesController : Controller
    {
        // GET: GetRoomDevices

        SensorApiMethods sensorApiMethods;
        public GetRoomDevicesController()
        {
            sensorApiMethods = new SensorApiMethods();
        }
        public ActionResult Index()
        {
            return View();
        }

        public string Get([FromBody] string currentModel, String roomName)
        {
            return sensorApiMethods.getRoomDevices(currentModel, roomName);
        }
    }
}