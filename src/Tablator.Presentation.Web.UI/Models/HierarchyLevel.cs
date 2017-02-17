namespace Tablator.Presentation.Web.UI.Models
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
}