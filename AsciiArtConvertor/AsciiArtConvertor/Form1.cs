using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using System.Threading;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string[] asciiChars = { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", "&nbsp;" };
        private string content;
        private bool isPlaying = false;

        private void Form1_Load(object sender, EventArgs e)
        {
        }


        private void btnConvertToAscii_Click(object sender, EventArgs e)
        {
            btnConvertToAscii.Enabled = false;
            try
            {
                using (var stream = new FileStream(txtPath.Text, FileMode.Open, FileAccess.Read))
                {
                    Bitmap image = new Bitmap(txtPath.Text, true);
                    SetControlUI(pictureBox1, () => pictureBox1.BackgroundImage = image);
                    image = GetReSizedImage(image, this.trackBar.Value);

                    content = ConvertToAscii(image);

                    SetControlUI(browserMain, () => browserMain.DocumentText = "<pre>" + "<Font size=0>" + content + "</Font></pre>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("해당 위치에 파일이 없거나, 이미지 파일이 아닌것 같습니다.!!\n{0}", ex.ToString());
            }
            finally
            {
                btnConvertToAscii.Enabled = true;
            }
        }

        private void ConvertMp4ToBitmap(object sender, EventArgs e)
        {

            if (isPlaying)
            {
                isPlaying = false;
                button1.Text = "Video Convert";
                return;
            }

            if (!File.Exists(txtPath.Text))
            {
                Console.WriteLine("file path가 정확하지 않은것 같습니다.!");
                return;
            }

            int value = trackBar.Value;
            Mat frame = new Mat();

            string extension = Path.GetExtension(txtPath.Text).ToLower(); // 파일 확장자를 소문자로 변환하여 가져옴
            if (extension == ".mp4" || extension == ".avi" || extension == ".wmv" || extension == ".mov")
            {
                Task.Factory.StartNew(() =>
                {

                    VideoCapture capture = new VideoCapture(txtPath.Text);
                    isPlaying = true;
                    SetControlUI(button1, () => button1.Text = "Video Stop");

                    Bitmap bitmap;
                    while (true)
                    {
                        capture.Read(frame);
                        if (frame.Empty() || !isPlaying) break;
                        bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);
                        if (pictureBox1.Visible)
                        {
                            SetControlUI(pictureBox1, () => pictureBox1.BackgroundImage = bitmap);
                        }
                        bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);

                        bitmap = GetReSizedImage(bitmap, value);

                        content = ConvertToAscii(bitmap);
                        SetControlUI(browserMain, () => browserMain.DocumentText = "<pre>" + "<Font size=0>" + content + "</Font></pre>");
                    }
                    capture.Release();
                    isPlaying = false;
                    Console.WriteLine("Exit");
                });
            }
            else
            {
                Console.WriteLine("파일이 영상파일이 아닌것 같습니다 다시한번 확인해 주세요");
            }




        }

        private delegate void ControlDelegate(Control ctrl, Action func);

        private void SetControlUI(Control ctrl, Action func)
        {
            try
            {
                if (ctrl.InvokeRequired)
                {
                    var delegateCallback = new ControlDelegate(SetControlUI);
                    this.Invoke(delegateCallback, new object[] { ctrl, func });
                }
                else
                {
                    func();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        private string ConvertToAscii(Bitmap image)
        {
            Boolean toggle = false;
            StringBuilder sb = new StringBuilder();

            for (int h = 0; h < image.Height; h++)
            {
                for (int w = 0; w < image.Width; w++)
                {
                    Color pixelColor = image.GetPixel(w, h);

                    int red = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    int green = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    int blue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    Color grayColor = Color.FromArgb(red, green, blue);

                    if (!toggle)
                    {
                        int index = (grayColor.R * 10) / 255;
                        sb.Append(asciiChars[index]);
                    }
                }
                if (!toggle)
                {
                    sb.Append("<BR>");
                    toggle = true;
                }
                else
                {
                    toggle = false;
                }
            }
            return sb.ToString();
        }


        private Bitmap GetReSizedImage(Bitmap inputBitmap, int asciiWidth)
        {
            int asciiHeight = (int)Math.Ceiling((double)inputBitmap.Height * asciiWidth / inputBitmap.Width);
            Bitmap result = new Bitmap(asciiWidth, asciiHeight);
            Graphics g = Graphics.FromImage((Image)result);
            SetControlUI(label2, () => label2.Text = $"Resolution Height : {asciiHeight.ToString()}");
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(inputBitmap, 0, 0, asciiWidth, asciiHeight);
            g.Dispose();
            return result;
        }


        private void btnBrowse_Click(object sender, EventArgs e)
        {
            DialogResult diag = openFileDialog1.ShowDialog();
            if (diag == DialogResult.OK)
            {
                txtPath.Text = openFileDialog1.FileName;
            }
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Visible)
            {
                button2.Text = "AsciiArt View";
            }
            else
            {
                button2.Text = "Original View";
            }

            pictureBox1.Visible = !pictureBox1.Visible;
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            label1.Text = $"Resolution : {trackBar.Value}";
        }
    }
}
