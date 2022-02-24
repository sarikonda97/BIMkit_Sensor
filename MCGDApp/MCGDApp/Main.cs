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
using System.Drawing;
using System.Linq;
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
        private string ruleServiceURL = "https://localhost:44370/api/";
        private string dbmsURL = "https://localhost:44322//api/";
        private string mcURL = "https://localhost:44346//api/";
        private string gdURL = "https://localhost:44328///api/"; 

        private ModelChecker ModelChecker;
        private GenerativeDesignSettings GDSettings;

        public Main()
        {
            InitializeComponent();

            DBMSController = new DBMSAPIController(dbmsURL);
            RuleAPIController = new RuleAPIController(ruleServiceURL);
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
            List<RuleResult> ruleResults = ModelChecker.CheckModel(0);
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

        private List<CatalogObjectMetadata> GetCheckedObjects()
        {
            List<CatalogObjectMetadata> selectedObjects = new List<CatalogObjectMetadata>();
            foreach(CatalogObjectMetadata com in this.listBoxSelectedCatalogList.Items)
            {
                selectedObjects.Add(com);
            }
            return selectedObjects;
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
    }
}