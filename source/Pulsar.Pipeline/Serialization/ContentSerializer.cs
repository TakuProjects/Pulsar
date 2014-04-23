﻿using System;

namespace Pulsar.Pipeline.Serialization
{
    /// <summary>
    /// Represents a generic serializer
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    public abstract class ContentSerializer<T> : IContentSerializer
    {
        #region Constructors

        /// <summary>
        /// Constructor of ContentSerializer class
        /// </summary>
        protected ContentSerializer()
        {
            TargetType = typeof (T);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the serializer
        /// </summary>
        /// <param name="manager">Reader manager that owns this serializer</param>
        public virtual void Initialize(SerializerManager manager)
        {
        }

        /// <summary>
        /// Converts a string to an object
        /// </summary>
        /// <param name="value">String value</param>
        /// <param name="context">Current context</param>
        /// <returns>Returns an object</returns>
        object IContentSerializer.Deserialize(string value, SerializerContext context)
        {
            return Deserialize(value, context);
        }

        /// <summary>
        /// Converts an object to a string representation
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Returns a string that represents the object</returns>
        string IContentSerializer.Serialize(object value)
        {
            return Serialize((T)value);
        }

        /// <summary>
        /// Converts a string to an instance of T
        /// </summary>
        /// <param name="value">String value</param>
        /// <param name="context">Current context</param>
        /// <returns>Returns a T instance</returns>
        public abstract T Deserialize(string value, SerializerContext context = null);

        /// <summary>
        /// Converts an instance of T to a string representation
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Returns a string that represents the object</returns>
        public abstract string Serialize(T value);

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type associated to the serializer
        /// </summary>
        public Type TargetType { get; private set; }

        #endregion
    }
}
