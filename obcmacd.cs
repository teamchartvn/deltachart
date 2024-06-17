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
using System.IO.MemoryMappedFiles;    // chia sẻ file
using System.IO;

using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using BOT2;

#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class  CustomOrder
    {
	    long           ticketId; //8 bytes unsigned long long (c++)
	    double         price;     //8 bytes
	    double         lotSize;
	    double         stopLoss;
	    double         takeProfit;
	    int            orderType;
	    int            action;                
   };
  
  
	public class OBVMACD : Indicator
	{
		
		private int windowLen;
		private int vlen;
		private int obvLength;
		
		private int obvMFlag;
		private int currentObvMFlag;
		
		private int obvEmaStart;
		private int fastMaStart;
		private int slowMaStart;
		
		private List<double> hlDelta; // high-low
		private List<double> obv;
		private List<double> obvSmooth;
		private List<double> shadowOut;
		private List<double> obvEma;
		private List<double> fastMa;
		private List<double> slowMa;
		private List<double> macd;
		//private List<double> processedMacd;
		private List<double> mcadSum;
		
		private Series<int> signal;
		private Series<double> processedMacd;
		
		// Khai báo biến của Delta:
		private double lastDelta;
		private double buyTmp ;
		private double sellTmp ;
		private List<double> deltaList;
		private Series<int> tnrSignal;
		private SimpleFont textFont;        // debug in sigleTnr
		// phần chia sẻ file 
		private MemoryMappedFile sharedMemory;
        private MemoryMappedViewAccessor accessor;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "01-OBVMACD";
				Calculate									= Calculate.OnEachTick;
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
				AddLine(Brushes.DarkGray,					0,				Custom.Resource.NinjaScriptIndicatorZeroLine);
				AddPlot(Brushes.SkyBlue, "MACD");
				Plots[0].Width 		= 2;
				
				FastLength					= 9;
				SlowLength					= 26;
			}
			else if (State == State.Configure)
			{
				windowLen = 28;
				vlen = 14;
				obvLength = 1;
				
				obvMFlag = 0;
				currentObvMFlag = 0;
				
				obvEmaStart = vlen + windowLen + obvLength - 2;
				fastMaStart = vlen + windowLen + obvLength + FastLength - 3;
				slowMaStart = SlowLength - 1;
				
				hlDelta = new List<double>();
				obv = new List<double>();
				obvSmooth = new List<double>();
				shadowOut = new List<double>();
				obvEma = new List<double>();
				fastMa = new List<double>();
				slowMa = new List<double>();
				macd = new List<double>();
				mcadSum = new List<double>();
				signal = new Series<int>(this);   //signal OBV
				
				processedMacd = new Series<double>(this);
				BOT2.TinHieuThoiGianThuc.tmp_obv = processedMacd;
				// delta 
				lastDelta = sellTmp = buyTmp =0.0;
				deltaList = new List<double>();
				tnrSignal = new Series<int>(this);
				
				textFont = new SimpleFont("Arial", 12); // // debug in sigleTnr cỡ chữ
				// chia sẻ mem
				try
				{ // thử mở với quyền đọc ghi
						//Print("Thử mở file");
					// sharedMemory = MemoryMappedFile.OpenExisting("NT8SharedMemory", MemoryMappedFileRights.ReadWrite);
						//accessor = sharedMemory.CreateViewAccessor();
			       Print("Tao File chia se moi CreateNew");
			       sharedMemory = MemoryMappedFile.CreateNew("NT8SharedMemory",128,MemoryMappedFileAccess.ReadWrite,MemoryMappedFileOptions.None,HandleInheritability.None);
			       accessor = sharedMemory.CreateViewAccessor();
					
			    }
				catch (Exception ex)
				{
					Print(" File da ton tai >>> thu mo  ");
					//MemoryMappedFile.DeleteFileMapping("NT8SharedMemory");
					try
					{
						sharedMemory = MemoryMappedFile.OpenExisting("NT8SharedMemory", MemoryMappedFileRights.ReadWrite);
						accessor = sharedMemory.CreateViewAccessor();
					// xóa file
					//File.Delete("NT8SharedMemory");
					}
					catch(Exception xex)
					{
						Print("Loi Mo file" );
						File.Delete("NT8SharedMemory");
						sharedMemory = MemoryMappedFile.CreateNew("NT8SharedMemory",128,MemoryMappedFileAccess.ReadWrite,MemoryMappedFileOptions.None,HandleInheritability.None);
			            accessor = sharedMemory.CreateViewAccessor();
					}
				}
			}
			else if(State == State.DataLoaded)
			{
				//hlDelta = new Series<double>(this);
			}
			else if(State == State.Terminated)
			{ // Hủy các giá trị khi ....
				if (accessor != null)
                {
					Print("xoa accessor ");
                    accessor.Dispose();
                    accessor = null;
                }
				
                if (sharedMemory != null)
                {
					Print("xoa sharedMemory ");
                    sharedMemory.Dispose();
                    sharedMemory = null;
                }
			}
		}
		
		///////////////////////////// kết thúc OnStateChange()
		// Khi có tín hiệu entry-> call sharemem -> ghi vào một struct.
		private int CheckInBar(int barIndex){
		    
			while(true)
			{
				if(High[barIndex+1]< High[barIndex] & Low[barIndex+1] > Low[barIndex]){
					barIndex++;
				}
				else{
					return barIndex;
				}
			}
			
			
		}
		private double[] GetPriceI2(int i){
		     double[] rT = new double[4];
			rT[0]= BarsArray[0].GetOpen(CurrentBars[i]);
			rT[1]= BarsArray[0].GetClose(CurrentBars[i]);
			rT[2]= BarsArray[0].GetHigh(CurrentBars[i]);
			rT[3]= BarsArray[0].GetLow(CurrentBars[i]);
			return rT;
		}
		
		public void KeoKhoNhat()
		{
			bool loop = false;
			//int signalMot = signal[1];
			int i =1;
			if( (signal[i] ^ signal[i+1]) < 0){// đỏ kèo xuống , bắt đầu từ 2 trở về trước phải là xanh 
			   loop = true;
				i++;
			}
			
			do {  // bắt đầu từ i thứ 2:
				if((signal[i] ^signal[i+1]) >0 ){  // cùng dấu
					if( tnrSignal[i] ==0){
						loop =true;
						i++;
						continue;
					}else if ((tnrSignal[i] ^ signal[1]) >0){ // bấm kèo ở đây cùng dấu
						// lấy giá của nến đóng cửa của nến trước
						var getPrice = GetPriceI2(i);   
						if(tnrSignal[i] >0){ // buy
							
						        if(CheckInBar(i)==i){ //ko ôm nến nào :D
									
								}
						}
						else{ //sell
							
						}
					}
				}
				else{
					loop =false;
					return;
				}
			}
				
			while(loop);
				
		}
		
		//public void KeoOSB
		
        // Phần Delta của ông Việt điên.
		protected override void OnMarketData(MarketDataEventArgs e)
		{			
			if(e.MarketDataType == MarketDataType.Last)
			{				
				if(e.Price >= e.Ask)
				{
					buyTmp += e.Volume;
					lastDelta  += e.Volume;
				}
				else if (e.Price <= e.Bid)
				{
					sellTmp += e.Volume;
					lastDelta -= e.Volume;
				}
			}	
		}
		
		// Hàm này tính delta TNR
		private void Turnaround()
		{
			//  Chọn kiểu tính toán sma volum
		
			// xanh 
			if(deltaList[CurrentBars[0]-1] > 0)
			{
                if(deltaList[CurrentBars[0]-2] > 0)
				  return; 
				if(BarsArray[0].GetLow(CurrentBars[0]-1) <= BarsArray[0].GetLow(CurrentBars[0]-2) && BarsArray[0].GetClose(CurrentBars[0]-1) > BarsArray[0].GetOpen(CurrentBars[0]-1))
				{
					// Tín hiệu delta TNR up
					Draw.ArrowUp(this, "mua"+CurrentBar, true, 1, BarsArray[0].GetLow(CurrentBars[0]-1) - 8* TickSize,Brushes.Green,true);
					//Draw.ArrowUp(this, "mua"+CurrentBar, true, 1, BarsArray[0].GetLow(CurrentBars[0]-1) - 8* TickSize,volup_color,true);
					// Thêm vào mảng của delta phân kỳ 
					//BoT.delta_phanky.Add(CurrentBars[0]-1);
					tnrSignal[1] = 1;
					return; // Delta Tunaround ok => done
				}
					
			}else 
			{ // đỏ
				if(deltaList[CurrentBars[0]-2] < 0)
					return;
				if(BarsArray[0].GetHigh(CurrentBars[0]-1) >= BarsArray[0].GetHigh(CurrentBars[0]-2) &&BarsArray[0].GetClose(CurrentBars[0]-1) < BarsArray[0].GetOpen(CurrentBars[0]-1))
				{
					// Tín hiệu delta TNR down
					Draw.ArrowDown(this, "ban"+CurrentBar, true, 1, BarsArray[0].GetHigh(CurrentBars[0]-1) - 8* TickSize,Brushes.Red,true);
					tnrSignal[1] = -1;
					//Draw.ArrowDown(this, "ban"+CurrentBar, true, 1,BarsArray[0].GetHigh(CurrentBars[0]-1) + 2* TickSize,voldow_color,true);
					// BoT.delta_phanky.Add(CurrentBars[0]-1);
					return; // delta tunaround ok done !
				}
			}
			tnrSignal[1] = 0;
			
		}
		
		private int CheckOsb()
		{   // Điều kiện để thỏa mãn OSB : high_1 -high_2 > 0 && low_1-low_2 <0
			if(BarsArray[0].GetHigh(CurrentBars[0]-1) - BarsArray[0].GetHigh(CurrentBars[0]-2)>0 &&  BarsArray[0].GetLow(CurrentBars[0]-2)-BarsArray[0].GetLow(CurrentBars[0]-1) > 0)
			{
				//OSB xanh
				if(BarsArray[0].GetClose(CurrentBars[0]-1) > BarsArray[0].GetOpen(CurrentBars[0]-1))
				{
					return 1;
				}
				// OSB đỏ
				else if(BarsArray[0].GetClose(CurrentBars[0]-1) < BarsArray[0].GetOpen(CurrentBars[0]-1))
				{
					return 2;
				}	
				return -1;
			}
			return -1;
			
		}
		
		
		
		// Phần OBV của MaiDev
		protected override void OnBarUpdate()
		{
			
			//if(CurrentBars[0] <= fastMaStart + 1)
			//{
				if(BarsInProgress == 0 && IsFirstTickOfBar && CurrentBars[0] > 0)
				{
					// Delta 
					deltaList.Add(lastDelta);
					lastDelta = 0.0;
					
					// Tính TRN
					if(CurrentBars[0] >=2 )
					{
						Turnaround();
					}
					// Debug TRN in giá tri sigleTnr
					double position = Low[1] - Math.Abs(Open[1] - Close[1]);
					//Draw.Text(this, "OpenPrice" + CurrentBar, tnrSignal[1].ToString(), 0, position, Brushes.White);
                    Draw.Text(this, "OpenPrice" + CurrentBar,true , tnrSignal[1].ToString(), 1, position,0, Brushes.White, textFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                    // Draw.Text(this, "TrnSigle"+ CurrentBar,true ,1,);
					
					//
					//hlDelta[1] = BarsArray[0].GetHigh(CurrentBars[0]-1) - BarsArray[0].GetLow(CurrentBars[0]-1);
					//calculate high - low 
					double delta = BarsArray[0].GetHigh(CurrentBars[0]-1) - BarsArray[0].GetLow(CurrentBars[0]-1);
					hlDelta.Add(delta);

					
					//calculate obv
					CalculateOBV(ref BarsArray[0], ref obv, CurrentBars[0]-1);

					//calculate smooth (sma of obv), exclude obv index 0 
					double currentSmooth =  CurrentBars[0] > vlen ? CalculateSMA(ref obv, vlen, CurrentBars[0]-1) : 0;
				//	Print("smooth: " + currentSmooth);
					
					//add (current obv - smooth) to array
					double currentOBVSmooth = CurrentBars[0] > vlen ? obv[CurrentBars[0] - 1] - currentSmooth : 0.0;
					obvSmooth.Add(currentOBVSmooth);


					double shadow = 0.0, priceSpread = 0.0, volumeSpread = 0.0, currentOut = 0.0;
					
					if(CurrentBars[0] >= vlen + windowLen)
					{
						//calculate stdev of  (high-low)
						priceSpread = StdDev(ref hlDelta, windowLen, CurrentBars[0] - 1);
						
						//calculate stdev of (obv - smooth) 
						volumeSpread = StdDev(ref obvSmooth, windowLen, CurrentBars[0] - 1);
						
						//calculate shadow = (obv - smooth)/ stdev obvsmooth * stdev highlow
						shadow = currentOBVSmooth / volumeSpread * priceSpread;
						
						//calculate out = shadow > 0 ? high + shadow : low + shadow 
						currentOut = shadow > 0 ? BarsArray[0].GetHigh(CurrentBars[0]-1) + shadow : BarsArray[0].GetLow(CurrentBars[0]-1) + shadow;
						shadowOut.Add(currentOut);
					}
					else
						shadowOut.Add(currentOut);

					
					//calculate ema of out 
					double currentObvEma = CurrentBars[0] > obvEmaStart ? CalculateEMA(ref shadowOut, ref obvEma, obvLength, obvEmaStart, CurrentBars[0]-1) : 0.0;
					obvEma.Add(currentObvEma);
					//Print("obvema: " + currentObvEma);
					
					//calculate slow ma (ema(close, slowLength))
					double currentSlowMa = CurrentBars[0] > slowMaStart ? CalculateEMA(ref BarsArray[0], ref slowMa, SlowLength, slowMaStart, CurrentBars[0]-1) : 0.0;
					slowMa.Add(currentSlowMa);
					//Print("slow ma: " + currentSlowMa);
					
					double currentFastMa = 0.0 , currentMACD = 0.0, currentMacdSum = 0.0;
					if(CurrentBars[0] > fastMaStart)
					{
						//calculate fast ma (ema(obvema, fastLength))
						currentFastMa = CalculateEMA(ref obvEma, ref fastMa, FastLength, fastMaStart, CurrentBars[0]-1);
						fastMa.Add(currentFastMa);
						
						
						//calculate macd
						currentMACD = currentFastMa - slowMa[CurrentBars[0]-1];
						macd.Add(currentMACD);
						
						//calculate processed macd
						if(CurrentBars[0] == fastMaStart + 1)
						{
							mcadSum.Add(currentMacdSum);
							processedMacd[1] = currentMACD;
							Values[0][1] = processedMacd[1];
							signal[1] = 0;
						}
						else
						{
							currentMacdSum = Math.Abs(currentMACD - processedMacd[2]) + mcadSum[CurrentBars[0] - 2];
							mcadSum.Add(currentMacdSum);
							double average = currentMacdSum/(CurrentBars[0] - 1);
							
							processedMacd[1] = (currentMACD > processedMacd[2]+average) || (currentMACD<processedMacd[2]-average) ? currentMACD : processedMacd[2];
							
							//Add plot
							Values[0][1] =processedMacd[1];
							
							if(processedMacd[1] - processedMacd[2] > 0)
							{
								//chu trình xanh
								PlotBrushes[0][1] = Brushes.SkyBlue;
								currentObvMFlag = 1;
								
							}
							else if(processedMacd[1] - processedMacd[2] < 0)
							{
								//chu trình đỏ
								PlotBrushes[0][1] = Brushes.Red;
								currentObvMFlag = -1;
							}
							else
							{
								PlotBrushes[0][1] = PlotBrushes[0][2];
								currentObvMFlag = obvMFlag;	
							}
							//---
							
							if(currentObvMFlag - obvMFlag > 0)
							{
								//Tín hiệu chuyển obv đỏ -> xanh
								signal[1] = 1;
								Draw.ArrowUp(this, (CurrentBars[0]-1).ToString()+ "up", true, 1, High[1] + 5*TickSize, Brushes.Blue);
								obvMFlag = currentObvMFlag;
								
							}
							else if(currentObvMFlag - obvMFlag < 0)
							{
								//Tín hiệu chuyển obv xanh -> đỏ
								signal[1] = -1;
								Draw.ArrowDown(this, (CurrentBars[0]-1).ToString()+ "down", true, 1, Low[1] - 5*TickSize, Brushes.Red);
								obvMFlag = currentObvMFlag;
								
							}
							else
							{
								signal[1] = 0;
							}
							
							
							/*
							signal[1] = processedMacd[1] - processedMacd[2] > 0 ? 1 : processedMacd[1] - processedMacd[2] < 0 ? -1 : signal[2];
							
							if(signal[1] == 1){
								// chu trình xanh
								PlotBrushes[0][1] = Brushes.Cyan;
							}
							else{
								// chu trình đỏ
								PlotBrushes[0][1] = Brushes.DarkRed;
							   
							}
							
							if(signal[1] - signal[2] > 0)
							{
//								// Tín hiệu chuyển obv đỏ -> xanh
								Draw.ArrowUp(this, (CurrentBars[0]-1).ToString()+ "upobv", true, 1, Low[1] -  (High[1]-Low[1])*TickSize, Brushes.Blue);
								
							}
							if(signal[1] - signal[2] < 0)
							{
//								// tín hiệu chuyển ovb xanh -> đỏ.
								Draw.ArrowDown(this, (CurrentBars[0]-1).ToString()+ "downobv", true, 1, High[1] + (High[1]-Low[1])*TickSize, Brushes.Yellow);
								//Print("signal down ");
								
								
							}
							*/
							
						}
					}
					else
					{
						fastMa.Add(currentFastMa);
						macd.Add(currentMACD);
						mcadSum.Add(currentMacdSum);
						processedMacd[1] = 0.0; 
					}
					//Print("fast ma: " + currentFastMa);
					//Print("macd: " + currentMACD);
					//Print("sum macd " + currentMacdSum);
					//Print("macd processed: " + processedMacd[1]);

					
					//
				}
		        // Check kèo ở đây :
				
				
			
			
		}
		//calculate standared deviation
		protected double StdDev(ref List<double> array, int period, int index)
		{
			double sum=0.0;
			for(int i=index;i>index-period;i--)
				sum+=array[i];
			
			double mean = sum/period, sumSquaredDiff = 0.0;
			for(int i=index;i>index-period;i--)
				sumSquaredDiff += (array[i]-mean) * (array[i]-mean);
			return (double)Math.Sqrt(sumSquaredDiff / period);
			// 
		}
		
		//calculate obv
		protected void CalculateOBV(ref Bars barArray, ref List<double> obvArray,int index)
		{
			double currentOBV = 0.0;
			if(index == 0)
			{
				currentOBV = 0.0;
				obvArray.Add(currentOBV);
		//		Print("obv: " + currentOBV);
				return;
			}
			double vol = barArray.GetVolume(index);
			double prevClose = barArray.GetClose(index-1);
			double currentClose = barArray.GetClose(index);
			
			if(currentClose < prevClose)
				currentOBV = obvArray[index - 1] - vol;
			else if(currentClose > prevClose)
				currentOBV = obvArray[index - 1] + vol;
			else
				currentOBV = obvArray[index - 1];
				
			obvArray.Add(currentOBV);
			//Print("obv: " + currentOBV);
		}
		
		//calculate sma
		protected double CalculateSMA(ref List<double> array, int period, int index)
		{
			double sum=0.0;
			if(index < period - 1)
				return 0.0;
			for(int i=index; i>index-period;i--)
				sum += array[i];
			return (double)(sum / period);
		}
		
		//calculate ema
		protected double CalculateEMA(ref List<double> array, ref List<double> emaArray, int period, int start, int index)
		{
			double ema = 0.0;
			if(index == start)
			{
				double sum = 0.0, average=0.0;
				for(int i = index; i > index-period; i--)
					sum += array[i];
				ema = (double)(sum/period);
				return ema;
			}
			
			double alpha = 2.0 / (period + 1.0);
			ema = alpha * array[index] + (1.0 - alpha) * emaArray[index - 1];
			return ema;
		}
		
		//overloaded method CalculateEMA
		protected double CalculateEMA(ref Bars barArray, ref List<double> emaArray, int period, int start, int index)
		{
			double ema = 0.0;
			if(index == start)
			{
				double sum = 0.0, average=0.0;
				for(int i = index; i > index-period; i--)
					sum += barArray.GetClose(i);
				ema = (double)(sum/period);
				return ema;
			}
			
			double alpha = 2.0 / (period + 1.0);
			ema = alpha * barArray.GetClose(index) + (1.0 - alpha) * emaArray[index - 1];
			return ema;
		}
		#region Properties

	
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Fast Length", Order=1, GroupName="Parameters")]
		public int FastLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Slow Length", Order=2, GroupName="Parameters")]
		public int SlowLength
		{ get; set; }
		#endregion
		
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OBVMACD[] cacheOBVMACD;
		public OBVMACD OBVMACD(int fastLength, int slowLength)
		{
			return OBVMACD(Input, fastLength, slowLength);
		}

		public OBVMACD OBVMACD(ISeries<double> input, int fastLength, int slowLength)
		{
			if (cacheOBVMACD != null)
				for (int idx = 0; idx < cacheOBVMACD.Length; idx++)
					if (cacheOBVMACD[idx] != null && cacheOBVMACD[idx].FastLength == fastLength && cacheOBVMACD[idx].SlowLength == slowLength && cacheOBVMACD[idx].EqualsInput(input))
						return cacheOBVMACD[idx];
			return CacheIndicator<OBVMACD>(new OBVMACD(){ FastLength = fastLength, SlowLength = slowLength }, input, ref cacheOBVMACD);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OBVMACD OBVMACD(int fastLength, int slowLength)
		{
			return indicator.OBVMACD(Input, fastLength, slowLength);
		}

		public Indicators.OBVMACD OBVMACD(ISeries<double> input , int fastLength, int slowLength)
		{
			return indicator.OBVMACD(input, fastLength, slowLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OBVMACD OBVMACD(int fastLength, int slowLength)
		{
			return indicator.OBVMACD(Input, fastLength, slowLength);
		}

		public Indicators.OBVMACD OBVMACD(ISeries<double> input , int fastLength, int slowLength)
		{
			return indicator.OBVMACD(input, fastLength, slowLength);
		}
	}
}

#endregion
