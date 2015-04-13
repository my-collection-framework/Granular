﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Granular.Extensions;

namespace System.Windows.Media
{
    public class Visual : DependencyObject
    {
        public event EventHandler VisualAncestorChanged;

        public event EventHandler VisualParentChanged;
        private Visual visualParent;
        public Visual VisualParent
        {
            get { return visualParent; }
            private set
            {
                if (visualParent == value)
                {
                    return;
                }

                if (visualParent != null)
                {
                    visualParent.VisualAncestorChanged -= OnVisualAncestorChanged;
                }

                Visual oldVisualParent = visualParent;
                visualParent = value;

                if (visualParent != null)
                {
                    visualParent.VisualAncestorChanged += OnVisualAncestorChanged;
                }

                OnVisualParentChanged(oldVisualParent, visualParent);
                VisualParentChanged.Raise(this);

                OnVisualAncestorChanged();
                VisualAncestorChanged.Raise(this);
            }
        }

        private List<Visual> visualChildren;
        public ReadOnlyCollection<Visual> VisualChildren { get; private set; }

        public Point VisualOffset { get { return VisualBounds.Location; } }
        public Size VisualSize { get { return VisualBounds.Size; } }

        private Brush visualBackground;
        protected Brush VisualBackground
        {
            get { return visualBackground; }
            set
            {
                if (visualBackground == value)
                {
                    return;
                }

                visualBackground = value;

                foreach (IVisualRenderElement visualRenderElement in visualRenderElements.Values)
                {
                    visualRenderElement.Background = visualBackground;
                }
            }
        }

        private Rect visualBounds;
        protected Rect VisualBounds
        {
            get { return visualBounds; }
            set
            {
                if (visualBounds == value)
                {
                    return;
                }

                visualBounds = value;

                foreach (IVisualRenderElement visualRenderElement in visualRenderElements.Values)
                {
                    visualRenderElement.Bounds = visualBounds;
                }
            }
        }

        private bool visualClipToBounds;
        protected bool VisualClipToBounds
        {
            get { return visualClipToBounds; }
            set
            {
                if (visualClipToBounds == value)
                {
                    return;
                }

                visualClipToBounds = value;

                foreach (IVisualRenderElement visualRenderElement in visualRenderElements.Values)
                {
                    visualRenderElement.ClipToBounds = visualClipToBounds;
                }
            }
        }

        private bool visualIsHitTestVisible;
        protected bool VisualIsHitTestVisible
        {
            get { return visualIsHitTestVisible; }
            set
            {
                if (visualIsHitTestVisible == value)
                {
                    return;
                }

                visualIsHitTestVisible = value;

                foreach (IVisualRenderElement visualRenderElement in visualRenderElements.Values)
                {
                    visualRenderElement.IsHitTestVisible = visualIsHitTestVisible;
                }
            }
        }

        private bool visualIsVisible;
        protected bool VisualIsVisible
        {
            get { return visualIsVisible; }
            set
            {
                if (visualIsVisible == value)
                {
                    return;
                }

                visualIsVisible = value;

                foreach (IVisualRenderElement visualRenderElement in visualRenderElements.Values)
                {
                    visualRenderElement.IsVisible = visualIsVisible;
                }
            }
        }

        private double visualOpacity;
        protected double VisualOpacity
        {
            get { return visualOpacity; }
            set
            {
                if (visualOpacity == value)
                {
                    return;
                }

                visualOpacity = value;

                foreach (IVisualRenderElement visualRenderElement in visualRenderElements.Values)
                {
                    visualRenderElement.Opacity = visualOpacity;
                }
            }
        }

        private Transform visualTransform;
        protected Transform VisualTransform
        {
            get { return visualTransform; }
            set
            {
                if (visualTransform == value)
                {
                    return;
                }

                visualTransform = value;

                foreach (IVisualRenderElement visualRenderElement in visualRenderElements.Values)
                {
                    visualRenderElement.Transform = visualTransform;
                }
            }
        }

        private int visualLevel;
        public int VisualLevel
        {
            get
            {
                if (visualLevel == -1)
                {
                    visualLevel = VisualParent != null ? VisualParent.VisualLevel + 1 : 0;
                }

                return visualLevel;
            }
        }

        private Dictionary<IRenderElementFactory, IVisualRenderElement> visualRenderElements;
        private bool containsContentRenderElement;

        public Visual()
        {
            visualChildren = new List<Visual>();
            VisualChildren = new ReadOnlyCollection<Visual>(visualChildren);

            visualRenderElements = new Dictionary<IRenderElementFactory, IVisualRenderElement>();

            VisualBackground = null;
            VisualBounds = Rect.Zero;
            VisualClipToBounds = true;
            VisualIsHitTestVisible = true;
            VisualIsVisible = true;
            VisualOpacity = 1;
            VisualTransform = Transform.Identity;

            visualLevel = -1;
        }

