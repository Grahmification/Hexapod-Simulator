﻿using HelixToolkit.Wpf;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Hexapod_Simulator.Helix.Views
{
    public class LinearActuatorVisual3D : ModelVisual3D 
    {
        /// <summary>
        /// Identifies the <see cref="Position"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
            "Position",
            typeof(double[]),
            typeof(LinearActuatorVisual3D),
            new UIPropertyMetadata(new double[] { 0, 0, 0 }, GeometryChanged));

        /// <summary>
        /// Identifies the <see cref="ArmEndPosition"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ArmEndPositionProperty = DependencyProperty.Register(
            "ArmEndPosition",
            typeof(double[]),
            typeof(LinearActuatorVisual3D),
            new UIPropertyMetadata(new double[] { 0, 0, 1 }, GeometryChanged));

        /// <summary>
        /// Identifies the <see cref="LinkEndPosition"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LinkEndPositionProperty = DependencyProperty.Register(
            "LinkEndPosition",
            typeof(double[]),
            typeof(LinearActuatorVisual3D),
            new UIPropertyMetadata(new double[] { 0, 0, 5 }, GeometryChanged));

        /// <summary>
        /// Identifies the <see cref="ArmColorProperty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ArmColorProperty = DependencyProperty.Register(
            "ArmColor",
            typeof(Color),
            typeof(LinearActuatorVisual3D),
            new UIPropertyMetadata(Colors.Yellow, GeometryChanged));

        /// <summary>
        /// Identifies the <see cref="LinkColorProperty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LinkColorProperty = DependencyProperty.Register(
            "LinkColor",
            typeof(Color),
            typeof(LinearActuatorVisual3D),
            new UIPropertyMetadata(Colors.Red, GeometryChanged));


        /// <summary>
        /// Coordinates where the base of the actuator is located on the base [x,y,z]
        /// </summary>
        public double[] Position
        {
            get { return (double[])GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        /// <summary>
        /// Coordinates of actuator arm end position, where it attaches to link start [x,y,z]
        /// </summary>
        public double[] ArmEndPosition
        {
            get { return (double[])GetValue(ArmEndPositionProperty); }
            set { SetValue(ArmEndPositionProperty, value); }
        }

        /// <summary>
        /// Coordinates where the link attaches to the moving portion of the device [x,y,z]
        /// </summary>
        public double[] LinkEndPosition
        {
            get { return (double[])GetValue(LinkEndPositionProperty); }
            set { SetValue(LinkEndPositionProperty, value); }
        }

        /// <summary>
        /// The arm color.
        /// </summary>
        /// <value>The color.</value>
        public Color ArmColor
        {
            get { return (Color)this.GetValue(ArmColorProperty); }
            set { this.SetValue(ArmColorProperty, value); }
        }

        /// <summary>
        /// The link color.
        /// </summary>
        /// <value>The color.</value>
        public Color LinkColor
        {
            get { return (Color)this.GetValue(LinkColorProperty); }
            set { this.SetValue(LinkColorProperty, value); }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public LinearActuatorVisual3D() : base()
        {
            OnGeometryChanged();
        }

        /// <summary>
        /// Called by a dependency property when the geometry gets changed.
        /// </summary>
        /// <param name="d">The object</param>
        /// <param name="e">The args.</param>
        protected static void GeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LinearActuatorVisual3D)d).OnGeometryChanged();
        }


        /// <summary>
        /// Called when the geometry has changed.
        /// </summary>
        protected virtual void OnGeometryChanged()
        {
            this.Children.Clear();

            //----------------- Add the actuator arm ---------------------------
            var arm = new PipeVisual3D();
            arm.BeginEdit();
            arm.Point1 = new Point3D(ArmEndPosition[0], ArmEndPosition[1], ArmEndPosition[2]);
            arm.Point2 = new Point3D(Position[0], Position[1], Position[2]);
            arm.Diameter = 1;
            arm.Fill = new SolidColorBrush(ArmColor);
            arm.EndEdit();
            this.Children.Add(arm);

            //-------------- Add the link -------------------
            var link = new PipeVisual3D();
            link.BeginEdit();
            link.Point1 = new Point3D(ArmEndPosition[0], ArmEndPosition[1], ArmEndPosition[2]);
            link.Point2 = new Point3D(LinkEndPosition[0], LinkEndPosition[1], LinkEndPosition[2]);
            link.Diameter = 0.5;
            link.Fill = new SolidColorBrush(LinkColor);
            link.EndEdit();
            this.Children.Add(link);

       
        }
    }
}
