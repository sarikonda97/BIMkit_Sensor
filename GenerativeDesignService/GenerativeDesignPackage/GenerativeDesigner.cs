using DbmsApi;
using DbmsApi.API;
using MathPackage;
using ModelCheckPackage;
using RuleAPI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenerativeDesignPackage
{
    public class GenerativeDesigner : GenerativeDesignSuperClass
    {
        private ModelChecker ModelCheck;

        public GenerativeDesigner(Model model, List<Rule> rules, List<CatalogInitializer> catalogInitializers, GenerativeDesignSettings settings)
        {
            CatalogInitializers = catalogInitializers;
            Settings = settings;

            ModelCheck = new ModelChecker(model, rules);
        }

        public GenerativeDesigner(Model model, List<Tuple<Rule, Type, MethodInfo>> compiledRules, List<CatalogInitializer> catalogInitializers, GenerativeDesignSettings settings)
        {
            CatalogInitializers = catalogInitializers;
            Settings = settings;

            ModelCheck = new ModelChecker(model, compiledRules);
        }

        public Model ExecuteGenDesignRoundRobin()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // Only get points above a floor:
            List<RuleCheckObject> floorObjects = ModelCheck.Model.Objects.Where(o => o.Type == "Floor").ToList();

            SceneConfiguration scene = new SceneConfiguration()
            {
                ObjectConfigurations = new List<ObjectConfiguration>()
            };
            foreach (CatalogInitializer catalogInitializer in CatalogInitializers)
            {
                //place inital config in scene
                ObjectConfiguration config = new ObjectConfiguration()
                {
                    Location = catalogInitializer.Location,
                    CatalogObject = catalogInitializer.CatalogObject,
                    Orientation = Orientations.First()
                };
                string newObjId = ModelCheck.Model.AddObject(config.CatalogObject, config.Location, config.Orientation);
                config.ObjectModelID = newObjId;
                scene.ObjectConfigurations.Add(config);
            }

            foreach (ObjectConfiguration config in scene.ObjectConfigurations)
            {
                config.Eval = ModelCheck.CheckModel(0, config.CatalogObject.TypeId).Sum(r => r.PassVal);
            }

            scene.Eval = ModelCheck.CheckModel(0).Sum(r => r.PassVal);
            int interationNum = 0;
            double moveAmount = Settings.Movement;
            double reductionRate = Settings.Rate;
            int movesPerItteration = Settings.Moves;
            bool itemMoved = false;
            while (Settings.Itterations > interationNum)
            {
                itemMoved = false;

                // All rules passed so may as well stop
                if (scene.Eval == ModelCheck.Rules.Count)
                {
                    break;
                }

                for (int i = 0; i < scene.ObjectConfigurations.Count; i++)
                {
                    ObjectConfiguration currentConfig = scene.ObjectConfigurations[i];
                    List<Vector3D> locations = GetPlacementLocations(floorObjects, moveAmount, movesPerItteration, currentConfig);

                    //remove object
                    ModelCheck.Model.RemoveObject(currentConfig.ObjectModelID);

                    double improveVal = GetBestPlacement(ref currentConfig, locations, Orientations);

                    //place object
                    currentConfig.ObjectModelID = ModelCheck.Model.AddObject(currentConfig.CatalogObject, currentConfig.Location, currentConfig.Orientation);

                    //double newEval = ModelCheck.CheckModel(0).Sum(r => r.PassVal);
                    double newEval = scene.Eval + improveVal;
                    if (scene.Eval < newEval)
                    {
                        scene.Eval = newEval;
                        itemMoved = true;

                        // All rules passed so may as well stop
                        if (scene.Eval == ModelCheck.Rules.Count)
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

            timer.Stop();
            Console.WriteLine(timer.Elapsed.ToString());
            return ModelCheck.Model.FullModel();
        }

        private double GetBestPlacement(ref ObjectConfiguration config, List<Vector3D> newLocations, List<Vector4D> newOrientations)
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
    }

    // ==================================================================================================================

    public class GenerativeDesignerThread : GenerativeDesignSuperClass
    {
        private List<ThreadConfiguration> ThreadConfigurations;
        private int ThreadCount;

        public GenerativeDesignerThread(Model model, List<Rule> rules, List<CatalogInitializer> catalogInitializers, GenerativeDesignSettings settings)
        {
            CatalogInitializers = catalogInitializers;
            Settings = settings;

            // The +1 is because one will always keep the best eval so far for the rest to compare against
            ThreadCount = Settings.Moves * Orientations.Count + 1;
            ThreadConfigurations = new List<ThreadConfiguration>();
            for (int i = 0; i < ThreadCount; i++)
            {
                ThreadConfigurations.Add(new ThreadConfiguration()
                {
                    ModelChecker = new ModelChecker(model, rules)
                });
            }
        }

        public Model ExecuteGenDesignRoundRobin()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // Only get points above a floor:
            List<RuleCheckObject> floorObjects = ThreadConfigurations.First().ModelChecker.Model.Objects.Where(o => o.Type == "Floor").ToList();

            foreach (ThreadConfiguration threadConfiguration in ThreadConfigurations)
            {
                threadConfiguration.SceneConfiguration = new SceneConfiguration()
                {
                    ObjectConfigurations = new List<ObjectConfiguration>()
                };
                foreach (CatalogInitializer catalogInitializer in CatalogInitializers)
                {
                    //place inital config in scene
                    ObjectConfiguration config = new ObjectConfiguration()
                    {
                        Location = catalogInitializer.Location,
                        CatalogObject = catalogInitializer.CatalogObject,
                        Orientation = Orientations.First()
                    };
                    string newObjId = threadConfiguration.ModelChecker.Model.AddObject(config.CatalogObject, config.Location, config.Orientation);
                    config.ObjectModelID = newObjId;
                    threadConfiguration.SceneConfiguration.ObjectConfigurations.Add(config);
                }
            }

            ThreadConfigurations[0].SceneConfiguration.Eval = ThreadConfigurations[0].ModelChecker.CheckModel(0).Sum(r => r.PassVal);
            foreach (ObjectConfiguration config in ThreadConfigurations[0].SceneConfiguration.ObjectConfigurations)
            {
                config.Eval = ThreadConfigurations[0].ModelChecker.CheckModel(0, config.CatalogObject.TypeId).Sum(r => r.PassVal);
            }

            int interationNum = 0;
            double moveAmount = Settings.Movement;
            double reductionRate = Settings.Rate;
            int movesPerItteration = Settings.Moves;
            ThreadConfiguration bestThreadForConfig = ThreadConfigurations[0];
            ThreadConfiguration bestThreadForScene = ThreadConfigurations[0];
            while (Settings.Itterations > interationNum)
            {
                bestThreadForScene = ThreadConfigurations[0];
                if (bestThreadForScene.SceneConfiguration.Eval == bestThreadForScene.ModelChecker.Rules.Count)
                {
                    break;
                }

                for (int i = 0; i < bestThreadForScene.SceneConfiguration.ObjectConfigurations.Count; i++)
                {
                    ThreadConfigurations = ThreadConfigurations.OrderByDescending(t => t.SceneConfiguration.Eval).ToList();
                    bestThreadForConfig = ThreadConfigurations[0];
                    if (bestThreadForConfig.SceneConfiguration.Eval == bestThreadForConfig.ModelChecker.Rules.Count)
                    {
                        break;
                    }

                    ObjectConfiguration bestObjectConfigFori = bestThreadForScene.SceneConfiguration.ObjectConfigurations[i];
                    List<Vector3D> locations = GetPlacementLocations(floorObjects, moveAmount, movesPerItteration, bestObjectConfigFori);
                    if (locations.Count == 0)
                    {
                        continue;
                    }

                    // Get all object location and orientation pairs:
                    List<Tuple<Vector3D, Vector4D>> locOrientPairs = new List<Tuple<Vector3D, Vector4D>>();
                    foreach (Vector3D location in locations)
                    {
                        foreach (Vector4D orientation in Orientations)
                        {
                            locOrientPairs.Add(new Tuple<Vector3D, Vector4D>(location, orientation));
                        }
                    }

                    // Parallel:
                    Task[] tasks = new Task[ThreadCount - 1];
                    // In all Threads (not including the first since it it the one to beat), update the ith item so it is at a new  location and orientation, then evaluate it
                    for (int j = 1; j < ThreadCount; j++)
                    {
                        int val = j;
                        Task newTask = Task.Run(() =>
                        {
                            ThreadConfiguration currentThreadConfig = ThreadConfigurations[val];
                            int k = (i - 1 + currentThreadConfig.SceneConfiguration.ObjectConfigurations.Count) % currentThreadConfig.SceneConfiguration.ObjectConfigurations.Count;
                            ObjectConfiguration configK = currentThreadConfig.SceneConfiguration.ObjectConfigurations[k];
                            ObjectConfiguration bestConfigK = bestThreadForConfig.SceneConfiguration.ObjectConfigurations[k];
                            ObjectConfiguration oConfig = currentThreadConfig.SceneConfiguration.ObjectConfigurations[i];
                            oConfig.Location = locOrientPairs[val - 1].Item1;
                            oConfig.Orientation = locOrientPairs[val - 1].Item2;
                            // Make it match the best config again by removing the item that was last moved and putting it in the best spot again (k should be known as i-1):
                            configK.Location = bestConfigK.Location;
                            configK.Orientation = bestConfigK.Orientation;
                            configK.ObjectModelID = currentThreadConfig.ModelChecker.Model.MoveObject(configK.ObjectModelID, configK.CatalogObject, configK.Location, configK.Orientation);
                            configK.Eval = bestConfigK.Eval;
                            currentThreadConfig.SceneConfiguration.Eval = bestThreadForConfig.SceneConfiguration.Eval;
                            // At this point, all threads and evaluations should look the same!

                            // Move the item to its location and orientation and see if it makes it better
                            oConfig.ObjectModelID = currentThreadConfig.ModelChecker.Model.MoveObject(oConfig.ObjectModelID, oConfig.CatalogObject, oConfig.Location, oConfig.Orientation);
                            double newEval = currentThreadConfig.ModelChecker.CheckModel(0, oConfig.CatalogObject.TypeId).Sum(r => r.PassVal);
                            double improveVal = newEval - oConfig.Eval;
                            currentThreadConfig.SceneConfiguration.Eval += improveVal;
                            oConfig.Eval = newEval;
                        });
                        tasks[j - 1] = newTask;
                    }

                    Task.WaitAll(tasks);
                }

                interationNum++;
                ThreadConfigurations = ThreadConfigurations.OrderByDescending(t => t.SceneConfiguration.Eval).ToList();
                if (ThreadConfigurations[0] == bestThreadForScene)
                {
                    // Only if the no better thread was found then we reduce the move amounts
                    moveAmount *= reductionRate;
                }
            }

            timer.Stop();
            Console.WriteLine(timer.Elapsed.ToString());
            return bestThreadForScene.ModelChecker.Model.FullModel();
        }
    }

    public abstract class GenerativeDesignSuperClass
    {
        protected GenerativeDesignSettings Settings;
        protected List<CatalogInitializer> CatalogInitializers;
        protected List<Vector4D> Orientations = new List<Vector4D>()
        {
            Utils.GetQuaterion(new Vector3D(0, 0, 1), 0.0 * Math.PI / 180.0),
            Utils.GetQuaterion(new Vector3D(0, 0, 1), 90.0 * Math.PI / 180.0),
            Utils.GetQuaterion(new Vector3D(0, 0, 1), 180.0 * Math.PI / 180.0),
            Utils.GetQuaterion(new Vector3D(0, 0, 1), 270.0 * Math.PI / 180.0)
        };

        protected List<Vector3D> GetPlacementLocations(List<RuleCheckObject> floorObjects, double moveAmount, int movesPerItteration, ObjectConfiguration configuration)
        {
            List<Vector3D> locations = new List<Vector3D>();
            for (int j = 0; j < movesPerItteration; j++)
            {
                double deltaX = Utils.RandomGausian(0, moveAmount);
                double deltaY = Utils.RandomGausian(0, moveAmount);

                if (j < (movesPerItteration / 3.0))
                {
                    deltaY = 0;
                }
                else if (j < (movesPerItteration * 2.0 / 3.0))
                {
                    deltaX = 0;
                }

                Vector3D newLoc = new Vector3D(configuration.Location.x + deltaX, configuration.Location.y + deltaY, configuration.Location.z);

                // Check if the object is above the floor or not:
                if (floorObjects.Any(floor => Utils.RayIntersectsMesh(newLoc, new Vector3D(0, 0, -1), floor.GetGlobalMesh())))
                {
                    locations.Add(newLoc);
                }
            }

            return locations;
        }
    }
}