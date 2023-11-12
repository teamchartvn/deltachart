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
	public class GexLine : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "GexLine";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
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



		protected override void OnMarketDepth(MarketDepthEventArgs marketDepthUpdate)
		{
			
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			
			Draw.HorizontalLine(this,"tag1",false,1950.0,Brushes.Cyan,DashStyleHelper.Dash,200,);
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GexLine[] cacheGexLine;
		public GexLine GexLine()
		{
			return GexLine(Input);
		}

		public GexLine GexLine(ISeries<double> input)
		{
			if (cacheGexLine != null)
				for (int idx = 0; idx < cacheGexLine.Length; idx++)
					if (cacheGexLine[idx] != null &&  cacheGexLine[idx].EqualsInput(input))
						return cacheGexLine[idx];
			return CacheIndicator<GexLine>(new GexLine(), input, ref cacheGexLine);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GexLine GexLine()
		{
			return indicator.GexLine(Input);
		}

		public Indicators.GexLine GexLine(ISeries<double> input )
		{
			return indicator.GexLine(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GexLine GexLine()
		{
			return indicator.GexLine(Input);
		}

		public Indicators.GexLine GexLine(ISeries<double> input )
		{
			return indicator.GexLine(input);
		}
	}
}

#endregion
