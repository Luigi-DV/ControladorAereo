﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlightsLib;

namespace FormPrincipal
{
    public partial class FlightInfoForm : Form
    {
        Flight flightInfo = new Flight();
        const string FLIGHTIMAGEIB = "Resources\\Images\\flightPictureIberia.png";
        const string FLIGHTIMAGEFR = "Resources\\Images\\flightPictureAirFrance.png";
        const string FLIGHTIMAGESWA = "Resources\\Images\\flightPictureSwiss.png";
        const string FLIGHTIMAGEDEFAULT = "Resources\\Images\\flightPicture.png";

        public FlightInfoForm()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            this.Text = flightInfo.flightID + "- Información del vuelo";
            //Big Label Flight ID
            this.flightIDLabel.Text = flightInfo.flightID;
            //Load images
            string imageFileIB = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, FLIGHTIMAGEIB);
            string imageFileFR = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, FLIGHTIMAGEFR);
            string imageFileLX = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, FLIGHTIMAGESWA);
            string imageFile = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, FLIGHTIMAGEDEFAULT);

            try
            {
                if (flightInfo.flightID.Contains("IB"))
                    this.flightPictureBox.Image = Image.FromFile(imageFileIB);
                else
                {
                    if(flightInfo.flightID.Contains("FR"))
                        this.flightPictureBox.Image = Image.FromFile(imageFileFR);
                    else
                        if (flightInfo.flightID.Contains("LX"))
                            this.flightPictureBox.Image = Image.FromFile(imageFileLX);
                        else
                            this.flightPictureBox.Image = Image.FromFile(imageFile);
                }
            }
            catch
            {
                MessageBox.Show("El archivo " + '"' + imageFile + '"' + "no se ha podido cargar.", "Recursos comprometidos",
                                     MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            //Flight ID Label
            this.flightID2Label.Text = flightInfo.flightID;
            //Flight Company label
            this.flightCompanyLabel.Text = flightInfo.company; 
            //Flight Position label
            this.flightPositionLabel.Text = "("+flightInfo.positionX + "," +flightInfo.positionY+")"; 

            //Flight Origin label
            this.flightOriginLabel.Text = "(" + flightInfo.originX + "," + flightInfo.originY + ")";
             //Flight Destination label
            this.flightDestinationLabel.Text = "(" + flightInfo.destinationX + "," + flightInfo.destinationY + ")";

            try
            {
                SetProgress();
            }
            catch
            {

                DialogResult result = MessageBox.Show("La información de origen/posición/destino no son lógicas por favor corrija este vuelo y vuelva a intentarlo. Puede ignorar este mensaje si presiona cancelar.", "Error en el vuelo" + '"' + flightInfo.flightID + '"',
                                  MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
                while (result != DialogResult.Cancel)
                {
                        try
                        {
                            SetProgress();
                        }
                        catch
                        {
                             result = MessageBox.Show("La información de origen/posición/destino no son lógicas por favor corrija este vuelo y vuelva a intentarlo. Puede ignorar este mensaje si presiona cancelar.", "Error en el vuelo" + '"' + flightInfo.flightID + '"',
                             MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
                        }
                    
                }
                   
            }
           
        }
        // Allows setting the value of color from Form1
        public void SetInfo(Flight flight )
        {
            this.flightInfo = flight;
        }

        private void SetProgress()
        {

            Point minimum = new Point((int)flightInfo.originX, (int)flightInfo.originY);

            int position;
            double percentX = (flightInfo.positionX - flightInfo.originX) / (flightInfo.destinationX - flightInfo.originX) * 100;
            double percentY = (flightInfo.positionY - flightInfo.originY) / (flightInfo.destinationY - flightInfo.originY) * 100;

            if (percentX <= 100 && percentY <= 100)
                position = Convert.ToInt32(percentX);
            else
                if (percentX < percentY)
                position = Convert.ToInt32(percentX);
                else
                position = Convert.ToInt32(percentY);

            //Progress Bar
            flightProgress.Maximum = 100;
            flightProgress.Minimum = 0;
            flightProgress.Step = 1;
            flightProgress.Value = position;
        }
       

    }
}
