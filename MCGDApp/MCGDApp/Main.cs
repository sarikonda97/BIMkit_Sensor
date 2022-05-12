using DbmsApi;
using DbmsApi.API;
using GenerativeDesignAPI;
using GenerativeDesignPackage;
using MathPackage;
using ModelCheckAPI;
using ModelCheckPackage;
using RuleAPI;
using RuleAPI.Models;
using RuleGeneratorPackage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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

        private void ResetDsiplays()
        {
            this.richTextBoxGenDesign.Text = "";
            this.listBoxRuleResults.Items.Clear();
            this.treeViewRuleInstance.Nodes.Clear();
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

        #region DBMS

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

        private List<CatalogObjectMetadata> GetCheckedObjects()
        {
            List<CatalogObjectMetadata> selectedObjects = new List<CatalogObjectMetadata>();
            foreach (CatalogObjectMetadata com in this.listBoxSelectedCatalogList.Items)
            {
                selectedObjects.Add(com);
            }
            return selectedObjects;
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

        private void buttonRecommended_Click(object sender, EventArgs e)
        {
            List<Rule> rules = GetCheckedRules(this.treeViewRules.Nodes);
            List<string> typeOrder = RecommenderUtil.GetRecommendedTypeOrderFromRules(rules);

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

        #endregion

        #region RMS

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

        private void buttonSelectAllRules_Click(object sender, EventArgs e)
        {
            foreach (TreeNode tn in this.treeViewRules.Nodes)
            {
                tn.Checked = true;
            }
        }

        private void treeViewRules_AfterCheck(object sender, TreeViewEventArgs e)
        {
            foreach (TreeNode n in e.Node.Nodes)
            {
                n.Checked = e.Node.Checked;
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

        #endregion

        #region MC

        private async void buttonModelCheck_Click(object sender, EventArgs e)
        {
            ResetDsiplays();

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
            ResetDsiplays();

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

        #endregion

        #region GD

        private async void buttonGDWeb_Click(object sender, EventArgs e)
        {
            ResetDsiplays();

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
                                                              GDSettings,
                                                              GenerationType.Sequential);

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
            ResetDsiplays();

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

            List<CatalogInitializer> catalogObjectsInits = await GetCatalogInits(catalogObjectsMeta, ModelStartingLocation);

            GenerativeDesigner generativeDesigner = new GenerativeDesigner(model, rules, catalogObjectsInits, GDSettings);
            Model newModel = generativeDesigner.ExecuteGenDesignSequential();

            // Save the model:
            newModel.Name = "Generated Model";
            APIResponse<string> response3 = await DBMSController.CreateModel(newModel);
            if (response3.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response3.ReasonPhrase);
                return;
            }

            DisplayGDResults(rules, sw, generativeDesigner, newModel);
        }

        private async void buttonGDRR_Click(object sender, EventArgs e)
        {
            ResetDsiplays();

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

            List<CatalogInitializer> catalogObjectsInits = await GetCatalogInits(catalogObjectsMeta, ModelStartingLocation);

            GenerativeDesigner generativeDesigner = new GenerativeDesigner(model, rules, catalogObjectsInits, GDSettings);
            Model newModel = generativeDesigner.ExecuteGenDesignRoundRobin();

            // Save the model:
            newModel.Name = "Generated Model";
            APIResponse<string> response3 = await DBMSController.CreateModel(newModel);
            if (response3.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response3.ReasonPhrase);
                return;
            }

            DisplayGDResults(rules, sw, generativeDesigner, newModel);
        }

        private void DisplayGDResults(List<Rule> rules, Stopwatch sw, GenerativeDesigner generativeDesigner, Model newModel)
        {
            buttonSignInDBMS_Click(null, null);
            ModelChecker = new ModelChecker(newModel, generativeDesigner.GetCompiledRules());
            List<RuleResult> ruleResults = ModelChecker.CheckModel(0, false);
            CheckScore cs = ModelChecker.GetCheckScore();

            this.richTextBoxGenDesign.Text = "Done\nRuntime: " + sw.Elapsed.ToString() +
                                             "\nCheck Result: " + cs.TotalScore() + "/" + rules.Count +
                                             "\nErrors: " + cs.ErrorScore + "/" + +rules.Count(r => r.ErrorLevel == ErrorLevel.Error) +
                                             "\nWarning: " + cs.WarningScore + "/" + +rules.Count(r => r.ErrorLevel == ErrorLevel.Warning) +
                                             "\nRecommended: " + cs.RecommendScore + "/" + rules.Count(r => r.ErrorLevel == ErrorLevel.Recommended);
        }

        private async Task<List<CatalogInitializer>> GetCatalogInits(List<CatalogObjectMetadata> catalogObjectsMeta, Vector3D ModelStartingLocation)
        {
            List<CatalogInitializer> catalogObjectsInits = new List<CatalogInitializer>();
            foreach (var catalogObjectMeta in catalogObjectsMeta)
            {
                APIResponse<CatalogObject> response2 = await DBMSController.GetCatalogObject(new ItemRequest(catalogObjectMeta.CatalogObjectId, LevelOfDetail.LOD100));
                if (response2.Code != System.Net.HttpStatusCode.OK)
                {
                    MessageBox.Show(response2.ReasonPhrase);
                    continue;
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

            return catalogObjectsInits;
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

        #endregion

        #region Other

        private async void buttonVoxelCreator_Click(object sender, EventArgs e)
        {
            ResetDsiplays();

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

            List<string> typeOrder = RecommenderUtil.GetRecommendedTypeOrderFromRules(rules);

            MessageBox.Show(string.Join("\n", typeOrder));
        }

        private void buttonRuleLearner_Click(object sender, EventArgs e)
        {
            // Prompt for the types and the EC clauses (and in the future the relations fo interest)
            RuleGeneratorSettingsForm rgsf = new RuleGeneratorSettingsForm();
            if (rgsf.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            // Read in data into examples
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            List<Example> exampleList = new List<Example>();
            foreach (string file in Directory.GetFiles(fbd.SelectedPath, "*.csv"))
            {
                Example currentExample = new Example();
                foreach (string line in File.ReadAllLines(file))
                {
                    string[] lineSplit = line.Split(',');
                    RelationInstance newRi = new RelationInstance()
                    {
                        id1 = lineSplit[0],
                        type1 = lineSplit[1],
                        id2 = lineSplit[2],
                        type2 = lineSplit[3],
                        distance = Convert.ToDouble(lineSplit[4]),
                        facing12 = Convert.ToDouble(lineSplit[5]),
                        facing21 = Convert.ToDouble(lineSplit[6]),
                    };
                    newRi.GetBooleansFromValues();
                    currentExample.relationInstances.Add(newRi);
                }
                exampleList.Add(currentExample);
            }

            Rule newRule = RuleGenerator.LearnRuleBoolean(rgsf.OC1, rgsf.Type1, rgsf.OC2, rgsf.Type2, exampleList);

            this.richTextBoxGenDesign.Text = newRule.String();

            if (RuleAPIController.CurrentUser != null)
            {
                if (MessageBox.Show("Save the new rule?", "Save Rule", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SaveRule(newRule);
                }
            }
        }

        public async void SaveRule(Rule rule)
        {
            APIResponse<string> response = await RuleAPIController.CreateRuleAsync(rule);
            if (response.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response.ReasonPhrase);
                return;
            }
        }

        #endregion
    }
}