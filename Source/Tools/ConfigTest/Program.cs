using System;
using GSF.Configuration;

namespace ConfigTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings["systemSettings"];
            CategorizedSettingsElementCollection examples = config.Settings["examples"];

            settings.Add("connectionString", "abc=123", "My device connection string");
            settings.Add("property2", "clever", "Smack, smack!");
            examples.Add("newProperty", "test", "New property description");
            examples.Add("areWeThereYet", "never", "Why?");

            string connectionString = settings["connectionString"].ValueAs("default");
            string property2 = settings["property2"].ValueAs("default2");
            string newProperty = examples["newProperty"].ValueAs("default3");
            string areWeThereYet = examples["areWeThereYet"].ValueAs("default4");

            Console.WriteLine();
            Console.WriteLine("systemSettings.connectionString setting = " + connectionString);
            Console.WriteLine("systemSettings.property2 setting = " + property2);
            Console.WriteLine("examples.newProperty = " + newProperty);
            Console.WriteLine("examples.areWeThereYet = " + areWeThereYet);

            Console.WriteLine();
            Console.WriteLine("Updating setting to new random value...");
            settings["connectionString", true].Value = Guid.NewGuid().ToString();

            Console.WriteLine();
            Console.WriteLine("New setting: " + settings["connectionString"].Value);

            Console.WriteLine();
            Console.WriteLine("Saving updated configuration settings...");

            config.Save();

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
