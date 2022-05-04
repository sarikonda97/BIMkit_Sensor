using DbmsApi.API;
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

namespace MCGDApp
{
    public partial class RuleGeneratorSettingsForm : Form
    {
        public string Type1;
        public string Type2;
        public OccurrenceRule OC1;
        public OccurrenceRule OC2;

        public RuleGeneratorSettingsForm()
        {
            InitializeComponent();

            this.comboBoxEC1.Items.Clear();
            this.comboBoxEC2.Items.Clear();
            this.comboBoxType1.Items.Clear();
            this.comboBoxType2.Items.Clear();

            foreach (OccurrenceRule occurrence in Enum.GetValues(typeof(OccurrenceRule)))
            {
                this.comboBoxEC1.Items.Add(occurrence);
                this.comboBoxEC2.Items.Add(occurrence);
            }

            this.comboBoxType1.Items.AddRange(ObjectTypeTree.GetAllTypes().Select(t => t.Name).ToArray());
            this.comboBoxType2.Items.AddRange(ObjectTypeTree.GetAllTypes().Select(t => t.Name).ToArray());

            this.comboBoxEC1.SelectedIndex = 0;
            this.comboBoxEC2.SelectedIndex = 0;
            this.comboBoxType1.SelectedIndex = 0;
            this.comboBoxType2.SelectedIndex = 0;
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            Type1 = comboBoxType1.SelectedItem as string;
            Type2 = comboBoxType2.SelectedItem as string;
            OC1 = Enum.GetValues(typeof(OccurrenceRule)).Cast<OccurrenceRule>().ToList()[this.comboBoxEC1.SelectedIndex];
            OC2 = Enum.GetValues(typeof(OccurrenceRule)).Cast<OccurrenceRule>().ToList()[this.comboBoxEC2.SelectedIndex];

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
