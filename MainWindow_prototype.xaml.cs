using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.ComponentModel;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Agent ag = new Agent();
        public const string MUST_LOGIN="You must login first.";
        private BackgroundWorker bw1;
        private object oj= new Object();
        private string user = "";
        private string pw="";
        private string host="";
     

        public MainWindow()
        {

        

           this.bw1= new BackgroundWorker();
             bw1.WorkerReportsProgress = true;
             bw1.WorkerSupportsCancellation=true;
             InitializeThread();
             InitializeComponent();
          
        }

        private void InitializeThread()
        {

             
               bw1.DoWork += 
                new DoWorkEventHandler(bw1_DoWork);
            bw1.RunWorkerCompleted += 
                new RunWorkerCompletedEventHandler(
            bw1_RunWorkerCompleted);
            bw1.ProgressChanged += 
                new ProgressChangedEventHandler(
            bw1_ProgressChanged);
   
	
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
                   user = txtUser.Text;
                   pw = txtPassword.Password;  
                   host=txtHost.Text;
                   btnLogin.IsEnabled = false;
            if (ag.loggedin)
            {
                oj = "logout";
             
                 this.bw1.RunWorkerAsync(oj);
            }
            else
            {
                oj="login";
            
                this.bw1.RunWorkerAsync(oj);
               
            }

        }
   

        private void LoadVMNames()
        {

          
            string[] separators = { ";"};
            string[] words = ag.VMNames.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
                    this.tvVM.Items.Add(word);

                  }

        private void UnLoadVMNames()
        {
            tvVM.Items.Clear();

        
         
        }


        private void btnTime_Click(object sender, RoutedEventArgs e)
        {
            this.pbInfo.Value = 0;
            oj = "time";
            if (ag.loggedin)
            {
                this.bw1.RunWorkerAsync(oj);
              //  lblInfo.Content += "\nTime retrieved.";
            }
            else
            {
                showMessage(MUST_LOGIN);

            }

        }

        private void btnGetHost_Click(object sender, RoutedEventArgs e)
        
        {

          //  this.pbInfo.Value = 0;
           oj="host";

            if (ag.loggedin)
            {
                this.bw1.RunWorkerAsync(oj);
               
            }
            else
            {
                showMessage(MUST_LOGIN);

            }
       }

        private void showMessage(string msg)
        {

             MessageBox.Show(msg);

        }

        private void btnVM_Click(object sender, RoutedEventArgs e)
        {
            this.pbInfo.Value = 0;
             oj = "vm";
            if (ag.loggedin)
            {
                this.bw1.RunWorkerAsync(oj);
            }
            else
            {
                showMessage(MUST_LOGIN);

            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }

        private void bw1_DoWork(object sender, DoWorkEventArgs e)
        {
         //   BackgroundWorker worker = sender as BackgroundWorker;
          
            string[] separators = { "," };
            string[] words = e.Argument.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);
          
        //    switch(e.Argument.ToString())
            switch (words[0])
                {

                    case "host":
                    e.Result = ag.getHostInfo(bw1);
                    break;
                    case "login":
                    e.Result = ag.login(user, pw, host, bw1);
                     break;
                    case "logout":
                    e.Result = ag.logout(bw1);
                    break;
                    case "vm":
                    e.Result = ag.getVM(bw1);
                    break;
                    case "time":
                    e.Result=ag.getCurrentTime(bw1);
                    break;
                    case "suspend":
                    e.Result = ag.suspendVM(bw1);
                    break;
                    case "poweron":
                    e.Result = ag.powerOnVM(bw1, words[1]);
                    break;
                    case "poweroff":
                    e.Result = ag.powerOffVM(bw1, words[1]);
                    break;
                    case "refreshVMList":
                    e.Result = ag.refreshVMList(bw1);
                    break;
                    case "rebootVM":
                    e.Result = ag.rebootVM(bw1, words[1]);
                    break; 
                    default:
                    e.Result="No data";
                    break;      

                }
           
        }

        private void bw1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

              //  this.lblInfo.Content += " "+e.ProgressPercentage;
               this.pbInfo1.Value=(e.ProgressPercentage *10);
        }



        private void bw1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.pbInfo1.Value = 100;
            // First, handle the case where an exception was thrown. 
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled  
                // the operation. 
                // Note that due to a race condition in  
                // the DoWork event handler, the Cancelled 
                // flag may not have been set, even though 
                // CancelAsync was called.
                this.rtbUpdates.AppendText("\nOperation Cancelled");
            }
            else
            {
                // Finally, handle the case where the operation  
                // succeeded.
                //e.Result
                if(ag.loggedin)
                {
                    btnLogin.Content = "Logout";
                  

                }
                if (!ag.loggedin)
                {
                    btnLogin.Content = "Login";

                }
              //  showMessage(e.Result.ToString());
            }

          //  lblInfo.Content += "\n" + e.Result;
            rtbUpdates.AppendText(e.Result.ToString()+"\n");
            if (e.Result.ToString() == "Logged in")
            {
                LoadVMNames();
                btnLogin.IsEnabled = true;
            }

            if (e.Result.ToString() == "Logged out")
            {
                UnLoadVMNames();
                btnLogin.IsEnabled = true;

            }

            if (e.Result.ToString()== "Refreshed VM List")
            {
                UnLoadVMNames();
                LoadVMNames();

            }

            this.pbInfo1.Value = 0;
            ag.reportProgress = false;

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

            if (bw1.IsBusy)
            {
                this.bw1.CancelAsync();
            }         

        }

        private void btnSuspendVM_Click(object sender, RoutedEventArgs e)
        {

            this.pbInfo.Value = 0;
            oj = "suspend";
            if (ag.loggedin)
            {
                this.bw1.RunWorkerAsync(oj);
            }
            else
            {
                showMessage(MUST_LOGIN);

            }

        }

       

        private void btnPowerOffVM_Click(object sender, RoutedEventArgs e)
        {
           
            if (ag.loggedin)
            {
                this.tvVM.SelectedItem.ToString();
                oj = "poweroff," + this.tvVM.SelectedItem.ToString(); ;
                this.bw1.RunWorkerAsync(oj);
            }
            else
            {
                showMessage(MUST_LOGIN);

            }

        }

        private void btnPowerOnVM_Click(object sender, RoutedEventArgs e)
        {
            
            if (ag.loggedin)
            {
                this.tvVM.SelectedItem.ToString();
                oj = "poweron," + this.tvVM.SelectedItem.ToString(); ;
                this.bw1.RunWorkerAsync(oj);
            }
            else
            {
                showMessage(MUST_LOGIN);

            }

        }

        private void btnRefreshVM_Click(object sender, RoutedEventArgs e)
        {
            oj="refreshVMList";

            if (ag.loggedin)
            {
                this.bw1.RunWorkerAsync(oj);
            }
            else
            {
                showMessage(MUST_LOGIN);

            }



        }

        private void btnReboot_Click(object sender, RoutedEventArgs e)
        {
            
            if (ag.loggedin)
            {
                this.tvVM.SelectedItem.ToString();
                oj = "rebootVM," + this.tvVM.SelectedItem.ToString(); ;
                this.bw1.RunWorkerAsync(oj);
            }
            else
            {
                showMessage(MUST_LOGIN);

            }


        }


       

       
    }
}
