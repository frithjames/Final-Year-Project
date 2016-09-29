//TwitterApplication version 2
//DownloadTweets class - Downloads tweets to text file for creating training and test data

//Author: James Frith S13164173

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; //For reading and writing files
using LinqToTwitter; //For streaming tweets to download (Install-Package linqtotwitter)

namespace TwitterApplicationSVM_2
{
    class DownloadTweets
    {
        //Variables
        List<Status> currentTweets;
        List<string> lines = new List<string>();

        //No constructor necessary
        /*public DownloadTweets()
        {
        }*/

        //DownloadAndSaveTweets method
        public void DownloadAndSaveTweets()
        {
            //Authorizer, takes in access keys to allow twitter access
            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = "5cDzt1PrQD2AF8jrMx40NMfOA",
                    ConsumerSecret = "3NxX354pYGJDlZq90AcGAdBzs8VlPyRPjfX47yomvpNmPmQo2V",
                    AccessToken = "368313916-KK687HBeEc1ZuAECezHefnf4UpLqwUTpl9TenvSc",
                    AccessTokenSecret = "rZo730cZVte3gbwFr9ZD1zrGlIycAmaga4hFWxHGoNJm5"
                }
            };

            //Calls for 200 tweets
            var twitterContext = new TwitterContext(auth);
            var tweets = from tweet in twitterContext.Status
                         where tweet.Type == StatusType.Home &&
                         tweet.Count == 200
                         select tweet;
            
            //Creates list of tweets
            currentTweets = tweets.ToList();
            foreach (var t in currentTweets)
            {
                //Converts to list of strings
                lines.Add(t.Text);
            }

            //List of strings is saved to a text file
            //Existing TweetFiles are overwritten
            string fullPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName + @"\Downloaded Tweets\TweetFile.txt";
            System.IO.File.WriteAllLines(fullPath, lines.ToArray());

        }//Closes method

    }//Closes class
}
