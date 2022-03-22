using DbmsApi;
using DbmsApi.API;
using GenerativeDesignAPI;
using GenerativeDesignPackage;
using MathPackage;
using ModelCheckAPI;
using ModelCheckPackage;
using RuleAPI;
using RuleAPI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VoxelService;
using Rule = RuleAPI.Models.Rule;

namespace MCGDApp
{
    public partial class Main : Form
    {
        private RuleAPIController RuleAPIController;
        private DBMSAPIController DBMSController;
        private MCAPIController MCAPIController;
        private GDAPIController GDAPIController;
        private string rsURL = "https://localhost:44370//api/";
        private string dbmsURL = "https://localhost:44322//api/";
        private string mcURL = "https://localhost:44346//api/";
        private string gdURL = "https://localhost:44328///api/";

        private ModelChecker ModelChecker;
        private GenerativeDesignSettings GDSettings;

        public Main()
        {
            InitializeComponent();

            DBMSController = new DBMSAPIController(dbmsURL);
            RuleAPIController = new RuleAPIController(rsURL);
            MCAPIController = new MCAPIController(mcURL);
            GDAPIController = new GDAPIController(gdURL);

            GDSettings = new GenerativeDesignSettings();

            GetTypes();
        }

        private async Task GetTypes()
        {
            APIResponse<List<ObjectType>> response3 = await DBMSController.GetTypes();
            if (response3.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response3.ReasonPhrase);
                return;
            }
            ObjectTypeTree.BuildTypeTree(response3.Data);
        }

        private async void buttonSignInDBMS_Click(object sender, EventArgs e)
        {
            APIResponse<TokenData> response = await DBMSController.LoginAsync(this.textBoxDBMSUsername.Text, this.textBoxDBMSPassword.Text);
            if (response.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response.ReasonPhrase);
                return;
            }

