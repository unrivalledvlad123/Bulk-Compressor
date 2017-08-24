using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Bulk_Compressor
{
    public partial class Form1 : Form
    {
        public static List<string> AllDirectoriesFull = new List<string>();
        public static Dictionary<string,List<string>> StructuredCollection = new Dictionary<string, List<string>>();
        public static Dictionary<string, List<string>> StructuredFiles = new Dictionary<string, List<string>>();

        public Form1()
        {
            InitializeComponent();
            SetGridColomns();
            btnAdd.Enabled = false;
        }

        private void tbStart_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tbStart.Text = dialog.SelectedPath;
            }
        }

        private void tbOutput_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tbOutput.Text = dialog.SelectedPath;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            string[] subdirectoryEntries = Directory.GetDirectories(tbStart.Text);
            AllDirectoriesFull = subdirectoryEntries.ToList();
            List<Grid> clearDirectories = new List<Grid>();
            foreach (string dir in subdirectoryEntries)
            {
                Grid grid = new Grid();
                grid.Checked = false;
                grid.Name = dir.Replace(tbStart.Text, "").Replace("\\", "");
                grid.Fullpath = dir;
                clearDirectories.Add(grid);
            }
            dgvFolders.DataSource = clearDirectories;
        }

        private void SetGridColomns()
        {
            dgvFolders.DataSource = null;
            dgvFolders.Columns.Clear();
            dgvFolders.AutoGenerateColumns = false;
            dgvFolders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFolders.RowHeadersVisible = false;


            DataGridViewCheckBoxColumn c1 = new DataGridViewCheckBoxColumn();
            c1.Name = "Checked";
            c1.HeaderText = "Checked";
            c1.DataPropertyName = "Checked";
            c1.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dgvFolders.Columns.Add(c1);

            DataGridViewTextBoxColumn c2 = new DataGridViewTextBoxColumn();
            c2.Name = "Name";
            c2.HeaderText = "Name";
            c2.DataPropertyName = "Name";
            c2.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvFolders.Columns.Add(c2);

            DataGridViewTextBoxColumn c3 = new DataGridViewTextBoxColumn();
            c3.Name = "Fullpath";
            c3.HeaderText = "Fullpath";
            c3.DataPropertyName = "Fullpath";
            c3.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            c3.Visible = false;
            dgvFolders.Columns.Add(c3);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            List<Grid> newDatasource = new List<Grid>();
            List<string> folders = new List<string>();
            foreach (DataGridViewRow i in dgvFolders.Rows)
            {
               
                Grid row = (Grid) i.DataBoundItem;
                if (row.Checked)
                {
                    folders.Add(row.Fullpath);
                }
                else
                {
                    newDatasource.Add(row);
                }
            }
            StructuredCollection.Add(tbOutputName.Text,folders);
            tbOutputName.Text = "";
            dgvFolders.DataSource = null;
            dgvFolders.DataSource = newDatasource;

        }

        private void btnCompress_Click(object sender, EventArgs e)
        {

            float progressCounter = 1;
            float totalCount = 0;
            foreach (var element in StructuredCollection)
            {
                List<string> rows = new List<string>();
                foreach (var folders in element.Value)
                {
                    string[] files = Directory.GetFiles(folders);
                    foreach (string file in files)
                    {
                        rows.Add(file);
                    }
                    totalCount += files.Length;
                }
                StructuredFiles.Add(element.Key, rows);
            }
            foreach (var row in StructuredFiles)
            {
                Directory.CreateDirectory(tbOutput.Text + @"\" + row.Key);
                foreach (var file in row.Value)
                {
                    string[] output = file.Split(Convert.ToChar(@"\"));
                    resizeImage(file, tbOutput.Text + @"\" + row.Key, output.Last(), (int) tbHeight.Value,
                        (int) tbQuality.Value);
                    progressCounter++;
                    if (progressCounter % 5 == 0)
                    {
                        float progressPercent = (progressCounter / totalCount) * 100;
                        progressBar.Value = (int) progressPercent;
                        labelProgress.Text = "Compressing...." + progressPercent.ToString("#.##") + " ready!";
                        progressBar.Update();
                    }
                }
            }
            progressBar.Value = 100;
            labelProgress.Text = "Done!";
            progressBar.Update();
        }

        private void resizeImage(string fullPath,string newPath, string originalFilename, int canvasHeight,int quality)
        {
            Image image = Image.FromFile(fullPath);
            // must calculate width before that based on hights
            var originalHeight1 = image.Height;
            var ratio1 = originalHeight1 / canvasHeight;
            var newwidth = 0;
            newwidth = ratio1 == 0 ? image.Width / 1 : image.Width / ratio1;
            
            Image thumbnail = new Bitmap(newwidth, canvasHeight);
            Graphics graphic = Graphics.FromImage(thumbnail);
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.SmoothingMode = SmoothingMode.HighQuality;
            graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = CompositingQuality.HighQuality;

            double ratioX = (double)newwidth / (double) image.Width;
            double ratioY = (double) canvasHeight / (double) image.Height;

            double ratio = ratioX < ratioY ? ratioX : ratioY;


            int newHeight = Convert.ToInt32(image.Height * ratio);
            int newWidth = Convert.ToInt32(image.Width * ratio);

            int posX = Convert.ToInt32((newwidth - (image.Width * ratio)) / 2);
            int posY = Convert.ToInt32((canvasHeight - (image.Height * ratio)) / 2);

            graphic.Clear(Color.Black);
            graphic.DrawImage(image, posX, posY, newWidth, newHeight);

            ImageCodecInfo[] info = ImageCodecInfo.GetImageEncoders();
            EncoderParameters encoderParameters;
            encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            thumbnail.Save(newPath + @"\" +originalFilename, info[1], encoderParameters);
        }

        private void tbOutputName_TextChanged(object sender, EventArgs e)
        {
            btnAdd.Enabled = !string.IsNullOrWhiteSpace(tbOutputName.Text);
        }
    }
}
