using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfQNXtemplates
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnThreadGen_Click(object sender, RoutedEventArgs e)
        {
            String ThreadDetachAttr= "PTHREAD_CREATE_DETACHED";
            String ThreadSchedpolicy = "SCHED_NOCHANGE";
            switch (cbThreadDetachAttr.SelectedIndex)
            {
                case 0: 
                    ThreadDetachAttr = "PTHREAD_CREATE_JOINABLE"; 
                    break;
                case 1:
                    ThreadDetachAttr = "PTHREAD_CREATE_DETACHED";
                    break;
            }
            switch (cbThreadSchedpolicy.SelectedIndex)
            {
                case 0:
                    ThreadSchedpolicy = "SCHED_FIFO";
                    break;
                case 1:
                    ThreadSchedpolicy = "SCHED_NOCHANGE";
                    break;
                case 2:
                    ThreadSchedpolicy = "SCHED_RR";
                    break;
                case 3:
                    ThreadSchedpolicy = "SCHED_SPORADIC";
                    break;
            }

            tbSourceGen.Clear();
            //Создание функции
            tbSourceGen.AppendText("static void *" + tbThreadName.Text + " (void *){" + Environment.NewLine
                + Environment.NewLine +
                "return 0; " + Environment.NewLine +
                "};" + Environment.NewLine + Environment.NewLine);
            //Запуск потока
            tbSourceGen.AppendText("pthread_attr_t attr_th;"+Environment.NewLine);
            tbSourceGen.AppendText("pthread_attr_init(&attr_th);" + Environment.NewLine);
            tbSourceGen.AppendText("pthread_attr_setdetachstate(&attr_th, "+ ThreadDetachAttr + ");" + Environment.NewLine);
            if (cbThreadSchedpolicy.SelectedIndex != 1)
                tbSourceGen.AppendText("pthread_attr_setschedpolicy(&attr_th, " + ThreadSchedpolicy + ");" + Environment.NewLine);
            tbSourceGen.AppendText("pthread_create(NULL, &attr_th, " + tbThreadName.Text + ", NULL);" + Environment.NewLine);
        }

        private void BtnCopyToBuf_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbSourceGen.Text);
        }
    }
}
