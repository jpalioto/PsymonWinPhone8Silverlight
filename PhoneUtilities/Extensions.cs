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
using System.Linq.Expressions;
using System.ComponentModel;
using System.Reflection;

namespace Phone.Utilities
{
    public static class Extensions
    {
        static public void RaiseEvent<T>(this EventHandler<T> evt, object sender, T e)
            where T : EventArgs
        {
            if (evt != null)
                evt(sender, e);
        }

        static public void RaiseEvent(this EventHandler evt, object sender, EventArgs e)
        {
            if (evt != null)
                evt(sender, e);
        } 
    }
}
