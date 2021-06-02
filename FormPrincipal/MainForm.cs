﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlightsLib;

namespace FormPrincipal
{
    public partial class MainForm : Form
    {
        private Auth userAuthenticated = new Auth();
        public Auth UserAuthenticated
        {
            get { return userAuthenticated; }
            set { userAuthenticated = value; }
        }

        private const string IMG = "Resources\\Graphics\\plane.png";
        private const string IMGBG = "Resources\\Backgrounds\\spainBg.png";

        //300 flights inside the picture box array
        const int MAX = 300;

        //The flight list
        private FlightsList myFlightsList = new FlightsList();

        //Stack with all the previous cycles
        Stack<FlightsList> FlightStack = new Stack<FlightsList>();

        //The sector
        private Sector mySector = new Sector();

        //A single flight
        private Flight myFlight = new Flight();

        private PictureBox[] aircraftVector = new PictureBox[MAX];

        

        public MainForm()
        {
            InitializeComponent();
        }

        /*************************************** Form Load ****************************************************/
        private void Form1_Load(object sender, EventArgs e)
        {
           
          
            //Check if user is logged
            if (!userAuthenticated.Authenticated)
            {
                //Welcome message to username 
                this.userNameLabel.Text = "Guest";
                this.logInMenu.Text = "Iniciar Sesión";

                this.sesionBtn.Text = "Iniciar Sesión";
                this.sesionBtn.BackColor = Color.Green;
                
                
            }
            else
            {
                this.userNameLabel.Text = userAuthenticated.GetUsername();
                this.logInMenu.Text = "Cerrar Sesión";
                this.sesionBtn.Text = "Cerrar Sesión";
            }
           
            
            
        }

        /*************************************** Auth  ****************************************************
         *   1.Sistema Auth 
         **      1.1.Iniciar Sesion Menu Strip
         **      1.2.Visualizar automáticamente los vuelos 
         **      1.3.Salvar lista de vuelos 
         *   2.Sector
         **      2.1.Cargar el sector
         **      2.2.Visualizar el sector en el espacio aéreo.      
         *   3. Mostrar en otro formulario los datos del vuelo sobre el que ha clicado el usuario.
         **  4. Listar en otro formulario los datos de todos los vuelos que hay en el espacio aéreo.
        ****************************************************************************************************************/
        //1.1.Cargar la lista de vuelos: Receive the LoadFlight StripMenuevent and let answer to open a file with openFileDialog1
        private void logInMenu_Click(object sender, EventArgs e)
        {   
            //Al hacer click en el menuStrip si se ha iniciado sesion se cierra la sesion
            if (userAuthenticated.Authenticated)
            {
                userAuthenticated.Authenticated = false;
                SesionClose();
            }
            else
            {
                SesionLogIn();
            }
            
            
        }
        private void sesionBtn_Click(object sender, EventArgs e)
        {
            if (!userAuthenticated.Authenticated)
            {
                SesionLogIn();
            }
            else
            {
                SesionClose();
            }
        }

        //0.1.Helper - SesionClose
        private void SesionClose() 
        {
            if (MessageBox.Show("¿Está seguro de que desea cerrar sesión? Toda la información no guardada se perderá.", "Cerrar Sesión", MessageBoxButtons.YesNo) ==
                DialogResult.Yes)
            {
                // The user wants to exit the application. Close everything down.
                this.Close();
                Login loginForm = new Login();
                loginForm.ShowDialog();
            }
        }
        //0.2.Helper - SesionClose
        private void SesionLogIn()
        {
            if (MessageBox.Show("¿Está seguro de que desea iniciar sesión? Toda la información no guardada se perderá.", "Iniciar Sesión", MessageBoxButtons.YesNo) ==
                DialogResult.Yes)
            {
                // The user wants to exit the application. Close everything down.
                this.Close();
                Login loginForm = new Login();
                loginForm.ShowDialog();
            }
        }


