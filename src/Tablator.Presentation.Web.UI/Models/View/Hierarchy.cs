namespace Tablator.ViewModel
{
    using BusinessModel;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class HierarchyViewModel : List<HierarchyLevelViewModel>
    {
        public HierarchyViewModel(HierarchyCollectionModel hiers)
        {
            foreach (HierarchyModel hier in hiers)
                Add(new HierarchyLevelViewModel(hier));
        }
    }
}