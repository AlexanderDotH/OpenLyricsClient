using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;

namespace OpenLyricsClient.UI.Scaling;

/// <summary>
    /// Represents a manager for the content scaling of a <see cref="ScalableWindow"/>.
    /// </summary>
    public class ScalingManager
    {
        private static ScalingManager _instance;
        
        private const double minimumScaling = 0.2d;

        private double previousScaling = 1d;

        private readonly ScalableWindow window;

        private readonly IViewModel viewModel;

        private readonly ScalableObject ScalableMainWindow;
        /// <summary>
        /// The <see cref="IScalable"/> responsible for scaling the height of the linked window.
        /// </summary>
        public Scalable<double> MainWindowHeightScalable { get; }
        /// <summary>
        /// The <see cref="IScalable"/> responsible for scaling the width of the linked window.
        /// </summary>
        public Scalable<double> MainWindowWidthScalable { get; }

        /// <summary>
        /// The scaling factor currently applied to the UI.
        /// </summary>
        public double CurrentScaling { get; private set; } = 1d;

        /// <summary>
        /// Indicates whether the application is currently rescaling.
        /// </summary>
        public bool IsRescaling { get; private set; }

        /// <summary>
        /// The bindings managed by this <see cref="ScalingManager"/>.
        /// </summary>
        public Dictionary<Type, BindingContext> Bindings { get; } = new Dictionary<Type, BindingContext>();

        /// <summary>
        /// Creates a new <see cref="ScalingManager"/> and sets up all bindings of the <paramref name="window"/>.
        /// </summary>
        /// <param name="window">The <see cref="ScalableWindow"/> to be managed by this <see cref="ScalingManager"/>.</param>
        /// <param name="viewModel">The <see cref="IViewModel"/> implemented by the view model belonging to the <paramref name="window"/>.</param>
        public ScalingManager(ScalableWindow window, IViewModel viewModel)
        {
            _instance = this;
            this.viewModel = viewModel;
            this.window = window;
            ScalableMainWindow = new ScalableObject(window); 
            
            BindingContext windowBindingContext = new BindingContext
            {
                ScalableMainWindow
            };
            
            Bindings.Add(window.GetType(), windowBindingContext);
            Queue<IEnumerable<ILogical>> logicals = new Queue<IEnumerable<ILogical>>();
            logicals.Enqueue(window.GetLogicalChildren());
            RegisterControls(logicals, window.GetType());
            
            MainWindowHeightScalable = ScalableMainWindow.Bindings.GetValueOrDefault(nameof(Layoutable.Height)) as Scalable<double>;
            MainWindowWidthScalable = ScalableMainWindow.Bindings.GetValueOrDefault(nameof(Layoutable.Width)) as Scalable<double>;

            double optimalWidth = window.Screens.Primary.WorkingArea.Width / 2;
            double optimalHeight = window.Screens.Primary.WorkingArea.Height / 2;

            double width = optimalWidth;
            double height = optimalHeight;
            
            for (int i = 1; i < 4; i++)
            {
                if (height < MainWindowHeightScalable.DefaultValue || width < MainWindowWidthScalable.DefaultValue)
                {
                    width = optimalWidth + (i * 0.125d);
                    height = optimalHeight + (i * 0.125d);
                }
            }
            
            double scalingFactor = Math.Min(width / MainWindowWidthScalable.DefaultValue, height / MainWindowHeightScalable.DefaultValue);
            SetScaling(scalingFactor);
            
            window.BeginResize += Window_BeginResize;
            window.Resize += Window_Resize;
            window.EndResize += Window_EndResize;
            window.PropertyChanged += Window_PropertyChanged;
            window.PositionChanged += Window_PositionChanged;
        }

        private bool windowResizing = false;
        private double windowScalingFactor = 1d;
        private Point mouseDownPosition = default;
        private bool windowNeedsRefresh = false;
        private bool onlyScaleOnStartup = false;

        private void Window_EndResize(object? sender, PointerReleasedEventArgs e)
        {
            windowResizing = false;
            if (windowNeedsRefresh)
            {
                SetScaling(windowScalingFactor);
                windowNeedsRefresh = false;
                viewModel.IsResizing = false;
            }
        }

        private void Window_Resize(object? sender, PointerEventArgs e)
        {
            Point currentPosition = e.GetPosition(window);

            double distance = (Math.Abs(mouseDownPosition.X - this.currentPosition.X) +
                               Math.Abs(mouseDownPosition.Y - this.currentPosition.Y)) / 2;
            
            if (windowResizing && distance > 12)
            {
                windowScalingFactor = Math.Max(Math.Min(currentPosition.X / MainWindowWidthScalable.DefaultValue, currentPosition.Y / MainWindowHeightScalable.DefaultValue), minimumScaling);
                if (!windowNeedsRefresh)
                {
                    windowNeedsRefresh = true;
                    viewModel.IsResizing = true;
                }
                if (window.WindowState == WindowState.Maximized)
                {
                    window.WindowState = WindowState.Normal;
                }
                MainWindowHeightScalable.ApplyScaling(windowScalingFactor);
                MainWindowWidthScalable.ApplyScaling(windowScalingFactor);
            }
        }

        private PixelPoint prevprevPosition;
        private PixelPoint currentPosition;
        private PixelPoint previousPosition;

        private void Window_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            prevprevPosition = previousPosition;
            previousPosition = currentPosition;
            currentPosition = e.Point;
        }

        private void Window_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (onlyScaleOnStartup)
                return;
            
            Type propertyType = e.Property.PropertyType;
            if (propertyType == typeof(WindowState))
            {
                WindowState newWindowState = (WindowState)e.NewValue;
                Debug.WriteLine("Width before WindowState change: " + ((Window)sender).Width.ToString());
                if (newWindowState == WindowState.Maximized)
                {
                    double scalingFactor = Math.Min(window.Width / MainWindowWidthScalable.DefaultValue, window.Height / MainWindowHeightScalable.DefaultValue);
                    double width = window.Width;
                    double height = window.Height;
                    SetScaling(scalingFactor);
                    double xOffset = (width - window.Width) / 2d;
                    double yOffset = (height - window.Height) / 2d;
                    currentPosition = prevprevPosition;
                    window.Position = new PixelPoint(window.Position.X + (int)xOffset, window.Position.Y + (int)yOffset);
                }
                else if (newWindowState == WindowState.Normal)
                {
                    window.Position = prevprevPosition;
                    UndoScaling();
                }
                else if (newWindowState == WindowState.Minimized)
                {
                    currentPosition = prevprevPosition;
                    UndoScaling();
                }
                Debug.WriteLine("Width after WindowState change: " + ((Window)sender).Width.ToString());
                Debug.WriteLine("Changed WindowState!");
                window.InvalidateVisual();
            }
        }

        private void Window_BeginResize(object? sender, PointerPressedEventArgs e)
        {
            windowResizing = true;
            mouseDownPosition = e.GetPosition((Window)sender);
        }

        /// <summary>
        /// Applies a new <paramref name="scalingFactor"/> to all <see cref="Bindings"/>.
        /// </summary>
        /// <param name="scalingFactor">The new scaling factor.</param>
        public void SetScaling(double scalingFactor)
        {
            foreach (BindingContext bindingContext in Bindings.Values)
            {
                SetScaling(scalingFactor, bindingContext);
            }
        }

        /// <summary>
        /// Applies a new <paramref name="scalingFactor"/> to the specified <paramref name="bindingContext"/>.
        /// </summary>
        /// <param name="scalingFactor">The new scaling factor.</param>
        /// <param name="bindingContext">The <see cref="BindingContext"/> to be rescaled.</param>
        public void SetScaling(double scalingFactor, BindingContext bindingContext)
        {
            if (scalingFactor >= minimumScaling)
            {
                IsRescaling = true;
                foreach (ScalableObject scalableObject in bindingContext)
                {
                    scalableObject.ApplyScaling(scalingFactor);
                }
                previousScaling = CurrentScaling;
                CurrentScaling = scalingFactor;
                IsRescaling = false;
            }
        }

        /// <summary>
        /// Undoes the last scaling operation.
        /// </summary>
        public void UndoScaling()
        {
            SetScaling(previousScaling);
        }

        /// <summary>
        /// Adds a new <see cref="BindingContext"/> to the <see cref="Bindings"/> dictionary.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of the object represented by the new <paramref name="bindingContext"/>.</param>
        /// <param name="bindingContext">The <see cref="BindingContext"/> to be added.</param>
        public void AddToBindingContext(Type type, BindingContext bindingContext)
        {
            if (Bindings.TryGetValue(type, out BindingContext context))
            {
                foreach (ScalableObject scalableObject in bindingContext)
                {
                    context.Add(scalableObject);
                }
            }
            else
            {
                Bindings.Add(type, bindingContext);
            }
        }

        /// <summary>
        /// Registers the <paramref name="root"/> control and all of it's children to a new <see cref="BindingContext"/>.
        /// </summary>
        /// <param name="root">The root of the avalonia logical sub tree that should be registered to the new <see cref="BindingContext"/>.</param>
        public static BindingContext RegisterControlsToBindingContext(ILogical root)
        {
            BindingContext bindingContext = new BindingContext();
            Queue<IEnumerable<ILogical>> logicals = new Queue<IEnumerable<ILogical>>();
            logicals.Enqueue(new List<ILogical>() { root });
            RegisterControls(logicals, bindingContext);
            return bindingContext;
        }

        /// <summary>
        /// Registers the <paramref name="root"/> control and all of it's children to the <see cref="BindingContext"/> of the specified <paramref name="key"/> in <see cref="Bindings"/>.
        /// </summary>
        /// <param name="root">The root of the avalonia logical sub tree that should be registered.</param>
        /// <param name="key">The <see cref="Type"/> linked by the <see cref="Bindings"/> to the target <see cref="BindingContext"/>.</param>
        public void RegisterControls(ILogical root, Type key)
        {
            Queue<IEnumerable<ILogical>> logicals = new Queue<IEnumerable<ILogical>>();
            logicals.Enqueue(new List<ILogical>() { root });
            RegisterControls(logicals, key);
        }

        /// <summary>
        /// Registers all <see cref="ILogical"/>s in <paramref name="logicals"/> to the <see cref="Bindings"/> using the specified <paramref name="key"/>, creating a new <see cref="BindingContext"/> if necessary.
        /// </summary>
        /// <param name="logicals">A <see cref="Queue{T}"/> containing the root controls that will be recursively registered.</param>
        /// <param name="key">The key of the <see cref="BindingContext"/> the <paramref name="logicals"/> will be registered to as specified in <see cref="Bindings"/>.</param>
        public void RegisterControls(Queue<IEnumerable<ILogical>> logicals, Type key)
        {
            if (!Bindings.TryGetValue(key, out BindingContext bindingContext))
            {
                bindingContext = new BindingContext();
                Bindings.Add(key, bindingContext);
            }
            RegisterControls(logicals, bindingContext);
        }

        /// <summary>
        /// Recursively registers  all <see cref="ILogical"/>s in <paramref name="logicals"/> to the <paramref name="bindingContext"/>.
        /// </summary>
        /// <param name="logicals">A <see cref="Queue{T}"/> containing the root controls that will be recursively registered.</param>
        /// <param name="bindingContext">The <see cref="BindingContext"/> the <see cref="ScalableObject"/>s will be registered to.</param>
        public static void RegisterControls(Queue<IEnumerable<ILogical>> logicals, BindingContext bindingContext)
        {
            while (logicals.Count > 0)
            {
                IEnumerable<ILogical> children = logicals.Dequeue();
                foreach (ILogical child in children)
                {
                    logicals.Enqueue(child.GetLogicalChildren());
                    if (child is AvaloniaObject avaloniaObject)
                    {
                        ScalableObject scalableObject = new ScalableObject(avaloniaObject);
                        bindingContext.Add(scalableObject);
                    }
                }
            }
        }

        public bool OnlyScaleOnStartup
        {
            get => onlyScaleOnStartup;
            set => onlyScaleOnStartup = value;
        }

        public bool WindowNeedsRefresh
        {
            get => windowNeedsRefresh;
            set => windowNeedsRefresh = value;
        }

        public static ScalingManager Instance
        {
            get => _instance;
        }
    }