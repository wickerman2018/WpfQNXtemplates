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
        private const char NL = '\n';

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
            if (cbThreadHeader.IsChecked == true)
            {
                tbSourceGen.AppendText("#include <pthread.h>" + NL + NL);
            }
            tbSourceGen.AppendText("static void *" + tbThreadName.Text + " (void *){" + NL + NL +
                "return 0; " + NL +
                "};" + NL + NL);
            //Запуск потока
            tbSourceGen.AppendText("pthread_attr_t attr_th;"+ NL);
            tbSourceGen.AppendText("pthread_attr_init(&attr_th);" + NL);
            tbSourceGen.AppendText("pthread_attr_setdetachstate(&attr_th, "+ ThreadDetachAttr + ");" + NL);
            if (cbThreadSchedpolicy.SelectedIndex != 1)
                tbSourceGen.AppendText("pthread_attr_setschedpolicy(&attr_th, " + ThreadSchedpolicy + ");" + NL);
            tbSourceGen.AppendText("pthread_create(NULL, &attr_th, " + tbThreadName.Text + ", NULL);" + NL);
        }

        private void BtnCopyToBuf_Click(object sender, RoutedEventArgs e) => Clipboard.SetText(tbSourceGen.Text);

        private void BtnMessServGen_Click(object sender, RoutedEventArgs e)
        {
            tbSourceGen.Clear();

            tbSourceGen.AppendText("int rcvid;" + NL);
            tbSourceGen.AppendText("dispatch_t *dpp;" + NL);
            tbSourceGen.AppendText("name_attach_t *my_prefix=NULL;" + NL);
            tbSourceGen.AppendText("dpp=dispatch_create();" + NL);
            tbSourceGen.AppendText("my_prefix=name_attach(dpp, \""+ tbMessChanName.Text + "\", 0);" + NL);
            tbSourceGen.AppendText(" " + NL);
            tbSourceGen.AppendText(" " + NL);
        }
        private void BtnMessClientGen_Click(object sender, RoutedEventArgs e)
        {
            tbSourceGen.Clear();

        }
        private void BtnInterGen_Click(object sender, RoutedEventArgs e)
        {
            tbSourceGen.Clear();
        }

        private void BtnTimerGen_Click(object sender, RoutedEventArgs e)
        {
            tbSourceGen.Clear();
        }

        private void BtnRMGen_Click(object sender, RoutedEventArgs e)
        {
            tbSourceGen.Clear();
        }


    }
}
