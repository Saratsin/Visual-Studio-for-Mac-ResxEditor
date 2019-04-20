using System;
using Gtk;

namespace ResxEditor.Core.Views
{
    public class ResourceEditedEventArgs : EventArgs
    {
        public string Path { get; set; }

        public string NextText { get; set; }
    }

    public class ResourceList : TreeView
    {
        public LocalizationColumn NameColumn { get; private set; }

        public ResourceList()
        {
            NameColumn = new LocalizationColumn(true) { Title = "Name" };
            var valueColumn = new LocalizationColumn(true) { Title = "Value" };
            var commentColumn = new LocalizationColumn(true) { Title = "Comment" };

            AddCol(NameColumn, 0);
            AddCol(valueColumn, 1);
            AddCol(commentColumn, 2);

            NameColumn.Edited += (sender, e) =>
            {
                OnNameEdited?.Invoke(this, e);
                OnResourceAdded?.Invoke(this, e.NextText);
                SetCursor(new TreePath(e.Path), valueColumn, true);
            };

            valueColumn.Edited += (sender, e) =>
            {
                OnValueEdited?.Invoke(this, e);
                SetCursor(new TreePath(e.Path), commentColumn, true);
            };

            commentColumn.Edited += (sender, e) =>
            {
                OnCommentEdited?.Invoke(this, e);
            };

            ButtonReleaseEvent += (object sender, ButtonReleaseEventArgs e) =>
            {
                // Right click event
                if (e.Event.Type == Gdk.EventType.ButtonRelease && e.Event.Button == 3)
                {
                    RightClicked(this, e);
                }
            };

            KeyPressEvent += OnUserKeyPress;
        }

        public event EventHandler<ResourceEditedEventArgs> OnNameEdited;

        public event EventHandler<ResourceEditedEventArgs> OnValueEdited;

        public event EventHandler<ResourceEditedEventArgs> OnCommentEdited;

        public event EventHandler<string> OnResourceAdded;

        public event EventHandler<ButtonReleaseEventArgs> RightClicked;

        public TreeSelection GetSelectedResource()
        {
            return Selection;
        }

        private void OnUserKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.Event.Key == Gdk.Key.Tab)
            {
                //get a pointer in the tree to the selected item
                var resList = sender as ResourceList;
                resList.GetCursor(out var path, out var col);
                if (col != null)
                {
                    //read updated values from the control in focus which currently is always an Gtk.Entry.
                    var next = resList.FocusChild as Entry;
                    //sanity check that we're within the expected range of values
                    var selectedIndex = (int)col.Data["text"];
                    if (selectedIndex < Columns.Length)
                    {
                        col.CellRenderers[0].StopEditing(false);

                        //Call the save changes handler for the column 
                        //this also handles the change in tab focus via SetCursor 
                        ((LocalizationColumn)col).InvokeEdited(new ResourceEditedEventArgs 
                        { 
                            Path = path.ToString(), 
                            NextText = next.Text 
                        });

                        e.RetVal = true; //eat the event
                    }
                }
            }
        }

        private void AddCol(LocalizationColumn col, int index)
        {
            AppendColumn(col);
            col.Data.Add("text", index);
            col.AddAttribute("text", index);
        }
    }
}