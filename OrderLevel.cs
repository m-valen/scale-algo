using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SterlingAlgos
{
    class OrderLevel
    {
        public decimal startPrice;  //The entry price for this level. Not the executed price.
        
        public SterlingLib.ISTIOrder sittingOrder;
        public SterlingLib.structSTIOrderUpdate lastFilledOrder;
        public List<SterlingLib.ISTIOrder> completedOrders = new List<SterlingLib.ISTIOrder>();
        public int totalFills;
        public decimal PL = 0;
        public bool isRestore;
        public bool isStop; 

        public OrderLevel()
        {


        }
    }
}
