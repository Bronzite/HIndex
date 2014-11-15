using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;

namespace HIndex
{
    class Program
    {
        static void Main(string[] args)
        {
            WebClient wc = new WebClient();
            string sUserName="";
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: HIndex [BGG Username]");
                return;
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                    sUserName = string.Format("{0}{1}%20", sUserName, args[i]);
                sUserName = sUserName.Substring(0, sUserName.Length - 3);
            }
            //Console.WriteLine("User: {0}", sUserName);
            //v1 API
            //string sRequestString = string.Format("http://www.boardgamegeek.com/xmlapi/collection/{0}", sUserName);

            //v2 API
            string sRequestString = string.Format("http://www.boardgamegeek.com/xmlapi2/plays?username={0}&subtype=boargame,boardgameexpansion", sUserName);

            

            Dictionary<string, int> dicPlays = new Dictionary<string, int>();
            Dictionary<string, string> dicNames = new Dictionary<string, string>();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.XmlResolver = null;
            //settings.ProhibitDtd = false;
            settings.DtdProcessing = DtdProcessing.Parse;
            int iTotalPlays = int.MaxValue;
            int iRecordedPlays = 0;
            int iPage = 1;
            while (iRecordedPlays < iTotalPlays)
            {
                sRequestString = string.Format("http://www.boardgamegeek.com/xmlapi2/plays?username={0}&page={1}&subtype=boardgame", sUserName, iPage++);
                string sResponse = wc.DownloadString(sRequestString);
                
                XmlDocument xdoc = new XmlDocument();

                System.IO.StringReader sr = new System.IO.StringReader(sResponse);
                XmlReader reader = XmlReader.Create(sr, settings);

                xdoc.Load(reader);

                XmlNodeList xmlMessages = xdoc.SelectNodes("//message");

                if (xmlMessages.Count > 0)
                {
                    foreach (XmlNode curMessage in xmlMessages)
                    {
                        Console.WriteLine(curMessage.InnerText);

                    }

                    return;
                }

                XmlNode xmlPlaysNode = xdoc.SelectSingleNode("//plays");

                iTotalPlays = int.Parse(xmlPlaysNode.Attributes["total"].Value);


                XmlNodeList plays = xmlPlaysNode.SelectNodes("//play");
                foreach (XmlNode playNode in plays)
                {
                    int iQuantity = int.Parse(playNode.Attributes["quantity"].Value);
                    XmlNodeList items = playNode.SelectNodes("item");
                    //Console.WriteLine("Loading {0} items", items.Count);
                    foreach (XmlNode curNode in items)
                    {
                        string sObjectName = curNode.Attributes["name"].Value;
                        iRecordedPlays++;
                        if (dicPlays.ContainsKey(curNode.Attributes["objectid"].Value))
                        {
                            dicPlays[curNode.Attributes["objectid"].Value]+= iQuantity;
                        }
                        else
                        {
                            dicPlays.Add(curNode.Attributes["objectid"].Value, iQuantity);
                            dicNames.Add(curNode.Attributes["objectid"].Value, sObjectName);
                        }
                    }
                }
            }
            List<KeyValuePair<string, int>> lstHIndex = dicPlays.ToList();
            lstHIndex.Sort((firstPair, nextPair) => { return firstPair.Value.CompareTo(nextPair.Value); });
            /*
            foreach(KeyValuePair<string,int> curVal in lstHIndex)
            {
                Console.WriteLine("{0}: {1}",dicNames[curVal.Key],curVal.Value);
            }
            */
            int iHIndex = 0;
            int iLowThreshold = int.MaxValue;
            int iGameCount = 0;
            for(int i=lstHIndex.Count;i>0;i--)
            {
                if (lstHIndex[i - 1].Value > iGameCount)
                {
                    iHIndex = ++iGameCount;
                    //Console.WriteLine("{0} is an H-Index Game with {1} plays.", dicNames[lstHIndex[i - 1].Key], lstHIndex[i - 1].Value);
                }
                else
                {
                    if (iLowThreshold == int.MaxValue && lstHIndex[i-1].Value > 0)
                    {
                        iLowThreshold = lstHIndex[i-1].Value;
                        break;
                    }
                }
            }

            Console.WriteLine("H-Index: {0}",iHIndex);
            Console.WriteLine("H-Active:");
            int iPrintedGames = 0;
            foreach(KeyValuePair<string,int> kvp in lstHIndex)
            {
                if(kvp.Value <= iHIndex && kvp.Value >= iLowThreshold)
                {
                    Console.WriteLine("{0}", dicNames[ kvp.Key]);
                    
                }
            }

            //Console.ReadLine();
        }
    }
}
