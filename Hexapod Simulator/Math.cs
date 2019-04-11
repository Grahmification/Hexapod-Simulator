﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Hexapod_Simulator
{
    public class KinematicMath
    {
        public static DenseMatrix RotationMatrixFromPRY(double[] Rot)
        {
            double Pitch = Rot[0] * Math.PI / 180.0;
            double Roll = Rot[1] * Math.PI / 180.0;
            double Yaw = Rot[2] * Math.PI / 180.0;

            //Math: http://planning.cs.uiuc.edu/node102.html

            //------------ Pitch Matrix -----------------------

            DenseMatrix PitchMat = new DenseMatrix(3, 3);

            PitchMat[0, 0] = Math.Cos(Pitch);
            PitchMat[1, 0] = 0;
            PitchMat[2, 0] = -1 * Math.Sin(Pitch);
            PitchMat[0, 1] = 0;
            PitchMat[1, 1] = 1;
            PitchMat[2, 1] = 0;
            PitchMat[0, 2] = Math.Sin(Pitch);
            PitchMat[1, 2] = 0;
            PitchMat[2, 2] = Math.Cos(Pitch);

            //------------ Roll Matrix -----------------------

            var RollMat = new DenseMatrix(3, 3);

            RollMat[0, 0] = 1;
            RollMat[1, 0] = 0;
            RollMat[2, 0] = 0;
            RollMat[0, 1] = 0;
            RollMat[1, 1] = Math.Cos(Roll);
            RollMat[2, 1] = Math.Sin(Roll);
            RollMat[0, 2] = 0;
            RollMat[1, 2] = -1 * Math.Sin(Roll);
            RollMat[2, 2] = Math.Cos(Roll);

            //------------ Yaw Matrix -----------------------

            var YawMat = new DenseMatrix(3, 3);

            YawMat[0, 0] = Math.Cos(Yaw);
            YawMat[1, 0] = Math.Sin(Yaw);
            YawMat[2, 0] = 0;
            YawMat[0, 1] = -1 * Math.Sin(Yaw);
            YawMat[1, 1] = Math.Cos(Yaw);
            YawMat[2, 1] = 0;
            YawMat[0, 2] = 0;
            YawMat[1, 2] = 0;
            YawMat[2, 2] = 1;


            DenseMatrix output = YawMat * PitchMat * RollMat;

            return output;
        }
        public static double[] CalcGlobalCoord(double[] LocalCoord, double[] Trans1, double[] Trans2, double[] Rot)
        {
            DenseMatrix LocalCoords = new DenseMatrix(3, 1);
            LocalCoords.SetColumn(0, LocalCoord);

            DenseMatrix TranslationMat = new DenseMatrix(3, 1);
            TranslationMat.SetColumn(0, Trans1);

            DenseMatrix StartingMat = new DenseMatrix(3, 1);
            StartingMat.SetColumn(0, Trans2);

            DenseMatrix GlobalCoords = (KinematicMath.RotationMatrixFromPRY(Rot) * LocalCoords) + TranslationMat + StartingMat;

            return new double[] { GlobalCoords[0, 0], GlobalCoords[1, 0], GlobalCoords[2, 0] };
        }
        public static double[] CalcGlobalCoord2(double[] LocalCoord, double[] Trans1, double[] Trans2, double[] Rot, double[] RelativeRotCenter = null)
        {
            double[] coords = new double[] { 0, 0, 0 };

            if (RelativeRotCenter != null)
                coords = RelativeRotCenter;

            DenseMatrix RotCenter = new DenseMatrix(3, 1);
            RotCenter.SetColumn(0, coords);

            DenseMatrix LocalCoords = new DenseMatrix(3, 1);
            LocalCoords.SetColumn(0, LocalCoord);

            DenseMatrix TranslationMat = new DenseMatrix(3, 1);
            TranslationMat.SetColumn(0, Trans1);

            DenseMatrix TranslationMat2 = new DenseMatrix(3, 1);
            TranslationMat2.SetColumn(0, Trans2);


            DenseMatrix GlobalCoords = (KinematicMath.RotationMatrixFromPRY(Rot) * (LocalCoords - RotCenter + TranslationMat)) + TranslationMat2 + RotCenter; //Rotcenter needs to be added before rotation, then removed after

            return new double[] { GlobalCoords[0, 0], GlobalCoords[1, 0], GlobalCoords[2, 0] };
        }

        public static double VectorLength(double[] pos1, double[] pos2)
        {
            double output = 0;

            for (int i = 0; i < pos1.Length; i++)
            {
                output += (pos2[i] - pos1[i]) * (pos2[i] - pos1[i]);
            }

            return Math.Sqrt(output);
        }

    }
    public class Calculus
    {
        public static double Integrate(double X0, double X1, double timeStep)
        {
            return ((X0 + X1) / 2.0 + Math.Min(Math.Abs(X0), Math.Abs(X1))) * timeStep; //triangular region + square region
        }
        public static double Integrate(double X0, double timeStep)
        {
            return X0 * timeStep; //square region
        }

        public static double Derivative(double X0, double X1, double timeStep)
        {
            return (X1 - X0) / timeStep;
        }
    }

    public class PIDController
    {
        private bool _firstCycle = true;

 
        private double _prevProcessVar = 0;      
        private double _previousError = 0;
        
        private double _target = 0;

        private double _integralSum = 0; //may consider adding limit executed in private set property

        public double Gain { get; set; }
        public double P { get; set; }
        public double I { get; set; }
        public double D { get; set; }
        public double SatLimit { get; set; }  //limits max/min output value of controller, 0 = no limit
        public bool TargetIsolatedDeriv { get; set; } //if True, d is calculated from output change, not error change (isolating it from setpoint changes)

        public PIDController(double gain, double p, double i, double d, double satLimit = 0)
        {
            this.Gain = gain; 
            this.P = p;
            this.I = i;
            this.D = d;
            this.SatLimit = satLimit;
            this.TargetIsolatedDeriv = true; 
        }
        public void SetTarget(double Target)
        {
            _target = Target; 
        }
        public double CalcOutput(double ProcessVar, double TimeStep)
        {
            var error = _target - ProcessVar; //calculate error

            if (_firstCycle)
            {
                this._previousError = error; //no previous error data saved
                this._prevProcessVar = ProcessVar; 
            }
                
            
            this._integralSum += Calculus.Integrate(error, TimeStep);

            double output = 0;
            output += this.P * error ; //proportional term
            output += this.I * this._integralSum; //integral term 

            if (this.TargetIsolatedDeriv)
            {
                output += this.D * Calculus.Derivative(this._prevProcessVar, ProcessVar, TimeStep); //derivative from process variable
            }
            else
            {
                output += this.D * Calculus.Derivative(this._previousError, error, TimeStep); //derivative from process variable
            }

            
            output *= this.Gain; //apply the gain
            output = CheckSatLimit(output, this.SatLimit); //check if the output is inside the saturation limit

            this._previousError = error; //save current value for next cycle
            this._prevProcessVar = ProcessVar; 
            this._firstCycle = false; 

            return output; 
        }



        private double CheckSatLimit(double output, double satLimit)
        {
            if (satLimit == 0) //0 = no saturation limit
                return output;

            if (output > satLimit)
                return satLimit;

            if (output < satLimit * -1)
                return satLimit * -1;

            return output; //output is inside limit
        }
    }

    public class IterativeSolver
    {
        public delegate double DblFunction(double param1);

        private bool _solutionValid = false;
        private double _stepSize = 0.01; //initial change step size to test
        private double _maxSteps = 100; //max allowable steps before solution fails
        private double _errorTolerance = 0.001; //error tolerance for successful solution
        private DblFunction _errorFunc; //function which will calculate the error

        private double _maxValue; //max allowable value for solution
        private double _minValue; //min allowable value for solution

        public bool SolutionValid
        {
            get
            {
                return this._solutionValid;
            }
        }

        public IterativeSolver(double stepSize, double maxSteps, double errorTolerance, DblFunction errorFunc, double maxValue, double minValue)
        {
            this._stepSize = stepSize;
            this._maxSteps = maxSteps;
            this._errorTolerance = errorTolerance;
            this._errorFunc = errorFunc;
            this._maxValue = maxValue;
            this._minValue = minValue; 

        }

        public double Solve(double previousSolution)
        {
            this._solutionValid = false;

            double ScalingStepSize = this._stepSize; //default starting step size
            double newError = 0;
            double ratio = 0;

            double iterationValue = previousSolution;
            double prevError = this._errorFunc(iterationValue); //calculate the error

            for (int i = 0; i < this._maxSteps; i++)
            {
                if (Math.Abs(prevError) <= this._errorTolerance) //Solution is with error tolerance (valid)
                {
                    if (iterationValue > this._maxValue || iterationValue < this._minValue)
                        break; //soln has failed if out of allowable range, exit loop and go to last pos

                    this._solutionValid = true;
                    return iterationValue;
                }

                iterationValue += ScalingStepSize; //change value by one step size

                newError = this._errorFunc(iterationValue); //calculate the error
                ratio = (prevError - newError) / ScalingStepSize; //ratio between change in k and error
                ScalingStepSize = newError / (ratio * 2.0);
                prevError = newError;

                while (Math.Abs(ScalingStepSize) > (this._maxValue - this._minValue)/2.0)
                {
                    ScalingStepSize /= 2.0; //helps to prevent value from running away by growing too large
                }
            }

            return previousSolution; //solution has failed. Reset to starting. 
        }

    } 

    public class Trajectory
    {        
        public double StartPos { get; private set; }
        public double EndPos { get; private set; }

        public double MaxAccel { get; set; } //acceleration
        public double MaxVeloc { get; set; } //max tragectory velocity

        public int InterpolationFrequency { get; set; } //number of positions/second to generate between start/end

        public Trajectory(double maxAccel, double maxVeloc, int interpolationFreq)
        {           
            this.MaxAccel = maxAccel;
            this.MaxVeloc = maxVeloc;
            this.InterpolationFrequency = interpolationFreq; 
        }    

        public double[] CalculatePositions(double startPos, double endPos, double moveTime)
        {
            this.StartPos = startPos;
            this.EndPos = endPos;

            //-------------------- Calculate number of points --------------------------

            int numPts = (int)(moveTime * this.InterpolationFrequency); //add 1 so last position gets calculated

            //----------------- Interpolate to get each intermediate position ------------

            List<double> output = new List<double>();

            for(int i = 0; i < numPts; i++)
            {
                output.Add(this.StartPos + (1.0 * i / (numPts-1)) * (this.EndPos - this.StartPos)); //linearly interpolate
            }
            return output.ToArray();
        }

    } //currently not finished, just interpolates between start and end

}
