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
#region obv

namespace NinjaTrader.NinjaScript.Indicators
{
	
	
	
    public class OBVWithColor : Indicator
    {
        private double previousObv;
        private Series<double> obv;
        
		private double lastDelta;
		private List<double> deltaList;
		private Series<int> tnrSignal;
		
		// Series chứa nến có tín hiệu vào
		
		private Series<int> Lenh;  //  -1 bán -2 bán kiểu 2 , 0 null , 1 mua 1 , 2 mua kiểu 2  . Các lệnh đã được bbaams
        
		private Series<int> SigleNote;    // lưu các vị trí có tín hiệu dù ko vào lệnh , để tín hiệu sau check xem khoảng cách gần nhất có tín hiệu ngược chiều ko
		
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Tính toán obv và delta Tunaround.";
                Name = "1.obvmod";
                Calculate = Calculate.OnEachTick;
                IsOverlay = false;

                // Thêm một plot với màu xanh mặc định
                AddPlot(Brushes.Blue, "OBVPlot");
            }
            else if (State == State.DataLoaded)
            {
                obv = new Series<double>(this);
            }
			else if (State == State.Configure)
			{
				lastDelta = 0.0;
				tnrSignal = new Series<int>(this);
				deltaList = new List<double>();
				Lenh = new Series<int>(this);
				SigleNote = new Series<int>(this);
				
			}
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar == 0)
            {
                obv[0] = Volume[0];
                previousObv = obv[0];
                return;
            }
            
            // Tính toán OBV
            if (Close[0] > Close[1])
			{ obv[0] = obv[1] + Volume[0];}
            else if (Close[0] < Close[1])
			{ obv[0] = obv[1] - Volume[0];}
            else{
				  obv[0] = obv[1];
			}
              
             
			if(IsFirstTickOfBar && CurrentBars[0] > 0)
            {
				deltaList.Add(lastDelta);
					lastDelta = 0.0;
				if(CurrentBars[0] >=2 )
					{
					      KeoOBV();
					}
				
			}
