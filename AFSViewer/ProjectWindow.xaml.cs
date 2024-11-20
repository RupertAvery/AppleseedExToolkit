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
using Microsoft.Win32;

namespace AFSViewer
{
    public class ProjectWindowModel : BaseNotify
    {
        private string _projectPath;
        private string _isoPath;
        private string _patchedISOPath;

        public string ProjectPath
        {
            get => _projectPath;
            set => SetField(ref _projectPath, value);
        }

        public string ISOPath
        {
            get => _isoPath;
            set => SetField(ref _isoPath, value);
        }

        public string PatchedISOPath
        {
            get => _patchedISOPath;
            set => SetField(ref _patchedISOPath, value);
        }
    }

    /// <summary>
    /// Interaction logic for ProjectWindow.xaml
    /// </summary>
    public partial class ProjectWindow : Window
    {
        public ProjectWindowModel Model;

        public ProjectWindow()
        {
            InitializeComponent();

            Model = new ProjectWindowModel();
            DataContext = Model;
        }

        private void ProjectPath_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.ProjectPath = dialog.FolderName;
            }
        }

        private void ISOPath_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "ISO files|*.iso|All files|*.*";
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.ISOPath = dialog.FileName;
            }
        }

        private void PatchedISOPath_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "ISO files|*.iso|All files|*.*";
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.PatchedISOPath = dialog.FileName;
            }
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}
