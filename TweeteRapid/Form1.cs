// Last modified on 26th May 2010 1:31 IST by Naren.
// This is not a fully tested version. Only the Core working components have been tested.
// TwitPic must be authenticated via OAuth, currently direct authentication is used.
// This code is too old to be maintained anymore! - Naren June 2, 2014.
// Note: Twitter allows only 150 Requests per hour.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using TweeteRapid;
using System.IO;
using System.Web;
using System.Xml;
using Twitterizer;


namespace TweeteRapid
{
    public partial class Form1 : Form
    {
        public Form1()
        {
           
            InitializeComponent();

        }

        OAuthTokenResponse mytoken = new OAuthTokenResponse();
        String longurl;
        OAuthTokens authToken = new OAuthTokens();
        String[] frndNames = new String[10];
        String[] followerNames = new String[10];
        String filename;
        XmlDocument mydoc = new XmlDocument();
        String TwitpicAPIKey = " ";  //Specify the TwipPic API here for V 1 and V 2


        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://dev.twitter.com/apps/new");

        }

        string HttpPost(string uri, string parameters)
        {
            // parameters: name1=value1&name2=value2	
            WebRequest webRequest = WebRequest.Create(uri);
      
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(parameters);
            Stream os = null;
            try
            { // send the Post
                webRequest.ContentLength = bytes.Length;   //Count bytes to send
                os = webRequest.GetRequestStream();
                os.Write(bytes, 0, bytes.Length);         //Send it
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Message, "HttpPost: Request error",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (os != null)
                {
                    os.Close();
                }
            }

            try
            { // get the response
                WebResponse webResponse = webRequest.GetResponse();
                if (webResponse == null)
                { return null; }
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                return sr.ReadToEnd().Trim();
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Message, "HttpPost: Response error",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        } // Method to make HTTP Posts

        private void button3_Click(object sender, EventArgs e)
        {
            consumerKeyText.ReadOnly = false;
            consumerSecretText.ReadOnly = false;
            consumerKeyText.Text = "";
            consumerSecretText.Text = "";
            Settings1.Default.consumerKey = "";
            Settings1.Default.consumerSecret = "";
            Settings1.Default.Save();
            appRegisteration.Enabled = true;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            pinText.Text = "";
            Settings1.Default.pin = "";
            Settings1.Default.Save();
            pinText.ReadOnly = false;
            authenticationButton.Enabled = true;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            bitly mybit = new bitly();
            mybit.ShowDialog();
        }

        public String urlShortner()
        {
            if (Settings1.Default.link.Length == 0)
            {
                MessageBox.Show("Please enter a link", "Error", MessageBoxButtons.OK);
            }
            String biturl = " http://api.bit.ly/v3/shorten?";

            longurl = HttpUtility.UrlEncode(Settings1.Default.link);
            String paramaters = "login=" + Settings1.Default.bitlyUser + "&apiKey=" + Settings1.Default.bitlyAPI + "&longurl=" + longurl + "&format=txt";

            String shortenurl = HttpPost(biturl, paramaters);

            return shortenurl;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Settings1.Default.consumerKey = consumerKeyText.Text;
            Settings1.Default.consumerSecret = consumerSecretText.Text;
            Settings1.Default.Save();
            consumerSecretText.ReadOnly = true;
            consumerKeyText.ReadOnly = true;
            appRegisteration.Enabled = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {

            if (Settings1.Default.bitlyUser.Length == 0 || Settings1.Default.bitlyAPI.Length == 0)
            {
                DialogResult dlgResult = MessageBox.Show("The Bit.ly API settings were not provided, would you like to enter them now?", "Confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (dlgResult == DialogResult.Yes)
                {
                    bitly mybit = new bitly(); // Open the Biy.ly API Settings dialog
                    mybit.ShowDialog();

                    link mylink = new link(); // Open the Insert Link Dialog
                    mylink.ShowDialog();

                    try
                    {
                        tweetText.AppendText(urlShortner()); //Append the shortened link in the text box
                    }
                    catch (Exception ex)
                    {

                    }


                }

                else if (dlgResult == DialogResult.No)
                {
                    link mylink = new link(); // Open the Insert link dialog box
                    mylink.ShowDialog();

                    tweetText.AppendText(Settings1.Default.link); // Append the link in the text box
                }

                else
                {
                    // Do nothing
                }

            }
            else
            {
                link mylink = new link(); // Open the Insert Link Dialog
                mylink.ShowDialog();

                if (Settings1.Default.link.Length != 0)
                    tweetText.AppendText(urlShortner());
            }


        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            
            
            mytoken = OAuthUtility.GetRequestToken(Settings1.Default.consumerKey,Settings1.Default.consumerSecret);
            System.Diagnostics.Process.Start("http://twitter.com/oauth/authorize?oauth_token=" + mytoken.Token);

            
        }
     

        private void button4_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;


            
            Settings1.Default.pin = pinText.Text;
            Settings1.Default.Save();
            mytoken = OAuthUtility.GetAccessToken(Settings1.Default.consumerKey, Settings1.Default.consumerSecret, mytoken.Token, Settings1.Default.pin);

            authToken.ConsumerKey = Settings1.Default.consumerKey;
            authToken.ConsumerSecret = Settings1.Default.consumerSecret;
            authToken.AccessToken = mytoken.Token;
            authToken.AccessTokenSecret = mytoken.TokenSecret;

            TwitterUser myuser = new TwitterUser(authToken);
            TwitterStatusCollection mycollection = TwitterUser.GetTimeline(authToken);
            
           

            Settings1.Default.consumerKey = authToken.ConsumerKey;
            Settings1.Default.consumerSecret = authToken.ConsumerSecret;
            Settings1.Default.token = authToken.AccessToken;
            Settings1.Default.secretToken = authToken.AccessTokenSecret;

            Settings1.Default.Save();
            pinText.Text = Settings1.Default.pin;
            pinText.ReadOnly = true;
            authenticationButton.Enabled = false;
            authStatus.Text = "Authentication Successful!";

           
           
           
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;



            if (tweetText.Text.Length > 140)
            {

                String serviceurl = "http://tinypaste.com/api/create.xml";

                tweetText.AppendText(" ");
                String text = tweetText.Text;
                String tolinkify = text.Replace(text.Remove(110), "");
                String paramaters = "paste=" + text;

                String result = HttpPost(serviceurl, paramaters);

                mydoc.LoadXml(result);
                XmlNodeList shrtlink = mydoc.GetElementsByTagName("response");
                result = shrtlink[0].InnerText;

                result = "http://tinypaste.com/" + result;

                String acttext = tweetText.Text.Replace(tolinkify, result);

                tweetText.Text = acttext;
                authToken.ConsumerKey = Settings1.Default.consumerKey;
                authToken.ConsumerSecret = Settings1.Default.consumerSecret;
                authToken.AccessToken = Settings1.Default.token;
                authToken.AccessTokenSecret = Settings1.Default.secretToken;
                Twitterizer.TwitterStatus.Update(authToken, acttext);

                postStatus.Text = "Posted Successfully!";

                tweetText.Text = "";


            }
            else
            {
                authToken.ConsumerKey = Settings1.Default.consumerKey;
                authToken.ConsumerSecret = Settings1.Default.consumerSecret;
                authToken.AccessToken = Settings1.Default.token;
                authToken.AccessTokenSecret = Settings1.Default.secretToken;
                Twitterizer.TwitterStatus.Update(authToken, tweetText.Text);
                tweetText.Text = "";
                postStatus.Text = "Posted Successfully!";

            }
            Cursor.Current = Cursors.Default;

        }

        private void button9_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            credentials mycredentials = new credentials();
            mycredentials.ShowDialog();
            UploadPictureAndPost(Settings1.Default.username, Settings1.Default.password, filename, tweetText.Text);
            Cursor.Current = Cursors.Default;
            tweetText.Text = "";
            postStatus.Text = "Posted Successfully!";
        }

        //Uses old authentication method. Must be changed to OAuth before June 30th. http://countdowntooauth.com/
        string UploadPictureAndPost(String username, String password, String path, String message)
        {


            string resonseText = string.Empty;
            byte[] imagebytes = File.ReadAllBytes(path);
            string fileBinary = Encoding.GetEncoding("iso-8859-1").GetString(imagebytes);
            string boundary = Guid.NewGuid().ToString();
            string header = "--" + boundary;
            string footer = "--" + boundary + "--";

            HttpWebRequest uploadRequest =
         (HttpWebRequest)WebRequest.Create("http://api.twitpic.com/uploadAndPost");
            uploadRequest.PreAuthenticate = true;
            uploadRequest.AllowWriteStreamBuffering = true;

            uploadRequest.ContentType = "multipart/form-data; boundary=" + boundary;
            uploadRequest.Method = "POST";

            StringBuilder headers = new StringBuilder();

            headers.AppendLine(header);
            headers.AppendLine("Content-Disposition: file; name=\"media\"; filename=\"" + path + "\"");
            headers.AppendLine("Content-Type: image/" + Path.GetExtension(path).ToLower().Substring(1, Path.GetExtension(path).Length - 1));
            headers.AppendLine();
            headers.AppendLine(fileBinary);
            headers.AppendLine(header);




            headers.AppendLine("Content-Disposition: form-data; name=\"username\";");
            headers.AppendLine();
            headers.AppendLine(username);
            headers.AppendLine(header);

            headers.AppendLine("Content-Disposition: form-data; name=\"password\";");
            headers.AppendLine();
            headers.AppendLine(password);
            headers.AppendLine(header);

            /*
            headers.AppendLine("Content-Disposition: form-data; name=\"key\";");
            headers.AppendLine();
            headers.AppendLine("0af2a47613a03bd5956b4a56089e0d24");
            headers.AppendLine(header);

            headers.AppendLine("Content-Disposition: form-data; name=\"consumer_token\";");
            headers.AppendLine();
            headers.AppendLine(Settings1.Default.consumerKey);
            headers.AppendLine(header);

            headers.AppendLine("Content-Disposition: form-data; name=\"consumer_secret\";");
            headers.AppendLine();
            headers.AppendLine(Settings1.Default.consumerSecret);
            headers.AppendLine(header);

            headers.AppendLine("Content-Disposition: form-data; name=\"oauth_token\";");
            headers.AppendLine();
            headers.AppendLine(Settings1.Default.token);
            headers.AppendLine(header);

            headers.AppendLine("Content-Disposition: form-data; name=\"oauth_secret\";");
            headers.AppendLine();
            headers.AppendLine(Settings1.Default.secretToken);
            headers.AppendLine(header);

          
            */





            if (!string.IsNullOrEmpty(message.Trim()))
            {
                headers.AppendLine("Content-Disposition: form-data; name=\"message\";");
                headers.AppendLine();
                headers.AppendLine(message);
            }


            headers.AppendLine(footer);

            byte[] contents = Encoding.GetEncoding("iso-8859-1").GetBytes(headers.ToString());
            uploadRequest.ContentLength = contents.Length;

            using (Stream requestStream = uploadRequest.GetRequestStream())
            {
                requestStream.Write(contents, 0, contents.Length);
                requestStream.Flush();
                requestStream.Close();

                using (WebResponse response = uploadRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        resonseText = reader.ReadToEnd();
                    }
                }
            }

            return resonseText;
        }  

