using DbmsApi.API;
using MathPackage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeDesignPackage
{
    public class Configuration
    {
        public string ObjectModelID;
        public CatalogObject CatalogObject;
        public Vector3D Location;
        public Vector4D Orientation;
        public double Eval = 0;
    }

    public class CatalogInitializerID
    {
        public string CatalogID;
        public Vector3D Location;
    }

    public class CatalogInitializer
    {
        public CatalogObject CatalogObject;
        public Vector3D Location;
    }
}
