using ModelConverter;
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
    public partial class DatasetTypeSelectForm : Form
    {
        public ConverterGeneral.Datasets SelectedDataset;

        public DatasetTypeSelectForm()
        {
            InitializeComponent();

            this.comboBoxDatasetType.Items.AddRange(Enum.GetValues(typeof(ConverterGeneral.Datasets)).Cast<ConverterGeneral.Datasets>().Select(t => t.ToString()).ToArray());
            this.comboBoxDatasetType.SelectedIndex = 0;
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            SelectedDataset = (ConverterGeneral.Datasets)Enum.Parse(typeof(ConverterGeneral.Datasets), this.comboBoxDatasetType.SelectedItem.ToString());
            this.Close();

        }
    }
}
