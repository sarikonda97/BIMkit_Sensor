using DbmsApi.API;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdminApp
{
    public partial class TypeEditForm : Form
    {
        public ObjectType Type;
        public List<ObjectType> Types;

        public TypeEditForm(List<ObjectType> types)
        {
            InitializeComponent();
            Types = types;

            this.comboBoxTypes.Items.Clear();
            this.comboBoxTypes.Items.AddRange(types.Select(t => t.Name).ToArray());
            this.comboBoxTypes.SelectedIndex = 0;

            this.textBoxTypeName.Text = "NewType";
        }

        public TypeEditForm(ObjectType Type, List<ObjectType> types)
        {
            InitializeComponent();

            this.comboBoxTypes.Items.Clear();
            this.comboBoxTypes.Items.AddRange(types.Select(t => t.Name).ToArray());
            this.comboBoxTypes.SelectedItem = Type.ParentName;

            this.textBoxTypeName.Text = Type.Name;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.textBoxTypeName.Text))
            {
                return;
            }

            Type = new ObjectType(this.textBoxTypeName.Text, (string)this.comboBoxTypes.SelectedItem);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
