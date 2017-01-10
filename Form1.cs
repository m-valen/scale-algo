//CODE DISCLAIMER
//Solely as an accommodation to Subscriber, Sterling has agreed to provide sample source code
//("Code") to demonstrate how Subscriber might be able to develop its own software code to
//facilitate Subscriber's interaction with the Sterling front-end modules via Sterling's Application
//Programming Interface (API).  Sterling is providing the Code "as is", and Sterling has no obligation to
//provide any updates, revisions, modifications or enhancements to the Code.  Sterling is not writing
//or assisting in writing Subscriber's code, and consequently Sterling has no responsibility or liability
//for Subscriber's use or modification of the Code.  STERLING EXPRESSLY DISCLAIMS ALL
//REPRESENTATIONS AND WARRANTIES, EXPRESS OR IMPLIED, WITH RESPECT
//TO THE CODE, INCLUDING THE WARRANTIES OF MERCHANTABILITY AND OF
//FITNESS FOR A PARTICULAR PURPOSE. UNDER NO CIRCUMSTANCES
//INCLUDING NEGLIGENCE SHALL STERLING BE LIABLE FOR ANY DAMAGES,
//INCIDENTAL, SPECIAL, CONSEQUENTIAL OR OTHERWISE (INCLUDING WITHOUT
//LIMITATION DAMAGES FOR LOSS OF PROFITS, BUSINESS INTERRUPTION, LOSS
//OF INFORMATION OR OTHER PECUNIARY LOSS) THAT MAY RESULT FROM THE
//USE OF OR INABILITY TO USE THE CODE, EVEN IF STERLING HAS BEEN ADVISED
//OF THE POSSIBILITY OF SUCH DAMAGES.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Xml.Serialization;
using System.Diagnostics;


namespace SterlingAlgos
{
    public partial class Form1 : Form
    {
        private SterlingLib.STIQuote stiQuote = new SterlingLib.STIQuote();
        private SterlingLib.STIApp stiApp = new SterlingLib.STIApp();
        private List<string> listMsg = new List<string>();
        private bool bModeXML = true;
        private ScaleTrade scaleTrade = new ScaleTrade();
        private string lastModified;
        private bool rangeBoxTopEdited = true;
        private bool rangeBoxBottomEdited = true;
        private bool startPriceEdited = true;
        private bool totalRangeBoxEdited = true;
        private bool autoProfit = false;
        private System.Timers.Timer _delayTimer = new System.Timers.Timer();


        public Form1()
        {
            InitializeComponent();
            stiApp.SetModeXML(bModeXML);
            
            //stiQuote.OnSTIQuoteUpdateXML += new SterlingLib._ISTIQuoteEvents_OnSTIQuoteUpdateXMLEventHandler(OnSTIQuoteUpdateXML);
            //stiQuote.OnSTIQuoteSnapXML += new SterlingLib._ISTIQuoteEvents_OnSTIQuoteSnapXMLEventHandler(OnSTIQuoteSnapXML);
        }


        //Overload for auto profit
        public Form1(decimal startingPrice, bool restart, int initialPosition, int maxSize, int startingSize, decimal incrementPrice, decimal profitOffset, int incrementSize, string symbol, string direction)
        {
            
            InitializeComponent();
            stiApp.SetModeXML(bModeXML);

            //stiQuote.OnSTIQuoteUpdateXML += new SterlingLib._ISTIQuoteEvents_OnSTIQuoteUpdateXMLEventHandler(OnSTIQuoteUpdateXML);
            //stiQuote.OnSTIQuoteSnapXML += new SterlingLib._ISTIQuoteEvents_OnSTIQuoteSnapXMLEventHandler(OnSTIQuoteSnapXML);
            Shown += Form1_Shown;
           

            autoProfit = true;

            //Connect to symbol
            //stiQuote.DeRegisterAllQuotes();
            //lbMsgs.Items.Clear();
            //stiQuote.RegisterQuote(symbol, "");
            symbolText.Text = symbol;
            
            if (direction == "B")
            {
                radioButton1.Checked = true;
            }
            else if (direction == "S")
            {
                radioButton2.Checked = true;
            }


            numericUpDown6.Value = startingPrice;
            checkBox4.Checked = true;
            numericUpDown10.Value = initialPosition;
            numericUpDown3.Value = maxSize;
            numericUpDown4.Value = startingSize;
            numericUpDown7.Value = incrementPrice;
            numericUpDown5.Value = incrementSize;
            

            if (errorChecker()) fieldCalculators();

            numericUpDown9.Value = profitOffset;

            //MessageBox.Show("End of load");

            /*_delayTimer.Interval = 3000;

            _delayTimer.Elapsed += _delayTimer_Elapsed;
            _delayTimer.Start(); */

        }

