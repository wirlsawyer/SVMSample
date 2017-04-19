using LibSVMsharp;
using LibSVMsharp.Extensions;
using LibSVMsharp.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SVMSample
{
    public partial class Form1 : Form
    {
        const String FILE_MODEL = @"Model\wine_model.txt";
        private int mWidth;
        private int mHeight;
        private List<DataInfo> mList = new List<DataInfo>();
        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int selIndex = 0;

                if (radioButton1.Checked) selIndex = 0;
                if (radioButton2.Checked) selIndex = 1;
                if (radioButton3.Checked) selIndex = 2;
                if (radioButton4.Checked) selIndex = 3;

                mList.Add(new DataInfo(selIndex, e.X, e.Y));

                Draw();
            }
            else if (e.Button == MouseButtons.Right)
            {

                SVMNode[] node = new SVMNode[2];
                node[0] = new SVMNode(1, (double)e.X / (double)mWidth);
                node[1] = new SVMNode(2, (double)e.Y / (double)mHeight);

                SVMModel model = SVM.LoadModel(FILE_MODEL);
                double result = SVM.Predict(model, node);
                Console.WriteLine("result=" + result);
            }
            
        }

        private void Draw()
        {
            Pen[] pen = new Pen[4];

            pen[0] = new Pen(Color.Black, 1);
            pen[1] = new Pen(Color.Red, 1);
            pen[2] = new Pen(Color.LightGreen, 1);
            pen[3] = new Pen(Color.Blue, 1);

            Bitmap canvas = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);

            using (Graphics g = Graphics.FromImage(canvas))
            {

                foreach (DataInfo info in mList)
                {
                    g.DrawEllipse(pen[(int)info.Group], (float)info.X - 5, (float)info.Y - 5, 5, 5);
                }
            }

            Bitmap image = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
            pictureBox1.BackgroundImage = canvas; // 設置為背景層
            pictureBox1.Refresh();
            pictureBox1.CreateGraphics().DrawImage(canvas, 0, 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mList.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SVMProblem trainingSet = new SVMProblem();
            SVMProblem testSet = trainingSet;
            foreach (DataInfo info in mList)
            {
                SVMNode[] node = new SVMNode[2];
                node[0] = new SVMNode(1, info.X / mWidth);
                node[1] = new SVMNode(2, info.Y / mHeight);
                trainingSet.Add(node, info.Group);
            }


            // Normalize the datasets if you want: L2 Norm => x / ||x||
            //trainingSet = trainingSet.Normalize(SVMNormType.L2);
           
            // Select the parameter set
            SVMParameter parameter = new SVMParameter();
            parameter.Type = SVMType.C_SVC;
            parameter.Kernel = SVMKernelType.RBF;
            parameter.C = 1;
            parameter.Gamma = 4;
            parameter.Coef0 = hScrollBar1.Value;
            parameter.Degree = 3;

            // Do cross validation to check this parameter set is correct for the dataset or not
            double[] crossValidationResults; // output labels
            int nFold = 5;
            trainingSet.CrossValidation(parameter, nFold, out crossValidationResults);

            // Evaluate the cross validation result
            // If it is not good enough, select the parameter set again
            double crossValidationAccuracy = trainingSet.EvaluateClassificationProblem(crossValidationResults);

            // Train the model, If your parameter set gives good result on cross validation
            SVMModel model = trainingSet.Train(parameter);
           
            // Save the model
            SVM.SaveModel(model, FILE_MODEL);

            // Predict the instances in the test set
            double[] testResults = testSet.Predict(model);

            // Evaluate the test results
            int[,] confusionMatrix;
            double testAccuracy = testSet.EvaluateClassificationProblem(testResults, model.Labels, out confusionMatrix);

            // Print the resutls
            Console.WriteLine("\n\nCross validation accuracy: " + crossValidationAccuracy);
            Console.WriteLine("\nTest accuracy: " + testAccuracy);
            Console.WriteLine("\nConfusion matrix:\n");

            // Print formatted confusion matrix
            Console.Write(String.Format("{0,6}", ""));
            for (int i = 0; i < model.Labels.Length; i++)
                Console.Write(String.Format("{0,5}", "(" + model.Labels[i] + ")"));
            Console.WriteLine();
            for (int i = 0; i < confusionMatrix.GetLength(0); i++)
            {
                Console.Write(String.Format("{0,5}", "(" + model.Labels[i] + ")"));
                for (int j = 0; j < confusionMatrix.GetLength(1); j++)
                    Console.Write(String.Format("{0,5}", confusionMatrix[i, j]));
                Console.WriteLine();
            }

            Pen[] pen = new Pen[4];
            pen[0] = new Pen(Color.Black, 1);
            pen[1] = new Pen(Color.Red, 1);
            pen[2] = new Pen(Color.LightGreen, 1);
            pen[3] = new Pen(Color.Blue, 1);

            Pen[] pen2 = new Pen[4];
            pen2[0] = new Pen(Color.LightGray, 1);
            pen2[1] = new Pen(Color.DarkRed, 1);
            pen2[2] = new Pen(Color.DarkGreen, 1);
            pen2[3] = new Pen(Color.DarkBlue, 1);

            Bitmap canvas = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);

            using (Graphics g = Graphics.FromImage(canvas))
            {

                for (int i = 0; i < pictureBox1.ClientSize.Width; i++)
                {
                    for (int j = 0; j < pictureBox1.ClientSize.Height; j++)
                    {
                        SVMNode[] node = new SVMNode[2];
                        node[0] = new SVMNode(1, (double)i / (double)mWidth);
                        node[1] = new SVMNode(2, (double)j / (double)mHeight);

                        double result = SVM.Predict(model, node);
                        g.DrawRectangle(pen2[(int)result], i, j, 1, 1);
                    }
                }

                foreach (DataInfo info in mList)
                {
                    g.DrawEllipse(pen[(int)info.Group], (float)info.X - 5, (float)info.Y - 5, 5, 5);
                }
            }

            Bitmap image = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
            pictureBox1.BackgroundImage = canvas; // 設置為背景層
            pictureBox1.Refresh();
            pictureBox1.CreateGraphics().DrawImage(canvas, 0, 0);


            

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mWidth = this.pictureBox1.ClientSize.Width;
            mHeight = this.pictureBox1.ClientSize.Height;
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            label1.Text = "C:" + e.NewValue;
        }
    }
}
