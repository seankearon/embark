﻿using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Embark.Interaction.MVVM
{
    /// <summary>
    /// Lambda expressions to get NotifyChangeBase property strings and raise properties.
    /// <para>Intended to avoid using magic strings that are not easily refactor-safe</para>
    /// </summary>
    public static class PropertyChangeBaseReflection
    {
        /// <summary>
        /// Raise the PropertyChangedEvent of a property passed in via lambda
        /// <para>example:</para>
        /// <example>
        /// <para>this.RaisePropertyChangedEvent((dog) => dog.BarkType)</para>
        /// <para>refactor-safe equivalent to this.RaisePropertyChangedEvent("BarkType")</para>
        /// </example>
        /// </summary>
        /// <typeparam name="TSource">Implementation type of NotifyChangeBase class</typeparam>
        /// <typeparam name="TProperty">Property type to raise event for</typeparam>
        /// <param name="obj">Instance of NotifyChangeBase object</param>
        /// <param name="property">Property to raise event for</param>
        public static bool RaisePropertyChangedEvent<TSource, TProperty>(this TSource obj, Expression<Func<TSource, TProperty>> property) 
            where TSource : PropertyChangeBase
        {
            var propertyName = GetPropertyString(obj, property);
            return obj.RaisePropertyChangedEvent(propertyName);
        }

        /// <summary>
        /// Call an action when a certain property is changed
        /// <para>example:</para>
        /// <example>
        /// <para>this.RaisePropertyChangedEvent((dev) => dev.Age, HappyBirthday)</para>
        /// </example>
        /// </summary>
        /// <typeparam name="TSource">Implementation type of INotifyPropertyChanged</typeparam>
        /// <typeparam name="TProperty">Property type to raise event for</typeparam>
        /// <param name="obj">Instance of INotifyPropertyChanged object</param>
        /// <param name="property">Property to raise event for</param>
        /// <param name="action">Void or anonymous method to call when property raises PropertyChanged event</param>
        public static void TriggerWhenPropertyChanged<TSource, TProperty>(this TSource obj, Expression<Func<TSource, TProperty>> property, Action action)
            where TSource : INotifyPropertyChanged
        {
            var propertyName = GetPropertyString(obj, property);
            obj.PropertyChanged += (sender, eventArgs) =>
            {
                if (eventArgs.PropertyName == propertyName)
                    action();
            };
        }

        /// <summary>
        /// Return the string name of a property to avoid using magic strings
        /// <para>example:</para>
        /// <example>
        /// <para>dog.GetPropertyString((d) => d.BarkType)</para>
        /// <para>returns string "BarkType"</para>
        /// </example>
        /// </summary>
        /// <typeparam name="TSource">Implementation type of INotifyPropertyChanged</typeparam>
        /// <typeparam name="TProperty">Property type to raise event for</typeparam>
        /// <param name="obj">Instance of INotifyPropertyChanged object</param>       
        /// <param name="property">Property to raise event for</param> 
        /// <returns>String of property lambda</returns>
        public static string GetPropertyString<TSource, TProperty>(this TSource obj, Expression<Func<TSource, TProperty>> property) 
            where TSource : INotifyPropertyChanged
        {
            var lambda = (LambdaExpression)property;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else memberExpression = (MemberExpression)lambda.Body;
            return memberExpression.Member.Name;
        }
    }
}
