// ViewModels/MainViewModel.cs
namespace System.Windows
{
    internal class Forms
    {
        public static object DialogResult { get; internal set; }

        internal class FolderBrowserDialog
        {
            public string Description { get; set; }
            public bool ShowNewFolderButton { get; set; }
            public string SelectedPath { get; internal set; }

            internal object ShowDialog()
            {
                throw new NotImplementedException();
            }
        }
    }
}