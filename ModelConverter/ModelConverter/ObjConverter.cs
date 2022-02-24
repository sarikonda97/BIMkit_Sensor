using DbmsApi.API;
using DbmsApi.Mongo;
using MathPackage;
using ObjLoader.Loader.Data.VertexData;
using ObjLoader.Loader.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModelConverter
{
    public static class ObjConverter
    {
        public enum FaceSide { FRONT, BACK, BOTH }

        public static CatalogObject ConvertObjFile(FileStream fileStream, string objectName,double scale, bool flipTriangles, bool flipYZ)
        {
            // Load and parse the obj file
            IObjLoader objLoader = new ObjLoaderFactory().Create();
            LoadResult result = objLoader.Load(fileStream);
            CatalogObject newModelObject = ConvertObjFile(result, objectName, scale, flipTriangles, flipYZ);
            newModelObject.Tags.Add(new KeyValuePair<string, string>("Dataset", ConverterGeneral.Datasets.OBJ.ToString()));
            return newModelObject;
        }

        public static CatalogObject ConvertObjFile(LoadResult objectLoadResult, string objectName, double scale, bool flipTriangles, bool flipYZ)
        {
            List<Component> components = new List<Component>();
            foreach (var group in objectLoadResult.Groups)
            {
                //  Get faces
                List<List<Vector3D>> faces = group.Faces.Select(face =>
                {
                    List<Vector3D> vertices = new List<Vector3D>();
                    for (int i = 0; i < face.Count; i++)
                    {
                        Vertex vertex = objectLoadResult.Vertices[face[i].VertexIndex - 1];
                        if (flipYZ)
                        {
                            vertices.Add(new Vector3D(vertex.X, vertex.Z, vertex.Y));
                        }
                        else
                        {
                            vertices.Add(new Vector3D(vertex.X, vertex.Y, vertex.Z));
                        }
                    }
                    return vertices;
                }).ToList();

                // Attempt to tesellate the faces
                List<int> triangles = new List<int>();
                List<Vector3D> finalVertices = new List<Vector3D>();
                foreach (List<Vector3D> face in faces)
                {
                    List<int> tempTriangleInts = new List<int>();
                    if (face.Count >= 3)
                    {
                        tempTriangleInts = EarClippingVariant(face, FaceSide.FRONT);
                    }

                    foreach (int intVal in tempTriangleInts)
                    {
                        triangles.Add(intVal + finalVertices.Count);
                    }
                    finalVertices.AddRange(face);
                }

                List<int[]> trianglesTuples = new List<int[]>();
                for (int index = 0; index < triangles.Count; index += 3)
                {
                    if (flipTriangles)
                    {
                        trianglesTuples.Add(new int[3] { triangles[index], triangles[index + 2], triangles[index + 1] });
                    }
                    else
                    {
                        trianglesTuples.Add(new int[3] { triangles[index], triangles[index + 1], triangles[index + 2] });
                    }
                }

                Component component = new Component()
                {
                    Triangles = trianglesTuples,
                    Vertices = finalVertices.Select(v => v.Copy()).ToList(),
                    MaterialId = group.Material != null ? group.Material.Name : "Default",
                    Properties = new DbmsApi.API.Properties(),
                    Tags = new List<KeyValuePair<string, string>>()
                };
                components.Add(component);
            }

            ConverterGeneral.CenterObject(components, scale, new Vector4D(0, 0, 0, 1));

            CatalogObject modelSpecificObject = new CatalogObject()
            {
                Name = objectName,
                CatalogID = null,
                Components = components,
                Properties = new DbmsApi.API.Properties(),
                TypeId = "N/A"
            };

            return modelSpecificObject;
        }

        public static List<int> EarClippingVariant(List<Vector3D> points, FaceSide faceSide)
        {
            List<int> triangleList = new List<int>();

            List<Tuple<int, Vector3D>> remainingPoints = new List<Tuple<int, Vector3D>>();
            for (int i = 0; i < points.Count; i++)
            {
                Vector3D p = points[i];
                remainingPoints.Add(new Tuple<int, Vector3D>(i, p));
            }

            // Remove points that are in a straight line
            int pCount = remainingPoints.Count;
            for (int i = 0; i < pCount && pCount > 3; i++)
            {
                int index1 = (i + 0) % pCount;
                int index2 = (i + 1) % pCount;
                int index3 = (i + 2) % pCount;
                Vector3D p1 = remainingPoints[index1].Item2;
                Vector3D p2 = remainingPoints[index2].Item2;
                Vector3D p3 = remainingPoints[index3].Item2;

                Vector3D v1 = Vector3D.Subract(p1, p2);
                Vector3D v2 = Vector3D.Subract(p3, p2);
                Vector3D pointsFacingDir = Vector3D.Cross(v1, v2);
                if (pointsFacingDir.Length() < 0.01 * v1.Length() * v2.Length())
                {
                    double l1 = Vector3D.Distance(p1, p2);
                    double l2 = Vector3D.Distance(p1, p3);
                    double l3 = Vector3D.Distance(p2, p3);
                    if (Math.Abs(l1 - (l2 + l3)) < 0.001)
                    {
                        remainingPoints.RemoveAt(index3);
                        pCount = remainingPoints.Count;
                        i = -1;
                        continue;
                    }
                    if (Math.Abs(l2 - (l1 + l3)) < 0.001)
                    {
                        remainingPoints.RemoveAt(index2);
                        pCount = remainingPoints.Count;
                        i = -1;
                        continue;
                    }
                    if (Math.Abs(l3 - (l2 + l1)) < 0.001)
                    {
                        remainingPoints.RemoveAt(index1);
                        pCount = remainingPoints.Count;
                        i = -1;
                        continue;
                    }
                }
            }

            Vector3D facingDir = null;
            while (remainingPoints.Count > 3)
            {
                pCount = remainingPoints.Count;
                bool removedEar = false;
                for (int i = 0; i < pCount; i++)
                {
                    int index1 = (i + 0) % pCount;
                    int index2 = (i + 1) % pCount;
                    int index3 = (i + 2) % pCount;
                    Vector3D p1 = remainingPoints[index1].Item2;
                    Vector3D p2 = remainingPoints[index2].Item2;
                    Vector3D p3 = remainingPoints[index3].Item2;

                    // Check that the direction is parallel to the first triangle direction (the first three points will determine the direction)
                    Vector3D v1 = Vector3D.Subract(p1, p2);
                    Vector3D v2 = Vector3D.Subract(p3, p2);
                    Vector3D pointsFacingDir = Vector3D.Cross(v1, v2);
                    if (facingDir == null)
                    {
                        facingDir = pointsFacingDir.Copy();
                        if (facingDir.x != 0 || facingDir.y != 0)
                        {
                            int asda = 0;
                        }
                        else
                        {
                            if (facingDir.z > 0)
                            {
                                facingDir = facingDir.Neg();
                            }
                        }
                    }
                    double angle = Vector3D.AngleRad(facingDir, pointsFacingDir);
                    if (angle == double.NaN)
                    {
                        throw new Exception();
                    }
                    if (angle > Math.PI / 2.0)
                    {
                        continue;
                    }

                    bool inTriangle = false;
                    for (int j = 0; j < remainingPoints.Count; j++)
                    {
                        if ((j == index1) || (j == index2) || (j == index3))
                        {
                            continue;
                        }

                        Vector3D p4 = remainingPoints[j].Item2;
                        if (Utils.PointInTriangle(p4, p1, p2, p3))
                        {
                            inTriangle = true;
                            break;
                        }
                    }
                    if (!inTriangle)
                    {
                        if (faceSide == FaceSide.FRONT)
                        {
                            triangleList.Add(remainingPoints[index1].Item1);
                            triangleList.Add(remainingPoints[index3].Item1);
                            triangleList.Add(remainingPoints[index2].Item1);
                        }
                        if (faceSide == FaceSide.BACK)
                        {
                            triangleList.Add(remainingPoints[index1].Item1);
                            triangleList.Add(remainingPoints[index2].Item1);
                            triangleList.Add(remainingPoints[index3].Item1);
                        }
                        remainingPoints.RemoveAt(index2);
                        removedEar = true;
                        break;
                    }
                }

                if (!removedEar)
                {
                    break;
                }
            }

            if (faceSide == FaceSide.FRONT || faceSide == FaceSide.BOTH)
            {
                triangleList.Add(remainingPoints[0].Item1);
                triangleList.Add(remainingPoints[2].Item1);
                triangleList.Add(remainingPoints[1].Item1);
            }
            if (faceSide == FaceSide.BACK || faceSide == FaceSide.BOTH)
            {
                triangleList.Add(remainingPoints[0].Item1);
                triangleList.Add(remainingPoints[1].Item1);
                triangleList.Add(remainingPoints[2].Item1);
            }

            return triangleList;
        }
    }
}