//Twitter Application version 2
//Reads in Tweets as text input and classifies them as positive or negative
//TwitterProgram class - Acts as main menu for application

//Author: James Frith S13164173

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libsvm; //For SVM features (Install-Package libsvm.net)

namespace TwitterApplicationSVM_2
{
    class TwitterProgram
    {
        static void Main(string[] args)
        {
            //Variables
            int number;
            bool result;
            string input;
            string fileName;
            const string stopWordFileName = "stopWords";
            DownloadTweets downloader;
            GetFeatureVector features;
            GetFeatureVector testFeatures;

            //Prompts user for input
            Console.WriteLine("1. Train and Test SVM");
            Console.WriteLine("2. Download tweets to file (Development Use Only)");
            Console.WriteLine("3. Exit\n");
            Console.Write("Please Enter 1 or 2 to select option: ");
            input = Console.ReadLine();
            result = Int32.TryParse(input, out number);

            //If the input is number 1 then Train and Test SVM
            if (result && number == 1)
            {
                //Asks user for name of training file
                Console.Clear();
                Console.WriteLine("Enter name of training file (Must be in Input folder. No extension nedded. Example: 'sampleTweets')");
                Console.Write("Or press enter to use default file: ");
                fileName = Console.ReadLine();
                Console.WriteLine();

                //Loads training file into GetFeatureVector class
                if (fileName == "")
                {
                    features = new GetFeatureVector(stopWordFileName, "BCDE");
                }
                else 
                {
                    features = new GetFeatureVector(stopWordFileName, fileName);
                }

                //"0" Indicates that FeatureExtraction is being called for training.
                //When reading a training file a list of features is created, but not when reading a test file.
                features.FeatureExtraction("0");

                //SVM problem is created from test file
                var problem = new svm_problem();
                problem.l = (features.GetPolarityClass()).Count;
                problem.x = (features.GetListOfTweetValues()).ToArray();
                problem.y = (features.GetPolarityClass()).ToArray();

                //Asks user for name of test file
                Console.WriteLine("Enter name of testing file (Must be in Input folder. No extension nedded. Example: 'sampleTestTweets')");
                Console.Write("Or press enter to use default file: ");
                fileName = Console.ReadLine();
                Console.WriteLine();

                //Loads test file into new GetFeatureVector class
                if (fileName == "")
                {
                    testFeatures = new GetFeatureVector(stopWordFileName, "60A");
                }
                else
                {
                    testFeatures = new GetFeatureVector(stopWordFileName, fileName);
                }

                //The FeatureList generated with the test file is passed to this GetFeatureVector
                testFeatures.SetFeatureList(features.GetFeatureList());
                //"1" Indicates FeatureExtraction is being called for testing.
                //No feature list is created.
                testFeatures.FeatureExtraction("1");

                //The SVM test problem is created
                var test = new svm_problem();
                test.l = (testFeatures.GetPolarityClass()).Count;
                test.x = (testFeatures.GetListOfTweetValues()).ToArray();
                test.y = (testFeatures.GetPolarityClass()).ToArray();

                //Create SVM model from training problem
                const int C = 1;
                var model = new C_SVC(problem, KernelHelper.LinearKernel(), C);

                //Not implemented yet
                //int nr_fold = 10;
                //var accuracy = model.GetCrossValidationAccuracy(nr_fold); crossvalidation applied

                //Results are displayed
                double accuracy;
                double correct = 0;
                Console.Clear();
                Console.WriteLine("SVM Results:");
                Console.WriteLine("Matched Features | Class label | Predicted label\n");

                //For each test tweet
                for (int i = 0; i < test.l; i++)
                {
                    var x = test.x[i];//Features of tweet
                    var y = test.y[i];//Polarity of tweet
                    var predictedY = model.Predict(x); //Predicted polarity of tweet

                    //Not implemented yet
                    //var probabilities = model.PredictProbabilities(x); returns the probabilities for each class

                    //For each feature of a tweet, if it was matched to a feature from the training data the feature is added to a string
                    //Creates string of matched features
                    string displayLine = "";
                    foreach (var node in x)
                    {
                        if (node.value == 1)
                        {
                            displayLine += features.GetFeatureList().ElementAt(node.index) + " ";
                        }
                        
                    }
                    //Writes features, actual value and predicted value to the screen
                    Console.Write(displayLine); Console.Write(y); Console.Write(" " + predictedY);
                    Console.WriteLine();

                    if (y == predictedY)
                    {
                        correct += 1;
                    }
                }//Closes for loop

                //Accuracy is calculated
                accuracy = (correct / test.l) * 100;
                Console.WriteLine();
                Console.WriteLine("CORRECT: {0}/{1}", correct, test.l);
                Console.Write("ACCURACY: {0}%", accuracy);

            }//Closes If "1"

            //If the input is number 2 then Download Tweets
            //This is used for creating training and test data
            else if (result && number == 2)
            {
                Console.Clear();
                Console.WriteLine("Downloading...");

                //DownloadTweets class is created, method is called to download tweets to a text file
                downloader = new DownloadTweets();
                downloader.DownloadAndSaveTweets();
                Console.Write("Downloaded successfully!");
                Console.ReadLine();
            }

            //If neither "1" or "2" are entered, close program
            else 
            {
                Environment.Exit(0);
            }

            Console.ReadLine(); //Pauses program
        }//Closes Main
    }
}
