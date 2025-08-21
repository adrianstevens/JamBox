using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReactiveUI;

namespace JamBox.Core
{
    public class ViewLocator : IDataTemplate
    {
        public bool SupportsRecycling => false;

        public Control Build(object? data)
        {
            if (data == null)
                return new TextBlock { Text = "Data is null" };

            // Get the full name of the ViewModel.
            var viewModelName = data.GetType().FullName!;

            // This is the key change: Replace the namespace part
            var viewTypeName = viewModelName.Replace(".ViewModels.", ".Views.");
            viewTypeName = viewTypeName.Replace("Model", "");

            var type = typeof(ViewLocator).Assembly.GetType(viewTypeName);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            else
            {
                return new TextBlock { Text = "Not Found: " + viewTypeName };
            }
        }

        public bool Match(object? data)
        {
            return data is ReactiveObject;
        }
    }
}