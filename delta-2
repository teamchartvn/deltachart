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
			if ( ask > open ){
				last_delta -=volume;
			        }
			    else{
					last_delta += volume;
			        }
				
			long Abs_delta = Math.Abs(last_delta);
			
			if (SessionIterator == null)
				SessionIterator = new SessionIterator(bars);

			bool isNewSession = SessionIterator.IsNewSession(time, isBar);
			if (isNewSession)
				SessionIterator.GetNextSession(time, isBar);

			long barsPeriodValue = bars.BarsPeriod.Value; 
			if (bars.Instrument.MasterInstrument.InstrumentType == InstrumentType.CryptoCurrency)
           
			
			Print("run Calculator \n");
          
			if (bars.Count == 0)          //  nếu chưa có nến. check delta hiện tại lớn hơn max delta của một nến ( 100 200..) tạo một nến mới với volume vol_ask + vol_bid
			{     
				Print (" Nến đầu tiên của phiên = " + bars.Count) ;
                  AddBar(bars, open, high, low, close, time, volume);
				if(volume > barsPeriodValue)
				{
					last_delta = 0;
				}
				else{
					if ( ask > open ){last_delta = -volume;}
					else{last_delta = volume;}
				}
				last_delta=0;
				return;
			}
			else
			{
				
                long old_delta = last_delta;
				long last_vol = bars.GetVolume(bars.Count-1);
				
				if(Abs_delta <= barsPeriodValue){
					UpdateBar(bars, high, low, close, time, last_vol +volume);//
					if ( ask > open ){last_delta -=  volume;}
					else{last_delta +=volume;}
					return;
				}else{
					Print("Update nen : " +(bars.Count -1).ToString() +" old_delta :" + old_delta +" abs_old "+ Math.Abs(old_delta).ToString());
					long vol_update = barsPeriodValue -Math.Abs(old_delta);
					UpdateBar(bars, high, low, close, time, last_vol +vol_update);// update vào nến cũ trước rồi tiến hành tạo nến mới sau !
					//AddBar(bars, open, high, low, close, time,Abs_delta -barsPeriodValue);
					Print ("old_detla : " + Math.Abs(old_delta) +" vol_update  : " +vol_update.ToString());
					Print("abs truoc : "+ Abs_delta);
					Abs_delta = Abs_delta- vol_update; // abs_Delta còn lại sau khi update nến cũ => sẽ dùng tính toán tạo nến mới
                   Print("abs Sau : "+ Abs_delta);
					
					long deltatmp = Math.Min(Abs_delta ,barsPeriodValue); // kiểm tra xem delta thực nhỏ  hơn hay barsPeriodValue nhỏ hơn = dùng cái nhỏ hơn.
					int dem =0;
					while (deltatmp >0)
					{
						dem++;
						
						AddBar(bars, open, high, low, close, time,deltatmp); // lúc này vol chính = delta 
						//delta còn lại 
						Abs_delta -= deltatmp;
						deltatmp = Math.Min(Abs_delta ,barsPeriodValue);
						Print("Tao nen moi thu : " +dem);
						Print("Delta Abs con lai : " + Abs_delta);
					}
					
					// Cập nhật lại giá trị last_delta cho lần tính sau của hàm OnDataPoint
					if ( ask > open ){last_delta =-Abs_delta;}
					else{last_delta =Abs_delta ;}
				}
				
				//============================	   
			}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= "Delta-2";
				BarsPeriod		= new BarsPeriod { BarsPeriodType = (BarsPeriodType)9981, Value = 100 };
				BuiltFrom		= BarsPeriodType.Tick;
				DaysToLoad		= 1;
				IsIntraday		= true;
				IsTimeBased		= false;
			}
			else if (State == State.Configure)
			{
				Name = string.Format(Core.Globals.GeneralOptions.CurrentCulture, "Delta2", BarsPeriod.Value, (BarsPeriod.MarketDataType != MarketDataType.Last ? string.Format(" - {0}", Core.Globals.ToLocalizedObject(BarsPeriod.MarketDataType, Core.Globals.GeneralOptions.CurrentUICulture)) : string.Empty));

				Properties.Remove(Properties.Find("BaseBarsPeriodType",			true));
				Properties.Remove(Properties.Find("BaseBarsPeriodValue",		true));
				Properties.Remove(Properties.Find("PointAndFigurePriceType",	true));
				Properties.Remove(Properties.Find("ReversalType",				true));
				Properties.Remove(Properties.Find("Value2",						true));
			}
		}
	}
}








