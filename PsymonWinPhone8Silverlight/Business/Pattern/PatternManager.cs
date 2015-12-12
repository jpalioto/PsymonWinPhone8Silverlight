using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Psymon.Business
{
    public static class PatternManager
    {
        private static readonly Random _rnd = new Random();

        public static Pattern GeneratePattern(Pattern seedPattern, int numberToAdd, int maxVal)
        {
            var retVal = new Pattern(seedPattern);

            for (int i = 0; i < numberToAdd; i++)
            {
                retVal.Add(_rnd.Next(0, maxVal));
            }

            return retVal;
        }
    }
}
