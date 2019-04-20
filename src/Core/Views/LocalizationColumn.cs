using System;
using Gtk;
using ResxEditor.Core.Interfaces;

namespace ResxEditor.Core.Views
{
    public class LocalizationColumn : TreeViewColumn, IListViewColumn
    {
        public LocalizationColumn(bool editable = false)
        {
            var textRenderer = new CellRendererText { Editable = editable };

            textRenderer.Edited += (_, args) => Edited(this, new ResourceEditedEventArgs
            {
                Path = args.Path,
                NextText = args.NewText
            });

            Renderer = textRenderer;
            ColumnType = typeof(string);

            PackStart(Renderer, true);
        }

        public event EventHandler<ResourceEditedEventArgs> Edited;

        public Type ColumnType { get; } = typeof(string);

        public CellRenderer Renderer { get; }

        public int Position { get; private set; }

        public void InvokeEdited(ResourceEditedEventArgs args)
        {
            Edited?.Invoke(this, args);
        }

        public void AddAttribute(string attribute, int position)
        {
            AddAttribute(Renderer, attribute, position);
            Position = position;
        }
    }
}