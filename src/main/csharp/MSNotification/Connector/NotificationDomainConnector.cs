/***
 * Licensed to the Austrian Association for Software Tool Integration (AASTI)
 * under one or more contributor license agreements. See the NOTICE file
 * distributed with this work for additional information regarding copyright
 * ownership. The AASTI licenses this file to you under the Apache License,
 * Version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ***/

namespace Org.OpenEngSB.Connector.MSNotification.Connector
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows.Input;
    using System.Windows.Threading;
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

        private IDomainFactory factory;
        private bool disposed = false;

        public event EventHandler<SimpleEventArgs<Notification>> Notified;

        public Dispatcher SynchronizationDispatcher { get; set; }
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
                logger.Warn("Couldn't load the notifications-file.", fex);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to load notifications.", ex);
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
                factory = DomainFactoryProvider.GetDomainFactoryInstance(Settings.Instance.Version, Settings.Instance.Destination, this);
                factory.CreateDomainService(DomainName);
                factory.RegisterConnector(factory.getServiceId(DomainName), DomainName);
            }
            catch (Exception ex)
            {
                factory = null;
                logger.Error("Couldn't create factory with the current settings.", ex);
            }
        }

        #region INotificationDomainSoap11Binding Members

        public void notify(Notification args0)
        {
            if (SynchronizationDispatcher != null && !SynchronizationDispatcher.CheckAccess())
            {
                SynchronizationDispatcher.Invoke(new Action(() => notify(args0)), DispatcherPriority.DataBind);
                return;
            }

            Notifications.Insert(0, args0);
            Notified.SafeInvoke(this, new SimpleEventArgs<Notification>(args0));
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (disposed) return;

            disposed = true;

            try
            {
                XmlSerializer ser = new XmlSerializer(Notifications.GetType());

                using (var sw = new StreamWriter(_notificationsPath))
                {
                    ser.Serialize(sw, Notifications);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to save notifications.", ex);
            }

            factory.UnRegisterConnector(DomainName);
            factory.DeleteDomainService(DomainName);
            factory.StopConnection(DomainName);
        }

        #endregion
    }
}
