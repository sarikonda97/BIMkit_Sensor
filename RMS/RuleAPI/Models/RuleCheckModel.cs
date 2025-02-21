﻿using DbmsApi.API;
using DbmsApi.Mongo;
using MathPackage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleAPI.Models
{
    public class RuleCheckModel
    {
        public List<RuleCheckObject> Objects;
        public List<RuleCheckRelation> Relations;
        private Model _Model;

        [JsonConstructor]
        private RuleCheckModel() { }

        public RuleCheckModel(Model model)
        {
            _Model = model;

            Objects = new List<RuleCheckObject>();
            foreach (var modelObj in model.ModelObjects)
            {
                if (modelObj.GetType() == typeof(ModelCatalogObject))
                {
                    ModelCatalogObject modelCatObj = (ModelCatalogObject)modelObj;
                    Objects.Add(new RuleCheckObject(modelCatObj, modelCatObj.CatalogId));
                }
                else
                {
                    Objects.Add(new RuleCheckObject(modelObj));
                }
            }
            Relations = new List<RuleCheckRelation>();
            foreach (var relation in model.Relations)
            {
                Relations.Add(new RuleCheckRelation(relation, Objects));
            }
        }

        public string AddObject(CatalogObject catalogObject, Vector3D location, Vector4D orientation, string setID = null)
        {
            ModelCatalogObject mo = new ModelCatalogObject()
            {
                Name = catalogObject.Name,
                Id = setID == null ? Guid.NewGuid().ToString() : setID,
                CatalogId = catalogObject.CatalogID,
                TypeId = catalogObject.TypeId,
                Location = location,
                Orientation = orientation,
                Components = catalogObject.Components,
                Properties = catalogObject.Properties,
                Tags = new List<KeyValuePair<string, string>>(),
            };
            Objects.Add(new RuleCheckObject(mo, mo.CatalogId));
            return mo.Id;
        }

        public void RemoveObject(string objectId)
        {
            Objects.RemoveAll(o => o.ID == objectId);
            Relations.RemoveAll(r => r.FirstObj.ID == objectId || r.SecondObj.ID == objectId);
        }

        public string MoveObject(string objectId, CatalogObject catalogObject, Vector3D location, Vector4D orientation)
        {
            RemoveObject(objectId);
            return AddObject(catalogObject, location, orientation);
        }

        public Model FullModel()
        {
            // Go over each object and update it based on the RuleCheck Changes (only changes to location orientation and Porperties is permited)
            foreach (RuleCheckObject rco in Objects)
            {
                if (rco.VirtualObject)
                {
                    continue;
                }

                ModelObject modelObject = _Model.ModelObjects.FirstOrDefault(o => o.Id == rco.ID);
                if (modelObject != null)
                {
                    // Object was moved
                    modelObject.Location = rco.Location;
                    modelObject.Orientation = rco.Orientation;

                    if (modelObject.GetType() != typeof(ModelCatalogObject))
                    {
                        modelObject.Properties = rco.Properties;
                    }

                    // Might also be allowed to edit something about the ModelObjects here:

                }
                else
                {
                    // Object is new
                    List<int[]> triangles = new List<int[]>();
                    for (int i = 0; i < rco.Triangles.Count; i++)
                    {
                        triangles.Add(new int[3] { rco.Triangles[i][0], rco.Triangles[i][1], rco.Triangles[i][2] });
                    }
                    ModelObject newModelObject = new ModelObject()
                    {
                        Location = rco.Location,
                        Orientation = rco.Orientation,
                        Components = new List<Component>(){new Component()
                        {
                            Triangles = triangles,
                            Vertices = rco.LocalVerticies.Select(v=>v.Copy()).ToList()
                        } },
                        Id = rco.ID,
                        Name = rco.Name,
                        Properties = rco.Properties,
                        TypeId = rco.Type
                    };

                    if (!string.IsNullOrWhiteSpace(rco.CatalogId))
                    {
                        ModelCatalogObject newMco = new ModelCatalogObject
                        {
                            CatalogId = rco.CatalogId,
                            Location = rco.Location,
                            Orientation = rco.Orientation,
                            Components = new List<Component>(){new Component()
                            {
                                Triangles = triangles,
                                Vertices = rco.LocalVerticies.Select(v=>v.Copy()).ToList()
                            } },
                            Id = rco.ID,
                            Name = rco.Name,
                            Properties = rco.Properties,
                            TypeId = rco.Type
                        };
                        newModelObject = newMco;
                    }

                    _Model.ModelObjects.Add(newModelObject);
                }
            }

            foreach (RuleCheckRelation rcr in Relations)
            {
                Relation relation = _Model.Relations.FirstOrDefault(r => r.ObjectId1 == rcr.FirstObj.ID && r.ObjectId2 == rcr.SecondObj.ID);
                if (relation != null)
                {
                    relation.Properties = rcr.Properties;
                }
                else
                {
                    _Model.Relations.Add(new Relation()
                    {
                        ObjectId1 = rcr.FirstObj.ID,
                        ObjectId2 = rcr.SecondObj.ID,
                        Properties = rcr.Properties
                    });
                }
            }

            return _Model;
        }
    }
}