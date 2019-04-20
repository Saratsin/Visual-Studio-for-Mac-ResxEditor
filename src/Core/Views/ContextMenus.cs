using System;
using System.Linq;
using Gtk;
using ResxEditor.Core.Interfaces;

namespace ResxEditor.Core.Views
{
    public class RowActionMenuItem : MenuItem
    {
        public RowActionMenuItem(string label, System.Action rowAction) : base(label)
        {
            ButtonReleaseEvent += (o, args) => rowAction?.Invoke();
        }
    }

    public class CopyCellMenuItem : MenuItem
    {
        private readonly TreePath[] _selectedRows;
        private readonly Func<TreePath, string> _getValueFromRow;

        public CopyCellMenuItem(TreePath[] selectedRows, string label, Func<TreePath, string> getValue) : base(label)
        {
            if (selectedRows.Length == 0)
            {
                throw new IndexOutOfRangeException("Missing selected resource rows");
            }

            _selectedRows = selectedRows;
            _getValueFromRow = getValue;

            ButtonReleaseEvent += (o, e) => OnCopy();
        }

        void OnCopy()
        {
            if (_selectedRows.Length > 1)
            {
                Console.WriteLine("Multiple rows selected. Currently not supported: defaulting to first.");
            }

            var selectedPath = _selectedRows.First();

            Clipboard clipboard = GetClipboard(Gdk.Selection.Clipboard);
            clipboard.Text = _getValueFromRow.Invoke(selectedPath);
        }
    }

    public class CellContextMenu : Menu
    {
        public CellContextMenu(IResourceController resourceController, IResourceListStore storeController, TreePath[] selectedRows)
        {
            Append(new RowActionMenuItem("Add New Row", resourceController.AddNewResource));
            Append(new RowActionMenuItem("Remove Current Row", resourceController.RemoveCurrentResource));

            Append(new SeparatorMenuItem());

            Append(new CopyCellMenuItem(selectedRows, "Copy Name", storeController.GetName));
            Append(new CopyCellMenuItem(selectedRows, "Copy Value", storeController.GetValue));
            Append(new CopyCellMenuItem(selectedRows, "Copy Comment", storeController.GetComment));

            ShowAll();
        }
    }

    public class NoCellContextMenu : Menu
    {
        public NoCellContextMenu(IResourceController resourceController)
        {
            Append(new RowActionMenuItem("Add New Row", resourceController.AddNewResource));

            ShowAll();
        }
    }
}