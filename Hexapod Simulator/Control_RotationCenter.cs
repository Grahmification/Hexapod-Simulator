﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hexapod_Simulator
{
    public partial class Control_RotationCenter : UserControl
    {
        private List<NumericalInputTextBox> Txts = new List<NumericalInputTextBox>();
        private Platform platform;
        
        
        public Control_RotationCenter()
        {
            InitializeComponent();
        }

        public void AssignPlatform(Platform platform)
        {
            this.platform = platform;

            label_title.Text = platform.Name + " Rotation Center";

            checkBox_fixedCenter.Checked = platform.FixedRotationCenter; 

            numericalInputTextBox_posX.Value = platform.RotationCenter[0];
            numericalInputTextBox_posY.Value = platform.RotationCenter[1];
            numericalInputTextBox_posZ.Value = platform.RotationCenter[2];
       
            foreach (Control cont in this.Controls)
            {
                if (cont is NumericalInputTextBox)                
                    Txts.Add((NumericalInputTextBox)cont);              
            }
        }
        private void button_apply_Click(object sender, EventArgs e)
        {
            foreach (NumericalInputTextBox txt in Txts) //make sure all textboxes are valid
            {
                if (txt.TextValid == false)
                    return;
            }
     
            double[] Position = new double[] { numericalInputTextBox_posX.Value, numericalInputTextBox_posY.Value, numericalInputTextBox_posZ.Value };

            platform.UpdateRotationCenter(Position, checkBox_fixedCenter.Checked);
        }

     
    }
}
