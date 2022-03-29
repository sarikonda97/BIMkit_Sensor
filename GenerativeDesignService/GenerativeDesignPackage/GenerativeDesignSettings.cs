using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeDesignPackage
{
    public class GenerativeDesignSettings
    {
        public int Itterations { get; }
        public double Movement { get; }
        public double Rate { get; }
        public int Moves { get; }
        public bool ShowRoute { get; }
        public bool FixedItterations { get; }

        public GenerativeDesignSettings(int itterations, double movement, double rate, int moves, bool showRoute, bool fixedItterations)
        {
            Itterations = itterations;
            Movement = movement;
            Rate = rate;
            Moves = moves;
            ShowRoute = showRoute;
            FixedItterations = fixedItterations;
        }

        // Defaults:
        public GenerativeDesignSettings()
        {
            Itterations = 10;
            Movement = 10;
            Rate = 0.5;
            Moves = 10;
            ShowRoute = false;
            FixedItterations = false;
        }
    }
}