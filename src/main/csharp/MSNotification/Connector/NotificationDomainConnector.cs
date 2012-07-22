namespace Org.OpenEngSB.Connector.MSNotification.Connector
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows.Input;
    using System.Xml.Serialization;
    using Org.Openengsb.Loom.CSharp.Bridge.Implementation;
    using Org.Openengsb.Loom.CSharp.Bridge.Interface;
    using Org.OpenEngSB.Connector.MSNotification.Common;
    using Org.OpenEngSB.Domain.NotificationInterfaces;

    public class NotificationDomainConnector : SingletonConnector<NotificationDomainConnector>, INotificationDomainSoap11Binding, IDisposable
    {
        private const string DomainName = "notification";
        private static readonly string _notificationsPath;

        static NotificationDomainConnector()
        {
            _notificationsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\MSNotification\notifications.xml";
        }

        public event EventHandler<SimpleEventArgs<Notification>> Notified;
        private IDomainFactory _factory;

        public ObservableCollection<Notification> Notifications { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();

            Notifications = new ObservableCollection<Notification>();

            try
            {
                FileInfo fi = new FileInfo(_notificationsPath);
                if (!fi.Directory.Exists)
                    Directory.CreateDirectory(fi.DirectoryName);

                XmlSerializer ser = new XmlSerializer(Notifications.GetType());

                using (var sr = new StreamReader(_notificationsPath))
                {
                    Notifications = (ObservableCollection<Notification>)ser.Deserialize(sr);
                }
            }
            catch (FileNotFoundException fex)
            {
                // file not found is ok, most probably no settings are there yet
                _logger.Warn("Couldn't load the notifications-file.", fex);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load notifications.", ex);
            }

            Settings.Instance.SaveBinding.Executed += new ExecutedRoutedEventHandler(SaveBinding_Executed);
            ReloadFactory();
        }

        void SaveBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ReloadFactory();
        }

        private void ReloadFactory()
        {
            try
            {
                _factory = DomainFactoryProvider.GetDomainFactoryInstance(Settings.Instance.Version, Settings.Instance.Destination, this);
                _factory.CreateDomainService(DomainName);
                _factory.RegisterConnector(_factory.getServiceId(DomainName), DomainName);
            }
            catch (Exception ex)
            {
                _factory = null;
                _logger.Error("Couldn't create factory with the current settings.", ex);
            }
        }

        #region INotificationDomainSoap11Binding Members

        public void notify(Notification args0)
        {
            Notifications.Insert(0, args0);
            Notified.SafeInvoke(this, new SimpleEventArgs<Notification>(args0));
        }

        #endregion

        #region IDisposable Members

        private bool _disposed = false;

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                XmlSerializer ser = new XmlSerializer(Notifications.GetType());

                using (var sw = new StreamWriter(_notificationsPath))
                {
                    ser.Serialize(sw, this);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to save notifications.", ex);
            }

            _factory.UnRegisterConnector(DomainName);
            _factory.DeleteDomainService(DomainName);
            _factory.StopConnection(DomainName);
        }

        #endregion
    }
}
