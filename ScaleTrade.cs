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


namespace SterlingAlgos
{
    class ScaleTrade
    {
        public bool isRunning = false;
        public bool isStopped = false;
        public string symbol;
        public string direction;
        public int maxSize, startingSize, incrementSize;
        public decimal startingPrice, incrementPrice, rangeTop, rangeBottom, hardStop;
        public bool takeProfit, restoreSize, hasHardStop;
        public decimal profitOffset;
        public decimal range;
        public int numberOfPriceLevels;
        public List<decimal> priceLevels = new List<decimal>();
        public decimal risk; //Hard stop +1 cent
        public bool isRestart;
        public int restartInitialLevels;
        public int restartInitialPosition;
        public List<OrderLevel> orderLevels = new List<OrderLevel>();
        public SterlingLib.STIEvents stiEvents = new SterlingLib.STIEvents();




        public ScaleTrade()
        {

            stiEvents.SetOrderEventsAsStructs(true);
            //stiEvents.OnSTITradeUpdateXML += new SterlingLib._ISTIEventsEvents_OnSTITradeUpdateXMLEventHandler(stiEvents_OnSTITradeUpdateXML);
            stiEvents.OnSTIOrderUpdateXML += new SterlingLib._ISTIEventsEvents_OnSTIOrderUpdateXMLEventHandler(stiEvents_OnSTIOrderUpdateXML);

            //stiEvents.OnSTIOrderRejectXML += new SterlingLib._ISTIEventsEvents_OnSTIOrderRejectXMLEventHandler(stiEvents_OnSTIOrderRejectXML);

        }

       

