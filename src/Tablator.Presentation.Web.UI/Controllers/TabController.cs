using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tablator.Presentation.Web.UI.Models.Configuration;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tablator.Presentation.Web.UI.Controllers
{
    /// <summary>
    /// Controller to deal with tablatures
    /// </summary>
    public class TabController : Controller
    {
        #region Ctor & Pprts

        /// <summary>
        /// Service to perform logging
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Catalog's root directory
        /// </summary>
        private readonly string _catalogRootDirectory;

        public TabController(
            ILoggerFactory loggerFactory
            , IOptions<CatalogSettings> catalogSettings)
        {
            _logger = loggerFactory.CreateLogger<TabController>();
            _catalogRootDirectory = catalogSettings.Value.RootDirectory;
        }

        #endregion

        [HttpGet]
        public IActionResult Get()
        {
            return View();
        }
    }
}