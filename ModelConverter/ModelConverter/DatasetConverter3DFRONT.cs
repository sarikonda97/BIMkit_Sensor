using DbmsApi.API;
using MathPackage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelConverter
{
    public static class DatasetConverter3DFRONT
    {
        // Models

        public static Model Convert3DFRONTModel(string path, double scale, bool flipTriangles, bool flipYZ)
        {
            Rootobject inputModel = null;
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                inputModel = JsonConvert.DeserializeObject<Rootobject>(json);
            }

            Model newModel = new Model()
            {
                Id = inputModel.uid,
                Name = inputModel.uid,
                ModelObjects = new List<ModelObject>(),
                Properties = new DbmsApi.API.Properties(),
                Relations = new List<Relation>(),
                Tags = new List<KeyValuePair<string, string>>()
            };

            Dictionary<string, ModelObject> modelObjects = GetModelObjects(inputModel, flipTriangles, flipYZ);
            List<ModelCatalogObject> modelCatalogObj = GetCatalogObjects(inputModel);
            foreach (ModelCatalogObject mo in modelCatalogObj)
            {
                if (modelObjects.TryGetValue(mo.Id, out ModelObject existingMO))
                {
                    int i = 0;
                }
                else
                {
                    modelObjects.Add(mo.Id, mo);
                }
            }

            // For each object, find the room it is referenced in and its location in that room
            foreach (Room r in inputModel.scene.room)
            {
                foreach (Child c in r.children)
                {
                    if (modelObjects.ContainsKey(c._ref))
                    {
                        ModelObject objOfInterest = modelObjects[c._ref];
                        if (flipYZ)
                        {
                            objOfInterest.Location = new Vector3D(c.pos[0], c.pos[2], c.pos[1]);
                            objOfInterest.Orientation = new Vector4D(c.rot[0], c.rot[2], c.rot[1], c.rot[3]);
                        }
                        else
                        {
                            objOfInterest.Location = new Vector3D(c.pos[0], c.pos[1], c.pos[2]);
                            objOfInterest.Orientation = new Vector4D(c.rot[0], c.rot[1], c.rot[2], c.rot[3]);
                        }

                        objOfInterest.Location = ConverterGeneral.CenterObject(objOfInterest.Components, scale, objOfInterest.Orientation);
                        newModel.ModelObjects.Add(objOfInterest);
                    }
                }
            }
            foreach (Door d in inputModel.extension.door)
            {
                foreach (string _ref in d._ref)
                {

                }
            }

            return newModel;
        }

        public static Dictionary<string, ModelObject> GetModelObjects(Rootobject rootobject, bool flipTriangles, bool flipYZ)
        {
            Dictionary<string, ModelObject> objectList = new Dictionary<string, ModelObject>();
            foreach (Mesh m in rootobject.mesh)
            {
                ModelObject modelObject = new ModelObject()
                {
                    Id = m.uid,
                    TypeId = m.type,
                    Components = new List<Component>(),
                    Properties = new DbmsApi.API.Properties(),
                    Tags = new List<KeyValuePair<string, string>>(),
                    Name = m.constructid
                };

                Component newComponent = new Component()
                {
                    Vertices = ConvertArayToVectList(m.xyz, flipYZ),
                    Triangles = ConvertArayToTriangleList(m.faces.Select(f=>(int)f).ToArray(), flipTriangles),
                    MaterialId = m.material
                };

                modelObject.Components.Add(newComponent);
                objectList.Add(modelObject.Id, modelObject);
            }

            return objectList;
        }

        public static List<Vector3D> ConvertArayToVectList(float[] array, bool flipYZ)
        {
            List<Vector3D> returnList = new List<Vector3D>();
            for (int i = 0; i < array.Length; i += 3)
            {
                Vector3D newVect;
                if (flipYZ)
                {
                    newVect = new Vector3D(array[i], array[i + 2], array[i + 1]);
                }
                else
                {
                    newVect = new Vector3D(array[i], array[i + 1], array[i + 2]);
                }
                returnList.Add(newVect);
            }
            return returnList;
        }

        public static List<int[]> ConvertArayToTriangleList(int[] array, bool flipTriangles)
        {
            List<int[]> returnList = new List<int[]>();
            for (int i = 0; i < array.Length; i += 3)
            {
                int[] newTri = new int[3];
                if (flipTriangles)
                {
                    newTri = new int[3] { array[i], array[i + 2], array[i + 1] };
                }
                else
                {
                    newTri = new int[3] { array[i], array[i + 1], array[i + 2] };
                }
                returnList.Add(newTri);
            }
            return returnList;
        }

        public static List<ModelCatalogObject> GetCatalogObjects(Rootobject rootobject)
        {
            List<ModelCatalogObject> objectList = new List<ModelCatalogObject>();
            foreach (Furniture f in rootobject.furniture)
            {
                if (!f.valid)
                {
                    continue;
                }
                ModelCatalogObject mco = new ModelCatalogObject()
                {
                    Id = f.uid,
                    CatalogId = f.jid,
                    Name = f.title,
                    Components = new List<Component>(),
                    Properties = new DbmsApi.API.Properties(),
                    TypeId = "N/A",
                    Tags = new List<KeyValuePair<string, string>>()
                };
                objectList.Add(mco);
            }

            return objectList;
        }

        // Objects

        public static CatalogObject Convert3DFRONTCatalogObject(string path, double scale, bool flipTriangles, bool flipYZ)
        {
            string id = Path.GetFileNameWithoutExtension(path);
            FileStream fileStream = new FileStream(path + "\\raw_model.obj", FileMode.Open);
            CatalogObject catalogObject = ObjConverter.ConvertObjFile(fileStream, id, scale, flipTriangles, flipYZ);
            fileStream.Close();
            catalogObject.CatalogID = id;
            return catalogObject;
        }
    }
}
