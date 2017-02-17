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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tablator.Presentation.Web.UI.Controllers
{
    public class TestController : Controller
    {
        private readonly ILogger _logger;

        private readonly string _catalogRootDirectory;

        public TestController(
            ILoggerFactory loggerFactory
            , IOptions<CatalogSettings> catalogSettings)
        {
            _logger = loggerFactory.CreateLogger<TestController>();
            _catalogRootDirectory = catalogSettings.Value.RootDirectory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            CatalogModel c = new CatalogModel();
            c.Load(_catalogRootDirectory);

            return View(c.GetArborescence());
        }

        [HttpGet]
        public IActionResult Tab01()
        {
            string json = string.Empty;
            Newtonsoft.Json.Linq.JObject o2;


            using (System.IO.StreamReader file = System.IO.File.OpenText(System.IO.Path.Combine(_catalogRootDirectory, "01.tab")))
            {
                using (JsonTextReader rdr = new JsonTextReader(file))
                {
                    o2 = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.Linq.JToken.ReadFrom(rdr);
                    json = o2.ToString();
                }
            }

            Tablature tab = JsonConvert.DeserializeObject<Tablature>(json);

            SVGTabGenerator tabGenerator = new SVGTabGenerator(tab, null, null);
            if (tabGenerator.TryBuild())
                return View(new TabViewModel(tabGenerator.SVGContent));

            return Content(json, "text/json");
        }

        [HttpGet]
        public IActionResult Chord01()
        {
            GuitarChordManager gcm = new Controllers.GuitarChordManager();
            return Content(null, "text/json");
        }

        [HttpGet]
        public IActionResult Tab02([FromQuery]int width)
        {
            string json = string.Empty;
            Newtonsoft.Json.Linq.JObject o2;


            using (System.IO.StreamReader file = System.IO.File.OpenText(System.IO.Path.Combine(_catalogRootDirectory, "01.tab")))
            {
                using (JsonTextReader rdr = new JsonTextReader(file))
                {
                    o2 = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.Linq.JToken.ReadFrom(rdr);
                    json = o2.ToString();
                }
            }


            Tablature tab = JsonConvert.DeserializeObject<Tablature>(json);

            SVGTabOptions opts = new SVGTabOptions();
            opts.Width = width;

            SVGTabGenerator tabGenerator = new SVGTabGenerator(tab, opts, null);
            if (tabGenerator.TryBuild())
                return View(new TabViewModel(tabGenerator.SVGContent));

            return Content(json, "text/json");
        }
    }

    public class TabViewModel
    {
        public string SVGContent { get; set; }

        public TabViewModel(string svgContent)
        {
            SVGContent = svgContent;
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

    public class ChordCollection : List<Chord>
    {
        public ChordCollection()
        { }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Chord
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; }

        [JsonProperty(PropertyName = "key")]
        public char Key { get; }

        [JsonProperty(PropertyName = "capo")]
        public int Capo { get; }

        [JsonProperty(PropertyName = "positions")]
        public Dictionary<int, int?> Positions { get; }

        private string _Composition;

        public string Composition
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
    }

    

    public class GuitarChordDrawer
    {
        private string SVGContent { get; set; } = string.Empty;

        //public int cursorHeight { get; set; } = 0;

        //public int cursorWith { get; set; } = 0;

        public GuitarChordDrawerOptions Options { get; }

        public GuitarChordDrawer(GuitarChordDrawerOptions options = null)
        {
            Options = options == null ? new GuitarChordDrawerOptions() : options;
        }

        public bool DrawChord(string chord, out string svg, int cursorWidth = 0, int cursorHeight = 0)
        {
            SVGContent = string.Empty;
            svg = string.Empty;

            if (Options.DisplayLabel)
            {
                // Nom accord
                SVGContent += "<text x=\"" + (cursorWidth + (Options.Width / 2)) + "\" y=\"" + (cursorHeight + 5) + "\" font-family=\"" + Options.Typeface + "\" font-size=\"17\" fill=\"black\" text-anchor=\"middle\">" + chord + "</text>";
            }

            // ligne haut

            SVGContent += "<line x1=\"" + (cursorWidth + 5) + "\" y1=\"" + (cursorHeight + 25) + "\" x2=\"" + (cursorWidth + 105) + "\" y2=\"" + (cursorHeight + 25) + "\" stroke=\"" + Options.StringColor + "\" stroke-width=\"3\" fill=\"" + Options.StringColor + "\"></line>";

            // lignes guitare

            for (int i = 0; i < 6; i++)
            {
                SVGContent += "<line x1=\"" + (cursorWidth + 5 + i * 20) + "\" y1=\"" + (cursorHeight + 25) + "\" x2=\"" + (cursorWidth + 5 + i * 20) + "\" y2=\"" + (cursorHeight + 145) + "\" stroke=\"" + Options.StringColor + "\" stroke-width=\"1\" fill=\"" + Options.StringColor + "\"></line>";
            }

            // Frets
            
            for (int i = 0; i < 4; i++)
            {
                SVGContent += "<line x1=\"" + (cursorWidth + 5) + "\" y1=\"" + (cursorHeight + 55 + i * 30) + "\" x2=\"" + (cursorWidth + 105) + "\" y2=\"" + (cursorHeight + 55 + i * 30) + "\" stroke=\"" + Options.StringColor + "\" stroke-width=\"1\" fill=\"" + Options.StringColor + "\"></line>";
            }

            try
            {
                switch (chord)
                {
                    case "eeer":
                        // Capo
                        // Finger positions
                        // Free strings
                        return false;
                    case "C":
                        AddFingersPositions(GuitarChordEnum.C.GetDisplayDescription(), cursorWidth, cursorHeight);
                        break;
                    case "A":
                        AddFingersPositions(GuitarChordEnum.A.GetDisplayDescription(), cursorWidth, cursorHeight);
                        break;
                    case "G":
                        AddFingersPositions(GuitarChordEnum.G.GetDisplayDescription(), cursorWidth, cursorHeight);
                        break;
                    case "Am":
                        AddFingersPositions(GuitarChordEnum.Am.GetDisplayDescription(), cursorWidth, cursorHeight);
                        break;
                    default:
                        // Fonctionne mais statique

                        // Capo
                        // Finger positions
                        //if (Options.DisplayFretNumberInFingerPositions)
                        //    SVGContent += "<g><circle cx=\"" + (cursorWidth + 66) + "\" cy=\"" + (cursorHeight + 40) + "\" r=\"8\" stroke=\"" + Options.FingerPositionBackgroundColor + "\" stroke-width=\"0\" fill=\"" + Options.FingerPositionBackgroundColor + "\" /><text x=\"" + (cursorWidth + 66) + "\" y=\"" + (cursorHeight + 44) + "\" font-family=\"" + Options.Typeface + "\" font-size=\"12\" fill=\"" + Options.FingerPositionTextColor + "\" text-anchor=\"middle\">1</text></g>";
                        //else
                        //    SVGContent += "<circle cx=\"" + (cursorWidth + 66) + "\" cy=\"" + (cursorHeight + 40) + "\" r=\"8\" stroke=\"" + Options.FingerPositionBackgroundColor + "\" stroke-width=\"0\" fill=\"" + Options.FingerPositionBackgroundColor + "\" />";
                        //SVGContent += "<g><circle cx=\"" + (cursorWidth + 45) + "\" cy=\"" + (cursorHeight + 70) + "\" r=\"8\" stroke=\"" + Options.FingerPositionBackgroundColor + "\" stroke-width=\"0\" fill =\"" + Options.FingerPositionBackgroundColor + "\" /><text x=\"" + (cursorWidth + 45) + "\" y=\"" + (cursorHeight + 74) + "\" font-family=\"" + Options.Typeface + "\" font-size=\"12\" fill=\"" + Options.FingerPositionTextColor + "\" text-anchor=\"middle\">2</text></g>";
                        //SVGContent += "<g><circle cx=\"" + (cursorWidth + 25) + "\" cy=\"" + (cursorHeight + 70) + "\" r=\"8\" stroke=\"" + Options.FingerPositionBackgroundColor + "\" stroke-width=\"0\" fill=\"" + Options.FingerPositionBackgroundColor + "\" /><text x=\"" + (cursorWidth + 25) + "\" y=\"" + (cursorHeight + 74) + "\" font-family=\"" + Options.Typeface + "\" font-size=\"12\" fill=\"" + Options.FingerPositionTextColor + "\" text-anchor=\"middle\">2</text></g>";
                        //// Free strings
                        //SVGContent += "<circle cx=\"" + (cursorWidth + 85) + "\" cy=\"" + (cursorHeight + 15) + "\" r=\"5\" stroke=\"" + Options.FingerPositionBackgroundColor + "\" stroke-width=\"1\" fill=\"" + Options.PlayedFreeStringColor + "\" />";
                        //SVGContent += "<circle cx=\"" + (cursorWidth + 105) + "\" cy=\"" + (cursorHeight + 15) + "\" r=\"5\" stroke=\"" + Options.FingerPositionBackgroundColor + "\" stroke-width=\"1\" fill=\"" + Options.PlayedFreeStringColor + "\" />";
                        //SVGContent += "<text x=\"" + (cursorWidth + 2) + "\" y=\"" + (cursorHeight + 17) + "\" font-family=\"" + Options.Typeface + "\" font-size=\"13\" fill =\"" + Options.MutedFreeStringColor + "\" text-anchor=\"start\">x</text>";

                        // Fonctionne et dynamique
                        AddFingersPositions(GuitarChordEnum.A.GetDisplayDescription(), cursorWidth, cursorHeight);

                        break;
                        //default:
                        //    // Capo
                        //    // Finger positions
                        //    // Free strings
                        //    return false;
                }

                svg = SVGContent;
                return true;
            }
            catch (Exception)
            {

            }
            finally
            {
                SVGContent = string.Empty;
            }

            return false;
        }

        private void GenerateGrille()
        {

        }

        private void AddFingersPositions(string chordComposition, int cursorWidth, int cursorHeight)
        {
            string[] strings = chordComposition.Split(new char[] { ChordConstants.CompositionSeparator }, StringSplitOptions.None);

            if (strings.Length != 7)
                throw new Exception();

            for (int i = 0; i < 6; i++)
            {
                if (string.IsNullOrWhiteSpace(strings[i]))
                {
                    // corde mutée
                    SVGContent += "<text x=\"" + (cursorWidth + 2 + i * 20) + "\" y=\"" + (cursorHeight + 17) + "\" font-family=\"" + Options.Typeface + "\" font-size=\"13\" fill =\"" + Options.MutedFreeStringColor + "\" text-anchor=\"start\">x</text>";
                    continue;
                }

                int posi = Convert.ToInt32(strings[i]);
                if (posi == 0)
                {
                    SVGContent += "<circle cx=\"" + (cursorWidth + 5 + i * 20) + "\" cy=\"" + (cursorHeight + 15) + "\" r=\"5\" stroke=\"" + Options.FingerPositionBackgroundColor + "\" stroke-width=\"1\" fill=\"" + Options.PlayedFreeStringColor + "\" />";
                }
                else if (posi > 0)
                {
                    if (Options.DisplayFretNumberInFingerPositions)
                        SVGContent += "<g><circle cx=\"" + (cursorWidth + 5 + i * 20) + "\" cy=\"" + (cursorHeight + 10 + posi * 30) + "\" r=\"8\" stroke=\"" + Options.FingerPositionBackgroundColor + "\" stroke-width=\"0\" fill=\"" + Options.FingerPositionBackgroundColor + "\" /><text x=\"" + (cursorWidth + 5 + i * 20) + "\" y=\"" + (cursorHeight + 10 + 4 + posi * 30) + "\" font-family=\"" + Options.Typeface + "\" font-size=\"12\" fill=\"" + Options.FingerPositionTextColor + "\" text-anchor=\"middle\">" + posi + "</text></g>";
                    else
                        SVGContent += "<circle cx=\"" + (cursorWidth + 5 + i * 20) + "\" cy=\"" + (cursorHeight + 10 + posi * 30) + "\" r=\"8\" stroke=\"" + Options.FingerPositionBackgroundColor + "\" stroke-width=\"0\" fill=\"" + Options.FingerPositionBackgroundColor + "\" />";
                }
                else
                {
                    throw new Exception();
                }
            }

            int capo = Convert.ToInt32(strings[6]);
            if (capo > 0)
            {

            }
        }
    }

    public class GuitarChordDrawerOptions
    {
        public int Width { get; } = 130;

        public int Height { get; }

        public bool DisplayLabel { get; } = true;

        /// <summary>
        /// Affiche les numéros de fret dans les positions des doigts
        /// </summary>
        public bool DisplayFretNumberInFingerPositions { get; } = true;

        /// <summary>
        /// Couleur des cordes et des frets
        /// </summary>
        public string StringColor { get; set; } = "rgb(20, 20, 20)";

        public string FingerPositionBackgroundColor { get; set; } = "rgb(30, 30, 30)";

        public string FingerPositionTextColor { get; set; } = "rgb(255, 255, 255)";

        /// <summary>
        /// Couleur des cercles indiquant qu'une corde vide doit être jouée
        /// </summary>
        public string PlayedFreeStringColor { get; set; } = "rgb(255, 255, 255)";

        /// <summary>
        /// Couleur du cigle indiquant qu'une corde vide ne doit pas être jouée
        /// </summary>
        public string MutedFreeStringColor { get; set; } = "rgb(0, 0, 0)";

        public string Typeface { get; set; } = "Verdana";

        public GuitarChordDrawerOptions()
        {
            Height = Convert.ToInt32(Width * 1.4);
        }
    }

    #endregion

    

    public class SVGTabGenerator
    {
        public TabGenerationStatus Status { get; private set; }

        public string SVGContent { get; private set; } = string.Empty;

        private Tablature Tablature { get; }

        private SVGTabOptions Options { get; }

        private CultureInfo Culture { get; }

        public SVGTabGenerator(Tablature tab, SVGTabOptions options, CultureInfo culture)
        {
            Tablature = tab;
            Options = options == null ? new SVGTabOptions() : options;
            Culture = culture == null ? CultureInfo.CurrentUICulture : culture;
        }

        public int cursorWith { get; set; } = 0;

        public int cursorHeight { get; set; } = 20;

        public int svgHeight { get; set; } = 20;

        public bool TryBuild()
        {
            try
            {
                // en-tête du document

                if (!string.IsNullOrWhiteSpace(Tablature.SongName))
                {
                    cursorHeight += 8;
                    svgHeight += 8;
                    SVGContent += "<text x=\"50%\" y=\"" + cursorHeight + "\" font-family=\"" + Options.Typeface + "\" font-size=\"30\" text-anchor=\"middle\">" + Tablature.SongName + "</text>";
                    cursorHeight += 30;
                    svgHeight += 30;
                }

                if (!string.IsNullOrWhiteSpace(Tablature.ArtistName))
                {
                    cursorHeight += 5;
                    svgHeight += 5;
                    SVGContent += "<text x=\"50%\" y=\"" + cursorHeight + "\" font-family=\"" + Options.Typeface + "\" font-size=\"15\" text-anchor=\"middle\" font-style=\"italic\">" + Tablature.ArtistName + "</text>";
                    cursorHeight += 15;
                    svgHeight += 15;
                }

                if (Tablature.Capodastre > 0)
                {
                    cursorHeight += 5;
                    svgHeight += 5;
                    SVGContent += "<text x=\"0\" y=\"" + cursorHeight + "\" font-family=\"" + Options.Typeface + "\" font-size=\"12\" text-anchor=\"left\">Capodastre: " + Tablature.Capodastre + "</text>";
                    cursorHeight += 15;
                    svgHeight += 15;
                }

                if (!string.IsNullOrWhiteSpace(Tablature.Tuning))
                {
                    cursorHeight += 5;
                    svgHeight += 5;
                    SVGContent += "<text x=\"0\" y=\"" + cursorHeight + "\" font-family=\"" + Options.Typeface + "\" font-size=\"12\" text-anchor=\"left\">Tuning: " + Tablature.Tuning + "</text>";
                    cursorHeight += 15;
                    svgHeight += 15;
                }

                if (Tablature.Tempo > 0)
                {
                    cursorHeight += 5;
                    svgHeight += 5;
                    SVGContent += "<text x=\"0\" y=\"" + cursorHeight + "\" font-family=\"" + Options.Typeface + "\" font-size=\"12\" text-anchor=\"left\">Tempo: " + Tablature.Tempo + "</text>";
                    cursorHeight += 15;
                    svgHeight += 15;
                }

                if (Options.DisplayEnchainement && Tablature.Enchainement != null && Tablature.Enchainement.Count > 0)
                {
                    cursorHeight += 5;
                    svgHeight += 5;

                    if (!Options.AffichageEnchainementDetaille.HasValue || !Options.AffichageEnchainementDetaille.Value)
                    {
                        // Affichage simple
                        SVGContent += "<text x=\"0\" y=\"" + cursorHeight + "\" font-family=\"" + Options.Typeface + "\" font-size=\"12\" text-anchor=\"left\">Enchaînement: ";
                        foreach (EnchainementItem ei in Tablature.Enchainement)
                        {
                            SVGContent += "(" + Tablature.GetPartName(ei.PartieId, Culture) + " x" + ei.Repeat + ") ";
                        }
                        SVGContent += "</text>";
                    }
                    else
                    {
                        // Affichage détaillé
                        SVGContent += "<text x=\"0\" y=\"" + cursorHeight + "\" font-family=\"" + Options.Typeface + "\" font-size=\"12\" text-anchor=\"left\">Enchaînement:</text>";
                        foreach (EnchainementItem ei in Tablature.Enchainement)
                        {
                            cursorHeight += 15;
                            svgHeight += 15;
                            SVGContent += "<text x=\"20\" y=\"" + cursorHeight + "\" font-family=\"" + Options.Typeface + "\" font-size=\"12\" text-anchor=\"left\">- " + Tablature.GetPartName(ei.PartieId, Culture) + " x" + ei.Repeat + "</text>";
                        }
                    }

                    cursorHeight += 15;
                    svgHeight += 15;
                }

                // Accord d'en-tête

                if (Options.DisplayChordsHeader && Tablature.Accords != null && Tablature.Accords.Count > 0)
                {
                    GuitarChordDrawerOptions chordOptions = new Controllers.GuitarChordDrawerOptions();
                    GuitarChordDrawer chordDrawer = new GuitarChordDrawer(options: chordOptions);

                    cursorWith = 0;
                    cursorHeight += 10;
                    svgHeight += 10;

                    //for (int i = 0; i < Tablature.Accords.Count; i++)
                    //{
                    //    string chordSVG = string.Empty;

                    //    if (chordDrawer.DrawChord(Tablature.Accords[i], out chordSVG))
                    //        SVGContent += chordSVG;

                    //    chordSVG = null;
                    //}

                    int i = 0;
                    foreach (string s in Tablature.Accords)
                    {
                        cursorWith += 5;

                        if (i == 0)
                        {
                            // New line
                        }

                        string chordSVG = string.Empty;

                        if (chordDrawer.DrawChord(Tablature.Accords[i], out chordSVG, cursorWidth: cursorWith, cursorHeight: cursorHeight))
                            SVGContent += chordSVG;

                        cursorWith += chordOptions.Width + 10;

                        chordSVG = null;

                        //if (i >= (Options.Width - (10)) / chordOptions.Width)
                        //{
                        //    // New line

                        //    i = 0;

                        //    cursorHeight += 5 + chordOptions.Height;
                        //    svgHeight += 5 + chordOptions.Height;
                        //}
                        //else
                        //    i++;

                        if ((Options.Width - cursorWith) < chordOptions.Width + 10)
                        {
                            // New Line

                            i = 0;
                            cursorHeight += chordOptions.Height;
                            svgHeight += chordOptions.Height;
                            cursorWith = 0;
                        }
                        else
                        {
                            i++;
                        }
                    }

                    // On passe la ligne en cours

                    cursorHeight += chordOptions.Height;
                    svgHeight += chordOptions.Height;
                    cursorWith = 0;

                    chordOptions = null;
                    chordDrawer = null;
                }

                // contenu tab

                cursorWith = 0;

                foreach (Partie part in Tablature.Parties)
                {
                    if (!string.IsNullOrWhiteSpace(Tablature.GetPartName(part.Id, Culture)))
                    {
                        cursorHeight += 30;
                        SVGContent += "<text x=\"0\"  y=\"" + cursorHeight + "\" font-family=\"" + Options.Typeface + "\" font-size=\"15\">" + Tablature.GetPartName(part.Id, Culture) + "</text>";
                        cursorHeight += 15;
                        cursorHeight += 5;
                        svgHeight += 50;
                    }

                    // On crée une ligne vide de tab
                    CreateNewLine();

                    // Et on mets les notes

                    int iMesures = 0;

                    // Fonctionne mais avec des lignes verticales en trop (car plus assez d'espace après pour caler des temps, donc moche)
                    //foreach (Mesure mes in part.Mesures)
                    //{
                    //    iMesures++;

                    //    foreach (Temps tmp in mes.Temps)
                    //    {
                    //        foreach (Son s in tmp.Sons)
                    //        {
                    //            if (cursorWith >= (Options.Width - 20)) // 20 = largeur d'une note à peu près, avec marges côtés
                    //            {
                    //                cursorHeight += Options.StringSpacing * 6 + 20;
                    //                CreateNewLine();
                    //            }

                    //            if (s.Type == TypeSonEnum.Note)
                    //                CreateNote(s.Corde, s.Position);
                    //            else if (s.Type == TypeSonEnum.Accord)
                    //                CreateChord(s.Chord, s.SensGrattageCode);
                    //        }
                    //    }

                    //    if (iMesures < part.Mesures.Count)
                    //        CreateVerticalLine();
                    //}


                    for (int i = 0; i < part.Mesures.Count; i++)
                    {
                        iMesures++;

                        foreach (Temps tmp in part.Mesures[i].Temps)
                        {
                            foreach (Son s in tmp.Sons)
                            {
                                //if (cursorWith >= (Options.Width - 20)) // 20 = largeur d'une note à peu près, avec marges côtés
                                //{
                                //    cursorHeight += Options.StringSpacing * 6 + 20;
                                //    CreateNewLine();
                                //}

                                if (s.Type == TypeSonEnum.Note)
                                    CreateNote(s.Corde, s.Position);
                                else if (s.Type == TypeSonEnum.Accord)
                                    CreateChord(s.Chord, s.SensGrattageCode);
                            }
                        }

                        if (iMesures < part.Mesures.Count)
                        {
                            int nbNotesNextMesure = 0;
                            if (part.Mesures[i + 1] != null && part.Mesures[i + 1].Temps != null && part.Mesures[i + 1].Temps.Count > 0)
                            {
                                part.Mesures[i + 1].Temps.ForEach(delegate (Temps t)
                                {
                                    nbNotesNextMesure += t.Sons != null ? t.Sons.Count() : 0;
                                });
                            }

                            if (cursorWith < (Options.Width - (20 + nbNotesNextMesure * 20)))
                                CreateVerticalLine();
                            else
                            {
                                cursorHeight += Options.StringSpacing * 6 + 20;
                                CreateNewLine();
                            }
                        }
                    }

                    // on mets à jour la hauteur du svg

                    cursorHeight += Options.StringSpacing * 5;
                    svgHeight += cursorHeight;
                }

                // Response

                SVGContent = "<svg width=\"" + Options.Width + "\" height=\"" + (svgHeight + 20) + "\">" + SVGContent + "</svg>";

                Status = TabGenerationStatus.Succeed;
                return true;
            }
            catch (Exception)
            {
                Status = TabGenerationStatus.Failed;
                throw;
            }
            finally
            {

            }
        }

        public void CreateChord(string chord, int? sensGrattageCode)
        {
            string[] chordComp = chord.Split(new char[] { '|' }, StringSplitOptions.None);
            if (chordComp.Length != 6)
                throw new Exception();

            //bool downDirection = chordComp[6] == "d" ? true : false;

            chordComp = chordComp.Reverse().ToArray();

            int yPosi = cursorHeight;
            cursorWith += 10;

            if (sensGrattageCode.HasValue)
            {
                if ((SensGrattageCordes)sensGrattageCode.Value == SensGrattageCordes.Down)
                {
                    int startPosi = yPosi;
                    int endPosi = yPosi;

                    List<int> cordesJouees = new List<int>();
                    for (int i = 0; i < 5; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(chordComp[i]))
                            cordesJouees.Add(i);
                    }

                    startPosi += cordesJouees.Max() * 20;
                    endPosi += cordesJouees.Min() * 20;

                    SVGContent += "<line x1=\"" + cursorWith + "\" y1=\"" + (startPosi + 8) + "\" x2=\"" + cursorWith + "\" y2=\"" + (endPosi - 8) + "\" stroke-width=\"1\" stroke=\"black\"/>";
                    SVGContent += "<line x1=\"" + cursorWith + "\" y1=\"" + (endPosi - 8) + "\" x2=\"" + (cursorWith - 4) + "\" y2=\"" + (endPosi + 6) + "\" stroke-width=\"1\" stroke=\"black\"/>";
                    SVGContent += "<line x1=\"" + cursorWith + "\" y1=\"" + (endPosi - 8) + "\" x2=\"" + (cursorWith + 4) + "\" y2=\"" + (endPosi + 6) + "\" stroke-width=\"1\" stroke=\"black\"/>";
                    //cf http://vanseodesign.com/web-design/svg-markers/
                }
                else if ((SensGrattageCordes)sensGrattageCode.Value == SensGrattageCordes.Up)
                {

                }

                cursorWith += 5;
            }

            for (int i = 0; i < 5; i++)
            {
                if (i > 0)
                    yPosi += 20;

                if (string.IsNullOrWhiteSpace(chordComp[i]))
                    continue;

                SVGContent += "<circle cx=\"" + (cursorWith + 8) + "\" cy=\"" + yPosi + "\" r=\"8\" stroke=\"rgb(255, 255, 255)\" stroke-width=\"0\" fill=\"rgb(255, 255, 255)\" /><text x=\"" + (cursorWith + 4) + "\" y=\"" + (yPosi + 4) + "\" font-family=\"" + Options.Typeface + "\" font-size=\"15\" fill=\"black\">" + chordComp[i] + "</text>";
            }
        }

        public void CreateNote(int corde, int posi)
        {
            int yPosi = cursorHeight;
            yPosi += (6 - corde) * 20;
            SVGContent += "<circle cx=\"" + (cursorWith + 8) + "\" cy=\"" + yPosi + "\" r=\"8\" stroke=\"rgb(255, 255, 255)\" stroke-width=\"0\" fill=\"rgb(255, 255, 255)\" /><text x=\"" + (cursorWith + 4) + "\" y=\"" + (yPosi + 4) + "\" font-family=\"" + Options.Typeface + "\" font-size=\"15\" fill=\"black\">" + posi + "</text>";
            cursorWith += 25;
        }

        public void CreateVerticalLine()
        {
            SVGContent += "<line x1=\"" + (cursorWith + 7) + "\" y1=\"" + cursorHeight + "\" x2=\"" + (cursorWith + 7) + "\" y2=\"" + (cursorHeight + (Options.StringSpacing * 5)) + "\" stroke=\"" + Options.StringColor + "\" stroke-width=\"" + Options.StringWidth + "\" fill=\"" + Options.StringColor + "\"></line>";
            cursorWith += 14;
        }

        /// <summary>
        /// On crée une nouvelle ligne vide (cordes + inscription TAB etc, sans aucune note)
        /// </summary>
        /// <returns></returns>
        public void CreateNewLine()
        {
            cursorWith = 0;

            // ligne début tab

            SVGContent += "<line x1=\"0\" y1=\"" + cursorHeight + "\" x2=\"0\" y2=\"" + (cursorHeight + (Options.StringSpacing * 5)) + "\" stroke=\"" + Options.StringColor + "\" stroke-width=\"" + Options.StringWidth + "\" fill=\"" + Options.StringColor + "\"></line>";

            // cordes guitare

            for (int i = 0; i < 6; i++)
                SVGContent += "<line x1=\"0\" y1=\"" + (cursorHeight + (Options.StringSpacing * i)) + "\" x2=\"100%\" y2=\"" + (cursorHeight + (Options.StringSpacing * i)) + "\" stroke=\"" + Options.StringColor + "\" stroke-width=\"" + Options.StringWidth + "\" fill=\"" + Options.StringColor + "\"></line>";

            // inscription "TAB"

            SVGContent += "<text x=\"10\" y=\"" + (cursorHeight + Options.StringSpacing + 15) + "\" text-anchor=\"middle\" font-family=\"" + Options.Typeface + "\" font-size=\"11\" stroke=\"" + Options.StringColor + "\" fill=\"" + Options.StringColor + "\">T</text>";
            SVGContent += "<text x=\"10\" y=\"" + (cursorHeight + (Options.StringSpacing * 2) + 15) + "\" text-anchor=\"middle\" font-family=\"" + Options.Typeface + "\" font-size=\"11\" stroke=\"" + Options.StringColor + "\" fill=\"" + Options.StringColor + "\">A</text>";
            SVGContent += "<text x=\"10\" y=\"" + (cursorHeight + (Options.StringSpacing * 3) + 15) + "\" text-anchor=\"middle\" font-family=\"" + Options.Typeface + "\" font-size=\"11\" stroke=\"" + Options.StringColor + "\" fill=\"" + Options.StringColor + "\">B</text>";

            cursorWith += 50; // taille de tab plus un peu d'espace

            // ligne début tab

            SVGContent += "<line x1=\"100%\" y1=\"" + cursorHeight + "\" x2=\"100%\" y2=\"" + (cursorHeight + (Options.StringSpacing * 5)) + "\" stroke=\"" + Options.StringColor + "\" stroke-width=\"" + Options.StringWidth + "\" fill=\"" + Options.StringColor + "\"></line>";

        }
    }



    public class SVGTabOptions
    {
        /// <summary>
        /// Affiche les accords composant la chanson en en-tête du document
        /// </summary>
        public bool DisplayChordsHeader { get; set; } = true;

        /// <summary>
        /// Affiche les accords le long de la tablature
        /// </summary>
        public bool DisplayTabChords { get; set; } = true;

        /// <summary>
        /// Affiche l'enchainement des parties
        /// </summary>
        public bool DisplayEnchainement { get; set; } = true;

        /// <summary>
        /// Mode d'affichage de l'enchainement
        /// true = détaillé (plusieurs lignes), false = simple
        /// </summary>
        public bool? AffichageEnchainementDetaille { get; set; } = null;

        /// <summary>
        /// Largeur du document
        /// </summary>
        public int Width { get; set; } = 890;

        public string StringColor { get; set; } = "rgb(20, 20, 20)";

        public int StringWidth { get; set; } = 1;

        public int StringSpacing { get; set; } = 20;

        public string Typeface { get; set; } = "Verdana";

        public SVGTabOptions()
        { }
    }
}