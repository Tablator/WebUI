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
using Tablator.Infrastructure.Enumerations;
using Tablator.Infrastructure.Extensions;
using Tablator.Infrastructure.Constants;
using Tablator.BusinessLogic.Services;
using Tablator.Infrastructure.Models;
using Tablator.Presentation.Web.UI.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tablator.Presentation.Web.UI.Controllers
{
    public class TestController : Controller
    {
        private readonly ILogger _logger;

        private readonly string _catalogRootDirectory;

        private readonly ICatalogService _catalogService;

        private readonly ITablatureService _tabService;

        public TestController(
            ILoggerFactory loggerFactory
            , IOptions<CatalogSettings> catalogSettings
            , ICatalogService catalogService
            , ITablatureService tabService)
        {
            _logger = loggerFactory.CreateLogger<TestController>();
            _catalogRootDirectory = catalogSettings.Value.RootDirectory;
            _catalogService = catalogService;
            _tabService = tabService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            HierarchyCollectionModel hiers = (await _catalogService.GetCatalog()).GetArborescence();
            HierarchyTabReferenceCollectionModel tabs = await _catalogService.ListReferences();
            hiers.PopulateWithTablatures(ref tabs);
            tabs = null;
            return View(hiers);
        }

        [HttpGet]
        public async Task<IActionResult> Tab01()
        {
            BusinessModel.Tablature.IInstrumentTablature tab = _tabService.Get(new Guid("4BBD4A605EDF40B2BF917687E3A94755"), InstrumentEnum.Guitar);

            // todo : load SVG Builder by reflection (Tablatore.Rendering.SVG dll)
            // get svg output and return it

            TablatureRenderingOptions opts = new TablatureRenderingOptions();
            opts.Width = 890;

            Tablator.Rendering.Core.ITablatureRenderingBuilder tabRenderingBuilder = new Tablator.Rendering.SVG.RenderingBuilder();
            string svg = string.Empty;
            TabGenerationStatus status = tabRenderingBuilder.BuildOutputContent(tab, opts, out svg);
            return View("Tab02", new TabViewModel(svg));
        }
    }

    #region chord

    public class GuitarChordManager
    {
        private readonly string Directory = null;

        private ChordCollection CommonChords { get; set; }

        private ChordCollection UncommonChords { get; set; }

        private bool CommonChordsLoaded { get; set; } = false;

        private bool UncommonChordsLoaded { get; set; } = false;

        public GuitarChordManager()
        {
            Directory = @"C:\Users\a.torris\Documents\Visual Studio 2015\Projects\WebApplication1\src\WebApplication3\catalog\chords\1";
            LoadCommonChords();
        }

        private void LoadCommonChords()
        {
            string jsonString = string.Empty;
            string filepath = Path.Combine(Directory, "commons.chord");

            if (!System.IO.File.Exists(filepath))
                throw new FileNotFoundException($"filename custom mapping file doesn't exists");

            using (StreamReader sr = new StreamReader(new FileStream(filepath, FileMode.Open)))
            {
                jsonString = sr.ReadToEnd();
            }
            CommonChords = Newtonsoft.Json.JsonConvert.DeserializeObject<ChordCollection>(jsonString);
            CommonChordsLoaded = true;
        }
    }

    public class GuitarChord : Chord
    {
        [JsonProperty(PropertyName = "capo")]
        public int Capo { get; }

        private string _Composition;

        public override string Composition
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_Composition))
                {
                    _Composition = string.Empty;
                    foreach (KeyValuePair<int, int?> kvp in Positions.OrderBy(x => x.Key))
                        _Composition += kvp.Value + ChordConstants.CompositionSeparator;
                    _Composition += Capo;
                }

                return _Composition;
            }
        }

        public GuitarChord()
        {

        }
    }

    public class GuitarChordCollection : List<GuitarChord>
    {
        public GuitarChordCollection()
        { }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Chord
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; }

        [JsonProperty(PropertyName = "key")]
        public char Key { get; }

        [JsonProperty(PropertyName = "positions")]
        public Dictionary<int, int?> Positions { get; }

        public virtual string Composition { get; }
    }

    public class ChordCollection : List<Chord>
    {
        public ChordCollection()
        { }
    }



    #endregion
}