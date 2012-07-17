namespace Org.OpenEngSB.Connector.MSNotification.Common
{
    using System;

    public class SimpleEventArgs<T> : EventArgs
    {
        public SimpleEventArgs(T data)
        {
            Data = data;
        }

        public T Data { get; private set; }
    }
}
