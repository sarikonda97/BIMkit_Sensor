using DbmsApi;
using DbmsApi.API;
using GenerativeDesignAPI;
using GenerativeDesignPackage;
using MathPackage;
using ModelCheckAPI;
using ModelCheckPackage;
using RuleAPI;
using RuleAPI.Methods;
using RuleAPI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VoxelService;
using static UnityEngine.UI.Dropdown;
using Component = DbmsApi.API.Component;
using Debug = UnityEngine.Debug;
using Material = UnityEngine.Material;
using Mesh = UnityEngine.Mesh;

public class GameController : MonoBehaviour
{
    #region Fields

    public Camera MainCamera;

    public Material HighlightMatRed;
    public Material HighlightMatYellow;
    public Material HighlightMatGreen;
    public Material DefaultMat;
    public Material VoxelMaterial;
    public Material FloorMaterial;

    public GameObject LoadingCanvas;

    public GameObject ModelViewCanvas;
    public Text ObjectDataText;

    public GameObject ModelEditCanvas;
    public Dropdown ObjectTypeChangeDropdown;

    public GameObject ModelSelectCanvas;
    public Button ModelButtonPrefab;
    public GameObject ModelListViewContent;
    public InputField UsernameInput;
    public InputField PasswordInput;
    public Dropdown LevelOfDetailDropdown;

    public GameObject RuleSelectCanvas;
    public GameObject ModelObjectPrefab;
    public GameObject ModelComponentPrefab;

    public InputField RuleUsernameInput;
    public Button RuleButtonPrefab;
    public GameObject RuleListViewContent;
    public Text RuleDescriptionText;
    private List<ButtonData> RuleButtonData;
    public Button ModelCheckButton;
    public Button GenDesignButton;

    public GameObject CheckResultCanvas;
    public Button CheckResultButtonPrefab;
    public GameObject ResultListViewContent;
    public Button CheckInstanceButtonPrefab;
    public GameObject InstanceListViewContent;
    public Text InstanceValueText;

    public GameObject AddObjectCanvas;
    public GameObject CatalogObjectListViewContent;
    public Button CatalogObjectButtonPrefab;
    public Button ContinueToGenButton;

    private RuleAPIController RuleAPIController;
    private DBMSAPIController DBMSController;
    private MCAPIController MCAPIController;
    private GDAPIController GDAPIController;
    private string ruleServiceURL = "https://localhost:44370/api/";
    private string dbmsURL = "https://localhost:44322//api/";
    private string mcURL = "https://localhost:44346//api/";
    private string gdURL = "https://localhost:44328///api/";

    private Model CurrentModel;
    public GameObject CurrentModelGameObj;
    public GameObject CurrentModelVoxelGameObj;
    public GameObject CurrentModelRuleCheckGameObj;
    private List<ModelObjectScript> ModelObjects = new List<ModelObjectScript>();

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        DBMSController = new DBMSAPIController(dbmsURL);
        RuleAPIController = new RuleAPIController(ruleServiceURL);
        MCAPIController = new MCAPIController(mcURL);
        GDAPIController = new GDAPIController(gdURL);

        ResetCanvas();
        this.ModelSelectCanvas.SetActive(true);

        // Some buttons should all start disabled:
        GenDesignButton.gameObject.SetActive(false);
        ModelCheckButton.gameObject.SetActive(false);
        ContinueToGenButton.gameObject.SetActive(false);

        FetchTypes();

        this.LevelOfDetailDropdown.options.Clear();
        this.LevelOfDetailDropdown.options.AddRange(Enum.GetValues(typeof(LevelOfDetail)).Cast<LevelOfDetail>().Select(l => new OptionData(l.ToString())));
        int index = LevelOfDetailDropdown.options.FindIndex((i) => { return i.text.Equals(LevelOfDetail.LOD500.ToString()); });
        this.LevelOfDetailDropdown.value = index;

