#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using System.Collections;
#endregion


// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
	
    /// <summary>
    /// Enter the description of your new custom indicator here
    /// </summary>
    [Description("")]
    public class wgEmaTrend : Indicator
    {				
		
		#region RegInputs
		
        private int iStrength = 3; 
        [Description("")]
        [GridCategory("Parameters")]
		[Gui.Design.DisplayName ("00. Swing Strength")]
        public int Strength
        {
            get { return iStrength; }
            set { iStrength = value; }        	
		}	
		private int iMaPeriod = 7;        				        
        [Description("")]
        [GridCategory("Parameters")]
		[Gui.Design.DisplayName ("01. Period MA1")]
        public int MaPeriod
        {
            get { return iMaPeriod; }
            set { iMaPeriod = value; }
        }	
		private int iMaPeriod2 = 13;        				        
        [Description("")]
        [GridCategory("Parameters")]
		[Gui.Design.DisplayName ("02. Period MA2")]
        public int MaPeriod2
        {
            get { return iMaPeriod2; }
            set { iMaPeriod2 = value; }
        }		
		private Color iUpColor = Color.Green;
        [Description("")]
        [GridCategory("Parameters")]
		[Gui.Design.DisplayName ("03. Up Color")]
        public Color UpColor
        {
            get { return iUpColor; }
            set { iUpColor = value; }
        }
		[Browsable(false)]
		public string UpColorSerialize
		{
			get { return NinjaTrader.Gui.Design.SerializableColor.ToString(iUpColor); }
			set { iUpColor = NinjaTrader.Gui.Design.SerializableColor.FromString(value); }
		}	
				
		private Color iDownColor = Color.Pink;
        [Description("")]
        [GridCategory("Parameters")]
		[Gui.Design.DisplayName ("04. Down Color")]
        public Color DownColor
        {
            get { return iDownColor; }
            set { iDownColor = value; }
        }
		[Browsable(false)]
		public string DownColorSerialize
		{
			get { return NinjaTrader.Gui.Design.SerializableColor.ToString(iDownColor); }
			set { iDownColor = NinjaTrader.Gui.Design.SerializableColor.FromString(value); }
		}	
	
		
		#endregion

		private int lastTrend;
		private DataSeries sMA1, sMA2;
		private DataSeries sSwingH, sSwingL;
		private int bOfCross, bOfCondition1, bOfCondition2, bOfDot, maTrend, dotTrend;
		
        /// <summary> 
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {								
			Add(new Plot(Color.FromKnownColor(KnownColor.DimGray), PlotStyle.Line, "TrendOutput"));
			
//			Add(new Plot(Color.FromKnownColor(KnownColor.Blue), PlotStyle.Line, "MA1"));	
//			Add(new Plot(Color.FromKnownColor(KnownColor.Orange), PlotStyle.Line, "MA2"));	
//			Plots[0].Pen.Width = Plots[1].Pen.Width = 2;
//			
//			Add(new Plot(Color.FromKnownColor(KnownColor.Green), PlotStyle.Dot, "SwingH"));
//			Add(new Plot(Color.FromKnownColor(KnownColor.Red), PlotStyle.Dot, "SwingL"));			
            CalculateOnBarClose	= true;
            Overlay				= false;
            PriceTypeSupported	= false;					

			sSwingL = new DataSeries(this);
			sSwingH = new DataSeries(this);			
			
			sMA1 = new DataSeries(this);
			sMA2 = new DataSeries(this);				
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			if(CurrentBar < Math.Max(this.iMaPeriod, this.iMaPeriod2))	
				return;
			
//			MA1.Set(EMA(this.iMaPeriod)[0]);
//			MA2.Set(EMA(this.iMaPeriod2)[0]);
//			
//			SwingH.Set(SwingN(Bars, this.iStrength).SwingHigh[0]);			
//			SwingL.Set(SwingN(Bars, this.iStrength).SwingLow[0]);	
						
			sMA1.Set(EMA(this.iMaPeriod)[0]);
			sMA2.Set(EMA(this.iMaPeriod2)[0]);
			
			sSwingH.Set(SwingN(Bars, this.iStrength).SwingHigh[0]);			
			sSwingL.Set(SwingN(Bars, this.iStrength).SwingLow[0]);				
			
			#region RegMaTrend
			
			if(CrossAbove(sMA1, sMA2, 1) || CrossBelow(sMA1, sMA2, 1))
			{				
				this.bOfCross = this.bOfCondition1 = this.bOfCondition2 = CurrentBar;
				
				if(sMA1[0] > sMA2[0])
				{
					this.maTrend = 1;
					if(Close[0] > Open[0])
						this.maTrend = 2;
				}
				if(sMA1[0] < sMA2[0])
				{
					this.maTrend = -1;
					if(Close[0] < Open[0])
						this.maTrend = -2;				
				}
			}
			
			if(maTrend > 0)
			{
				if(maTrend == 1 && Close[0] > Open[0])
				{
					if(Close[0] > Close[CurrentBar - this.bOfCross])
					{
						maTrend = 2;
						this.bOfCondition1 = CurrentBar;
					}
				}
				else if(maTrend == 2 && CurrentBar != this.bOfCondition1)
				{
					if(High[0] > Close[CurrentBar - this.bOfCondition1] && Math.Abs(High[0] - Close[CurrentBar - this.bOfCondition1]) >= TickSize && Close[0] > Open[0])
						maTrend = 3;
				}					
			}
			
			if(maTrend < 0)
			{
				if(maTrend == -1 && Close[0] < Open[0])
				{
					if(Close[0] < Close[CurrentBar - this.bOfCross])
					{
						maTrend = -2;
						this.bOfCondition1 = CurrentBar;
					}
				}
				else if(maTrend == -2 && CurrentBar != this.bOfCondition1)
				{
					if(Low[0] < Close[CurrentBar - this.bOfCondition1] && Math.Abs(Low[0] - Close[CurrentBar - this.bOfCondition1]) >= TickSize && Close[0] < Open[0])
						maTrend = -3;
				}					
			}
			
			#endregion
			
			#region RegDotTrend
			
			if(sSwingH[0] != sSwingH[1] || sSwingL[0] != sSwingL[1])
				this.dotTrend = 0;
			
			if(CrossAbove(Close, sSwingH, 1))
			{
				this.dotTrend = 1;	
				this.bOfDot = CurrentBar;
			}
			if(CrossBelow(Close, sSwingL, 1))
			{
				this.dotTrend = -1;
				this.bOfDot = CurrentBar;				
			}
			
			if(this.dotTrend == 1)
			{
				if(Close[0] > Close[CurrentBar - this.bOfDot])
					this.dotTrend = 2;
			}
			
			if(this.dotTrend == -1)
			{
				if(Close[0] < Close[CurrentBar - this.bOfDot])
					this.dotTrend = -2;
			}			
			 
			#endregion			
			
			int trend = 0;
			TrendOutput.Set(TrendOutput[1]);
			if(maTrend == 3 || dotTrend == 2)
			{
				trend = 1;
				BackColor = Color.FromArgb(150, this.iUpColor);
			}
			if(maTrend == -3 || dotTrend == -2)
			{
				trend = -1;
				BackColor = Color.FromArgb(150, this.iDownColor);	
			}	
			
			if(trend != 0)
			{
				this.lastTrend = trend;
				TrendOutput.Set(trend);
			}
			
			if(this.lastTrend == 1)
				BackColor = Color.FromArgb(150, this.iUpColor);
			if(this.lastTrend == -1)
				BackColor = Color.FromArgb(150, this.iDownColor);			
        }
		
		
        #region Properties

        public override string ToString()
        {
            return string.Empty;
        }	
		
		
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries TrendOutput
        {
            get { return Values[0]; }
        }	
		
//        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
//        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
//        public DataSeries MA1
//        {
//            get { return Values[0]; }
//        }				
//        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
//        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
//        public DataSeries MA2
//        {
//            get { return Values[1]; }
//        }	
//		
//        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
//        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
//        public DataSeries SwingH
//        {
//            get { return Values[2]; }
//        }				
//        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
//        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
//        public DataSeries SwingL
//        {
//            get { return Values[3]; }
//        }			
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    public partial class Indicator : IndicatorBase
    {
        private wgEmaTrend[] cachewgEmaTrend = null;

        private static wgEmaTrend checkwgEmaTrend = new wgEmaTrend();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public wgEmaTrend wgEmaTrend(Color downColor, int maPeriod, int maPeriod2, int strength, Color upColor)
        {
            return wgEmaTrend(Input, downColor, maPeriod, maPeriod2, strength, upColor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public wgEmaTrend wgEmaTrend(Data.IDataSeries input, Color downColor, int maPeriod, int maPeriod2, int strength, Color upColor)
        {
            if (cachewgEmaTrend != null)
                for (int idx = 0; idx < cachewgEmaTrend.Length; idx++)
                    if (cachewgEmaTrend[idx].DownColor == downColor && cachewgEmaTrend[idx].MaPeriod == maPeriod && cachewgEmaTrend[idx].MaPeriod2 == maPeriod2 && cachewgEmaTrend[idx].Strength == strength && cachewgEmaTrend[idx].UpColor == upColor && cachewgEmaTrend[idx].EqualsInput(input))
                        return cachewgEmaTrend[idx];

            lock (checkwgEmaTrend)
            {
                checkwgEmaTrend.DownColor = downColor;
                downColor = checkwgEmaTrend.DownColor;
                checkwgEmaTrend.MaPeriod = maPeriod;
                maPeriod = checkwgEmaTrend.MaPeriod;
                checkwgEmaTrend.MaPeriod2 = maPeriod2;
                maPeriod2 = checkwgEmaTrend.MaPeriod2;
                checkwgEmaTrend.Strength = strength;
                strength = checkwgEmaTrend.Strength;
                checkwgEmaTrend.UpColor = upColor;
                upColor = checkwgEmaTrend.UpColor;

                if (cachewgEmaTrend != null)
                    for (int idx = 0; idx < cachewgEmaTrend.Length; idx++)
                        if (cachewgEmaTrend[idx].DownColor == downColor && cachewgEmaTrend[idx].MaPeriod == maPeriod && cachewgEmaTrend[idx].MaPeriod2 == maPeriod2 && cachewgEmaTrend[idx].Strength == strength && cachewgEmaTrend[idx].UpColor == upColor && cachewgEmaTrend[idx].EqualsInput(input))
                            return cachewgEmaTrend[idx];

                wgEmaTrend indicator = new wgEmaTrend();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                indicator.DownColor = downColor;
                indicator.MaPeriod = maPeriod;
                indicator.MaPeriod2 = maPeriod2;
                indicator.Strength = strength;
                indicator.UpColor = upColor;
                Indicators.Add(indicator);
                indicator.SetUp();

                wgEmaTrend[] tmp = new wgEmaTrend[cachewgEmaTrend == null ? 1 : cachewgEmaTrend.Length + 1];
                if (cachewgEmaTrend != null)
                    cachewgEmaTrend.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cachewgEmaTrend = tmp;
                return indicator;
            }
        }
    }
}

// This namespace holds all market analyzer column definitions and is required. Do not change it.
namespace NinjaTrader.MarketAnalyzer
{
    public partial class Column : ColumnBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.wgEmaTrend wgEmaTrend(Color downColor, int maPeriod, int maPeriod2, int strength, Color upColor)
        {
            return _indicator.wgEmaTrend(Input, downColor, maPeriod, maPeriod2, strength, upColor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Indicator.wgEmaTrend wgEmaTrend(Data.IDataSeries input, Color downColor, int maPeriod, int maPeriod2, int strength, Color upColor)
        {
            return _indicator.wgEmaTrend(input, downColor, maPeriod, maPeriod2, strength, upColor);
        }
    }
}

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    public partial class Strategy : StrategyBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.wgEmaTrend wgEmaTrend(Color downColor, int maPeriod, int maPeriod2, int strength, Color upColor)
        {
            return _indicator.wgEmaTrend(Input, downColor, maPeriod, maPeriod2, strength, upColor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Indicator.wgEmaTrend wgEmaTrend(Data.IDataSeries input, Color downColor, int maPeriod, int maPeriod2, int strength, Color upColor)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.wgEmaTrend(input, downColor, maPeriod, maPeriod2, strength, upColor);
        }
    }
}
#endregion
