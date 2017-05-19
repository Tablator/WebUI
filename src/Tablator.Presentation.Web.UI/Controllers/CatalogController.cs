namespace Tablator.Presentation.Web.UI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Tablator.Presentation.Web.UI.Models.Configuration;
    using Microsoft.Extensions.Options;
    using Tablator.BusinessModel;
    using BusinessLogic.Services;

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

        /// <summary>
        /// Service to deal with the catalog
        /// </summary>
        private readonly ICatalogService _catalogService;

        /// <summary>
        /// New instance of catalog controller
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="catalogSettings"></param>
        /// <param name="catalogService"></param>
        public CatalogController(
            ILoggerFactory loggerFactory
            , ICatalogService catalogService)
        {
            _logger = loggerFactory.CreateLogger<TestController>();
            _catalogService = catalogService;
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> Index() => View((await _catalogService.GetCatalog()).GetArborescence());
    }
}