using System;
using Gtk;

namespace ResxEditor.Core.Views
{
    public class AddResourceButton : Button
    {
        public AddResourceButton() : base(ProduceLabel()) 
        {
        }

        private static Label ProduceLabel()
        {
            return new Label("Add Resource");
        }
    }

    public class RemoveResourceButton : Button
    {
        public RemoveResourceButton() : base(ProduceLabel()) 
        {
        }

        private static Label ProduceLabel()
        {
            return new Label("Remove Resource");
        }
    }

    public class ResourceControlBar : HButtonBox
    {
        private readonly AddResourceButton _addResourceButton;
        private readonly RemoveResourceButton _removeResourceButton;

        public ResourceControlBar()
        {
            Layout = ButtonBoxStyle.Start;
            Spacing = 10;

            _addResourceButton = new AddResourceButton();
            _removeResourceButton = new RemoveResourceButton();

            FilterEntry = new Entry();

            _addResourceButton.Clicked += (sender, e) => OnAddResource(this, e);

            _removeResourceButton.Clicked += (sender, e) => OnRemoveResource(this, e);

            PackStart(_addResourceButton, false, false, 10);
            PackStart(_removeResourceButton, false, false, 10);
            PackEnd(FilterEntry);
        }

        public event EventHandler OnAddResource;

        public event EventHandler OnRemoveResource;

        public Entry FilterEntry { get; }
    }
}