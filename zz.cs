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
	public class  DinhDay{
		
		public int bar;
		public bool diemCao;
		public double giaDinhDay;
		
		public DinhDay(int bar_index , bool dinhorday, double gia)
		{
			this.bar = bar_index;
			this.diemCao = dinhorday;
			this.giaDinhDay = gia;
		}
	}
	
	public class ZZ : Indicator
    {
        private double lastHigh;
        private double lastLow;
        private int lastHighBar;
        private int lastLowBar;
        private int trendDir;
        private bool timDinh =true; // true tăng , false giảm
        private double Deviation;
        private int Backstep;
		private int Depth;
		private List<DinhDay> drawdinhday;  // chứa dữ liệu các đỉnh đáy để vẽ
		//private bool timDinh;
		//private double lastHighDinh , lastLowDay;
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"ZigZag indicator with custom depth, deviation, and backstep.";
                Name = "1.zz";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true;
                //AddPlot(Brushes.Goldenrod, "ZigZag");
				devi =10;
				
				// Thiết lập các giá trị mặc định cho ZZ
				//Deviation =0.05;
				//Backstep =3;
				//Depth =12;
				SonenDebug =30;
				
				
            }
			else if (State == State.Configure)
			{
			
				 lastLow =lastLowBar =0; // Khi bắt đầu tất cả = 0
				 lastHigh = lastHighBar =0;
				 Deviation = devi*TickSize;
				 Backstep =3;
				 Depth =12;
				 timDinh =true;
				 drawdinhday = new List<DinhDay>();
			}
			else if (State == State.DataLoaded)
			{
					//ZigZag = new Series<double>(this);
				   // drawdinhday = new Series<DinhDay>(this);
			}
        }

        protected override void OnBarUpdate()
        {
			if( BarsInProgress ==0)
			{
			
			if(!IsFirstTickOfBar || CurrentBars[0] <2 )
			{
				if( CurrentBars[0] ==1)
				{
				 //  Print("??? " + High[CurrentBars[0]-1]);
				  lastHigh =  BarsArray[0].GetHigh(0);
				  lastLow   = BarsArray[0].GetLow(0);
				}
				
				return;
			}
			
			// DEBUG Chi chay toi nen nay : thiet lap so nen muon test ma
			
//			if(CurrentBar > SonenDebug)
//				return;
			
			// End Nen debug
			
				
			// In index bar trên chart
			Draw.Text(this, (CurrentBars[0]-1).ToString()+ "index",(CurrentBars[0]-1).ToString(),1 , High[1] + 5*TickSize);
			Print(CurrentBar + " (H) => " + lastHigh.ToString() + " ih : " + lastHighBar.ToString() + "    ( L) =>" + lastLow.ToString() + " il : " + lastLowBar.ToString() + timDinh.ToString() );
			
			//     LOGIC
				if(timDinh)
				{
					
					if(High[1] >=lastHigh){
						lastHigh = High[1];
						lastHighBar = CurrentBar-1;
						//vẽ
						Print("Cao");
						
					}
					else{  // Nếu có đỉnh mà trend bắt đầu giảm
						
						if(CurrentBar-1 -lastHighBar >= Backstep){
							Print(" Xac nhan DINH Backstep " +lastHighBar);
							 // in giá trị HH ( đỉnh của zz )
							//var Dinh = new DinhDay(lastHighBar,true,lastHigh);
							Print(CurrentBar + " (H1) => " + lastHigh.ToString() + " ih : " + lastHighBar.ToString() + "    (L1) =>" + lastLow.ToString() + " il : " + lastLowBar.ToString() + timDinh.ToString() );
						
							
							//thêm đỉnh vảo drawdinhday -vẽ
							//drawdinhday.Add (new DinhDay(lastHighBar,true,lastHigh));
							if(CurrentBar-1 - lastHighBar >= Depth || lastHigh - Low[1] >= Deviation)
							{
								if(!CheckHighBar())
									 return;
								 Print("TRUE -> FALSE");
									Draw.Dot(this, "dinh" + lastHighBar, true, CurrentBar - lastHighBar, lastHigh + 5*TickSize, Brushes.Cyan);// cos nhaam lan ve lasthigbar
								lastLow = Low[1];  // Đặt lại giá trị lastLow
								lastLowBar = CurrentBar-1;
								timDinh = false; // đã xác định được đáy thỏa mãn 3 điều kiện
								
								// thêm đáy vào drawdinhday -vẽ
								//drawdinhday.Add ( new DinhDay(lastLowBar,false,lastLow));
								drawdinhday.Add (new DinhDay(lastHighBar,true,lastHigh));
								
							}
						}
					}
				}
				else   // timDInh =false ( tạo đáy )
				{
					if(Low[1] <= lastLow){
						lastLow = Low[1];           // giá thấp hơn tiếp tục tạo đáy thấp
					    lastLowBar = CurrentBar-1;
						Print("hap");
					}
					
					else{// nếu có đáy mà giá tăng trend up khi có đáy
						if(CurrentBar-1 - lastLowBar >= Backstep)  // hiển thị đáy
						{
							Print(" Xac nhan DAY Backstep " + lastLowBar);
							//ZigZag[1] = lastLow;
							drawdinhday[0] =  new DinhDay(lastLowBar,false,lastLow);
							Print(CurrentBar + " (H2) => " + lastHigh.ToString() + " ih : " + lastHighBar.ToString() + "    (L2) =>" + lastLow.ToString() + " il : " + lastLowBar.ToString() + timDinh.ToString() );
							
							
							//thêm đáy vào drawdinhday -vẽ
							 //drawdinhday.Add ( new DinhDay(lastLowBar,false,lastLow));
							if(CurrentBar-1 - lastLowBar  >= Depth || High[1] - lastLow >= Deviation)
							{
								 Print(" FALSE -> TRUE");
								 Draw.Dot(this, "day"+lastLowBar.ToString(), true,CurrentBar- lastLowBar,lastLow - 5*TickSize, Brushes.Violet);
								lastHigh = High[1];
								lastHighBar = CurrentBar-1;
								timDinh = true;
								//thêm đỉnh vào drawdinhday -vẽ
								//drawdinhday.Add (new DinhDay(lastHighBar,true,lastHigh));
								drawdinhday.Add ( new DinhDay(lastLowBar,false,lastLow));
							}
						}
					}
					
				}
				
				
				
				
			
			}//end CurrentBars
	
			DrawZigZag();
		}//end func
		
		private bool CheckHighBar(int endbar)
		{
			for(int i=1 ; i <= endbar ; i ++ )
			{
				if(High[1] <  High[i])
					return false;        // tồn tại 1 nến coa hơn nến 1
				i++;
			}
			return true; // nếu ko có nến nào cao hơn nến 1
		}
		
		private bool CheckLowhBar(int endbar)
		{
			if(Low[1] >  Low[i])
					return false;        // tồn tại 1 nến thấp  hơn nến 1
				i++;
			return true;
		}
		
		private void DrawZigZag(){
			 for (int i = 0 ; i < drawdinhday.Count-1 ;i ++){
			 	//  Vẽ nối 2 đỉnh 2 đáy
				// nếu  phần tử đứng liền trước cùng loại ( cùng đỉnh cùng đáy ) next chọn tiếp theo đền phần tử tiếp theo thỏa mãn điều kiện khác loại vd : (Dinh day day )
				// 
				 
				 if(drawdinhday[i].diemCao) 
				 {
				 	Draw.Line(this, "do"+ drawdinhday[i].bar.ToString(), false, CurrentBar - drawdinhday[i].bar, High[CurrentBar - drawdinhday[i].bar],  CurrentBar - drawdinhday[i +1].bar, Low[CurrentBar - drawdinhday[i +1].bar], Brushes.Red, DashStyleHelper.Dot, 2);
				 }
				if(!drawdinhday[i].diemCao){
				 //	Draw.Line(this, "xanh"+ drawdinhday[i].bar.ToString(), false, CurrentBar - drawdinhday[i].bar, Low[CurrentBar - drawdinhday[i].bar],  CurrentBar - drawdinhday[i +1].bar, High[CurrentBar - drawdinhday[i +1].bar], Brushes.LimeGreen, DashStyleHelper.Dot, 2);
				 }
			 }
		}
		
		  
		 #region Properties
		[NinjaScriptProperty]
		[Range(12, int.MaxValue)]
		[Display(Name="Sonen", Description="sonedebug", Order=1, GroupName="Parameters")]
		public int SonenDebug{ get; set; }
		
		
		[Range(5, int.MaxValue)]
		[Display(Name="Do Lech", Description="sonedebug", Order=1, GroupName="Parameters")]
		 public double devi { get; set; }
		#endregion
            
        } // end class

    
    
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ZZ[] cacheZZ;
		public ZZ ZZ(int sonenDebug)
		{
			return ZZ(Input, sonenDebug);
		}

		public ZZ ZZ(ISeries<double> input, int sonenDebug)
		{
			if (cacheZZ != null)
				for (int idx = 0; idx < cacheZZ.Length; idx++)
					if (cacheZZ[idx] != null && cacheZZ[idx].SonenDebug == sonenDebug && cacheZZ[idx].EqualsInput(input))
						return cacheZZ[idx];
			return CacheIndicator<ZZ>(new ZZ(){ SonenDebug = sonenDebug }, input, ref cacheZZ);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ZZ ZZ(int sonenDebug)
		{
			return indicator.ZZ(Input, sonenDebug);
		}

		public Indicators.ZZ ZZ(ISeries<double> input , int sonenDebug)
		{
			return indicator.ZZ(input, sonenDebug);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ZZ ZZ(int sonenDebug)
		{
			return indicator.ZZ(Input, sonenDebug);
		}

		public Indicators.ZZ ZZ(ISeries<double> input , int sonenDebug)
		{
			return indicator.ZZ(input, sonenDebug);
		}
	}
}

#endregion
