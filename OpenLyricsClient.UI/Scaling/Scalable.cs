using System;
using System.Reactive.Subjects;
using Avalonia;

namespace OpenLyricsClient.UI.Scaling;

 /// <summary>
    /// Represents the most generic implementation of the <see cref="IScalable"/> and supports Avalonia bindings.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the <see cref="IScalable"/> that will be scaled.</typeparam>
    public abstract class Scalable<T> : IScalable
    {
        /// <summary>
        /// The default value of this <see cref="Scalable{T}"/> that the scaling factor will be applied to.
        /// </summary>
        public T DefaultValue { get; set; }
        /// <summary>
        /// The <see cref="Subject"/> for the Avalonia binding.
        /// </summary>
        private readonly Subject<T> source = new Subject<T>();
        /// <summary>
        /// The subscription that represents the binding.
        /// </summary>
        private IDisposable subscription;
        private bool isInitialized = false;


        /// <summary>
        /// Creates a new instance of <see cref="Scalable{T}"/>.
        /// </summary>
        private protected Scalable()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="Scalable{T}"/> and initializes a new binding.
        /// </summary>
        /// <param name="avaloniaObject">The <see cref="AvaloniaObject"/> to apply the new binding to.</param>
        /// <param name="avaloniaProperty">The <see cref="AvaloniaProperty{TValue}"/> the new binding should be applied to.</param>
        /// <param name="defaultValue">The default value of the <paramref name="avaloniaProperty"/> at <c>1.0</c> scaling.</param>
        private protected Scalable(AvaloniaObject avaloniaObject, AvaloniaProperty<T> avaloniaProperty, T defaultValue)
        {
            Initialize(avaloniaObject, avaloniaProperty, defaultValue);
            isInitialized = true;
        }

        /// <summary>
        /// Initializes a new binding.
        /// </summary>
        /// <param name="avaloniaObject">The <see cref="AvaloniaObject"/> to apply the new binding to.</param>
        /// <param name="avaloniaProperty">The <see cref="AvaloniaProperty{TValue}"/> the new binding should be applied to.</param>
        /// <param name="defaultValue">The default value of the <paramref name="avaloniaProperty"/> at <c>1.0</c> scaling.</param>
        public virtual void Initialize(AvaloniaObject avaloniaObject, AvaloniaProperty<T> avaloniaProperty, T defaultValue)
        {
            if (!isInitialized)
            {
                subscription = avaloniaObject.Bind(avaloniaProperty, source);
                DefaultValue = defaultValue;
                isInitialized = true;
            }
        }

        /// <inheritdoc/>
        public abstract void ApplyScaling(double scalingFactor);

        /// <summary>
        /// Sets the value of the binding to the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The new value.</param>
        private protected void SetValue(T value)
        {
            source.OnNext(value);
        }

        /// <summary>
        /// Releases the binding to the specified <see cref="AvaloniaProperty{TValue}"/>.
        /// </summary>
        public void Dispose()
        {
            subscription.Dispose();
        }
    }