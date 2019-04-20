using System;
using System.ComponentModel.Design;
using Gtk;
using ResxEditor.Core.Interfaces;
using ResxEditor.Core.Models;
using ResxEditor.Core.Views;

namespace ResxEditor.Core.Controllers
{
    public class ResourceController : IResourceController
    {
        private ResourceHandler _resxHandler;

        public ResourceController()
        {
            ResourceEditorView = new ResourceEditorView();
            StoreController = new ResourceStoreController(() => ResourceEditorView.ResourceControlBar.FilterEntry.Text);

            ResourceEditorView.ResourceControlBar.FilterEntry.Changed += (sender, e) => StoreController.Refilter();

            ResourceEditorView.ResourceList.OnResourceAdded += (sender, e) =>
            {
                ResourceEditorView.ResourceControlBar.FilterEntry.Text = "";
                StoreController.Refilter();
            };

            ResourceEditorView.ResourceList.RightClicked += (sender, e) =>
            {
                var selectedRows = ResourceEditorView.ResourceList.GetSelectedResource().GetSelectedRows();
                if (selectedRows.Length > 0)
                {
                    var contextMenu = new CellContextMenu(this, StoreController, selectedRows);
                    contextMenu.Popup();
                }
                else
                {
                    var contextMenu = new NoCellContextMenu(this);
                    contextMenu.Popup();
                }
            };

            ResourceEditorView.ResourceList.Model = StoreController.Model;

            AttachListeners();
        }

        public event EventHandler<bool> OnDirtyChanged;

        public string Filename { get; set; }

        public IResourceListStore StoreController { get; set; }

        public ResourceEditorView ResourceEditorView { get; set; }

        public void AddNewResource()
        {
            var iter = StoreController.Prepend();
            var path = StoreController.GetPath(iter);
            ResourceEditorView.ResourceList.SetCursor(path, ResourceEditorView.ResourceList.NameColumn, true);
            OnDirtyChanged(this, true);
        }

        public void RemoveCurrentResource()
        {
            var selectedPaths = ResourceEditorView.ResourceList.GetSelectedResource().GetSelectedRows();

            foreach (var selectedPath in selectedPaths)
            {
                var name = StoreController.GetName(selectedPath);
                StoreController.Remove(selectedPath);
                if (_resxHandler.RemoveResource(name) > 0)
                {
                    OnDirtyChanged(this, true);
                }
            }
        }

        public void Load(string fileName)
        {
            Filename = fileName;
            _resxHandler = new ResourceHandler(fileName);
            _resxHandler.Resources.ForEach((resource) =>
            {
                if (resource.FileRef == null)
                {
                    var value = resource.GetValue((ITypeResolutionService)null);
                    var str = value as string;
                    StoreController.AppendValues(new ResourceModel(resource.Name, str, resource.Comment));
                }
                else
                {
                    throw new NotImplementedException();
                }
            });
        }

        public void Save(string fileName)
        {
            _resxHandler.WriteToFile(fileName);
            OnDirtyChanged(this, false);
        }

        private void AttachListeners()
        {
            ResourceEditorView.OnAddResource += (sender, e) => AddNewResource();

            ResourceEditorView.OnRemoveResource += (sender, e) => RemoveCurrentResource();

            ResourceEditorView.ResourceList.OnNameEdited += (sender, e) =>
            {
                StoreController.GetIter(out var iter, new TreePath(e.Path));
                var oldName = StoreController.GetName(new TreePath(e.Path));

                _resxHandler.RemoveResource(oldName);
                _resxHandler.AddResource(e.NextText, string.Empty);

                StoreController.SetName(e.Path, e.NextText);
                OnDirtyChanged(this, true);
            };

            ResourceEditorView.ResourceList.OnValueEdited += (sender, e) =>
            {
                var name = StoreController.GetName(new TreePath(e.Path));

                _resxHandler.RemoveResource(name);
                _resxHandler.AddResource(name, e.NextText);

                StoreController.SetValue(e.Path, e.NextText);
                OnDirtyChanged(this, true);
            };

            ResourceEditorView.ResourceList.OnCommentEdited += (sender, e) =>
            {
                var path = new TreePath(e.Path);
                var name = StoreController.GetName(path);
                var value = StoreController.GetValue(path);

                _resxHandler.RemoveResource(name);
                _resxHandler.AddResource(name, value, e.NextText);

                StoreController.SetComment(e.Path, e.NextText);
                OnDirtyChanged(this, true);
            };
        }
    }
}