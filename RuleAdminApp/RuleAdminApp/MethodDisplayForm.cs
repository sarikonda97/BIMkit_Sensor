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

namespace RuleAdminApp
{
    public partial class MethodDisplayForm : Form
    {
        public MethodDisplayForm(List<ObjectType> types, Dictionary<string, ObjectType> VOs, Dictionary<string, Type> properties, Dictionary<string, Type> relations)
        {
            InitializeComponent();

            foreach (var kvp in types)
            {
                if (VOs.ContainsKey(kvp.Name))
                {
                    continue;
                }
                this.richTextBoxTypes.Text += kvp + "\n";
            }
            this.richTextBoxTypes.Text += "VOs===================\n";
            foreach (var kvp in VOs)
            {
                this.richTextBoxTypes.Text += kvp.Key.ToString() + " (" + kvp.Value + ")\n";
            }
            foreach (var kvp in properties)
            {
                this.richTextBoxProperties.Text += kvp.Key + " (" + kvp.Value.ToString() + ")\n";
            }
            foreach (var kvp in relations)
            {
                this.richTextBoxRelation.Text += kvp.Key + " (" + kvp.Value.ToString() + ")\n";
            }
        }
    }
}