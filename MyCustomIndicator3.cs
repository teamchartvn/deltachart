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
	public class TestPrice : Indicator
	{
		private double	buys;
		private double	sells;
		private int activeBar = 0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionBuySellVolume;
				Name					= "1test price";
				BarsRequiredToPlot		= 1;
				Calculate				= Calculate.OnEachTick;
				DrawOnPricePanel		= false;
				IsOverlay				= false;
				DisplayInDataBox		= true;

				// Plots will overlap each other no matter which one of these comes first
				// in NT8, we would add the Sells first in code and then Buys, and the "Sells" was always in front of the buys.
				AddPlot(new Stroke(Brushes.DarkCyan,	2), PlotStyle.Bar, "BuySellDiff");
				AddLine(Brushes.Gray, 0, "zeroLine");
			}
			else if (State == State.Historical)
			{
				if (Calculate != Calculate.OnEachTick)
				{
					Draw.TextFixed(this, "NinjaScriptInfo", string.Format(NinjaTrader.Custom.Resource.NinjaScriptOnBarCloseError, Name), TextPosition.BottomRight);
					Log(string.Format(NinjaTrader.Custom.Resource.NinjaScriptOnBarCloseError, Name), LogLevel.Error);
				}
			}
		}

		protected override void OnMarketData(MarketDataEventArgs e)
		{			
			
			if(e.MarketDataType == MarketDataType.Last)
			{	
				/*
				if(e.Price >= e.Ask)
				{
					buys += e.Volume;
				}
				else if (e.Price <= e.Bid)
				{
					sells += e.Volume;
				}
				*/
				Print(e.Ask.ToString() + "--" + e.Bid.ToString()+"-- " + e.Price.ToString() + " ="+ e.Volume.ToString());
			}	
			
			
			
		}
		
	
}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TestPrice[] cacheTestPrice;
		public TestPrice TestPrice()
		{
			return TestPrice(Input);
		}

		public TestPrice TestPrice(ISeries<double> input)
		{
			if (cacheTestPrice != null)
				for (int idx = 0; idx < cacheTestPrice.Length; idx++)
					if (cacheTestPrice[idx] != null &&  cacheTestPrice[idx].EqualsInput(input))
						return cacheTestPrice[idx];
			return CacheIndicator<TestPrice>(new TestPrice(), input, ref cacheTestPrice);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TestPrice TestPrice()
		{
			return indicator.TestPrice(Input);
		}

		public Indicators.TestPrice TestPrice(ISeries<double> input )
		{
			return indicator.TestPrice(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TestPrice TestPrice()
		{
			return indicator.TestPrice(Input);
		}

		public Indicators.TestPrice TestPrice(ISeries<double> input )
		{
			return indicator.TestPrice(input);
		}
	}
}

#endregion
