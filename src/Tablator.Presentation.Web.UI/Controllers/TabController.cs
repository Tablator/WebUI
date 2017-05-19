namespace Tablator.Presentation.Web.UI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Tablator.Presentation.Web.UI.Models.Configuration;
    using Tablator.BusinessLogic.Services;
    using Tablator.Presentation.Web.UI.Models;
    using Tablator.Infrastructure.Enumerations;
    using Tablator.Infrastructure.Models;

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
        /// Service to deal with the catalog
        /// </summary>
        private readonly ICatalogService _catalogService;

        /// <summary>
        /// Service to deal with tablatures
        /// </summary>
        private readonly ITablatureService _tabService;

        /// <summary>
        /// Chords catalog's root directory
        /// </summary>
        private readonly string _chordCatalogRootDirectory;

        public TabController(
            ILoggerFactory loggerFactory
            , IOptions<ChordCatalogSettings> chordCatalogSettings
            , ICatalogService catalogService
            , ITablatureService tabService)
        {
            _logger = loggerFactory.CreateLogger<TabController>();
            _chordCatalogRootDirectory = chordCatalogSettings.Value.RootDirectory;
            _catalogService = catalogService;
            _tabService = tabService;
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> Get(string urlPath)
        {
            if (string.IsNullOrWhiteSpace(urlPath))
                throw new Exception();

            Guid? id = await _catalogService.GetTablatureId(urlPath);
            if (!id.HasValue || id.Value == Guid.Empty)
                throw new Exception();

            BusinessModel.Tablature.IInstrumentTablature tab = _tabService.Get(id.Value, InstrumentEnum.Guitar);
            // todo : load SVG Builder by reflection (Tablatore.Rendering.SVG dll)
            // get svg output and return it
            TablatureRenderingOptions opts = new TablatureRenderingOptions();
            opts.Width = 890;
            Tablator.Rendering.Core.ITablatureRenderingBuilder tabRenderingBuilder = new Tablator.Rendering.SVG.RenderingBuilder();
            string svg = string.Empty;
            TabGenerationStatus status = tabRenderingBuilder.BuildOutputContent(tab, opts, out svg);
            return View(new TabViewModel(svg));
        }
    }
}