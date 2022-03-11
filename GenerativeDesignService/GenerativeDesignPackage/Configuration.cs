using DbmsApi.API;
using MathPackage;
using ModelCheckPackage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeDesignPackage
{
    public class ObjectConfiguration
    {
        public string ObjectModelID;
        public CatalogObject CatalogObject;
        public Vector3D Location;
        public Vector4D Orientation;
        public CheckScore Eval;
    }

    public class SceneConfiguration
    {
        public List<ObjectConfiguration> ObjectConfigurations;
        public CheckScore Eval;
    }

    public class ThreadConfiguration
    {
        public SceneConfiguration SceneConfiguration;
        public ModelChecker ModelChecker;
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
