using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFirst;

namespace Mannote
{
    class StatisticModel
    {
        SampleContext context;
        List<IncomeParams> sampleValues;
        public DateTime dtFromTime { get; set; }
        public DateTime dtToTime { get; set; }
        public int currentDepartment { get; set; }

        public StatisticModel(int _currentDepartment, DateTime _dtFromTime, DateTime _dtToTime)
        {
            context = new SampleContext();
            currentDepartment = _currentDepartment;
            dtFromTime = _dtFromTime;
            dtToTime = _dtToTime;
        }

        public void SetNewPeriod(DateTime dtFromTime, DateTime dtToTime)
        {
            if (dtFromTime >= dtToTime)
                throw new ArgumentException("Неверно выбран период отчета");
            this.dtFromTime = dtFromTime;
            this.dtToTime = dtToTime;
        }

        private List<IncomeParams> GetSampleValues(int department)
        {
            var trainsPeriod = context.Trains.Where(t => (t.Operations.OrderByDescending(o => o.OperationId).FirstOrDefault().Date >= this.dtFromTime 
                                              && t.Operations.OrderByDescending(o => o.OperationId).FirstOrDefault().Date < this.dtToTime))
                                             .Where(t => t.Operations.OrderByDescending(o => o.OperationId).FirstOrDefault().Code.CodeId == 205
                                             );
            if (department != 0)
                trainsPeriod = trainsPeriod.Where(t => t.Path.DepartureStation.Department == department);
            var DepartmentStat = trainsPeriod.Select(t => new
                                             {
                                                 aWeight = t.Weight,
                                                 aDistance = t.Path.Distance,
                                                 aDeparture = t.Operations.Where(o => o.Code.CodeId == 200)
                                                                            .Select(o => o.Date).FirstOrDefault(),
                                                 aArrival = t.Operations.Where(o => o.Code.CodeId == 201)
                                                                            .Select(o => o.Date).FirstOrDefault(),
                                                 aLokId = t.Lokomotive.LokomotiveId
                                             })
                                             .AsEnumerable()
                                             .Select(an => new IncomeParams
                                             {
                                                 weight = an.aWeight,
                                                 distance = an.aDistance,
                                                 ArrivTime = an.aArrival,
                                                 DepartTime = an.aDeparture,
                                                 lokId = an.aLokId
                                             }).ToList();
            return DepartmentStat;
        }

        private float GetPL()
        {
            float pl = 0;
            foreach (IncomeParams train in sampleValues)
            {
                pl += train.weight * train.distance;
            }
            return pl;
        }

        private float GetSumP()
        {
            return sampleValues.Select(s => s.weight).Sum();
        }

        private float GetAverageTrainWeight()
        {
            if (sampleValues.Count() == 0) return 0;
            return sampleValues.Select(t => t.weight).Average();
        }

        private float GetAverageTrainSpeed()
        {
            foreach(IncomeParams values in sampleValues)
            {
                values.speed = values.distance / (float)(values.ArrivTime - values.DepartTime).TotalHours;
            }
            if (sampleValues.Count() == 0) return 0;
            return sampleValues.Select(s => s.speed).Average();
        }

        private float GetSL()
        {
            int lokCount = sampleValues.Select(s => s.lokId).Distinct().Count();
            if (lokCount == 0) return 0;
            return sampleValues.Select(s => s.distance).Sum() / lokCount;
        }

        public List<StatisticValue> CalculateValues()
        {
            sampleValues = GetSampleValues(currentDepartment);
            List<StatisticValue> statList = new List<StatisticValue>();
            statList.Add(new StatisticValue("Грузооборот эксплуатационный", GetPL()));
            statList.Add(new StatisticValue("Перевезено грузов", GetSumP()));
            statList.Add(new StatisticValue("Участковая скорость", GetAverageTrainSpeed()));
            StatisticValue averageWeight = new StatisticValue("Средний вес поезда", GetAverageTrainWeight());
            statList.Add(averageWeight);
            StatisticValue sl = new StatisticValue("Среднесуточный пробег локомотива", GetSL());
            statList.Add(sl);
            statList.Add(new StatisticValue("Пробег локомотива", sl.value * averageWeight.value));
            return statList;
        }

        public class IncomeParams
        {
            public float weight;
            public int distance;
            public DateTime DepartTime;
            public DateTime ArrivTime;
            public float speed;
            public int lokId;
        }
    }

    public class StatisticValue
    {
        public StatisticValue(string name, float value)
        {
            this.name = name;
            this.value = value;
            plan = 0;
            percentage = 0;
        }
        public string name { get; set; }
        public float value { get; set; }
        public float plan { get; set; }
        public float percentage { get; set; }
    }
}


