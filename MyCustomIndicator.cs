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
	
	public class DeltarBar : BarsType
	{
		public override void ApplyDefaultBasePeriodValue(BarsPeriod period) {}
         //rong NinjaTrader 8 (NT8), hàm ApplyDefaultBasePeriodValue() là một phương thức được sử dụng để áp dụng giá trị mặc định cho khoảng thời gian cơ sở (base period) của một chỉ báo hoặc thông số khác trên biểu đồ.

		public override void ApplyDefaultValue(BarsPeriod period) // hàm gán giá trị mặc định
		{
			period.Value = 100;

			//delta =0;
			last_delta = 0;
			
		}
         /*
		private void UpdatedeltaStaticAdd(Bars bars,double ask , double open , long delta){
			
				if ( ask > open ){
						 barcache.newbar(bars.Count-1,-delta);
					}else{
						 barcache.newbar(bars.Count-1,delta);
					}
		}
		
		private void UpdatedeltaStaticUpdate(Bars bars, long delta){
			barcache.updatebar(bars.Count-1,delta);
		}
		*/
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
            
			
			if (SessionIterator == null)
				SessionIterator = new SessionIterator(bars);

			bool isNewSession = SessionIterator.IsNewSession(time, isBar);
			if (isNewSession)
				SessionIterator.GetNextSession(time, isBar);

			long barsPeriodValue = bars.BarsPeriod.Value; 
			if (bars.Instrument.MasterInstrument.InstrumentType == InstrumentType.CryptoCurrency)
               barsPeriodValue = Core.Globals.FromCryptocurrencyVolume(bars.BarsPeriod.Value);
			
			if (bars.Count == 0)          //  nếu chưa có nến. check delta hiện tại lớn hơn max delta của một nến ( 100 200..) tạo một nến mới với volume vol_ask + vol_bid
			{          
				if(volume > barsPeriodValue)
				{
					AddBar(bars, open, high, low, close, time, barsPeriodValue);        
					AddBar(bars, open, high, low, close, time, volume - barsPeriodValue);
					
					if ( ask > open )
					{last_delta = -(volume - barsPeriodValue);}
					else{last_delta = volume - barsPeriodValue;}
				}
				else{
					AddBar(bars, open, high, low, close, time, volume);
					// phần thêm delta vào static
					//   UpdatedeltaStaticAdd(bars,ask,open, volume);
					//
					if ( ask > open )
					{last_delta = -volume;}
					else{last_delta = volume;}
				}
			}
			else
			{   // Tính toán last_delta 
				if ( ask > open ){last_delta -=  volume;}
				else{last_delta +=volume;}
				
				long Abs_delta = Math.Abs(last_delta);
				long last_vol = bars.GetVolume(bars.Count-1);
				if(Abs_delta <= barsPeriodValue){
					
					UpdateBar(bars, high, low, close, time, last_vol +volume);//
                    
				}
				//Print("nến cũ delta : " + last_delta);
				//Abs_delta = Math.Abs(last_delta);
				long vol_update =0;
				long delta_tmp = Math.Abs(last_delta)-barsPeriodValue;
				while(delta_tmp >0){
					 vol_update= Math.Min(delta_tmp,barsPeriodValue);
					AddBar(bars, open, high, low, close, time, vol_update);
					
					
					delta_tmp -= barsPeriodValue;
					if ( ask > open ){last_delta =-vol_update;}
					    else{last_delta =vol_update ;}
						
				}
			} //==========end 95
		}//============================	   

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= "Delta Bar";
				BarsPeriod		= new BarsPeriod { BarsPeriodType = (BarsPeriodType)9912, Value = 100 };
				BuiltFrom		= BarsPeriodType.Tick;
				DaysToLoad		= 1;
				IsIntraday		= true;
				IsTimeBased		= false;
			}
			else if (State == State.Configure)
			{
				Name = string.Format(Core.Globals.GeneralOptions.CurrentCulture, "DELTA-"+  BarsPeriod.Value.ToString(), BarsPeriod.Value, (BarsPeriod.MarketDataType != MarketDataType.Last ? string.Format(" - {0}", Core.Globals.ToLocalizedObject(BarsPeriod.MarketDataType, Core.Globals.GeneralOptions.CurrentUICulture)) : string.Empty));

				Properties.Remove(Properties.Find("BaseBarsPeriodType",			true));
				Properties.Remove(Properties.Find("BaseBarsPeriodValue",		true));
				Properties.Remove(Properties.Find("PointAndFigurePriceType",	true));
				Properties.Remove(Properties.Find("ReversalType",				true));
				Properties.Remove(Properties.Find("Value2",						true));
			}
		}
	}
}
