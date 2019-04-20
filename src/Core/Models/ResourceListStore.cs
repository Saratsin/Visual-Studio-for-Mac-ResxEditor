using Gtk;

namespace ResxEditor.Core.Models
{
    public class ResourceListStore : ListStore
    {
        public ResourceListStore() : base(typeof(string), typeof(string), typeof(string)) 
        { 
        }
    }
}