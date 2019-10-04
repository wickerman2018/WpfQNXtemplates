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
        private const char TAB = '\t';

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ThreadHeader()
        {
            if (cbThreadHeader.IsChecked == true)
            {
                tbSourceGen.AppendText("#include <pthread.h>" + NL + NL);
            }
            tbSourceGen.AppendText("static void *" + tbThreadName.Text + " (void *){" + NL);
        }

        private void ThreadBottom()
        {
            String ThreadDetachAttr = "PTHREAD_CREATE_DETACHED";
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

            tbSourceGen.AppendText("return EXIT_SUCCESS; " + NL + "};" + NL + NL);
            //Запуск потока
            tbSourceGen.AppendText("pthread_attr_t attr_th;" + NL);
            tbSourceGen.AppendText("pthread_attr_init(&attr_th);" + NL);
            tbSourceGen.AppendText("pthread_attr_setdetachstate(&attr_th, " + ThreadDetachAttr + ");" + NL);
            if (cbThreadSchedpolicy.SelectedIndex != 1)
                tbSourceGen.AppendText("pthread_attr_setschedpolicy(&attr_th, " + ThreadSchedpolicy + ");" + NL);
            tbSourceGen.AppendText("pthread_create(NULL, &attr_th, " + tbThreadName.Text + ", NULL);" + NL);

        }
        private void BtnThreadGen_Click(object sender, RoutedEventArgs e)
        {

            tbSourceGen.Clear();
            //Создание функции
            ThreadHeader();
            tbSourceGen.AppendText(NL.ToString());
            ThreadBottom();
        }

        private void BtnCopyToBuf_Click(object sender, RoutedEventArgs e) => Clipboard.SetText(tbSourceGen.Text);

        private void BtnMessServGen_Click(object sender, RoutedEventArgs e)
        {            
            tbSourceGen.Clear();
            if(cbThreadMess.IsChecked==true) ThreadHeader();
            tbSourceGen.AppendText(TAB + tbMessRecBufTypeName.Text + " " + tbMessRecBufName.Text + ";" + NL);
            tbSourceGen.AppendText(TAB + tbMessRepBufTypeName.Text + " " + tbMessRepBufName.Text + ";" + NL);
            tbSourceGen.AppendText(TAB+ "int rcvid;" + NL);
            tbSourceGen.AppendText(TAB + "dispatch_t *dpp;" + NL);
            tbSourceGen.AppendText(TAB + "name_attach_t *my_prefix=NULL;" + NL);
            tbSourceGen.AppendText(TAB + "dpp=dispatch_create();" + NL);
            tbSourceGen.AppendText(TAB + "my_prefix=name_attach(dpp, \"" + tbMessChanName.Text + "\", 0);" + NL);
            tbSourceGen.AppendText(TAB + "rcvid=MsgReceive(my_prefix->chid, &" + tbMessRecBufName.Text + ", sizeof(" + tbMessRecBufName.Text + "), NULL);" + NL);
            tbSourceGen.AppendText(TAB + "MsgReply(rcvid, 1, &" + tbMessRepBufName.Text + ", sizeof("+ tbMessRepBufName.Text + "));" + NL);
            if (cbThreadMess.IsChecked == true) ThreadBottom();
        }
        private void BtnMessClientGen_Click(object sender, RoutedEventArgs e)
        {
            tbSourceGen.Clear();
            if (cbThreadMess.IsChecked == true) ThreadHeader();
            tbSourceGen.AppendText(TAB + tbMessRecBufTypeName.Text + " " + tbMessRecBufName.Text + ";" + NL);
            tbSourceGen.AppendText(TAB + tbMessRepBufTypeName.Text + " " + tbMessRepBufName.Text + ";" + NL);
            tbSourceGen.AppendText(TAB + "int coid=0, status=0;" + NL);
            tbSourceGen.AppendText(TAB + "coid=name_open( \"" + tbMessChanName.Text + "\", 0);" + NL);
            tbSourceGen.AppendText(TAB + "status=MsgSend(coid, &" + tbMessRecBufName.Text + ", sizeof(" + tbMessRecBufName.Text + "), &" + tbMessRepBufName.Text + ", sizeof(" + tbMessRepBufName.Text + ");"+ NL);
            tbSourceGen.AppendText(TAB + "name_close(coid);" + NL);
            if (cbThreadMess.IsChecked == true) ThreadBottom();

        }
        private void BtnInterGen_Click(object sender, RoutedEventArgs e)
        {
            tbSourceGen.Clear();
            //Поток прерывания 
            tbSourceGen.AppendText("const struct sigevent *IntHandler(void *arg, int id){" + NL + 
                "struct sigevent *event = (struct sigevent *)arg;" + NL + 
                TAB + "//TODO add interrupt handle here" + NL + NL);
            tbSourceGen.AppendText("return event;" + NL + "};" + NL + NL);
            //Вызов прерывания
            tbSourceGen.AppendText("struct sigevent event;" + NL);
            tbSourceGen.AppendText("SIGEV_INTR_INIT(&event);" + NL + 
                "intId = InterruptAttach(HW_IRQ, IntHandler, &event, sizeof(event), 0);" + NL +
                "InterruptUnmask(HW_IRQ, intId);" + NL);
            tbSourceGen.AppendText("for(;;){" + NL + 
                TAB + "InterruptWait(NULL, NULL);" + NL + NL + "}" + NL);
        }

        private void BtnRMGen_Click(object sender, RoutedEventArgs e)
        {
            tbSourceGen.Clear();
            //io_open
            tbSourceGen.AppendText("int io_open (resmgr_context_t *ctp, io_open_t *msg, RESMGR_HANDLE_T *handle, void *extra) {" + NL +
                "return (iofunc_open_default(ctp, msg, handle, extra));" + NL + "}" + NL + NL);
            //io_read
            tbSourceGen.AppendText("int io_read (resmgr_context_t *ctp, io_read_t *msg, RESMGR_OCB_T *ocb) { " + NL +
                "int rc, rmptr, nbytes, nleft; " + NL);
            tbSourceGen.AppendText(tbRMReadBufTypeName.Text + " *ptr;" + NL +
                "ptr = (" + tbRMReadBufTypeName.Text + "*) malloc(sizeof(" + tbRMReadBufTypeName.Text + ")); " + NL);
            tbSourceGen.AppendText("rc = iofunc_read_verify (ctp, msg, (iofunc_ocb_t*) ocb, NULL);" + NL +
                "if (rc != EOK) return rc;" + NL +
                "if ((msg->i.xtype & _IO_XTYPE_MASK) != _IO_XTYPE_NONE) return ENOSYS;" + NL +
                "if (msg->i.nbytes < 0) return EINVAL;" + NL +
                "if (msg->i.nbytes == 0) return EOK;" + NL);
            tbSourceGen.AppendText("nleft = sizeof(" + tbRMReadBufTypeName.Text + ");" + NL + 
                "nbytes = __min(msg->i.nbytes, nleft);" + NL +
                "*ptr = " + tbRMReadBufName.Text + ";" + NL);
            tbSourceGen.AppendText("ocb->offset += nbytes;" + NL +
                "_IO_SET_READ_NBYTES(ctp, nbytes);" + NL +
                "rmptr = _RESMGR_PTR(ctp, ptr, nbytes); " + NL+
                "ocb->attr->flags |= IOFUNC_ATTR_ATIME | IOFUNC_ATTR_DIRTY_TIME;" + NL +
                "free(ptr);" + NL + 
                "return _RESMGR_NPARTS(1);" + NL + "}" + NL + NL);
            //io_write
            tbSourceGen.AppendText("int io_write (resmgr_context_t *ctp, io_write_t *msg, RESMGR_OCB_T *ocb) {" + NL +
                 "int rc; " + NL);
            tbSourceGen.AppendText(tbRMWriteBufTypeName.Text + " *pSP;" + NL + 
                 "pSP = (" + tbRMWriteBufTypeName.Text + "*)malloc(sizeof(" + tbRMWriteBufTypeName.Text + ")); " + NL);
            tbSourceGen.AppendText("rc = iofunc_write_verify (ctp, msg, (iofunc_ocb_t*) ocb, NULL); " + NL +
                "if (rc != EOK) return rc;" + NL +
                "if ((msg->i.xtype & _IO_XTYPE_MASK) != _IO_XTYPE_NONE) return ENOSYS;" + NL +
                "if (msg->i.nbytes < 0) return EINVAL;" + NL +
                "if (msg->i.nbytes == 0) return EOK;" + NL +
                "_IO_SET_WRITE_NBYTES(ctp, msg->i.nbytes);" + NL);
            tbSourceGen.AppendText(tbRMWriteBufName.Text + "=*pSP;" + NL);
            tbSourceGen.AppendText("ocb->attr->flags |= IOFUNC_ATTR_MTIME | IOFUNC_ATTR_DIRTY_TIME;" + NL +
                "free(pSP);" + NL +
                "return EXIT_SUCCESS;" + NL +"}" + NL + NL);
            
            //resmgr
            tbSourceGen.AppendText("resmgr_attr_t resmgr_attr;" + NL +
                "dispatch_t * dpp;" + NL +
                "dispatch_context_t* ctp;" + NL +
                "int id;" + NL +
                "if ((dpp = dispatch_create()) == NULL) cerr << \"Unable to allocate dispatch handle.\\n\";" + NL +
                "memset(&resmgr_attr, 0, sizeof resmgr_attr);" + NL +
                "resmgr_attr.nparts_max = 1;" + NL +
                "resmgr_attr.msg_max_size = 2048;" + NL +
                "iofunc_func_init(_RESMGR_CONNECT_NFUNCS, &connect_funcs, _RESMGR_IO_NFUNCS, &io_funcs);" + NL +
                "connect_funcs.open = io_open;" + NL +
                "io_funcs.read = io_read;" + NL +
                "io_funcs.write = io_write;" + NL +
                "iofunc_attr_init(&attr, S_IFNAM | 0666, 0, 0); " + NL);
            tbSourceGen.AppendText("attr.nbytes = sizeof(" + tbRMReadBufTypeName.Text + ");" + NL +
                 "if ((id = resmgr_attach(dpp, &resmgr_attr, \"/dev/" + tbRMName.Text + "\", _FTYPE_ANY, 0," + NL +
                 "&connect_funcs, &io_funcs, &attr)) == -1) cerr << \"Unable to attach name.\\n\"; " + NL);
            tbSourceGen.AppendText("ctp = dispatch_context_alloc(dpp);" + NL +
                "while (1)" + NL  + "{" + NL +
                TAB + "if ((ctp = dispatch_block(ctp)) == NULL) cerr << \"block error\\n\";" + NL +
                TAB + "dispatch_handler(ctp);" + NL + "}" + NL + NL);
            tbSourceGen.AppendText("" + NL);

        }


    }
}
