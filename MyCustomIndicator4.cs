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
	public class TestIndi : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Delta Tumaround";
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
				AddVolumetric("",BarsPeriodType.Tick, 500, VolumetricDeltaType.BidAsk, 1);
			}
		}
		protected override void OnBarUpdate()
		{
			if(BarsInProgress == 0)
			{
				/*	
		        NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType barsType = BarsArray[1].BarsType as NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType;				
				if(barsType == null)
					return;		//		
						
				if(Close[0] < Open[0]){		//Down Red Bar	
					Draw.Text(this, "Bardltr"+ CurrentBar,(CurrentBar-2).ToString(),	0, Low[0] - (2.5 * TickSize + 0.25), Brushes.White);
//					Draw.Text(this, "Bardltchgr"+ CurrentBar, (barsType.Volumes[CurrentBar].BarDelta - barsType.Volumes[CurrentBar -1].BarDelta).ToString(),	0, Low[0] - (2.5 * TickSize + 0.25), Brushes.White);
				}
				if(Close[0] > Open[0]){     //Up Green Bar	
					Draw.Text(this, "Bardltb"+ CurrentBar, (CurrentBar-2).ToString(),	0, High[0] + (2.5 * TickSize + 0.25), Brushes.White);
//					Draw.Text(this, "Bardltchgb"+ CurrentBar, (barsType.Volumes[CurrentBar].BarDelta - barsType.Volumes[CurrentBar -1].BarDelta).ToString(),	0, High[0] + (2.5 * TickSize + 0.25), Brushes.White);
				}
				if(Close[0] == Open[0]){		//Doji 		
					Draw.Text(this, "Bardltd"+ CurrentBar, (CurrentBar-2).ToString(),	0, High[0] + (2.5 * TickSize + 0.25), Brushes.White);
//					Draw.Text(this, "Bardltchgb"+ CurrentBar, (barsType.Volumes[CurrentBar].BarDelta - barsType.Volumes[CurrentBar -1].BarDelta).ToString(),	0, High[0] + (2.5 * TickSize + 0.25), Brushes.White);
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
		private TestIndi[] cacheTestIndi;
		public TestIndi TestIndi()
		{
			return TestIndi(Input);
		}

		public TestIndi TestIndi(ISeries<double> input)
		{
			if (cacheTestIndi != null)
				for (int idx = 0; idx < cacheTestIndi.Length; idx++)
					if (cacheTestIndi[idx] != null &&  cacheTestIndi[idx].EqualsInput(input))
						return cacheTestIndi[idx];
			return CacheIndicator<TestIndi>(new TestIndi(), input, ref cacheTestIndi);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TestIndi TestIndi()
		{
			return indicator.TestIndi(Input);
		}

		public Indicators.TestIndi TestIndi(ISeries<double> input )
		{
			return indicator.TestIndi(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TestIndi TestIndi()
		{
			return indicator.TestIndi(Input);
		}

		public Indicators.TestIndi TestIndi(ISeries<double> input )
		{
			return indicator.TestIndi(input);
		}
	}
}

#endregion
