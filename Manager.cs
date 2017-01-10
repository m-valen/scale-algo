using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;

namespace SterlingAlgos
{
    public partial class Manager : Form
    {
        private SterlingLib.STIApp stiApp = new SterlingLib.STIApp();
        private bool bModeXML = true;
        
        private SterlingLib.ISTIOrder stiOrder = new SterlingLib.STIOrder();
        public bool autoProfitOn = false;



        public Manager()
        {
            
            InitializeComponent();
            stiApp.SetModeXML(bModeXML);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var myForm = new Form1();
            myForm.Show();
        }

        private void Manager_Load(object sender, EventArgs e)
        {
            textBox1.Text = Properties.Settings.Default.AccountID;
            comboBox1.Text = Properties.Settings.Default.ProfitTakeMethod;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!textBox1.Enabled) //Edit has been clicked
            {
                textBox1.Enabled = true;
                button2.Text = "Save";
            }
            else if (textBox1.Enabled) //Save has been clicked
            {
                Properties.Settings.Default.AccountID = textBox1.Text;
                Properties.Settings.Default.Save();
                textBox1.Enabled = false;
                button2.Text = "Edit";
            }
        }

        /*private void button3_Click(object sender, EventArgs e)
        {
            
        }*/



        private void button4_Click(object sender, EventArgs e)
        {
            autoProfitOn = !autoProfitOn;
            if (autoProfitOn) { 
                var autoProfitForm = new AutoProfit();
                autoProfitForm.Show();
                autoProfitForm.FormClosed += AutoProfit_OnClose;
                button4.Enabled = false;
            }
            else
            {
                button4.Enabled = true;
            }
        }

        private void AutoProfit_OnClose(object sender, FormClosedEventArgs e)
        {
            //MessageBox.Show("Triggered!");
            autoProfitOn = false;
            button4.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!comboBox1.Enabled) //Edit has been clicked
            {
                comboBox1.Enabled = true;
                button3.Text = "Save";
            }
            else if (comboBox1.Enabled) //Save has been clicked
            {
                Properties.Settings.Default.ProfitTakeMethod = comboBox1.Text;
                Properties.Settings.Default.Save();
                comboBox1.Enabled = false;
                button3.Text = "Edit";
            }
        }
    }
}
