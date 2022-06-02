using DBMS.Controllers.DBControllers;
using DbmsApi.Mongo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace SensorApp
{
    public partial class RDFUpload : Form
    {
        string path;
        IGraph g;
        String selectedSubject;
        String selectedObject;

        protected MongoDbController db = MongoDbController.Instance;

        public RDFUpload()
        {
            InitializeComponent();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            //To where your opendialog box get starting location. My initial directory location is desktop.
            openFileDialog1.InitialDirectory = "C://Desktop";
            //Your opendialog box title name.
            openFileDialog1.Title = "Select file to be upload.";
            //which type file format you want to upload in database. just add them.
            openFileDialog1.Filter = "Select Valid Document(*.ttl)|*.ttl";
            //FilterIndex property represents the index of the filter currently selected in the file dialog box.
            openFileDialog1.FilterIndex = 1;
            try
            {
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (openFileDialog1.CheckFileExists)
                    {
                        path = System.IO.Path.GetFullPath(openFileDialog1.FileName);
                        fileName.Text = path;
                    }
                }
                else
                {
                    MessageBox.Show("Please Upload document.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void loadNodesButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Populate the set of location subclasses
                HashSet<String> locations = new HashSet<String>();
                locations.Add("Building");
                locations.Add("Floor");
                locations.Add("Room");

                // get all possible nodes inside the RDF file.
                Object deviceTypes = g.ExecuteQuery("SELECT DISTINCT ?type WHERE { ?s a ?type }");

                List<String> nodes = new List<String>();

                if (deviceTypes is SparqlResultSet)
                {
                    SparqlResultSet rset = (SparqlResultSet)deviceTypes;
                    foreach (SparqlResult r in rset)
                    {
                        String rString = r.ToString();
                        String nodeName = rString.Substring(rString.LastIndexOf('#') + 1);
                        nodes.Add(nodeName);
                    }
                }

                // Add the nodes into MongoDB as a catalog
                foreach (String node in nodes)
                {
                    MongoDeviceObject mongoDeviceObject = new MongoDeviceObject()
                    {
                        Name = node,
                        TypeId = locations.Contains(node) ? "Location" : "Equipment",
                    };
                    string coId = db.CreateDeviceObject(mongoDeviceObject);
                }

                MessageBox.Show("Successfully populated Nodes");
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private Boolean CheckListContains(List<List<String>> mainList, List<String> subList)
        {
            foreach (List<String> list in mainList)
            {
                if (list.SequenceEqual(subList))
                {
                    return true;
                }
            }
            return false;
        }

        private void loadRelationshipsButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Populate a dictionary of prefixes
                Dictionary<String, String> prefixMap = new Dictionary<String, String>();
                prefixMap.Add("https://brickschema.org/schema/Brick", "brick");
                prefixMap.Add("http://www.w3.org/1999/02/22-rdf-syntax-ns", "rdf");
                prefixMap.Add("http://www.w3.org/2000/01/rdf-schema", "rdfs");

                // Query and process possible predicate types
                Object predicateTypes = g.ExecuteQuery("SELECT DISTINCT ?type WHERE { ?s ?type ?a}");

                List<List<String>> predicates = new List<List<String>>();

                if (predicateTypes is SparqlResultSet)
                {
                    SparqlResultSet rset = (SparqlResultSet)predicateTypes;
                    foreach (SparqlResult r in rset)
                    {
                        String rString = r.ToString();
                        int startLocation = rString.IndexOf("http");
                        int endLocation = rString.LastIndexOf('#') - rString.IndexOf("http");
                        String prefix = rString.Substring(startLocation, endLocation);
                        String predicate = rString.Substring(rString.LastIndexOf('#') + 1);

                        List<String> temp = new List<string>();
                        temp.Add(prefixMap[prefix]);
                        temp.Add(predicate);

                        predicates.Add(temp);
                    }
                }

                // Identify the unique parent relationships in the entire RDF graph
                List<List<String>> uniqueRelationships = new List<List<String>>();
                foreach (List<String> predicate in predicates)
                {

                    String predicatePrefix = predicate[0];
                    String predicateType = predicate[1];

                    IUriNode predicateNode = g.CreateUriNode(predicatePrefix + ":" + predicateType);
                    IEnumerable<Triple> predicateResult = g.GetTriplesWithPredicate(predicateNode);

                    // starts here

                    foreach (Triple triple in predicateResult)
                    {
                        String subjectString = triple.Subject.ToString();
                        String objectString = triple.Object.ToString();
                        String subjectName = subjectString.Substring(subjectString.LastIndexOf('#') + 1);
                        String objectName = objectString.Substring(objectString.LastIndexOf('#') + 1);

                        // This might break
                        IUriNode subjectNode = g.CreateUriNode("soda_hall:" + subjectName);
                        IUriNode objectNode = g.CreateUriNode("soda_hall:" + objectName);

                        IEnumerable<Triple> subjectParent = g.GetTriplesWithSubject(subjectNode);
                        IEnumerable<Triple> objectParent = g.GetTriplesWithSubject(objectNode);

                        String subjectParentName = "";
                        String objectParentName = "";

                        foreach (Triple subjectTriple in subjectParent)
                        {
                            String subjectTriplePredicateString = subjectTriple.Predicate.ToString();
                            String subjectTripleObjectString = subjectTriple.Object.ToString();
                            String subjectTriplePredicateName = subjectTriplePredicateString.Substring(subjectTriplePredicateString.LastIndexOf('#') + 1);
                            String subjectTripleObjectName = subjectTripleObjectString.Substring(subjectTripleObjectString.LastIndexOf('#') + 1);

                            if (subjectTriplePredicateName == "type")
                            {
                                subjectParentName = subjectTripleObjectName;
                            }
                        }

                        foreach (Triple objectTriple in objectParent)
                        {
                            String objectTriplePredicateString = objectTriple.Predicate.ToString();
                            String objectTripleObjectString = objectTriple.Object.ToString();
                            String objectTriplePredicateName = objectTriplePredicateString.Substring(objectTriplePredicateString.LastIndexOf('#') + 1);
                            String objectTripleObjectName = objectTripleObjectString.Substring(objectTripleObjectString.LastIndexOf('#') + 1);

                            if (objectTriplePredicateName == "type")
                            {
                                objectParentName = objectTripleObjectName;
                            }
                        }

                        List<String> temp = new List<String>();
                        temp.Add(subjectParentName);
                        temp.Add(predicateType);
                        temp.Add(objectParentName);

                        if (temp[0] != "" && temp[1] != "" && temp[2] != "" && !CheckListContains(uniqueRelationships, temp))
                        {
                            uniqueRelationships.Add(temp);
                        }
                    }
                }

                // Dump unique relationships into MongoDB as deviceRelationships document
                foreach (List<String> relationship in uniqueRelationships)
                {
                    MongoDeviceRelationships mongoRelationsObject = new MongoDeviceRelationships()
                    {
                        Subject = relationship[0],
                        Predicate = relationship[1],
                        Object = relationship[2]
                    };
                    string coId = db.CreateDeviceRelationship(mongoRelationsObject);
                }
                MessageBox.Show("Relationships populated successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void populateDropDownButton_Click(object sender, EventArgs e)
        {
            List<String> subjects = db.GetUniqueSubjects();
            List<String> objects = db.GetUniqueObjects();
            if (subjectDropDown.Items.Count == 0 && objectDropDown.Items.Count == 0)
            {
                foreach (String subject in subjects)
                {
                    subjectDropDown.Items.Add(subject);
                }
                foreach (String obj in objects)
                {
                    objectDropDown.Items.Add(obj);
                }
            }
        }

        private void subjectDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object selectedItem = subjectDropDown.SelectedItem;
            /*int selectedIndex = subjectDropDown.SelectedIndex;*/ // unnecessary for now

            if(selectedItem is String)
            {
                selectedSubject = (String)selectedItem;
            }
        }

        private void objectDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object selectedItem = objectDropDown.SelectedItem;
            /*int selectedIndex = subjectDropDown.SelectedIndex;*/ // unnecessary for now

            if (selectedItem is String)
            {
                selectedObject = (String)selectedItem;
            }
        }

        private void getValidButton_Click(object sender, EventArgs e)
        {
            if (selectedSubject != null && selectedObject != null) {
                List<String> possiblePredicates = db.GetDeviceRelationship(selectedSubject, selectedObject);

                predicateOutput.Text = "";
                if (possiblePredicates.Count == 0)
                {
                    predicateOutput.AppendText("No Valid Relationships" + Environment.NewLine);
                }

                foreach(String predicate in possiblePredicates)
                {
                    predicateOutput.AppendText(predicate + Environment.NewLine);
                }
            } 
            else
            {
                MessageBox.Show("Please select valid subject and object.");
            }
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            try
            {
                string filename = path;
                if (filename == null)
                {
                    MessageBox.Show("Please select a valid document.");
                }
                else
                {
                    g = new Graph();
                    TurtleParser ttlParser = new TurtleParser();
                    ttlParser.Load(g, path);
                    MessageBox.Show("Upload successful!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        
    }
}
