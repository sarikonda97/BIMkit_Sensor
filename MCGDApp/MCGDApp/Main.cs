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

                //if (ModelChecker != null)
                //{
                //    foreach (RuleCheckObject obj1 in ruleInstance.Objs)
                //    {
                //        foreach (RuleCheckObject obj2 in ruleInstance.Objs)
                //        {
                //            if (obj1 == obj2)
                //            {
                //                continue;
                //            }

                //            //TreeNode relTN = new TreeNode("Object1 Name: " + obj1.IfcProduct.Name + " \nObject2 Name: " + obj2.IfcProduct.Name);
                //            TreeNode relTN = new TreeNode(obj1.Name + " => " + obj2.Name);
                //            RuleCheckRelation rel = ModelChecker.FindOrCreateObjectRelation(ModelChecker.Model, obj1, obj2);
                //            foreach (Property prop in rel.Properties)
                //            {
                //                TreeNode propTN = new TreeNode(prop.String());
                //                relTN.Nodes.Add(propTN);
                //            }

                //            ruleInstanceTN.Nodes.Add(relTN);
                //        }
                //    }
                //}
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

            CatalogObjectMetadata catalogObjectMeta = GetCheckedObjects().First();
            APIResponse<CatalogObject> response2 = await DBMSController.GetCatalogObject(new ItemRequest(catalogObjectMeta.CatalogObjectId, LevelOfDetail.LOD100));
            if (response2.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response2.ReasonPhrase);
                return;
            }

            Model model = response.Data;
            List<Rule> rules = GetCheckedRules(this.treeViewRules.Nodes);
            CatalogObject catalogObject = response2.Data;

            float minZ = (float)catalogObject.Components.Min(c => c.Vertices.Min(v => v.z));
            float maxZ = (float)catalogObject.Components.Max(c => c.Vertices.Max(v => v.z));
            float heightOfset = (maxZ - minZ) / 2.0f + 0.0001f;

            // TODO: Should change the start location to the cetner of the whole model:
            GenerativeDesigner generativeDesigner = new GenerativeDesigner(model, rules, catalogObject, new Vector3D(0, 0, heightOfset));
            Model newModel = generativeDesigner.ExecuteGenDesign(GDSettings);

            // Save the models:
            newModel.Name = "Generated Model";
            APIResponse<string> response3 = await DBMSController.CreateModel(newModel);
            if (response3.Code != System.Net.HttpStatusCode.Created)
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
            CatalogObjectMetadata catalogObjectMeta = GetCheckedObjects().First();
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
            GenerativeRequest request = new GenerativeRequest(DBMSController.Token,
                                                              RuleAPIController.CurrentUser.Username,
                                                              modelMetaData.ModelId,
                                                              catalogObjectMeta.CatalogObjectId,
                                                              rules.Select(r => r.Id).ToList(),
                                                              LevelOfDetail.LOD100,
                                                              new Vector3D(0, 0, heightOfset),
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

        private async void buttonGDMultiple_Click(object sender, EventArgs e)
        {
            // Get the model, object, and rules
            ModelMetadata modelMetaData = this.listBoxModelList.SelectedItem as ModelMetadata;
            APIResponse<Model> response = await DBMSController.GetModel(new ItemRequest(modelMetaData.ModelId, LevelOfDetail.LOD100));
            if (response.Code != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response.ReasonPhrase);
                return;
            }

            List<CatalogObjectMetadata> catalogObjectsMeta = GetCheckedObjects();
            List<CatalogObject> catalogObjects = new List<CatalogObject>();
            List<Vector3D> initialLocations = new List<Vector3D>();
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
                float heightOfset = (maxZ - minZ) / 2.0f + 0.0001f;
                catalogObjects.Add(catalogObject);
                initialLocations.Add(new Vector3D(0, 0, heightOfset));
            }

            Model model = response.Data;
            List<Rule> rules = GetCheckedRules(this.treeViewRules.Nodes);

            // TODO: Should change the start location to the cetner of the whole model:
            GenerativeDesignerMultipleObjects generativeDesigner = new GenerativeDesignerMultipleObjects(model, rules, catalogObjects, initialLocations);
            Model newModel = generativeDesigner.ExecuteGenDesignRoundRobin(GDSettings);

            // Save the models:
            newModel.Name = "Generated Model";
            APIResponse<string> response3 = await DBMSController.CreateModel(newModel);
            if (response3.Code != System.Net.HttpStatusCode.Created)
            {
                MessageBox.Show(response3.ReasonPhrase);
                return;
            }

            buttonSignInDBMS_Click(null, null);

            this.richTextBoxGenDesign.Text = "Done";
        }
    }
}