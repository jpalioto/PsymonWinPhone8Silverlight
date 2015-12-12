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
    public class Pattern : List<int>
    {
        public Pattern() : base()
        {
        }

        public Pattern(Pattern seedPattern)
        {
            if (seedPattern == null)
            {
                return;
            }

            foreach (var i in seedPattern)
            {
                Add(i);
            }
        }
    }
}
