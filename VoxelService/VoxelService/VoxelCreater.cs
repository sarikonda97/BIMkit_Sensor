using DbmsApi.API;
using MathPackage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelService
{
    public class VoxelCreater
    {
        public Model Model;

        public VoxelCreater(Model model)
        {
            Model = model;
        }

        public List<Voxel> CreateVoxels(double size)
        {
            List<Voxel> voxels = new List<Voxel>();

            // Get the dimentions of the Model
            List<Vector3D> allModelVectors = new List<Vector3D>();
            foreach (ModelObject mo in Model.ModelObjects)
            {
                Matrix4 translationMatrix = Utils.GetTranslationMatrixFromLocationOrientation(mo.Location, mo.Orientation);
                foreach (Component c in mo.Components)
                {
                    allModelVectors.AddRange(Utils.TranslateVerticies(translationMatrix, c.Vertices));
                }
            }
            double minX = allModelVectors.Min(v => v.x);
            double maxX = allModelVectors.Max(v => v.x);
            double minY = allModelVectors.Min(v => v.y);
            double maxY = allModelVectors.Max(v => v.y);
            double minZ = allModelVectors.Min(v => v.z);
            double maxZ = allModelVectors.Max(v => v.z);

            double halfSize = size / 2.0;

            // Get each of the voxel locations
            List<Vector3D> locations = new List<Vector3D>();
            for (double x = minX + halfSize; x <= maxX; x += size)
            {
                for (double y = minY + halfSize; y <= maxY; y += size)
                {
                    for (double z = minZ + halfSize; z <= maxZ; z += size)
                    {
                        locations.Add(new Vector3D(x, y, z));
                    }
                }
            }

            // go over each location and find the object that overlaps it or if multiple then the closest object to the center
            foreach (Vector3D loc in locations)
            {
                PossibleObject bestPossible = new PossibleObject() 
                { 
                    Distance = double.MaxValue, 
                    ObjectID = null, 
                    Overlap = false 
                };
                Mesh voxelBox = Utils.CreateBoundingBox(loc, new Vector3D(size, size, size), FaceSide.FRONT);
                foreach (ModelObject mo in Model.ModelObjects)
                {
                    PossibleObject possible = new PossibleObject() { ObjectID = mo.Id, Distance = double.MaxValue, Overlap = false };
                    Matrix4 translationMatrix = Utils.GetTranslationMatrixFromLocationOrientation(mo.Location, mo.Orientation);
                    foreach (Component c in mo.Components)
                    {
                        Mesh mocMesh = new Mesh(Utils.TranslateVerticies(translationMatrix, c.Vertices), c.Triangles);
                        if (Utils.MeshOverlap(voxelBox, mocMesh, 1.0))
                        {
                            possible.Overlap = true;
                            possible.Distance = Math.Min(possible.Distance, Utils.PointToMeshDistance(loc, mocMesh));
                        }
                    }
                    if (possible.Overlap)
                    {
                        bestPossible = possible.Distance < bestPossible.Distance ? possible : bestPossible;
                    }
                }

                Voxel newVoxel = new Voxel(loc, size, bestPossible.ObjectID);
                voxels.Add(newVoxel);
            }

            return voxels;
        }
    }

    public class PossibleObject
    {
        public string ObjectID;
        public bool Overlap;
        public double Distance;
    }
}
