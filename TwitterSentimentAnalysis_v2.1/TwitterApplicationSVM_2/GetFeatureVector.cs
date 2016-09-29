//TwitterApplication version 2
//GetFeatureVector class - Reads files, extracts features, returns tweet feature and polarity lists

//Author: James Frith

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; //For reading and writing files
using System.Text.RegularExpressions; //For use of Regex operations
using libsvm; //For SVM objects (Install-Package libsvm.net)

namespace TwitterApplicationSVM_2
{
    class GetFeatureVector
    {
        //Variables
        string stopWordFile;
        string inputFile;
        string path;
        string line;
        string word;
        string tweet;
        string[] split = new string[2];
        List<string> stopList = new List<string>();
        List<string> wordList = new List<string>();
        List<string> polarityList = new List<string>();
        List<string> featureList = new List<string>();
        List<double> polarityClass = new List<double>();
        List<svm_node[]> listOfTweetValues = new List<svm_node[]>();
        List<List<string>> tweetPolarity = new List<List<string>>();
        List<List<List<string>>> featureVector = new List<List<List<string>>>();
        StreamReader textFile;

        //Constructor method, passes file names into the class
        public GetFeatureVector(string stopName, string inputName)
        {
            stopWordFile = stopName;
            inputFile = inputName;
        }

        //FeatureExtraction method, indicator parameter indicates whether its being used on a training file or testing file
        public void FeatureExtraction(string indicator)
        {
            //Initialises stop word list
            stopList.Clear();
            stopList.Add("AT_USER"); stopList.Add("URL"); stopList.Add("rt");
            path = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName + @"\Application Files\" + stopWordFile + ".txt";
            textFile = new StreamReader(path);
            line = textFile.ReadLine();

            while (line != null)
            {
                word = Regex.Replace(line, @"[\s]+", "");
                stopList.Add(word);
                line = textFile.ReadLine();
            }//Closes while

            textFile.Close();
            
            //Opens tweet file and proccesses each line
            path = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName + @"\Input\" + inputFile + ".txt";

            try 
            {
                textFile = new StreamReader(path);
                line = textFile.ReadLine();

                //Continue to read until you reach end of file
                while (line != null)
                {
                    //Splits line up into polarity score and tweet text
                    split = line.Split(new char[] { '|' }, 2);
                    polarityList.Add(split[0]); //Polarity is saved in a list
                    tweet = split[1];

                    //The text of the tweet is processed so the features can be extracted
                    //[^\s] represents anything that isn't white space
                    //+ means matched one or more times
                    tweet = tweet.ToLower(); //makes all lower case
                    string pattern = @"((www\.[^\s]+)|(https?://[^\s]+))";
                    tweet = Regex.Replace(tweet, pattern, "URL"); //Replaces urls with "URL"
                    tweet = Regex.Replace(tweet, "#", ""); //Removes hashes
                    tweet = Regex.Replace(tweet, @"@[^\s]+", "AT_USER"); //Replaces user names with AT_USER
                    tweet = Regex.Replace(tweet, @"[\s]+", " "); //Extra white space is removed
                    tweet = Regex.Replace(tweet, @"([a-z])\1{2,}", "$1");  //Duplicate letters in a word are replaced
                    //Example: "wooooooooord" becomes "word"

                    //Each tweet is split up into words
                    foreach (string s in tweet.Split(null as string[], StringSplitOptions.RemoveEmptyEntries))
                    {
                        //For each word in the tweet punctuation and white space are removed
                        word = Regex.Replace(s, "[\"\'\\?\\,\\.\\!]", "");
                        word = Regex.Escape(word);
                        word = Regex.Replace(word, @"[\s]+", "");
                        //Word is checked to see if it begins with a letter
                        var val = Regex.IsMatch(word, @"^[a-zA-Z]"); //Need to make exception for emoji
                        if (stopList.Contains(word) || val == false)
                        {
                            //If word is a stop word or does not begin with a letter it is ignored
                        }
                        else
                        {
                            if (indicator == "0")
                            {
                                //If the method is reading a training file then the word is added to the feature list
                                featureList.Add(word);
                            }
                            else
                            {
                                //Word is not added to featurelist
                            }

                            //Word is added to a list of all the other words in that tweet
                            wordList.Add(word);
                        }
                    }

                    //The tweetPolarity list contains the list of words in a tweet and the polarity of the tweet
                    //tweetPolarity represents all the information for one tweet
                    tweetPolarity.Add(new List<string>(wordList));
                    tweetPolarity.Add(new List<string>(polarityList));
                    featureVector.Add(new List<List<string>>(tweetPolarity)); //featureVector is a list of all information for all tweets

                    //Temporary lists are cleared to be used again in the next loop
                    wordList.Clear();
                    polarityList.Clear();
                    tweetPolarity.Clear();

                    //The next tweet in the file is read
                    line = textFile.ReadLine();

                }//Closes while

                textFile.Close();

                //The featureVector is processed further so it can be used as SVM input
                foreach (var line_i in featureVector)
                {
                    double score;
                    
                    //Converts the polarity of a tweet into +1 for positive and -1 for negative
                    //These are the class labels we want to predict
                    if ((line_i.ElementAt(1)).ElementAt(0) == "p ")
                    {
                        score = +1;
                    }
                    else 
                    {
                        score = -1;
                    }

                    polarityClass.Add(score); //The polarity score is added to a list of scores

                    //tweetValues contains the index:attribute pairs that the svm reads.
                    List<svm_node> tweetValues = new List<svm_node>();
                    double attributeValue;

                    //For each feature in the feature list and for each word in each tweet:
                    //The word is checked to see if its in the feature list. If yes then the value for that feature is 1
                    //Otherwise its set to 0
                    //This creates a set of pairs for each tweet that look like this: "1:0 2:0 3:1 4:1 5:0"
                    for (int j = 0; j < featureList.Count(); j++)
                    {
                        List<string> w = new List<string>();
                        w = line_i.ElementAt(0);
                        foreach (string wor in w)
                        {
                            if (featureList.Contains(wor))
                            {
                                attributeValue = 1;
                            }
                            else 
                            {
                                attributeValue = 0;
                            }

                            //These attirbute value pairs are added to the tweetValues list
                            tweetValues.Add(new svm_node() { index = featureList.IndexOf(wor), value = attributeValue });
                        }//Closes foreach word
                        w.Clear(); //Cleared for reading next tweet

                    }//Closes foreach feature

                    listOfTweetValues.Add(tweetValues.ToArray()); //List of arrays, each array is one set of attribute:value pairs
                }//Closes foreach

            }//Close try

            //If the file fails to open, an error is displayed and the program closes
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                Console.ReadLine();
                Environment.Exit(0);
            }

        }//Closes FeatureExtraction

        //GetPolarityClass returns List of tweet polarities
        public List<double> GetPolarityClass()
        {
            return polarityClass;
        }

        //GetListOfTweetValues returns index:value pairs for each tweet
        //Index represents a feature in the feature list, the value is 0 or 1 depending on if that feature is matched in the tweet
        public List<svm_node[]> GetListOfTweetValues()
        {
            return listOfTweetValues;
        }

        //GetFeatureList returns feature list generated from training data
        public List<string> GetFeatureList()
        {
            return featureList;
        }

        //SetFeatureList passes a featurelist into the class
        public void SetFeatureList(List<string> listOfFeatures)
        {
            featureList = listOfFeatures;
        }

    }//Closes class
}
