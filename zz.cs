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
        
		private double doLech;
		// điểm đỉnh đáy ZZ
        private double lowPoint, highPoint;
		
		public int Depth ;
        public double Deviation;
        public int Backstep ;
		private bool timDinh=false; // true => tim day , false tim Dinh
		
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"ZigZag indicator with custom depth, deviation, and backstep.";
                Name = "1.ZigZagCustom";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true;
                AddPlot(new Stroke(Brushes.Yellow),PlotStyle.Line, "ZigZag");
				
				
				// Thiết lập các giá trị mặc định cho ZZ
				Deviation =20;
				Backstep =3;
				Depth =12;
				
            }
			else if (State == State.Configure)
			{
			
				 lastLow =lastLowBar = 0; // Khi bắt đầu tất cả = 0
				 lastHigh = lastHighBar = 0;
				 doLech = 20* TickSize;
				Print( "dolec :  " +doLech);
				lowPoint = highPoint =0;          
				
			}
			else if (State == State.DataLoaded)
			{
					ZigZag = new Series<double>(this);
			}
        }

        protected override void OnBarUpdate()
        {
			if( BarsInProgress ==0)
			{
			
			if(!IsFirstTickOfBar || CurrentBars[0] <2 )
			{
				if( CurrentBars[0] ==0)
				{
				 //  Print("??? " + High[CurrentBars[0]-1]);
				  lastHigh =  BarsArray[0].GetHigh(0);
				  lastLow   = BarsArray[0].GetLow(0);
				}
				
				return;
			}
			
			
			Draw.Text(this, (CurrentBars[0]-1).ToString()+ "index",(CurrentBars[0]-1).ToString(),1 , High[1] + 5*TickSize);
			Print(CurrentBar-1);
				if(timDinh){
					
					if(High[1] >=lastHigh){
						lastHigh = High[1];
						lastHighBar = CurrentBar-1;
						//vẽ
						Values[0][0] = lastHigh;
						 PlotBrushes[0][0] = Brushes.Green;
					}
					else{  // Nếu có đỉnh mà trend bắt đầu giảm
						
						if(CurrentBar-1 -lastHighBar > Backstep){
							Print(" Xac nhan DINH Backstep");
							 // in giá trị HH ( đỉnh của zz )
							ZigZag[0] = lastHigh;
							
							if(CurrentBar-1 - lastHighBar >= Depth && lastHigh - Low[1] >= Deviation)
							{
								lastLow = Low[1];  // Đặt lại giá trị lastLow
								timDinh = false; // đã xác định được đáy thỏa mãn 3 điều kiện
							}
						}
					}
				}
				else
				{
					if(Low[1] <= lastLow){
						lastLow = Low[1];
					    lastLowBar = CurrentBar-1;
						
						// vẽ 
						Values[0][0] = lastLow;
						PlotBrushes[0][0] = Brushes.Red;
					}
					
					else{// nếu có đáy mà giá tăng trend up khi có đáy
						if(CurrentBar-1 - lastLowBar > Backstep)  // hiển thị đáy
						{
							Print(" Xac nhan DAY Backstep");
							ZigZag[1] = lastLow;
							if(CurrentBar-1 - lastLowBar  >= Depth && High[1] - lastLow >= Deviation)
							{
								lastHigh = High[1];
								timDinh = true;
							}
						}
					}
					
				}
				
				
				
				
				
				
				
				

					// end BarsInProgress=0
				
			}   
			
        } // end onbarupdate

    				//	Draw.Dot(this, CurrentBar.ToString() +"xanh", true, 1, Low[1] - 5*TickSize, Brushes.Cyan );
				  //Draw.Text (this ,CurrentBars[0]-1,"x",true, )
				  //Draw.Dot(NinjaScriptBase owner, string tag, bool isAutoScale, int barsAgo, double y, Brush brush)
					//Draw.Dot(this, CurrentBar.ToString()+"do", true, 1, Low[1] - 5*TickSize, Brushes.Red );


    } //end class
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ZigZagCustom[] cacheZigZagCustom;
		public ZigZagCustom ZigZagCustom()
		{
			return ZigZagCustom(Input);
		}

		public ZigZagCustom ZigZagCustom(ISeries<double> input)
		{
			if (cacheZigZagCustom != null)
				for (int idx = 0; idx < cacheZigZagCustom.Length; idx++)
					if (cacheZigZagCustom[idx] != null &&  cacheZigZagCustom[idx].EqualsInput(input))
						return cacheZigZagCustom[idx];
			return CacheIndicator<ZigZagCustom>(new ZigZagCustom(), input, ref cacheZigZagCustom);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ZigZagCustom ZigZagCustom()
		{
			return indicator.ZigZagCustom(Input);
		}

		public Indicators.ZigZagCustom ZigZagCustom(ISeries<double> input )
		{
			return indicator.ZigZagCustom(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ZigZagCustom ZigZagCustom()
		{
			return indicator.ZigZagCustom(Input);
		}

		public Indicators.ZigZagCustom ZigZagCustom(ISeries<double> input )
		{
			return indicator.ZigZagCustom(input);
		}
	}
}

#endregion
