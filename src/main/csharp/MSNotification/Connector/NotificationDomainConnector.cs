namespace Org.OpenEngSB.Connector.MSNotification.Connector
{
    using System;
    using Org.OpenEngSB.Connector.MSNotification.Common;
    using Org.OpenEngSB.Domain.Notification;

    public class NotificationDomainConnector : INotificationDomainSoap11Binding
    {
        public event EventHandler<SimpleEventArgs<Notification>> Notified;

        #region INotificationDomainSoap11Binding Members

        public void notify(Notification args0)
        {
            Notified.SafeInvoke(this, new SimpleEventArgs<Notification>(args0));
        }

        #endregion
    }
}
