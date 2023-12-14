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
	public class AmolRevRatio3 : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "1-Test";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
			}
			else if (State == State.Configure)
			{
			//	AddVolumetric("",BarsPeriodType.Tick, 500, VolumetricDeltaType.BidAsk, 1);
			}
		}
		protected override void OnBarUpdate()
		{
			if(BarsInProgress == 0)
			{
				
		        	
				
							/*	
				if(Close[0] < Open[0]){		//Down Red Bar	
				//	Draw.Text(this, "Bardltr"+ CurrentBar, barsType.Volumes[CurrentBar].BarDelta.ToString(),	0, Low[0] - (2.5 * TickSize + 0.25), Brushes.White);
					Draw.Text(this, "Bardltchgr"+ CurrentBar,Bars.GetVolume(CurrentBar).ToString(),	0, Low[0] - (1 * TickSize + 0.25), Brushes.White);
				}
				if(Close[0] > Open[0]){     //Up Green Bar	
				//	Draw.Text(this, "Bardltb"+ CurrentBar, barsType.Volumes[CurrentBar].BarDelta.ToString(),	0, High[0] + (2.5 * TickSize + 0.25), Brushes.White);
					Draw.Text(this, "Bardltchgb"+ CurrentBar,Bars.GetVolume(CurrentBar).ToString(),	0, High[0] + (1 * TickSize + 0.25), Brushes.White);
				}
				if(Close[0] == Open[0]){		//Doji 		
				//	Draw.Text(this, "Bardltd"+ CurrentBar, barsType.Volumes[CurrentBar].BarDelta.ToString(),	0, High[0] + (2.5 * TickSize + 0.25), Brushes.White);
				Draw.Text(this, "Bardltchgb"+ CurrentBar,Bars.GetVolume(CurrentBar).ToString(),	0, High[0] + (1 * TickSize + 0.25), Brushes.White);
				}							
						
							if(Close[0] > Open[0] && Low[0] <= Low[1]){   // nến tăng có giá thấp nhất lớn hơn or bằng
                                if (barsType.Volumes[CurrentBar].BarDelta >0 ){
                                  Draw.ArrowUp(this, "Mua", true, 0, Low[0] - TickSize, Brushes.Red);
								}
							}
							if(Close[0] < Open[0] && High[0] >= High[1]){
								if (barsType.Volumes[CurrentBar].BarDelta <0 ){
                                     Draw.ArrowDown(this, "Bán", true, 0, Low[0] - TickSize, Brushes.Red);
								}
							}
				*/
			}
			
		}
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AmolRevRatio3[] cacheAmolRevRatio3;
		public AmolRevRatio3 AmolRevRatio3()
		{
			return AmolRevRatio3(Input);
		}

		public AmolRevRatio3 AmolRevRatio3(ISeries<double> input)
		{
			if (cacheAmolRevRatio3 != null)
				for (int idx = 0; idx < cacheAmolRevRatio3.Length; idx++)
					if (cacheAmolRevRatio3[idx] != null &&  cacheAmolRevRatio3[idx].EqualsInput(input))
						return cacheAmolRevRatio3[idx];
			return CacheIndicator<AmolRevRatio3>(new AmolRevRatio3(), input, ref cacheAmolRevRatio3);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AmolRevRatio3 AmolRevRatio3()
		{
			return indicator.AmolRevRatio3(Input);
		}

		public Indicators.AmolRevRatio3 AmolRevRatio3(ISeries<double> input )
		{
			return indicator.AmolRevRatio3(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AmolRevRatio3 AmolRevRatio3()
		{
			return indicator.AmolRevRatio3(Input);
		}

		public Indicators.AmolRevRatio3 AmolRevRatio3(ISeries<double> input )
		{
			return indicator.AmolRevRatio3(input);
		}
	}
}

#endregion
