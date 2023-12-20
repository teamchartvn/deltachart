//
// Copyright (C) 2022, NinjaTrader LLC <www.ninjatrader.com>.
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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The MACD (Moving Average Convergence/Divergence) is a trend following momentum indicator
	/// that shows the relationship between two moving averages of prices.
	/// </summary>
	public class MACD_MOD : Indicator
	{
		private	Series<double>		fastEma;
		private	Series<double>		slowEma;
		private double				constant1;
		private double				constant2;
		private double				constant3;
		private double				constant4;
		private double				constant5;
		private double				constant6;
        

		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= Custom.Resource.NinjaScriptIndicatorDescriptionMACD;
				Name						= "MCAD MOD";
				Fast						= 12;
				IsSuspendedWhileInactive	= true;
				Slow						= 26;
				Smooth						= 9;

				AddPlot(Brushes.DarkCyan,									Custom.Resource.NinjaScriptIndicatorNameMACD);
				AddPlot(Brushes.Crimson,									Custom.Resource.NinjaScriptIndicatorAvg);
				AddPlot(new Stroke(Brushes.DodgerBlue, 2),	PlotStyle.Bar,	Custom.Resource.NinjaScriptIndicatorDiff);
				AddLine(Brushes.DarkGray,					0,				Custom.Resource.NinjaScriptIndicatorZeroLine);
			}
			else if (State == State.Configure)
			{
				constant1	= 2.0 / (1 + Fast);
				constant2	= 1 - (2.0 / (1 + Fast));
				constant3	= 2.0 / (1 + Slow);
				constant4	= 1 - (2.0 / (1 + Slow));
				constant5	= 2.0 / (1 + Smooth);
				constant6	= 1 - (2.0 / (1 + Smooth));
			}
			else if (State == State.DataLoaded)
			{
				fastEma = new Series<double>(this);
				slowEma = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			double input0	= Input[0];

			if (CurrentBar == 0)
			{
				fastEma[0]		= input0;
				slowEma[0]		= input0;
				Value[0]		= 0;
				Avg[0]			= 0;
				Diff[0]			= 0;
			}
			else
			{
				double fastEma0	= constant1 * input0 + constant2 * fastEma[1];
				double slowEma0	= constant3 * input0 + constant4 * slowEma[1];
				double macd		= fastEma0 - slowEma0;
				double macdAvg	= constant5 * macd + constant6 * Avg[1];

				fastEma[0]		= fastEma0;
				slowEma[0]		= slowEma0;
				Value[0]		= macd;
				Avg[0]			= macdAvg;
				Diff[0]			= macd - macdAvg;
				if(Diff[0]*Diff[1] <= 0){
					// thông báo ở đây
					Alert("MCAD25", Priority.High, "MCAD 25", NinjaTrader.Core.Globals.InstallDir+@"\sounds\MCAD25.wav.wav", 10, Brushes.Black, Brushes.Yellow);
				}
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
          [Display(Name="MyFilePath", Order=1, GroupName="Parameters")]
          [PropertyEditor("NinjaTrader.Gui.Tools.FilePathPick er", Filter="Sound Files (*.wav)|*.wav", Title="Sound")]
          public string MyFilePath_amthanh
           { get; set; }
	
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Avg
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Default
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Diff
		{
			get { return Values[2]; }
		}

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Fast
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow", GroupName = "NinjaScriptParameters", Order = 1)]
		public int Slow
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Smooth", GroupName = "NinjaScriptParameters", Order = 2)]
		public int Smooth
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MACD_MOD[] cacheMACD_MOD;
		public MACD_MOD MACD_MOD(string myFilePath_amthanh, int fast, int slow, int smooth)
		{
			return MACD_MOD(Input, myFilePath_amthanh, fast, slow, smooth);
		}

		public MACD_MOD MACD_MOD(ISeries<double> input, string myFilePath_amthanh, int fast, int slow, int smooth)
		{
			if (cacheMACD_MOD != null)
				for (int idx = 0; idx < cacheMACD_MOD.Length; idx++)
					if (cacheMACD_MOD[idx] != null && cacheMACD_MOD[idx].MyFilePath_amthanh == myFilePath_amthanh && cacheMACD_MOD[idx].Fast == fast && cacheMACD_MOD[idx].Slow == slow && cacheMACD_MOD[idx].Smooth == smooth && cacheMACD_MOD[idx].EqualsInput(input))
						return cacheMACD_MOD[idx];
			return CacheIndicator<MACD_MOD>(new MACD_MOD(){ MyFilePath_amthanh = myFilePath_amthanh, Fast = fast, Slow = slow, Smooth = smooth }, input, ref cacheMACD_MOD);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MACD_MOD MACD_MOD(string myFilePath_amthanh, int fast, int slow, int smooth)
		{
			return indicator.MACD_MOD(Input, myFilePath_amthanh, fast, slow, smooth);
		}

		public Indicators.MACD_MOD MACD_MOD(ISeries<double> input , string myFilePath_amthanh, int fast, int slow, int smooth)
		{
			return indicator.MACD_MOD(input, myFilePath_amthanh, fast, slow, smooth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MACD_MOD MACD_MOD(string myFilePath_amthanh, int fast, int slow, int smooth)
		{
			return indicator.MACD_MOD(Input, myFilePath_amthanh, fast, slow, smooth);
		}

		public Indicators.MACD_MOD MACD_MOD(ISeries<double> input , string myFilePath_amthanh, int fast, int slow, int smooth)
		{
			return indicator.MACD_MOD(input, myFilePath_amthanh, fast, slow, smooth);
		}
	}
}

#endregion
