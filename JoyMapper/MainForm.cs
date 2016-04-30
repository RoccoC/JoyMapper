using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace JoyMapper
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.loadControllers();

            //GameController controller = GameController.GetAll()[0];
            //controller.Connect();



            //// Poll events from joystick
            //while (true)
            //{
            //    //joystick.Poll();
            //    //var datas = joystick.GetBufferedData();
            //    //foreach (var state in datas)
            //    //    Console.WriteLine(state);
            //    Console.WriteLine(controller.GetState().X);
            //    Console.WriteLine(controller.GetState().Y);

            //    System.Threading.Thread.Sleep(10);
            //}
        }

        private void loadControllers()
        {
            cboGameController.DisplayMember = "Key";
            cboGameController.ValueMember = "Value";
            Dictionary<string, GameController> controllerDictionary = new Dictionary<string, GameController>();
            foreach (GameController controller in GameController.GetAll())
            {
                controllerDictionary.Add(controller.Name, controller);
            }
            cboGameController.DataSource = new BindingSource(controllerDictionary, null);
        }

        private void cboGameController_SelectedIndexChanged(object sender, EventArgs e)
        {
            // GameController c = (sender as ComboBox).SelectedValue as GameController;
            // c.Connect();
        }
    }
}
