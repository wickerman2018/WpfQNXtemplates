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
        private const String ThreadTempl = "#HEADERSstatic void *#THREAD_NAME (void *){\n#THREADBODY\nreturn EXIT_SUCCESS; \n};\n\n" +
            "pthread_attr_t attr_th;\npthread_attr_init(&attr_th);\npthread_attr_setdetachstate(&attr_th, #THREAD_DETACH_ATTR);\n" +
            "pthread_attr_setschedpolicy(&attr_th, #THREADSHEDPOLICY);\npthread_create(NULL, &attr_th, #THREAD_NAME, NULL);";

        private const String MessClientTempl = "#HEADERS\t#RECBUF_TYPE #RECBUF_NAME;\n\t#REPBUF_TYPE #REPBUF_NAME;\n\tint rcvid;\n\tdispatch_t *dpp;\n" +
            "\tname_attach_t *my_prefix=NULL;\n\tdpp=dispatch_create();\n\tmy_prefix=name_attach(dpp, \"#MESS_CHAN\", 0);\n" +
            "\trcvid=MsgReceive(my_prefix->chid, &#RECBUF_NAME, sizeof(#RECBUF_TYPE), NULL);\n" +
            "\tMsgReply(rcvid, 1, &#REPBUF_NAME, sizeof(#REPBUF_TYPE));";

        private const String MessServTempl = "#HEADERS\t#RECBUF_TYPE #RECBUF_NAME;\n\t#REPBUF_TYPE #REPBUF_NAME;\n\tint coid=0, status=0;\n" +
            "\tcoid=name_open( \"#MESS_CHAN\", 0);\n\tstatus=MsgSend(coid, &#RECBUF_NAME, sizeof(#RECBUF_TYPE), &#REPBUF_NAME, sizeof(#REPBUF_TYPE));\n\tname_close(coid);";

        private const String IntTempl = "#HEADERSconst struct sigevent *IntHandler(void *arg, int id){\nstruct sigevent *event = (struct sigevent *)arg;\n" +
            "\t//TODO add interrupt handle here\n\nreturn event;\n};\n\nstruct sigevent event;\nSIGEV_INTR_INIT(&event);\n" +
            "intId = InterruptAttach(HW_IRQ, IntHandler, &event, sizeof(event), 0);\nInterruptUnmask(HW_IRQ, intId);\nfor(;;){\n\tInterruptWait(NULL, NULL);\n\n}";

        private const String RMTempl = "int io_open (resmgr_context_t *ctp, io_open_t *msg, RESMGR_HANDLE_T *handle, void *extra) {\nreturn (iofunc_open_default(ctp, msg, handle, extra));\n}\n" +
            "\nint io_read (resmgr_context_t *ctp, io_read_t *msg, RESMGR_OCB_T *ocb) { \nint rc, rmptr, nbytes, nleft; \n#READBUF_TYPE *ptr;\nptr = (#READBUF_TYPE*) malloc(sizeof(#READBUF_TYPE)); \n" +
            "rc = iofunc_read_verify (ctp, msg, (iofunc_ocb_t*) ocb, NULL);\nif (rc != EOK) return rc;\nif ((msg->i.xtype & _IO_XTYPE_MASK) != _IO_XTYPE_NONE) return ENOSYS;\n" +
            "if (msg->i.nbytes < 0) return EINVAL;\nif (msg->i.nbytes == 0) return EOK;\nnleft = sizeof(#READBUF_TYPE);\nnbytes = __min(msg->i.nbytes, nleft);\n*ptr = #READBUF_NAME;\n" +
            "ocb->offset += nbytes;\n_IO_SET_READ_NBYTES(ctp, nbytes);\nrmptr = _RESMGR_PTR(ctp, ptr, nbytes); \nocb->attr->flags |= IOFUNC_ATTR_ATIME | IOFUNC_ATTR_DIRTY_TIME;\nfree(ptr);\n" +
            "return _RESMGR_NPARTS(1);\n}\n\nint io_write (resmgr_context_t *ctp, io_write_t *msg, RESMGR_OCB_T *ocb) {\nint rc; \n#WRITEBUF_TYPE *pSP;\n" +
            "pSP = (#WRITEBUF_TYPE*)malloc(sizeof(#WRITEBUF_TYPE)); \nrc = iofunc_write_verify (ctp, msg, (iofunc_ocb_t*) ocb, NULL); \nif (rc != EOK) return rc;\n" +
            "if ((msg->i.xtype & _IO_XTYPE_MASK) != _IO_XTYPE_NONE) return ENOSYS;\nif (msg->i.nbytes < 0) return EINVAL;\nif (msg->i.nbytes == 0) return EOK;\n" +
            "_IO_SET_WRITE_NBYTES(ctp, msg->i.nbytes);\n#WRITEBUF_NAME=*pSP;\nocb->attr->flags |= IOFUNC_ATTR_MTIME | IOFUNC_ATTR_DIRTY_TIME;\nfree(pSP);\nreturn EXIT_SUCCESS;\n}\n\n" +
            "resmgr_attr_t resmgr_attr;\ndispatch_t * dpp;\ndispatch_context_t* ctp;\nint id;\nif ((dpp = dispatch_create()) == NULL) cerr << \"Unable to allocate dispatch handle.\\n\";\n" +
            "memset(&resmgr_attr, 0, sizeof resmgr_attr);\nresmgr_attr.nparts_max = 1;\nresmgr_attr.msg_max_size = 2048;\n" +
            "iofunc_func_init(_RESMGR_CONNECT_NFUNCS, &connect_funcs, _RESMGR_IO_NFUNCS, &io_funcs);\nconnect_funcs.open = io_open;\nio_funcs.read = io_read;\nio_funcs.write = io_write;\ni" +
            "ofunc_attr_init(&attr, S_IFNAM | 0666, 0, 0); \nattr.nbytes = sizeof(#READBUF_TYPE);\nif ((id = resmgr_attach(dpp, &resmgr_attr, \"/dev/#RM_NAME\", _FTYPE_ANY, 0,\n" +
            "&connect_funcs, &io_funcs, &attr)) == -1) cerr << \"Unable to attach name.\\n\"; \nctp = dispatch_context_alloc(dpp);\nwhile (1)\n{\n" +
            "\tif ((ctp = dispatch_block(ctp)) == NULL) cerr << \"block error\\n\";\n\tdispatch_handler(ctp);\n}";
        private String GetThreadTempl()
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
            String GenCode = ThreadTempl;
            if (cbThreadHeader.IsChecked == true)
                GenCode = GenCode.Replace("#HEADERS", "#include <pthread.h>\n\n");
            else
                GenCode = GenCode.Replace("#HEADERS", "");
            GenCode = GenCode.Replace("#THREAD_NAME", tbThreadName.Text);            
            GenCode = GenCode.Replace("#THREAD_DETACH_ATTR", ThreadDetachAttr);
            GenCode = GenCode.Replace("#THREADSHEDPOLICY", ThreadSchedpolicy);
            return GenCode;
        }
        private void SetMessTemp(String TemlStr) {
            tbSourceGen.Clear();
            String MessPaste = TemlStr;
            MessPaste = MessPaste.Replace("#HEADERS", "");
            MessPaste = MessPaste.Replace("#RECBUF_TYPE", tbMessRecBufTypeName.Text);
            MessPaste = MessPaste.Replace("#RECBUF_NAME", tbMessRecBufName.Text);
            MessPaste = MessPaste.Replace("#REPBUF_TYPE", tbMessRecBufTypeName.Text);
            MessPaste = MessPaste.Replace("#REPBUF_NAME", tbMessRecBufName.Text);
            MessPaste = MessPaste.Replace("#MESS_CHAN", tbMessChanName.Text);
            if (cbThreadMess.IsChecked == true)
            {
                String ThreadPaste = GetThreadTempl();
                MessPaste = ThreadPaste.Replace("#THREADBODY", MessPaste);
            }
            else
            {
                MessPaste = MessPaste.Replace("\t", "");
            }
            tbSourceGen.AppendText(MessPaste);
        }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnThreadGen_Click(object sender, RoutedEventArgs e)
        {

            tbSourceGen.Clear();
            String ThreadPaste = GetThreadTempl();
            ThreadPaste = ThreadPaste.Replace("#THREADBODY", "//TODO add your code here");
            tbSourceGen.AppendText(ThreadPaste);
        }

        private void BtnCopyToBuf_Click(object sender, RoutedEventArgs e) => Clipboard.SetText(tbSourceGen.Text);

        private void BtnMessServGen_Click(object sender, RoutedEventArgs e)
        {
            SetMessTemp(MessServTempl);
        }
        private void BtnMessClientGen_Click(object sender, RoutedEventArgs e)
        {
            SetMessTemp(MessClientTempl);
        }
        private void BtnInterGen_Click(object sender, RoutedEventArgs e)
        {
            tbSourceGen.Clear();
            String IntPaste = IntTempl;
            IntPaste = IntPaste.Replace("#HEADERS", "");
            tbSourceGen.AppendText(IntPaste);
        }

        private void BtnRMGen_Click(object sender, RoutedEventArgs e)
        {
            tbSourceGen.Clear();
            String RMPaste = RMTempl;
            RMPaste = RMPaste.Replace("#HEADERS", "");
            RMPaste = RMPaste.Replace("#READBUF_TYPE", tbRMReadBufTypeName.Text);
            RMPaste = RMPaste.Replace("#READBUF_NAME", tbRMReadBufName.Text);
            RMPaste = RMPaste.Replace("#WRITEBUF_TYPE", tbMessRecBufTypeName.Text);
            RMPaste = RMPaste.Replace("#WRITEBUF_NAME", tbMessRecBufName.Text);
            RMPaste = RMPaste.Replace("#RM_NAME", tbMessChanName.Text);
            tbSourceGen.AppendText(RMPaste);

        }


    }
}
