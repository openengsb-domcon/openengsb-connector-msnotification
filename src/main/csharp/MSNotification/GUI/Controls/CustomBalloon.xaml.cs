using System.Windows;
using System.Windows.Controls;

namespace Org.OpenEngSB.Connector.MSNotification.GUI.Controls
{
    /// <summary>
    /// Interaction logic for CustomBalloon.xaml
    /// </summary>
    public partial class CustomBalloon : UserControl
    {
        public Window WindowToShow { get; set; }

        public CustomBalloon()
        {
            InitializeComponent();
        }

        private void UserControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (WindowToShow != null)
                WindowToShow.Activate();
        }
    }
}