        public void AddVisualChild(Visual child)
        {
            if (child.VisualParent == this)
            {
                return;
            }

            if (child.VisualParent != null)
            {
                child.VisualParent.RemoveVisualChild(child);
            }

            child.VisualParent = this;
            visualChildren.Add(child);

            int renderChildIndex = containsContentRenderElement ? visualChildren.Count : visualChildren.Count - 1;
            foreach (IRenderElementFactory factory in visualRenderElements.Keys)
            {
                visualRenderElements[factory].InsertChild(renderChildIndex, child.GetRenderElement(factory));
            }
        }

        public void RemoveVisualChild(Visual child)
        {
            if (child.VisualParent != this)
            {
                return;
            }

            visualChildren.Remove(child);
            child.VisualParent = null;

            foreach (IRenderElementFactory factory in visualRenderElements.Keys)
            {
                visualRenderElements[factory].RemoveChild(child.GetRenderElement(factory));
            }
        }

        public void SetVisualChildIndex(Visual child, int newIndex)
        {
            int oldIndex = visualChildren.IndexOf(child);
            if (oldIndex == -1 || oldIndex == newIndex)
            {
                return;
            }

            visualChildren.Remove(child);
            visualChildren.Insert(newIndex, child);

            foreach (IRenderElementFactory factory in visualRenderElements.Keys)
            {
                object childRenderElement = child.GetRenderElement(factory);

                visualRenderElements[factory].RemoveChild(childRenderElement);
                visualRenderElements[factory].InsertChild(newIndex, childRenderElement);
            }
        }

        public void ClearVisualChildren()
        {
            foreach (Visual child in visualChildren.ToArray())
            {
                RemoveVisualChild(child);
            }
        }

        protected virtual void OnVisualParentChanged(Visual oldVisualParent, Visual newVisualParent)
        {
            //
        }

        protected virtual void OnVisualAncestorChanged()
        {
            visualLevel = -1;
        }

        private void OnVisualAncestorChanged(object sender, EventArgs e)
        {
            OnVisualAncestorChanged();
            VisualAncestorChanged.Raise(this);
        }

        public IVisualRenderElement GetRenderElement(IRenderElementFactory factory)
        {
            IVisualRenderElement visualRenderElement;
            if (visualRenderElements.TryGetValue(factory, out visualRenderElement))
            {
                return visualRenderElement;
            }

            visualRenderElement = factory.CreateVisualRenderElement(this);

            visualRenderElement.Background = VisualBackground;
            visualRenderElement.Bounds = VisualBounds;
            visualRenderElement.ClipToBounds = VisualClipToBounds;
            visualRenderElement.IsHitTestVisible = VisualIsHitTestVisible;
            visualRenderElement.IsVisible = VisualIsVisible;
            visualRenderElement.Opacity = VisualOpacity;
            visualRenderElement.Transform = VisualTransform;

            int index = 0;
            foreach (Visual child in VisualChildren)
            {
                child.GetRenderElement(factory);
                visualRenderElement.InsertChild(index, child.GetRenderElement(factory));
                index++;
            }

            object contentRenderElement = CreateContentRenderElementOverride(factory);
            if (contentRenderElement != null)
            {
                visualRenderElement.InsertChild(0, contentRenderElement);
            }

            if (visualRenderElements.Count == 0)
            {
                containsContentRenderElement = contentRenderElement != null;
            }
            else if (containsContentRenderElement != (contentRenderElement != null))
            {
                throw new Granular.Exception("ContentRenderElement for type \"{0}\" must be created for all of the factories or none of them", GetType().Name);
            }

            visualRenderElements.Add(factory, visualRenderElement);
            return visualRenderElement;
        }

        public void RemoveRenderElement(IRenderElementFactory factory)
        {
            visualRenderElements.Remove(factory);

            foreach (Visual child in VisualChildren)
            {
                child.RemoveRenderElement(factory);
            }
        }

        protected virtual object CreateContentRenderElementOverride(IRenderElementFactory factory)
        {
            return null;
        }

        public Point PointToRoot(Point point)
        {
            if (VisualParent != null)
            {
                return VisualParent.PointToRoot(point + VisualOffset);
            }

            return point + VisualOffset;
        }

        public Point PointFromRoot(Point point)
        {
            if (VisualParent != null)
            {
                return VisualParent.PointFromRoot(point - VisualOffset);
            }

            return point - VisualOffset;
        }
    }
}
