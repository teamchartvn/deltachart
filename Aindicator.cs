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
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.AddOns;

#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class Aindicator : Indicator
	{
		private NinjaTrader.NinjaScript.AddOns.MyAddOnTab TpoCall;
		private NinjaTrader.NinjaScript.AddOns.Test t;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "1.Test load Indicator";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				//DaysToLoad = 15;  ko dùng được cho indicator
				//TpoCall = new NinjaTrader.NinjaScript.AddOns.AddonTPO();
			//	TpoCall.
				t = new NinjaTrader.NinjaScript.AddOns.Test();
				TpoCall = new NinjaTrader.NinjaScript.AddOns.MyAddOnTab();
			}
			else if (State == State.Configure)
			{
				
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			//Print(t.test_1);
			Print(TpoCall.barinfo[0]);
			if (State == State.Historical)
                {
                  Instrument myInstrument = Instrument.GetInstrument("GC");
               
                  // Print("s tick size is " + myInstrument.MasterInstrument.TickSize.ToString());
					//Print(Instrument.FullName);
					//Print(Instrument.Expiry);
                }
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Aindicator[] cacheAindicator;
		public Aindicator Aindicator()
		{
			return Aindicator(Input);
		}

		public Aindicator Aindicator(ISeries<double> input)
		{
			if (cacheAindicator != null)
				for (int idx = 0; idx < cacheAindicator.Length; idx++)
					if (cacheAindicator[idx] != null &&  cacheAindicator[idx].EqualsInput(input))
						return cacheAindicator[idx];
			return CacheIndicator<Aindicator>(new Aindicator(), input, ref cacheAindicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Aindicator Aindicator()
		{
			return indicator.Aindicator(Input);
		}

		public Indicators.Aindicator Aindicator(ISeries<double> input )
		{
			return indicator.Aindicator(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Aindicator Aindicator()
		{
			return indicator.Aindicator(Input);
		}

		public Indicators.Aindicator Aindicator(ISeries<double> input )
		{
			return indicator.Aindicator(input);
		}
	}
}

#endregion
