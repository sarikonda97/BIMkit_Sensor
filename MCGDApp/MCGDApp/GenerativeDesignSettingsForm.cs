using GenerativeDesignPackage;
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
    public partial class GenerativeDesignSettingsForm : Form
    {
        public GenerativeDesignSettings Settings;

        public GenerativeDesignSettingsForm(GenerativeDesignSettings settings)
        {
            InitializeComponent();

            Settings = settings;
            this.textBoxIterations.Text = Settings.Itterations.ToString();
            this.textBoxMovement.Text = Settings.Movement.ToString();
            this.textBoxRate.Text = Settings.Rate.ToString();
            this.textBoxMoves.Text = Settings.Moves.ToString();
            this.checkBoxShowRoute.Checked = Settings.ShowRoute;
        }

        private void buttonDone_Click(object sender, EventArgs e)
        {
            int itterations = Convert.ToInt32(this.textBoxIterations.Text);
            double movement = Convert.ToDouble(this.textBoxMovement.Text);
            double rate = Convert.ToDouble(this.textBoxRate.Text);
            int moves = Convert.ToInt32(this.textBoxMoves.Text);
            bool showRoute = this.checkBoxShowRoute.Checked;

            Settings = new GenerativeDesignSettings(itterations, movement, rate, moves, showRoute);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