            APIResponse<List<ModelMetadata>> response2 = await DBMSController.GetAvailableModels();
            if (response2.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response2.ReasonPhrase);
                return;
            }

            this.listBoxModelList.Items.Clear();
            List<ModelMetadata> models = response2.Data;
            foreach (ModelMetadata mm in models)
            {
                this.listBoxModelList.Items.Add(mm);
            }

            APIResponse<List<CatalogObjectMetadata>> response3 = await DBMSController.GetAvailableCatalogObjects();
            if (response3.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response3.ReasonPhrase);
                return;
            }

            this.listBoxCatalogList.Items.Clear();
            List<CatalogObjectMetadata> catalogObjects = response3.Data;
            foreach (CatalogObjectMetadata com in catalogObjects)
            {
                this.listBoxCatalogList.Items.Add(com);
            }

            this.listBoxSelectedCatalogList.Items.Clear();
        }

        private async void buttonSignInRMS_Click(object sender, EventArgs e)
        {
            APIResponse<RuleUser> response = await RuleAPIController.LoginAsync(this.textBoxRMSUsername.Text);
            if (response.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response.ReasonPhrase);
                return;
            }

            APIResponse<List<RuleSet>> response2 = await RuleAPIController.GetAllRuleSetsAsync();
            if (response2.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response2.ReasonPhrase);
                return;
            }

            List<RuleSet> ruleSets = response2.Data;
            this.treeViewRules.Nodes.Clear();
            foreach (RuleSet rs in ruleSets)
            {
                TreeNode ruleSetTn = new TreeNode(rs.Name);
                ruleSetTn.Tag = null;
                this.treeViewRules.Nodes.Add(ruleSetTn);

                foreach (Rule r in rs.Rules)
                {
                    TreeNode ruleTn = new TreeNode(r.Name);
                    ruleTn.Tag = r;
                    ruleSetTn.Nodes.Add(ruleTn);
                }
            }
            this.treeViewRules.ExpandAll();
        }

        private void treeViewRules_AfterCheck(object sender, TreeViewEventArgs e)
        {
            foreach (TreeNode n in e.Node.Nodes)
            {
                n.Checked = e.Node.Checked;
            }
        }

        private async void buttonModelCheck_Click(object sender, EventArgs e)
        {
            this.listBoxRuleResults.Items.Clear();

            // Get the model and rules
            ModelMetadata modelMetaData = this.listBoxModelList.SelectedItem as ModelMetadata;
            APIResponse<Model> response = await DBMSController.GetModel(new ItemRequest(modelMetaData.ModelId, LevelOfDetail.LOD500));
            if (response.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response.ReasonPhrase);
                return;
            }

            Model model = response.Data;
            List<Rule> rules = GetCheckedRules(this.treeViewRules.Nodes);

            // Execute
            ModelChecker = new ModelChecker(model, rules);
            List<RuleResult> ruleResults = ModelChecker.CheckModel(0, false);
            foreach (RuleResult rr in ruleResults)
            {
                this.listBoxRuleResults.Items.Add(rr);
            }
        }

        private async void buttonCheckService_Click(object sender, EventArgs e)
        {
            ModelMetadata modelMetaData = this.listBoxModelList.SelectedItem as ModelMetadata;
            List<Rule> rules = GetCheckedRules(this.treeViewRules.Nodes);

            CheckRequest request = new CheckRequest(DBMSController.Token, RuleAPIController.CurrentUser.Username, modelMetaData.ModelId, rules.Select(r => r.Id).ToList(), LevelOfDetail.LOD500);

            APIResponse<List<RuleResult>> response = await MCAPIController.PerformModelCheck(request);
            if (response.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response.ReasonPhrase);
                return;
            }

            this.listBoxRuleResults.Items.Clear();
            foreach (RuleResult rr in response.Data)
            {
                this.listBoxRuleResults.Items.Add(rr);
            }
        }

        private List<Rule> GetCheckedRules(TreeNodeCollection nodes)
        {
            List<Rule> tempList = new List<Rule>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Checked && nodes[i].Tag != null)
                {
                    tempList.Add(nodes[i].Tag as Rule);
                }
                tempList.AddRange(GetCheckedRules(nodes[i].Nodes));
            }
            return tempList;
        }

        private void listBoxRuleResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            RuleResult selectedResult = this.listBoxRuleResults.SelectedItem as RuleResult;

            treeViewRuleInstance.Nodes.Clear();
            foreach (RuleInstance ruleInstance in selectedResult.RuleInstances)
            {
                TreeNode ruleInstanceTN = new TreeNode("Objects: " + ruleInstance.Objs.Count + ", " + ruleInstance.PassVal.ToString());
                foreach (RuleCheckObject obj in ruleInstance.Objs)
                {
                    TreeNode objTN = new TreeNode("Name: " + obj.Name + " \nType: " + obj.Type);
                    foreach (Property prop in obj.Properties)
                    {
                        TreeNode propTN = new TreeNode(prop.String());
                        objTN.Nodes.Add(propTN);
                    }

                    ruleInstanceTN.Nodes.Add(objTN);
                }

                foreach (RuleCheckRelation rel in ruleInstance.Rels)
                {
                    TreeNode relTN = new TreeNode(rel.FirstObj.Name + " => " + rel.SecondObj.Name);
                    foreach (Property prop in rel.Properties)
                    {
                        TreeNode propTN = new TreeNode(prop.String());
                        relTN.Nodes.Add(propTN);
                    }

                    ruleInstanceTN.Nodes.Add(relTN);
                }

                treeViewRuleInstance.Nodes.Add(ruleInstanceTN);
            }
        }

        private List<CatalogObjectMetadata> GetCheckedObjects()
        {
            List<CatalogObjectMetadata> selectedObjects = new List<CatalogObjectMetadata>();
            foreach (CatalogObjectMetadata com in this.listBoxSelectedCatalogList.Items)
            {
                selectedObjects.Add(com);
            }
            return selectedObjects;
        }

        private async void buttonGDLocal_Click(object sender, EventArgs e)
        {
            // Get the model, object, and rules
            ModelMetadata modelMetaData = this.listBoxModelList.SelectedItem as ModelMetadata;
            APIResponse<Model> response = await DBMSController.GetModel(new ItemRequest(modelMetaData.ModelId, LevelOfDetail.LOD100));
            if (response.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response.ReasonPhrase);
                return;
            }

            Model model = response.Data;

            // Find the center of the floor:
            Vector3D ModelStartingLocation = new Vector3D();
            ModelObject floorObject = model.ModelObjects.Where(o => o.TypeId == "Floor").First();
            if (floorObject != null)
            {
                ModelStartingLocation = new Vector3D(floorObject.Location.x, floorObject.Location.y, floorObject.Components.Max(c => c.Vertices.Max(v => v.z)));
            }

            List<CatalogObjectMetadata> catalogObjectsMeta = GetCheckedObjects();
            List<CatalogInitializer> catalogObjectsInits = new List<CatalogInitializer>();
            foreach (var catalogObjectMeta in catalogObjectsMeta)
            {
                APIResponse<CatalogObject> response2 = await DBMSController.GetCatalogObject(new ItemRequest(catalogObjectMeta.CatalogObjectId, LevelOfDetail.LOD100));
                if (response2.Code != System.Net.HttpStatusCode.OK)
                {
                    MessageBox.Show(response2.ReasonPhrase);
                    return;
                }

                CatalogObject catalogObject = response2.Data;
                float minZ = (float)catalogObject.Components.Min(c => c.Vertices.Min(v => v.z));
                float maxZ = (float)catalogObject.Components.Max(c => c.Vertices.Max(v => v.z));
                float heightOfset = (maxZ - minZ) / 2.0f + 0.0001f; // We want it slightly off the ground for overlap purposes

                catalogObjectsInits.Add(new CatalogInitializer()
                {
                    CatalogObject = catalogObject,
                    Location = new Vector3D(ModelStartingLocation.x, ModelStartingLocation.y, ModelStartingLocation.z + heightOfset)
                });
            }

            List<Rule> rules = GetCheckedRules(this.treeViewRules.Nodes);

            //GenerativeDesigner generativeDesigner = new GenerativeDesigner(model, rules, catalogObjectsInits, GDSettings);
            GenerativeDesignerThread generativeDesigner = new GenerativeDesignerThread(model, rules, catalogObjectsInits, GDSettings);
            Model newModel = generativeDesigner.ExecuteGenDesignRoundRobin();

            // Save the model:
            newModel.Name = "Generated Model";
            APIResponse<string> response3 = await DBMSController.CreateModel(newModel);
            if (response3.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response3.ReasonPhrase);
                return;
            }

            buttonSignInDBMS_Click(null, null);

            this.richTextBoxGenDesign.Text = "Done";
        }

        private async void buttonGDWeb_Click(object sender, EventArgs e)
        {
            ModelMetadata modelMetaData = this.listBoxModelList.SelectedItem as ModelMetadata;
            List<Rule> rules = GetCheckedRules(this.treeViewRules.Nodes);
            List<CatalogObjectMetadata> catalogObjectsMeta = GetCheckedObjects();
            List<CatalogInitializerID> catalogObjectsInits = new List<CatalogInitializerID>();
            foreach (var catalogObjectMeta in catalogObjectsMeta)
            {
                // Normaly wouldnt fetch the object it but need to get the height somehow here
                APIResponse<CatalogObject> response2 = await DBMSController.GetCatalogObject(new ItemRequest(catalogObjectMeta.CatalogObjectId, LevelOfDetail.LOD100));
                if (response2.Code != System.Net.HttpStatusCode.OK)
                {
                    MessageBox.Show(response2.ReasonPhrase);
                    return;
                }

                CatalogObject catalogObject = response2.Data;
                float minZ = (float)catalogObject.Components.Min(c => c.Vertices.Min(v => v.z));
                float maxZ = (float)catalogObject.Components.Max(c => c.Vertices.Max(v => v.z));
                float heightOfset = (maxZ - minZ) / 2.0f + 0.0001f;
                catalogObjectsInits.Add(new CatalogInitializerID() { CatalogID = catalogObject.CatalogID, Location = new Vector3D(0, 0, heightOfset) });
            }

            GenerativeRequest request = new GenerativeRequest(DBMSController.Token,
                                                              RuleAPIController.CurrentUser.Username,
                                                              modelMetaData.ModelId,
                                                              catalogObjectsInits,
                                                              rules.Select(r => r.Id).ToList(),
                                                              LevelOfDetail.LOD100,
                                                              GDSettings
                                                              );

            APIResponse<string> response = await GDAPIController.PerformGenDesign(request);
            if (response.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response.ReasonPhrase);
                return;
            }

            buttonSignInDBMS_Click(null, null);

            this.richTextBoxGenDesign.Text = "Done";
        }

        private async void buttonGDSeq_Click(object sender, EventArgs e)
        {
            // Get the model, object, and rules
            List<Rule> rules = GetCheckedRules(this.treeViewRules.Nodes);
            List<CatalogObjectMetadata> catalogObjectsMeta = GetCheckedObjects();
            ModelMetadata modelMetaData = this.listBoxModelList.SelectedItem as ModelMetadata;
            if (modelMetaData == null || rules.Count == 0 || catalogObjectsMeta.Count == 0)
            {
                MessageBox.Show("Select a model, rules, and objects");
                return;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            APIResponse<Model> response = await DBMSController.GetModel(new ItemRequest(modelMetaData.ModelId, LevelOfDetail.LOD100));
            if (response.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response.ReasonPhrase);
                return;
            }

            Model model = response.Data;

            // Find the center of the floor:
            Vector3D ModelStartingLocation = new Vector3D();
            ModelObject floorObject = model.ModelObjects.Where(o => o.TypeId == "Floor").First();
            if (floorObject != null)
            {
                ModelStartingLocation = new Vector3D(floorObject.Location.x, floorObject.Location.y, floorObject.Components.Max(c => c.Vertices.Max(v => v.z)));
            }

            List<CatalogInitializer> catalogObjectsInits = new List<CatalogInitializer>();
            foreach (var catalogObjectMeta in catalogObjectsMeta)
            {
                APIResponse<CatalogObject> response2 = await DBMSController.GetCatalogObject(new ItemRequest(catalogObjectMeta.CatalogObjectId, LevelOfDetail.LOD100));
                if (response2.Code != System.Net.HttpStatusCode.OK)
                {
                    MessageBox.Show(response2.ReasonPhrase);
                    return;
                }

                CatalogObject catalogObject = response2.Data;
                float minZ = (float)catalogObject.Components.Min(c => c.Vertices.Min(v => v.z));
                float maxZ = (float)catalogObject.Components.Max(c => c.Vertices.Max(v => v.z));
                float heightOfset = (maxZ - minZ) / 2.0f + 0.0001f; // We want it slightly off the ground for overlap purposes

                catalogObjectsInits.Add(new CatalogInitializer()
                {
                    CatalogObject = catalogObject,
                    Location = new Vector3D(ModelStartingLocation.x, ModelStartingLocation.y, ModelStartingLocation.z + heightOfset)
                });
            }

            Model newModel = null;
            List<Tuple<Rule, Type, MethodInfo>> compliledRules = null;
            foreach (var catalogItem in catalogObjectsInits)
            {
                GenerativeDesigner generativeDesigner;
                if (compliledRules == null)
                {
                    generativeDesigner = new GenerativeDesigner(model, rules, new List<CatalogInitializer>() { catalogItem }, GDSettings);
                    compliledRules = generativeDesigner.GetCompiledRules();
                }
                else
                {
                    generativeDesigner = new GenerativeDesigner(model, compliledRules, new List<CatalogInitializer>() { catalogItem }, GDSettings);
                }

                newModel = generativeDesigner.ExecuteGenDesignRoundRobin();
            }

            // Save the model:
            newModel.Name = "Generated Model";
            APIResponse<string> response3 = await DBMSController.CreateModel(newModel);
            if (response3.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response3.ReasonPhrase);
                return;
            }

            buttonSignInDBMS_Click(null, null);
            ModelChecker = new ModelChecker(newModel, compliledRules);
            List<RuleResult> ruleResults = ModelChecker.CheckModel(0, false);
            CheckScore cs = ModelChecker.GetCheckScore();

            this.richTextBoxGenDesign.Text = "Done\nRuntime: " + sw.Elapsed.ToString() +
                                             "\nCheck Result: " + cs.TotalScore() + "/" + rules.Count +
                                             "\nErrors: " + cs.ErrorScore + "/" + +rules.Count(r => r.ErrorLevel == ErrorLevel.Error) +
                                             "\nWarning: " + cs.WarningScore + "/" + +rules.Count(r => r.ErrorLevel == ErrorLevel.Warning) +
                                             "\nRecommended: " + cs.RecommendScore + "/" + rules.Count(r => r.ErrorLevel == ErrorLevel.Recommended);
        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void buttonRight_Click(object sender, EventArgs e)
        {
            if (this.listBoxCatalogList.SelectedItem == null)
            {
                return;
            }
            this.listBoxSelectedCatalogList.Items.Add(this.listBoxCatalogList.SelectedItem);
        }

        private void buttonLeft_Click(object sender, EventArgs e)
        {
            if (this.listBoxSelectedCatalogList.SelectedItem == null)
            {
                return;
            }
            if (this.listBoxSelectedCatalogList.Items.Count > 0)
            {
                this.listBoxSelectedCatalogList.Items.RemoveAt(this.listBoxSelectedCatalogList.SelectedIndex);
            }
        }

        private void buttonGDSettings_Click(object sender, EventArgs e)
        {
            GenerativeDesignSettingsForm gdsf = new GenerativeDesignSettingsForm(GDSettings);
            if (gdsf.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            GDSettings = gdsf.Settings;
        }

        private async void buttonVoxelCreator_Click(object sender, EventArgs e)
        {
            ModelMetadata modelMetaData = this.listBoxModelList.SelectedItem as ModelMetadata;
            APIResponse<Model> response = await DBMSController.GetModel(new ItemRequest(modelMetaData.ModelId, LevelOfDetail.LOD500));
            if (response.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response.ReasonPhrase);
                return;
            }

            Model model = response.Data;

            VoxelCreater voxelCreater = new VoxelCreater(model);
            List<Voxel> voxels = voxelCreater.CreateVoxels(0.5);
        }

        private void buttonRuleTypeOrder_Click(object sender, EventArgs e)
        {
            List<Rule> rules = GetCheckedRules(this.treeViewRules.Nodes);

            List<string> typeOrder = GetRecommendedTypeOrderFromRules(rules);

            MessageBox.Show(string.Join("\n", typeOrder));
        }

        private List<string> GetRecommendedTypeOrderFromRules(List<Rule> rules)
        {
            List<TypeConnect> typeConnections = new List<TypeConnect>();
            foreach (Rule rule in rules)
            {
                List<string> keys = new List<string>(rule.ExistentialClauses.Keys);
                //for (int i = 0; i < keys.Count; i++)
                //{
                int i = 0; // For now you can kinda assume that the first type is the one that the rules is about (makes sense too)
                string key1 = keys[i];
                for (int j = i + 1; j < keys.Count; j++)
                {
                    string key2 = keys[j];

                    string firstType = rule.ExistentialClauses[key1].Characteristic.Type;
                    string secondType = rule.ExistentialClauses[key2].Characteristic.Type;
                    if (firstType == secondType)
                    {
                        continue;
                    }
                    // No duplicates
                    if (!typeConnections.Any(tc => tc.Type1 == firstType && tc.Type2 == secondType))
                    {
                        typeConnections.Add(new TypeConnect(firstType, secondType));
                    }
                }
                //}
            }

            // First add walls and floors to the list of placed items
            List<string> placedTypes = new List<string>() { "Floor", "Wall" };
            typeConnections.RemoveAll(tc => placedTypes.Contains(tc.Type1));
            while (typeConnections.Count > 0)
            {
                // Find everything that depends on the placed items
                List<string> potentialNextTypes = new List<string>();
                foreach (TypeConnect tc in typeConnections)
                {
                    if (!placedTypes.Contains(tc.Type1) && placedTypes.Contains(tc.Type2))
                    {
                        potentialNextTypes.Add(tc.Type1);
                    }
                }
                potentialNextTypes = potentialNextTypes.Distinct().ToList();

                string topTypeConnection = null;
                if (potentialNextTypes.Count == 0)
                {
                    // Find the remaining item with the most dependent connections (second type)
                    List<string> remainingTypes = typeConnections.Select(tc => tc.Type1).Distinct().ToList();
                    topTypeConnection = remainingTypes.OrderBy(ty => typeConnections.Count(tc => tc.Type2 == ty)).Last();
                }

                if (potentialNextTypes.Count == 1)
                {
                    topTypeConnection = potentialNextTypes.First();
                }

                if (potentialNextTypes.Count > 1)
                {
                    // If there is more than one, find the one that has the fewest other not-yet-placed dependencies (ideally zero)
                    Dictionary<string, int> typeDependedntCount = new Dictionary<string, int>();
                    foreach (string nextTypeOption in potentialNextTypes)
                    {
                        typeDependedntCount[nextTypeOption] = 0;
                        foreach (TypeConnect tc in typeConnections)
                        {
                            // Check if this type depends on something that is not placed (which is a bad thing)
                            if (tc.Type1 == nextTypeOption && !placedTypes.Contains(tc.Type2))
                            {
                                typeDependedntCount[nextTypeOption] += 1;
                            }
                        }
                    }
                    topTypeConnection = typeDependedntCount.Keys.First();
                    foreach (var key in typeDependedntCount.Keys)
                    {
                        if (typeDependedntCount[topTypeConnection] == typeDependedntCount[key])
                        {
                            // Might be better to place things that have other dependents first if there is a tie breaker
                            if (typeConnections.Count(tc => tc.Type2 == topTypeConnection) < typeConnections.Count(tc => tc.Type2 == key))
                            {
                                topTypeConnection = key;
                            }
                        }

                        // Lower number is better
                        if (typeDependedntCount[topTypeConnection] > typeDependedntCount[key])
                        {
                            topTypeConnection = key;
                        }
                    }
                }

                placedTypes.Add(topTypeConnection);
                typeConnections.RemoveAll(tc => placedTypes.Contains(tc.Type1));
            }

            return placedTypes;
        }

        public class TypeConnect
        {
            public string Type1;
            public string Type2;

            public TypeConnect(string firstType, string secondType)
            {
                this.Type1 = firstType;
                this.Type2 = secondType;
            }
        }

        private void buttonRecommended_Click(object sender, EventArgs e)
        {
            List<Rule> rules = GetCheckedRules(this.treeViewRules.Nodes);
            List<string> typeOrder = GetRecommendedTypeOrderFromRules(rules);

            // Find an item that matches the types in the type order
            foreach (string type in typeOrder)
            {
                foreach (CatalogObjectMetadata com in this.listBoxCatalogList.Items)
                {
                    if (com.Type == type)
                    {
                        this.listBoxSelectedCatalogList.Items.Add(com);
                        continue;
                    }
                }
            }
        }
    }
}