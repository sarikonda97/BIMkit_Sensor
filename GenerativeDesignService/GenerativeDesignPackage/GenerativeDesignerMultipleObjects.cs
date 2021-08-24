using DbmsApi;
using DbmsApi.API;
using MathPackage;
using ModelCheckPackage;
using RuleAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeDesignPackage
{
    public class GenerativeDesignerMultipleObjects
    {
        public ModelChecker ModelCheck { get; internal set; }
        public List<CatalogObject> CatalogObjects { get; internal set; }
        public List<Vector3D> Locations;

        private Random random = new Random();
        public GenerativeDesignerMultipleObjects(Model model, List<Rule> rules, List<CatalogObject> catalogObjects, List<Vector3D> initialLoc)
        {
            ModelCheck = new ModelChecker(model, rules);
            CatalogObjects = catalogObjects;
            Locations = initialLoc;
        }

        public Model ExecuteGenDesignRoundRobin(GenerativeDesignSettings settings)
        {
            List<Configuration> configsList = new List<Configuration>();

            // Get all the possible orientations:
            List<Vector4D> orientations = new List<Vector4D>()
            {
                Utils.GetQuaterion(new Vector3D(0, 0, 1), 0.0 * Math.PI / 180.0),
                Utils.GetQuaterion(new Vector3D(0, 0, 1), 90.0 * Math.PI / 180.0),
                Utils.GetQuaterion(new Vector3D(0, 0, 1), 180.0 * Math.PI / 180.0),
                Utils.GetQuaterion(new Vector3D(0, 0, 1), 270.0 * Math.PI / 180.0)
            };

            List<CatalogObjectPlacement> objectsToPlace = new List<CatalogObjectPlacement>();
            for (int i = 0; i < CatalogObjects.Count; i++)
            {
                Configuration config = new Configuration()
                {
                    Location = Locations[i],
                    CatalogObject = CatalogObjects[i],
                    Orientation = orientations.First()
                };

                //place inital config in scene
                string newObjId = ModelCheck.Model.AddObject(config.CatalogObject, config.Location, config.Orientation);
                objectsToPlace.Add(new CatalogObjectPlacement(config, newObjId));
            }

            double bestEval = evaluateModel();
            int interationNum = 0;
            double moveAmount = settings.Movement;
            double reductionRate = settings.Rate;
            int movesPerItteration = settings.Moves;
            while (settings.Itterations > interationNum)
            {
                interationNum++;
                moveAmount *= reductionRate;

                for (int i = 0; i < objectsToPlace.Count; i++)
                {
                    CatalogObjectPlacement currentObject = objectsToPlace[i];

                    //double moveAmount = Math.Pow(Math.E, -alphaMove * interationNum) * MoveSTD;
                    List<Vector3D> locations = new List<Vector3D>();
                    for (int j = 0; j < movesPerItteration; j++)
                    {
                        double deltaX = RandomGausian(0, moveAmount);
                        double deltaY = RandomGausian(0, moveAmount);
                        locations.Add(new Vector3D(currentObject.Location.x + deltaX, currentObject.Location.y + deltaY, currentObject.Location.z));
                    }

                    //remove object
                    ModelCheck.Model.RemoveObject(currentObject.id);

                    objectsToPlace[i].configuration = getBestPlacement(currentObject.configuration, locations, orientations, bestEval);

                    //place object
                    objectsToPlace[i].id = ModelCheck.Model.AddObject(currentObject.CatalogObject, currentObject.Location, currentObject.Orientation);

                    bestEval = evaluateModel();
                }

                // All rules passed so may as well stop
                if (bestEval == ModelCheck.Rules.Count)
                {
                    break;
                }
            }

            // Put the best back in:
            if (settings.ShowRoute)
            {
                foreach (Configuration config in configsList)
                {
                    ModelCheck.Model.AddObject(config.CatalogObject, config.Location, config.Orientation);
                }
            }

            return ModelCheck.Model.FullModel();
        }

        private Configuration getBestPlacement(Configuration objectToPlace, List<Vector3D> locations, List<Vector4D> orientations, double currentEval)
        {
            foreach (Vector3D location in locations)
            {
                foreach (Vector4D orienation in orientations)
                {
                    string newObjId = ModelCheck.Model.AddObject(objectToPlace.CatalogObject, location, orienation);
                    double evalVal = evaluateModel();

                    // Keep the best one
                    if (evalVal > currentEval)
                    {
                        //configsList.Add(new Configuration() { CatalogObject = bestConfig.CatalogObject, Location = bestConfig.Location, Orientation = bestConfig.Orientation });

                        currentEval = evalVal;

                        objectToPlace.Location = location;
                        objectToPlace.Orientation = orienation;
                    }

                    ModelCheck.Model.RemoveObject(newObjId);
                }
            }
            return objectToPlace;
        }

        private double evaluateModel()
        {
            ModelCheck.RecreateVirtualObjects();
            List<RuleResult> results = ModelCheck.CheckModel(0);
            double evalVal = results.Sum(r => r.PassVal);

            return evalVal;
        }

        private double RandomGausian(double mean, double std)
        {
            double u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal = mean + std * randStdNormal; //random normal(mean,stdDev^2)

            return randNormal;
        }

        class CatalogObjectPlacement
        {
            public Configuration configuration;
            public string id;
            public CatalogObject CatalogObject
            {
                get { return configuration.CatalogObject; }
            }
            public Vector3D Location
            {
                get { return configuration.Location; }
                set { configuration.Location = value; }

            }
            public Vector4D Orientation
            {
                get { return configuration.Orientation; }
                set { configuration.Orientation = value; }
            }

            public CatalogObjectPlacement(Configuration _configuration, string _id)
            {
                configuration = _configuration;
                id = _id;
            }
        }
    }
}