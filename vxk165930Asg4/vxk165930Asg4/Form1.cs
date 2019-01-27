/*Written by Vamseekrishna Kattika for CS6326.001, assignment 4,starting October 11, 2018
 * Net ID: vxk165930
 * This program searches a  text file for a string.
 * It will find all occurrences of the string and show them in a list.   
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace vxk165930Asg4
{
    public partial class TextSearch : Form
    {
        /*Background worker*/
        BackgroundWorker bgWorker = null;
        /*The percentage of progress completed to display the progress bar*/
        private int percentageCompleted = 0;
        /*No of occurences of the given search string*/
        private int resultsCount = 0;
        /*Queue for storing the results of the search*/
        ConcurrentQueue<Results> searchResults= null;


        public TextSearch()
        {
            InitializeComponent();
        }

        /*This method will be invoked hen the form is loaded
         *The screen contents are adjusted to match the resolution of the screen
         */
        private void Form1_Load(object sender, EventArgs e)
        {
            this.AutoSize = true;
            int height = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
            this.Height += height;
            int width = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
            this.Width += width;
            this.CenterToScreen();
            btnReset.Enabled = false;
            resetForm();
        }

        /*This method will be invoked when  the Open File button is clicked
         * If the file name is empty or if it does not exist or if files other than text files are selected,
         * then it will display appropriate error messages
         */
        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (txtFileName.Text == "")
            {
                resetForm();
                lblError.Text = "Please enter the file name and click Open File button or Select a file using Browse button!";
                txtFileName.Focus();
                txtSearch.Enabled = false;
                btnSearch.Enabled = false;
            }
            else if (!System.IO.File.Exists(txtFileName.Text))
            {
                String fileName = txtFileName.Text;
                resetForm();
                txtFileName.Text = fileName;
                lblError.Text = "The File you are trying to open doen't exist!";
                txtFileName.Focus();
                txtSearch.Enabled = false;
                btnSearch.Enabled = false;
            }

            else
            {
                String extension = System.IO.Path.GetExtension(txtFileName.Text.Trim());
                if (extension.Equals(".txt"))
                {
                    lblError.Text = "";
                    txtSearch.Enabled = true;
                }
                else
                {
                    resetForm();
                    String fileName = txtFileName.Text;
                    resetForm();
                    txtFileName.Text = fileName;
                    lblError.Text = "Only text files are allowed";
                    txtFileName.Focus();
                    txtSearch.Enabled = false;
                    btnSearch.Enabled = false;
                }

            }
        }

        /*This method will be invoked when  the Browse button is clicked
         *Only text files can be browsed to select
         */
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".txt";
            ofd.Filter = "Text Files|*.txt";
            ofd.InitialDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            ofd.ShowDialog();
            txtFileName.Text = ofd.FileName;
            if (txtFileName.Text == "")
            {
                resetForm();
                txtSearch.Enabled = false;
                btnSearch.Enabled = false;
            }
            else
            {
                lblError.Text = "";
                txtSearch.Enabled = true;
            }
        }


        /*This method will be invoked when  the Search button is clicked
         *When enabled, if the data provided is valid, the search starts and the button text will be changed to cancel
         * When the Cancel text is shown, if we click the cancel operation the search aborts
         */

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (btnSearch.Text == "Search" && bgWorker != null)
            {
                if (!initiateSearch())
                {
                    return;
                }
                bgWorker.RunWorkerAsync();
            }
            else if (btnSearch.Text == "Cancel" && bgWorker != null && bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();
            }
        }


        /*This method will be invoked when  the Clear button is clicked
         The textbox field for the file will be on focus */

        private void btnReset_Click(object sender, EventArgs e)
        {
            resetForm();
            txtFileName.Focus();
        }

        /*This method resets the form*/
        public void resetForm()
        {
            bgWorker = null;
            listViewResults.Items.Clear();
            lblError.Text = "";
            txtFileName.Text = "";
            txtSearch.Text = "";
            txtSearch.Enabled = false;
            btnSearch.Enabled = false;
            btnReset.Enabled = false;
            btnSearch.Text = "Search";
            searchResults = new ConcurrentQueue<Results>();
            matchCount.Text = "";
            matchCount.Enabled = false;
            resultsCount = 0;
            percentageCompleted = 0;
            progressBar.Value = 0;           
            statusStrip.Text = "";
            statusStrip.Visible = false;
            initializeBackgroundWorker();
            progressBar.Visible = false;
            lblProgress.Text = "";
            checkBox.Enabled = true;
            checkBox.Checked = false;

        }

        /*This method enables the clear button when some key is pressed*/
        private void startFillUp(object sender, KeyPressEventArgs e)
        {
            btnReset.Enabled = true;
        }
        
        /*This method will be invoked when the textbox field for file name is changed
         * If the text file name is empty, then both search textbox and search button will be disabled
         * When the textbox field is not empty then search textbox will be enabled and if the
         * search textbox contains some text, the search button will be enabled
         */
        private void txtFileNameChanged(object sender, EventArgs e)
        {
            if (txtFileName.Text == "")
            {
                txtSearch.Enabled = false;
                btnSearch.Enabled = false;
            }
            else
            {
                txtSearch.Enabled = true;
                if (txtSearch.Text != "")
                {
                    btnSearch.Enabled = true;
                }
            }
        }

        /*This method will be invoked when the textbox field for search is changed
         *If search textbox contains some text, the search button will be enabled
         * Otherwise, it will be disabled
         */
        private void txtSearchChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text == "")
            {
                btnSearch.Enabled = false;
            }
            else
            {
                btnSearch.Enabled = true;
            }
        }

        /*This method initializes the background worker and the 
         * background worker object is setup by attaching event handlers*/
        private void initializeBackgroundWorker()
        {
            bgWorker = new BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += new DoWorkEventHandler(bgWorkerDoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorkerRunWorkerCompleted);
            bgWorker.ProgressChanged += new ProgressChangedEventHandler(bgWorkerProgressChanged);
        }

        /*This event handler finds the string in the file and displays in the listview*/
        private void bgWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            /*We will get the background worker that raised this event and 
            *assign the results of the computation to DoWorkEvenArgs which will be
            *available to the RunWorkerComplted event handler
             */
            BackgroundWorker newWorker = sender as BackgroundWorker;
            searchThread(sender, e, newWorker);
        }

        /*This event handlers deals with the results of the background operation*/
        private void bgWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            /*If there is any exception it will be displayed on the screen*/
            if (e.Error != null)
            {
                lblError.Text = e.Error.Message;
            }
            /*If the user cancels the operation, this will be executed*/
            else if (e.Cancelled)
            {
                lblError.Text = "Search Cancelled!";
                btnSearch.Text = "Search";
                txtFileName.Enabled = true;
                txtSearch.Enabled = true;
                btnBrowse.Enabled = true;
                btnReset.Enabled = true;
                btnOpen.Enabled = true;
                checkBox.Enabled = true;
            }
            /*This will be executed when the operation is sucessfully completed*/
            else
            {
                lblError.Text = "Search Sucessfully Completed";
                progressBar.Value = 100;
                btnSearch.Text = "Search";
                txtFileName.Enabled = true;
                txtSearch.Enabled = true;
                btnBrowse.Enabled = true;
                btnReset.Enabled = true;
                btnOpen.Enabled = true;
                checkBox.Enabled = true;
                progressBar.Visible = false;
                lblProgress.Text = "";                
                if (listViewResults.Items.Count == 0)
                {
                    matchCount.Text = "No Matches Found";
                }
            }
        }
        /*This event handler is to update the progress bar value when the search is going on*/
        private void bgWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar.Value = e.ProgressPercentage > 100 ? 100 : e.ProgressPercentage;
        }

        /*This method initiates the search for the given string in the file*/
        public Boolean initiateSearch()
        {
            progressBar.Value = 0;
            /*Handling the cases when there is no file selected or file selected is not valid or no search string is there */
            if(txtFileName.Text == "")
            {
                resetForm();
                this.Invoke((MethodInvoker)(() => lblError.Text = "Please enter the file name and click Open File button or Select a file using Browse button!"));
                txtFileName.Focus();
                return false;
            }
            String fileName = txtFileName.Text.Trim();
            if (!fileExists(fileName))
            {
                resetForm();
                this.Invoke((MethodInvoker)(() => lblError.Text = "The File you are trying to open doen't exist!"));
                txtFileName.Text = fileName;
                txtFileName.Focus();
                return false;
            }

            String extension = System.IO.Path.GetExtension(fileName);
            if (!extension.Equals(".txt"))
            {
                resetForm();
                this.Invoke((MethodInvoker)(() => lblError.Text = "Only text files are allowed"));
                txtFileName.Text = fileName;
                txtFileName.Focus();
                return false;
            }
            if (txtSearch.Text == "")
            {
                this.Invoke((MethodInvoker)(() => lblError.Text = "Please enter the string to search"));
                txtSearch.Focus();
                return false;
            }
           /*Appropriate actions when the search is going on*/
            this.Invoke((MethodInvoker)(() => btnSearch.Text = "Cancel"));
            this.Invoke((MethodInvoker)(() => lblError.Text = "Searching for \""+txtSearch.Text+"\""));
            this.Invoke((MethodInvoker)(() => matchCount.Text = "No. of Matches: 0"));
            this.Invoke((MethodInvoker)(() => checkBox.Enabled = false));
            this.Invoke((MethodInvoker)(() => matchCount.Enabled = true));                    
            this.Invoke((MethodInvoker)(() => statusStrip.Visible = true));
            this.Invoke((MethodInvoker)(() => statusStrip.Text = "Total lines Searched:"));
            this.Invoke((MethodInvoker)(() => txtFileName.Enabled = false));
            this.Invoke((MethodInvoker)(() => txtSearch.Enabled = false));
            this.Invoke((MethodInvoker)(() => btnBrowse.Enabled = false));
            this.Invoke((MethodInvoker)(() => btnReset.Enabled = false));
            this.Invoke((MethodInvoker)(() => btnOpen.Enabled = false));
            this.Invoke((MethodInvoker)(() => progressBar.Visible = true));
            this.Invoke((MethodInvoker)(() => lblProgress.Text = "Progress"));
            this.Invoke((MethodInvoker)(() => listViewResults.Items.Clear()));
            this.Invoke((MethodInvoker)(() => progressBar.Value=0));

            bgWorker = null;
            percentageCompleted = 0;
            resultsCount = 0;
            searchResults = new ConcurrentQueue<Results>();
            initializeBackgroundWorker();
            return true;
        }

        /*This method contain logic to find the string in the text file and display the results */
        public void searchThread(object sender, DoWorkEventArgs e, BackgroundWorker bgWorker)
        {
            String fileName = txtFileName.Text.Trim();
            String searchString = txtSearch.Text.Trim();
            String newLine = null;
            int index = 0;
            System.IO.StreamReader file = new System.IO.StreamReader(fileName);
            long fileSize = new System.IO.FileInfo(fileName).Length;
            int lineSize = 0;
            try
            {
                while((newLine = file.ReadLine()) != null)
                {
                    index += 1;
                    
                    if(checkBox.Checked == true)
                    {
                        /*Case sensitive comparision is done*/
                        if (newLine.Trim().Contains(searchString))
                        {
                            resultsCount += 1;
                            Results newSearchResult = new Results();
                            newSearchResult.LineNo = index;
                            newSearchResult.LineText = newLine;
                            /*Add the results to the queue and display them in the listview*/
                            searchResults.Enqueue(newSearchResult);
                            displaySearchResults();
                        }
                    }
                    else
                    {    /*Case insensitive comparision is done*/
                        if (newLine.Trim().ToLower().Contains(searchString.ToLower()))
                        {
                            resultsCount += 1;
                            Results newSearchResult = new Results();
                            newSearchResult.LineNo = index;
                            newSearchResult.LineText = newLine;
                            /*Add the results to the queue and display them in the listview*/
                            searchResults.Enqueue(newSearchResult);
                            displaySearchResults();
                        }
                    }
                    
                    /*Diplay a message that the search for given string is going on*/
                    this.Invoke((MethodInvoker)(() => lblError.Text = "Searching for the string \""+txtSearch.Text +"\""));
                    this.Invoke((MethodInvoker)(() => statusStrip.Text = "Total lines Searched: " + index));
                    /*Calculating the percentage of search completed to display as progress bar*/
                    lineSize += newLine.Length;
                    int percentCompleted = (int)((float)lineSize/(float)fileSize*100);
                    if(percentCompleted > percentageCompleted)
                    {
                        percentageCompleted = percentCompleted;
                        bgWorker.ReportProgress(percentCompleted);
                    }
                    if (bgWorker.CancellationPending)
                    {   /*If cancellation is pending, then the search should be cancelled*/
                        e.Cancel = true;
                        return;
                    }
                    /*There will be a delay of 1 milli second for every line read */
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                /*If there is any exception print it on the console*/
                Console.WriteLine("Exception"+ex);
            }
            finally
            {
                /*Close the file*/
                file.Close();
            }
        }
        /*This method finds whether the file entered exists or not*/
        public Boolean fileExists(String @fileName)
        {
            if (!System.IO.File.Exists(fileName))
            {
                return false;
            }
            return true;
        }

        /*This method displays the search results in the listview
         *It will dequeue the queue and adds that row to the listview
          */
        public void displaySearchResults()
        {
            if(searchResults.Count > 0)
            {
                Results newSearchResult = null;
                searchResults.TryDequeue(out newSearchResult);
                String[] newRow = new string[] { Convert.ToString(newSearchResult.LineNo),newSearchResult.LineText};
                this.Invoke((MethodInvoker)(() => listViewResults.Items.Add(new ListViewItem(new[] { newRow[0], newRow[1] }))));
                this.Invoke((MethodInvoker)(() => matchCount.Text = "No. of Matches: " + listViewResults.Items.Count));               
            }
        }      
    }
}