        private void stiEvents_OnSTIOrderUpdateXML(ref string orderUpdateInfo)
        {
            

            XmlSerializer xs = new XmlSerializer(typeof(SterlingLib.structSTIOrderUpdate));
            SterlingLib.structSTIOrderUpdate structTrade = (SterlingLib.structSTIOrderUpdate)xs.Deserialize(new StringReader(orderUpdateInfo));
            //MessageBox.Show(structTrade.nOrderStatus.ToString());

            //Find associated order level, trigger new order

           


            SterlingAlgos.OrderLevel orderLevel = orderLevels.Where(i => i.sittingOrder.ClOrderID == structTrade.bstrClOrderId).FirstOrDefault();

            //MessageBox.Show(orderLevel.lastFilledOrder.fAvgExecPrice.ToString());

            Console.WriteLine("TU" + " : " + orderLevel.startPrice + " : " + orderLevel.sittingOrder.ClOrderID);

            if (orderLevel != null)
            {  //Check if orderLevel actually found

                if (orderLevel.isStop)
                {
                    if (structTrade.nOrderStatus == 5) { 
                        stop();
                    }
                }
                else if( structTrade.nOrderStatus == 5) //Check for complete fill
                {
                    bool isRestore = (direction == orderLevel.sittingOrder.Side) || (direction == "S" && orderLevel.sittingOrder.Side == "T");  //If true, size has been restored, so do TP order. If false, TP order is hit, calculate profit and do restore size order

                    if (direction == "B" && isRestore) // Restore Size has just occurred
                    {
                        orderLevel.totalFills += 1;
                        orderLevel.completedOrders.Add(orderLevel.sittingOrder);
                        orderLevel.lastFilledOrder = structTrade;

                        //Create take profit order
                        SterlingLib.STIOrder stiOrder = new SterlingLib.STIOrder();
                        stiOrder.Symbol = symbol;
                        stiOrder.Account = Globals.account;
                        stiOrder.Side = "S";
                        stiOrder.Quantity = incrementSize;
                        stiOrder.Tif = "D"; //day order
                        stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTILmt;
                        stiOrder.LmtPrice = Convert.ToDouble(orderLevel.startPrice + profitOffset);
                        stiOrder.Destination = Globals.desination;
                        stiOrder.ClOrderID = Guid.NewGuid().ToString();
                        if (!isStopped)
                        {
                            //Submit order
                            int ord = stiOrder.SubmitOrder();
                            if (ord != 0)
                            {
                                MessageBox.Show("Order Error: " + ord.ToString());
                            }
                            else //No error, new sitting order is the new "buy" order (restore size order)
                            {
                                orderLevel.sittingOrder = stiOrder;
                                orderLevel.isRestore = false;
                            }
                        }
                        else
                        {
                            orderLevel.sittingOrder = stiOrder;
                            orderLevel.isRestore = false;
                        }
                    }
                    if (direction == "B" && !isRestore) //Take Profit has just occurred
                    {
                        //orderLevel.PL += Convert.ToDecimal(structTrade.fExecPrice - orderLevel.lastFilledOrder.fExecPrice);
                        orderLevel.totalFills += 1;
                        orderLevel.completedOrders.Add(orderLevel.sittingOrder);
                        orderLevel.lastFilledOrder = structTrade;

                         

                        //Create restore size order
                        SterlingLib.STIOrder stiOrder = new SterlingLib.STIOrder();
                        stiOrder.Symbol = symbol;
                        stiOrder.Account = Globals.account;
                        stiOrder.Side = "B";
                        stiOrder.Quantity = incrementSize;
                        stiOrder.Tif = "D"; //day order
                        stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTILmt;
                        stiOrder.LmtPrice = Convert.ToDouble(orderLevel.startPrice);
                        stiOrder.Destination = Globals.desination;
                        stiOrder.ClOrderID = Guid.NewGuid().ToString();

                        if (!isStopped)
                        {
                            //Submit order
                            if (restoreSize) { 
                                int ord = stiOrder.SubmitOrder();
                                if (ord != 0)
                                {
                                    MessageBox.Show("Order Error: " + ord.ToString());
                                }
                                else //No error, new sitting order is the new "buy" order (restore size order)
                                {
                                    orderLevel.sittingOrder = stiOrder;
                                }
                            }
                            else
                            {
                                orderLevel.sittingOrder = stiOrder; //Order will sit there but not execute is no restore size option
                                orderLevel.isRestore = true;
                            }
                        }

                        else
                        {
                            orderLevel.sittingOrder = stiOrder;
                        }
                    }


                    if (direction == "S" && isRestore) // Restore Size has just occurred
                    {
                        orderLevel.totalFills += 1;
                        orderLevel.completedOrders.Add(orderLevel.sittingOrder);
                        orderLevel.lastFilledOrder = structTrade;

                        //Create take profit order
                        SterlingLib.STIOrder stiOrder = new SterlingLib.STIOrder();
                        stiOrder.Symbol = symbol;
                        stiOrder.Account = Globals.account;
                        stiOrder.Side = "B";
                        stiOrder.Quantity = incrementSize;
                        stiOrder.Tif = "D"; //day order
                        stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTILmt;
                        stiOrder.LmtPrice = Convert.ToDouble(orderLevel.startPrice - profitOffset);
                        stiOrder.Destination = Globals.desination;
                        stiOrder.ClOrderID = Guid.NewGuid().ToString();

                        if (!isStopped)
                        {
                            //Submit order
                            int ord = stiOrder.SubmitOrder();
                            if (ord != 0)
                            {
                                MessageBox.Show("Order Error: " + ord.ToString());
                            }
                            else //No error, new sitting order is the new "buy" order (restore size order)
                            {
                                orderLevel.sittingOrder = stiOrder;
                                orderLevel.isRestore = false;
                            }
                        }
                        else
                        {
                            orderLevel.sittingOrder = stiOrder;
                            orderLevel.isRestore = false;
                        }

                    }
                    if (direction == "S" && !isRestore) //Take Profit has just occurred
                    {
                        //orderLevel.PL += Convert.ToDecimal(orderLevel.lastFilledOrder.fExecPrice - structTrade.fExecPrice);
                        orderLevel.totalFills += 1;
                        orderLevel.completedOrders.Add(orderLevel.sittingOrder);
                        orderLevel.lastFilledOrder = structTrade;

                        //Create restore size order
                        SterlingLib.STIOrder stiOrder = new SterlingLib.STIOrder();
                        stiOrder.Symbol = symbol;
                        stiOrder.Account = Globals.account;
                        stiOrder.Side = "S";
                        stiOrder.Quantity = incrementSize;
                        stiOrder.Tif = "D"; //day order
                        stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTILmt;
                        stiOrder.LmtPrice = Convert.ToDouble(orderLevel.startPrice);
                        stiOrder.Destination = Globals.desination;
                        stiOrder.ClOrderID = Guid.NewGuid().ToString();


                        if (!isStopped)
                        {
                            if (restoreSize) { 
                            //Submit order
                                int ord = stiOrder.SubmitOrder();
                                if (ord != 0)
                                {
                                    MessageBox.Show("Order Error: " + ord.ToString());
                                }
                                else //No error, new sitting order is the new "buy" order (restore size order)
                                {
                                    orderLevel.sittingOrder = stiOrder;
                                }
                            }
                            else
                            {
                                orderLevel.sittingOrder = stiOrder; //Order will sit there but not execute is no restore size option
                                orderLevel.isRestore = true;
                            }
                        }
                        else
                        {
                            orderLevel.sittingOrder = stiOrder;
                        }
                    }
                }
            }


        }
        
