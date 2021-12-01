using MathPackage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelService
{
    public class Voxel
    {
        public Vector3D Location;
        public double Size;
        public string ModelObjectID;

        public Voxel(Vector3D location, double size, string modelObjectID)
        {
            Location = location;
            Size = size;
            ModelObjectID = modelObjectID;
        }
    }
}
