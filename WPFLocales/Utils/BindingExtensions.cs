﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WPFLocales.View;

namespace WPFLocales.Utils
{
    static class BindingExtensions
    {
        internal static IEnumerable<DependencyObject> EnumerateVisualChildren(this DependencyObject dependencyObject)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                yield return VisualTreeHelper.GetChild(dependencyObject, i);
            }
        }

        internal static IEnumerable<DependencyObject> EnumerateVisualDescendents(this DependencyObject dependencyObject)
        {
            yield return dependencyObject;

            foreach (var child in dependencyObject.EnumerateVisualChildren())
            {
                foreach (var descendent in child.EnumerateVisualDescendents())
                {
                    yield return descendent;
                }
            }
        }

        internal static void UpdateBindings(this DependencyObject dependencyObject, Action<DependencyObject, DependencyProperty> process)
        {
            foreach (var element in dependencyObject.EnumerateVisualDescendents())
            {
                var localValueEnumerator = element.GetLocalValueEnumerator();
                while (localValueEnumerator.MoveNext())
                {
                    process(element, localValueEnumerator.Current.Property);
                }
            }
        }

        internal static void UpdateBindingTargets(this DependencyObject dependencyObject)
        {
            dependencyObject.UpdateBindings((element, property) =>
            {
                var bindingExpression = BindingOperations.GetBindingExpressionBase(element, property);
                if (bindingExpression != null)
                {
                    bindingExpression.UpdateTarget();
                }
            });
        }

        internal static void UpdateBindingConverterParents(this DependencyObject dependencyObject)
        {
            dependencyObject.UpdateBindings((element, property) =>
            {
                var binding = BindingOperations.GetBinding(element, property);
                if (binding != null)
                {
                    var converter = binding.Converter;
                    if (converter is LocalizableConverter)
                    {
                        (converter as LocalizableConverter).ParentDependencyObject = element;
                    }
                }
            });
        }

        internal static void UpdateBindingTargets(this Application application)
        {
            foreach (var window in application.Windows)
            {
                if (window is DependencyObject)
                    (window as DependencyObject).UpdateBindingTargets();
            }
        }
    }
}
