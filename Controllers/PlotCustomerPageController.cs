using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using VGN_CRM_CORE.Models;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace VGN_CRM_CORE.Controllers
{
    public class PlotCustomerPageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
