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
	public class Acanhbaokietvol : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "2-Cảnh báo kiệt vol";
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
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			Print(CurrentBar);
			if(BarsInProgress == 0 && CurrentBar > 5)
			{
				
				if(Bars.GetVolume(CurrentBar) < 0.4*Bars.GetVolume(CurrentBar-1) ){
					Print("Thap hon bat thuong dieu kien 1");
					float trungbinh = ( Bars.GetVolume(CurrentBar-1) + Bars.GetVolume(CurrentBar-2)+Bars.GetVolume(CurrentBar-3))/3 ;
                   Print("trung binh 3 nến : ");
					if( trungbinh > Bars.GetVolume(CurrentBar)){
						 Print ("thoa man dieu kien trung binh ");
					}
					
					////////////////////////////////////////
					/// 
					
                                  if (Close[0] > Open[0]){
                                  Draw.ArrowUp(this, "Mua", true, 0, Low[0] - TickSize, Brushes.Red);
								  }else{
						
								
                                     Draw.ArrowDown(this, "Bán", true, 0, High[0] - TickSize, Brushes.Red);
								
								  }
					
					
					
					
				}
			}
			else{
				Print("xit !");
			}
			//Add your custom indicator logic here.
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Acanhbaokietvol[] cacheAcanhbaokietvol;
		public Acanhbaokietvol Acanhbaokietvol()
		{
			return Acanhbaokietvol(Input);
		}

		public Acanhbaokietvol Acanhbaokietvol(ISeries<double> input)
		{
			if (cacheAcanhbaokietvol != null)
				for (int idx = 0; idx < cacheAcanhbaokietvol.Length; idx++)
					if (cacheAcanhbaokietvol[idx] != null &&  cacheAcanhbaokietvol[idx].EqualsInput(input))
						return cacheAcanhbaokietvol[idx];
			return CacheIndicator<Acanhbaokietvol>(new Acanhbaokietvol(), input, ref cacheAcanhbaokietvol);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Acanhbaokietvol Acanhbaokietvol()
		{
			return indicator.Acanhbaokietvol(Input);
		}

		public Indicators.Acanhbaokietvol Acanhbaokietvol(ISeries<double> input )
		{
			return indicator.Acanhbaokietvol(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Acanhbaokietvol Acanhbaokietvol()
		{
			return indicator.Acanhbaokietvol(Input);
		}

		public Indicators.Acanhbaokietvol Acanhbaokietvol(ISeries<double> input )
		{
			return indicator.Acanhbaokietvol(input);
		}
	}
}

#endregion