//            // Thiết lập màu sắc cho plot
//            if (obv[0] > previousObv)
//                PlotBrushes[0][0] = Brushes.Green;
//            else if (obv[0] < previousObv)
//                PlotBrushes[0][0] = Brushes.Red;
			// So sánh màu khi obv cũ trước đó
			PlotBrushes[0][0] = obv[0] >= previousObv ? Brushes.Green : Brushes.Red;
            
            // Cập nhật giá trị OBV cho plot
            Values[0][0] = obv[0];
			
			
			
			
			// cuối cùng mới cập nhật obv mới
            previousObv = obv[0];
        }
		
		
		
		private void KeoOBV(){
				if (KtraDojBua(1) ==true || Volume[1] < Volume[2]) // Nếu là nến doj nến búa = done
							return;
						
						var sigleDelta = Turnaround();
						if(sigleDelta ==1){
							SigleNote[1] =3;
//							if (ThanNen())
//								return;
//							// kiếm tra nến liền trước [2] có lệnh ngược chiều sell  ko và volume ra sao ?
//							if (CheckLenhTruoc(3,true) & Volume[1] < Volume[2])
//							{
//								Draw.Text(this,"huy"+ CurrentBar,"hmua",1,BarsArray[0].GetLow(CurrentBars[0]-1) + 8* TickSize);
//								return;
//							} else
//							{
//								if (obv[1] > obv[2])
//								{
//								       Draw.ArrowUp(this, "mua1"+CurrentBar, true, 1, BarsArray[0].GetLow(CurrentBars[0]-1) - 8* TickSize,Brushes.Blue,true);
//								      SigleNote[1] =1;
//							     }
//							    else if (obv[1] > obv[2] & obv[1] > obv[3]){
//							          	Draw.ArrowUp(this, "mua2"+CurrentBar, true, 1, BarsArray[0].GetLow(CurrentBars[0]-1) - 8* TickSize,Brushes.Cyan,true);
//							          	SigleNote[1] =2;
//							          }
//							}
							
								
						}
						else if (sigleDelta == -1){
							SigleNote[1] =-3;
							// kiếm tra nến liền trước [2] có lệnh ngược chiều ( (buy )ko và volume ra sao ?
//							if (CheckLenhTruoc(3,false) & Volume[1] < Volume[2])
//							{
//								Draw.Text(this,"huy"+ CurrentBar,"hban",1,BarsArray[0].GetHigh(CurrentBars[0]-1) + 8* TickSize);
//								return;
//							} else
//							{
//								if (obv[1] < obv[2]){
//								     Draw.ArrowDown(this, "ban1"+CurrentBar, true, 1, BarsArray[0].GetHigh(CurrentBars[0]-1) +8* TickSize,Brushes.OrangeRed,true);
//								     SigleNote[1] =-1;
//							     }
//							     else if (obv[1] < obv[2] & obv[1] < obv[3]){
//							     	Draw.ArrowDown(this, "ban2"+CurrentBar, true, 1, BarsArray[0].GetHigh(CurrentBars[0]-1) + 8* TickSize,Brushes.Red,true);
//							     	SigleNote[1] =-2;
//							        }
//							}
							
						}
		}
		private bool CheckLenhTruoc(int range,bool xanh){  // số nến tối đa mà xuất hiện lệnh ngược chiều => done : range có thể cấu hình biến nhập từ GUI
			for(int i = 1; i< 1+ range; i++)
			{
				if(xanh==true & SigleNote[i] <0)
					return true; // done
				if(xanh ==false & SigleNote[i] >0)
					return true;  // done
			}
			return false;
		}
		
		public bool ThanNen(){
			if(Math.Abs(Open[1]-Close[1]) < 0.5* Math.Abs(Open[2]-Close[2]))
				return true;
			
			return false;
		}
		public bool KtraDojBua(int i)
		{
			if( Math.Abs(Open[i]-Close[i])< 0.35*(High[i]-Low[i]))  // nếu thân nến < 25% chiều dài nến
				return true;
			
			return  false;
		}
		// Delta TRN
		private int Turnaround()
		{
			//  Chọn kiểu tính toán sma volum
		
			// xanh 
			if(deltaList[CurrentBars[0]-1] > 0)
			{
                if(deltaList[CurrentBars[0]-2] > 0)
				  return 0; 
				if(BarsArray[0].GetLow(CurrentBars[0]-1) <= BarsArray[0].GetLow(CurrentBars[0]-2) && BarsArray[0].GetClose(CurrentBars[0]-1) > BarsArray[0].GetOpen(CurrentBars[0]-1))
				{
					// Tín hiệu delta TNR up
					tnrSignal[1] = 1;
					//Draw.ArrowUp(this, "mua"+CurrentBar, true, 1, BarsArray[0].GetLow(CurrentBars[0]-1) - 12* TickSize,Brushes.Green,true);
					Draw.Dot(this, "mua"+CurrentBar,  true,  1, BarsArray[0].GetLow(CurrentBars[0]-1) - 12* TickSize,Brushes.Cyan);
					//Draw.ArrowUp(this, "mua"+CurrentBar, true, 1, BarsArray[0].GetLow(CurrentBars[0]-1) - 8* TickSize,volup_color,true);
					// Thêm vào mảng của delta phân kỳ 
					//BoT.delta_phanky.Add(CurrentBars[0]-1);
					
					return 1; // Delta Tunaround ok => done
				}
					
			}else 
			{ // đỏ
				if(deltaList[CurrentBars[0]-2] < 0)
					return 0;
				if(BarsArray[0].GetHigh(CurrentBars[0]-1) >= BarsArray[0].GetHigh(CurrentBars[0]-2) &&BarsArray[0].GetClose(CurrentBars[0]-1) < BarsArray[0].GetOpen(CurrentBars[0]-1))
				{
					// Tín hiệu delta TNR down
					//Draw.ArrowDown(this, "ban"+CurrentBar, true, 1, BarsArray[0].GetHigh(CurrentBars[0]-1) + 12* TickSize,Brushes.Yellow,true);
					tnrSignal[1] = -1;
						Draw.Dot(this, "bán"+CurrentBar,  true,  1, BarsArray[0].GetHigh(CurrentBars[0]-1) + 12* TickSize,Brushes.Violet);
					//Draw.ArrowDown(this, "ban"+CurrentBar, true, 1,BarsArray[0].GetHigh(CurrentBars[0]-1) + 2* TickSize,voldow_color,true);
					// BoT.delta_phanky.Add(CurrentBars[0]-1);
					return -1; // delta tunaround ok done !
				}
			}
			tnrSignal[1] = 0;
			return 0;
		}
		// Hàm tính Delta
		protected override void OnMarketData(MarketDataEventArgs e)
		{			
			if(e.MarketDataType == MarketDataType.Last)
			{				
				if(e.Price >= e.Ask)
				{
					//buyTmp += e.Volume;
					lastDelta  += e.Volume;
				}
				else if (e.Price <= e.Bid)
				{
				//	sellTmp += e.Volume;
					lastDelta -= e.Volume;
				}
			}	
		}
		
		
		
		private double[] GetPriceI2(int i)
		{
		     double[] rT = new double[4];
			rT[0]= BarsArray[0].GetOpen(CurrentBars[i]);
			rT[1]= BarsArray[0].GetClose(CurrentBars[i]);
			rT[2]= BarsArray[0].GetHigh(CurrentBars[i]);
			rT[3]= BarsArray[0].GetLow(CurrentBars[i]);
			return rT;
		}
				
				
    }
}

#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OBVWithColor[] cacheOBVWithColor;
		public OBVWithColor OBVWithColor()
		{
			return OBVWithColor(Input);
		}

		public OBVWithColor OBVWithColor(ISeries<double> input)
		{
			if (cacheOBVWithColor != null)
				for (int idx = 0; idx < cacheOBVWithColor.Length; idx++)
					if (cacheOBVWithColor[idx] != null &&  cacheOBVWithColor[idx].EqualsInput(input))
						return cacheOBVWithColor[idx];
			return CacheIndicator<OBVWithColor>(new OBVWithColor(), input, ref cacheOBVWithColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OBVWithColor OBVWithColor()
		{
			return indicator.OBVWithColor(Input);
		}

		public Indicators.OBVWithColor OBVWithColor(ISeries<double> input )
		{
			return indicator.OBVWithColor(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OBVWithColor OBVWithColor()
		{
			return indicator.OBVWithColor(Input);
		}

		public Indicators.OBVWithColor OBVWithColor(ISeries<double> input )
		{
			return indicator.OBVWithColor(input);
		}
	}
}

#endregion
