﻿using SensorAppWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SensorAppWeb.Controllers
{
    public class GetRelationshipPathController : Controller
    {
        // GET: GetRelationshipPath

        SensorApiMethods sensorApiMethods;
        public GetRelationshipPathController()
        {
            sensorApiMethods = new SensorApiMethods();
        }
        public ActionResult Index()
        {
            return View();
        }

        public string Get([FromBody] string currentModel, string sourceSubject, string targetObject)
        {
            return sensorApiMethods.getRelationshipPath(currentModel, sourceSubject, targetObject);
        }
    }
}