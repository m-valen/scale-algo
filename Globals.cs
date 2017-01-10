using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SterlingAlgos
{
    public static class Globals
    {
        public static string account = Properties.Settings.Default.AccountID;
        public static string desination = "BATS";
        public static string profitTakeMethod = Properties.Settings.Default.ProfitTakeMethod;
    }
}