        private void button8_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            filename = openFileDialog1.FileName;
            pictureText.Text = filename;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (tweetText.Text.Length > 0)
            {
                tweet.Enabled = true;
                uploadAndTweet.Enabled = true;
            }
            else
            {
                tweet.Enabled = false;
                uploadAndTweet.Enabled = false;
            }
            int count;
            count = tweetText.Text.Length;
            label26.Text = count.ToString();




        }

        private void Form1_Load(object sender, EventArgs e)
        {

            tweet.Enabled = false;
            uploadAndTweet.Enabled = false;
            if (Settings1.Default.consumerKey.Length != 0)
            {
                consumerKeyText.Text = Settings1.Default.consumerKey;
                consumerSecretText.Text = Settings1.Default.consumerSecret;
                consumerSecretText.ReadOnly = true;
                consumerKeyText.ReadOnly = true;
                appRegisteration.Enabled = false;
            }

            if (Settings1.Default.pin.Length != 0)
            {
                pinText.Text = Settings1.Default.pin;
                pinText.ReadOnly = true;
                authenticationButton.Enabled = false;
            }

            
        }

        public String[] getProfilePicture(String profileURL, bool list)
        {
            String[] myids = new String[10];
            String twitterGetURL = "";
            if (list == true)
                twitterGetURL = "http://api.twitter.com/1/friends/ids.xml?id=";
            else
                twitterGetURL = "http://api.twitter.com/1/followers/ids.xml?id=";

            String friendsXML = twitterGetURL + userHandle.Text;
            XmlDocument mydoc = new XmlDocument();
            try
            {
                mydoc.Load(friendsXML);
            }
            catch (Exception ex)
            {

            }
            XmlNodeList id = mydoc.GetElementsByTagName("id");

            for (int i = 0; i < myids.Length; i++)
            {
                myids[i] = id[i].InnerText;

            }


            String[] imageURLS = new String[10];


            for (int i = 0; i < imageURLS.Length; i++)
            {
                String userinfoURL = "http://api.twitter.com/1/users/show.xml?id=";
                userinfoURL = userinfoURL + myids[i].ToString();
                mydoc.Load(userinfoURL);
                XmlNodeList imageurl = mydoc.GetElementsByTagName("profile_image_url");
                XmlNodeList screen_name = mydoc.GetElementsByTagName("screen_name");

                imageURLS[i] = imageurl[0].InnerText;
                frndNames[i] = screen_name[0].InnerText;

            }

            return imageURLS;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            getprofile(userHandle.Text);
            Cursor.Current = Cursors.Default;
        }

        private void getprofile(String userHandle)
        {

            XmlDocument mydoc = new XmlDocument();
            String twitterUserTimeline = "http://api.twitter.com/1/users/show.xml?id=";
            String twitterHandle = userHandle;
            String profileXML = twitterUserTimeline + twitterHandle;
            mydoc.Load(profileXML);

            XmlNodeList name = mydoc.GetElementsByTagName("name");
            XmlNodeList location = mydoc.GetElementsByTagName("location");
            XmlNodeList bio = mydoc.GetElementsByTagName("description");
            XmlNodeList site = mydoc.GetElementsByTagName("url");
            XmlNodeList followers = mydoc.GetElementsByTagName("friends_count");
            XmlNodeList following = mydoc.GetElementsByTagName("followers_count");
            XmlNodeList created = mydoc.GetElementsByTagName("created_at");
            XmlNodeList statuses = mydoc.GetElementsByTagName("statuses_count");
            XmlNodeList profileimageurl = mydoc.GetElementsByTagName("profile_image_url");
            XmlNodeList geotag = mydoc.GetElementsByTagName("geo");
            XmlNodeList verification = mydoc.GetElementsByTagName("verified");
            XmlNodeList userID = mydoc.GetElementsByTagName("id");
            XmlNodeList profileCreatedOn = mydoc.GetElementsByTagName("created_at");
            XmlNodeList timezone = mydoc.GetElementsByTagName("time_zone");
            XmlNodeList statusText = mydoc.GetElementsByTagName("text");

            userName.Text = name[0].InnerText;
            userLocation.Text = location[0].InnerText;
            userWebsite.Text = site[0].InnerText;
            textBox2.Text = bio[0].InnerText;
            userTwitterID.Text = userID[0].InnerText;
            userFrndsCount.Text = followers[0].InnerText;
            userFllwrsCount.Text = following[0].InnerText;
            userStatusCount.Text = statuses[0].InnerText;
            userVerified.Text = verification[0].InnerText;
            userTimeZone.Text = timezone[0].InnerText;


            String since = profileCreatedOn[0].InnerText;

            String month = since[4].ToString() + since[5].ToString() + since[6].ToString();
            String date = since[8].ToString() + since[9].ToString();
            String year = since[26].ToString() + since[27].ToString() + since[28].ToString() + since[29].ToString();

            memberSince.Text = (month + ", " + date + ", " + year);

            if (statusText[0].InnerText.Length != 0)
                recentStatusText.Text = statusText[0].InnerText;
            pictureBox1.ImageLocation = profileimageurl[0].InnerText;

            if (geotag[0].InnerText.Length == 0)
                userGeo.Text = "Not Enabled";
            else
                userGeo.Text = "Enabled";



        }

        private void loadFollowers()
        {
            String[] followerimagelocation = getProfilePicture(userHandle.Text, false);

            pictureBox12.ImageLocation = followerimagelocation[0].ToString();
            pictureBox13.ImageLocation = followerimagelocation[1].ToString();
            pictureBox14.ImageLocation = followerimagelocation[2].ToString();
            pictureBox15.ImageLocation = followerimagelocation[3].ToString();
            pictureBox16.ImageLocation = followerimagelocation[4].ToString();
            pictureBox17.ImageLocation = followerimagelocation[5].ToString();
            pictureBox18.ImageLocation = followerimagelocation[6].ToString();
            pictureBox19.ImageLocation = followerimagelocation[7].ToString();
            pictureBox20.ImageLocation = followerimagelocation[8].ToString();
            pictureBox21.ImageLocation = followerimagelocation[9].ToString();
        }  // Call cautiously as it may take more requests (Twitter reate limit is 150/hour)

        private void loadFriends()
        {
            String[] frndImagelocations = getProfilePicture(userHandle.Text, true);

            pictureBox2.ImageLocation = frndImagelocations[0].ToString();
            pictureBox3.ImageLocation = frndImagelocations[1].ToString();
            pictureBox4.ImageLocation = frndImagelocations[2].ToString();
            pictureBox5.ImageLocation = frndImagelocations[3].ToString();
            pictureBox6.ImageLocation = frndImagelocations[4].ToString();
            pictureBox7.ImageLocation = frndImagelocations[5].ToString();
            pictureBox8.ImageLocation = frndImagelocations[6].ToString();
            pictureBox9.ImageLocation = frndImagelocations[7].ToString();
            pictureBox10.ImageLocation = frndImagelocations[8].ToString();
            pictureBox11.ImageLocation = frndImagelocations[9].ToString();
        }   // Call cautiously as it may take more requests (Twitter reate limit is 150/hour)

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://twitter.com/" + userHandle.Text);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            getprofile(frndNames[0]);

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            loadFriends();
            Cursor.Current = Cursors.Default;
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            loadFollowers();
            Cursor.Current = Cursors.Default;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 myaboutbox = new AboutBox1();
            myaboutbox.ShowDialog(); ;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }



    }
}
