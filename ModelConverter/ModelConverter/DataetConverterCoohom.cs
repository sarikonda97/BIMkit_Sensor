using DbmsApi.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelConverter
{
    public class DataetConverterCoohom
    {
        //    // Models

        //    public static Model ConvertCoohomModel(string path, double scale, bool flipTriangles, bool flipYZ)
        //    {
        //        string id = Path.GetFileNameWithoutExtension(path);
        //        FileStream fileStream = new FileStream(path + "\\raw_model.obj", FileMode.Open);
        //        CatalogObject catalogObject = ObjConverter.ConvertObjFile(fileStream, id, scale, flipTriangles, flipYZ);
        //        fileStream.Close();
        //        catalogObject.CatalogID = id;

        //        Rootobject inputModel = null;
        //        using (StreamReader r = new StreamReader(path))
        //        {
        //            string json = r.ReadToEnd();
        //            inputModel = JsonConvert.DeserializeObject<Rootobject>(json);
        //        }

        //        Model newModel = new Model()
        //        {
        //            Id = inputModel.uid,
        //            Name = inputModel.uid,
        //            ModelObjects = new List<ModelObject>(),
        //            Properties = new DbmsApi.API.Properties(),
        //            Relations = new List<Relation>(),
        //            Tags = new List<KeyValuePair<string, string>>()
        //        };

        //        Dictionary<string, ModelObject> modelObjects = GetModelObjects(inputModel, flipTriangles, flipYZ);
        //        List<ModelCatalogObject> modelCatalogObj = GetCatalogObjects(inputModel);
        //        foreach (ModelCatalogObject mo in modelCatalogObj)
        //        {
        //            if (modelObjects.TryGetValue(mo.Id, out ModelObject existingMO))
        //            {
        //                int i = 0; // Something weird here...
        //            }
        //            else
        //            {
        //                modelObjects.Add(mo.Id, mo);
        //            }
        //        }

        //        // For each object, find the room it is referenced in and its location in that room
        //        foreach (Room r in inputModel.scene.room)
        //        {
        //            foreach (Child c in r.children)
        //            {
        //                if (modelObjects.ContainsKey(c._ref))
        //                {
        //                    ModelObject objOfInterest = modelObjects[c._ref];
        //                    Vector3D loc;
        //                    Vector4D orient;
        //                    if (flipYZ)
        //                    {
        //                        loc = new Vector3D(c.pos[0], c.pos[2], c.pos[1]);
        //                        orient = new Vector4D(c.rot[0], c.rot[2], c.rot[1], c.rot[3]);
        //                    }
        //                    else
        //                    {
        //                        loc = new Vector3D(c.pos[0], c.pos[1], c.pos[2]);
        //                        orient = new Vector4D(c.rot[0], c.rot[1], c.rot[2], c.rot[3]);
        //                    }

        //                    if (objOfInterest.GetType() == typeof(ModelCatalogObject))
        //                    {
        //                        objOfInterest.Location = loc;
        //                    }
        //                    else
        //                    {
        //                        objOfInterest.Location = ConverterGeneral.CenterObject(objOfInterest.Components, scale, orient);
        //                    }
        //                    objOfInterest.Orientation = orient;
        //                }
        //            }
        //        }

        //        newModel.ModelObjects = CombineModelObjects(modelObjects);

        //        return newModel;
        //    }

        //    // Objects

        //    public static CatalogObject ConvertCoohomCatalogObject(string path, double scale, bool flipTriangles, bool flipYZ)
        //    {
        //        string id = Path.GetFileNameWithoutExtension(path);
        //        FileStream fileStream = new FileStream(path + "\\raw_model.obj", FileMode.Open);
        //        CatalogObject catalogObject = ObjConverter.ConvertObjFile(fileStream, id, scale, flipTriangles, flipYZ);
        //        fileStream.Close();
        //        catalogObject.CatalogID = id;
        //        return catalogObject;
        //    }
    }
}