        public void Form1_Shown(object sender, EventArgs e)
        {
            //if (errorChecker()) fieldCalculators();
            //numericUpDown6.Value = numericUpDown2.Value + numericUpDown11.Value;
            //MessageBox.Show("Click to Submit", "Submit", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //button2.PerformClick();
        }




        private void _delayTimer_Elapsed (object sender, System.Timers.ElapsedEventArgs e)
        {
            MessageBox.Show("Here");
            button2.PerformClick();
            
            _delayTimer.Stop();
        }

        private void RegQuote_Click(object sender, EventArgs e)
        {
            stiQuote.DeRegisterAllQuotes();
            lbMsgs.Items.Clear();
            stiQuote.RegisterQuote(symbolText.Text, "");
            
            
        }


        private void OnSTIQuoteSnapXML(ref string strQuote)
        {
            XmlSerializer xs = new XmlSerializer(typeof(SterlingLib.structSTIQuoteSnap));
            SterlingLib.structSTIQuoteSnap structQuote = (SterlingLib.structSTIQuoteSnap)xs.Deserialize(new StringReader(strQuote));
            AddListBoxItem(structQuote.fLastPrice);
            decimal lastPrice = Convert.ToDecimal(structQuote.fLastPrice);
            lastPrice = Math.Round(lastPrice, 2);
            SetValue(lastPrice, structQuote.bstrSymbol); //Also clears error on symbolText 


        }

        private void OnSTIQuoteUpdateXML(ref string strQuote)
        {
            XmlSerializer xs = new XmlSerializer(typeof(SterlingLib.structSTIQuoteUpdate));
            SterlingLib.structSTIQuoteUpdate structQuote = (SterlingLib.structSTIQuoteUpdate)xs.Deserialize(new StringReader(strQuote));
            AddListBoxItem(structQuote.fLastPrice);
        }

        delegate void SetValueCallback(decimal value, string symbol);

        private void SetValue(decimal value, string symbol)
        {
            if (this.numericUpDown6.InvokeRequired)
            {
                SetValueCallback d = new SetValueCallback(SetValue);
                this.Invoke(d, new object[] { value, symbol });
            }
            else
            {
                if (!autoProfit) { 
                    this.numericUpDown6.Value = value;
                }
            }
            if (this.numericUpDown8.InvokeRequired)
            {
                SetValueCallback d = new SetValueCallback(SetValue);
                this.Invoke(d, new object[] { value, symbol });
            }
            else
            {
                this.numericUpDown8.Value = value;
            }
            if (this.textBox4.InvokeRequired)
            {
                SetValueCallback d = new SetValueCallback(SetValue);
                this.Invoke(d, new object[] { value, symbol });
            }
            else
            {
                this.textBox4.Text = symbol;
            }
            if (this.symbolText.InvokeRequired)
            {
                SetValueCallback d = new SetValueCallback(SetValue);
                this.Invoke(d, new object[] { value, symbol });
            }
            else
            {
                errorProvider1.SetError(symbolText, "");
                if (errorChecker()) fieldCalculators();
            }


        }

        public delegate void AddListBoxItemDelegate(object structQuote);

