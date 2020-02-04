using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Mannote.ValidationRules
{
    class PriceRule : ValidationRule
    {
        private decimal min = 0;
        private decimal max = 10000;

        public decimal Min
        {
            get { return min; }
            set { min = value; }
        }

        public decimal Max
        {
            get { return max; }
            set { max = value; }
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            decimal price = 0;

            try
            {
                price = Decimal.Parse((string)value);
            }
            catch
            {
                return new ValidationResult(false, "Недопустимые символы.");
            }

            if ((price < Min || price > Max))
            {
                return new ValidationResult(false, "Стоимость не входит в диапазон " + Min + " до " + Max + ".");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
