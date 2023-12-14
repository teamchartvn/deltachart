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
	public class Wilder : Indicator
	{
		private Series<double> firstAvg;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Wilder's Average";
				Name										= "Wilder";
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
				
				Period										= 1;
				
				AddPlot(Brushes.Purple, "WAvg");
			}
			else if (State == State.DataLoaded)
			{				
				firstAvg = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar == 0 ) 
			{
				firstAvg[0] = 0;
				return;
			}
			
			firstAvg[0] = SMA(Input, Period)[0];
			WAvg[0] = (firstAvg[1] * (Period -1) + Input[0]) / Period;
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Description="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> WAvg
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
		private Wilder[] cacheWilder;
		public Wilder Wilder(int period)
		{
			return Wilder(Input, period);
		}

		public Wilder Wilder(ISeries<double> input, int period)
		{
			if (cacheWilder != null)
				for (int idx = 0; idx < cacheWilder.Length; idx++)
					if (cacheWilder[idx] != null && cacheWilder[idx].Period == period && cacheWilder[idx].EqualsInput(input))
						return cacheWilder[idx];
			return CacheIndicator<Wilder>(new Wilder(){ Period = period }, input, ref cacheWilder);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Wilder Wilder(int period)
		{
			return indicator.Wilder(Input, period);
		}

		public Indicators.Wilder Wilder(ISeries<double> input , int period)
		{
			return indicator.Wilder(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Wilder Wilder(int period)
		{
			return indicator.Wilder(Input, period);
		}

		public Indicators.Wilder Wilder(ISeries<double> input , int period)
		{
			return indicator.Wilder(input, period);
		}
	}
}

#endregion
