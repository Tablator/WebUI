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