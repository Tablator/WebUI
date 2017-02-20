using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tablator.Presentation.Web.UI.Models.Configuration;
using Microsoft.Extensions.Options;
using Tablator.BusinessModel;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tablator.Presentation.Web.UI.Controllers
{
    /// <summary>
    /// Controller to deal with catalog
    /// </summary>
    public class CatalogController : Controller
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

        public CatalogController(
            ILoggerFactory loggerFactory
            , IOptions<CatalogSettings> catalogSettings)
        {
            _logger = loggerFactory.CreateLogger<TestController>();
            _catalogRootDirectory = catalogSettings.Value.RootDirectory;
        }

        #endregion

        [HttpGet]
        public IActionResult Index()
        {
            CatalogModel c = new CatalogModel();
            c.Load(_catalogRootDirectory);

            return View(c.GetArborescence());
        }
    }
}