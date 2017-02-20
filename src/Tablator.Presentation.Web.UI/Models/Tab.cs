using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tablator.Presentation.Web.UI.Models
{
    public class TabViewModel
    {
        public string SVGContent { get; set; }

        public TabViewModel(string svgContent)
        {
            SVGContent = svgContent;
        }
    }
}