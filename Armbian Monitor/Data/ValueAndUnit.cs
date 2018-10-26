using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Armbian_Monitor.Data
{
    public struct ValueAndUnit
    {
        public float Value { get; }
        public string Unit { get; }

        public ValueAndUnit(string whole)
        {
            this.Unit = "";

            string valueStr = "";

            for (int i = 0; i < whole.Length; i++)
            {
                char c = whole[i];

                if (char.IsDigit(c) || c == '.')
                {
                    valueStr += c;
                }
                else
                {
                    this.Unit = whole.Substring(i).Trim();
                    break;
                }
            }

            this.Value = float.Parse(valueStr, CultureInfo.InvariantCulture);
        }

        public override string ToString() => $"{Value}{Unit}";
    }
}
