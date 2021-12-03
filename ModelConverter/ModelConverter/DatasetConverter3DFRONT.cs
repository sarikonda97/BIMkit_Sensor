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

            Dictionary<string, ModelObject> modelObjects = GetModelObjects(inputModel, flipTriangles);
            List<ModelCatalogObject> modelCatalogObj = GetCatalogObjects(inputModel);
            foreach (ModelCatalogObject mo in modelCatalogObj)
            {
                modelObjects.Add(mo.Id, mo);
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

        public static Dictionary<string, ModelObject> GetModelObjects(Rootobject rootobject, bool flipTriangles)
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
                    Vertices = ConvertArayToVectList(m.xyz),
                    Triangles = ConvertArayToTriangleList(m.faces.Select(f=>(int)f).ToArray(), flipTriangles),
                    MaterialId = m.material
                };

                modelObject.Components.Add(newComponent);
                objectList.Add(modelObject.Id, modelObject);
            }

            return objectList;
        }

        public static List<Vector3D> ConvertArayToVectList(float[] array)
        {
            List<Vector3D> returnList = new List<Vector3D>();
            for (int i = 0; i < array.Length; i += 3)
            {
                Vector3D newVect = new Vector3D(array[i], array[i + 1], array[i + 2]);
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
                    newTri = new int[3] { array[i], array[i + 1], array[i + 2] };
                }
                else
                {
                    newTri = new int[3] { array[i], array[i + 2], array[i + 1] };
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

    public class Type
    {
        public string model_id { get; set; }
        public string supercategory { get; set; }
        public string category { get; set; }
        public string style { get; set; }
        public string theme { get; set; }
        public string material { get; set; }
    }

    public class Rootobject
    {
        public string uid { get; set; }
        public string jobid { get; set; }
        public string design_version { get; set; }
        public string code_version { get; set; }
        public float[] north_vector { get; set; }
        public Furniture[] furniture { get; set; }
        public Mesh[] mesh { get; set; }
        public Material[] material { get; set; }
        public Light[] lights { get; set; }
        public Extension extension { get; set; }
        public Scene scene { get; set; }
        public object[] groups { get; set; }
        public string[] materialList { get; set; }
        public string version { get; set; }
    }

    public class Extension
    {
        public Door[] door { get; set; }
        public string outdoor { get; set; }
        public object[] pano { get; set; }
        public string mini_map { get; set; }
        public Perspective_View perspective_view { get; set; }
        public object[] area { get; set; }
        public object[] snapshots { get; set; }
        public string temperature { get; set; }
        public Skybox skybox { get; set; }
    }

    public class Perspective_View
    {
        public string img { get; set; }
    }

    public class Skybox
    {
        public string name { get; set; }
        public int intensity { get; set; }
        public int rotationY { get; set; }
    }

    public class Door
    {
        public string type { get; set; }
        public string roomId { get; set; }
        public string dir { get; set; }

        [JsonProperty("ref")]
        public string[] _ref { get; set; }
    }

    public class Scene
    {
        [JsonProperty("ref")]
        public string _ref { get; set; }
        public float[] pos { get; set; }
        public float[] rot { get; set; }
        public float[] scale { get; set; }
        public Room[] room { get; set; }
        public Boundingbox boundingBox { get; set; }
    }

    public class Boundingbox
    {
        public float[] min { get; set; }
        public float[] max { get; set; }
    }

    public class Room
    {
        public string type { get; set; }
        public string instanceid { get; set; }
        public float size { get; set; }
        public float[] pos { get; set; }
        public float[] rot { get; set; }
        public float[] scale { get; set; }
        public Child[] children { get; set; }
        public int empty { get; set; }
    }

    public class Child
    {
        [JsonProperty("ref")]
        public string _ref { get; set; }
        public string instanceid { get; set; }
        public float[] pos { get; set; }
        public float[] rot { get; set; }
        public float[] scale { get; set; }
        public string replace_jid { get; set; }
        public Replace_Bbox replace_bbox { get; set; }
    }

    public class Replace_Bbox
    {
        public float xLen { get; set; }
        public float yLen { get; set; }
        public float zLen { get; set; }
    }

    public class Furniture
    {
        public string uid { get; set; }
        public string jid { get; set; }
        public object[] aid { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public float[] size { get; set; }
        public string sourceCategoryId { get; set; }
        public object[] bbox { get; set; }
        public bool valid { get; set; }
        public string category { get; set; }
    }

    public class Mesh
    {
        public object[] aid { get; set; }
        public string jid { get; set; }
        public string uid { get; set; }
        public float[] xyz { get; set; }
        public float[] normal { get; set; }
        public float[] uv { get; set; }
        public float[] faces { get; set; }
        public string material { get; set; }
        public string type { get; set; }
        public string constructid { get; set; }
        public string instanceid { get; set; }
        public int lightBandIndex { get; set; }
    }

    public class Material
    {
        public string uid { get; set; }
        public object[] aid { get; set; }
        public string jid { get; set; }
        public string texture { get; set; }
        public string normaltexture { get; set; }
        public float[] color { get; set; }
        public float seamWidth { get; set; }
        public bool useColor { get; set; }
        public float[] normalUVTransform { get; set; }
        public string[] contentType { get; set; }
        public float[] UVTransform { get; set; }
        public Uv_Override uv_override { get; set; }
    }

    public class Uv_Override
    {
        public float tile_x { get; set; }
        public float tile_y { get; set; }
        public float normaltile_x { get; set; }
        public float normaltile_y { get; set; }
    }

    public class Light
    {
        public string uid { get; set; }
        public string nodeType { get; set; }
        public string roomId { get; set; }
        public float[] src_position { get; set; }
        public float[] direction { get; set; }
        public float[] up_vector { get; set; }
        public float color_temperature { get; set; }
        public float multiplier { get; set; }
        public bool DoubleSided { get; set; }
        public bool skylightPortal { get; set; }
        public float size0 { get; set; }
        public float size1 { get; set; }
        public float type { get; set; }
        public bool enabled { get; set; }
        public float units { get; set; }
        public bool affectSpecular { get; set; }
        public string hostInstanceId { get; set; }
        public string _ref { get; set; }
        public string entityId { get; set; }
        public float[] pos { get; set; }
        public float[] rot { get; set; }
        public float[] scale { get; set; }
        public bool invisible { get; set; }
        public bool on { get; set; }
        public float[] target_position { get; set; }
        public float[] filter_Color { get; set; }
        public float intensity_multiplier { get; set; }
        public float turbidity { get; set; }
        public float size_multiplier { get; set; }
        public float sky_model { get; set; }
    }
}
