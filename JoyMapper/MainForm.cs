using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using vJoyInterfaceWrap;

namespace JoyMapper
{
    public partial class MainForm : Form
    {
        private vJoy virtualJoystick;
        private vJoy.JoystickState virtualJoystickState;
        private const int virtualJoystickId = 1;

        public MainForm()
        {
            InitializeComponent();
            this.loadControllers();
            bool enabled = this.initVirtualJoystick();



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

        private bool initVirtualJoystick()
        {
            this.virtualJoystick = new vJoy();
            this.virtualJoystickState = new vJoy.JoystickState();
            return this.virtualJoystick.vJoyEnabled();
        }

        private void cboGameController_SelectedIndexChanged(object sender, EventArgs e)
        {
            // GameController c = (sender as ComboBox).SelectedValue as GameController;
            // c.Connect();
        }
    }
}
