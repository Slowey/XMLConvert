using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class XMLConverter : MonoBehaviour {

    public InputField m_input = null;
    public void M_ConvertToXML()
    {
        //Grab the text from input
        string fileSource = m_input.text;
        //open, read and save the lines of the document
        string[] lines = File.ReadAllLines(fileSource);

        //declare datastructure that will hold all the information that will be printed to our XML document (IO is expensive. Do 1 big write instead of many small)
        List<string> outputLines = new List<string>();
        //Start populating our list. People is a header and will always be first no matter what
        outputLines.Add("<people>");

        //inizialize some variables we need to store some information and to handle the codeflow.
        int lenght = lines.Length;
        bool isHandlingFamily = false;
        Person t_person = null;
        Family t_family = null;
        //loop over the lines we read
        for (int i = 0; i < lenght; i++)
        {
            //Take care of Person
            if (lines[i].StartsWith("P"))
            {
                //If we are currently handling familymembers and end up here. We know that the family member is done and we can add that item.
                if (isHandlingFamily)
                {
                    t_person.family.Add(t_family);
                    isHandlingFamily = false;
                }
                //null check to avoid adding the first empty person to the list
                if (t_person != null)
                {
                    //we hit P after adding stuff to our temp person. this means the person is done. Lets add it.
                    t_person.M_AddMeToList(ref outputLines);
                }
                t_person = new Person();

                //Use split to grab the nessecary information. Have to cast to charArray apparently.
                string[] splitLine = lines[i].Split("|".ToCharArray());
                //Save the names in our person class.
                //This only works if there is always a first and a last name. With the separator | between them. For this assignment i make that assumption. otherwise need more checks
                t_person.name.firstName = splitLine[1];
                t_person.name.lastName = splitLine[2];
                //P is finished. Fall through for next line
            }
            else if (lines[i].StartsWith("T"))
            {
                //basically the same as P but for telephone. However, both a person and a family can have a number. Check our bool for which struct should save the text.
                string[] splitLine = lines[i].Split("|".ToCharArray());
                if (!isHandlingFamily)
                {
                    t_person.number.mobile = splitLine[1];
                    t_person.number.landLine = splitLine[2];
                }
                else
                {
                    t_family.number.mobile = splitLine[1];
                    t_family.number.landLine = splitLine[2];
                }
            }
            else if (lines[i].StartsWith("A"))
            {
                // same as phone
                string[] splitLine = lines[i].Split("|".ToCharArray());
                if (!isHandlingFamily)
                {
                    t_person.adress.adress = splitLine[1];
                    t_person.adress.city = splitLine[2];
                    if (splitLine.Length >3)
                    {
                        t_person.adress.postCode = splitLine[3];
                    }
                }
                else
                {
                    t_family.adress.adress = splitLine[1];
                    t_family.adress.city = splitLine[2];
                    t_family.adress.postCode = splitLine[3];
                }
            }
            else if (lines[i].StartsWith("F"))
            {
                // family works almost the same as person. Here we flip our bool for isHandlingFamily
                string[] splitLine = lines[i].Split("|".ToCharArray());
                if (t_family != null)
                {
                    t_person.family.Add(t_family);
                }
                t_family = new Family();
                t_family.name = splitLine[1];
                t_family.birthYear = splitLine[2];
                isHandlingFamily = true;

            }

        }

        //We have iterated every line. Add the last person
        if (t_person != null)
        {
            t_person.M_AddMeToList(ref outputLines);
        }

        //close the people class
        outputLines.Add("</people>");

        //time to write to a new file. Send the source of the file. That way we can put our output next to the input. Could add an option to decide outputdirectory
        WriteToFile(outputLines, fileSource);
    }

    private void WriteToFile(List<string> p_outputLines, string sourcePath)
    {
        ///Im just gonna write to the sourcePath and make an XML document instead. 
        string[] splitPathBeforeTheDot = sourcePath.Split(".".ToCharArray());
        File.WriteAllLines(splitPathBeforeTheDot[0] + ".xml", p_outputLines.ToArray());
    }



    /// <summary>
    /// Down here are the structs and classes to make the above code easier to understand and read.
    /// </summary>
    public struct PhoneNumber
    {
        public string mobile;
        public string landLine;
    }
    public struct Adress
    {
        public string adress;
        public string city;
        public string postCode;
    }

    private class Family
    {
        public string name;
        public string birthYear;
        public PhoneNumber number;
        public Adress adress;
    }

    private class Person
    {
        public Name name;
        public PhoneNumber number;
        public Adress adress;
        public List<Family> family;
        public Person()
        {
            family = new List<Family>();
        }
        public struct Name
        {
            public string firstName;
            public string lastName;
        }


        public void M_AddMeToList(ref List<string> outputLines)
        {
            //Basically hard coded stringadds to convert to the new format.
            outputLines.Add(" " + "<person>");
            
            outputLines.Add("  " + "<firstname>" + name.firstName + "</firstname>");
            outputLines.Add("  " + "<lastname>" + name.lastName + "</lastname>");

            outputLines.Add("  " + "<adress>");
            outputLines.Add("   " + "<street>" + adress.adress + "</street>");
            outputLines.Add("   " + "<city>" + adress.city + "</city>");
            outputLines.Add("   " + "<postcode>" + adress.postCode + "</postcode>");
            outputLines.Add("  " + "</adress>");

            outputLines.Add("  " + "<phone>");
            outputLines.Add("   " + "<mobile>" + number.mobile + "</mobile>");
            outputLines.Add("   " + "<landLine>" + number.landLine+ "</landLine>");
            outputLines.Add("  " + "</phone>");
            int famCount = family.Count;
            for (int i = 0; i < famCount; i++)
            {
                outputLines.Add("  " + "<family>");
                outputLines.Add("   " + "<name>" + family[i].name + "</name>");
                outputLines.Add("    " + "<born>" + family[i].birthYear+ "</born>");

                outputLines.Add("    " + "<adress>");
                outputLines.Add("     " + "<street>" + family[i].adress.adress+ "</street>");
                outputLines.Add("     " + "<city>" + family[i].adress.city + "</city>");
                outputLines.Add("     " + "<postcode>" + family[i].adress.postCode + "</postcode>");
                outputLines.Add("    " + "</adress>");
                
                outputLines.Add("    " + "<phone>");
                outputLines.Add("     " + "<mobile>" + family[i].number.mobile + "</mobile>");
                outputLines.Add("     " + "<landLine>" + family[i].number.landLine + "</landLine>");
                outputLines.Add("    " + "</phone>");

                outputLines.Add("  " + "</family>");
            }
            outputLines.Add("  " + "</person>");


        }
    }

}