        // For testing:
        this.PasswordInput.text = "admin";
        this.RuleUsernameInput.text = "admin";
        this.UsernameInput.text = "admin";
    }

    // Update is called once per frame
    void Update()
    {
        MoveCamera();
        if (this.ModelViewCanvas.activeInHierarchy)
        {
            ViewingMode();
        }
        if (placingObject)
        {
            MoveObject();
        }
        if (this.ModelEditCanvas.activeInHierarchy)
        {
            EditingMode();
        }
        if (roatatingObject)
        {
            RotatingObject();
        }
        if (CheckingMode)
        {
            CheckingOverlapMode();
        }
    }

    #region Camera Controls

    public float cameraSpeed = 200f;
    float sensitivity = 30f;

    private void MoveCamera()
    {
        if (Input.GetMouseButton(1))
        {
            MainCamera.transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y") * cameraSpeed * Time.deltaTime, Input.GetAxis("Mouse X") * cameraSpeed * Time.deltaTime, 0));
            float X = MainCamera.transform.rotation.eulerAngles.x;
            float Y = MainCamera.transform.rotation.eulerAngles.y;
            MainCamera.transform.rotation = Quaternion.Euler(X, Y, 0);
        }

        if (Input.GetMouseButton(2))
        {
            var newPosition = new Vector3();
            newPosition.x = Input.GetAxis("Mouse X") * cameraSpeed * Time.deltaTime;
            newPosition.y = Input.GetAxis("Mouse Y") * cameraSpeed * Time.deltaTime;
            MainCamera.transform.Translate(-newPosition);
        }

        if (!roatatingObject)
        {
            MainCamera.transform.position += MainCamera.transform.forward * Input.GetAxis("Mouse ScrollWheel") * sensitivity;
        }
    }

    #endregion

    #region Model Select Mode

    public void ModelViewClicked()
    {
        ResetCanvas();
        this.ModelViewCanvas.SetActive(true);
    }

    public async void SignInClicked()
    {
        LoadingCanvas.SetActive(true);
        APIResponse<TokenData> response = await DBMSController.LoginAsync(this.UsernameInput.text, this.PasswordInput.text);
        if (response.Code != System.Net.HttpStatusCode.OK)
        {
            Debug.LogWarning(response.ReasonPhrase);
            LoadingCanvas.SetActive(false);
            return;
        }

        APIResponse<List<ModelMetadata>> response2 = await DBMSController.GetAvailableModels();
        if (response2.Code != System.Net.HttpStatusCode.OK)
        {
            Debug.LogWarning(response2.ReasonPhrase);
            LoadingCanvas.SetActive(false);
            return;
        }

        RemoveAllChidren(this.ModelListViewContent);
        List<ModelMetadata> models = response2.Data;
        foreach (ModelMetadata mm in models)
        {
            Button newButton = GameObject.Instantiate(this.ModelButtonPrefab, this.ModelListViewContent.transform);
            newButton.GetComponentInChildren<Text>().text = mm.ToString();
            UnityAction action = new UnityAction(() => LoadDBMSModel(mm.ModelId));
            newButton.onClick.AddListener(action);
        }

        LoadingCanvas.SetActive(false);
    }

    private async void LoadDBMSModel(string modelId)
    {
        LoadingCanvas.SetActive(true);

        string selectedLOD = LevelOfDetailDropdown.options[LevelOfDetailDropdown.value].text;
        LevelOfDetail lod = (LevelOfDetail)Enum.Parse(typeof(LevelOfDetail), selectedLOD);
        APIResponse<Model> response = await DBMSController.GetModel(new ItemRequest(modelId, lod));
        if (response.Code != System.Net.HttpStatusCode.OK)
        {
            Debug.LogWarning(response.ReasonPhrase);
            LoadingCanvas.SetActive(false);
            return;
        }

        CurrentModel = response.Data;

        RemoveAllChidren(CurrentModelGameObj);

        Bounds b = new Bounds(VectorConvert(CurrentModel.ModelObjects[0].Location), Vector3.zero);
        ModelObjects = new List<ModelObjectScript>();
        foreach (ModelObject obj in CurrentModel.ModelObjects)
        {
            GameObject modelObject = CreateModelObject(obj, CurrentModelGameObj);
            Bounds objBound = CreateComponents(obj.Components, modelObject);
            b.Encapsulate(objBound);

            modelObject.name = obj.Name;
            modelObject.transform.SetPositionAndRotation(VectorConvert(obj.Location), VectorConvert(obj.Orientation));
            ModelObjectScript script = modelObject.GetComponent<ModelObjectScript>();
            script.ModelObject = obj;
            if (obj.TypeId == "Floor")
            {
                script.SetMainMaterial(FloorMaterial);
            }
            else
            {
                script.SetMainMaterial(DefaultMat);
            }

            ModelObjects.Add(script);
        }

        SetupMainCamera();

        ResetCanvas();
        ModelViewCanvas.SetActive(true);
        LoadingCanvas.SetActive(false);
    }

    #endregion

    #region Model Load Methods

    private GameObject CreateModelObject(ModelObject o, GameObject parentObj)
    {
        o.Id = o.Id ?? Guid.NewGuid().ToString();
        o.Orientation = o.Orientation ?? Utils.GetQuaterion(new Vector3D(0, 0, 1), 0.0 * Math.PI / 180.0);
        o.Location = o.Location ?? new Vector3D(0, 0, 0);
        return Instantiate(ModelObjectPrefab, parentObj.transform);
    }

    private Bounds CreateComponents(List<Component> components, GameObject parentObj)
    {
        Bounds b = new Bounds(Vector3.zero, Vector3.zero);
        foreach (Component c in components)
        {
            GameObject meshObject = Instantiate(ModelComponentPrefab, parentObj.transform);
            Mesh mesh = new Mesh();
            MeshFilter meshFilter = meshObject.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            MeshCollider meshCollider = meshObject.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            mesh.vertices = c.Vertices.Select(v => VectorConvert(v)).ToArray();
            mesh.uv = mesh.vertices.Select(v => new Vector2(v.x, v.y)).ToArray();
            //mesh.uv = CalculateUVs(mesh, mesh.vertices.ToList());
            mesh.triangles = c.Triangles.SelectMany(t => new List<int>() { t[0], t[1], t[2] }).Reverse().ToArray();
            mesh.RecalculateNormals();
            b.Encapsulate(mesh.bounds);
        }

        return b;
    }

    private static Vector2[] CalculateUVs(Mesh mesh, List<Vector3> newVerticesFinal)
    {
        // calculate UVs ============================================
        float scaleFactor = 0.5f;
        Vector2[] uvs = new Vector2[newVerticesFinal.Count];
        int len = mesh.GetIndices(0).Length;
        int[] idxs = mesh.GetIndices(0);
        for (int i = 0; i < len; i = i + 3)
        {
            Vector3 v1 = newVerticesFinal[idxs[i + 0]];
            Vector3 v2 = newVerticesFinal[idxs[i + 1]];
            Vector3 v3 = newVerticesFinal[idxs[i + 2]];
            Vector3 normal = Vector3.Cross(v3 - v1, v2 - v1);
            Quaternion rotation;
            if (normal == Vector3.zero)
                rotation = new Quaternion();
            else
                rotation = Quaternion.Inverse(Quaternion.LookRotation(normal));
            uvs[idxs[i + 0]] = (Vector2)(rotation * v1) * scaleFactor;
            uvs[idxs[i + 1]] = (Vector2)(rotation * v2) * scaleFactor;
            uvs[idxs[i + 2]] = (Vector2)(rotation * v3) * scaleFactor;
        }
        //==========================================================
        return uvs;
    }

    public static Vector3 VectorConvert(Vector3D v)
    {
        return new Vector3((float)v.x, (float)v.z, (float)v.y);
    }
    public static Quaternion VectorConvert(Vector4D v)
    {
        return new Quaternion((float)v.x, (float)v.z, (float)v.y, (float)v.w);
    }
    public static Vector3D VectorConvert(Vector3 v)
    {
        return new Vector3D((float)v.x, (float)v.z, (float)v.y);
    }
    public static Vector4D VectorConvert(Quaternion v)
    {
        return new Vector4D((float)v.x, (float)v.z, (float)v.y, (float)v.w);
    }

    #endregion

    #region Model View Mode

    private GameObject ViewingGameObject;
    private void ViewingMode()
    {
        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;
        if (Physics.Raycast(ray, out hitData, 1000))
        {
            ModelObjectScript mos;
            VoxelScript vos;
            if (ViewingGameObject != null)
            {
                mos = ViewingGameObject.GetComponent<ModelObjectScript>();
                if (mos != null)
                {
                    mos.UnHighlight();
                }
                vos = ViewingGameObject.GetComponent<VoxelScript>();
                if (vos != null)
                {
                    vos.ModelObjectScript.UnHighlight();
                }
            }

            GameObject hitObject = hitData.collider.gameObject;
            if (hitObject.GetComponent<VoxelScript>() != null)
            {
                // Hit a voxel
                ViewingGameObject = hitData.collider.gameObject;
            }
            else
            {
                // Hit a model object component
                ViewingGameObject = hitData.collider.gameObject.transform.parent.gameObject;
            }

            mos = ViewingGameObject.GetComponent<ModelObjectScript>();
            if (mos != null)
            {
                mos.Highlight(HighlightMatYellow);
                DisplayObjectInfo(mos);
            }
            vos = ViewingGameObject.GetComponent<VoxelScript>();
            if (vos != null)
            {
                vos.ModelObjectScript.Highlight(HighlightMatYellow);
                DisplayObjectInfo(vos.ModelObjectScript);
            }
        }
    }

    private void DisplayObjectInfo(ModelObjectScript mos)
    {
        if (mos == null)
        {
            return;
        }

        ObjectDataText.text = "Name: " + mos.ModelObject.Name + "\n";
        ObjectDataText.text += "Id: " + mos.ModelObject.Id + "\n";

        if (mos.ModelObject.GetType() == typeof(ModelCatalogObject))
        {
            ObjectDataText.text += "Catalog Id: " + ((ModelCatalogObject)mos.ModelObject).CatalogId + "\n";
        }

        ObjectDataText.text += "TypeId: " + mos.ModelObject.TypeId + "\n\n";
        foreach (Property p in mos.ModelObject.Properties)
        {
            ObjectDataText.text += p.Name + ": " + p.GetValueString() + "\n";
        }
    }

    public void ModelCheckClicked()
    {
        ResetCanvas();
        this.RuleSelectCanvas.SetActive(true);
        GenDesignButton.gameObject.SetActive(false);
        ModelCheckButton.gameObject.SetActive(true);
    }

    public void EditModelClicked()
    {
        ResetCanvas();
        ModelEditCanvas.SetActive(true);
    }

    public void AddObjectClicked()
    {
        ResetCanvas();
        RefreshCatalogClicked();
        AddObjectCanvas.SetActive(true);
        ContinueToGenButton.gameObject.SetActive(false);
    }

    public void GenDesignClicked()
    {
        RefreshCatalogClicked();
        ResetCanvas();
        AddObjectCanvas.SetActive(true);
        genDesignMode = true;
        GenDesignButton.gameObject.SetActive(true);
        ModelCheckButton.gameObject.SetActive(false);
        ContinueToGenButton.gameObject.SetActive(true);
    }

    public async void SaveModelClicked()
    {
        LoadingCanvas.SetActive(true);
        APIResponse<string> response = await this.DBMSController.UpdateModel(CurrentModel);
        if (response.Code != System.Net.HttpStatusCode.OK)
        {
            Debug.LogWarning(response.ReasonPhrase);
            LoadingCanvas.SetActive(false);
            return;
        }

        Debug.LogWarning("Saved");
        LoadingCanvas.SetActive(false);
    }

    public void ResetClicked()
    {
        string modelId = CurrentModel.Id;
        ExitClicked();
        LoadDBMSModel(modelId);
    }

    public void ExitClicked()
    {
        RemoveAllChidren(CurrentModelGameObj);
        CurrentModel = null;
        ResetCanvas();
        this.ModelSelectCanvas.SetActive(true);
    }

    #endregion

    #region Model Edit Mode

    private void EditingMode()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitData;
            if (Physics.Raycast(ray, out hitData, 1000))
            {
                ModelObjectScript mos;
                if (EditingGameObject != null)
                {
                    mos = EditingGameObject.GetComponent<ModelObjectScript>();
                    mos.UnHighlight();
                }

                EditingGameObject = hitData.collider.gameObject.transform.parent.gameObject;
                mos = EditingGameObject.GetComponent<ModelObjectScript>();
                mos.Highlight(HighlightMatRed);

                int index = ObjectTypeChangeDropdown.options.FindIndex((i) => { return i.text.Equals(mos.ModelObject.TypeId.ToString()); });
                this.ObjectTypeChangeDropdown.value = index;
            }
        }
    }

    public void TypeDropdownChange()
    {
        if (EditingGameObject != null)
        {
            string selectedType = ObjectTypeChangeDropdown.options[ObjectTypeChangeDropdown.value].text;
            ModelObjectScript mos = EditingGameObject.GetComponent<ModelObjectScript>();
            mos.ModelObject.TypeId = selectedType;
        }
    }

    public void DeleteObject()
    {
        if (EditingGameObject != null)
        {
            ModelObjectScript mos = EditingGameObject.GetComponent<ModelObjectScript>();
            ModelObjects.Remove(mos);
            CurrentModel.ModelObjects.Remove(mos.ModelObject);
            Destroy(EditingGameObject);
            EditingGameObject = null;
        }
    }

    public void MoveObjectClicked()
    {
        if (EditingGameObject == null)
        {
            return;
        }
        PlaceOject();
    }

    public void RotateObjectClicked()
    {
        roatatingObject = true;
        if (EditingGameObject == null)
        {
            roatatingObject = false;
            return;
        }
    }

    bool roatatingObject;
    private void RotatingObject()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ChangeAllChidrenTagsAndLayer(EditingGameObject, "Untagged", 0);
            ModelObject mo = EditingGameObject.GetComponent<ModelObjectScript>().ModelObject;
            mo.Location = VectorConvert(EditingGameObject.transform.position);
            mo.Orientation = VectorConvert(EditingGameObject.transform.rotation);
            roatatingObject = false;
            return;
        }

        float rotationAmount = Input.mouseScrollDelta.y * 90.0f;
        //Vector4D quaternion = Utils.GetQuaterion(new Vector3D(0, 0, 1), rotationAmount * Math.PI / 180.0);
        EditingGameObject.transform.Rotate(Vector3.up, rotationAmount);
    }

    public void AddPropertyClicked()
    {
        if (EditingGameObject != null)
        {
            ModelObjectScript mos = EditingGameObject.GetComponent<ModelObjectScript>();
            foreach (var method in MethodFinder.GetAllPropertyInfos())
            {
                object result = method.Value.Invoke(null, new object[] { new RuleCheckObject(mos.ModelObject) });
                if (result.GetType() == typeof(string))
                {
                    mos.ModelObject.Properties.Add(method.Key, (string)result);
                }
                if (result.GetType() == typeof(double))
                {
                    mos.ModelObject.Properties.Add(method.Key, (double)result);
                }
                if (result.GetType() == typeof(bool))
                {
                    mos.ModelObject.Properties.Add(method.Key, (bool)result);
                }
            }
        }
    }

    #endregion

    #region Model Select Catalog Object Mode:

    private async void PopulateCatalog()
    {
        // Access all object from the catalog:
        LoadingCanvas.SetActive(true);
        APIResponse<List<CatalogObjectMetadata>> response = await DBMSController.GetAvailableCatalogObjects();
        if (response.Code != System.Net.HttpStatusCode.OK)
        {
            Debug.LogWarning(response.ReasonPhrase);
            LoadingCanvas.SetActive(false);
            return;
        }

        // Create a button for each catalog item and place in the list
        foreach (CatalogObjectMetadata com in response.Data)
        {
            Button newButton = GameObject.Instantiate(this.CatalogObjectButtonPrefab, this.CatalogObjectListViewContent.transform);
            newButton.GetComponentInChildren<Text>().text = com.ToString();
            UnityAction action = new UnityAction(() => PlaceCatalogObject(com.CatalogObjectId));
            newButton.onClick.AddListener(action);
        }

        LoadingCanvas.SetActive(false);
    }

    public void RefreshCatalogClicked()
    {
        RemoveAllChidren(this.CatalogObjectListViewContent);
        PopulateCatalog();
    }

    public void DoneEditClicked()
    {
        RemoveAllChidren(this.CatalogObjectListViewContent);
        ResetCanvas();
        ModelViewCanvas.SetActive(true);
    }

    #endregion

    #region Place Catalog Object Mode:

    private Vector3 worldPosition = new Vector3();
    private float heightOfset = 0;
    private float ERROR = 0.01f;

    private async void PlaceCatalogObject(string catalogId)
    {
        EditingGameObject = await LoadCatalogObject(catalogId);
        PlaceOject();
    }

    private void PlaceOject()
    {
        ChangeAllChidrenTagsAndLayer(EditingGameObject, "Temp", 2);

        ModelObject mo = EditingGameObject.GetComponent<ModelObjectScript>().ModelObject;
        float minZ = (float)mo.Components.Min(c => c.Vertices.Min(v => v.z));
        float maxZ = (float)mo.Components.Max(c => c.Vertices.Max(v => v.z));
        heightOfset = (maxZ - minZ) / 2.0f + ERROR;

        placingObject = true;
    }

    #endregion

    #region Moving Objects

    private GameObject EditingGameObject;
    private bool placingObject;
    private void MoveObject()
    {
        ModelObjectScript mosEditingObj = EditingGameObject.GetComponent<ModelObjectScript>();
        ModelObject mo = mosEditingObj.ModelObject;
        if (Input.GetMouseButtonDown(0))
        {
            ChangeAllChidrenTagsAndLayer(EditingGameObject, "Untagged", 0);
            mo.Location = VectorConvert(EditingGameObject.transform.position);
            mo.Orientation = VectorConvert(EditingGameObject.transform.rotation);
            placingObject = false;

            if (genDesignMode)
            {
                GeneratingObjects.Add(EditingGameObject);
            }

            return;
        }

        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;
        if (Physics.Raycast(ray, out hitData, 1000) && hitData.collider.transform.tag != "Temp")
        {
            worldPosition = hitData.point + new Vector3(0, heightOfset, 0);
        }
        EditingGameObject.transform.position = worldPosition;
        mo.Location = VectorConvert(EditingGameObject.transform.position);
        mo.Orientation = VectorConvert(EditingGameObject.transform.rotation);

        CheckOverlapRuntime(mosEditingObj, mo);
    }

    #endregion

    #region Catalog Object Load Methods

    private async Task<GameObject> LoadCatalogObject(string catalogId)
    {
        LoadingCanvas.SetActive(true);

        APIResponse<CatalogObject> response = await DBMSController.GetCatalogObject(new ItemRequest(catalogId, LevelOfDetail.LOD500));
        if (response.Code != System.Net.HttpStatusCode.OK)
        {
            Debug.LogWarning(response.ReasonPhrase);
            LoadingCanvas.SetActive(false);
            return null;
        }

        ModelCatalogObject modelCatalogObject = CreateModelCatalogObject(response.Data);

        GameObject modelObject = CreateModelObject(modelCatalogObject, CurrentModelGameObj);
        Bounds objBound = CreateComponents(modelCatalogObject.Components, modelObject);

        modelObject.name = modelCatalogObject.Name;
        modelObject.transform.SetPositionAndRotation(VectorConvert(modelCatalogObject.Location), VectorConvert(modelCatalogObject.Orientation));
        ModelObjectScript script = modelObject.GetComponent<ModelObjectScript>();
        script.SetMainMaterial(DefaultMat);
        script.ModelObject = modelCatalogObject;

        CurrentModel.ModelObjects.Add(modelCatalogObject);
        ModelObjects.Add(script);

        LoadingCanvas.SetActive(false);

        return modelObject;
    }

    private ModelCatalogObject CreateModelCatalogObject(CatalogObject o)
    {
        return new ModelCatalogObject()
        {
            CatalogId = o.CatalogID,
            Components = o.Components,
            Name = o.Name,
            Properties = o.Properties,
            TypeId = o.TypeId
        };
    }

    #endregion

    #region Rule Select Mode

    public async void RuleSignInClicked()
    {
        LoadingCanvas.SetActive(true);

        APIResponse<RuleUser> response = await RuleAPIController.LoginAsync(this.RuleUsernameInput.text);
        if (response.Code != System.Net.HttpStatusCode.OK)
        {
            Debug.LogWarning(response.ReasonPhrase);
            LoadingCanvas.SetActive(false);
            return;
        }

        APIResponse<List<RuleSet>> response2 = await RuleAPIController.GetAllRuleSetsAsync();
        if (response2.Code != System.Net.HttpStatusCode.OK)
        {
            Debug.LogWarning(response2.ReasonPhrase);
            LoadingCanvas.SetActive(false);
            return;
        }

        List<RuleSet> ruleSets = response2.Data;
        RemoveAllChidren(this.RuleListViewContent);
        RuleButtonData = new List<ButtonData>();
        foreach (RuleSet rs in ruleSets)
        {
            foreach (Rule rule in rs.Rules)
            {
                Button newButton = GameObject.Instantiate(this.RuleButtonPrefab, this.RuleListViewContent.transform);
                newButton.GetComponent<ButtonData>().Item = rule;
                newButton.GetComponentInChildren<Text>().text = rs.Name + " - " + rule.Name;
                UnityAction action = new UnityAction(() => RuleButtonClicked(newButton));
                newButton.onClick.AddListener(action);

                RuleButtonData.Add(newButton.GetComponent<ButtonData>());
            }
        }

        LoadingCanvas.SetActive(false);
    }

    private void RuleButtonClicked(Button ruleButton)
    {
        ButtonData data = ruleButton.GetComponent<ButtonData>();
        data.Clicked = !data.Clicked;
        ruleButton.image.color = data.Clicked ? new Color(0, 250, 0) : new Color(255, 255, 255);
        RuleDescriptionText.text = ((Rule)data.Item).Description;
    }

    public async void CheckModelClicked()
    {
        LoadingCanvas.SetActive(true);
        ResetCanvas();

        List<string> rules = RuleButtonData.Where(rbd => rbd.Clicked).Select(r => ((Rule)r.Item).Id).ToList();
        CheckRequest request = new CheckRequest(DBMSController.Token, RuleAPIController.CurrentUser.Username, CurrentModel.Id, rules, LevelOfDetail.LOD100);
        APIResponse<List<RuleResult>> response = await MCAPIController.PerformModelCheck(request);
        if (response.Code != System.Net.HttpStatusCode.OK)
        {
            Debug.LogWarning(response.ReasonPhrase);
            LoadingCanvas.SetActive(false);
            return;
        }

        List<RuleResult> results = response.Data;

        RemoveAllChidren(this.ResultListViewContent);
        foreach (RuleResult result in results)
        {
            Button newButton = GameObject.Instantiate(this.CheckResultButtonPrefab, this.ResultListViewContent.transform);
            newButton.GetComponentInChildren<Text>().text = result.ToString();
            UnityAction action = new UnityAction(() => ResultButtonClicked(result));
            newButton.onClick.AddListener(action);
        }

        RemoveAllChidren(this.InstanceListViewContent);

        CheckResultCanvas.SetActive(true);
        LoadingCanvas.SetActive(false);
    }

    private List<GameObject> GeneratingObjects = new List<GameObject>();
    public async void PerfromGenDesignClicked()
    {
        LoadingCanvas.SetActive(true);

        List<string> rules = RuleButtonData.Where(rbd => rbd.Clicked).Select(r => ((Rule)r.Item).Id).ToList();

        List<CatalogInitializerID> catalogInitializerIDs = new List<CatalogInitializerID>();
        foreach (var generatingObj in GeneratingObjects)
        {
            ModelCatalogObject mo = (ModelCatalogObject)generatingObj.GetComponent<ModelObjectScript>().ModelObject;
            CatalogInitializerID newCatInit = new CatalogInitializerID() { CatalogID = mo.CatalogId, Location = VectorConvert(generatingObj.transform.position) };
            catalogInitializerIDs.Add(newCatInit);
        }
        GenerativeRequest request = new GenerativeRequest(DBMSController.Token,
                                                          RuleAPIController.CurrentUser.Username,
                                                          CurrentModel.Id,
                                                          catalogInitializerIDs,
                                                          rules,
                                                          LevelOfDetail.LOD100,
                                                          new GenerativeDesignSettings(
                                                                Convert.ToInt32(100),
                                                                Convert.ToDouble(10),
                                                                Convert.ToDouble(0.75),
                                                                Convert.ToInt32(5),
                                                                false
                                                                )
                                                          );

        APIResponse<string> response = await GDAPIController.PerformGenDesign(request);
        if (response.Code != System.Net.HttpStatusCode.OK)
        {
            Debug.LogWarning(response.ReasonPhrase);
            LoadingCanvas.SetActive(false);
            return;
        }

        RestGenDesignMode();
        ExitClicked();
        SignInClicked();
        LoadDBMSModel(response.Data);
        LoadingCanvas.SetActive(false);
    }

    public void CancelRuleSelectClicked()
    {
        this.ResetCanvas();
        this.ModelViewCanvas.SetActive(true);
        RestGenDesignMode();
    }

    private void RestGenDesignMode()
    {
        GeneratingObjects = new List<GameObject>();
        genDesignMode = false;
    }

    #endregion

    #region Generative Design Mode

    bool genDesignMode;

    public void ContinueToRulesclicked()
    {
        ResetCanvas();
        this.RuleSelectCanvas.SetActive(true);
        this.genDesignMode = true;
    }

    #endregion

    #region Model Check Mode

    private void ResultButtonClicked(RuleResult result)
    {
        RemoveAllChidren(this.InstanceListViewContent);
        foreach (RuleInstance instance in result.RuleInstances)
        {
            Button newButton = GameObject.Instantiate(this.CheckInstanceButtonPrefab, this.InstanceListViewContent.transform);
            newButton.GetComponentInChildren<Text>().text = instance.ToString();
            UnityAction action = new UnityAction(() => InstanceButtonClicked(instance, result));
            newButton.onClick.AddListener(action);
        }

        UnHighlightAllObjects();
        foreach (var objId in result.RuleInstances.SelectMany(i => i.Objs.Select(o => o.ID)))
        {
            ModelObjects.First(o => o.ModelObject.Id == objId).Highlight(HighlightMatYellow);
        }
    }

    private void InstanceButtonClicked(RuleInstance instance, RuleResult result)
    {
        UnHighlightAllObjects();
        foreach (var objId in result.RuleInstances.SelectMany(i => i.Objs.Select(o => o.ID)))
        {
            ModelObjects.First(o => o.ModelObject.Id == objId).Highlight(HighlightMatYellow);
        }
        foreach (var objId in instance.Objs.Select(i => i.ID))
        {
            ModelObjects.First(o => o.ModelObject.Id == objId).Highlight(instance.PassVal == 1 ? HighlightMatGreen : HighlightMatRed);
        }

        string displayStr = "";
        foreach (RuleCheckObject ruleCheckObject in instance.Objs)
        {
            displayStr += ruleCheckObject.Name + "\n";
            foreach (Property property in ruleCheckObject.Properties)
            {
                displayStr += property.String() + "\n";
            }
            displayStr += "\n";
        }
        displayStr += "=====================================\n";
        foreach (RuleCheckRelation ruleCheckRelation in instance.Rels)
        {
            displayStr += ruleCheckRelation.FirstObj.Name + " => " + ruleCheckRelation.SecondObj.Name + "\n";
            foreach (Property property in ruleCheckRelation.Properties)
            {
                displayStr += property.String() + "\n";
            }
        }
        InstanceValueText.text = displayStr;
    }

    public void DoneCheckClicked()
    {
        UnHighlightAllObjects();
        this.ResetCanvas();
        this.ModelViewCanvas.SetActive(true);
    }

    #endregion

    #region Voxel Methods

    public void CreateVoxels()
    {
        if (CurrentModelVoxelGameObj != null)
        {
            Destroy(CurrentModelVoxelGameObj);
            CurrentModelVoxelGameObj = null;
            return;
        }
        if (CurrentModel == null)
        {
            return;
        }

        double size = 0.25;
        VoxelCreater voxelCreater = new VoxelCreater(CurrentModel);
        List<Voxel> voxels = voxelCreater.CreateVoxels(size);

        CurrentModelVoxelGameObj = new GameObject("Voxels");
        foreach (Voxel voxel in voxels)
        {
            if (voxel.ModelObjectID != null)
            {
                GameObject voxelObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                voxelObject.transform.parent = CurrentModelVoxelGameObj.transform;
                voxelObject.transform.position = VectorConvert(voxel.Location);
                voxelObject.transform.localScale = new Vector3((float)size, (float)size, (float)size);
                voxelObject.name = ModelObjects.First(mos => mos.ModelObject.Id == voxel.ModelObjectID).ModelObject.Name;
                VoxelScript voxelScript = voxelObject.AddComponent<VoxelScript>();
                voxelScript.Voxel = voxel;
                voxelScript.ModelObjectScript = ModelObjects.First(mos => mos.ModelObject.Id == voxel.ModelObjectID);
                voxelObject.GetComponent<MeshRenderer>().material = VoxelMaterial;
            }
        }
    }

    #endregion

    #region Random Methods

    private async void FetchTypes()
    {
        APIResponse<List<ObjectType>> response = await DBMSController.GetTypes();
        if (response.Code != System.Net.HttpStatusCode.OK)
        {
            Debug.LogWarning(response.ReasonPhrase);

            // Should avoid using this...
            ObjectTypeTree.BuildTypeTree(ObjectTypeTree.DefaultTypesList());
        }
        else
        {
            ObjectTypeTree.BuildTypeTree(response.Data);
        }

        List<string> types = ObjectTypeTree.GetAllTypes().Select(t => t.Name).ToList();
        List<string> leafTypes = types.Where(t => ObjectTypeTree.GetTypeChildren(t).Count == 0).ToList();
        this.ObjectTypeChangeDropdown.options.Clear();
        this.ObjectTypeChangeDropdown.options.AddRange(types.Select(t => new OptionData(t)));
    }

    private static void RemoveAllChidren(GameObject obj)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            var child = obj.transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    private static void ChangeAllChidrenTagsAndLayer(GameObject obj, string newTag, int newLayer)
    {
        obj.transform.tag = newTag;
        obj.layer = newLayer;
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            var child = obj.transform.GetChild(i);
            ChangeAllChidrenTagsAndLayer(child.gameObject, newTag, newLayer);
        }
    }

    private void ResetCanvas()
    {
        this.ModelEditCanvas.SetActive(false);
        this.RuleSelectCanvas.SetActive(false);
        this.CheckResultCanvas.SetActive(false);
        this.ModelViewCanvas.SetActive(false);
        this.AddObjectCanvas.SetActive(false);
        this.ModelSelectCanvas.SetActive(false);
        this.OverlapCanvas.SetActive(false);

        UnHighlightAllObjects();

        ViewingGameObject = null;
        EditingGameObject = null;
        placingObject = false;

        RemoveAllChidren(this.ModelListViewContent);
        RemoveAllChidren(this.RuleListViewContent);
        RemoveAllChidren(this.ResultListViewContent);
        RemoveAllChidren(this.InstanceListViewContent);
        RemoveAllChidren(this.CatalogObjectListViewContent);
    }

    private void UnHighlightAllObjects()
    {
        foreach (ModelObjectScript mo in ModelObjects)
        {
            if (mo.IsHighlighted)
            {
                mo.UnHighlight();
            }
        }
    }

    #endregion

    #region Overlap Check Methods:

    public GameObject OverlapCanvas;
    public Text OverlapCheckText;
    private ModelObjectScript FirstObject;
    private ModelObjectScript SecondObject;
    private bool CheckingMode = false;
    private bool selectingFirstObject = true;

    private void CheckingOverlapMode()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitData;
            if (Physics.Raycast(ray, out hitData, 1000))
            {
                if (selectingFirstObject)
                {
                    if (FirstObject != null)
                    {
                        FirstObject.UnHighlight();
                    }

                    GameObject go1 = hitData.collider.gameObject.transform.parent.gameObject;
                    FirstObject = go1.GetComponent<ModelObjectScript>();
                    FirstObject.Highlight(HighlightMatRed);
                }
                else
                {
                    if (SecondObject != null)
                    {
                        SecondObject.UnHighlight();
                    }

                    GameObject go1 = hitData.collider.gameObject.transform.parent.gameObject;
                    SecondObject = go1.GetComponent<ModelObjectScript>();
                    SecondObject.Highlight(HighlightMatRed);
                }

                selectingFirstObject = !selectingFirstObject;
                if (FirstObject != null && SecondObject != null)
                {
                    CheckObjectOverlap();
                }
            }
        }
    }

    private void CheckObjectOverlap()
    {
        Debug.Log("Checking");

        RuleCheckObject rco1 = new RuleCheckObject(FirstObject.ModelObject);
        RuleCheckObject rco2 = new RuleCheckObject(SecondObject.ModelObject);
        Stopwatch sw = new Stopwatch();
        sw.Start();
        bool result1 = Utils.MeshOverlapTest1(rco1.GetGlobalMesh(), rco2.GetGlobalMesh(), 1.0);
        TimeSpan ts1 = sw.Elapsed;
        sw.Restart();
        bool result2 = Utils.MeshOverlapTest2(rco1.GetGlobalMesh(), rco2.GetGlobalMesh(), 1.0);
        TimeSpan ts2 = sw.Elapsed;
        sw.Restart();
        bool result3 = Utils.MeshOverlapTest3(rco1.GetGlobalMesh(), rco2.GetGlobalMesh(), 1.0);
        TimeSpan ts3 = sw.Elapsed;
        sw.Restart();
        bool result4 = Utils.MeshOverlapTest4(rco1.GetGlobalMesh(), rco2.GetGlobalMesh(), 1.0);
        TimeSpan ts4 = sw.Elapsed;

        this.OverlapCheckText.text = "Overlap Result 1: " + result1 + " (" + ts1.ToString() + ")" +
                                     "\nOverlap Result 2: " + result2 + " (" + ts2.ToString() + ")" +
                                     "\nOverlap Result 3: " + result3 + " (" + ts3.ToString() + ")" +
                                     "\nOverlap Result 4: " + result4 + " (" + ts4.ToString() + ")";
    }

    public void CheckOverlapClicked()
    {
        ResetCanvas();
        OverlapCanvas.SetActive(true);
        CheckingMode = true;
    }

    public void DoneOverlap()
    {
        selectingFirstObject = true;
        FirstObject = null;
        SecondObject = null;
        CheckingMode = false;

        this.ResetCanvas();
        this.ModelViewCanvas.SetActive(true);
    }

    public void RuleObjectCheckClicked()
    {
        if (CurrentModelRuleCheckGameObj != null)
        {
            Destroy(CurrentModelRuleCheckGameObj);
            CurrentModelRuleCheckGameObj = null;
            return;
        }
        if (CurrentModel == null)
        {
            return;
        }

        CurrentModelRuleCheckGameObj = new GameObject("RuleCheckModel");
        foreach (ModelObject mo in CurrentModel.ModelObjects)
        {
            RuleCheckObject rco = new RuleCheckObject(mo);

            // Create an object based on the RuleCheckObject rather than the model object to see if it has the same shape

            GameObject meshObject = Instantiate(ModelComponentPrefab, CurrentModelRuleCheckGameObj.transform);
            Mesh mesh = new Mesh();
            MeshFilter meshFilter = meshObject.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            MeshCollider meshCollider = meshObject.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            mesh.vertices = rco.GetGlobalMesh().VertexList.Select(v => VectorConvert(v)).ToArray();
            mesh.uv = mesh.vertices.Select(v => new Vector2(v.x, v.y)).ToArray();
            mesh.triangles = rco.GetGlobalMesh().TriangleList.SelectMany(t => new List<int>() { t[0], t[1], t[2] }).Reverse().ToArray();
            mesh.RecalculateNormals();

            meshObject.GetComponent<MeshRenderer>().material = VoxelMaterial;
        }
    }

    private void CheckOverlapRuntime(ModelObjectScript mosEditingObj, ModelObject mo)
    {
        // Just an Overlap Check for testing
        bool overlapingSomething = false;
        RuleCheckObject rco1 = new RuleCheckObject(mo);
        foreach (ModelObjectScript mos in ModelObjects)
        {
            if (mos == mosEditingObj)
            {
                continue;
            }
            mos.UnHighlight();

            RuleCheckObject rco2 = new RuleCheckObject(mos.ModelObject);
            if (Utils.MeshOverlapTest1(rco1.GetGlobalMesh(), rco2.GetGlobalMesh(), 1.0))
            {
                mos.Highlight(HighlightMatRed);
                overlapingSomething = true;
            }
        }
        if (overlapingSomething)
        {
            mosEditingObj.Highlight(HighlightMatRed);
        }
        else
        {
            mosEditingObj.UnHighlight();
        }
    }

    public void SaveOverlapInstanceClicked()
    {
        RuleCheckObject rco1 = new RuleCheckObject(FirstObject.ModelObject);
        RuleCheckObject rco2 = new RuleCheckObject(SecondObject.ModelObject);

        string outputString = "";
        outputString += string.Join("\n", rco1.GlobalVerticies.Select(v => v.ToString()));
        outputString += "\n\n";
        outputString += string.Join("\n", rco1.Triangles.Select(t => string.Join(",",t)));
        outputString += "\n\n";
        outputString += string.Join("\n", rco2.GlobalVerticies.Select(v => v.ToString()));
        outputString += "\n\n";
        outputString += string.Join("\n", rco2.Triangles.Select(t => string.Join(",", t)));

        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\testing.txt";
        File.WriteAllText(path, outputString);
    }

    #endregion

    #region Camera Setup Stuff:

    private void SetupMainCamera()
    {
        List<ModelObject> mos = ModelObjects.Select(m => m.ModelObject).ToList();
        List<Vector3D> vList = mos.SelectMany(m => m.Components.SelectMany(c => c.Vertices.Select(v => Vector3D.Add(v, m.Location)))).ToList();
        Utils.GetXYZDimentions(vList, out Vector3D mid, out Vector3D dims);

        Vector3 center = VectorConvert(mid);
        Vector3 diment = VectorConvert(dims);

        Debug.Log(vList.Count);
        Debug.Log(mos.Count);
        Debug.Log(center);

        MainCamera.orthographic = false;
        MainCamera.nearClipPlane = 0.1f;
        MainCamera.farClipPlane = 100.0f;
        MainCamera.transform.position = new Vector3(center.x, center.y + 2.0f * diment.y, center.z);
        MainCamera.transform.LookAt(center, Vector3.up);
    }

    #endregion
}