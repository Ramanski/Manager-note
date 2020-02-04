using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Mannote
{
    class PlansPeeker
    {
        string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
            @"\plans.xml";
        XmlDocument xDoc;

        public PlansPeeker()
        {
            FileInfo fileInfo = new FileInfo(path);
            if(!fileInfo.Exists)
            {
                throw new Exception($"Не найден документ с плановыми значениями {Path.GetFullPath(path)}");
            }
            else
            {
                xDoc = new XmlDocument();
                xDoc.Load(path);
            }
        }

        public float[] getPlan(DateTime monthYear)
        {
            float [] planValues = new float [8];
            int i = 0;
            string searchAttribute = monthYear.Month.ToString() + monthYear.Year.ToString();
            XmlElement xRoot = xDoc.DocumentElement;
            foreach(XmlNode xnode in xRoot)
            {
                if (xnode.Attributes.GetNamedItem("period").Value.Equals(searchAttribute))
                {
                    foreach (XmlNode childnode in xnode)
                    {
                        float val;
                        float.TryParse(childnode.InnerText, out val);
                        planValues[i++] = val;
                    }
                    return planValues;
                }
            }
            throw new ArgumentException("Не найдено плановых значений за отчетный месяц. Для полного расчета необходимо их установить.");
        }

        public void updatePlan(List<StatisticValue> statisticValues, DateTime monthYear, double k)
        {
            string attributeName = monthYear.Month.ToString() + monthYear.Year.ToString();
            XmlElement xRoot = xDoc.DocumentElement;

            foreach (XmlNode xnode in xRoot)
            {
                if (xnode.Attributes.GetNamedItem("period").Value.Equals(attributeName))
                {
                    int i = 0;
                    foreach (XmlNode childnode in xnode)
                    {
                        childnode.InnerText = (statisticValues.ElementAt(i++).plan * k).ToString();
                    }
                    xDoc.Save(path);
                    return;
                }
            }
            throw new ArgumentException("Не удалось найти план для обновления.");
        }

        public void addPlan(List<StatisticValue> statisticValues, DateTime monthYear, double k)
        {
            string attributeName = monthYear.Month.ToString() + monthYear.Year.ToString();
            XmlElement xRoot = xDoc.DocumentElement;
            XmlElement planValues = xDoc.CreateElement("planValues");
            XmlAttribute period = xDoc.CreateAttribute("period");
            XmlElement trainCount = xDoc.CreateElement("train");
            XmlElement cargoTrainCount = xDoc.CreateElement("cargoTrain");
            XmlElement pl = xDoc.CreateElement("pl");
            XmlElement sumP = xDoc.CreateElement("sumP");
            XmlElement avSpeed = xDoc.CreateElement("avSpeed");
            XmlElement avWeight = xDoc.CreateElement("avWeight");
            XmlElement sl = xDoc.CreateElement("sl");
            XmlElement wl = xDoc.CreateElement("wl");

            period.AppendChild(xDoc.CreateTextNode(attributeName));
            trainCount.AppendChild(xDoc.CreateTextNode((statisticValues.ElementAt(0).plan * k).ToString()));
            cargoTrainCount.AppendChild(xDoc.CreateTextNode((statisticValues.ElementAt(1).plan * k).ToString()));
            pl.AppendChild(xDoc.CreateTextNode((statisticValues.ElementAt(2).plan * k).ToString()));
            sumP.AppendChild(xDoc.CreateTextNode((statisticValues.ElementAt(3).plan * k).ToString()));
            avSpeed.AppendChild(xDoc.CreateTextNode((statisticValues.ElementAt(4).plan * k).ToString()));
            avWeight.AppendChild(xDoc.CreateTextNode((statisticValues.ElementAt(5).plan * k).ToString()));
            sl.AppendChild(xDoc.CreateTextNode((statisticValues.ElementAt(6).plan * k).ToString()));
            wl.AppendChild(xDoc.CreateTextNode((statisticValues.ElementAt(7).plan * k).ToString()));

            planValues.Attributes.Append(period);
            planValues.AppendChild(trainCount);
            planValues.AppendChild(cargoTrainCount);
            planValues.AppendChild(pl);
            planValues.AppendChild(sumP);
            planValues.AppendChild(avSpeed);
            planValues.AppendChild(avWeight);
            planValues.AppendChild(sl);
            planValues.AppendChild(wl);
            xRoot.AppendChild(planValues);
            xDoc.Save(path);
        }
    }
}