        /*************************************** Load, Show and Save ****************************************************
         *   1.Lista de vuelos  
         **      1.1.Cargar la lista de vuelos 
         **      1.2.Visualizar automáticamente los vuelos 
         **      1.3.Salvar lista de vuelos 
         *   2.Sector
         **      2.1.Cargar el sector
         **      2.2.Visualizar el sector en el espacio aéreo.      
         *   3. Mostrar en otro formulario los datos del vuelo sobre el que ha clicado el usuario.
         **  4. Listar en otro formulario los datos de todos los vuelos que hay en el espacio aéreo.
        ****************************************************************************************************************/

        //1.1.Cargar la lista de vuelos: Receive the LoadFlight StripMenuevent and let answer to open a file with openFileDialog1
        private void LoadFlightsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Call to 0.1.Helper
            OpenFileDialog1Function();

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog1.FileName;
                int result = myFlightsList.LoadFlightsFile(filename);
                FlightStack.Push(myFlightsList);
                if (result != 0)
                {
                    if (result == -1)
                    {
                        MessageBox.Show("Windows no puede encontrar el archivo " + '"' + openFileDialog1.FileName + '"' + ". Asegúrese que el nombre esté bien escrito correctamente e inténtelo de nuevo.",
                                        openFileDialog1.FileName,
                                         MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        if (result == -2)
                        {

                            MessageBox.Show("El fichero " + '"' + openFileDialog1.FileName + '"' + " no tiene el formato deseado, por favor seleccione un fichero con el formato adecuado", '"' + openFileDialog1.FileName + '"',
                                       MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show("Ha habido un problema por favor reinicie el programa.", "Error",
                                         MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    /********************* Please read README.md - Load Resources From Debug *************************
                     **   This code is for Debug route (bin/debug..) in case you want to run a resource in debug mode: 
                     ***    string path = System.IO.Directory.GetCurrentDirectory();
                     ***    string imageFile = path + IMG;
                    ************************************************************************************************/

                    //1.2.Visualizar automáticamente los vuelos

                    //Display Flights Total Number into the Right side panel
                    this.totalFlightsLabel.Text = Convert.ToString(myFlightsList.Number);
                    //Call 0.5.Helper: Function to print the sector occupation
                    PrintOcuppation();
                    //Define the path and the name of the image into the string imageFile
                    string imageFile = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, IMG);
                    for (int i = 0; i < myFlightsList.Number; i++)
                    {
                        //Trying to generate a Picture Box with the image
                        try
                        {
                            aircraftVector[i] = new PictureBox();
                            aircraftVector[i].ClientSize = new Size(20, 20);
                            aircraftVector[i].Location = new Point((int)myFlightsList.Flights[i].PositionX, (int)myFlightsList.Flights[i].PositionY);
                            aircraftVector[i].SizeMode = PictureBoxSizeMode.StretchImage;
                            aircraftVector[i].Tag = myFlightsList.Flights[i];
                            aircraftVector[i].Click += new System.EventHandler(this.AircraftVector_Click);
                            //Defining our imageFile as a Bitmap image
                            Bitmap image = new Bitmap(imageFile);
                            aircraftVector[i].Image = (Image)image;
                            panel1.Refresh();
                            panel1.Controls.Add(aircraftVector[i]);

                        }
                        //The imageFile path is not correct or the file don't exist
                        catch (Exception)
                        {
                            MessageBox.Show("Error de carga puede que el fichero " + '"' + imageFile + '"' + " no se haya encontrado, posiblemente la carpeta Resources esté comprometida",
                                             '"' + imageFile + '"', MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                    }

                }

            }
        }

        //1.3.Salvar lista de vuelos: Receive the click event from the Save ToolStripMenu and show the user the saveFileDialog
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //Check if the first list vector is empty and ask the user ab this
            if (myFlightsList.Flights[0] == null)
            {
                var result = MessageBox.Show("¿Esta seguro que desea guardar la lista?", "La lista esta vacia",
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    SaveFileDialog1Function();
                }
            }
            //Passing here when the FlightsList is not empty 
            else
            {
                SaveFileDialog1Function();
            }

        }



        //2.1.Cargar el sector: Receive the click event from the CargarSector ToolStripMenu and show the user the openFileDialog2
        private void CargarSectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog2.Title = "Selecciona el fichero con la lista de sectores";
            openFileDialog2.InitialDirectory = @"C:\";
            openFileDialog2.FileName = "";
            openFileDialog2.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog2.FilterIndex = 2;
            openFileDialog2.Multiselect = false;
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog2.FileName;

                int result = mySector.LoadSectorFile(filename);
                if (result != 0)
                {
                    if (result == -1)
                    {
                        MessageBox.Show("Windows no puede encontrar el archivo " + '"' + openFileDialog2.FileName + '"' + ". Asegúrese que el nombre esté bien escrito correctamente e inténtelo de nuevo.",
                                        openFileDialog2.FileName,
                                         MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        if (result == -2)
                        {

                            MessageBox.Show("El fichero " + '"' + openFileDialog2.FileName + '"' + " no tiene el formato deseado, por favor seleccione un fichero con el formato adecuado", '"' + openFileDialog2.FileName + '"',
                                       MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show("Ha habido un problema por favor reinicie el programa.", "Error",
                                         MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    this.sectorIDLabel.Text = Convert.ToString(mySector.SectorID);
                    //0.5.Helper: Function to print the sector occupation
                    PrintOcuppation();
                    panel1.Invalidate();
                }

            }
        }

        //2.2.Visualizar el sector en el espacio aéreo.
        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            //Xo,Yo (Left top corner)
            Point x0y0 = new Point(Convert.ToInt32(mySector.PositionX), Convert.ToInt32(mySector.PositionY));
            //Xo,Yf (Left bottom corner)
            Point x0yF = new Point(Convert.ToInt32(mySector.PositionX), Convert.ToInt32(mySector.PositionY + mySector.Height));
            //Xf,Yf (Right bottom corner)
            Point xFyF = new Point(Convert.ToInt32(mySector.PositionX + mySector.Width), Convert.ToInt32(mySector.PositionY + mySector.Height));
            //Xo,Yo (Right top corner)
            Point xFy0 = new Point(Convert.ToInt32(mySector.PositionX + mySector.Width), Convert.ToInt32(mySector.PositionY));

            System.Drawing.Graphics graphics = e.Graphics;
            // Points that define the rectangle
            Point[] polygonPoints = new Point[4];
            polygonPoints[0] = x0y0;
            polygonPoints[1] = x0yF;
            polygonPoints[2] = xFyF;
            polygonPoints[3] = xFy0;

            int ocupacion = mySector.GetTraffic(myFlightsList);
            if (mySector.Capacity <= ocupacion)
            {
                //Call to 0.3.Helper
                graphics.DrawPolygon(Painting("Red"), polygonPoints);
                Painting("Red").Dispose();

            }
            else
            {
                //Call to 0.3.Helper
                graphics.DrawPolygon(Painting("Green"), polygonPoints);
                Painting("Green").Dispose();
            }
        }



        /************************************************************ Interaction Section *****************************************************************
         *  3.Mostrar en otro formulario los datos del vuelo sobre el que ha clicado el usuario
         *  4.Listar en otro formulario los datos de todos los vuelos que hay en el espacio aéreo.
        ****************************************************************************************************************************************************/
        //3.Mostrar en otro formulario los datos del vuelo sobre el que ha clicado el usuario:
        //Receive the AircraftVector(defined as an PictureBox array) event and make a call to display the FlightInfoForm for show de Flight requested data
        private void AircraftVector_Click(object sender, EventArgs e)
        {
            //Receiving the PictureBox
            PictureBox flightBox = (PictureBox)sender;
            //Declaring the new form
            FlightInfoForm flightInfo = new FlightInfoForm();
            //On click plane icon background will be green
            flightBox.BackColor = Color.Green;
            //Getting the info from the tag, myFlight declared in line 31
            myFlight = (Flight)flightBox.Tag;
            //Pass Flight info to the f3 form
            flightInfo.SetInfo(myFlight);
            //Show the f3 form
            flightInfo.ShowDialog();
            //When f3 is closed the color returns to transparent
            flightBox.BackColor = Color.Transparent;
        }

        //4.Listar en otro formulario los datos de todos los vuelos que hay en el espacio aéreo: Receive the click event from the listaDeVuelos ToolStripMenu
        private void ListaDeVuelosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FlightsListInfoForm flightsListInfo = new FlightsListInfoForm();
            //Show the Flights Informations in List, calling to the respect form
            flightsListInfo.SetInfo(myFlightsList);
            flightsListInfo.ShowDialog();
        }

        /*************************************** Simulation Section ****************************************************
         *  5. Introducir el tiempo de ciclo de simulación (en minutos) y avanzar la simulación ciclo a ciclo.
         *  6. Reiniciar la simulación.
         *  7. Introducir el tiempo de ciclo (en minutos) y la duración de la simulación (en minutos) y
            avanzar de forma automática (un ciclo cada segundo). La aplicación debe permitir pausar y
            reanudar la simulación en cualquier momento.
         *  8. Identificar visualmente, en cada ciclo, el nivel de ocupación del sector.
       ****************************************************************************************************************/

        private void AvanzarSimulacion_Click(object sender, EventArgs e)
        {

            try
            {
                string cycleTime = cycleTimeInput.Text;
                int time = Convert.ToInt32(cycleTime);
                int flightsInDestination = 0;
                //Simulates a Cycle and updates position in Picturebox
                myFlightsList.FlightsSimulation(time);
                //0.5.Helper: Function to print the sector occupation

                //Saves simulation to the stack
                FlightStack.Push(myFlightsList);
                PrintOcuppation();
                panel1.Invalidate();
                for (int i = 0; i < myFlightsList.Number; i++)
                {
                    aircraftVector[i].Location = new Point((int)myFlightsList.Flights[i].PositionX, (int)myFlightsList.Flights[i].PositionY);

                    if (myFlightsList.Flights[i].Simulator(time) == -1)
                    {
                        flightsInDestination++;
                    }
                }

                //Shows a message if at least one flight has arrived
                if (flightsInDestination > 0)
                    MessageBox.Show(flightsInDestination + " vuelos han llegado con éxito a su destino, para más información consulte la tabla de información de vuelos.",
                                     "Hay vuelos que han llegado a su destino.",
                                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (FormatException)
            {
                MessageBox.Show("Asegurece de insertar un tiempo de simulación y que este sea de valor entero.", "Tiempo de simulación nulo",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            //Change Sector Color
            int StackNum = FlightStack.Count;
            panel1.Invalidate();
            //Checks if the stack is empty and shows a messagebox informing
            if(StackNum < 1)
            {
                MessageBox.Show("No hay ciclos anteriores");
            }
            else
            {
                myFlightsList = FlightStack.Pop();
                for (int i = 0; i < myFlightsList.Number; i++)
                {


                    aircraftVector[i].Location = new Point((int)myFlightsList.Flights[i].PositionX, (int)myFlightsList.Flights[i].PositionY);

                }
            }
           
            //0.6.Helper: Stop and change button color and text, show is stopped and can start(Green)
            StopSimulation();
            //0.5.Helper: Function to print the sector occupation
            PrintOcuppation();
        }


        private void StartSimulation_Click(object sender, EventArgs e)
        {
          
            if (timer1.Enabled)
            {
                timer1.Stop();
                //0.6.Helper: Stop and change button color and text, show is stopped and can start(Green)
                StopSimulation();
            }
            else
            {
                StartSimulation.Text = "Stop";
                StartSimulation.BackColor = Color.Red;
                string cycleTime = cycleTimeInput.Text;
                string cycleNum = cycleNumInput.Text;

                if (!string.IsNullOrWhiteSpace(cycleTime) && // Not empty
                       int.TryParse(cycleTime, out int time))
                {
                    if (!string.IsNullOrWhiteSpace(cycleNum) && // Not empty
                    int.TryParse(cycleNum, out int num))
                    {
                        timer1.Interval = 1000;
                        timer1.Enabled = true;
                    }
                    else
                    {
                        MessageBox.Show("Asegurece de insertar un número de simulaciones y que este sea de valor entero.", "Número de simulaciones nulo",
                                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Asegurece de insertar un tiempo de simulación y que este sea de valor entero.", "Tiempo de simulación nulo",
                                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                }  
            }
        }


        private void timer_tick(object sender, EventArgs e)
        {
    
            string cycleTime = cycleTimeInput.Text;
            int.TryParse(cycleTime, out int time);

            //int time = Convert.ToInt32(cycleTime);
            int flightsInDestination = 0;

            //Simulates a Cycle and updates position in Picturebox
            myFlightsList.FlightsSimulation(time);

            //Saves flight to the stack
            FlightStack.Push(myFlightsList);

            // i es el contador para los ciclos de la simulacion
            for (int j = 0; j < myFlightsList.Number; j++)
            {
                aircraftVector[j].Location = new Point((int)myFlightsList.Flights[j].PositionX, (int)myFlightsList.Flights[j].PositionY);
                //0.5.Helper: Function to print the sector occupation
                PrintOcuppation();
                //Change Sector Color
                panel1.Invalidate();

                if (myFlightsList.Flights[j].Simulator(time) == -1)
                {
                    flightsInDestination++;
                }
            }
  
        }

        /*************************************** Helpers Section ****************************************************
         * Specialized code that help us create, modify, or eliminate things externally to the fundamental processes.
        ************************************************************************************************************/

        //0.1.Helper: Funtion for openFileDialog1 
        private void OpenFileDialog1Function()
        {
            openFileDialog1.Title = "Selecciona el fichero con la lista de vuelos";
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.Multiselect = false;
        }
        //0.2.Helper: Funtion for saveFileDialog1 
        private void SaveFileDialog1Function()
        {
            saveFileDialog1.Title = "Guardar Como";
            saveFileDialog1.InitialDirectory = @"C:\";
            saveFileDialog1.FileName = "";
            saveFileDialog1.Filter = "txt files (*.txt)|*.txt";
            saveFileDialog1.FilterIndex = 2;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filename = saveFileDialog1.FileName;
                myFlightsList.SaveList(filename);
            }
        }

        //0.3.Helper: Function for create a new pen with the code as an atribute
        private Pen Painting(string color)
        {
            //Default Color value value
            Pen myPen = new Pen(Color.Green);
            //Colour to draw the rectangle
            if (color == "Red")
                myPen = new Pen(Color.Red);
            return myPen;

        }

        //0.4.Helper: Function to visit a custom link
        private void VisitLink(string url)
        {
            //Call the Process.Start method to open the default browser
            //with a URL:
            System.Diagnostics.Process.Start(url);
        }

        //0.5.Helper: Function to print the sector occupation
        private void PrintOcuppation()
        {
            this.occupationNumLabel.Text = Convert.ToString(mySector.GetTraffic(myFlightsList));
        }

        //0.6.Helper: Stop and change button color and text
        private void StopSimulation()
        {
            timer1.Stop();
            StartSimulation.BackColor = Color.Green;
            StartSimulation.Text = "Start";
        }

        /*************************************** Extras Section ***************************************************
        * Extra code for increase the user experiencie.
       ************************************************************************************************************/
        //0.Extra
        private void GitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //Call to 0.4.Helper
                VisitLink("https://github.com/LuigeloDV/ProjectG6");
            }
            catch (Exception)
            {
                MessageBox.Show("No se ha podido abrir el repositorio remoto.");
            }
        }

        private string GetUserNameFunction()
        {
            string userName;
            try
            {
                userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            }
            catch (Exception)
            {
                userName = "Username_default";
            }

            return userName;
        }

        /*************************************** Exit Section ***************************************************
        * Code for handle the exit event and diferents situations.
       ************************************************************************************************************/
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* Determines whether the user wants to exit the application.
            * If not, adds another Number to the list box. */
            if (MessageBox.Show("¿Está seguro de que desea salir del simulador de vuelo?", "Saliendo", MessageBoxButtons.YesNo) ==
               DialogResult.Yes)
            {
                // The user wants to exit the application. Close everything down.
                Application.Exit();
            }
        }

        private void cycleTimeInput_TextChanged(object sender, EventArgs e)
        {

        }

       
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GroupInfo formGroup = new GroupInfo();
            formGroup.ShowDialog();
        }
    }
}