        private void AddListBoxItem(object structQuote){
            if (InvokeRequired)
            {
                this.lbMsgs.Invoke(new AddListBoxItemDelegate(AddListBoxItem), structQuote);
            }
            else
                if (Convert.ToInt32(structQuote) != 0)
                {
                    lbMsgs.Items.Insert(0, structQuote);
                }
            }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void symbolText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                RegQuote_Click(sender, e);
               
                
            }
        }

        private void lbMsgs_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (e.KeyChar == 118) //Right Arrow
            {
                numericUpDown1.Value += Convert.ToDecimal(.05);
            }

            if (e.KeyChar == 122) //Left Arrow
            {
                numericUpDown1.Value -= Convert.ToDecimal(.05);
            }

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
    (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            


            /* only allow two decimal points
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -2))
            {
                e.Handled = true;
            }*/
        }

        private void numericUpDown2_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 118) //Right Arrow
            {
                numericUpDown2.Value += Convert.ToDecimal(.05);
            }

            if (e.KeyChar == 122) //Left Arrow
            {
                numericUpDown2.Value -= Convert.ToDecimal(.05);
            }

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
    (e.KeyChar != '.'))
            {
                e.Handled = true;
            }




            /* only allow two decimal points
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -2))
            {
                e.Handled = true;
            }*/
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }


        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                radioButton1.Checked = true;
            }
            if (e.KeyCode == Keys.F5)
            {
                radioButton2.Checked = true;
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown6_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 122) //Z
            {
                numericUpDown6.Value -= Convert.ToDecimal(0.05);
            }
            if (e.KeyChar == 118) //V
            {
                numericUpDown6.Value += Convert.ToDecimal(.05);
            }
        }

        private void numericUpDown7_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 122) //Z
            {
                numericUpDown7.Value -= Convert.ToDecimal(0.05);
            }
            if (e.KeyChar == 118) //V
            {
                numericUpDown7.Value += Convert.ToDecimal(.05);
            }
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (errorChecker()) fieldCalculators();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (errorChecker()) fieldCalculators();
        }


        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (errorChecker()) fieldCalculators();
        }

        private bool errorChecker() //returns false if errors found
        {
            bool isError = false;
            // Check for multiples of 50

            /*if (textBox4.Text == "" || !stiQuote.IsQuoteRegistered(textBox4.Text, ""))
            {
                errorProvider1.SetError(symbolText, "Connect to a symbol");
                isError = true;
            }
            else
            {
                errorProvider1.SetError(symbolText, "");
            }*/

            if (isError) return false;

            if ((numericUpDown3.Value % 50) != 0)
            {
                errorProvider1.SetError(numericUpDown3, "Must be a multiple of 50");
                isError = true;
            }
            else
            {
                errorProvider1.SetError(numericUpDown3, "");
            }
            if ((numericUpDown4.Value % 50) != 0)
            {
                errorProvider1.SetError(numericUpDown4, "Must be a multiple of 50");
                isError = true;
            }
            else
            {
                errorProvider1.SetError(numericUpDown4, "");
            }
            if ((numericUpDown5.Value % 50) != 0)
            {
                errorProvider1.SetError(numericUpDown5, "Must be a multiple of 50");
                isError = true;
            }
            else
            {
                errorProvider1.SetError(numericUpDown5, "");
            }

            if (isError)
            {
                return false;
            }

            // Share size multi-fields
            // Max shares must be greater than starting shares
            if (numericUpDown4.Value > numericUpDown3.Value)
            {
                errorProvider1.SetError(numericUpDown4, "Starting shares must be equal or less than max shares");
                errorProvider1.SetError(numericUpDown3, "Max shares must be equal or greater than starting shares");
                isError = true;
            }
            else
            {
                errorProvider1.SetError(numericUpDown3, "");
                errorProvider1.SetError(numericUpDown4, "");
            }

            if (isError) return false;


            //Share sizes must be divisible by increment size

            if (numericUpDown3.Value % numericUpDown5.Value != 0)
            {
                errorProvider1.SetError(numericUpDown3, "Max size must be divisible by increment size");
                errorProvider1.SetError(numericUpDown5, "Increment size must be a divisor of max size & starting size");
                isError = true;
            }
            else
            {
                errorProvider1.SetError(numericUpDown3, "");
                if (numericUpDown4.Value % numericUpDown5.Value == 0) errorProvider1.SetError(numericUpDown5, "");
            }
            if (numericUpDown4.Value % numericUpDown5.Value != 0)
            {
                errorProvider1.SetError(numericUpDown4, "Max size must be divisible by increment size");
                errorProvider1.SetError(numericUpDown5, "Increment size must be a divisor of max size & starting size");
                isError = true;
            }
            else
            {
                errorProvider1.SetError(numericUpDown4, "");
                if (numericUpDown3.Value % numericUpDown5.Value == 0) errorProvider1.SetError(numericUpDown5, "");
            }

            //Restart initial position must be equal or less than max size, and divisible by increment size
            if (checkBox4.Checked)
            {
                if (numericUpDown10.Value > numericUpDown3.Value)
                {
                    errorProvider1.SetError(numericUpDown10, "Initial position must be equal or less than max size");
                    isError = true;
                }
                else
                {
                    if (numericUpDown10.Value % numericUpDown5.Value == 0) errorProvider1.SetError(numericUpDown10, "");
                }
                if (numericUpDown10.Value % numericUpDown5.Value != 0)
                {
                    errorProvider1.SetError(numericUpDown10, "Initial position must be divisible by increment size");
                    isError = true;
                }
                else
                {
                    if (numericUpDown10.Value <= numericUpDown3.Value) errorProvider1.SetError(numericUpDown10, "");
                }
            }

            //Hard stop must be higher than top of range for sell, and lower than bottom of range for buy
            if (checkBox3.Checked)
            {
                if (radioButton1.Checked) //Buy order
                {
                    if (numericUpDown8.Value >= numericUpDown2.Value)
                    {
                        errorProvider1.SetError(numericUpDown8, "Stop price must be lower than bottom of range for buy trades");
                        isError = true;
                    }
                    else
                    {
                        errorProvider1.SetError(numericUpDown8, "");
                    }
                }
                else if (radioButton2.Checked) //Sell order
                {
                    if (numericUpDown8.Value <= numericUpDown1.Value)
                    {
                        errorProvider1.SetError(numericUpDown8, "Stop price must be higher than top of range for sell trades");
                        isError = true;
                    }
                    else
                    {
                        errorProvider1.SetError(numericUpDown8, "");
                    }
                }
            }
            //Check range values
            /*
            if (numericUpDown1.Value <= numericUpDown2.Value)
            {
                errorProvider1.SetError(numericUpDown1, "Top of range must be greater than bottom of range");
                errorProvider1.SetError(numericUpDown2, "Bottom of range must be less than top of range");
                isError = true;
            }
            else
            {
                errorProvider1.SetError(numericUpDown1, "");
                errorProvider1.SetError(numericUpDown2, "");
            }
            */
            if (isError) return false;



            return true;

        }

        private void numericUpDown7_Leave(object sender, EventArgs e)
        {
            if (errorChecker()) fieldCalculators();
        }

        private void fieldCalculators()
        {
            int incrementsBelow = 0;
            int incrementsAbove = 0;

            //MessageBox.Show("Calculating"); 
            if (radioButton2.Checked)
            {
                incrementsBelow = Decimal.ToInt32(numericUpDown4.Value / numericUpDown5.Value);
                incrementsAbove = Decimal.ToInt32((numericUpDown3.Value - numericUpDown4.Value) / numericUpDown5.Value);
            }

            else
            { 
                incrementsBelow = Decimal.ToInt32((numericUpDown3.Value - numericUpDown4.Value) / numericUpDown5.Value);
                incrementsAbove = Decimal.ToInt32(numericUpDown4.Value / numericUpDown5.Value);
            }

            rangeBoxTopEdited = false; //Setting flag to skip over range box value change function
            rangeBoxBottomEdited = false;


            decimal rangeTop = numericUpDown6.Value + (incrementsAbove * numericUpDown7.Value);
            decimal rangeBottom = numericUpDown6.Value - (incrementsBelow * numericUpDown7.Value);

            

            if (rangeBottom < Convert.ToDecimal(0.01))
            {
                errorProvider1.SetError(numericUpDown2, "Bottom of range must be 0.01 or greater");
                return;
            }
            else
            {
                errorProvider1.SetError(numericUpDown2, "");
            }

            numericUpDown1.Value = rangeTop; //Range Top
            numericUpDown2.Value = rangeBottom; //Range Bottom

            rangeBoxBottomEdited = true;
            rangeBoxTopEdited = true;

            numericUpDown9.Value = numericUpDown7.Value; //Profit Offset

            totalRangeBoxEdited = false;
            numericUpDown11.Value = numericUpDown1.Value - numericUpDown2.Value; //Range input
            totalRangeBoxEdited = true;


            textBox2.Text = Convert.ToString(numericUpDown1.Value - numericUpDown2.Value);
            textBox1.Text = Convert.ToString(incrementsBelow + incrementsAbove);

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                radioButton1.BackColor = System.Drawing.Color.RoyalBlue;
                radioButton2.BackColor = System.Drawing.SystemColors.Control;
            }
            else
            {
                radioButton1.BackColor = System.Drawing.SystemColors.Control;
                radioButton2.BackColor = System.Drawing.Color.Coral;
            }
            if (errorChecker()) fieldCalculators();
        }

        private void numericUpDown6_Leave(object sender, EventArgs e)
        {
            if (startPriceEdited == true) { 
                if (errorChecker()) fieldCalculators();
            }
        }

        private void button2_Click(object sender, EventArgs e) //Initiate Scale Trade
        {
            if (!errorChecker()) return; //Error checker
            
            if (!scaleTrade.isRunning || checkBox4.Enabled) { //checkbox4.Enabled = restart scale trade 

            //Symbol
            scaleTrade.symbol = symbolText.Text;

            //direction
            if (radioButton1.Checked) scaleTrade.direction = "B";
            else scaleTrade.direction = "S";

            //Share sizing
            scaleTrade.startingSize = Convert.ToInt32(numericUpDown4.Value);
            scaleTrade.maxSize = Convert.ToInt32(numericUpDown3.Value);
            scaleTrade.incrementSize = Convert.ToInt32(numericUpDown5.Value);

            //Prices
            scaleTrade.startingPrice = numericUpDown6.Value;
            scaleTrade.incrementPrice = numericUpDown7.Value;

            //Range
            scaleTrade.rangeTop = numericUpDown1.Value;
            scaleTrade.rangeBottom = numericUpDown2.Value;

            //Hard stop
            scaleTrade.hasHardStop = checkBox3.Checked;
            if (scaleTrade.hasHardStop) scaleTrade.hardStop = numericUpDown8.Value;

            //Take Profit orders
            scaleTrade.takeProfit = checkBox1.Checked;
            if (scaleTrade.takeProfit) scaleTrade.profitOffset = numericUpDown9.Value;

            scaleTrade.restoreSize = checkBox2.Checked;


            
            //Restart
            if (checkBox4.Checked)
            {
                scaleTrade.isRestart = true;
                scaleTrade.restartInitialLevels = Convert.ToInt32(numericUpDown10.Value / numericUpDown5.Value);
            }

            //Summary
            scaleTrade.range = Convert.ToDecimal(textBox2.Text);
            scaleTrade.numberOfPriceLevels = Convert.ToInt32(textBox1.Text);



            

            bool tradeSuccess = scaleTrade.initiateTrade();
            if (tradeSuccess)
                {
                    button2.Enabled = false;
                    button1.Enabled = true;
                    checkBox4.Checked = false;

                    //Disable form fields while trade is running and not restart
                    radioButton1.Enabled = false;
                    radioButton2.Enabled = false;
                    numericUpDown3.Enabled = false;
                    numericUpDown4.Enabled = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    numericUpDown7.Enabled = false;
                    checkBox3.Enabled = false;

                    textBox5.Text = "Running";
                }
            }
            else
            {
                MessageBox.Show("There is already a scale trade in progress.");
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (scaleTrade.isRunning)
            {
                scaleTrade.stop();
                startButton.Enabled = true;
                button1.Enabled = false;
                textBox5.Text = "Stopped";
            }
            else
            {
                MessageBox.Show("No scale trade currently running.");
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (scaleTrade.isRunning)
            {
                scaleTrade.restoreSize = checkBox2.Checked;
                scaleTrade.start();
                button1.Enabled = true;
                startButton.Enabled = false;
                textBox5.Text = "Running";
            }
            else
            {
                MessageBox.Show("No scale trade currently running.");
            }
        }

        private void checkBox3_Click(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                numericUpDown8.Enabled = true;
            }
            else if (!checkBox3.Checked)
            {
                numericUpDown8.Enabled = false;
            }
        }

        private void checkBox4_Click(object sender, EventArgs e)
        {
            checkBox4_CheckedChanged(sender, e);
        }

        private void numericUpDown10_ValueChanged(object sender, EventArgs e)
        {
            if (errorChecker()) fieldCalculators();
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            if (errorChecker()) fieldCalculators();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e) //Top range box
        {
            if (rangeBoxTopEdited) //Checking flag so only triggers when user modifies the field directly
            {
                rangeChangedFieldCalculator("TOP");
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (rangeBoxBottomEdited) //Checking flag so only triggers when user modifies the field directly
            {
                rangeChangedFieldCalculator("BOTTOM");
            }
        }

        private void rangeChangedFieldCalculator(string rangeBoxChanged)
        {
            int incrementsBelow = 0;
            int incrementsAbove = 0;

            if (radioButton2.Checked)
            {
                incrementsBelow = Decimal.ToInt32(numericUpDown4.Value / numericUpDown5.Value);
                incrementsAbove = Decimal.ToInt32((numericUpDown3.Value - numericUpDown4.Value) / numericUpDown5.Value);
            }

            else
            {
                incrementsBelow = Decimal.ToInt32((numericUpDown3.Value - numericUpDown4.Value) / numericUpDown5.Value);
                incrementsAbove = Decimal.ToInt32(numericUpDown4.Value / numericUpDown5.Value);
            }

            startPriceEdited = false;

            if (rangeBoxChanged == "TOP")
            {
                //Starting Price
                rangeBoxBottomEdited = false;
                

                numericUpDown6.Value = numericUpDown1.Value - (incrementsAbove * numericUpDown7.Value);

                //Range Bottom
                
                numericUpDown2.Value = numericUpDown6.Value - (incrementsBelow * numericUpDown7.Value);

            }

            else if (rangeBoxChanged == "BOTTOM")
            {
                rangeBoxTopEdited = false;

                //Starting Price
                numericUpDown6.Value = numericUpDown2.Value + (incrementsBelow * numericUpDown7.Value);

                //Range Top
                numericUpDown1.Value = numericUpDown6.Value + (incrementsAbove * numericUpDown7.Value);

            }
            rangeBoxBottomEdited = true;
            startPriceEdited = true;
        }

        private void numericUpDown11_ValueChanged(object sender, EventArgs e)
        {
            if (totalRangeBoxEdited == true && ((numericUpDown11.Value * 10000) % numericUpDown3.Value == 0))
            {
                //Set increment price
                numericUpDown7.Value = (numericUpDown11.Value * 100) / numericUpDown3.Value;
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                numericUpDown10.Enabled = true;
                startButton.Enabled = false;
                button2.Enabled = true;

                //Enable form fields
                radioButton1.Enabled = true;
                radioButton2.Enabled = true;
                numericUpDown3.Enabled = true;
                numericUpDown4.Enabled = true;
                numericUpDown5.Enabled = true;
                numericUpDown6.Enabled = true;
                numericUpDown7.Enabled = true;
                checkBox3.Enabled = true;
            }
            else if (!checkBox4.Checked)
            {
                numericUpDown10.Enabled = false;
                if (scaleTrade.isRunning)
                {
                    button2.Enabled = false;

                    radioButton1.Enabled = false;
                    radioButton2.Enabled = false;
                    numericUpDown3.Enabled = false;
                    numericUpDown4.Enabled = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    numericUpDown7.Enabled = false;
                    checkBox3.Enabled = false;

                }

            }
        }
    }
}
 