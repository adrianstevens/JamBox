using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReactiveUI;
using System.Reflection;

namespace JamBox.Core
{
    public class ViewLocator : IDataTemplate
    {
        public bool SupportsRecycling => false;

        public Control Build(object? data)
        {
            var name = data!.GetType().FullName!.Replace("ViewModel", "View");
            var type = Assembly.GetEntryAssembly()!.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            else
            {
                return new TextBlock { Text = "Not Found: " + name };
            }
        }

        public bool Match(object? data)
        {
            return data is ReactiveObject;
        }
    }
}