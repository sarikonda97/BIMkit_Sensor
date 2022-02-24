using DbmsApi.API;
using DbmsApi.Mongo;
using MathPackage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdminApp
{
    public partial class TestBoxObjectForm : Form
    {
        public MongoCatalogObject MongoCatalogObject;

        public TestBoxObjectForm(List<ObjectType> types)
        {
            InitializeComponent();

            this.comboBoxType.Items.Clear();
            this.comboBoxType.Items.AddRange(types.Select(t => t.Name).ToArray());
            this.comboBoxType.SelectedIndex = 0;
        }

        private void buttonDone_Click(object sender, EventArgs e)
        {
            double x = 0, y = 0, z = 0;
            bool validValues = true;
            try
            {
                x = Convert.ToDouble(this.textBoxWidth.Text);
                y = Convert.ToDouble(this.textBoxDepth.Text);
                z = Convert.ToDouble(this.textBoxHeight.Text);
            }
            catch
            {
                validValues = false;
            }

            if (string.IsNullOrWhiteSpace(this.textBoxName.Text) || string.IsNullOrWhiteSpace((string)this.comboBoxType.SelectedItem))
            {
                validValues = false;
            }

            if (!validValues)
            {
                MessageBox.Show("Invalid Values");
                return;
            }

            Mesh bbMesh = Utils.CreateBoundingBox(new Vector3D(), new Vector3D(x, y, z), FaceSide.FRONT);

            MeshRep newMesh = new MeshRep()
            {
                Components = new List<Component>() { new Component() { Triangles = bbMesh.TriangleList, Vertices = bbMesh.VertexList } },
                Joints = new List<Joint>(),
                LevelOfDetail = LevelOfDetail.LOD200
            };

            MongoCatalogObject = new MongoCatalogObject()
            {
                Id = null,
                MeshReps = new List<MeshRep>() { newMesh },
                Name = this.textBoxName.Text,
                Properties = new DbmsApi.API.Properties(),
                TypeId = (string)this.comboBoxType.SelectedItem
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