        private void stiEvents_OnSTITradeUpdateXML(ref string tradeUpdateInfo)
        {
            /*
            XmlSerializer xs = new XmlSerializer(typeof(SterlingLib.structSTITradeUpdate));
            SterlingLib.structSTITradeUpdate structTrade = (SterlingLib.structSTITradeUpdate)xs.Deserialize(new StringReader(tradeUpdateInfo));
            //MessageBox.Show(structTrade.fExecPrice.ToString());

            //Find associated order level, trigger new order

            

            SterlingAlgos.OrderLevel orderLevel = orderLevels.Where(i => i.sittingOrder.ClOrderID == structTrade.bstrClOrderId).FirstOrDefault();

            Console.WriteLine("TU" + " : " + orderLevel.startPrice + " : " + orderLevel.sittingOrder.ClOrderID);

            if (orderLevel != null)
            {  //Check if orderLevel actually found

                if (orderLevel.isStop)
                {
                    stop();
                }
                else { 
                bool restoreSize = (direction == orderLevel.sittingOrder.Side) || (direction == "S" && orderLevel.sittingOrder.Side == "T");  //If true, size has been restored, so do TP order. If false, TP order is hit, calculate profit and do restore size order

                if (direction == "B" && restoreSize) // Restore Size has just occurred
                {
                    orderLevel.totalFills += 1;
                    orderLevel.completedOrders.Add(orderLevel.sittingOrder);
                    orderLevel.lastFilledOrder = structTrade;

                    //Create take profit order
                    SterlingLib.STIOrder stiOrder = new SterlingLib.STIOrder();
                    stiOrder.Symbol = symbol;
                    stiOrder.Account = Globals.account;
                    stiOrder.Side = "S";
                    stiOrder.Quantity = incrementSize;
                    stiOrder.Tif = "D"; //day order
                    stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTILmt;
                    stiOrder.LmtPrice = Convert.ToDouble(orderLevel.startPrice + incrementPrice);
                    stiOrder.Destination = Globals.desination;
                    stiOrder.ClOrderID = Guid.NewGuid().ToString();
                    if (!isStopped)
                    {
                        //Submit order
                        int ord = stiOrder.SubmitOrder();
                        if (ord != 0)
                        {
                            MessageBox.Show("Order Error: " + ord.ToString());
                        }
                        else //No error, new sitting order is the new "buy" order (restore size order)
                        {
                            orderLevel.sittingOrder = stiOrder;
                        }
                    }
                    else
                    {
                        orderLevel.sittingOrder = stiOrder;
                    }
                }
                if (direction == "B" && !restoreSize) //Take Profit has just occurred
                {
                    orderLevel.PL += Convert.ToDecimal(structTrade.fExecPrice - orderLevel.lastFilledOrder.fExecPrice);
                    orderLevel.totalFills += 1;
                    orderLevel.completedOrders.Add(orderLevel.sittingOrder);
                    orderLevel.lastFilledOrder = structTrade;

                    //Create restore size order
                    SterlingLib.STIOrder stiOrder = new SterlingLib.STIOrder();
                    stiOrder.Symbol = symbol;
                    stiOrder.Account = Globals.account;
                    stiOrder.Side = "B";
                    stiOrder.Quantity = incrementSize;
                    stiOrder.Tif = "D"; //day order
                    stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTILmt;
                    stiOrder.LmtPrice = Convert.ToDouble(orderLevel.startPrice);
                    stiOrder.Destination = Globals.desination;
                    stiOrder.ClOrderID = Guid.NewGuid().ToString();

                    if (!isStopped)
                    {
                        //Submit order
                        int ord = stiOrder.SubmitOrder();
                        if (ord != 0)
                        {
                            MessageBox.Show("Order Error: " + ord.ToString());
                        }
                        else //No error, new sitting order is the new "buy" order (restore size order)
                        {
                            orderLevel.sittingOrder = stiOrder;
                        }
                    }
                    else
                    {
                        orderLevel.sittingOrder = stiOrder;
                    }
                }


                if (direction == "S" && restoreSize) // Restore Size has just occurred
                {
                    orderLevel.totalFills += 1;
                    orderLevel.completedOrders.Add(orderLevel.sittingOrder);
                    orderLevel.lastFilledOrder = structTrade;

                    //Create take profit order
                    SterlingLib.STIOrder stiOrder = new SterlingLib.STIOrder();
                    stiOrder.Symbol = symbol;
                    stiOrder.Account = Globals.account;
                    stiOrder.Side = "B";
                    stiOrder.Quantity = incrementSize;
                    stiOrder.Tif = "D"; //day order
                    stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTILmt;
                    stiOrder.LmtPrice = Convert.ToDouble(orderLevel.startPrice - incrementPrice);
                    stiOrder.Destination = Globals.desination;
                    stiOrder.ClOrderID = Guid.NewGuid().ToString();

                    if (!isStopped)
                    {
                        //Submit order
                        int ord = stiOrder.SubmitOrder();
                        if (ord != 0)
                        {
                            MessageBox.Show("Order Error: " + ord.ToString());
                        }
                        else //No error, new sitting order is the new "buy" order (restore size order)
                        {
                            orderLevel.sittingOrder = stiOrder;
                        }
                    }
                    else
                    {
                        orderLevel.sittingOrder = stiOrder;
                    }

                }
                if (direction == "S" && !restoreSize) //Take Profit has just occurred
                {
                    orderLevel.PL += Convert.ToDecimal(orderLevel.lastFilledOrder.fExecPrice - structTrade.fExecPrice);
                    orderLevel.totalFills += 1;
                    orderLevel.completedOrders.Add(orderLevel.sittingOrder);
                    orderLevel.lastFilledOrder = structTrade;

                    //Create restore size order
                    SterlingLib.STIOrder stiOrder = new SterlingLib.STIOrder();
                    stiOrder.Symbol = symbol;
                    stiOrder.Account = Globals.account;
                    stiOrder.Side = "S";
                    stiOrder.Quantity = incrementSize;
                    stiOrder.Tif = "D"; //day order
                    stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTILmt;
                    stiOrder.LmtPrice = Convert.ToDouble(orderLevel.startPrice);
                    stiOrder.Destination = Globals.desination;
                    stiOrder.ClOrderID = Guid.NewGuid().ToString();


                    if (!isStopped)
                    {
                        //Submit order
                        int ord = stiOrder.SubmitOrder();
                        if (ord != 0)
                        {
                            MessageBox.Show("Order Error: " + ord.ToString());
                        }
                        else //No error, new sitting order is the new "buy" order (restore size order)
                        {
                            orderLevel.sittingOrder = stiOrder;
                        }
                    }
                    else
                    {
                        orderLevel.sittingOrder = stiOrder;
                    }
                }
                }
            }
            */
        }
        

