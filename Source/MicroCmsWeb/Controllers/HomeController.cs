﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using MicroCms;

namespace MicroCms.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(Cms.GetArea().Documents.GetByPath("home"));
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}