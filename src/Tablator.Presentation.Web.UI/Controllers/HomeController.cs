using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tablator.Presentation.Web.UI.Models.Configuration;
using Newtonsoft.Json;
using System.IO;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Reflection;
using Tablator.BusinessModel;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tablator.Presentation.Web.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;

        private readonly string _catalogRootDirectory;

        public HomeController(
            ILoggerFactory loggerFactory
            , IOptions<CatalogSettings> catalogSettings)
        {
            _logger = loggerFactory.CreateLogger<HomeController>();
            _catalogRootDirectory = catalogSettings.Value.RootDirectory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}