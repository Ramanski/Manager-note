using System.Globalization;
using System.Windows.Controls;

namespace Mannote.ValidationRules
{
        class WeightRule : ValidationRule
        {
            private float min = 0;
            private float max = 7000;

            public float Min
            {
                get { return min; }
                set { min = value; }
            }

            public float Max
            {
                get { return max; }
                set { max = value; }
            }

            public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            {
                float weight = 0;

                try
                {
                    weight = float.Parse((string)value);
                }
                catch
                {
                    return new ValidationResult(false, "Недопустимые символы.");
                }

                if ((weight < Min) || (weight > Max))
                {
                    return new ValidationResult(false, "Масса не входит в диапазон " + Min + " до " + Max + ".");
                }
                else
                {
                    return new ValidationResult(true, null);
                }
            }
        }
}
