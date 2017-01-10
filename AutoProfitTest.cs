using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SterlingAlgos
{
    public partial class AutoProfitTest : Form
    {
        public AutoProfitTest()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var myForm = new Form2(Convert.ToDecimal(45.00), true, 700, 700, 700, 4, Convert.ToDecimal(45.2), "XOP", "S");
            this.Invoke((MethodInvoker)delegate ()
            {
                myForm.Show();
            });
        }
    }
}
