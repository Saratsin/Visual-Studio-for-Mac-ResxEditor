using System;
using Gtk;
using ResxEditor.Core.Interfaces;
using ResxEditor.Core.Models;

namespace ResxEditor.Core.Controllers
{
    public class ResourceStoreController : IResourceListStore
    {
        private readonly ResourceListStore _baseModel;
        private readonly ResourceFilter _resourceFilter;
        private readonly Func<string> _getFilterTextFunc;

        public ResourceStoreController(Func<string> getFilterTextFunc)
        {
            _baseModel = new ResourceListStore();
            _getFilterTextFunc = getFilterTextFunc;
            _resourceFilter = new ResourceFilter(getFilterTextFunc, _baseModel, null);
        }

        public bool IsFilterable => _getFilterTextFunc != null;

        public TreeModel Model => IsFilterable ? _resourceFilter : (TreeModel)_baseModel;

        public void AppendValues(IResourceModel item)
        {
            _baseModel.AppendValues(item.Name, item.Value, item.Comment);
        }

        public bool SetColumnValue(string path, int column, string value)
        {
            if (!_baseModel.GetIter(out var iter, new TreePath(path)))
            {
                return false;
            }

            _baseModel.SetValue(iter, column, value);
            return true;
        }

        public bool SetName(string path, string nextName)
        {
            if (!_baseModel.GetIter(out var iter, new TreePath(path)))
            {
                return false;
            }

            _baseModel.SetValue(iter, (int)Enums.ResourceColumns.Name, nextName);
            return true;
        }

        public bool SetValue(string path, string nextValue)
        {
            if (!_baseModel.GetIter(out var iter, new TreePath(path)))
            {
                return false;
            }

            _baseModel.SetValue(iter, (int)Enums.ResourceColumns.Value, nextValue);
            return true;
        }

        public bool SetComment(string path, string nextValue)
        {
            if (!_baseModel.GetIter(out var iter, new TreePath(path)))
            {
                return false;
            }

            _baseModel.SetValue(iter, (int)Enums.ResourceColumns.Comment, nextValue);
            return true;
        }

        public string GetName(TreePath path)
        {
            return Model.GetIter(out var iter, path) ? Model.GetValue(iter, (int)Enums.ResourceColumns.Name) as string : null;
        }

        public string GetValue(TreePath path)
        {
            return Model.GetIter(out var iter, path) ? Model.GetValue(iter, (int)Enums.ResourceColumns.Value) as string : null;
        }

        public string GetComment(TreePath path)
        {
            return Model.GetIter(out var iter, path) ? Model.GetValue(iter, (int)Enums.ResourceColumns.Comment) as string : null;
        }

        public bool GetIter(out TreeIter iter, TreePath path)
        {
            return Model.GetIter(out iter, path);
        }

        public TreeIter Prepend()
        {
            return _baseModel.Prepend();
        }

        public TreePath GetPath(TreeIter iter)
        {
            return _baseModel.GetPath(iter);
        }

        /// <summary>
        /// Remove the specified resource at the path
        /// </summary>
        /// <param name="path">Returns true if the path is a valid path.</param>
        public bool Remove(TreePath path)
        {
            var iter = default(TreeIter);
            if (IsFilterable)
            {
                _resourceFilter.GetIter(out iter, path);
                iter = _resourceFilter.ConvertIterToChildIter(iter);
            }
            else
            {
                _baseModel.GetIter(out iter, path);
            }

            return _baseModel.Remove(ref iter);
        }

        public void Refilter()
        {
            _resourceFilter.Refilter();
        }
    }
}