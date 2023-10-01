// 
// Copyright (C) 2022, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.ComponentModel;
using NinjaTrader;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
#endregion

namespace NinjaTrader.NinjaScript.BarsTypes
{
	public class Deltadev2 : BarsType
	{
		public override void ApplyDefaultBasePeriodValue(BarsPeriod period) {}
         //rong NinjaTrader 8 (NT8), hàm ApplyDefaultBasePeriodValue() là một phương thức được sử dụng để áp dụng giá trị mặc định cho khoảng thời gian cơ sở (base period) của một chỉ báo hoặc thông số khác trên biểu đồ.

		public override void ApplyDefaultValue(BarsPeriod period) // hàm gán giá trị mặc định
		{
			period.Value = 100;

			//delta =0;
			last_delta = 0;
			
		}

		public override string ChartLabel(DateTime time)
		{
			return time.ToString("T", Core.Globals.GeneralOptions.CurrentCulture);
		}

		public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack) { return 1; }  //lấy số ngày load dữ liệu
	
		public override double GetPercentComplete(Bars bars, DateTime now) // tính % hoàn thành của 1 thanh nến đang chạy
		{
			return bars.Count == 0 ? 0 : (double) bars.GetVolume(bars.Count - 1) / bars.BarsPeriod.Value;
		}

		private long last_delta;

        
		protected override void OnDataPoint(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isBar, double bid, double ask)
		{
		//	Print(open + "high : " +high+ " Low : "+low + "  Close : "+ close +bid.ToString() +"--"+ ask.ToString() +"---"+ volume.ToString());
			
		  
          // long delta=0;
	
				
			long Abs_delta = Math.Abs(last_delta);
			
			if (SessionIterator == null)
				SessionIterator = new SessionIterator(bars);

			bool isNewSession = SessionIterator.IsNewSession(time, isBar);
			if (isNewSession)
				SessionIterator.GetNextSession(time, isBar);

			long barsPeriodValue = bars.BarsPeriod.Value; 
			if (bars.Instrument.MasterInstrument.InstrumentType == InstrumentType.CryptoCurrency)
           
			
			//Print("run Calculator \n");
          
			if (bars.Count == 0)          //  nếu chưa có nến. check delta hiện tại lớn hơn max delta của một nến ( 100 200..) tạo một nến mới với volume vol_ask + vol_bid
			{     
			//	Print (" Nến đầu tiên của phiên = " + bars.Count) ;
                 
				if(volume > barsPeriodValue)
				{
				//Print("p1");
				//	Print("p1 \n volume: " +volume +"\n last_delta :" + last_delta);      //====> Debug
					AddBar(bars, open, high, low, close, time, barsPeriodValue);
					AddBar(bars, open, high, low, close, time, volume - barsPeriodValue);
					if ( ask > open )
					{last_delta = -(volume - barsPeriodValue);}
					else{last_delta = volume - barsPeriodValue;}
				//	Draw.ArrowUp(this, "tag1", true, 0, low - TickSize, Brushes.Red);
				//	 BarBrush = Brushes.Yellow;
				}
				else{
					//	Print("p2 \n volume: " +volume +"\n last_delta :" + last_delta);   //====> Debug
					AddBar(bars, open, high, low, close, time, volume);
					if ( ask > open )
					{last_delta = -volume;}
					else{last_delta = volume;}
				}
				//Print("End newsession : " + last_delta);
			}
			else
			{
				//Print("Count > 1");
			//	Print("p3 \n volume: " +volume +"\n last_delta :" + last_delta);             //====> Debug
                //long old_delta = last_delta;
				long last_vol = bars.GetVolume(bars.Count-1);
				if(Abs_delta <= barsPeriodValue){
					
					UpdateBar(bars, high, low, close, time, last_vol +volume);//
					if ( ask > open ){last_delta -=  volume;
					// Print("bid ");
						}
					else{last_delta +=volume;
						//Print("ask");
					}
					//Print("end 3 : "+ last_delta);             // Nếu sau tính toán delta >=100 thì sao ? 102 101 103 ???         
				}
				
				// TIish toán lại Abs_detla
				Abs_delta = Math.Abs(last_delta);
				int dem =0;
				long vol_update =0;
				long delta_tmp = Math.Abs(last_delta)-barsPeriodValue;
				while(delta_tmp >0){
					dem++;
					 vol_update= Math.Min(delta_tmp,barsPeriodValue);
					AddBar(bars, open, high, low, close, time, vol_update);
					//Print("Tao nến mới thứ : "+ dem + " volume : " + vol_update);
					delta_tmp -= barsPeriodValue;
					if ( ask > open ){last_delta =-vol_update;}
					    else{last_delta =vol_update ;}
					
				}
					    
					// Cập nhật lại giá trị last_delta cho lần tính sau của hàm OnDataPoint
					
				
				
				
			} //==========end 95
		}//============================	   

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= "Deltadev2";
				BarsPeriod		= new BarsPeriod { BarsPeriodType = (BarsPeriodType)54556, Value = 100 };
				BuiltFrom		= BarsPeriodType.Tick;
				DaysToLoad		= 1;
				IsIntraday		= true;
				IsTimeBased		= false;
			}
			else if (State == State.Configure)
			{
				Name = string.Format(Core.Globals.GeneralOptions.CurrentCulture, "Deltadev2", BarsPeriod.Value, (BarsPeriod.MarketDataType != MarketDataType.Last ? string.Format(" - {0}", Core.Globals.ToLocalizedObject(BarsPeriod.MarketDataType, Core.Globals.GeneralOptions.CurrentUICulture)) : string.Empty));

				Properties.Remove(Properties.Find("BaseBarsPeriodType",			true));
				Properties.Remove(Properties.Find("BaseBarsPeriodValue",		true));
				Properties.Remove(Properties.Find("PointAndFigurePriceType",	true));
				Properties.Remove(Properties.Find("ReversalType",				true));
				Properties.Remove(Properties.Find("Value2",						true));
			}
		}
	}
}
