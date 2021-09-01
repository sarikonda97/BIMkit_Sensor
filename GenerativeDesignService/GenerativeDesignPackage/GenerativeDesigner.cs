﻿using DbmsApi;
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
    public class GenerativeDesigner
    {
        public ModelChecker ModelCheck { get; internal set; }
        public List<CatalogInitializer> CatalogInitializers { get; internal set; }

        public GenerativeDesigner(Model model, List<Rule> rules, List<CatalogInitializer> catalogInitializers)
        {
            ModelCheck = new ModelChecker(model, rules);
            CatalogInitializers = catalogInitializers;
        }

        public Model ExecuteGenDesignRoundRobin(GenerativeDesignSettings settings)
        {
            // Get all the possible orientations:
            List<Vector4D> orientations = new List<Vector4D>()
            {
                Utils.GetQuaterion(new Vector3D(0, 0, 1), 0.0 * Math.PI / 180.0),
                Utils.GetQuaterion(new Vector3D(0, 0, 1), 90.0 * Math.PI / 180.0),
                Utils.GetQuaterion(new Vector3D(0, 0, 1), 180.0 * Math.PI / 180.0),
                Utils.GetQuaterion(new Vector3D(0, 0, 1), 270.0 * Math.PI / 180.0)
            };

            List<Configuration> configsList = new List<Configuration>();
            foreach(CatalogInitializer catalogInitializer in CatalogInitializers)
            {
                //place inital config in scene
                Configuration config = new Configuration()
                {
                    Location = catalogInitializer.Location,
                    CatalogObject = catalogInitializer.CatalogObject,
                    Orientation = orientations.First()
                };
                string newObjId = ModelCheck.Model.AddObject(config.CatalogObject, config.Location, config.Orientation);
                config.ObjectModelID = newObjId;
                configsList.Add(config);
            }

            foreach (Configuration config in configsList)
            {
                config.Eval = ModelCheck.CheckModel(0, config.CatalogObject.TypeId).Sum(r => r.PassVal);
            }

            double bestEval = ModelCheck.CheckModel(0).Sum(r => r.PassVal);
            int interationNum = 0;
            double moveAmount = settings.Movement;
            double reductionRate = settings.Rate;
            int movesPerItteration = settings.Moves;
            bool itemMoved = false;
            while (settings.Itterations > interationNum)
            {
                itemMoved = false;

                // All rules passed so may as well stop
                if (bestEval == ModelCheck.Rules.Count)
                {
                    break;
                }

                for (int i = 0; i < configsList.Count; i++)
                {
                    Configuration currentConfig = configsList[i];

                    List<Vector3D> locations = new List<Vector3D>();
                    for (int j = 0; j < movesPerItteration; j++)
                    {
                        double deltaX = RandomGausian(0, moveAmount);
                        double deltaY = RandomGausian(0, moveAmount);
                        locations.Add(new Vector3D(currentConfig.Location.x + deltaX, currentConfig.Location.y + deltaY, currentConfig.Location.z));
                    }

                    //remove object
                    ModelCheck.Model.RemoveObject(currentConfig.ObjectModelID);

                    double improveVal = GetBestPlacement(ref currentConfig, locations, orientations);

                    //place object
                    currentConfig.ObjectModelID = ModelCheck.Model.AddObject(currentConfig.CatalogObject, currentConfig.Location, currentConfig.Orientation);

                    //double newEval = ModelCheck.CheckModel(0).Sum(r => r.PassVal);
                    double newEval = bestEval + improveVal;
                    if (bestEval < newEval)
                    {
                        bestEval = newEval;
                        itemMoved = true;

                        // All rules passed so may as well stop
                        if (bestEval == ModelCheck.Rules.Count)
                        {
                            break;
                        }
                    }
                }

                interationNum++;
                if (!itemMoved)
                {
                    moveAmount *= reductionRate;
                }
            }

            return ModelCheck.Model.FullModel();
        }

        private double GetBestPlacement(ref Configuration config, List<Vector3D> newLocations, List<Vector4D> newOrientations)
        {
            double ImprovementVal = 0;
            foreach (Vector3D location in newLocations)
            {
                foreach (Vector4D orienation in newOrientations)
                {
                    string newObjId = ModelCheck.Model.AddObject(config.CatalogObject, location, orienation);
                    double newEval = ModelCheck.CheckModel(0, config.CatalogObject.TypeId).Sum(r => r.PassVal);

                    // Keep the best one
                    if (config.Eval < newEval)
                    {
                        ImprovementVal += newEval - config.Eval;
                        config.Eval = newEval;
                        config.Location = location;
                        config.Orientation = orienation;
                    }

                    ModelCheck.Model.RemoveObject(newObjId);
                }
            }
            return ImprovementVal;
        }

        private Random random = new Random();
        private double RandomGausian(double mean, double std)
        {
            double u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal = mean + std * randStdNormal; //random normal(mean,stdDev^2)

            return randNormal;
        }
    }
}