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
        PlansPeeker planPeeker;
        List <IncomeParams> sampleValues;
        DateTime dtFromTime { get; set; }
        DateTime dtToTime { get; set; }
        List<StatisticValue> statList { get; set; }
        public int currentDepartment { get; set; }
        delegate void saveMode(List<StatisticValue> statisticValues, DateTime monthYear, double k);
        saveMode savePlanMode;

        public StatisticModel()
        {
            context = new SampleContext();
        }

        public DateTime[] GetPeriod()
        {
            return new DateTime[] { dtFromTime, dtToTime };
        }

        public List<StatisticValue> GetStatisticValues()
        {
            return statList;
        }

        // Определение периода для отчета
        public void SetNewPeriod(DateTime dtFromTime, DateTime dtToTime)
        {
            if (dtFromTime >= dtToTime)
                throw new ArgumentException("Дата конца периода не может превышать даты начала отсчета");
            this.dtFromTime = dtFromTime;
            this.dtToTime = dtToTime;
        }

        // Получение исходных значений для расчета показателей
        private List<IncomeParams> GetSampleValues(int department)
        {
            // Выборка расформированных поездов за отчетный период
            var trainsPeriod = context.Trains.Where(t => (t.Operations.OrderByDescending(o => o.OperationId).FirstOrDefault().Date >= this.dtFromTime 
                                              && t.Operations.OrderByDescending(o => o.OperationId).FirstOrDefault().Date < this.dtToTime))
                                             .Where(t => t.Operations.OrderByDescending(o => o.OperationId).FirstOrDefault().Code.CodeId == 205
                                             );
            if (department != 0)
            // Выборка по конкретному отделению
                trainsPeriod = trainsPeriod.Where(t => t.Path.DepartureStation.Department == department);
            // Формирование объектов из выборки значений для расчета
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

        // Расчет грузооборота
        private float GetPL()
        {
            float pl = 0;
            foreach (IncomeParams train in sampleValues)
            {
                pl += train.weight * train.distance;
            }
            return pl;
        }

        // Расчет веса перевезенных грузов
        private float GetSumP()
        {
            return sampleValues.Select(s => s.weight).Sum();
        }

        // Расчет среднего веса поезда
        private float GetAverageTrainWeight()
        {
            if (sampleValues.Count() == 0) return 0;
            return sampleValues.Select(t => t.weight).Average();
        }

        // Расчет средней скорости поездов
        private float GetAverageTrainSpeed()
        {
            foreach(IncomeParams values in sampleValues)
            {
                values.speed = values.distance / (float)(values.ArrivTime - values.DepartTime).TotalHours;
            }
            if (sampleValues.Count() == 0) return 0;
            return sampleValues.Select(s => s.speed).Average();
        }

        // Расчет кол-ва груженых поездов
        private float GetCargoTrainsCount()
        {
            return sampleValues.Where(v => v.weight != 0).Count();
        }

        private float GetSL()
        {
            int lokCount = sampleValues.Select(s => s.lokId).Distinct().Count();
            if (lokCount == 0) return 0;
            return sampleValues.Select(s => s.distance).Sum() / lokCount;
        }

        // Поиск и расчет плановых значений
        public bool SetPlanValues()
        {
            if (statList == null)
                throw new Exception("Плановые показатели не могут быть рассчитаны до определения фактических значений");
            // Период выходит за рамки одного отчетного месяца
            if (dtFromTime.AddDays(1).Month != dtToTime.Month && dtFromTime.Year != dtToTime.Year)
                return false;
            planPeeker = new PlansPeeker();
            try
            {
                float[] planValues = planPeeker.getPlan(dtToTime);

                int duration = (dtToTime - dtFromTime).Days;

                for (int i = 0; i < statList.Count; i++)
                {
                    StatisticValue statisticValue = statList.ElementAt(i);
                    statisticValue.plan = (planValues[i] / (DateTime.DaysInMonth(dtToTime.Year, dtToTime.Month))) * duration;
                    if (statisticValue.value != 0)
                        statisticValue.percentage = (statisticValue.value / statisticValue.plan);
                }
                savePlanMode = planPeeker.updatePlan;
                return true;
            }
            catch(ArgumentException ex)
            {
                savePlanMode = planPeeker.addPlan;
                throw ex;
            }
        }

        public void SavePlanValues()
        {
            if (planPeeker == null) planPeeker = new PlansPeeker();
            double k = (double)DateTime.DaysInMonth(dtToTime.Year, dtToTime.Month) / (dtToTime - dtFromTime).Days;
                //DateTime.DaysInMonth(dtToTime.Year, dtToTime.Month) / (dtToTime - dtFromTime).Days;
            savePlanMode.Invoke(statList, dtToTime, k);
        }

        public void CalculateValues()
        {
            sampleValues = GetSampleValues(currentDepartment);
            statList = new List<StatisticValue>();
            statList.Add(new StatisticValue("Поездов расформировано", sampleValues.Count));
            statList.Add(new StatisticValue("в т.ч. груженых", GetCargoTrainsCount()));
            statList.Add(new StatisticValue("Грузооборот эксплуатационный, т-км", GetPL()));
            statList.Add(new StatisticValue("Перевезено грузов, т", GetSumP()));
            statList.Add(new StatisticValue("Маршрутная скорость, км/ч", GetAverageTrainSpeed()));
            StatisticValue averageWeight = new StatisticValue("Средний вес поезда, т", GetAverageTrainWeight());
            statList.Add(averageWeight);
            StatisticValue sl = new StatisticValue("Среднесуточный пробег локомотива, км", GetSL());
            statList.Add(sl);
            statList.Add(new StatisticValue("Производительность локомотива, т-км", sl.value * averageWeight.value));
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


