namespace Home
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class ApplicationCommand : RoutedUICommand
    {
        public ApplicationCommand(string text, string name, Type pageType)
            : base(text, name, typeof(ApplicationCommands))
        {
            Debug.Assert(pageType != null);

            this.PageType = pageType;
        }

        public ApplicationCommand(string text, string name, Type pageType, InputGestureCollection inputGestures)
            : base(text, name, typeof(ApplicationCommands), inputGestures)
        {
            Debug.Assert(pageType != null);

            this.PageType = pageType;
        }

        public Type PageType { get; private set; }
    }
}
