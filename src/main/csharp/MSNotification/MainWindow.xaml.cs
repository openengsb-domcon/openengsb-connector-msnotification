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

namespace Org.OpenEngSB.Connector.MSNotification
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using Org.OpenEngSB.Connector.MSNotification.Common;
    using Org.OpenEngSB.Connector.MSNotification.Connector;
    using Org.OpenEngSB.Connector.MSNotification.GUI.Controls;
    using Org.OpenEngSB.Domain.NotificationInterfaces;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            Closed += new EventHandler(MainWindow_Closed);
            StateChanged += new EventHandler(MainWindow_StateChanged);

            InitializeComponent();

            NotificationDomainConnector.Instance.SynchronizationDispatcher = this.Dispatcher;
            NotificationDomainConnector.Instance.Notified += new EventHandler<SimpleEventArgs<Notification>>(Instance_Notified);
        }

        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
            else
            {
                ShowInTaskbar = true;
                NotifyIcon.Icon = GUI.Images.Resources.openEngSB;
            }
        }

        void _balloon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = System.Windows.WindowState.Normal;
            Activate();
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            NotificationDomainConnector.Instance.Dispose();
        }

        void Instance_Notified(object sender, SimpleEventArgs<Notification> e)
        {
            NotifyIcon.ShowCustomBalloon(CreateBalloon(e.Data), PopupAnimation.Fade, 10000);

            if (!IsActive)
                NotifyIcon.Icon = GUI.Images.Resources.openEngSB_info;
        }

        private UIElement CreateBalloon(object dataContext)
        {
            CustomBalloon balloon = new CustomBalloon();

            balloon.MouseLeftButtonDown += new MouseButtonEventHandler(_balloon_MouseLeftButtonDown);
            balloon.DataContext = dataContext;

            return balloon;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= new RoutedEventHandler(MainWindow_Loaded);

            settingsView.DataContext = Settings.Instance;
            lstNotifications.ItemsSource = NotificationDomainConnector.Instance.Notifications;

            CommandBindings.Add(Settings.Instance.SaveBinding);
            CommandBindings.Add(Settings.Instance.ResetBinding);

            if (Environment.GetCommandLineArgs().Contains("/m"))
            {
                this.WindowState = System.Windows.WindowState.Minimized;
            }
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string processName = e.Parameter.ToString();
            string arguments = null;
            int delimiterIndex = processName.IndexOf('\\');

            if (delimiterIndex > 0)
            {
                arguments = processName.Substring(delimiterIndex + 1);
                processName = processName.Substring(0, delimiterIndex);
            }

            ProcessStartInfo psi = new ProcessStartInfo(processName, arguments);
            Process p = new Process() { StartInfo = psi };

            p.Start();
        }

        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            string param = e.Parameter as string;

            e.CanExecute = !string.IsNullOrWhiteSpace(param);
            e.Handled = true;
        }
    }
}
