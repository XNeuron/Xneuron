﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Drawing;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using Emgu.CV.ML.Structure;
using Emgu.CV;
using System.IO;

namespace ANN1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            int trainSampleCount = 100;

            #region Generate the traning data and classes
            Matrix<float> trainData = new Matrix<float>(trainSampleCount, 2);
            Matrix<float> trainClasses = new Matrix<float>(trainSampleCount, 1);

            Image<Bgr, Byte> img = new Image<Bgr, byte>(500, 500);

            Matrix<float> sample = new Matrix<float>(1, 2);
            Matrix<float> prediction = new Matrix<float>(1, 1);

            Matrix<float> trainData1 = trainData.GetRows(0, trainSampleCount >> 1, 1);
            trainData1.SetRandNormal(new MCvScalar(200), new MCvScalar(50));
            Matrix<float> trainData2 = trainData.GetRows(trainSampleCount >> 1, trainSampleCount, 1);
            trainData2.SetRandNormal(new MCvScalar(300), new MCvScalar(50));

            Matrix<float> trainClasses1 = trainClasses.GetRows(0, trainSampleCount >> 1, 1);
            trainClasses1.SetValue(1);
            Matrix<float> trainClasses2 = trainClasses.GetRows(trainSampleCount >> 1, trainSampleCount, 1);
            trainClasses2.SetValue(2);
            #endregion

            using (Matrix<int> layerSize = new Matrix<int>(new int[] { 2,10 ,2 }))
            using (Mat layerSizeMat = layerSize.Mat)

            using (TrainData td = new TrainData(trainData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, trainClasses))
            using (ANN_MLP network = new ANN_MLP())
            {
                network.SetLayerSizes(layerSizeMat);
                network.SetActivationFunction(ANN_MLP.AnnMlpActivationFunction.SigmoidSym);
                network.TermCriteria = new MCvTermCriteria(10000, 1.0e-8);
                network.SetTrainMethod(ANN_MLP.AnnMlpTrainMethod.Backprop, 0.1, 0.1);
                network.Train(td);
                network.Save("temp.txt");
                for (int i = 0; i < img.Height; i++)
                {
                    for (int j = 0; j < img.Width; j++)
                    {
                        sample.Data[0, 0] = j;
                        sample.Data[0, 1] = i;
                        network.Predict(sample, prediction);

                        // estimates the response and get the neighbors' labels
                        float response = prediction.Data[0, 0];

                        // highlight the pixel depending on the accuracy (or confidence)
                        if (response < 1.5)
                            img[i, j] =  new Bgr(90, 0, 0);
                        else
                            img[i, j] =  new Bgr(0, 90, 0);

                    }
                }
            }

            // display the original training samples
            for (int i = 0; i < (trainSampleCount >> 1); i++)
            {
                PointF p1 = new PointF(trainData1[i, 0], trainData1[i, 1]);
                img.Draw(new CircleF(p1, 2), new Bgr(255, 100, 100), -1);
                PointF p2 = new PointF((int)trainData2[i, 0], (int)trainData2[i, 1]);
                img.Draw(new CircleF(p2, 2), new Bgr(100, 255, 100), -1);
            }
            pictureBox1.Image = img.ToBitmap();
           // Emgu.CV.UI.ImageViewer.Show(img);
        }
    }
}
