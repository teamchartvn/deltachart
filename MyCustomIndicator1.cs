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
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class MyCustomIndicator1 : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "MyCustomIndicator1";
				Calculate									= Calculate.OnBarClose;
				//Calculate = Calculate.OnEachTick;
				//Calculate = Calculate.OnPriceChange;
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
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			 Print(" Ket qua \n"+State.ToString());
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MyCustomIndicator1[] cacheMyCustomIndicator1;
		public MyCustomIndicator1 MyCustomIndicator1()
		{
			return MyCustomIndicator1(Input);
		}

		public MyCustomIndicator1 MyCustomIndicator1(ISeries<double> input)
		{
			if (cacheMyCustomIndicator1 != null)
				for (int idx = 0; idx < cacheMyCustomIndicator1.Length; idx++)
					if (cacheMyCustomIndicator1[idx] != null &&  cacheMyCustomIndicator1[idx].EqualsInput(input))
						return cacheMyCustomIndicator1[idx];
			return CacheIndicator<MyCustomIndicator1>(new MyCustomIndicator1(), input, ref cacheMyCustomIndicator1);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MyCustomIndicator1 MyCustomIndicator1()
		{
			return indicator.MyCustomIndicator1(Input);
		}

		public Indicators.MyCustomIndicator1 MyCustomIndicator1(ISeries<double> input )
		{
			return indicator.MyCustomIndicator1(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MyCustomIndicator1 MyCustomIndicator1()
		{
			return indicator.MyCustomIndicator1(Input);
		}

		public Indicators.MyCustomIndicator1 MyCustomIndicator1(ISeries<double> input )
		{
			return indicator.MyCustomIndicator1(input);
		}
	}
}

#endregion
