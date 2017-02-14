namespace Tablator.DomainModel
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    [JsonObject(MemberSerialization.OptIn)]
    public class Hierarchy
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "desc")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "pic")]
        public string Picture { get; set; }

        [JsonProperty(PropertyName = "pid")]
        public Guid? ParentId { get; set; }

        [JsonProperty(PropertyName = "posi")]
        public int Position { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Catalog
    {
        [JsonProperty(PropertyName = "hrrch")]
        public List<Hierarchy> Hierarchy { get; set; }
    }
}

namespace Tablator.ViewModel
{
    using BusinessModel;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    [JsonObject(MemberSerialization.OptIn)]
    public class HierarchyLevelViewModel 
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "desc")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "pic")]
        public string Picture { get; set; }

        [JsonProperty(PropertyName = "pid")]
        public Guid? ParentId { get; set; }

        [JsonProperty(PropertyName = "posi")]
        public int Position { get; set; }

        public HierarchyLevelViewModel Ascendance { get; }

        public List<HierarchyLevelViewModel> Descendance { get; }

        public bool Root => ParentId == null || ParentId.Value == Guid.Empty ? true : false;

        public bool HasChildren => Descendance == null || Descendance.Count == 0 ? true : false;

        public HierarchyLevelViewModel(Guid id, string name, string desc, string pic, int posi)
        {
            Id = id;
            Name = name;
            Description = desc;
            Picture = pic;
            Position = posi;

            Descendance = new List<HierarchyLevelViewModel>();
        }

        public HierarchyLevelViewModel(HierarchyModel hier)
        {
            Id = hier.Id;
            Name = hier.Name;
        }
    }

    public class HierarchyViewModel : List<HierarchyLevelViewModel>
    {
        public HierarchyViewModel(HierarchyCollectionModel hiers)
        {
            foreach (HierarchyModel hier in hiers)
                Add(new HierarchyLevelViewModel(hier));
        }
    }
}

namespace Tablator.BusinessModel
{
    using DomainModel;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class HierarchyModel
    {
        public Guid Id { get; }

        public string Name { get; }

        public string Description { get; }

        public string Picture { get; }

        public int position { get; }

        public Guid? ParentId { get; }

        public HierarchyModel Ascendance { get; }

        public HierarchyCollectionModel Descendance { get; }

        public bool Root => ParentId == null || ParentId.Value == Guid.Empty ? true : false;

        public bool HasChildren => Descendance == null || Descendance.Count == 0 ? true : false;

        public HierarchyModel(Guid id, string name, string desc, string pic, int posi)
        {
            Id = id;
            Name = name;
            Description = desc;
            Picture = pic;
            position = posi;

            Descendance = new HierarchyCollectionModel();
        }
    }

    public class HierarchyCollectionModel : List<HierarchyModel>
    {
        public HierarchyCollectionModel()
        { }

        public HierarchyCollectionModel GetRootLevel()
        {
            HierarchyCollectionModel ret = new HierarchyCollectionModel();
            ret.AddRange(this.Where(x => !x.ParentId.HasValue));
            return ret;
        }
    }
}