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
using System.Windows.Shapes;

namespace AFSViewer
{
    public class ProcessWindowModel : BaseNotify
    {
        private string _message;

        public string Message
        {
            get => _message;
            set => SetField(ref _message, value);
        }
    }

    /// <summary>
    /// Interaction logic for ProcessWindow.xaml
    /// </summary>
    public partial class ProcessWindow : Window
    {
        private ProcessWindowModel _model;

        public ProcessWindow()
        {
            InitializeComponent();
            _model = new ProcessWindowModel();
            DataContext = _model;
        }

        public void SetMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                _model.Message = message;
            });
        }
    }
}
