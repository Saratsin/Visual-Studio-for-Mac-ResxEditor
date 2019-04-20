using System;
using Gtk;

namespace ResxEditor.Core.Interfaces
{
	public interface IListViewColumn
	{
		CellRenderer Renderer { get; }

		Type ColumnType { get; }
	}
}