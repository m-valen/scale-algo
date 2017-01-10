using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Specialized; //NameValueCollections
using System.Xml.Serialization;
using System.IO;
using System.Threading;

namespace SterlingAlgos
{
    public partial class AutoProfit : Form
    {
        bool autoProfitOn = false;
        private SterlingLib.STIApp stiApp = new SterlingLib.STIApp();
        private bool bModeXML = true;
        public SterlingLib.STIEvents stiEvents = new SterlingLib.STIEvents();
        private SterlingLib.STIPosition stiPosition = new SterlingLib.STIPosition();
        private Thread thread1 = new Thread(new ThreadStart(A));

        private decimal startingPrice;
        private int numIntervals;
        private decimal incrementPrice;
        private int startingSize;
        private decimal rangeSize;
        private decimal rangeEnd;
        private string direction;
        private decimal stopPrice;
   
        




        public AutoProfit()
        {
            InitializeComponent();
            stiApp.SetModeXML(bModeXML);
            stiEvents.SetOrderEventsAsStructs(true);
            stiEvents.OnSTIOrderUpdateXML += new SterlingLib._ISTIEventsEvents_OnSTIOrderUpdateXMLEventHandler(stiEvents_OnSTIOrderUpdateXML);
            stiPosition.DeRegisterPositions();
            stiPosition.RegisterForPositions();
            
            //stiPosition.OnSTIPositionUpdateXML += new SterlingLib._ISTIPositionEvents_OnSTIPositionUpdateXMLEventHandler(OnSTIPositionUpdateXML);
        }

        /*private void stiEvents_OnSTIOrderUpdateXML(ref string orderUpdateInfo)
        {
            XmlSerializer xs = new XmlSerializer(typeof(SterlingLib.structSTIOrderUpdate));
            SterlingLib.structSTIOrderUpdate structTrade = (SterlingLib.structSTIOrderUpdate)xs.Deserialize(new StringReader(orderUpdateInfo));
            //MessageBox.Show("Picked Up");


            
        }*/

