using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes_Modbus_3
{
    public class Machine
    {
        public Machine()
        {

        }

        private IddleReason _currentIddleReason;


        // Static values
        #region Static Values
        ///<summary>
        ///The Name will be displaying during the ANDON
        ///</summary>
        public string Name;
        ///<summary>
        ///The amount of Parts that are expected to be Produced during the Shift.
        ///</summary>
        public int ExpectedParts;

        public string ShiftStart;
        public string ShiftEnd;

        public int PlannedDownTime;

        public string IPAddress;

        #endregion

        public bool ModbusConnection;

        //Dynamic values
        ///<summary>
        ///The amount of minutes on an Ideal CycleTime and is calculated: 
        ///(TotalShift - PlannedDowntime) / ExpectedParts
        ///</summary>
        public float CycleTime
        {
            get
            {
                if (ExpectedParts > 0)
                {
                    // Console.WriteLine("Cycle Time: " + (TotalShift - PlannedDownTime - UnproductiveMinutes) + " / " + ExpectedParts);
                    return ((TotalShift - OnGoingTime - UnproductiveMinutes) / ExpectedParts) * 100; // Less Unproductive Minutes
                }
                return 0;
            }
        }
        ///<summary>
        ///Amount of minutes from a BreakDown or Worker Iddle.
        ///</summary>    
        // [NonSerialized]
        public float UnproductiveMinutes
        {
            get
            {
                float currentMinutes = (float)(DateTime.Now - DateTime.Parse(ShiftStart)).TotalMinutes;
                //Console.WriteLine("Unproductive minutes: " + IddleWorker);
                float unproductiveMinutes = 0;
                if (currentMinutes > 0)
                {
                    if (IddleWorker)
                    {
                        DateTime d = IddleTime.Max();
                        TimeSpan tempU = DateTime.Now - d;
                        unproductiveMinutes += (float)tempU.TotalMinutes;
                        //Console.WriteLine("Temp " + tempU.ToString());
                    }
                    if (unproductiveTimes.Any())
                    {
                        foreach (UnproductiveTime ut in unproductiveTimes)
                        {
                            unproductiveMinutes += (float)ut.TotalMinutes;
                        }


                    }
                    // Console.WriteLine(Name + " Unproductive time: " + unproductiveMinutes);

                    return unproductiveMinutes;
                }
                return 0;
            }
        }
        ///<summary>
        ///True if machine date is between ShiftStart and ShiftEnd.
        ///</summary>     
        public bool Offline { get { return !((DateTime.Now <= DateTime.Parse(ShiftEnd)) && (DateTime.Now >= DateTime.Parse(ShiftStart))); } }

        ///<summary>
        ///The total of parts produced during the Shift.
        ///</summary>
        public int CurrentParts;
        ///<summary>
        ///The total of minutes of Production, NOT considering downtimes.
        ///</summary>
        public float TotalShift
        {
            get
            {
                return (float)(DateTime.Parse(ShiftEnd) - DateTime.Parse(ShiftStart)).TotalMinutes;
                // return 0;
            }
        }
        ///<summary>
        ///The total of minutes of Production that are Planned during the Shift less the PlannedDowntime.
        ///</summary>
        public int PlannedProductionTime;

        ///<summary>
        ///The total of minutes of Production less unexpected downtime, unplanned stops or planned stops.
        ///</summary>
        public int RunTime;
        ///<summary>
        ///This is triggered whenever the worker is not in station, when unproductive minutes is ongoing.
        ///</summary>
        public bool IddleWorker;

        
        public Machine.IddleReason CurrentIddleReason {
            get
            {
                if (unproductiveTimes.Any())
                {
                    foreach(UnproductiveTime ut in unproductiveTimes)
                    {
                        if (ut.OnGoing)
                        {
                            return ut.Reason;
                        }
                    }
                }
                return IddleReason.Unexpected;
            }
        }

        ///<summary>
        ///The total of minutes of Production from the Start Shift to DateTime.Now.
        ///</summary>
        public int ElapsedMinutes => (int)(DateTime.Now - DateTime.Parse(ShiftStart)).TotalMinutes;

        public bool IsOnShift => (DateTime.Now > DateTime.Parse(ShiftStart)) && (DateTime.Now < DateTime.Parse(ShiftEnd));

        ///<summary>
        ///The total of Minutes elapsed from ShiftStart to the first produced piece.
        ///</summary>
        public float OnGoingTime
        {
            get
            {
                if (TimeStamps.Any())
                {
                    return (float)(TimeStamps.Min() - DateTime.Parse(ShiftStart)).TotalMinutes;
                }
                else return 0;
            }
        }
        ///<summary>
        ///The CurrentCycleTime is total of minutes that is taking the Machine in Production.
        ///</summary>
        public float CurrentCycleTime;


        // Dynamic values
        ///<summary>
        ///Performance is calculated from (%)  currentCycleTime / IdealCycleTime
        ///</summary>
        public int Availability;
        ///<summary>
        ///Performance is calculated from (%)  currentCycleTime / IdealCycleTime
        ///</summary>
        public int Performance;
        public int OEE;


        // private string[] timeStamps;
        //private List<string> timeStamps = new List<string>();
        public List<DateTime> TimeStamps = new List<DateTime>();
        public List<TimeSpan> CycleSpans = new List<TimeSpan>();

        [NonSerialized]
        private List<DateTime> IddleTime = new List<DateTime>();
        [NonSerialized]
        private Dictionary<DateTime, TimeSpan> IddleCollection = new Dictionary<DateTime, TimeSpan>();

        public enum IddleReason : int
        {
            Unexpected = 0,
            Break = 1,
            ChangeOverTime = 2,
            Trials = 3,
            Spare = 4
        }



        [NonSerialized]
        ///<summary>
        ///A list of objects that are produced when worker is Iddle.
        ///</summary>
        public List<UnproductiveTime> unproductiveTimes = new List<UnproductiveTime>();
        //public Dictionary<DateTime, UnproductiveTime> UnexpectedDowntimes = new Dictionary<DateTime, UnproductiveTime>();


        private bool CurrentCoilStatus;
        //private int CurrentPerformance;
        // the during the CoilStatus state change
        public bool CoilStatus
        {
            get { return CurrentCoilStatus; }
            set
            {
                if (value && IddleWorker)
                {
                    // Console.WriteLine("Iddle : " + iddleSpan.ToString());
                    DateTime iddleTimestamp = TimeStamps.Max();

                    // DateTime virtualTimestamp =lastIddleTime();
                    // Console.WriteLine("" +Machines[c].Name + " : " + iddleTimestamp.ToString() + " - " + virtualTimestamp.ToString());
                    TimeSpan span = DateTime.Now - iddleTimestamp;

                    FinishIddleTime(DateTime.Now, span);
                }
                if (value != CoilStatus)
                {
                    if (value)
                    {

                        // IddleWorker = false;

                        CurrentParts++;

                        WriteLog(DateTime.Now);
                        if (TimeStamps.Any())
                        {
                            CurrentCycleTime = (float)(DateTime.Now - TimeStamps.Max()).TotalSeconds;

                        }

                        TimeStamps.Add(DateTime.Now);
                        //TimeStamps.Sort((x, y) => x.CompareTo(y));
                        //foreach(var s in TimeStamps)
                        //{
                        //    Console.WriteLine(s.ToString());
                        //}
                        //Console.WriteLine(Name + " - " + CurrentParts.ToString() + " - " + TimeStamps.Count);

                        // var list = dateList.OrderBy(x => x.TimeOfDay).ToList(); 
                        //TimeStamps.OrderBy(x => x.TimeOfDay);


                    }
                }
                CurrentCoilStatus = value;
            }
        }


        public bool WorkerIddle
        {
            get { return _workerIddle; }
            set
            {
                if (value && !_workerIddle) { StartIddleTime(DateTime.Now, IddleReason.Unexpected); }
                if (!value && _workerIddle) { FinishIddleTime(DateTime.Now, IddleReason.Unexpected); }

                _workerIddle = value;
            }
        }
        #region Button Coils

        public bool BreakCoil
        {
            get { return _breakCoil; }
            set
            {
                if (value && !_breakCoil) { StartIddleTime(DateTime.Now, IddleReason.Break); }
                if (!value && _breakCoil) { FinishIddleTime(DateTime.Now, IddleReason.Break); }
                _breakCoil = value;
            }
        }
        public bool ChangeOverTimeCoil
        {
            get { return _changeOverTimeCoil; }
            set
            {
                if (value && !_changeOverTimeCoil) { StartIddleTime(DateTime.Now, IddleReason.ChangeOverTime); }
                if (!value && _changeOverTimeCoil) { FinishIddleTime(DateTime.Now, IddleReason.ChangeOverTime); }
                _changeOverTimeCoil = value;

            }
        }
        public bool TrialCoil
        {
            get { return _trialCoil; }
            set
            {
                if (value && !_trialCoil) { StartIddleTime(DateTime.Now, IddleReason.Trials); }
                if (!value && _trialCoil) { FinishIddleTime(DateTime.Now, IddleReason.Trials); }

                _trialCoil = value;
            }
        }
        public bool SpareCoil
        {
            get { return _spareCoil; }
            set
            {
                if (value && !_spareCoil) { StartIddleTime(DateTime.Now, IddleReason.Spare); }
                if (!value && _spareCoil) { FinishIddleTime(DateTime.Now, IddleReason.Spare); }

                _spareCoil = value;
            }
        }

        bool _workerIddle;
        bool _breakCoil;
        bool _changeOverTimeCoil;
        bool _trialCoil;
        bool _spareCoil;
        #endregion
        ///<summary>
        ///It is used to get CurrentCycleTime and update TimeStamps and CycleSpans
        ///</summary>
        public void WriteLog(DateTime time)
        {
            /*
             * Add timestamp to TimestampArray, it is used to calculate the Performance metric.
             * To calcute the performance is ppMinute * 100 / CycleTime
             * ppMinute is a minute / CurrentCycleTime
             * TimeStamps is a list used to set the current time to a piece
             * CycleSpans is a list of TimeSpan used to get the amount of time it took from the last piece to the current one.
             */
            //timeStamps.Add(time.ToString("yyyyMMddHHmmssfff"));
            // TimeStamps.Add(time);
            // Console.WriteLine("TimeStamp: "+TimeStamps.Count.ToString()+" "+time.ToString("yyyyMMddHHmmssfff"));


            if (TimeStamps.Count > 2)
            {
                // TODO: Implementation when Cycle Time is 0 or less than 0.
                try
                {
                    TimeSpan currentTimeSpan = TimeStamps.Max() - TimeStamps[TimeStamps.Count - 2];

                    DateTime latest = TimeStamps.Max();

                    //Console.WriteLine("Current TimeSpan: " + currentTimeSpan);

                    CycleSpans.Add(currentTimeSpan);

                    if (CycleTime <= 0) { Console.WriteLine("Cycle Time is missing"); return; }

                    // CurrentCycleTime = (float)currentTimeSpan.TotalSeconds;//(decimal)TimeSpan.FromMinutes(1).Ticks/ (decimal)currentTimeSpan.Ticks;


                }
                catch (System.ArgumentOutOfRangeException e)
                {
                    Console.WriteLine("Arg: " + e.Message);
                    throw;
                }



            }

        }

        ///<summary>
        ///It is used to get Performance and CurrentCycleTime and update TimeStamps and CycleSpans
        ///</summary>
        ///
        public void CalculatePerformance()
        {
            if (TimeStamps.Any())
            {


                int currentParts = TimeStamps.Count - 1;
                int currentSpan = CycleSpans.Count - 1;

                if (CycleTime <= 0) { return; }

                int lastMinuteParts = 0;

                //  Performance = (int)((lastMinute.Count * 100) / (60 / CycleTime))
                for (int i = currentParts; i > 0; i--)
                {

                    // Create a 2 dimension array to consider TimeStamps by minute.
                    ///<summary>
                    ///This Span represents the lookup time to calculate metrics span = Now - TimeStamp.ElementAt(i) 
                    ///</summary>
                    TimeSpan spanMax = TimeSpan.FromMinutes(1);
                    // int   = TimeStamps.IndexOf(TimeStamps.Last());
                    DateTime date = TimeStamps.ElementAt(i); // Date
                    TimeSpan span = DateTime.Now - date;
                    // for(DateTime d = TimeStamps.ElementAt(i); DateTime.Now - d <= spanMax; times )
                    List<DateTime> lastMinute = new List<DateTime>();

                    if (DateTime.Now - date <= spanMax)
                    {
                        lastMinuteParts++;
                    }

                    //List<DateTime> ReversedStamps = TimeStamps;
                    //ReversedStamps.Reverse();
                    //foreach(DateTime d in ReversedStamps)
                    //{
                    //    Console.WriteLine("BOOL " + span+ (span <=spanMax));
                    //    if(DateTime.Now - d <= spanMax)
                    //    {
                    //        Console.WriteLine(Name + " Span: " + span.ToString());
                    //        lastMinute.Add(d);
                    //        Console.WriteLine("Last minute: " + lastMinute.Count + " / " + lastMinute.Last());
                    //    }
                    //}
                    //if (lastMinute.Any())
                    //{
                    // Console.WriteLine("Last minute: " + lastMinuteParts + " / " + CycleTime);


                    //if (span > spanMax)
                    //{
                    //    // Console.WriteLine(Name + " Span lenth: " + span.ToString() + "/" + spanMax.ToString());

                    //    // Get the neares date in List according to spanMax: https://stackoverflow.com/questions/1757136/find-the-closest-time-from-a-list-of-times
                    //    //Console.WriteLine("This is the sampling time: " + i.ToString() + "/"+limit.ToString() +" - "+ TimeStamps.ElementAt(i).ToString());
                    //    List<DateTime> ts = TimeStamps.GetRange(i, TimeStamps.IndexOf(TimeStamps.Last()) - i);
                    //    List<TimeSpan> lastMinuteCycle = CycleSpans.GetRange(i, CycleSpans.Count - i);
                    //    int realParts = currentParts;
                    //    if (lastMinuteCycle.Any())
                    //    {
                    //        double idealCycleTime = ExpectedParts / TotalShift; // Amount of pieces produced in one minute

                    //        // TimeSpan IdealCycleTime = TimeSpan.FromMinutes(idealCycleTime);

                    //        // Console.WriteLine("Real parts: " + CurrentCycleTime.ToString() + " / " + idealCycleTime + " = " + Performance);

                    //        // Performance = (int)((60/CurrentCycleTime) * 100/(60/idealCycleTime));

                    //        double realCycleTime = currentParts;
                    //        double avgCycle = lastMinuteCycle.Average(x => x.Ticks);
                    //        // TimeSpan avgCycleTime = TimeSpan.FromTicks(avgCycle);
                    //        //decimal average = ts.Average();
                    //        TimeSpan avg = TimeSpan.FromTicks((long)avgCycle);
                    //        // Console.WriteLine("Sampling data: " + Name + " - " + t.ToString());
                    //        //Console.WriteLine("Average Cycle Time: " + avg + " = " + avgCycle.ToString());
                    //        //Console.WriteLine("Avg: " + avg.TotalSeconds + " /" + ((decimal)CycleTime*60).ToString());
                    //        //Console.WriteLine();

                    //        // Performance = (int)((decimal)(avg.TotalSeconds * 100) / CycleTime);

                    //        break;
                    //    }

                    //}
                    //Console.WriteLine("Date: " + date.ToString() + " - " + DateTime.Now);
                }
                Performance = (int)((lastMinuteParts * 100) / (60 / CycleTime));

                // int rf = Properties.Settings.Default.ReadingFrequency;
                // TimeStamps.Count* rf;
                // int bf = Properties.Settings.Default.fre

                // Calculate performance AMinuteCycleTime / IdealCycleTime
                // Performance = (int)((CurrentCycleTime * 100) / CycleTime);

                // Console.WriteLine("Performance: " + Performance);
                // Console.WriteLine(@"{0} = {1} / {2}: ", Performance, ppMinute.ToString(), CycleTime);

                // Get the average of ALL timespans.
                //double doubleAverageTicks = CycleSpans.Average(timeSpan => timeSpan.Ticks);
                //long longAverageTicks = Convert.ToInt64(doubleAverageTicks);

                //TimeSpan average = new TimeSpan(longAverageTicks);
                //Console.WriteLine("Average: " + average.ToString());
            }
        }

        public void CalculateAvailability()
        {
            // Consider OnGoingTime
            //Console.WriteLine("Ongoing time: "+ Name + " : " + OnGoingTime.ToString());
            //Console.WriteLine("Total Shift  UnproductiveMinutes - OngoingTime");
            //Console.WriteLine("" + TotalShift + " - " + UnproductiveMinutes + " - "+OnGoingTime + " = " + (TotalShift-UnproductiveMinutes-OnGoingTime));
            //Console.WriteLine("Total Shift  PlannedDowntime - OngoingTime");
            //Console.WriteLine("" + TotalShift + " - " + PlannedDownTime + " - " + OnGoingTime + " = " + (TotalShift - PlannedDownTime - OnGoingTime));

            //Console.WriteLine();

            if (OnGoingTime + PlannedDownTime >= TotalShift)
            {
                Availability = 0;
            }
            else
            {
                Availability = (int)Math.Ceiling((TotalShift - UnproductiveMinutes - PlannedDownTime - OnGoingTime) * 100 / (TotalShift - PlannedDownTime - OnGoingTime)); // TotalShift-PlannedDowntime

            }
        }

        ///<summary>
        ///Registers the amount of t
        ///</summary>
        public void StartIddleTime(DateTime time)
        {
            if (!IddleWorker) // Might be redundant
            {
                IddleTime.Add(time);
                // IddleCollection.Add(time, )
                // Console.WriteLine(TimeStamps.IndexOf(time).ToString() );
                // Console.WriteLine(Name+" Start iddle Time: " + time.ToString());
                // Console.WriteLine("Now vs Iddle: " + DateTime.Now.ToString() + " - " + time.ToString());
                IddleWorker = true;
            }


        }
        public void StartIddleTime(DateTime time, IddleReason reason)
        {

            Console.WriteLine("Start Iddle Time: " + Name + ": " + reason.ToString());
            UnproductiveTime up = new UnproductiveTime(time, reason);
            unproductiveTimes.Add(up);
            IddleWorker = true;
        }
        ///<summary>
        ///Stores time and span into a Dictionary called IddleCollection .
        ///</summary>
        public void FinishIddleTime(DateTime time, TimeSpan span)
        {
            // Console.WriteLine("Finish iddle: ");
            //ArgumentException
            if (IddleWorker)
            {
                try
                {
                    if (IddleTime.Any())
                    {
                        UnproductiveTime unproductive = new UnproductiveTime(IddleTime.Last(), span);
                        unproductiveTimes.Add(unproductive);
                        //Console.WriteLine("Unproductive: " + unproductive.TotalMinutes);
                        //Console.WriteLine(unproductive.UnproductiveSpan.ToString());
                    }

                    // Console.WriteLine("Finish Iddle: " + time.ToString() + " - " + span.ToString());

                    // IddleCollection.Add(time, span);
                }
                catch (ArgumentException)
                {

                    throw;
                }


            }

            IddleWorker = false;

        }

        public void FinishIddleTime(DateTime time, IddleReason reason)
        {
            Console.WriteLine("Finish iddle time" + time.ToString());
            Console.WriteLine(unproductiveTimes.Count.ToString());
            foreach (UnproductiveTime up in unproductiveTimes)
            {
                Console.WriteLine("UP: " + up.StartDate.ToString());
                Console.WriteLine("" + up.Reason + " - " + up.OnGoing);
                if (up.OnGoing && up.Reason == reason)
                {
                    TimeSpan ts = up.Finish(time);
                    Console.WriteLine(up.EndDate.ToString());

                    Console.WriteLine(ts.ToString());
                }
            }
        }

        public class UnproductiveTime
        {
            private DateTime startDate;
            private DateTime endDate;
            private double totalMinutes;
            private TimeSpan timeSpan;

            public bool OnGoing;

            public DateTime StartDate { get { return startDate; } }
            public DateTime EndDate { get { return endDate; } }
            public TimeSpan UnproductiveSpan { get { return timeSpan; } }
            public IddleReason Reason;

            //public TimeSpan ProductiveTime;
            //public TimeSpan UnproductiveTime;

            public double TotalMinutes { get { return totalMinutes; } }

            public UnproductiveTime()
            {

            }

            public UnproductiveTime(DateTime start, DateTime end)
            {
                if (start > end)
                {
                    Console.WriteLine("Shift must be positive");

                }

                startDate = start;
                endDate = end;
                totalMinutes = (end - start).TotalMinutes;
                OnGoing = false;
            }
            public UnproductiveTime(DateTime shiftStart, TimeSpan span)
            {
                if (span <= TimeSpan.Zero)
                {
                    Console.WriteLine("Span is 0");
                    //throw new Exception("Start DateTime is no early than End DateTime.");
                }

                timeSpan = span;
                startDate = shiftStart;
                endDate = startDate + span;
                totalMinutes = span.TotalMinutes;
                OnGoing = false;
            }

            public UnproductiveTime(DateTime shiftStart, IddleReason reason)
            {
                startDate = shiftStart;
                Reason = reason;
                // endDate = DateTime.Now;
                OnGoing = true;
            }

            public TimeSpan Finish(DateTime end)
            {
                endDate = end;
                OnGoing = false;
                return end - StartDate;
            }

            public bool IsUnproductive(DateTime date)
            {
                if (StartDate < date && EndDate > date)
                {
                    return true;
                }
                return false;
            }

            public bool WithinRange(DateTime fromDate, DateTime toDate)
            {
                //return dateToCheck >= startDate && dateToCheck < endDate;

                return fromDate <= StartDate && toDate >= EndDate;
            }
        }
    }
}
