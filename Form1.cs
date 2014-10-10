/* Created By:  
 * Mell Rosandich
 * Mell@ourace.com
 * 2014-10-10
 * http://ourace.com
 * http://ourace.com/130-wpl2usb
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;





namespace WPL2USB
{
    public partial class Form1 : Form
    {

        //Used for stopping the copy process at any time
        bool isCopying = false;

        public Form1()
        {
            InitializeComponent();
            textBox3.Text = Properties.Settings.Default.SelectWPLDirectory;
            textBox2.Text = Properties.Settings.Default.SelectDestination;

            if (Properties.Settings.Default.CleanNames == "True")
            {
                checkBox1.Checked = true;
            }

            if (Properties.Settings.Default.PlaylistOrArtist == "True")
            {
                checkBox2.Checked = true;
            }
           
        }

       
        //This function takes a folder path
        //it will iterate through all the wpl file types
        //it will create the data grid view show counts
        private void LoadWPLList(string SelectedPath)
        {
            int totalFilesCounted = 0;
            progressBar1.Value = 0;
            Application.DoEvents();

            DataTable dt = new DataTable();
            DataColumn PLNAME = new DataColumn("Play List", typeof(string));
            PLNAME.ReadOnly = true;


            DataColumn RemoteFolderName = new DataColumn("Folder To Copy To", typeof(string));
            RemoteFolderName.ReadOnly = true;

            DataColumn SongsInList = new DataColumn("Songs In List", typeof(string));
            SongsInList.ReadOnly = true;

            DataColumn SongsCopied = new DataColumn("Songs Copied", typeof(string));
            SongsCopied.ReadOnly = false;

            dt.Columns.Add(PLNAME);
            dt.Columns.Add(RemoteFolderName);
            dt.Columns.Add(SongsInList);
            dt.Columns.Add(SongsCopied);

            
            List<string> Files = new List<string>(Directory.EnumerateFiles(SelectedPath, "*.wpl", SearchOption.TopDirectoryOnly));
            foreach (var File in Files)
            {
                string DestNameFolder = File.Substring(SelectedPath.Length + 1);
                DestNameFolder = DestNameFolder.Replace(".wpl", "");

                int cpd = LoadWPL(File, DestNameFolder, false);
                totalFilesCounted += cpd;
                dt.Rows.Add(File, DestNameFolder, cpd.ToString(), "0");
            }
            dataGridView1.DataSource = dt;
            dataGridView1.AutoResizeColumns();
            progressBar1.Maximum = totalFilesCounted;


        }//end LoadWPLList


        //This is called to copy all files in the play list (wpl)
        private void RunCopy()
        {

            string SelectedPath = textBox3.Text;
            List<string> Files = new List<string>(Directory.EnumerateFiles(SelectedPath, "*.wpl", SearchOption.TopDirectoryOnly));
            foreach (var File in Files)
            {
                if (isCopying == true)
                {
                    string DestNameFolder = File.Substring(SelectedPath.Length + 1);
                    DestNameFolder = DestNameFolder.Replace(".wpl", "");
                    int cpd = LoadWPL(File, DestNameFolder, true);
                    UpdateGridCount(File, cpd);
                }
            }

            toolStripStatusLabel1.Text = "Idle: Just finished copying.";
            button4.Text = "Start Copy Songs";
            isCopying = false;
        }//end RunCopy


        //As files are copied lets up data the totals in the grid view.
        private void UpdateGridCount(string WPLPath, int FilesCopiedCnt)
        {
            //update the data grid count
            int rowIndex = -1;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value.ToString().Equals(WPLPath))
                {
                    rowIndex = row.Index;
                    break;
                }
            }
            if (rowIndex != -1)
            {

                dataGridView1.Rows[rowIndex].Cells[3].Value = FilesCopiedCnt;
            }
        }//end UpdateGridCount


        //This function loads the WPL file and find all the songs in the file
        //This will copy the files to the selected folder
        //returns how many files copied.
        private int LoadWPL(string LoadPath,string ToPath,bool DoCopy)
        {
            //This is the count of files copied from the current WPL or files counted
            int RetVal = 0;

            //Create an XML document and load the WPL
            XmlDocument document = new XmlDocument();
            document.Load(LoadPath);

            XmlNodeList MediaList = document.GetElementsByTagName("media");


            List<string> sources = new List<string>();
            for (int ix = 0; ix < MediaList.Count; ix++)
            {
                if (DoCopy == true && isCopying == true)
                {


                    //This is the path to the media file
                    string FromPath = MediaList[ix].Attributes[0].Value.ToString();

                    //this means a relative path to the wpl file
                    //it is up one directory
                    
                    if (FromPath.Contains("..\\"))
                    {

                        var FilePartsRel = LoadPath.Split('\\');
                        string FilePathNewRelative = ""; 
                        int RelPathMinusFileAndFolder = FilePartsRel.Count() - 2;
                        for (int fidx = 0; fidx < RelPathMinusFileAndFolder; fidx++)
                        {
                            FilePathNewRelative += FilePartsRel[fidx] + "\\";
                        }
                        FromPath = FromPath.Replace("..\\", FilePathNewRelative);
                    }


                    if (checkBox2.Checked == true)
                    {
                        //We just create the playlist name as a folder here.
                        if (!System.IO.Directory.Exists(textBox2.Text + ToPath))
                        {
                            System.IO.Directory.CreateDirectory(textBox2.Text + ToPath);
                        }
                    }
                    else
                    {
                        //we need to iterate down the path to create album artist
                        //F:\Music\Green Day\American Idiot (Parental Advisory)\01 American Idiot.mp3
                        //ToPath needs to be: Green Day\American Idiot (Parental Advisory)
                        string ArtistAlbumPath = "";
                        var FilePartsRel = FromPath.Split('\\');
                        ArtistAlbumPath = FilePartsRel[FilePartsRel.Count() - 3] + "\\" + FilePartsRel[FilePartsRel.Count() - 2];
                        ToPath = ArtistAlbumPath;

                        if (checkBox1.Checked == true)
                        {
                            FilePartsRel[FilePartsRel.Count() - 3] = Regex.Replace(FilePartsRel[FilePartsRel.Count() - 3], "[^a-zA-Z0-9\\s\\-\\\\\\.]", "");
                            FilePartsRel[FilePartsRel.Count() - 2] = Regex.Replace(FilePartsRel[FilePartsRel.Count() - 2], "[^a-zA-Z0-9\\s\\-\\\\\\.]", "");
                        }




                        if (!System.IO.Directory.Exists(textBox2.Text + FilePartsRel[FilePartsRel.Count() - 3]))
                        {
                            System.IO.Directory.CreateDirectory(textBox2.Text + FilePartsRel[FilePartsRel.Count() - 3]);
                        }
                        if (!System.IO.Directory.Exists(textBox2.Text + FilePartsRel[FilePartsRel.Count() - 3] + "\\" + FilePartsRel[FilePartsRel.Count() - 2]))
                        {
                            System.IO.Directory.CreateDirectory(textBox2.Text + FilePartsRel[FilePartsRel.Count() - 3] + "\\" + FilePartsRel[FilePartsRel.Count() - 2]);
                        }
                    }


                    var Fileparts = FromPath.Split('\\');
                    string FName = Fileparts[Fileparts.Count() - 1];

                    //ToPath  will be the PlayListName or Alubum/Artist
                    //Fname is the name of the file.
                    string DestPathPart = ToPath + "\\" + FName;

                    //if clean names, lets remove everything but a-z A_Z space and - and _ and \
                    if (checkBox1.Checked == true)
                    {
                        DestPathPart = Regex.Replace(DestPathPart, "[^a-zA-Z0-9\\s\\-\\\\\\.]", "");
                    }
                    string FullpathTo = textBox2.Text + DestPathPart;


                    if (File.Exists(FromPath))
                    {
                        if (!File.Exists(FullpathTo))
                        {


                            toolStripStatusLabel1.Text = "Copying: " + FromPath;
                            File.Copy(FromPath, FullpathTo, true);

                            Application.DoEvents();
                        }
                        else
                        {
                            toolStripStatusLabel1.Text = "Already Exsist: " + FromPath;
                        }
                        if (isCopying == true)
                        {
                            progressBar1.Value = progressBar1.Value + 1;
                        }
                    }
                    else
                    {
                        if (isCopying == true)
                        {
                            toolStripStatusLabel1.Text = "Skipping: " + FromPath;
                            progressBar1.Value = progressBar1.Value + 1;
                            Application.DoEvents();
                        }
                    }
                   
                }
                RetVal++;
            }//end for loop

            return RetVal;
        } //end LoadWPL


       


      

       

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadWPLList(folderBrowserDialog1.SelectedPath);
                textBox3.Text = folderBrowserDialog1.SelectedPath;
            }
        }//end button1_Click



        private void button2_Click(object sender, EventArgs e)
        {
            LoadWPLList(textBox3.Text);
        }//button2_Click



        private void button3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
                if (textBox2.Text.Substring((textBox2.Text.Length - 1), 1) != "\\")
                {
                    textBox2.Text += "\\";
                }
            }
        }//end button3_Click



        private void button4_Click(object sender, EventArgs e)
        {
            if( Directory.Exists(textBox2.Text) && Directory.Exists(textBox3.Text) )
            {
                if (isCopying == true)
                {
                    button4.Text = "Start Copy Songs";
                    isCopying = false;
                    if( toolStripStatusLabel1.Text.Contains("\\") )
                    {
                        toolStripStatusLabel1.Text = "You hit Stop: Last file copied was: " + toolStripStatusLabel1.Text;
                    }
                }
                else
                {
                    isCopying = true;
                    button4.Text = "Stop Copy Songs";
                    RunCopy();
                }
            }
            else
            {
                string Errormessage = "";
                if (!Directory.Exists(textBox2.Text) )
                {
                    Errormessage = "A Valid Destination Folder / Drive is Required.";
                }

                if (!Directory.Exists(textBox3.Text))
                {
                    Errormessage = "A Valid WPL Directory is Required.";
                }
                if (!Directory.Exists(textBox2.Text) && !Directory.Exists(textBox3.Text))
                {
                    Errormessage = "A Valid Destination Folder / Drive is Required." + Environment.NewLine + "A Valid WPL Directory is Required.";
                }
                MessageBox.Show(Errormessage);

            }
        }//end button4_Click

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SelectDestination = textBox2.Text;
            Properties.Settings.Default.Save();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SelectWPLDirectory = textBox3.Text;
            Properties.Settings.Default.Save();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
           
            Properties.Settings.Default.CleanNames = checkBox1.Checked.ToString() ;
            Properties.Settings.Default.Save();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.PlaylistOrArtist = checkBox2.Checked.ToString();
            Properties.Settings.Default.Save();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://ourace.com/130-wpl2usb");
        } 
    }
}
