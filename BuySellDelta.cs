 // 
// Copyright (C) 2017, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class BuySellDelta : Indicator
	{
		private double	buys;
		private double	sells;
		private int activeBar = 0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionBuySellVolume;
				Name					= "BuySellDelta";
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
				if(e.Price >= e.Ask)
				{
					buys += e.Volume;
				}
				else if (e.Price <= e.Bid)
				{
					sells += e.Volume;
				}
			}	
		}
		
		protected override void OnBarUpdate()
		{
			
		
			if (CurrentBar < activeBar || CurrentBar <= BarsRequiredToPlot)
				return;

			// New Bar has been formed
			// - Assign last volume counted to the prior bar
			// - Reset volume count for new bar
			// - replot final values and color 
			if (CurrentBar != activeBar)
			{
				BuySellDiff[1] = buys - sells;
				
				if (BuySellDiff[1] > 0)
				{
					PlotBrushes[0][1] = Brushes.Green;
				}
				else
				{
					PlotBrushes[0][1] = Brushes.Red;
				}	
				
				buys = 0;
				sells = 0;
				activeBar = CurrentBar;
				
			}
			
			// Dynamically update current bar
			
			BuySellDiff[0] =  buys - sells;	   //store intrabar diff		
			
			if (BuySellDiff[0] > 0)  // update intrabar
			{
				PlotBrushes[0][0] = Brushes.Green;
			}
			else
			{
				PlotBrushes[0][0] = Brushes.Red;
			}
		}

		#region Properties


		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuySellDiff
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BuySellDelta[] cacheBuySellDelta;
		public BuySellDelta BuySellDelta()
		{
			return BuySellDelta(Input);
		}

		public BuySellDelta BuySellDelta(ISeries<double> input)
		{
			if (cacheBuySellDelta != null)
				for (int idx = 0; idx < cacheBuySellDelta.Length; idx++)
					if (cacheBuySellDelta[idx] != null &&  cacheBuySellDelta[idx].EqualsInput(input))
						return cacheBuySellDelta[idx];
			return CacheIndicator<BuySellDelta>(new BuySellDelta(), input, ref cacheBuySellDelta);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BuySellDelta BuySellDelta()
		{
			return indicator.BuySellDelta(Input);
		}

		public Indicators.BuySellDelta BuySellDelta(ISeries<double> input )
		{
			return indicator.BuySellDelta(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BuySellDelta BuySellDelta()
		{
			return indicator.BuySellDelta(Input);
		}

		public Indicators.BuySellDelta BuySellDelta(ISeries<double> input )
		{
			return indicator.BuySellDelta(input);
		}
	}
}

#endregion
