#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
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

//This namespace holds Add ons in this folder and is required. Do not change it.
/// <summary>
/// http://ninjatrader.com/support/helpGuides/nt8/en-us/marketdata.htm
/// </summary>
namespace NinjaTrader.NinjaScript.AddOns
{
    public class Test11394 : NinjaTrader.NinjaScript.AddOnBase
    {
        private Dictionary<string, bool> ExistingTicks;
        private MarketData marketData;

        private static Instrument EMiniSnP
        {
            get
            {
                if (null == _EMiniSnP)
                {
                    foreach(InstrumentList l in NinjaTrader.Cbi.InstrumentList.All)
                    {
                        foreach(Instrument i in l.Instruments)
                        {
                            if (i.FullName == "GC 02-24")
                            {
                                return (_EMiniSnP = i);
                            }
                        }
                    }
                }
                return _EMiniSnP;
            }
        }
        private static Instrument _EMiniSnP;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Add on here.";
                Name = "Test11394";
                ExistingTicks = new Dictionary<string, bool>();
            }
            else if (State == State.Terminated)
            {
                if (marketData != null)
                {
                    marketData.Update -= OnMarketData;
                }
            }
        }

        protected override void OnWindowCreated(Window window)
        {
            if (null == marketData)
            {
                marketData = new MarketData(EMiniSnP);
                marketData.Update += OnMarketData;
            }
        }

        private void OnMarketData(object sender, MarketDataEventArgs e)
        {
            string key = String.Format (
                    "(price, volume, time, type) = ({0:C}, {1}, {2}, {3})"
                    , e.Price, e.Volume
                    , e.Time.ToString("dd MMM yyyy HH:mm:ss.ffff")
                    , e.MarketDataType
                );
            if (ExistingTicks.ContainsKey(key))
            {
                NinjaTrader.Code.Output.Process(key, PrintTo.OutputTab1);
                return;
            }
            ExistingTicks[key] = true;
        }
    }
}
