﻿using Hexapod_Simulator.Shared;
using GFunctions.Timing;
using GFunctions.Mathematics;
using System.Windows.Input;
using System;

namespace Hexapod_Simulator.Helix.ViewModels
{
    /// <summary>
    /// The top level view model for the main window
    /// </summary>
    public class MainVM : BaseViewModel
    {
        /// <summary>
        /// The window's hexapod
        /// </summary>
        public HexapodVM Hexapod { get; private set; }

        /// <summary>
        /// The window's ball
        /// </summary>
        public BallVM Ball { get; private set; }

        /// <summary>
        /// The simulation refresh interval in ms
        /// </summary>
        public int SimulationInterval { get; set; } = 50;

        /// <summary>
        /// Returns true if the simulation is running
        /// </summary>
        public bool SimulationRunning { get { return SimModel.Running; } }

        /// <summary>
        /// The simulation frames per second
        /// </summary>
        public int SimulationFPS { get; set; } = 0;

        /// <summary>
        /// True if hexapod servoing is active to correct the ball's position
        /// </summary>
        public bool ServoActive { get; set; } = false;


        /// <summary>
        /// The model for managing the time-based simulation
        /// </summary>
        private TimeSimulation SimModel = new TimeSimulation();

        /// <summary>
        /// PID Controller Tracking Monitoring the X Position of the <see cref="Ball"/>
        /// </summary>
        private PIDController XController = new PIDController(3, 1, 1, -0.5, 30);

        /// <summary>
        /// PID Controller Tracking Monitoring the Y Position of the <see cref="Ball"/>
        /// </summary>
        public PIDController YController = new PIDController(-3, 1, 1, -0.5, 30);


        /// <summary>
        /// RelayCommand for <see cref="ToggleSimulation"/>
        /// </summary>
        public ICommand ToggleSimulationCommand { get; set; }

        /// <summary>
        /// RelayCommand for <see cref="ResetSimulation"/>
        /// </summary>
        public ICommand ResetSimulationCommand { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainVM()
        {
            InitializeModels();

            //Setup all relay commands
            ToggleSimulationCommand = new RelayCommand(ToggleSimulation);
            ResetSimulationCommand = new RelayCommand(ResetSimulation);
        }

        /// <summary>
        /// Resets the simulation to starting values
        /// </summary>
        public void ResetSimulation()
        {
            //if the simulation is running, stop it first
            if (SimulationRunning)
                ToggleSimulation();

            InitializeModels();
        }

        /// <summary>
        /// Toggles the simulation state from on to off
        /// </summary>
        public void ToggleSimulation()
        {
            if (SimulationRunning)
            {
                SimModel.Stop();               
            }
            else
            {
                SimModel.Start(SimulationInterval);
            }

            Hexapod.TopPlatform.SetSimulationState(SimulationRunning);
            OnPropertyChanged("SimulationRunning");
        }

        /// <summary>
        /// Initializes any sub VMs in this class
        /// </summary>
        private void InitializeModels()
        {
            //setup hexapod - if it already exists just reset the position so we save the configuration
            if(Hexapod is null)
                Hexapod = new HexapodVM(new Hexapod(15, 12, 8, 30, 5));
            else
                Hexapod.TopPlatform.ResetPositionCommand.Execute(null);
             
            //setup ball
            Ball = new BallVM(new Ball_Local_Test(0.0025, 9800, new double[] { 0, 0, 0 }));

            //setup simulation model
            SimModel = new TimeSimulation();
            SimModel.SimulationDoWorkRequest += this.SimulationTimeStepDoWork;
            SimModel.RunFreqUpdated += this.SimulationFrequencyReported;

            //Setup PID Controllers
            XController = new PIDController(3, 1, 1, -0.5, 30);
            YController = new PIDController(-3, 1, 1, -0.5, 30);
        }

        /// <summary>
        /// Executes all required calculations for each simulation timestep.
        /// </summary>
        /// <param name="sender">The simulation model object</param>
        /// <param name="e">Simulation eventArgs</param>
        private void SimulationTimeStepDoWork(object sender, TimeSimulationStepEventArgs e)
        {
            //------------ Do Calculations -------------------------

            //Calculate the ball XY accel/velocity/position
            Ball.BallModel.CalculateTimeStep(e.TimeIncrement, Hexapod.TopPlatform.PlatformModel.NormalVector);

            //Calculate the latest position of the top platform      
            Hexapod.TopPlatform.PlatformModel.CalculateTimeStep(e.TimeIncrement);

            //Update the ball servo target positions to the center of the platform
            XController.SetTarget(Hexapod.TopPlatform.PlatformModel.Position[0]);
            YController.SetTarget(Hexapod.TopPlatform.PlatformModel.Position[1]);

            //Update the Z coordinate of the ball based on it's XY position and the platform's tilt 
            var coords = Ball.BallModel.Position;
            coords[2] = Ball.Radius; //Z offset relative to the platform should be the ball's radius
            Ball.BallModel.UpdateGlobalCoords(Hexapod.TopPlatform.PlatformModel.CalcGlobalCoord(coords));

            //If the servo is active, rotate the platform to try and center the ball
            if (ServoActive)
                Hexapod.TopPlatform.PlatformModel.RotateAbs(new double[] { XController.CalcOutput(Ball.BallModel.Position[0], e.TimeIncrement), YController.CalcOutput(Ball.BallModel.Position[1], e.TimeIncrement), 0 });

            //------------ Cleanup -------------------------
            e.WorkDoneCallback.Set(); //allow simulation to proceed
        }
        
        /// <summary>
        /// Gets called whenever the simulation FPS is reported
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SimulationFrequencyReported(object sender, int e)
        {
            SimulationFPS = e;
        }

    }
}
