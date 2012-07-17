namespace Org.OpenEngSB.Connector.MSNotification.Common
{
    using System;
    using System.ComponentModel;

    public static class ExtensionMethods
    {
        public static void SafeInvoke<T>(this EventHandler<T> me, object sender = null, T e = default(T)) where T : EventArgs
        {
            if (me != null)
                me(sender, e);
        }

        public static void SafeInvoke(this EventHandler me, object sender = null, EventArgs e = null)
        {
            if (me != null)
                me(sender, e);
        }

        public static void SafeInvoike(this PropertyChangedEventHandler me, object sender = null, string property = null)
        {
            me.SafeInvoike(sender, new PropertyChangedEventArgs(property));
        }

        public static void SafeInvoike(this PropertyChangedEventHandler me, object sender = null, PropertyChangedEventArgs e = null)
        {
            if (me != null)
                me(sender, e);
        }
    }
}
