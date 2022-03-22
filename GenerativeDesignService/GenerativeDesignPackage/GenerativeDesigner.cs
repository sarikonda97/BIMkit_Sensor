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

            // Get the initial scene (global) config score
            ModelCheck.CheckModel(1.0, false);
            scene.Eval = ModelCheck.GetCheckScore();

            // Evaluate all initial configs (When evaluating object configs we only use evaluations relevant to those objects not the whole eval)
            foreach (ObjectConfiguration config in scene.ObjectConfigurations)
            {
                ModelCheck.CheckModel(1.0, true, config.CatalogObject.TypeId);
                config.Eval = ModelCheck.GetCheckScore();
            }

            int interationNum = 0;
            double moveAmount = Settings.Movement;
            double reductionRate = Settings.Rate;
            int movesPerItteration = Settings.Moves;
            bool itemMoved = false;
            while (Settings.Itterations > interationNum)
            {
                itemMoved = false;

                // All rules passed so may as well stop
                if (scene.Eval.TotalScore() == ModelCheck.Rules.Count)
                {
                    break;
                }

                // Going over each object at a time, trying various locations, picking the best, then updating the scene evaluation
                for (int i = 0; i < scene.ObjectConfigurations.Count; i++)
                {
                    ObjectConfiguration currentConfig = scene.ObjectConfigurations[i];
                    List<Vector3D> locations = GetPlacementLocations(floorObjects, moveAmount, movesPerItteration, currentConfig);

                    //remove object
                    ModelCheck.Model.RemoveObject(currentConfig.ObjectModelID);

                    // Find best place outa all options
                    itemMoved = GetBestPlacement(ref currentConfig, locations, Orientations);

                    //place object
                    currentConfig.ObjectModelID = ModelCheck.Model.AddObject(currentConfig.CatalogObject, currentConfig.Location, currentConfig.Orientation);

                    // Update scene evaluation if the item moved
                    if (itemMoved)
                    {
                        // If the item moved then the total scores could have only improved
                        scene.Eval.UpdateCheckScore(currentConfig.Eval);

                        // But the other configs evaulations could be out of date
                        foreach (ObjectConfiguration config in scene.ObjectConfigurations)
                        {
                            config.Eval.UpdateCheckScore(currentConfig.Eval);
                        }

                        // All rules passed so may as well stop
                        if (scene.Eval.TotalScore() == ModelCheck.Rules.Count)
                        {
                            break;
                        }
                    }
                }

                interationNum++;
                if (Settings.FixedItterations || !itemMoved)
                {
                    moveAmount *= reductionRate;
                }
            }

            timer.Stop();
            Console.WriteLine(timer.Elapsed.ToString());
            return ModelCheck.Model.FullModel();
        }

        private bool GetBestPlacement(ref ObjectConfiguration config, List<Vector3D> newLocations, List<Vector4D> newOrientations)
        {
            bool itemMoved = false;
            foreach (Vector3D location in newLocations)
            {
                foreach (Vector4D orienation in newOrientations)
                {
                    string newObjId = ModelCheck.Model.AddObject(config.CatalogObject, location, orienation);
                    double newEval = ModelCheck.CheckModel(1.0, true, config.CatalogObject.TypeId).Sum(r => r.PassVal);
                    CheckScore newScore = ModelCheck.GetCheckScore();

                    // Keep the best one
                    if (config.Eval < newScore)
                    {
                        config.Eval = newScore;
                        config.Location = location;
                        config.Orientation = orienation;
                        itemMoved = true;
                    }

                    ModelCheck.Model.RemoveObject(newObjId);
                }
            }
            return itemMoved;
        }

        public List<Tuple<Rule, Type, MethodInfo>> GetCompiledRules()
        {
            return ModelCheck.CompiledRules;
        }
    }

    // ==================================================================================================================

    public class GenerativeDesignerThread : GenerativeDesignSuperClass
    {
        // The part that is threaded is the multiple location and orientation checks that happen for each object
        private List<ThreadConfiguration> ThreadConfigurations;
        private int ThreadCount;

        public GenerativeDesignerThread(Model model, List<Rule> rules, List<CatalogInitializer> catalogInitializers, GenerativeDesignSettings settings)
        {
            CatalogInitializers = catalogInitializers;
            Settings = settings;

            // The +1 is because one will always keep the best eval so far for the rest to compare against
            ThreadCount = Settings.Moves * Orientations.Count + 1;
            ThreadConfigurations = new List<ThreadConfiguration>();
            List<Tuple<Rule, Type, MethodInfo>> compiledRules = null;
            for (int i = 0; i < ThreadCount; i++)
            {
                ThreadConfigurations.Add(new ThreadConfiguration()
                {
                    ModelChecker = compiledRules == null ? new ModelChecker(model, rules) : new ModelChecker(model, compiledRules)
                });
            }
        }

        public Model ExecuteGenDesignRoundRobin()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // Only get points above a floor:
            List<RuleCheckObject> floorObjects = ThreadConfigurations.First().ModelChecker.Model.Objects.Where(o => o.Type == "Floor").ToList();

            // All threads get the full scene with all objects placed in their starting locations
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

            // The first thread holds the best location for all objects and so is the master thread
            ThreadConfigurations[0].ModelChecker.CheckModel(1.0, false);
            ThreadConfigurations[0].SceneConfiguration.Eval = ThreadConfigurations[0].ModelChecker.GetCheckScore();
            foreach (ObjectConfiguration config in ThreadConfigurations[0].SceneConfiguration.ObjectConfigurations)
            {
                ThreadConfigurations[0].ModelChecker.CheckModel(1.0, false, config.CatalogObject.TypeId);
                config.Eval = ThreadConfigurations[0].ModelChecker.GetCheckScore();
            }
            ThreadConfiguration bestThreadForScene = ThreadConfigurations[0];

            int interationNum = 0;
            double moveAmount = Settings.Movement;
            double reductionRate = Settings.Rate;
            int movesPerItteration = Settings.Moves;
            while (Settings.Itterations > interationNum)
            {
                bestThreadForScene = ThreadConfigurations[0];
                if (bestThreadForScene.SceneConfiguration.Eval.TotalScore() == bestThreadForScene.ModelChecker.Rules.Count)
                {
                    break;
                }

                // Itterate over each object i to find the best spot for it
                for (int i = 0; i < bestThreadForScene.SceneConfiguration.ObjectConfigurations.Count; i++)
                {
                    // Sort the threads by scene evaluation and the bes one will be the one they all edit from
                    ThreadConfigurations = ThreadConfigurations.OrderByDescending(t => t.SceneConfiguration.Eval).ToList();
                    bestThreadForScene = ThreadConfigurations[0];
                    if (bestThreadForScene.SceneConfiguration.Eval.TotalScore() == bestThreadForScene.ModelChecker.Rules.Count)
                    {
                        break;
                    }

                    // Get all the locations around the current best configuration
                    ObjectConfiguration bestObjectConfigFori = bestThreadForScene.SceneConfiguration.ObjectConfigurations[i];
                    List<Vector3D> locations = GetPlacementLocations(floorObjects, moveAmount, movesPerItteration, bestObjectConfigFori);

                    // Get all object location and orientation pairs:
                    List<Tuple<Vector3D, Vector4D>> locOrientPairs = new List<Tuple<Vector3D, Vector4D>>();
                    foreach (Vector3D location in locations)
                    {
                        foreach (Vector4D orientation in Orientations)
                        {
                            locOrientPairs.Add(new Tuple<Vector3D, Vector4D>(location, orientation));
                        }
                    }
                    if (locOrientPairs.Count == 0)
                    {
                        continue;
                    }

                    // Parallel:
                        Task[] tasks = new Task[ThreadCount - 1];
                    // In all Threads (not including the first), update the ith item so it is at location and orientation pair j and then evaluate
                    for (int j = 1; j < ThreadCount; j++)
                    {
                        int val = j;
                        Task newTask = Task.Run(() =>
                        {
                            ThreadConfiguration currentThreadConfig = ThreadConfigurations[val];

                            // k is the previous item that needs to be placed in the location of the best config
                            int k = (i - 1 + currentThreadConfig.SceneConfiguration.ObjectConfigurations.Count) % currentThreadConfig.SceneConfiguration.ObjectConfigurations.Count;
                            ObjectConfiguration configK = currentThreadConfig.SceneConfiguration.ObjectConfigurations[k];
                            ObjectConfiguration bestConfigK = bestThreadForScene.SceneConfiguration.ObjectConfigurations[k];

                            // Make it match the best config again by removing the item that was last moved and putting it in the best config spot again
                            configK.Location = bestConfigK.Location;
                            configK.Orientation = bestConfigK.Orientation;
                            configK.ObjectModelID = currentThreadConfig.ModelChecker.Model.MoveObject(configK.ObjectModelID, configK.CatalogObject, configK.Location, configK.Orientation);
                            configK.Eval = bestConfigK.Eval;
                            currentThreadConfig.SceneConfiguration.Eval = bestThreadForScene.SceneConfiguration.Eval;
                            // At this point, all threads and evaluations should look the same!

                            // Now move the ith item to its location and orientation and see if it makes it better
                            ObjectConfiguration oConfig = currentThreadConfig.SceneConfiguration.ObjectConfigurations[i];
                            int locOrientIndex = (val - 1) % locOrientPairs.Count; // Use the mod incase some locations were over floors so they were removed but still have more threads (probably just shouldn't even run this thread)
                            oConfig.Location = locOrientPairs[locOrientIndex].Item1;
                            oConfig.Orientation = locOrientPairs[locOrientIndex].Item2;
                            oConfig.ObjectModelID = currentThreadConfig.ModelChecker.Model.MoveObject(oConfig.ObjectModelID, oConfig.CatalogObject, oConfig.Location, oConfig.Orientation);
                            currentThreadConfig.ModelChecker.CheckModel(1.0, true, oConfig.CatalogObject.TypeId);
                            CheckScore newEval = currentThreadConfig.ModelChecker.GetCheckScore();
                            currentThreadConfig.SceneConfiguration.Eval.UpdateCheckScore(oConfig.Eval);
                            oConfig.Eval = newEval;
                        });

                        tasks[j - 1] = newTask;
                    }

                    Task.WaitAll(tasks);
                }

                interationNum++;
                ThreadConfigurations = ThreadConfigurations.OrderByDescending(t => t.SceneConfiguration.Eval).ToList();
                if (Settings.FixedItterations || ThreadConfigurations[0] == bestThreadForScene)
                {
                    // Only if the no better thread was found then we reduce the move amounts (a smaller move is required)
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

                //if (j < (movesPerItteration / 3.0))
                //{
                //    deltaY = 0;
                //}
                //else if (j < (movesPerItteration * 2.0 / 3.0))
                //{
                //    deltaX = 0;
                //}

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