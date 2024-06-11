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
	public class ZigZagCustom : Indicator
    {
        private double lastHigh;
        private double lastLow;
        private int lastHighBar;
        private int lastLowBar;
        private int trendDir;
		private Series<double>  ZigZag;
        private bool trend; // true tăng , false giảm
        

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"ZigZag indicator with custom depth, deviation, and backstep.";
                Name = "1.ZigZagCustom";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true;
                AddPlot(Brushes.Goldenrod, "ZigZag");
				
				
				// Thiết lập các giá trị mặc định cho ZZ
				Deviation =0.05;
				Backstep =3;
				Depth =12;
				
            }
			else if (State == State.Configure)
			{
			
				 lastLow =lastLowBar =0; // Khi bắt đầu tất cả = 0
				 lastHigh = lastHighBar =0;
			}
			else if (State == State.DataLoaded)
			{
					ZigZag = new Series<double>(this);
			}
        }

        protected override void OnBarUpdate()
        {
			if(IsFirstTickOfBar &&CurrentBars[0]!=0 )
				return;
			Print("Tick dau tien cua nen");
            if(trend) // trend tăng
			{
				if(High[1] > lastHigh)
				{  // Chế độ ontick , chạy ở fistoftick
				  lastHigh = High[1];
				  lastHighBar = CurrentBars[0]-1; 
			    }
				
				if (lastHigh  > (1+Deviation)*lastLow && CurrentBar - lastHighBar >= Backstep) // độ lệch + backstep
                {
                    trend = false;  // Đổi đảo qua trend giảm
                    ZigZag[1]= lastHigh;
                    lastLow = Low[1];
                    lastLowBar = CurrentBars[0]-1;
					
					//plot
					  Values[0][lastHighBar] = lastHigh;
					Print( "đỉnh : " +lastHighBar);
                }
			}
            
			
			if(!trend)  // trend giảm
			{
				if(Low[1] < lastLow)
			    {
				    lastLow = Low[1];
				    lastLowBar = CurrentBars[0]-1; 
			    }
				if (lastLow  < (1+Deviation)*lastHigh && CurrentBar - lastHighBar >= Backstep) // độ lệch + backstep
                {
                    trend = true; // Đảo qua tăng 
					
                    ZigZag[1] = lastLow;
                    lastHigh = High[1];
                    lastHighBar = CurrentBars[0]-1;
					
					//plot
					  Values[0][lastLowBar] = lastLow;
					Print( "đáy : " + lastLowBar);
                }
			}
			
			
			
			
            
        }

    
      [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="Depth", Order=1, GroupName="Parameters")]
        public int Depth { get; set; }

        [NinjaScriptProperty]
        [Range(0.0, double.MaxValue)]
        [Display(Name="Deviation", Order=2, GroupName="Parameters")]
        public double Deviation { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="Backstep", Order=3, GroupName="Parameters")]
        public int Backstep { get; set; }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ZigZagCustom[] cacheZigZagCustom;
		public ZigZagCustom ZigZagCustom(int depth, double deviation, int backstep)
		{
			return ZigZagCustom(Input, depth, deviation, backstep);
		}

		public ZigZagCustom ZigZagCustom(ISeries<double> input, int depth, double deviation, int backstep)
		{
			if (cacheZigZagCustom != null)
				for (int idx = 0; idx < cacheZigZagCustom.Length; idx++)
					if (cacheZigZagCustom[idx] != null && cacheZigZagCustom[idx].Depth == depth && cacheZigZagCustom[idx].Deviation == deviation && cacheZigZagCustom[idx].Backstep == backstep && cacheZigZagCustom[idx].EqualsInput(input))
						return cacheZigZagCustom[idx];
			return CacheIndicator<ZigZagCustom>(new ZigZagCustom(){ Depth = depth, Deviation = deviation, Backstep = backstep }, input, ref cacheZigZagCustom);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ZigZagCustom ZigZagCustom(int depth, double deviation, int backstep)
		{
			return indicator.ZigZagCustom(Input, depth, deviation, backstep);
		}

		public Indicators.ZigZagCustom ZigZagCustom(ISeries<double> input , int depth, double deviation, int backstep)
		{
			return indicator.ZigZagCustom(input, depth, deviation, backstep);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ZigZagCustom ZigZagCustom(int depth, double deviation, int backstep)
		{
			return indicator.ZigZagCustom(Input, depth, deviation, backstep);
		}

		public Indicators.ZigZagCustom ZigZagCustom(ISeries<double> input , int depth, double deviation, int backstep)
		{
			return indicator.ZigZagCustom(input, depth, deviation, backstep);
		}
	}
}

#endregion