        private void stiEvents_OnSTIOrderUpdateXML(ref string orderUpdateInfo)
        {
            XmlSerializer xs = new XmlSerializer(typeof(SterlingLib.structSTIOrderUpdate));
            SterlingLib.structSTIOrderUpdate structTrade = (SterlingLib.structSTIOrderUpdate)xs.Deserialize(new StringReader(orderUpdateInfo));
            //MessageBox.Show("Picked Up");


            if (autoProfitOn && Globals.profitTakeMethod == "3-Block")
            {

                if (structTrade.nOrderStatus == 13)
                {
                    if (structTrade.nPriceType == 7 || structTrade.nPriceType == 8)
                    {

                        Form rangePicker = new RangePicker() { TopMost = true, TopLevel = true, StartPosition = FormStartPosition.CenterScreen };
                        int rangeMultiple = 1;
                        DialogResult result = rangePicker.ShowDialog();
                        rangePicker.Activate();
                        rangePicker.TopMost = true;
                        rangePicker.Focus();
                        rangePicker.BringToFront();


                        if (result == DialogResult.Yes) rangeMultiple = 2;
                        if (result == DialogResult.No) rangeMultiple = 3;
                        if (result == DialogResult.OK) rangeMultiple = 4;
                        if (rangeMultiple == 1) return; //Exit if no value chosen
                        string positionSize = stiPosition.GetPositionInfo(structTrade.bstrSymbol, "", Globals.account);
                        if (Math.Abs(Convert.ToInt32(positionSize)) == (structTrade.nQuantity))
                        {
                            SterlingLib.structSTIPositionUpdate positionInfo = stiPosition.GetPositionInfoStruct(structTrade.bstrSymbol, "", Globals.account);
                            decimal positionCost = Math.Abs(Convert.ToDecimal(positionInfo.fPositionCost));
                            decimal averagePrice = Math.Abs(positionCost / Convert.ToDecimal(positionSize));

                            decimal stopRange = Math.Abs(averagePrice - Convert.ToDecimal(structTrade.fStpPrice));
                            //MessageBox.Show(averagePrice.ToString() + " : " + structTrade.fStpPrice + " : " + stopRange.ToString());

                            decimal profitRange = stopRange * rangeMultiple;
                            //MessageBox.Show(profitRange.ToString());
                            //Now that we have the range, create a summary of the profit taker with the info...

                            stopPrice = Convert.ToDecimal(structTrade.fStpPrice);
                            incrementPrice = Math.Abs(Math.Round((profitRange) / (Convert.ToInt32(positionSize) / 100), 2));
                            numIntervals = Math.Abs(Convert.ToInt32(positionSize) / 100);
                            profitRange = numIntervals * incrementPrice;
                            //MessageBox.Show(incrementPrice.ToString()); 

                            //Now we have increment price, max size, initial size, starting size, starting price (average price, rounded to 2 decimal places)

                            startingPrice = Math.Round(averagePrice, 2);
                            rangeSize = Math.Abs(Math.Round(profitRange, 2));
                            startingSize = Convert.ToInt32(positionSize);

                            if (Convert.ToInt32(positionSize) < 0)
                            {
                                direction = "S";
                                rangeEnd = startingPrice - rangeSize;
                            }
                            else if (Convert.ToInt32(positionSize) > 0)
                            {
                                direction = "B";
                                rangeEnd = startingPrice + rangeSize;
                            }
                            else
                            {
                                direction = "N";
                            }




                            var myForm = new Form2(startingPrice, true, Math.Abs(Convert.ToInt32(positionSize)), Math.Abs(Convert.ToInt32(positionSize)), Math.Abs(Convert.ToInt32(positionSize)),
                                                    rangeMultiple, Convert.ToDecimal(structTrade.fStpPrice), structTrade.bstrSymbol, direction);
                            this.Invoke((MethodInvoker)delegate ()
                            {

                                myForm.Show();

                            });







                        }
                        else MessageBox.Show("Stop order size must be equal to position size");
                    }
                }
            }
            else if (autoProfitOn && Globals.profitTakeMethod == "Increment")
            {

                if (structTrade.nOrderStatus == 13)
                {
                    if (structTrade.nPriceType == 7 || structTrade.nPriceType == 8)
                    {

                        Form rangePicker = new RangePicker() { TopMost = true, TopLevel = true, StartPosition = FormStartPosition.CenterScreen };
                        int rangeMultiple = 1;
                        DialogResult result = rangePicker.ShowDialog();
                        rangePicker.Activate();
                        rangePicker.TopMost = true;
                        rangePicker.Focus();
                        rangePicker.BringToFront();


                        if (result == DialogResult.Yes) rangeMultiple = 2;
                        if (result == DialogResult.No) rangeMultiple = 3;
                        if (result == DialogResult.OK) rangeMultiple = 4;
                        if (rangeMultiple == 1) return; //Exit if no value chosen
                        string positionSize = stiPosition.GetPositionInfo(structTrade.bstrSymbol, "", Globals.account);
                        if (Math.Abs(Convert.ToInt32(positionSize)) == (structTrade.nQuantity))
                        {
                            SterlingLib.structSTIPositionUpdate positionInfo = stiPosition.GetPositionInfoStruct(structTrade.bstrSymbol, "", Globals.account);
                            decimal positionCost = Math.Abs(Convert.ToDecimal(positionInfo.fPositionCost));
                            decimal averagePrice = Math.Abs(positionCost / Convert.ToDecimal(positionSize));

                            decimal stopRange = Math.Abs(averagePrice - Convert.ToDecimal(structTrade.fStpPrice));
                            //MessageBox.Show(averagePrice.ToString() + " : " + structTrade.fStpPrice + " : " + stopRange.ToString());

                            decimal profitRange = stopRange * rangeMultiple;
                            //MessageBox.Show(profitRange.ToString());
                            //Now that we have the range, create a summary of the profit taker with the info...

                            stopPrice = Convert.ToDecimal(structTrade.fStpPrice);
                            incrementPrice = Math.Abs(Math.Round((profitRange) / (Convert.ToInt32(positionSize) / 100), 2));
                            numIntervals = Math.Abs(Convert.ToInt32(positionSize) / 100);
                            profitRange = numIntervals * incrementPrice;
                            //MessageBox.Show(incrementPrice.ToString()); 

                            //Now we have increment price, max size, initial size, starting size, starting price (average price, rounded to 2 decimal places)

                            startingPrice = Math.Round(averagePrice, 2);
                            rangeSize = Math.Abs(Math.Round(profitRange, 2));
                            startingSize = Convert.ToInt32(positionSize);

                            if (Convert.ToInt32(positionSize) < 0)
                            {
                                direction = "S";
                                rangeEnd = startingPrice - rangeSize;
                            }
                            else if (Convert.ToInt32(positionSize) > 0)
                            {
                                direction = "B";
                                rangeEnd = startingPrice + rangeSize;
                            }
                            else
                            {
                                direction = "N";
                            }


                            decimal profitOffset = stopRange;

                            var myForm = new Form1(startingPrice, true, Math.Abs(Convert.ToInt32(positionSize)), Math.Abs(Convert.ToInt32(positionSize)), Math.Abs(Convert.ToInt32(positionSize)),
                                                    incrementPrice, profitOffset, 100, structTrade.bstrSymbol, direction);
                            this.Invoke((MethodInvoker)delegate ()
                            {

                                myForm.Show();

                            });







                        }
                        else MessageBox.Show("Stop order size must be equal to position size");
                    }
                }
            }
        }

        private static void A()
        {
            
            
            
        }

       /* private void OnSTIPositionUpdateXML(ref string strPosition)
        {
            XmlSerializer xs = new XmlSerializer(typeof(SterlingLib.structSTIPositionUpdate));
            SterlingLib.structSTIPositionUpdate structPosition = (SterlingLib.structSTIPositionUpdate)xs.Deserialize(new StringReader(strPosition));
            int netPos = (structPosition.nSharesBot - structPosition.nSharesSld);
            MessageBox.Show(netPos.ToString());
        }*/

        private void button1_Click(object sender, EventArgs e)
        {
            autoProfitOn = !autoProfitOn;
            if (autoProfitOn)
            {
                //Display "Auto Profit On" text
                label1.Text = "ON";
                button1.Text = "Stop Auto Profit";
            }
            else
            {
                //Display "Auto Profit Off" text
                label1.Text = "OFF";
                button1.Text = "Start Auto Profit";
            }

        }

        private void AutoProfit_Load(object sender, EventArgs e)
        {

        }
    }
}
