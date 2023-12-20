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
using NinjaTrader.NinjaScript.AddOns;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class AAdelta : Indicator
	{
		private double	buys;
		private double	sells;
		private int activeBar = 0;
        
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionBuySellVolume;
				Name					= "1-delta";
				BarsRequiredToPlot		= 1;
				Calculate				= Calculate.OnBarClose;
				DrawOnPricePanel		= false;
				IsOverlay				= false;
				DisplayInDataBox		= true;

				// Plots will overlap each other no matter which one of these comes first
				// in NT8, we would add the Sells first in code and then Buys, and the "Sells" was always in front of the buys.
				AddPlot(new Stroke(Brushes.DarkCyan,	2), PlotStyle.Bar, "BuySellDiff");
				AddLine(Brushes.Gray, 0, "zeroLine");
				
				//AddonTpo = new NinjaTrader.NinjaScript.AddOns.TPO();
				
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

      
		
		protected override void OnBarUpdate()
		{
			
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
		private AAdelta[] cacheAAdelta;
		public AAdelta AAdelta()
		{
			return AAdelta(Input);
		}

		public AAdelta AAdelta(ISeries<double> input)
		{
			if (cacheAAdelta != null)
				for (int idx = 0; idx < cacheAAdelta.Length; idx++)
					if (cacheAAdelta[idx] != null &&  cacheAAdelta[idx].EqualsInput(input))
						return cacheAAdelta[idx];
			return CacheIndicator<AAdelta>(new AAdelta(), input, ref cacheAAdelta);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AAdelta AAdelta()
		{
			return indicator.AAdelta(Input);
		}

		public Indicators.AAdelta AAdelta(ISeries<double> input )
		{
			return indicator.AAdelta(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AAdelta AAdelta()
		{
			return indicator.AAdelta(Input);
		}

		public Indicators.AAdelta AAdelta(ISeries<double> input )
		{
			return indicator.AAdelta(input);
		}
	}
}

#endregion