        private void stiEvents_OnSTIOrderRejectXML(ref string orderRejectInfo)
        {
            XmlSerializer xs = new XmlSerializer(typeof(SterlingLib.structSTIOrderReject));
            SterlingLib.structSTIOrderReject structReject = (SterlingLib.structSTIOrderReject)xs.Deserialize(new StringReader(orderRejectInfo));
            //MessageBox.Show(structReject.nRejectReason.ToString());
        }

        public bool initiateTrade()
        {
            int restartLevels = 0; //Remaining restart levels

            if (isRestart)
            {
                if (isRunning) { 
                    stop();
                }
                orderLevels.Clear();
                restartLevels = restartInitialLevels;
                isStopped = false;
            }


            priceLevels.Clear();
            if (direction == "B")
            {
                SterlingLib.STIOrder stiOrder = new SterlingLib.STIOrder();
                //Stop Order First
                if (hasHardStop)
                {

                    stiOrder.Symbol = symbol;
                    stiOrder.Account = Globals.account;
                    stiOrder.Side = "S";
                    stiOrder.Quantity = maxSize;
                    stiOrder.Tif = "D"; //day order
                    stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTILmtStp;
                    stiOrder.StpPrice = Convert.ToDouble(hardStop);
                    stiOrder.Destination = "EDGX";
                    stiOrder.ClOrderID = Guid.NewGuid().ToString();

                    int ord = stiOrder.SubmitOrder();
                    if (ord != 0)
                    {
                        MessageBox.Show("Order Error: " + ord.ToString() + " - 1");
                    }
                    else
                    {
                        OrderLevel orderLevel = new OrderLevel();
                        orderLevel.isStop = true;
                        orderLevel.startPrice = hardStop;
                        orderLevel.sittingOrder = stiOrder; //Setting start size order as trigger order for each level
                        orderLevels.Add(orderLevel);
                    }
                }


                //Initial position

                //Create Order Levels for Starting Size and all levels above it (for buy side) 

                //for (decimal price = startingPrice; price < rangeTop; price += incrementPrice)
                
                for (decimal price = rangeTop - incrementPrice; price >= startingPrice; price -= incrementPrice)
                {

                    stiOrder = new SterlingLib.STIOrder();
                    stiOrder.Symbol = symbol;
                    stiOrder.Account = Globals.account;
                    stiOrder.Side = direction;
                    stiOrder.Quantity = incrementSize;
                    stiOrder.Tif = "D"; //day order
                    stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTILmt;
                    stiOrder.LmtPrice = Convert.ToDouble(startingPrice);
                    stiOrder.Destination = Globals.desination;
                    stiOrder.ClOrderID = Guid.NewGuid().ToString();

                    if (restartLevels > 0) //If this is a restart scale trade and one of the initial position levels, make it a take profit instead of a restore size
                    {
                        stiOrder.Side = "S";
                        stiOrder.LmtPrice = Convert.ToDouble(price + profitOffset);
                        restartLevels -= 1;
                    }


                    //Submit order
                    int ord = stiOrder.SubmitOrder();
                    if (ord != 0)
                    {
                        MessageBox.Show("Order Error: " + ord.ToString() + " - 2");
                    }
                    else { 
                        OrderLevel orderLevel = new OrderLevel();
                        orderLevel.startPrice = price;
                        orderLevel.sittingOrder = stiOrder; //Setting start size order as trigger order for each level
                        
                        if (stiOrder.Side == "B")
                        {
                            orderLevel.isRestore = true;
                        }

                        orderLevels.Add(orderLevel);
                    }
                }

            //Price Levels

            for (decimal price = rangeBottom; price < rangeTop; price += incrementPrice )
            {
                priceLevels.Add(price);
            }

            priceLevels.Reverse();


                //allOrders.Add(stiOrder);
     

                //Stopwatch sw = new Stopwatch();
                //sw.Start();

                
            //All prices below (for buy side) starting price
            foreach (decimal price in priceLevels)
            {
                SterlingLib.STIOrder order = new SterlingLib.STIOrder();
                if (price < startingPrice)
                { 
                    order.Symbol = symbol;
                    order.Account = Globals.account;
                    order.Side = direction;
                    order.Quantity = incrementSize;
                    order.Tif = "D"; //day order
                    order.PriceType = SterlingLib.STIPriceTypes.ptSTILmt;
                    order.LmtPrice = Convert.ToDouble(price);
                    order.Destination = Globals.desination;
                    order.ClOrderID = Guid.NewGuid().ToString();
                    
                    if (restartLevels > 0) //If this is a restart scale trade and one of the initial position levels, make it a take profit instead of a restore size
                    {
                        order.Side = "S";
                        order.LmtPrice = Convert.ToDouble(price + profitOffset);
                        restartLevels--;
                    }
                    

                    int orderSuccess = order.SubmitOrder();

                    if (orderSuccess != 0)
                    {
                        Console.WriteLine("Order Error :" + order.ToString() + " - 3");
                    }

                    else  //Create order level then append to orderLevels list
                    {
                        OrderLevel orderLevel = new OrderLevel();
                        orderLevel.startPrice = Convert.ToDecimal(price);
                        orderLevel.sittingOrder = order;
                        orderLevels.Add(orderLevel);
                    }

                }

            }

            orderLevels.Sort((x, y) => x.startPrice.CompareTo(y.startPrice));
            orderLevels.Reverse();

            //sw.Stop();
            //Console.WriteLine(sw.Elapsed);

            foreach (OrderLevel level in orderLevels) 
            {
                Console.WriteLine(level.startPrice);
            }
        }

        else if (direction == "S")
        {
                //Stop order first
                SterlingLib.STIOrder stiOrder = new SterlingLib.STIOrder();
                if (hasHardStop)
                {

                    stiOrder.Symbol = symbol;
                    stiOrder.Account = Globals.account;
                    stiOrder.Side = "B";
                    stiOrder.Quantity = maxSize;
                    stiOrder.Tif = "D"; //day order
                    stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTILmtStp;
                    stiOrder.StpPrice = Convert.ToDouble(hardStop);
                    stiOrder.Destination = "EDGX";
                    stiOrder.ClOrderID = Guid.NewGuid().ToString();


                    int ord = stiOrder.SubmitOrder();
                    if (ord != 0)
                    {
                        MessageBox.Show("Order Error: " + ord.ToString() + " - 4");
                    }
                    else
                    {
                        OrderLevel orderLevel = new OrderLevel();
                        orderLevel.isStop = true;
                        orderLevel.startPrice = hardStop;
                        orderLevel.sittingOrder = stiOrder; //Setting start size order as trigger order for each level
                        orderLevels.Add(orderLevel);
                    }
                }


                //Create Order Levels for Starting Size and all levels above it (for sell side) 
                //Initial Position

                for (decimal price = rangeBottom + incrementPrice; price <= startingPrice; price += incrementPrice)
                {
                    stiOrder = new SterlingLib.STIOrder();

                    stiOrder.Symbol = symbol;
                    stiOrder.Account = Globals.account;
                    stiOrder.Side = "T";
                    stiOrder.Quantity = incrementSize;
                    stiOrder.Tif = "D"; //day order
                    stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTILmt;
                    stiOrder.LmtPrice = Convert.ToDouble(startingPrice);
                    stiOrder.Destination = Globals.desination;
                    stiOrder.ClOrderID = Guid.NewGuid().ToString();

                    if (restartLevels > 0) //If this is a restart scale trade and one of the initial position levels, make it a take profit instead of a restore size
                    {
                        stiOrder.Side = "B";
                        stiOrder.LmtPrice = Convert.ToDouble(price - profitOffset);
                        restartLevels--;
                    }

                    //Submit order
                    int ord = stiOrder.SubmitOrder();
                    if (ord != 0)
                    {
                        MessageBox.Show("Order Error: " + ord.ToString() + " - 5");
                    }
                    else
                    {
                        OrderLevel orderLevel = new OrderLevel();
                        orderLevel.startPrice = price;
                        orderLevel.sittingOrder = stiOrder; //Setting start size order as trigger order for each level

                        if (stiOrder.Side == "S")
                        {
                            orderLevel.isRestore = true;
                        }

                        orderLevels.Add(orderLevel);
                    }
                }


                //Price Levels

                for (decimal price = rangeBottom + incrementPrice; price <= rangeTop; price += incrementPrice)
                {
                    priceLevels.Add(price);
                }

             
                //allOrders.Add(stiOrder);

 
                //Stopwatch sw = new Stopwatch();
                //sw.Start();


                //All prices below (for buy side) starting price
                foreach (decimal price in priceLevels)
                {
                    SterlingLib.STIOrder order = new SterlingLib.STIOrder();
                    if (price > startingPrice)
                    {
                        order.Symbol = symbol;
                        order.Account = Globals.account;
                        order.Side = "T";
                        order.Quantity = incrementSize;
                        order.Tif = "D"; //day order
                        order.PriceType = SterlingLib.STIPriceTypes.ptSTILmt;
                        order.LmtPrice = Convert.ToDouble(price);
                        order.Destination = Globals.desination;
                        order.ClOrderID = Guid.NewGuid().ToString();


                        if (restartLevels > 0) //If this is a restart scale trade and one of the initial position levels, make it a take profit instead of a restore size
                        {
                            order.Side = "B";
                            order.LmtPrice = Convert.ToDouble(price - profitOffset);
                            restartLevels--;
                        }

                        int orderSuccess = order.SubmitOrder();

                        if (orderSuccess != 0)
                        {
                            Console.WriteLine("Order Error :" + order.ToString() + " - 6");
                        }

                        else  //Create order level then append to orderLevels list
                        {
                            OrderLevel orderLevel = new OrderLevel();
                            orderLevel.startPrice = Convert.ToDecimal(price);
                            orderLevel.sittingOrder = order;
                            orderLevels.Add(orderLevel);
                        }

                    }

                }

                orderLevels.Sort((x, y) => x.startPrice.CompareTo(y.startPrice));
                


                //sw.Stop();
                //Console.WriteLine(sw.Elapsed);

                foreach (OrderLevel level in orderLevels)
                {
                    Console.WriteLine(level.startPrice);
                }
            }

            foreach (OrderLevel level in orderLevels)
            {
                Console.WriteLine("IO" + " : " + level.startPrice + " : " + level.sittingOrder.ClOrderID + " : " + level.sittingOrder.LmtPrice);
            }
            isRunning = true;
            return true;
        }

        public void stop()
        {
            if (isRunning && orderLevels.Count > 0) //Make sure there are orders 
            {
                isStopped = true; 

                SterlingLib.ISTIOrderMaint orderMaint = new SterlingLib.STIOrderMaint();

                foreach (OrderLevel level in orderLevels) //Cancel all sitting orders
                {
                    orderMaint.CancelOrder(level.sittingOrder.Account, 0, level.sittingOrder.ClOrderID, Guid.NewGuid().ToString());
                }
            } 
            else
            {
                MessageBox.Show("No scale trade running.");
            }
        }
        public void start()
        {
            if (isRunning && orderLevels.Count > 0)
            {
                isStopped = false;

                foreach (OrderLevel level in orderLevels)
                {
                    //Refresh Client Order ID
                    level.sittingOrder.ClOrderID = Guid.NewGuid().ToString();
                    if (restoreSize || (!restoreSize && !level.isRestore)) {  
                        //Submit order
                        int ord = level.sittingOrder.SubmitOrder();
                        if (ord != 0)
                        {
                            MessageBox.Show("Order Error: " + ord.ToString());
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("No scale trade currently running");
            }
        }
    }
}
