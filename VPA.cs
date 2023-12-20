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
	public class VPA : Indicator
	{
		private Series<double> spread;
		private Series<double> volumeSma;
		private Series<double> fiveDaysSma;
		private Series<bool> isUpThrustBar;
		private Series<bool> isPseudoUpThrustBar;
		private Series<bool> isConfirmedUpThrustBar;
		private Series<bool> isNewConfirmedUpThrustBar;
		private Series<bool> bycond1;
		private Series<bool> supplyTestBar;
		private Series<bool> effortUpMoveBar;
		private Series<bool> isUpBar;
		private Series<bool> isDownBar;
		private Series<bool> isWideSpreadBar;
		private Series<bool> isNarrowSpreadBar;
		
		private Brush bandAreaColor = Brushes.Silver;
        private Brush bannerColor = Brushes.White;
        private Brush textColor = Brushes.White;
		
		private double avgSpread;	//Wilders average of Range
		private double avgVolume = 0;

        private double VolSD;
        private double x1;
        private double LongTermTrendSlope;
        private double MiddleTermTrendSlope;
        private double ShortTermTrendSlope;

        private double narrowSpreadFactor = 0.7;
        private double wideSpreadFactor = 1.5;

        private bool upThrustConditionOne;
        private bool upThrustConditionTwo;
        private bool upThrustConditionThree;
        private bool reversalLikelyBar;
        private bool pseudoUpThrustConfirmation;
        private bool weaknessBar;
        private bool strenghtInDownTrend0;
        private bool strenghtInDownTrend;
        private bool strenghtInDownTrend1;
        private bool strenghtInDownTrend2;
        private bool isStrengthConfirmationBar;
        private bool stopVolBar;
        private bool noDemandBar;
        private bool noSupplyBar;
        private bool supplyTestInUpTrendBar;
        private bool successfulSupplyTestBar;
        private bool distributionBar;
        private bool failedEffortUpMove;
        private bool effortDownMove;
        private bool isTwoDaysLowVol;
        private bool isUpCloseBar;
        private bool isMidCloseBar;
        private bool isVeryHighCloseBar;
        private bool isDownCloseBar;
		
		private string bannerstring;
        private string titlestring;
		
		private Dictionary<string, DXMediaMap>	dxmBrushes;
		
		private Brush RegionBrush
		{
			get { return dxmBrushes["RegionBrush"].MediaBrush; }
			set
			{
				dxmBrushes["RegionBrush"].MediaBrush = value;
				SetOpacity("RegionBrush");
			}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Volume Spread Analysis";
				Name										= "VPA";
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
				
				VolumeEmaAve								= 30;
				DwWideSpread								= 1.5;
				DwNarrowSpread								= 0.7;
				DwHighClose									= 0.7;
				DwLowClose									= 0.25;
				DwUltraHighVol								= 2;
				DwAboveAvgVol								= 1.5;
				Banners										= true;
				Alertss										= false;
				BandAreaColor								= Brushes.Silver;
				ColorBarToTrend								= false;
				BandAreaColorOpacity						= 50;
				Font 										= new SimpleFont("Arial", 12);
				
				AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Line, "VolumeEma");
				AddPlot(new Stroke(Brushes.LimeGreen, 5), PlotStyle.Bar, "Strong");
				AddPlot(new Stroke(Brushes.Red, 5), PlotStyle.Bar, "Weak");
				AddPlot(new Stroke(Brushes.Yellow, 5), PlotStyle.Bar, "StrongWeak");
				
				Plots[1].AutoWidth = Plots[2].AutoWidth = Plots[3].AutoWidth = true;
				
				dxmBrushes = new Dictionary<string, DXMediaMap>();

				foreach (string brushName in new string[] { "RegionBrush" })
					dxmBrushes.Add(brushName, new DXMediaMap());
				
				RegionBrush = Plots[0].Brush;
			}
			else if (State == State.DataLoaded)
			{				
				spread 						= new Series<double>(this);
				volumeSma 					= new Series<double>(this);
				fiveDaysSma 				= new Series<double>(this);
				isUpThrustBar 				= new Series<bool>(this);
				isPseudoUpThrustBar 		= new Series<bool>(this);
				isConfirmedUpThrustBar 		= new Series<bool>(this);
				isNewConfirmedUpThrustBar 	= new Series<bool>(this);
				bycond1 					= new Series<bool>(this);
				supplyTestBar 				= new Series<bool>(this);
				effortUpMoveBar 			= new Series<bool>(this);
				isUpBar 					= new Series<bool>(this);
				isDownBar 					= new Series<bool>(this);
				isWideSpreadBar 			= new Series<bool>(this);
				isNarrowSpreadBar 			= new Series<bool>(this);
			}
		}

		protected override void OnBarUpdate()
        {
            if (CurrentBar < VolumeEmaAve) return;

            spread[0] = (High[0] - Low[0]);
            avgVolume = EMA(Volume, VolumeEmaAve)[0];
            VolumeEma[0] = (avgVolume);

            // Calculate Volume moving average and it's standard deviation
            volumeSma[0] = (SMA(Volume, VolumeEmaAve)[0]);
            VolSD = StdDev(volumeSma, VolumeEmaAve)[0];

            // check if the vloume has been decreasing in the past two days.
            isTwoDaysLowVol = (Volume[0] < Volume[1] && Volume[0] < Volume[2]);

            // Calculate Range information
            avgSpread = (Wilder(spread, VolumeEmaAve)[0]);
            isWideSpreadBar[0] = (spread[0] > (DwWideSpread * avgSpread));
            isNarrowSpreadBar[0] = (spread[0] < (DwNarrowSpread * avgSpread));

            // Price information
            isUpBar[0] = (Close[0] > Close[1]);
            isDownBar[0] = (Close[0] < Close[1]);

            // Check if the close is in the Highs/Lows/Middle of the bar.
            x1 = (Close[0] - Low[0] == 0.00 ? avgSpread : (spread[0] / (Close[0] - Low[0])));
            isUpCloseBar = (x1 < 2);
            isDownCloseBar = (x1 > 2);
            isMidCloseBar = (x1 < 2.2 && x1 > 1.8);
            isVeryHighCloseBar = (x1 < 1.35);

            // Trend Definitions
            fiveDaysSma[0] = (SMA(Close, 5)[0]);
            LongTermTrendSlope = LinRegSlopeSFX(fiveDaysSma, 40, false, 1)[0];
            MiddleTermTrendSlope = LinRegSlopeSFX(fiveDaysSma, 15, false, 1)[0];
            ShortTermTrendSlope = LinRegSlopeSFX(fiveDaysSma, 5, false, 1)[0];

            // VSA Definitions
			
			// utbar
            isUpThrustBar[0] = (isWideSpreadBar[0] && isDownCloseBar && ShortTermTrendSlope > 0);
            // utcond1
			upThrustConditionOne = (isUpThrustBar[1] && isDownBar[0]);
            // utcond2
			upThrustConditionTwo = (isUpThrustBar[1] && isDownBar[0] && Volume[0] > Volume[1]);
            // utcond3
			upThrustConditionThree = (isUpThrustBar[0] && Volume[0] > 2 * volumeSma[0]);
            // scond1
			isConfirmedUpThrustBar[0] = (upThrustConditionOne || upThrustConditionTwo || upThrustConditionThree);
            // scond
			isNewConfirmedUpThrustBar[0] = (isConfirmedUpThrustBar[0] && !isConfirmedUpThrustBar[1]);

            // trbar
            reversalLikelyBar = (Volume[1] > volumeSma[0] && isUpBar[1] && isWideSpreadBar[1] && isDownBar[0] && isDownCloseBar && isWideSpreadBar[0] && LongTermTrendSlope > 0 && High[0] == MAX(High, 10)[0]);
            
            //hutbar
            isPseudoUpThrustBar[0] = (isUpBar[1] && (Volume[1] > DwAboveAvgVol * volumeSma[0]) && isDownBar[0] && isDownCloseBar && !isWideSpreadBar[0] && !isUpThrustBar[0]);
            //hutcond
            pseudoUpThrustConfirmation = (isPseudoUpThrustBar[1] && isDownBar[0] && isDownCloseBar && !isUpThrustBar[0]);
  
            // tcbar
            weaknessBar = (isUpBar[1] && High[0] == MAX(High, 5)[0] && isDownBar[0] && (isDownCloseBar || isMidCloseBar) && Volume[0] > volumeSma[0] && !isWideSpreadBar[0] && !isPseudoUpThrustBar[0]);

            // stdn, stdn0, stdn1, stdn2
            strenghtInDownTrend =  (Volume[0] > Volume[1] && isDownBar[1] && isUpBar[0] && (isUpCloseBar || isMidCloseBar) && ShortTermTrendSlope < 0 && MiddleTermTrendSlope < 0);
            strenghtInDownTrend0 = (Volume[0] > Volume[1] && isDownBar[1] && isUpBar[0] && (isUpCloseBar || isMidCloseBar) && ShortTermTrendSlope < 0 && MiddleTermTrendSlope < 0 && LongTermTrendSlope < 0);
            strenghtInDownTrend1 = (Volume[0] > (volumeSma[0] * DwAboveAvgVol) && isDownBar[1] && isUpBar[0] && (isUpCloseBar || isMidCloseBar) && ShortTermTrendSlope < 0 && MiddleTermTrendSlope < 0 && LongTermTrendSlope < 0);
            strenghtInDownTrend2 = (Volume[1] < volumeSma[0] && isUpBar[0] && isVeryHighCloseBar && Volume[0] > volumeSma[0] && ShortTermTrendSlope < 0);
            
            bycond1[0] = (strenghtInDownTrend || strenghtInDownTrend1);
            // bycond
			isStrengthConfirmationBar = (isUpBar[0] && bycond1[1]);

            // stvol
            stopVolBar = (Low[0] == MIN(Low, 5)[0] && (isUpCloseBar || isMidCloseBar) && Volume[0] > DwAboveAvgVol * volumeSma[0] && LongTermTrendSlope < 0);
			
			// ndbar, nsbar
            noDemandBar = (isUpBar[0] && isNarrowSpreadBar[0] && isTwoDaysLowVol && isDownCloseBar); 
            noSupplyBar = (isDownBar[0] && isNarrowSpreadBar[0] && isTwoDaysLowVol && isDownCloseBar);

            // lvtbar, lvtbar1, lvtbar2
            supplyTestBar[0] = (isTwoDaysLowVol && Low[0] < Low[1] && isUpCloseBar);
            supplyTestInUpTrendBar = (Volume[0] < volumeSma[0] && Low[0] < Low[1] && isUpCloseBar && LongTermTrendSlope > 0 && MiddleTermTrendSlope > 0 && isWideSpreadBar[0]);
            successfulSupplyTestBar = (supplyTestBar[1] && isUpBar[0] && isUpCloseBar);
			
			// dbar
            distributionBar = (Volume[0] > DwUltraHighVol * volumeSma[0] && isDownCloseBar && isUpBar[0] && ShortTermTrendSlope > 0 && MiddleTermTrendSlope > 0 && !isConfirmedUpThrustBar[0] && !isUpThrustBar[0]);

            // eftup, eftupfl, eftdn
            effortUpMoveBar[0] = (High[0] > High[1] && Low[0] > Low[1] && Close[0] > Close[1] && Close[0] >= ((High[0] - Low[0]) * DwHighClose + Low[0]) && spread[0] > avgSpread && Volume[0] > Volume[1]);
            failedEffortUpMove = (effortUpMoveBar[1] && (isUpThrustBar[0] || upThrustConditionOne || upThrustConditionTwo || upThrustConditionThree));
            effortDownMove = (High[0] < High[1] && Low[0] < Low[1] && Close[0] < Close[1] && Close[0] <= ((High[0] - Low[0]) * DwLowClose + Low[0]) && spread[0] > avgSpread && Volume[0] > Volume[1]);

            #region Candle Definition
			if (ColorBarToTrend)
			{
				if(ShortTermTrendSlope>0 && MiddleTermTrendSlope>0 && LongTermTrendSlope>0)
					{
						BarBrush = Brushes.Lime;
					}
				else if(ShortTermTrendSlope>0 && MiddleTermTrendSlope>0 && LongTermTrendSlope<0)
					{
						BarBrush = Brushes.Green;
					}
				else if(ShortTermTrendSlope>0 && MiddleTermTrendSlope<0 && LongTermTrendSlope<0)
					{
						BarBrush = Brushes.PaleGreen;
					}
				else if(ShortTermTrendSlope<0 && MiddleTermTrendSlope<0 && LongTermTrendSlope<0)
					{
						BarBrush = Brushes.Red;
					}
				else if(ShortTermTrendSlope<0 && MiddleTermTrendSlope>0 && LongTermTrendSlope>0)
					{
						BarBrush = Brushes.PaleGreen;
					}
					
				else if(ShortTermTrendSlope<0 && MiddleTermTrendSlope<0 && LongTermTrendSlope>0)
					{
						BarBrush = Brushes.Orange;
					}	
					
				else 
					{
						BarBrush = Brushes.Yellow;
					}
			}
            #endregion

            #region Volume Bar Definition

            if (upThrustConditionThree)
                Weak[0] = (Volume[0]);
            else if (upThrustConditionTwo)
                Weak[0] = (Volume[0]);
            else if (isUpThrustBar[0])
                Weak[0] = (Volume[0]);
            else if (strenghtInDownTrend)
                Strong[0] = (Volume[0]);
            else if (strenghtInDownTrend0)
                Strong[0] = (Volume[0]);
            else if (strenghtInDownTrend1)
                Strong[0] = (Volume[0]);
            else if (strenghtInDownTrend2)
                Strong[0] = (Volume[0]);
            else
                StrongWeak[0] = (Volume[0]);
            #endregion

            #region Title Banners

            if (isUpThrustBar[0])
            {
                titlestring = " An Upthrust Bar. A sign of weakness. ";

                textColor = Brushes.Yellow;
                bannerColor = Brushes.Red;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
            }

            if (upThrustConditionOne)
            {
                titlestring = " A downbar after an Upthrust. Confirm weakness. ";

                textColor = Brushes.White;
                bannerColor = Brushes.Lime;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }
            if (upThrustConditionTwo && !upThrustConditionOne)
            {
                titlestring = " A High Volume downbar after an Upthrust. Confirm weakness.";

                textColor = Brushes.White;
                bannerColor = Brushes.Lime;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }

            if (upThrustConditionThree)
            {
                titlestring = "This upthrust at very High Voume, Confirms weakness";

                textColor = Brushes.Yellow;
                bannerColor = Brushes.Blue;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
            }

            if (strenghtInDownTrend1)
            {
                titlestring = "Strength seen returning after a down trend. High volume adds to strength. ";

                textColor = Brushes.Black;
                bannerColor = Brushes.Yellow;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }

            if (strenghtInDownTrend0 && !strenghtInDownTrend)
            {
                titlestring = "Strength seen returning after a down trend. ";

                textColor = Brushes.Red;
                bannerColor = Brushes.Lime;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }

            if (strenghtInDownTrend && !strenghtInDownTrend1)
            {
                titlestring = "Strength seen returning after a long down trend. ";

                textColor = Brushes.Red;
                bannerColor = Brushes.Lime;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }

            if (supplyTestBar[0])
            {
                titlestring = "Test for supply. ";

                textColor = Brushes.Blue;
                bannerColor = Brushes.Aqua;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }

            if (successfulSupplyTestBar)
            {
                titlestring = "An Upbar closing near High after a Test confirms strength. ";

                textColor = Brushes.Blue;
                bannerColor = Brushes.Aqua;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }

            if (isStrengthConfirmationBar)
            {
                titlestring = "An Upbar closing near High. Confirms return of Strength. ";

                textColor = Brushes.Blue;
                bannerColor = Brushes.Lime;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }

            if (distributionBar)
            {
                titlestring = "A High Volume Up Bar closing down in a uptrend shows Distribution. ";

                textColor = Brushes.Blue;
                bannerColor = Brushes.Yellow;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }

            if (isPseudoUpThrustBar[0])
            {
                titlestring = "Psuedo UpThrust.   A Sign of Weakness. ";

                textColor = Brushes.White;
                bannerColor = Brushes.Lime;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }
            if (pseudoUpThrustConfirmation)
            {
                titlestring = "A Down Bar closing down after a Pseudo Upthrust confirms weakness. ";

                textColor = Brushes.Yellow;
                bannerColor = Brushes.Blue;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }
            if (supplyTestInUpTrendBar)
            {
                titlestring = "Test for supply in a uptrend. Sign of Strength. ";

                textColor = Brushes.Yellow;
                bannerColor = Brushes.Blue;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }

            if (strenghtInDownTrend2)
            {
                titlestring = "High volume upbar closing on the high indicates strength. ";

                textColor = Brushes.Black;
                bannerColor = Brushes.Yellow;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }

            if (weaknessBar)
            {
                titlestring = "High volume Downbar after an upmove on high volume indicates weakness. ";

                textColor = Brushes.Yellow;
                bannerColor = Brushes.Blue;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }
            if (noDemandBar)
            {
                titlestring = "No Demand. A sign of Weakness. ";

                textColor = Brushes.White;
                bannerColor = Brushes.Yellow;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }

            if (noSupplyBar)
            {
                titlestring = "No Supply. A sign of Strength. ";

                textColor = Brushes.White;
                bannerColor = Brushes.Lime;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }

            if (effortUpMoveBar[0])
            {
                titlestring = "Effort to Rise. Bullish sign ";

                textColor = Brushes.Blue;
                bannerColor = Brushes.Aquamarine;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }
            if (effortDownMove)
            {
                titlestring = "Effort to Fall. Bearish sign ";

                textColor = Brushes.Red;
                bannerColor = Brushes.Blue;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }

            if (failedEffortUpMove)
            {
                titlestring = "Effort to Move up has failed. Bearish sign ";

                textColor = Brushes.Red;
                bannerColor = Brushes.Blue;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }

            if (stopVolBar)
            {
                titlestring = "Stopping volume. Normally indicates end of bearishness is nearing. ";

                textColor = Brushes.Blue;
                bannerColor = Brushes.Turquoise;

                if (Alertss)
                    Alert("myAlert" + CurrentBar, Priority.High, titlestring, "Alert1.wav", 10, bannerColor, Brushes.Yellow);
            }


            if (Banners)
                Draw.TextFixed(this, "Banner1", titlestring, TextPosition.TopLeft, Brushes.Black, Font, Brushes.White, Brushes.White, 10);
            else
                RemoveDrawObject("Banner1");


            #endregion

            #region Message Banners

            if (isUpThrustBar[0])
            {
                bannerstring = "Up-thrusts are designed to catch stops and " + "\n" +
                "to mislead as many traders as possible.They are normally " + "\n" +
                "seen after there has been weakness in the background. The market" + "\n" +
                "makers know that the market is weak, so the price is marked up to " + "\n" +
                "catch stops, encourage traders to go long in a weak market,AND panic" + "\n" +
                "traders that are already Short into covering their very good position.";

                textColor = Brushes.Yellow;
                bannerColor = Brushes.Red;
            }
            if (upThrustConditionThree)
            {
                bannerstring = "This upthrust bar is at high volume." + "\n" +
                "This is a sure sign of weakness. One may even seriously" + "\n" +
                "consider ending the Longs AND be ready to reverse";

                textColor = Brushes.Yellow;
                bannerColor = Brushes.Blue;
            }
            if (isUpThrustBar[0] || upThrustConditionThree)
            {
                bannerstring = "Also note that A wide spread down-bar" + "\n" +
                "that appears immediately after any up-thrust, tends to " + "\n" +
                "confirm the weakness (the market makers are locking in traders" + "\n" +
                "into poor positions). With the appearance of an upthrust you " + "\n" +
                "should certainly be paying attention to your trade AND your stops." + "\n" +
                "On many upthrusts you will find that the market will 'test' " + "\n" +
                "almost immediately.";

                textColor = Brushes.Yellow;
                bannerColor = Brushes.Red;
            }
            if (upThrustConditionOne)
            {
                bannerstring = "A wide spread down bar following a Upthrust Bar." + "\n" +
                "This confirms weakness. The Smart Money is locking in Traders " + "\n" +
                "into poor positions";

                textColor = Brushes.White;
                bannerColor = Brushes.Lime;
            }
            if (upThrustConditionTwo)
            {
                bannerstring = "Also here the volume is high(Above Average)." + "\n" +
                "This is a sure sign of weakness. The Smart Money is locking in" + "\n" +
                "Traders into poor positions";

                textColor = Brushes.White;
                bannerColor = Brushes.Lime;
            }
            if (strenghtInDownTrend)
            {
                bannerstring = "Strength Bar. The stock has been in a down Trend." + "\n" +
                "An upbar with higher Volume closing near the High is a sign of " + "\n" +
                "strength returning. The downtrend is likely to reverse soon.";

                textColor = Brushes.Red;
                bannerColor = Brushes.Lime;
            }
            if (strenghtInDownTrend1)
            {
                bannerstring = "Here the volume is very much above average." + "\n" +
                "This makes this indication more stronger.";

                textColor = Brushes.Black;
                bannerColor = Brushes.Yellow;
            }
            if (isStrengthConfirmationBar)
            {
                bannerstring = "The previous bar saw strength coming back. " + "\n" +
                "This upbar confirms strength.";

                textColor = Brushes.Blue;
                bannerColor = Brushes.Lime;
            }
            if (isPseudoUpThrustBar[0])
            {
                bannerstring = "A pseudo Upthrust. This normally appears after an" + "\n" +
                "Up Bar with above average volume. This looks like an upthrust bar " + "\n" +
                "closing down near the Low. But the Volume is normally Lower than average." + "\n" +
                "this is a sign of weakness.If the Volume is High then weakness increases." + "\n" +
                "Smart Money is trying to trap the retailers into bad position.";

                textColor = Brushes.White;
                bannerColor = Brushes.Lime;
            }
            if (pseudoUpThrustConfirmation)
            {
                bannerstring = "A downbar after a pseudo Upthrust Confirms weakness. " + "\n" +
                "If the volume is above average the weakness is increased.";

                textColor = Brushes.Yellow;
                bannerColor = Brushes.Blue;
            }
            if (supplyTestInUpTrendBar)
            {
                bannerstring = "The previous bar was a successful Test of supply." + "\n" +
                "The current bar is a upbar with higher volume. This confirms strength";

                textColor = Brushes.Yellow;
                bannerColor = Brushes.Blue;
            }
            if (distributionBar)
            {
                bannerstring = "A wide range, high volume bar in a up trend " + "\n" +
                "closing down is an indication the Distribution is in progress. " + "\n" +
                "The smart money is Selling the stock to the late Comers rushing to " + "\n" +
                "Buy the stock NOT to be Left Out Of a Bullish move.";

                textColor = Brushes.Blue;
                bannerColor = Brushes.Yellow;
            }
            if (successfulSupplyTestBar)
            {
                bannerstring = "The previous bar was a successful Test of" + "\n" +
                "supply. The current bar is a upbar with higher volume. This " + "\n" +
                "confirms strength";

                textColor = Brushes.Blue;
                bannerColor = Brushes.Aqua;
            }
            if (weaknessBar)
            {
                bannerstring = "The stock has been moving up on high volume." + "\n" +
                "The current bar is a Downbar with high volume. Indicates weakness" + "\n" +
                "and probably end of the up move";

                textColor = Brushes.Yellow;
                bannerColor = Brushes.Blue;
            }
            if (effortUpMoveBar[0])
            {
                bannerstring = "Effort to Rise bar. This normally " + "\n" +
                "found in the beginning of a Markup Phase and is bullish" + "\n" +
                "sign.These may be found at the top of an Upmove as the " + "\n" +
                "Smart money makes a last effort to move the price to the maximum";

                textColor = Brushes.Blue;
                bannerColor = Brushes.Aquamarine;
            }
            if (effortDownMove)
            {
                bannerstring = "Effort to Fall bar. This normally" + "\n" +
                "found in the beginning of a Markdown phase.";

                textColor = Brushes.Red;
                bannerColor = Brushes.Blue;
            }
            if (noSupplyBar)
            {
                bannerstring = "No Supply. A no supply bar indicates" + "\n" +
                "supply has been removed and the Smart money can markup the" + "\n" +
                "price. It is better to wait for confirmation";

                textColor = Brushes.White;
                bannerColor = Brushes.Lime;
            }

            if (stopVolBar)
            {
                bannerstring = "Stopping Volume. This will be " + "\n" +
                "an downbar during a bearish period closing towards" + "\n" +
                "the Top accompanied by High volume. A stopping Volume " + "\n" +
                "normally indicates that smart money is absorbing the " + "\n" +
                "supply which is a Indication that they are Bullishon " + "\n" +
                "the MArket.Hence we Can expect a reversal in the down trend.";

                textColor = Brushes.Blue;
                bannerColor = Brushes.Turquoise;
            }

            if (noDemandBar)
            {
                bannerstring = "No Demand Brief Description: " + "\n" +
                "Any up bar which closes in the middle OR Low," + "\n" +
                "especially if the Volume has fallen off, is a " + "\n" +
                "potential sign of weakness. Things to Look Out for: " + "\n" +
                "if the market is still strong, you will normally see " + "\n" +
                "signs of strength in the next few bars, which will most " + "\n" +
                "probably show itself as a: Down bar with a narrow spread," + "\n" +
                " closing in the middle OR High. * Down bar on Low Volume.";

                textColor = Brushes.White;
                bannerColor = Brushes.Yellow;
            }

            if (Banners)
                Draw.TextFixed(this, "Banner2", bannerstring, TextPosition.BottomLeft, Brushes.Black, Font, Brushes.White, Brushes.White, 10);
            else
                RemoveDrawObject("Banner2");

            #endregion

            #region Trend Text Definition

            string textMsg1 = "Vol:";
            if (Volume[0] > (volumeSma[0] + 2.0 * VolSD))
                textMsg1 = textMsg1 + " Very High";
            else if (Volume[0] > (volumeSma[0] + 1.0 * VolSD))
                textMsg1 = textMsg1 + " High";
            else if (Volume[0] > volumeSma[0])
                textMsg1 = textMsg1 + " Above Average";
            else if (Volume[0] < volumeSma[0] && Volume[0] > (volumeSma[0] - 1.0 * VolSD))
                textMsg1 = textMsg1 + " Less Than Average";
            else if (Volume[0] < (volumeSma[0] - 1.0 * VolSD))
                textMsg1 = textMsg1 + " Low";
            else textMsg1 = textMsg1 + " ";


            textMsg1 = textMsg1 + "\nSpread:";
            if (Range()[0] > (avgSpread * 2.0))
                textMsg1 = textMsg1 + " Wide";
            else if (Range()[0] > avgSpread)
                textMsg1 = textMsg1 + " Above Average";
            else
                textMsg1 = textMsg1 + " Narrow";


            textMsg1 = textMsg1 + "\nClose:";
            if (isVeryHighCloseBar)
                textMsg1 = textMsg1 + " Very High";
            else if (isUpCloseBar)
                textMsg1 = textMsg1 + " High";
            else if (isMidCloseBar)
                textMsg1 = textMsg1 + " Mid";
            else if (isDownCloseBar)
                textMsg1 = textMsg1 + " Down";
            else
                textMsg1 = textMsg1 + "Very Low";

            string textMsg2 = "Trend:";
            if (ShortTermTrendSlope > 0)
                textMsg2 = textMsg2 + "  Shrt Trm-Up";
            else
                textMsg2 = textMsg2 + "  Shrt Trm-Down";

            if (MiddleTermTrendSlope > 0)
                textMsg2 = textMsg2 + "\nMid Trm-Up";
            else
                textMsg2 = textMsg2 + "\nMid Trm-Down";
            if (LongTermTrendSlope > 0)
                textMsg2 = textMsg2 + "  Lng Trm-Up";
            else
                textMsg2 = textMsg2 + "  Lng Trm-Down";
            Draw.TextFixed(this, "Msg2", textMsg1 + "\n" + textMsg2, TextPosition.BottomRight, Brushes.Black, Font, Brushes.White, Brushes.White, 10);
            #endregion

            #region Shapes

			// IF YOU CHANGE THE SHAPES/COLORS PLEASE UPDATE THE LEGEND SECTION AT THE TOP.
			
            if (isUpThrustBar[0] && !isNewConfirmedUpThrustBar[0])
            {
                Draw.Square(this, "MySquare" + CurrentBar, true, 0, High[0] + 2 * TickSize, Brushes.Red);
            }
            if (reversalLikelyBar)
            {
                //Change Small Circle from original code to Diamond
                Draw.Diamond(this, "MyDiamond" + CurrentBar, true, 0, High[0] + 2 * TickSize, Brushes.Blue);
            }
            if (isNewConfirmedUpThrustBar[0])
            {
                Draw.TriangleDown(this, "MyTriangleDown" + CurrentBar, true, 0, High[0] + 2 * TickSize, Brushes.Red);
            }
            if (strenghtInDownTrend)
            {
                Draw.Square(this, "MySquare" + CurrentBar, true, 0, Low[0] - 2 * TickSize, Brushes.Lime);
            }
            if (strenghtInDownTrend1)
            {
                Draw.Square(this, "MySquare" + CurrentBar, true, 0, Low[0] - 2 * TickSize, Brushes.Lime);
            }
            if (supplyTestInUpTrendBar)
            {
                Draw.Square(this, "MySquare" + CurrentBar, true, 0, Low[0] - 2 * TickSize, Brushes.Lime);
            }
            if (successfulSupplyTestBar)
            {
                Draw.TriangleUp(this, "MyTriangleUp" + CurrentBar, true, 0, Low[0] - 2 * TickSize, Brushes.Yellow);
            }
            if (stopVolBar)
            {
                //Change Small Circle from original code to Diamond
                Draw.Diamond(this, "MyDiamond" + CurrentBar, true, 0, Low[0] - 2 * TickSize, Brushes.Lime);
            }
            if (isStrengthConfirmationBar)
            {
                Draw.TriangleUp(this, "MyTriangleUp" + CurrentBar, true, 0, Low[0] - 2 * TickSize, Brushes.Lime);
            }
            if (isPseudoUpThrustBar[0])
            {
                Draw.Square(this, "MySquare" + CurrentBar, true, 0, High[0] + 2 * TickSize, Brushes.Blue);
            }
            if (pseudoUpThrustConfirmation)
            {
                Draw.TriangleDown(this, "MyTriangleDown" + CurrentBar, true, 0, High[0] + 2 * TickSize, Brushes.Blue);
            }
            if (weaknessBar)
            {
                Draw.TriangleDown(this, "MyTriangleDown" + CurrentBar, true, 0, High[0] + 2 * TickSize, Brushes.Yellow);
            }
            if (strenghtInDownTrend2)
            {
                Draw.TriangleUp(this, "MyTriangleUp" + CurrentBar, true, 0, Low[0] - 2 * TickSize, Brushes.Aqua);
            }
            if (distributionBar)
            {
                Draw.Square(this, "MySquare" + CurrentBar, true, 0, High[0] + 2 * TickSize, Brushes.Blue);
            }
            if (supplyTestBar[0])
            {
                Draw.Square(this, "MySquare" + CurrentBar, true, 0, Low[0] - 2 * TickSize, Brushes.DeepPink);
            }
            if (noDemandBar)
            {
                Draw.Square(this, "MySquare" + CurrentBar, true, 0, High[0] + 2 * TickSize, Brushes.Blue);
            }
            if (noSupplyBar)
            {
                Draw.Diamond(this, "MyDiamond" + CurrentBar, true, 0, Low[0] - 2 * TickSize, Brushes.Lime);
            }

            if (effortUpMoveBar[0])
            {
                Draw.Diamond(this, "MyDiamond" + CurrentBar, true, 0, Median[0], Brushes.Turquoise);
            }

            if (effortDownMove)
            {
                Draw.Diamond(this, "MyDiamond" + CurrentBar, true, 0, Median[0], Brushes.Yellow);
            }


            #endregion
        }
		
		public override void OnRenderTargetChanged()
		{
			// Dispose and recreate our DX Brushes
			try
			{
				foreach (KeyValuePair<string, DXMediaMap> item in dxmBrushes)
				{
					if (item.Value.DxBrush != null)
						item.Value.DxBrush.Dispose();

					if (RenderTarget != null)
						item.Value.DxBrush = item.Value.MediaBrush.ToDxBrush(RenderTarget);					
				}
			}
			catch (Exception exception)
			{
				Log(exception.ToString(), LogLevel.Error);
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			// Call base OnRender() method to paint defined Plots.
			base.OnRender(chartControl, chartScale);
			
			// Store previous AA mode
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode 	= RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode 							= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			
			// Draw Region between VolumeEma and Zero
			DrawRegionFromZero(chartScale, VolumeEma, "RegionBrush", Displacement);
			
			// Reset AA mode.
			RenderTarget.AntialiasMode = oldAntialiasMode;
		}		
		
		#region SharpDX Helper Classes & Methods
		
		private void SetOpacity(string brushName)
		{
			if (dxmBrushes[brushName].MediaBrush == null)
				return;

			if (dxmBrushes[brushName].MediaBrush.IsFrozen)
				dxmBrushes[brushName].MediaBrush = dxmBrushes[brushName].MediaBrush.Clone();

			dxmBrushes[brushName].MediaBrush.Opacity = BandAreaColorOpacity / 100.0;
			dxmBrushes[brushName].MediaBrush.Freeze();
		}
				
		private class DXMediaMap
		{
			public SharpDX.Direct2D1.Brush		DxBrush;
			public System.Windows.Media.Brush	MediaBrush;
		}

		private class SharpDXFigure
		{
			public SharpDX.Vector2[] Points;
			public string Color;
			
			public SharpDXFigure(SharpDX.Vector2[] points, string color)
			{
				Points = points;
				Color = color;
			}
		}
		
		private void DrawFigure(SharpDXFigure figure)
		{
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);	
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(figure.Points[0], new SharpDX.Direct2D1.FigureBegin());
			
			for (int i = 0; i < figure.Points.Length; i++)
				sink.AddLine(figure.Points[i]);
				
			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink.Close();
			
            RenderTarget.FillGeometry(geometry, dxmBrushes[figure.Color].DxBrush);
			RenderTarget.DrawGeometry(geometry, Plots[0].BrushDX);
			geometry.Dispose();
			sink.Dispose();
		}
		
		private void DrawRegionFromZero(ChartScale chartScale, Series<double> inputSeries, string BrushName, int displacement)
		{	
			List<SharpDX.Vector2> 	SeriesPoints 	= new List<SharpDX.Vector2>();
			List<SharpDX.Vector2> 	tmpPoints 		= new List<SharpDX.Vector2>();
			List<SharpDXFigure>		SharpDXFigures	= new List<SharpDXFigure>();
			
			// Convert Series to points
			int start 	= ChartBars.FromIndex-displacement > 0 ? ChartBars.FromIndex-displacement : 0;
			int end 	= ChartBars.ToIndex;
			for (int barIndex = start; barIndex <= end; barIndex++)
		    {
				if(inputSeries.IsValidDataPointAt(barIndex))
				{
					SeriesPoints.Add(new SharpDX.Vector2((float)ChartControl.GetXByBarIndex(ChartBars, barIndex+displacement), (float)chartScale.GetYByValue(inputSeries.GetValueAt(barIndex))));
				}
		    }
			
			for (int i = 0; i < SeriesPoints.Count; i++)
			{
				tmpPoints.Add(SeriesPoints[i]);
				
				if (i == SeriesPoints.Count -1)
					tmpPoints.Add(new SharpDX.Vector2((float)ChartControl.GetXByBarIndex(ChartBars, end), (float)ChartPanel.Y + ChartPanel.H));
			}
			
			for (int i = SeriesPoints.Count -1; i >= 0; i--)
			{
				tmpPoints.Add(new SharpDX.Vector2(SeriesPoints[i].X, (float)ChartPanel.Y + ChartPanel.H));
				
				if (i == 0)
					tmpPoints.Add(SeriesPoints[i]);
			}
			
			SharpDXFigure figure = new SharpDXFigure(tmpPoints.ToArray(), BrushName);
			DrawFigure(figure);
		}
		#endregion

		#region Properties
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="VolumeEmaAve", Description="Periods Volume EMA Average", Order=1, GroupName="Parameters")]
		public int VolumeEmaAve
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="DwWideSpread", Description="Wide Spread Factor", Order=2, GroupName="Parameters")]
		public double DwWideSpread
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="DwNarrowSpread", Description="Narrow Spread Factor", Order=3, GroupName="Parameters")]
		public double DwNarrowSpread
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="DwHighClose", Description="High Close Factor", Order=4, GroupName="Parameters")]
		public double DwHighClose
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="DwLowClose", Description="Low Close Factor", Order=5, GroupName="Parameters")]
		public double DwLowClose
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="DwUltraHighVol", Description="Ultra HighVol Factor", Order=6, GroupName="Parameters")]
		public double DwUltraHighVol
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="DwAboveAvgVol", Description="Above AvgVol Factor", Order=7, GroupName="Parameters")]
		public double DwAboveAvgVol
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Banners", Description="Show Banners", Order=8, GroupName="Parameters")]
		public bool Banners
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Alertss", Description="Show Alerts", Order=9, GroupName="Parameters")]
		public bool Alertss
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="BandAreaColor", Description="Band Color", Order=10, GroupName="Parameters")]
		public Brush BandAreaColor
		{ get; set; }

		[Browsable(false)]
		public string BandAreaColorSerializable
		{
			get { return Serialize.BrushToString(BandAreaColor); }
			set { BandAreaColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="ColorBarToTrend", Description="Change Bars color according to trend", Order=11, GroupName="Parameters")]
		public bool ColorBarToTrend
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BandAreaColorOpacity", Description="Band Color Opacity", Order=12, GroupName="Parameters")]
		public double BandAreaColorOpacity
		{ get; set; }
		
		[Display(Name="Font", Description="Message Font", Order=13, GroupName="Parameters")]
		public SimpleFont Font { get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> VolumeEma
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Strong
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Weak
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> StrongWeak
		{
			get { return Values[3]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private VPA[] cacheVPA;
		public VPA VPA(int volumeEmaAve, double dwWideSpread, double dwNarrowSpread, double dwHighClose, double dwLowClose, double dwUltraHighVol, double dwAboveAvgVol, bool banners, bool alertss, Brush bandAreaColor, bool colorBarToTrend, double bandAreaColorOpacity)
		{
			return VPA(Input, volumeEmaAve, dwWideSpread, dwNarrowSpread, dwHighClose, dwLowClose, dwUltraHighVol, dwAboveAvgVol, banners, alertss, bandAreaColor, colorBarToTrend, bandAreaColorOpacity);
		}

		public VPA VPA(ISeries<double> input, int volumeEmaAve, double dwWideSpread, double dwNarrowSpread, double dwHighClose, double dwLowClose, double dwUltraHighVol, double dwAboveAvgVol, bool banners, bool alertss, Brush bandAreaColor, bool colorBarToTrend, double bandAreaColorOpacity)
		{
			if (cacheVPA != null)
				for (int idx = 0; idx < cacheVPA.Length; idx++)
					if (cacheVPA[idx] != null && cacheVPA[idx].VolumeEmaAve == volumeEmaAve && cacheVPA[idx].DwWideSpread == dwWideSpread && cacheVPA[idx].DwNarrowSpread == dwNarrowSpread && cacheVPA[idx].DwHighClose == dwHighClose && cacheVPA[idx].DwLowClose == dwLowClose && cacheVPA[idx].DwUltraHighVol == dwUltraHighVol && cacheVPA[idx].DwAboveAvgVol == dwAboveAvgVol && cacheVPA[idx].Banners == banners && cacheVPA[idx].Alertss == alertss && cacheVPA[idx].BandAreaColor == bandAreaColor && cacheVPA[idx].ColorBarToTrend == colorBarToTrend && cacheVPA[idx].BandAreaColorOpacity == bandAreaColorOpacity && cacheVPA[idx].EqualsInput(input))
						return cacheVPA[idx];
			return CacheIndicator<VPA>(new VPA(){ VolumeEmaAve = volumeEmaAve, DwWideSpread = dwWideSpread, DwNarrowSpread = dwNarrowSpread, DwHighClose = dwHighClose, DwLowClose = dwLowClose, DwUltraHighVol = dwUltraHighVol, DwAboveAvgVol = dwAboveAvgVol, Banners = banners, Alertss = alertss, BandAreaColor = bandAreaColor, ColorBarToTrend = colorBarToTrend, BandAreaColorOpacity = bandAreaColorOpacity }, input, ref cacheVPA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VPA VPA(int volumeEmaAve, double dwWideSpread, double dwNarrowSpread, double dwHighClose, double dwLowClose, double dwUltraHighVol, double dwAboveAvgVol, bool banners, bool alertss, Brush bandAreaColor, bool colorBarToTrend, double bandAreaColorOpacity)
		{
			return indicator.VPA(Input, volumeEmaAve, dwWideSpread, dwNarrowSpread, dwHighClose, dwLowClose, dwUltraHighVol, dwAboveAvgVol, banners, alertss, bandAreaColor, colorBarToTrend, bandAreaColorOpacity);
		}

		public Indicators.VPA VPA(ISeries<double> input , int volumeEmaAve, double dwWideSpread, double dwNarrowSpread, double dwHighClose, double dwLowClose, double dwUltraHighVol, double dwAboveAvgVol, bool banners, bool alertss, Brush bandAreaColor, bool colorBarToTrend, double bandAreaColorOpacity)
		{
			return indicator.VPA(input, volumeEmaAve, dwWideSpread, dwNarrowSpread, dwHighClose, dwLowClose, dwUltraHighVol, dwAboveAvgVol, banners, alertss, bandAreaColor, colorBarToTrend, bandAreaColorOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VPA VPA(int volumeEmaAve, double dwWideSpread, double dwNarrowSpread, double dwHighClose, double dwLowClose, double dwUltraHighVol, double dwAboveAvgVol, bool banners, bool alertss, Brush bandAreaColor, bool colorBarToTrend, double bandAreaColorOpacity)
		{
			return indicator.VPA(Input, volumeEmaAve, dwWideSpread, dwNarrowSpread, dwHighClose, dwLowClose, dwUltraHighVol, dwAboveAvgVol, banners, alertss, bandAreaColor, colorBarToTrend, bandAreaColorOpacity);
		}

		public Indicators.VPA VPA(ISeries<double> input , int volumeEmaAve, double dwWideSpread, double dwNarrowSpread, double dwHighClose, double dwLowClose, double dwUltraHighVol, double dwAboveAvgVol, bool banners, bool alertss, Brush bandAreaColor, bool colorBarToTrend, double bandAreaColorOpacity)
		{
			return indicator.VPA(input, volumeEmaAve, dwWideSpread, dwNarrowSpread, dwHighClose, dwLowClose, dwUltraHighVol, dwAboveAvgVol, banners, alertss, bandAreaColor, colorBarToTrend, bandAreaColorOpacity);
		}
	}
}

#endregion
