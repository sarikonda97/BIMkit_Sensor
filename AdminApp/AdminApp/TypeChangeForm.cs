using DbmsApi;
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
    public partial class TypeChangeForm : Form
    {
        public string Type;

        public TypeChangeForm(List<ObjectType> types)
        {
            InitializeComponent();

            this.comboBoxTypes.Items.Clear();
            this.comboBoxTypes.Items.AddRange(types.Select(t=>t.Name).ToArray());
            this.comboBoxTypes.SelectedIndex = 0;
        }

        private void buttonDone_Click(object sender, EventArgs e)
        {
            this.Type = (string)this.comboBoxTypes.SelectedItem;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
