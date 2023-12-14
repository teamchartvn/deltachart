#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Gui.Tools;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns
{
	
public class MyAddOnTab :  NinjaTrader.NinjaScript.AddOnBase
{
private int daysBack = 5;
private bool barsRequestSubscribed = false;
private BarsRequest barsRequest; 
public double [] barinfo = new double[2];

	public MyAddOnTab()
{
  // create a new bars request.  This will determine the insturment and range for the bars to be requested
  barsRequest = new BarsRequest(Cbi.Instrument.GetInstrument("GC"), DateTime.Now.AddDays(-1), DateTime.Now);
 
  // Parametrize your request.
  barsRequest.BarsPeriod = new BarsPeriod { BarsPeriodType = BarsPeriodType.Tick, Value = 1 };
  barsRequest.TradingHours = TradingHours.Get("Default 24 x 7");
 
  // Attach event handler for real-time events if you want to process real-time data
  barsRequest.Update     += OnBarUpdate;
 
  // Request the bars
  barsRequest.Request(new Action<BarsRequest, ErrorCode, string>((bars, errorCode, errorMessage) =>
  {
    if (errorCode != ErrorCode.NoError)
    {
      // Handle any errors in requesting bars here
      NinjaTrader.Code.Output.Process(string.Format("Error on requesting bars: {0}, {1}",
                                      errorCode, errorMessage), PrintTo.OutputTab1);
      return;
    }
 
    // Output the bars we requested. Note: The last returned bar may be a currently in-progress bar
    for (int i = 0; i < bars.Bars.Count; i++)
    {
      // Output the bars
      
                                      
    }
 
    // If requesting real-time bars, but there are currently no connections
    lock (Connection.Connections)
      if (Connection.Connections.FirstOrDefault() == null)
        NinjaTrader.Code.Output.Process("Real-Time Bars: Not connected.", PrintTo.OutputTab1);
  }));
}
 

private void OnBarUpdate(object sender, BarsUpdateEventArgs e)
{

  for (int i = e.MinIndex; i <= e.MaxIndex; i++)
  {
    // Processing every single tick
                                       // e.BarsSeries.GetOpen(i);
                                 
                             barinfo[0]  =       e.BarsSeries.GetClose(i);
                                   barinfo[1]  =           e.BarsSeries.GetVolume(i);
  }
}
 

public  void Cleanup()
{
  // Make sure to unsubscribe to the bars request subscription
  if (barsRequest != null)
    {
       barsRequest.Update -= OnBarUpdate;
       barsRequest.Dispose();
      barsRequest = null;
     }
}
 

}
	public class Test{
		 public int test_1 ;
		
		public Test(){
			test_1 =5;
		}
		
	}
	
}
