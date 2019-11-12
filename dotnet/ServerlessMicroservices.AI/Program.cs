using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServerlessMicroservices.Models;
using ServerlessMicroservices.Shared.Helpers;

namespace ServerlessMicroservices.AI
{
    class Program
    {
        static string cosmosDbDatabaseName = "RideShare";
        static string cosmosDbRideShareMainCollectionName = "Main";
        static int cosmosDbThroughput = 400;
        static Random random = new Random();

        static void Main(string[] args)
        {
            var app = new CommandLineApplication {FullName = "RideShare AI Configuration Commands"};
            app.HelpOption("--help");

            // `seedMlTrainingData` Seed ML Training Data Command
            var cosmosDbApiKeyOption = app.Option("-a|--cosmosdbapikey", "Set Cosmos DB API Key", CommandOptionType.SingleValue, true);
            var cosmosDbEndpointUriOption = app.Option("-e|--cosmosdbendpointurl", "Set Cosmos DB Endpoint Uri", CommandOptionType.SingleValue, true);
            var averageRecordsPerDay = app.Option("-r|--averagerecordsperday", "Set the average records per day to generate", CommandOptionType.SingleValue, true);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .Build();

            app.Command("seedMlTrainingData", cmd =>
            {
                cmd.Description = "Test Demo Trips";
                cmd.HelpOption("--help");

                cmd.OnExecute(async () =>
                {
                    var cosmosDbApiKey = cosmosDbApiKeyOption.Value();
                    var cosmosDbEndpointUri = cosmosDbEndpointUriOption.Value();
                    int.TryParse(averageRecordsPerDay.Value(), out var recordsPerDay);

                    if (string.IsNullOrEmpty(cosmosDbApiKey) || !cosmosDbApiKeyOption.HasValue() ||
                        string.IsNullOrEmpty(cosmosDbEndpointUri) || !cosmosDbEndpointUriOption.HasValue())
                    {
                        Console.WriteLine("Required options: cosmosDbApiKey, cosmosDbEndpointUri");
                        return 0;
                    }

                    Console.WriteLine($"Generating ML training data....");
                    await SeedMlTrainingData(recordsPerDay);
                    Console.WriteLine($"Generation completed");

                    return 0;
                });
            });

            app.Execute(args);
        }

        static async Task SeedMlTrainingData(int averageRecordsPerDay = 100)
        {
            #region Seed data
            // Define seed parameters and collections:
            var drivers = new List<DriverItem>()
            {
                new DriverItem() { Code = "AA100", FirstName = "James", LastName = "Beaky", Latitude = 31.7157, Longitude = 117.1611, Car = new  CarItem () { DriverCode = "AA100", Make = "BMW", Model = "735", Year = "2015", Color = "Silver", LicensePlate = "CA-91099"} },
                new DriverItem() { Code = "AA110", FirstName = "Rod", LastName = "Snow", Latitude = 34.0552, Longitude = 118.2437, Car = new  CarItem () { DriverCode = "AA110", Make = "Toyota", Model = "Camry", Year = "2013", Color = "Gray", LicensePlate = "CA-78209"} },
                new DriverItem() { Code = "AA120", FirstName = "Sam", LastName = "Imada", Latitude = 37.7749, Longitude = 122.4194, Car = new  CarItem () { DriverCode = "AA120", Make = "Honda", Model = "Accord", Year = "2017", Color = "Black", LicensePlate = "CA-76215"} },
                new DriverItem() { Code = "YV764",FirstName = "Alfreda",LastName = "Cayzer",Latitude = 1.393278,Longitude = 124.684196,Car = new  CarItem () { DriverCode = "YV764",Make = "Mercedes-Benz",Model = "S-Class", Year = "1991", Color = "Violet",LicensePlate = "BGN-4349"} },
                new DriverItem() { Code = "BO095",FirstName = "Corry",LastName = "Litster",Latitude = -6.5643956,Longitude = 106.2522143,Car = new  CarItem () { DriverCode = "BO095",Make = "Ford",Model = "E150", Year = "2005", Color = "Puce",LicensePlate = "CBW-9255"} },
                new DriverItem() { Code = "FP686",FirstName = "Lilas",LastName = "Longfellow",Latitude = 5.000528,Longitude = -74.339439,Car = new  CarItem () { DriverCode = "FP686",Make = "Volvo",Model = "XC70", Year = "2012", Color = "Orange",LicensePlate = "OQI-7961"} },
                new DriverItem() { Code = "VX454",FirstName = "Antoinette",LastName = "Whithalgh",Latitude = -7.3279,Longitude = 108.6833,Car = new  CarItem () { DriverCode = "VX454",Make = "Chrysler",Model = "LeBaron", Year = "1993", Color = "Fuscia",LicensePlate = "RDW-2056"} },
                new DriverItem() { Code = "ID549",FirstName = "Barbaraanne",LastName = "Gaylor",Latitude = 46.1679323,Longitude = 14.5945759,Car = new  CarItem () { DriverCode = "ID549",Make = "Mercury",Model = "Grand Marquis", Year = "1991", Color = "Fuscia",LicensePlate = "WTB-8047"} },
                new DriverItem() { Code = "WF799",FirstName = "Bryan",LastName = "Auston",Latitude = 38.235241,Longitude = 125.750504,Car = new  CarItem () { DriverCode = "WF799",Make = "Audi",Model = "4000", Year = "1987", Color = "Blue",LicensePlate = "PUF-6882"} },
                new DriverItem() { Code = "JD342",FirstName = "Neville",LastName = "Radborne",Latitude = 38.914003,Longitude = 121.614682,Car = new  CarItem () { DriverCode = "JD342",Make = "Ford",Model = "Windstar", Year = "2002", Color = "Maroon",LicensePlate = "LDZ-0465"} },
                new DriverItem() { Code = "VV013",FirstName = "Nehemiah",LastName = "Staunton",Latitude = -15.6442505,Longitude = -74.0784785,Car = new  CarItem () { DriverCode = "VV013",Make = "Dodge",Model = "Ram Wagon B250", Year = "1994", Color = "Goldenrod",LicensePlate = "EQZ-7238"} },
                new DriverItem() { Code = "VT709",FirstName = "Lisle",LastName = "Ingerman",Latitude = 41.5051316,Longitude = -8.3877778,Car = new  CarItem () { DriverCode = "VT709",Make = "Buick",Model = "LeSabre", Year = "1987", Color = "Turquoise",LicensePlate = "ZCC-9192"} },
                new DriverItem() { Code = "AA024",FirstName = "Ardyce",LastName = "Forsbey",Latitude = 51.5969624,Longitude = 54.1584621,Car = new  CarItem () { DriverCode = "AA024",Make = "Dodge",Model = "Grand Caravan", Year = "2011", Color = "Mauv",LicensePlate = "NLV-5817"} },
                new DriverItem() { Code = "BZ697",FirstName = "Ambrose",LastName = "Underdown",Latitude = 55.8024235,Longitude = 37.4219388,Car = new  CarItem () { DriverCode = "BZ697",Make = "Infiniti",Model = "Q", Year = "1994", Color = "Aquamarine",LicensePlate = "QIV-1487"} },
                new DriverItem() { Code = "LD866",FirstName = "Nick",LastName = "Foffano",Latitude = 48.8809869,Longitude = 2.2760839,Car = new  CarItem () { DriverCode = "LD866",Make = "Toyota",Model = "4Runner", Year = "2005", Color = "Mauv",LicensePlate = "TNX-5715"} },
                new DriverItem() { Code = "HS055",FirstName = "Pepi",LastName = "Matiebe",Latitude = 21.9440983,Longitude = -78.4325615,Car = new  CarItem () { DriverCode = "HS055",Make = "Toyota",Model = "Avalon", Year = "2006", Color = "Teal",LicensePlate = "AER-8692"} },
                new DriverItem() { Code = "CG010",FirstName = "Rudolf",LastName = "Bowdidge",Latitude = -19.1054879,Longitude = 33.4607744,Car = new  CarItem () { DriverCode = "CG010",Make = "Mercury",Model = "Grand Marquis", Year = "2007", Color = "Green",LicensePlate = "RLR-2551"} },
                new DriverItem() { Code = "PE811",FirstName = "Sid",LastName = "Donoghue",Latitude = -8.536156,Longitude = -77.360077,Car = new  CarItem () { DriverCode = "PE811",Make = "Ford",Model = "Ranger", Year = "1987", Color = "Goldenrod",LicensePlate = "PTH-1215"} },
                new DriverItem() { Code = "PJ846",FirstName = "Lowe",LastName = "Keast",Latitude = 49.1845517,Longitude = -0.3553901,Car = new  CarItem () { DriverCode = "PJ846",Make = "Toyota",Model = "Avalon", Year = "2003", Color = "Green",LicensePlate = "BDG-2791"} },
                new DriverItem() { Code = "XX366",FirstName = "Natalya",LastName = "Balling",Latitude = 50.756497,Longitude = 21.8951181,Car = new  CarItem () { DriverCode = "XX366",Make = "Jeep",Model = "Cherokee", Year = "1999", Color = "Teal",LicensePlate = "LHG-7018"} },
                new DriverItem() { Code = "TI959",FirstName = "Langston",LastName = "Gerriessen",Latitude = -7.3742259,Longitude = 108.4454089,Car = new  CarItem () { DriverCode = "TI959",Make = "Ford",Model = "Escort", Year = "1999", Color = "Blue",LicensePlate = "JFG-9498"} },
                new DriverItem() { Code = "NS975",FirstName = "Port",LastName = "Petroselli",Latitude = -27.2369945,Longitude = 28.8476254,Car = new  CarItem () { DriverCode = "NS975",Make = "Ferrari",Model = "458 Italia", Year = "2010", Color = "Teal",LicensePlate = "QVC-4705"} },
                new DriverItem() { Code = "HH475",FirstName = "Christyna",LastName = "Darwin",Latitude = -8.271291,Longitude = 124.725037,Car = new  CarItem () { DriverCode = "HH475",Make = "Mazda",Model = "626", Year = "1986", Color = "Fuscia",LicensePlate = "ZSY-2776"} },
                new DriverItem() { Code = "BP540",FirstName = "Liam",LastName = "Hillburn",Latitude = -7.6426348,Longitude = 112.7032945,Car = new  CarItem () { DriverCode = "BP540",Make = "Dodge",Model = "Grand Caravan", Year = "2005", Color = "Khaki",LicensePlate = "DXW-1248"} },
                new DriverItem() { Code = "CA656",FirstName = "Stacy",LastName = "Shalcras",Latitude = 38.7409837,Longitude = -9.3605586,Car = new  CarItem () { DriverCode = "CA656",Make = "Ford",Model = "Explorer", Year = "2007", Color = "Khaki",LicensePlate = "TOG-1757"} },
                new DriverItem() { Code = "UG981",FirstName = "Aliza",LastName = "Duval",Latitude = -7.0206795,Longitude = 112.3963638,Car = new  CarItem () { DriverCode = "UG981",Make = "Jensen",Model = "Interceptor", Year = "1967", Color = "Turquoise",LicensePlate = "SOP-4973"} },
                new DriverItem() { Code = "UC733",FirstName = "Lorena",LastName = "Brimmacombe",Latitude = 32.023273,Longitude = 108.400024,Car = new  CarItem () { DriverCode = "UC733",Make = "Scion",Model = "xB", Year = "2006", Color = "Pink",LicensePlate = "SJC-1456"} },
                new DriverItem() { Code = "HR132",FirstName = "Thia",LastName = "Troy",Latitude = 22.358416,Longitude = 114.128354,Car = new  CarItem () { DriverCode = "HR132",Make = "Saturn",Model = "Ion", Year = "2006", Color = "Purple",LicensePlate = "GVZ-2463"} },
                new DriverItem() { Code = "YT414",FirstName = "Anson",LastName = "Darnborough",Latitude = -6.5619262,Longitude = 106.3786028,Car = new  CarItem () { DriverCode = "YT414",Make = "Isuzu",Model = "Rodeo", Year = "1998", Color = "Mauv",LicensePlate = "SZE-8139"} },
                new DriverItem() { Code = "FX106",FirstName = "Bernardina",LastName = "Das",Latitude = 41.32042,Longitude = 22.53047,Car = new  CarItem () { DriverCode = "FX106",Make = "Suzuki",Model = "Daewoo Magnus", Year = "2005", Color = "Khaki",LicensePlate = "SJY-5959"} },
                new DriverItem() { Code = "GE143",FirstName = "Elisabetta",LastName = "Bowbrick",Latitude = 51.904635,Longitude = -8.958931,Car = new  CarItem () { DriverCode = "GE143",Make = "Toyota",Model = "Yaris", Year = "2006", Color = "Blue",LicensePlate = "OYU-3278"} },
                new DriverItem() { Code = "TL958",FirstName = "Hillier",LastName = "Macy",Latitude = -12.9361186,Longitude = 45.1501554,Car = new  CarItem () { DriverCode = "TL958",Make = "Honda",Model = "Civic Si", Year = "2003", Color = "Green",LicensePlate = "RHY-5182"} },
                new DriverItem() { Code = "SD251",FirstName = "Celka",LastName = "Sextie",Latitude = -4.3253802,Longitude = 20.5870955,Car = new  CarItem () { DriverCode = "SD251",Make = "Ford",Model = "Aerostar", Year = "1990", Color = "Purple",LicensePlate = "ESG-1320"} },
                new DriverItem() { Code = "UZ782",FirstName = "Win",LastName = "Crellim",Latitude = 8.022507,Longitude = 123.48051,Car = new  CarItem () { DriverCode = "UZ782",Make = "Lincoln",Model = "Town Car", Year = "2004", Color = "Pink",LicensePlate = "PIG-4917"} },
                new DriverItem() { Code = "YV241",FirstName = "Joel",LastName = "Luttger",Latitude = 40.4140806,Longitude = -8.738467,Car = new  CarItem () { DriverCode = "YV241",Make = "Nissan",Model = "Murano", Year = "2004", Color = "Mauv",LicensePlate = "SPR-4108"} },
                new DriverItem() { Code = "KY108",FirstName = "Yulma",LastName = "Danter",Latitude = 18.3721392,Longitude = 121.5111279,Car = new  CarItem () { DriverCode = "KY108",Make = "Pontiac",Model = "Parisienne", Year = "1986", Color = "Purple",LicensePlate = "XTL-0685"} },
                new DriverItem() { Code = "MV548",FirstName = "Micah",LastName = "Blaby",Latitude = 25.6564843,Longitude = -100.3694401,Car = new  CarItem () { DriverCode = "MV548",Make = "Saturn",Model = "Astra", Year = "2009", Color = "Orange",LicensePlate = "JYM-7400"} },
                new DriverItem() { Code = "CA447",FirstName = "Sergio",LastName = "Durtnell",Latitude = 6.8322014,Longitude = 3.6319131,Car = new  CarItem () { DriverCode = "CA447",Make = "Mitsubishi",Model = "Montero Sport", Year = "1998", Color = "Khaki",LicensePlate = "LJV-4876"} },
                new DriverItem() { Code = "HZ513",FirstName = "Allard",LastName = "Kitteman",Latitude = -8.3743886,Longitude = 113.6383187,Car = new  CarItem () { DriverCode = "HZ513",Make = "Dodge",Model = "Durango", Year = "2002", Color = "Purple",LicensePlate = "SLP-2226"} },
                new DriverItem() { Code = "JK973",FirstName = "Mason",LastName = "Waith",Latitude = 27.775323,Longitude = 108.796556,Car = new  CarItem () { DriverCode = "JK973",Make = "GMC",Model = "3500 Club Coupe", Year = "1995", Color = "Yellow",LicensePlate = "PGW-7075"} },
                new DriverItem() { Code = "GL995",FirstName = "Mead",LastName = "Reggiani",Latitude = 14.6507289,Longitude = -90.5128018,Car = new  CarItem () { DriverCode = "GL995",Make = "Chevrolet",Model = "3500", Year = "2000", Color = "Fuscia",LicensePlate = "JDV-4153"} },
                new DriverItem() { Code = "UJ165",FirstName = "Gustave",LastName = "Guichard",Latitude = 38.4581162,Longitude = -7.7521244,Car = new  CarItem () { DriverCode = "UJ165",Make = "Honda",Model = "Ridgeline", Year = "2006", Color = "Teal",LicensePlate = "YPK-0774"} },
                new DriverItem() { Code = "HJ478",FirstName = "Glad",LastName = "Clemens",Latitude = 53.695696,Longitude = 54.276083,Car = new  CarItem () { DriverCode = "HJ478",Make = "Honda",Model = "Civic", Year = "2005", Color = "Pink",LicensePlate = "DEO-0515"} },
                new DriverItem() { Code = "YT157",FirstName = "Serena",LastName = "Pes",Latitude = 49.03353,Longitude = 2.0768291,Car = new  CarItem () { DriverCode = "YT157",Make = "Porsche",Model = "911", Year = "2000", Color = "Yellow",LicensePlate = "KDW-9300"} },
                new DriverItem() { Code = "FV388",FirstName = "Loutitia",LastName = "Robardley",Latitude = 41.6089452,Longitude = 20.006257,Car = new  CarItem () { DriverCode = "FV388",Make = "Suzuki",Model = "Kizashi", Year = "2011", Color = "Red",LicensePlate = "CJM-3291"} },
                new DriverItem() { Code = "CE038",FirstName = "Celestine",LastName = "Peplaw",Latitude = 44.9532555,Longitude = 2.8218035,Car = new  CarItem () { DriverCode = "CE038",Make = "Hyundai",Model = "Elantra", Year = "2009", Color = "Orange",LicensePlate = "AXD-1615"} },
                new DriverItem() { Code = "HE716",FirstName = "Aurore",LastName = "Noice",Latitude = 44.346784,Longitude = 5.8855505,Car = new  CarItem () { DriverCode = "HE716",Make = "Jaguar",Model = "S-Type", Year = "2000", Color = "Orange",LicensePlate = "INL-6917"} },
                new DriverItem() { Code = "KG191",FirstName = "Missy",LastName = "Viant",Latitude = 53.7090279,Longitude = 18.1802823,Car = new  CarItem () { DriverCode = "KG191",Make = "Mercedes-Benz",Model = "CL-Class", Year = "1999", Color = "Yellow",LicensePlate = "TVX-4694"} },
                new DriverItem() { Code = "JA032",FirstName = "Gabbie",LastName = "Boyton",Latitude = 61.0162875,Longitude = 35.485856,Car = new  CarItem () { DriverCode = "JA032",Make = "GMC",Model = "Sonoma", Year = "2002", Color = "Blue",LicensePlate = "VGV-7921"} },
                new DriverItem() { Code = "FR196",FirstName = "Hi",LastName = "Sallings",Latitude = 44.6380305,Longitude = 21.6773447,Car = new  CarItem () { DriverCode = "FR196",Make = "Land Rover",Model = "LR4", Year = "2011", Color = "Violet",LicensePlate = "QRP-6508"} },
                new DriverItem() { Code = "ZT452",FirstName = "Ivie",LastName = "Boerderman",Latitude = 50.0534923,Longitude = 5.9177071,Car = new  CarItem () { DriverCode = "ZT452",Make = "Chrysler",Model = "300", Year = "2009", Color = "Green",LicensePlate = "RXW-7970"} },
                new DriverItem() { Code = "CG845",FirstName = "Mendy",LastName = "Marklew",Latitude = 29.338873,Longitude = 110.525449,Car = new  CarItem () { DriverCode = "CG845",Make = "Subaru",Model = "Leone", Year = "1988", Color = "Indigo",LicensePlate = "BLO-5594"} },
                new DriverItem() { Code = "SW320",FirstName = "Devy",LastName = "Beadle",Latitude = 64.7457113,Longitude = 20.9568227,Car = new  CarItem () { DriverCode = "SW320",Make = "GMC",Model = "Jimmy", Year = "1999", Color = "Maroon",LicensePlate = "ADS-2829"} },
                new DriverItem() { Code = "IF251",FirstName = "Morrie",LastName = "Stainer",Latitude = 54.0254025,Longitude = 39.811744,Car = new  CarItem () { DriverCode = "IF251",Make = "Audi",Model = "A6", Year = "2000", Color = "Aquamarine",LicensePlate = "UIG-8317"} },
                new DriverItem() { Code = "BH210",FirstName = "Geoffry",LastName = "Truelock",Latitude = 29.330041,Longitude = 119.334789,Car = new  CarItem () { DriverCode = "BH210",Make = "Jaguar",Model = "XJ Series", Year = "2004", Color = "Red",LicensePlate = "AYS-5185"} },
                new DriverItem() { Code = "NO049",FirstName = "Eugine",LastName = "Crossman",Latitude = 27.4475626,Longitude = 57.5051616,Car = new  CarItem () { DriverCode = "NO049",Make = "Buick",Model = "Regal", Year = "1999", Color = "Puce",LicensePlate = "CLW-0165"} },
                new DriverItem() { Code = "FF883",FirstName = "Jere",LastName = "Frye",Latitude = 44.7416079,Longitude = 18.2727658,Car = new  CarItem () { DriverCode = "FF883",Make = "Cadillac",Model = "DeVille", Year = "1992", Color = "Khaki",LicensePlate = "PPR-6113"} },
                new DriverItem() { Code = "TK599",FirstName = "Guinna",LastName = "Bordone",Latitude = 28.322781,Longitude = -81.3912772,Car = new  CarItem () { DriverCode = "TK599",Make = "GMC",Model = "Terrain", Year = "2011", Color = "Fuscia",LicensePlate = "UMA-9855"} },
                new DriverItem() { Code = "TM662",FirstName = "Lucien",LastName = "Yellowlees",Latitude = 54.4900421,Longitude = 59.5853599,Car = new  CarItem () { DriverCode = "TM662",Make = "Mitsubishi",Model = "Truck", Year = "1990", Color = "Maroon",LicensePlate = "UXV-9287"} },
                new DriverItem() { Code = "NC608",FirstName = "Jeno",LastName = "Prester",Latitude = 43.1581207,Longitude = -77.6063541,Car = new  CarItem () { DriverCode = "NC608",Make = "GMC",Model = "Savana 3500", Year = "1997", Color = "Fuscia",LicensePlate = "KTB-0608"} },
                new DriverItem() { Code = "QK747",FirstName = "Harvey",LastName = "Degan",Latitude = -8.1928129,Longitude = 115.1177654,Car = new  CarItem () { DriverCode = "QK747",Make = "Mercury",Model = "Mystique", Year = "1998", Color = "Fuscia",LicensePlate = "SZT-0187"} },
                new DriverItem() { Code = "YS744",FirstName = "Rochette",LastName = "Mundy",Latitude = 21.664044,Longitude = 110.639569,Car = new  CarItem () { DriverCode = "YS744",Make = "Lincoln",Model = "Continental", Year = "1995", Color = "Puce",LicensePlate = "DMY-7362"} },
                new DriverItem() { Code = "HH563",FirstName = "Abby",LastName = "Tegeller",Latitude = 36.39535,Longitude = 36.6889,Car = new  CarItem () { DriverCode = "HH563",Make = "Dodge",Model = "Ram Van 3500", Year = "1999", Color = "Mauv",LicensePlate = "HDX-6303"} },
                new DriverItem() { Code = "UN097",FirstName = "Auberon",LastName = "Folkerd",Latitude = 45.7340335,Longitude = -74.1402449,Car = new  CarItem () { DriverCode = "UN097",Make = "Toyota",Model = "Supra", Year = "1995", Color = "Violet",LicensePlate = "GIC-5105"} },
                new DriverItem() { Code = "BJ592",FirstName = "Paco",LastName = "Vain",Latitude = 48.7865463,Longitude = 2.054647,Car = new  CarItem () { DriverCode = "BJ592",Make = "Porsche",Model = "911", Year = "1988", Color = "Maroon",LicensePlate = "XUH-0387"} },
                new DriverItem() { Code = "WY944",FirstName = "Monroe",LastName = "Veivers",Latitude = 37.0909826,Longitude = 138.1698009,Car = new  CarItem () { DriverCode = "WY944",Make = "Dodge",Model = "Intrepid", Year = "1993", Color = "Blue",LicensePlate = "YYH-2812"} },
                new DriverItem() { Code = "HU205",FirstName = "Kahlil",LastName = "Nuzzetti",Latitude = 37.467686,Longitude = 121.153674,Car = new  CarItem () { DriverCode = "HU205",Make = "Pontiac",Model = "Safari", Year = "1988", Color = "Aquamarine",LicensePlate = "PMX-8105"} },
                new DriverItem() { Code = "AH854",FirstName = "Grethel",LastName = "Harriday",Latitude = 32.540157,Longitude = 111.513127,Car = new  CarItem () { DriverCode = "AH854",Make = "Ford",Model = "Econoline E250", Year = "1998", Color = "Mauv",LicensePlate = "LIM-4475"} },
                new DriverItem() { Code = "YT223",FirstName = "Elianore",LastName = "Diviney",Latitude = 34.393136,Longitude = 115.865746,Car = new  CarItem () { DriverCode = "YT223",Make = "Toyota",Model = "MR2", Year = "1986", Color = "Aquamarine",LicensePlate = "DYA-6856"} },
                new DriverItem() { Code = "BF796",FirstName = "Bee",LastName = "Charsley",Latitude = 34.9196569,Longitude = 72.6325857,Car = new  CarItem () { DriverCode = "BF796",Make = "Saab",Model = "9-3", Year = "2009", Color = "Maroon",LicensePlate = "YAU-1269"} },
                new DriverItem() { Code = "XW504",FirstName = "Dallis",LastName = "Braxton",Latitude = 37.3833017,Longitude = 55.5025595,Car = new  CarItem () { DriverCode = "XW504",Make = "Toyota",Model = "Solara", Year = "2002", Color = "Goldenrod",LicensePlate = "OBA-3694"} },
                new DriverItem() { Code = "ZF391",FirstName = "Anastassia",LastName = "De la Harpe",Latitude = -7.869171,Longitude = 110.1236245,Car = new  CarItem () { DriverCode = "ZF391",Make = "Scion",Model = "xB", Year = "2005", Color = "Puce",LicensePlate = "CFA-6517"} },
                new DriverItem() { Code = "IO506",FirstName = "Jean",LastName = "Maric",Latitude = 36.097577,Longitude = 114.392392,Car = new  CarItem () { DriverCode = "IO506",Make = "Volvo",Model = "V40", Year = "2004", Color = "Blue",LicensePlate = "DOW-8444"} },
                new DriverItem() { Code = "HW362",FirstName = "Angeli",LastName = "Degnen",Latitude = 29.7444645,Longitude = 105.7948814,Car = new  CarItem () { DriverCode = "HW362",Make = "Pontiac",Model = "LeMans", Year = "1989", Color = "Fuscia",LicensePlate = "GTO-7502"} },
                new DriverItem() { Code = "QM056",FirstName = "Bernetta",LastName = "Hearns",Latitude = 23.422152,Longitude = 106.681138,Car = new  CarItem () { DriverCode = "QM056",Make = "GMC",Model = "Canyon", Year = "2006", Color = "Teal",LicensePlate = "YUP-0942"} },
                new DriverItem() { Code = "FR928",FirstName = "Lawrence",LastName = "Donovin",Latitude = 45.7089869,Longitude = 34.3874785,Car = new  CarItem () { DriverCode = "FR928",Make = "Audi",Model = "S8", Year = "2007", Color = "Pink",LicensePlate = "PNT-6810"} },
                new DriverItem() { Code = "UE418",FirstName = "Franny",LastName = "Allderidge",Latitude = 12.0310173,Longitude = 102.2928163,Car = new  CarItem () { DriverCode = "UE418",Make = "Pontiac",Model = "Vibe", Year = "2003", Color = "Teal",LicensePlate = "RYR-8843"} },
                new DriverItem() { Code = "NM419",FirstName = "Godfree",LastName = "De Morena",Latitude = 40.1427498,Longitude = 44.1241666,Car = new  CarItem () { DriverCode = "NM419",Make = "Audi",Model = "V8", Year = "1994", Color = "Goldenrod",LicensePlate = "EEO-9251"} },
                new DriverItem() { Code = "QW217",FirstName = "Cobb",LastName = "Storrock",Latitude = 30.4415185,Longitude = -87.2518824,Car = new  CarItem () { DriverCode = "QW217",Make = "Mercedes-Benz",Model = "CLS-Class", Year = "2012", Color = "Fuscia",LicensePlate = "KUJ-5518"} },
                new DriverItem() { Code = "VW657",FirstName = "Jeanie",LastName = "Arnley",Latitude = -0.819175,Longitude = 120.167297,Car = new  CarItem () { DriverCode = "VW657",Make = "Mitsubishi",Model = "Pajero", Year = "1992", Color = "Orange",LicensePlate = "BYK-7672"} },
                new DriverItem() { Code = "UV455",FirstName = "Mabel",LastName = "Butteris",Latitude = -3.4097,Longitude = 119.3077,Car = new  CarItem () { DriverCode = "UV455",Make = "Ford",Model = "Econoline E150", Year = "1993", Color = "Yellow",LicensePlate = "RAO-7190"} },
                new DriverItem() { Code = "UJ624",FirstName = "Claudian",LastName = "Claricoats",Latitude = 30.5383451,Longitude = 117.0666454,Car = new  CarItem () { DriverCode = "UJ624",Make = "Toyota",Model = "Supra", Year = "1993", Color = "Violet",LicensePlate = "QEO-1682"} },
                new DriverItem() { Code = "XX571",FirstName = "Eilis",LastName = "Vauter",Latitude = 22.270978,Longitude = 113.576677,Car = new  CarItem () { DriverCode = "XX571",Make = "Mitsubishi",Model = "3000GT", Year = "1998", Color = "Indigo",LicensePlate = "QMC-3122"} },
                new DriverItem() { Code = "BJ721",FirstName = "Celie",LastName = "Klazenga",Latitude = 63.4400274,Longitude = 10.4024274,Car = new  CarItem () { DriverCode = "BJ721",Make = "Toyota",Model = "RAV4", Year = "2008", Color = "Indigo",LicensePlate = "FYG-7751"} },
                new DriverItem() { Code = "CP173",FirstName = "Orelie",LastName = "Simnel",Latitude = 40.003695,Longitude = 116.103452,Car = new  CarItem () { DriverCode = "CP173",Make = "Mazda",Model = "Protege", Year = "1997", Color = "Purple",LicensePlate = "WJX-9196"} },
                new DriverItem() { Code = "HM250",FirstName = "Mavra",LastName = "Jack",Latitude = 31.2303904,Longitude = 121.4737021,Car = new  CarItem () { DriverCode = "HM250",Make = "Mercury",Model = "Mountaineer", Year = "2005", Color = "Purple",LicensePlate = "TKL-8520"} },
                new DriverItem() { Code = "QY007",FirstName = "Kata",LastName = "Royal",Latitude = -8.188844,Longitude = 122.9287931,Car = new  CarItem () { DriverCode = "QY007",Make = "Ford",Model = "Escort", Year = "1990", Color = "Mauv",LicensePlate = "EGK-1688"} },
                new DriverItem() { Code = "NU022",FirstName = "Celia",LastName = "Adess",Latitude = -8.3921715,Longitude = 115.2956118,Car = new  CarItem () { DriverCode = "NU022",Make = "Toyota",Model = "Venza", Year = "2012", Color = "Teal",LicensePlate = "FPE-8228"} },
                new DriverItem() { Code = "NP542",FirstName = "Jade",LastName = "Casari",Latitude = 7.91667,Longitude = 123.75,Car = new  CarItem () { DriverCode = "NP542",Make = "Ford",Model = "F-Series", Year = "1987", Color = "Crimson",LicensePlate = "IVL-7895"} },
                new DriverItem() { Code = "DN481",FirstName = "Stephine",LastName = "Cresar",Latitude = 9.791337,Longitude = -74.7975249,Car = new  CarItem () { DriverCode = "DN481",Make = "GMC",Model = "2500 Club Coupe", Year = "1994", Color = "Red",LicensePlate = "NFP-3158"} },
                new DriverItem() { Code = "VO266",FirstName = "Decca",LastName = "Mardling",Latitude = -6.3865373,Longitude = 107.4010519,Car = new  CarItem () { DriverCode = "VO266",Make = "Ford",Model = "Edge", Year = "2009", Color = "Teal",LicensePlate = "AZU-1992"} },
                new DriverItem() { Code = "LJ817",FirstName = "Lon",LastName = "Celli",Latitude = 39.420994,Longitude = 20.0143022,Car = new  CarItem () { DriverCode = "LJ817",Make = "Pontiac",Model = "Grand Am", Year = "1985", Color = "Aquamarine",LicensePlate = "RRU-3101"} },
                new DriverItem() { Code = "BT361",FirstName = "Percival",LastName = "Dusting",Latitude = -31.6434099,Longitude = 29.5358464,Car = new  CarItem () { DriverCode = "BT361",Make = "Mitsubishi",Model = "Endeavor", Year = "2011", Color = "Mauv",LicensePlate = "KUB-7877"} },
                new DriverItem() { Code = "QT706",FirstName = "Humfrid",LastName = "Byrne",Latitude = 25.386379,Longitude = 114.922922,Car = new  CarItem () { DriverCode = "QT706",Make = "Plymouth",Model = "Colt", Year = "1992", Color = "Aquamarine",LicensePlate = "SGC-6718"} },
                new DriverItem() { Code = "SW528",FirstName = "Serena",LastName = "Howler",Latitude = 15.3817945,Longitude = 120.7080369,Car = new  CarItem () { DriverCode = "SW528",Make = "Mitsubishi",Model = "Expo", Year = "1992", Color = "Khaki",LicensePlate = "UWQ-6052"} },
                new DriverItem() { Code = "VI913",FirstName = "Rainer",LastName = "Hendricks",Latitude = 15.5651234,Longitude = 102.5814416,Car = new  CarItem () { DriverCode = "VI913",Make = "Dodge",Model = "Spirit", Year = "1994", Color = "Puce",LicensePlate = "SBN-1166"} },
                new DriverItem() { Code = "BK994",FirstName = "Cecily",LastName = "Pentin",Latitude = -1.5904721,Longitude = -78.9995154,Car = new  CarItem () { DriverCode = "BK994",Make = "Mercury",Model = "Cougar", Year = "1989", Color = "Purple",LicensePlate = "XFA-9758"} },
                new DriverItem() { Code = "YS488",FirstName = "Wilden",LastName = "Saul",Latitude = 3.095417,Longitude = -76.1435915,Car = new  CarItem () { DriverCode = "YS488",Make = "Volvo",Model = "S80", Year = "2004", Color = "Purple",LicensePlate = "IJQ-2457"} },
                new DriverItem() { Code = "GS917",FirstName = "Bogart",LastName = "Scrowston",Latitude = 4.2867309,Longitude = -74.812294,Car = new  CarItem () { DriverCode = "GS917",Make = "Ford",Model = "Club Wagon", Year = "1994", Color = "Mauv",LicensePlate = "KWO-3137"} },
                new DriverItem() { Code = "IJ805",FirstName = "Evangelia",LastName = "Lambert",Latitude = 50.7056119,Longitude = 13.9301748,Car = new  CarItem () { DriverCode = "IJ805",Make = "Cadillac",Model = "Fleetwood", Year = "1992", Color = "Yellow",LicensePlate = "SIQ-0757"} },
                new DriverItem() { Code = "LZ243",FirstName = "Tresa",LastName = "Alexandre",Latitude = 33.036246,Longitude = 112.039383,Car = new  CarItem () { DriverCode = "LZ243",Make = "Ford",Model = "Flex", Year = "2010", Color = "Yellow",LicensePlate = "IXN-9029"} },
                new DriverItem() { Code = "JO051",FirstName = "Dara",LastName = "Stiegars",Latitude = 45.1839187,Longitude = 18.8237103,Car = new  CarItem () { DriverCode = "JO051",Make = "Hummer",Model = "H1", Year = "2006", Color = "Goldenrod",LicensePlate = "CSP-3807"} },
                new DriverItem() { Code = "UD700",FirstName = "Gertrude",LastName = "Allett",Latitude = 52.339987,Longitude = 20.3286222,Car = new  CarItem () { DriverCode = "UD700",Make = "Jeep",Model = "Liberty", Year = "2009", Color = "Green",LicensePlate = "TOU-0490"} }
            };

            var passengers = new List<PassengerItem>()
            {
                new PassengerItem() { Code = "joe.kassini@gmail.com", FirstName = "Joe", LastName = "Kassini", MobileNumber =  "3105551212", Email = "joe.kassini@gmail.com" },
                new PassengerItem() { Code = "rob.dart@gmail.com", FirstName = "Rob", LastName = "Dart", MobileNumber =  "7145551313", Email = "rob.dart@gmail.com" },
                new PassengerItem() { Code = "sue.faming@gmail.com", FirstName = "Sue", LastName = "Faming", MobileNumber =  "7145551414", Email = "sue.faming@gmail.com" },
                new PassengerItem() { Code = "tbranchflower0@tripadvisor.com",FirstName = "Trescha",LastName = "Branchflower",MobileNumber = "8728663243",Email = "tbranchflower0@tripadvisor.com"},
                new PassengerItem() { Code = "mwrankling1@hexun.com",FirstName = "Markus",LastName = "Wrankling",MobileNumber = "9004626998",Email = "mwrankling1@hexun.com"},
                new PassengerItem() { Code = "djohnson2@netvibes.com",FirstName = "Devonne",LastName = "Johnson",MobileNumber = "5712293883",Email = "djohnson2@netvibes.com"},
                new PassengerItem() { Code = "wbernardeschi3@ow.ly",FirstName = "Whitney",LastName = "Bernardeschi",MobileNumber = "5162998676",Email = "wbernardeschi3@ow.ly"},
                new PassengerItem() { Code = "apopplewell4@gnu.org",FirstName = "Annora",LastName = "Popplewell",MobileNumber = "9833411589",Email = "apopplewell4@gnu.org"},
                new PassengerItem() { Code = "glavrinov5@ft.com",FirstName = "Glyn",LastName = "Lavrinov",MobileNumber = "9114297662",Email = "glavrinov5@ft.com"},
                new PassengerItem() { Code = "sschule6@goo.gl",FirstName = "Sari",LastName = "Schule",MobileNumber = "4911356909",Email = "sschule6@goo.gl"},
                new PassengerItem() { Code = "kmycock7@feedburner.com",FirstName = "Karen",LastName = "Mycock",MobileNumber = "6414629897",Email = "kmycock7@feedburner.com"},
                new PassengerItem() { Code = "hbabalola8@deviantart.com",FirstName = "Hasheem",LastName = "Babalola",MobileNumber = "7365889195",Email = "hbabalola8@deviantart.com"},
                new PassengerItem() { Code = "rcockcroft9@gizmodo.com",FirstName = "Rafaello",LastName = "Cockcroft",MobileNumber = "3026971182",Email = "rcockcroft9@gizmodo.com"},
                new PassengerItem() { Code = "sblowicka@a8.net",FirstName = "Shannen",LastName = "Blowick",MobileNumber = "2132353729",Email = "sblowicka@a8.net"},
                new PassengerItem() { Code = "nrosendorfb@flavors.me",FirstName = "Nathalie",LastName = "Rosendorf",MobileNumber = "9051999353",Email = "nrosendorfb@flavors.me"},
                new PassengerItem() { Code = "vwitardc@baidu.com",FirstName = "Veriee",LastName = "Witard",MobileNumber = "8415367638",Email = "vwitardc@baidu.com"},
                new PassengerItem() { Code = "cblannind@toplist.cz",FirstName = "Cris",LastName = "Blannin",MobileNumber = "9203448812",Email = "cblannind@toplist.cz"},
                new PassengerItem() { Code = "divanikhine@umich.edu",FirstName = "Davida",LastName = "Ivanikhin",MobileNumber = "9944410477",Email = "divanikhine@umich.edu"},
                new PassengerItem() { Code = "cgosneyf@photobucket.com",FirstName = "Chic",LastName = "Gosney",MobileNumber = "2979521738",Email = "cgosneyf@photobucket.com"},
                new PassengerItem() { Code = "vkeighleyg@zdnet.com",FirstName = "Vance",LastName = "Keighley",MobileNumber = "9147503165",Email = "vkeighleyg@zdnet.com"},
                new PassengerItem() { Code = "ldarellh@cbsnews.com",FirstName = "Lissi",LastName = "Darell",MobileNumber = "8925702012",Email = "ldarellh@cbsnews.com"},
                new PassengerItem() { Code = "gdewitti@hibu.com",FirstName = "Garland",LastName = "De Witt",MobileNumber = "4101694480",Email = "gdewitti@hibu.com"},
                new PassengerItem() { Code = "ebrantj@bravesites.com",FirstName = "Enoch",LastName = "Brant",MobileNumber = "9672636791",Email = "ebrantj@bravesites.com"},
                new PassengerItem() { Code = "rciseck@weebly.com",FirstName = "Regen",LastName = "Cisec",MobileNumber = "7855694888",Email = "rciseck@weebly.com"},
                new PassengerItem() { Code = "jwolferl@irs.gov",FirstName = "Jeffie",LastName = "Wolfer",MobileNumber = "9491740764",Email = "jwolferl@irs.gov"},
                new PassengerItem() { Code = "idaverenm@soup.io",FirstName = "Issy",LastName = "Daveren",MobileNumber = "5285406906",Email = "idaverenm@soup.io"},
                new PassengerItem() { Code = "gscallonn@psu.edu",FirstName = "Geordie",LastName = "Scallon",MobileNumber = "2503579508",Email = "gscallonn@psu.edu"},
                new PassengerItem() { Code = "bmcfarlando@opera.com",FirstName = "Beck",LastName = "McFarland",MobileNumber = "7153958437",Email = "bmcfarlando@opera.com"},
                new PassengerItem() { Code = "mbridywaterp@army.mil",FirstName = "Mohandas",LastName = "Bridywater",MobileNumber = "7236526661",Email = "mbridywaterp@army.mil"},
                new PassengerItem() { Code = "smeanwellq@vinaora.com",FirstName = "Susanna",LastName = "Meanwell",MobileNumber = "4966822063",Email = "smeanwellq@vinaora.com"},
                new PassengerItem() { Code = "wkivlinr@livejournal.com",FirstName = "Witty",LastName = "Kivlin",MobileNumber = "5484972262",Email = "wkivlinr@livejournal.com"},
                new PassengerItem() { Code = "tsalamons@gmpg.org",FirstName = "Tamar",LastName = "Salamon",MobileNumber = "2703615837",Email = "tsalamons@gmpg.org"},
                new PassengerItem() { Code = "pbreddyt@umn.edu",FirstName = "Paolo",LastName = "Breddy",MobileNumber = "2211792746",Email = "pbreddyt@umn.edu"},
                new PassengerItem() { Code = "tbeckmannu@reuters.com",FirstName = "Tomasina",LastName = "Beckmann",MobileNumber = "9474612138",Email = "tbeckmannu@reuters.com"},
                new PassengerItem() { Code = "ahiscocksv@edublogs.org",FirstName = "Allyce",LastName = "Hiscocks",MobileNumber = "6869959430",Email = "ahiscocksv@edublogs.org"},
                new PassengerItem() { Code = "ayausw@timesonline.co.uk",FirstName = "Asa",LastName = "Yaus",MobileNumber = "2111020087",Email = "ayausw@timesonline.co.uk"},
                new PassengerItem() { Code = "lcheinex@about.me",FirstName = "Lothario",LastName = "Cheine",MobileNumber = "1702451992",Email = "lcheinex@about.me"},
                new PassengerItem() { Code = "barnaudiny@usnews.com",FirstName = "Bronson",LastName = "Arnaudin",MobileNumber = "6744677330",Email = "barnaudiny@usnews.com"},
                new PassengerItem() { Code = "atarbinz@earthlink.net",FirstName = "Alysa",LastName = "Tarbin",MobileNumber = "5519164298",Email = "atarbinz@earthlink.net"},
                new PassengerItem() { Code = "eclinton10@twitter.com",FirstName = "Eduard",LastName = "Clinton",MobileNumber = "6561591597",Email = "eclinton10@twitter.com"},
                new PassengerItem() { Code = "gboughton11@apache.org",FirstName = "Gianni",LastName = "Boughton",MobileNumber = "9602769458",Email = "gboughton11@apache.org"},
                new PassengerItem() { Code = "mlaurenz12@exblog.jp",FirstName = "Mikkel",LastName = "Laurenz",MobileNumber = "4536622590",Email = "mlaurenz12@exblog.jp"},
                new PassengerItem() { Code = "cquickenden13@va.gov",FirstName = "Calla",LastName = "Quickenden",MobileNumber = "9754713702",Email = "cquickenden13@va.gov"},
                new PassengerItem() { Code = "pgyngell14@ed.gov",FirstName = "Paton",LastName = "Gyngell",MobileNumber = "5637814166",Email = "pgyngell14@ed.gov"},
                new PassengerItem() { Code = "gkemer15@statcounter.com",FirstName = "Gherardo",LastName = "Kemer",MobileNumber = "4779445526",Email = "gkemer15@statcounter.com"},
                new PassengerItem() { Code = "mmaccarrane16@studiopress.com",FirstName = "Moria",LastName = "MacCarrane",MobileNumber = "1845649095",Email = "mmaccarrane16@studiopress.com"},
                new PassengerItem() { Code = "qkernocke17@rakuten.co.jp",FirstName = "Querida",LastName = "Kernocke",MobileNumber = "9573962632",Email = "qkernocke17@rakuten.co.jp"},
                new PassengerItem() { Code = "rjencken18@cnbc.com",FirstName = "Ronnica",LastName = "Jencken",MobileNumber = "3319674455",Email = "rjencken18@cnbc.com"},
                new PassengerItem() { Code = "bvasyunin19@state.tx.us",FirstName = "Bobbe",LastName = "Vasyunin",MobileNumber = "4871211073",Email = "bvasyunin19@state.tx.us"},
                new PassengerItem() { Code = "edellatorre1a@cargocollective.com",FirstName = "Edeline",LastName = "Dellatorre",MobileNumber = "2446483906",Email = "edellatorre1a@cargocollective.com"},
                new PassengerItem() { Code = "dcrummy1b@studiopress.com",FirstName = "Delilah",LastName = "Crummy",MobileNumber = "2316623106",Email = "dcrummy1b@studiopress.com"},
                new PassengerItem() { Code = "dmunn1c@nature.com",FirstName = "Dacy",LastName = "Munn",MobileNumber = "2063060751",Email = "dmunn1c@nature.com"},
                new PassengerItem() { Code = "goates1d@prlog.org",FirstName = "Gage",LastName = "Oates",MobileNumber = "9115446924",Email = "goates1d@prlog.org"},
                new PassengerItem() { Code = "eshortan1e@nifty.com",FirstName = "Elaine",LastName = "Shortan",MobileNumber = "2352697758",Email = "eshortan1e@nifty.com"},
                new PassengerItem() { Code = "ddecleyne1f@phoca.cz",FirstName = "Darcy",LastName = "De Cleyne",MobileNumber = "4605885845",Email = "ddecleyne1f@phoca.cz"},
                new PassengerItem() { Code = "scostall1g@instagram.com",FirstName = "Sandi",LastName = "Costall",MobileNumber = "7283550788",Email = "scostall1g@instagram.com"},
                new PassengerItem() { Code = "fsnalham1h@bbb.org",FirstName = "Fredek",LastName = "Snalham",MobileNumber = "3651172619",Email = "fsnalham1h@bbb.org"},
                new PassengerItem() { Code = "alodevick1i@google.com",FirstName = "Alphonse",LastName = "Lodevick",MobileNumber = "9565683073",Email = "alodevick1i@google.com"},
                new PassengerItem() { Code = "kdissman1j@sciencedaily.com",FirstName = "Kele",LastName = "Dissman",MobileNumber = "2629867135",Email = "kdissman1j@sciencedaily.com"},
                new PassengerItem() { Code = "abigg1k@tmall.com",FirstName = "Alexis",LastName = "Bigg",MobileNumber = "7663014908",Email = "abigg1k@tmall.com"},
                new PassengerItem() { Code = "mmoseley1l@cornell.edu",FirstName = "Manon",LastName = "Moseley",MobileNumber = "6399182960",Email = "mmoseley1l@cornell.edu"},
                new PassengerItem() { Code = "agirardeau1m@weibo.com",FirstName = "Ally",LastName = "Girardeau",MobileNumber = "9058321763",Email = "agirardeau1m@weibo.com"},
                new PassengerItem() { Code = "ajanes1n@about.me",FirstName = "Aurel",LastName = "Janes",MobileNumber = "3625135271",Email = "ajanes1n@about.me"},
                new PassengerItem() { Code = "mhugues1o@bbb.org",FirstName = "Mabel",LastName = "Hugues",MobileNumber = "9421044386",Email = "mhugues1o@bbb.org"},
                new PassengerItem() { Code = "murwen1p@tinyurl.com",FirstName = "Minne",LastName = "Urwen",MobileNumber = "9841420437",Email = "murwen1p@tinyurl.com"},
                new PassengerItem() { Code = "pbignold1q@princeton.edu",FirstName = "Pepe",LastName = "Bignold",MobileNumber = "5056239923",Email = "pbignold1q@princeton.edu"},
                new PassengerItem() { Code = "amcbrearty1r@economist.com",FirstName = "Albina",LastName = "McBrearty",MobileNumber = "1961175353",Email = "amcbrearty1r@economist.com"},
                new PassengerItem() { Code = "wcadreman1s@1und1.de",FirstName = "Wolfie",LastName = "Cadreman",MobileNumber = "9569939808",Email = "wcadreman1s@1und1.de"},
                new PassengerItem() { Code = "jjuarez1t@jugem.jp",FirstName = "Jacklin",LastName = "Juarez",MobileNumber = "2934752164",Email = "jjuarez1t@jugem.jp"},
                new PassengerItem() { Code = "hlindroos1u@delicious.com",FirstName = "Harley",LastName = "Lindroos",MobileNumber = "8801948833",Email = "hlindroos1u@delicious.com"},
                new PassengerItem() { Code = "ckulis1v@hexun.com",FirstName = "Cherri",LastName = "Kulis",MobileNumber = "7979292321",Email = "ckulis1v@hexun.com"},
                new PassengerItem() { Code = "ccozins1w@fastcompany.com",FirstName = "Charmian",LastName = "Cozins",MobileNumber = "8568545301",Email = "ccozins1w@fastcompany.com"},
                new PassengerItem() { Code = "jsthill1x@hexun.com",FirstName = "Jedd",LastName = "St. Hill",MobileNumber = "3812244280",Email = "jsthill1x@hexun.com"},
                new PassengerItem() { Code = "mmilesap1y@unesco.org",FirstName = "Maggee",LastName = "Milesap",MobileNumber = "2387088560",Email = "mmilesap1y@unesco.org"},
                new PassengerItem() { Code = "ccarruthers1z@nhs.uk",FirstName = "Chev",LastName = "Carruthers",MobileNumber = "3747366844",Email = "ccarruthers1z@nhs.uk"},
                new PassengerItem() { Code = "amarklin20@booking.com",FirstName = "Adelina",LastName = "Marklin",MobileNumber = "5171421545",Email = "amarklin20@booking.com"},
                new PassengerItem() { Code = "wpetrak21@example.com",FirstName = "Warde",LastName = "Petrak",MobileNumber = "3492187114",Email = "wpetrak21@example.com"},
                new PassengerItem() { Code = "smattersley22@blogger.com",FirstName = "Susanne",LastName = "Mattersley",MobileNumber = "6364967761",Email = "smattersley22@blogger.com"},
                new PassengerItem() { Code = "ekenway23@omniture.com",FirstName = "Eloisa",LastName = "Kenway",MobileNumber = "1198324310",Email = "ekenway23@omniture.com"},
                new PassengerItem() { Code = "mbassingden24@biblegateway.com",FirstName = "Meier",LastName = "Bassingden",MobileNumber = "8137577337",Email = "mbassingden24@biblegateway.com"},
                new PassengerItem() { Code = "malyoshin25@unblog.fr",FirstName = "Morgen",LastName = "Alyoshin",MobileNumber = "7763593016",Email = "malyoshin25@unblog.fr"},
                new PassengerItem() { Code = "spirson26@who.int",FirstName = "Stanly",LastName = "Pirson",MobileNumber = "1109895466",Email = "spirson26@who.int"},
                new PassengerItem() { Code = "wcurcher27@macromedia.com",FirstName = "Waly",LastName = "Curcher",MobileNumber = "2823209028",Email = "wcurcher27@macromedia.com"},
                new PassengerItem() { Code = "jwines28@constantcontact.com",FirstName = "Jayme",LastName = "Wines",MobileNumber = "4166089711",Email = "jwines28@constantcontact.com"},
                new PassengerItem() { Code = "htomaszynski29@theguardian.com",FirstName = "Hilarius",LastName = "Tomaszynski",MobileNumber = "7023018162",Email = "htomaszynski29@theguardian.com"},
                new PassengerItem() { Code = "tpeltzer2a@rediff.com",FirstName = "Tilda",LastName = "Peltzer",MobileNumber = "7124288180",Email = "tpeltzer2a@rediff.com"},
                new PassengerItem() { Code = "mbowland2b@fema.gov",FirstName = "Mavra",LastName = "Bowland",MobileNumber = "9359264555",Email = "mbowland2b@fema.gov"},
                new PassengerItem() { Code = "jlivermore2c@mysql.com",FirstName = "Jerad",LastName = "Livermore",MobileNumber = "7032101067",Email = "jlivermore2c@mysql.com"},
                new PassengerItem() { Code = "breiach2d@nationalgeographic.com",FirstName = "Beau",LastName = "Reiach",MobileNumber = "4932769007",Email = "breiach2d@nationalgeographic.com"},
                new PassengerItem() { Code = "ocarek2e@php.net",FirstName = "Ogdon",LastName = "Carek",MobileNumber = "1982391517",Email = "ocarek2e@php.net"},
                new PassengerItem() { Code = "smougin2f@soup.io",FirstName = "Shaina",LastName = "Mougin",MobileNumber = "9371588224",Email = "smougin2f@soup.io"},
                new PassengerItem() { Code = "ttruman2g@ftc.gov",FirstName = "Tiffanie",LastName = "Truman",MobileNumber = "8049473591",Email = "ttruman2g@ftc.gov"},
                new PassengerItem() { Code = "mkaiser2h@prweb.com",FirstName = "Melisandra",LastName = "Kaiser",MobileNumber = "9931118898",Email = "mkaiser2h@prweb.com"},
                new PassengerItem() { Code = "mrieme2i@phoca.cz",FirstName = "Major",LastName = "Rieme",MobileNumber = "9423374216",Email = "mrieme2i@phoca.cz"},
                new PassengerItem() { Code = "cfolomin2j@jalbum.net",FirstName = "Callie",LastName = "Folomin",MobileNumber = "1299803842",Email = "cfolomin2j@jalbum.net"},
                new PassengerItem() { Code = "lrubes2k@gizmodo.com",FirstName = "Libbey",LastName = "Rubes",MobileNumber = "5349995537",Email = "lrubes2k@gizmodo.com"},
                new PassengerItem() { Code = "atummasutti2l@yahoo.com",FirstName = "Ana",LastName = "Tummasutti",MobileNumber = "9408962915",Email = "atummasutti2l@yahoo.com"},
                new PassengerItem() { Code = "cstuddeard2m@reuters.com",FirstName = "Cirillo",LastName = "Studdeard",MobileNumber = "4497538611",Email = "cstuddeard2m@reuters.com"},
                new PassengerItem() { Code = "bcoultard2n@virginia.edu",FirstName = "Burt",LastName = "Coultard",MobileNumber = "8093716608",Email = "bcoultard2n@virginia.edu"},
                new PassengerItem() { Code = "lrablin2o@shutterfly.com",FirstName = "Lutero",LastName = "Rablin",MobileNumber = "5675843138",Email = "lrablin2o@shutterfly.com"},
                new PassengerItem() { Code = "fmilius2p@sitemeter.com",FirstName = "Fay",LastName = "Milius",MobileNumber = "1936467110",Email = "fmilius2p@sitemeter.com"},
                new PassengerItem() { Code = "npizey2q@nature.com",FirstName = "Nico",LastName = "Pizey",MobileNumber = "3339808041",Email = "npizey2q@nature.com"},
                new PassengerItem() { Code = "amulmuray2r@spiegel.de",FirstName = "Alexander",LastName = "Mulmuray",MobileNumber = "2492510213",Email = "amulmuray2r@spiegel.de"}
            };

            var pickupLocations = new List<PickupLocation>()
            {
                new PickupLocation() { Id = 1, Name = "Microsoft Corporate Office", Latitude = 47.6423354, Longitude = -122.1391189 },
                new PickupLocation() { Id = 2, Name = "Hyatt Regency Bellevue", Latitude = 47.618282, Longitude = -122.201035 },
                new PickupLocation() { Id = 3, Name = "Space Needle", Latitude = 47.620530, Longitude = -122.349300 }
            };

            var destinations = new List<TripLocation>()
            {
                new TripLocation() {Latitude = 47.6131746, Longitude = -122.4821466},
                new TripLocation() {Latitude = 47.5963256, Longitude = -122.1928181},
                new TripLocation() {Latitude = 47.6721228, Longitude = -122.1356409}
            };
            #endregion

            #region Schedule data
            // Each date has pickups distributed by percentages (weighted) for each location. If the number of trips/day is set to 100
            // and location 1 has the PickupLocation1Percentage value set to 10, then 10 (10%) TripItem records are generated for that
            // pickup location. The combined total of all three locations do not need to equal 100 if you want fewer trips in a given
            // day (such as a slow weekend). They can also exceed 100 if you want a spike in activity.
            var schedule = new List<PickupSchedule>()
            {
                new PickupSchedule() { Date = new DateTime(2019, 2, 3), PickupLocation1Percentage = 25, PickupLocation2Percentage = 35, PickupLocation3Percentage = 15 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 2, 4), PickupLocation1Percentage = 60, PickupLocation2Percentage = 28, PickupLocation3Percentage = 10 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 5), PickupLocation1Percentage = 37, PickupLocation2Percentage = 18, PickupLocation3Percentage = 42 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 6), PickupLocation1Percentage = 20, PickupLocation2Percentage = 65, PickupLocation3Percentage = 15 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 7), PickupLocation1Percentage = 45, PickupLocation2Percentage = 22, PickupLocation3Percentage = 33 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 8), PickupLocation1Percentage = 16, PickupLocation2Percentage = 27, PickupLocation3Percentage = 65 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 9), PickupLocation1Percentage = 12, PickupLocation2Percentage = 18, PickupLocation3Percentage = 33 },

                new PickupSchedule() { Date = new DateTime(2019, 2, 10), PickupLocation1Percentage = 21, PickupLocation2Percentage = 39, PickupLocation3Percentage = 17 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 2, 11), PickupLocation1Percentage = 56, PickupLocation2Percentage = 31, PickupLocation3Percentage = 12 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 12), PickupLocation1Percentage = 30, PickupLocation2Percentage = 22, PickupLocation3Percentage = 40 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 13), PickupLocation1Percentage = 18, PickupLocation2Percentage = 67, PickupLocation3Percentage = 16 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 14), PickupLocation1Percentage = 52, PickupLocation2Percentage = 19, PickupLocation3Percentage = 28 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 15), PickupLocation1Percentage = 14, PickupLocation2Percentage = 24, PickupLocation3Percentage = 51 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 16), PickupLocation1Percentage = 10, PickupLocation2Percentage = 16, PickupLocation3Percentage = 37 },

                new PickupSchedule() { Date = new DateTime(2019, 2, 17), PickupLocation1Percentage = 29, PickupLocation2Percentage = 31, PickupLocation3Percentage = 13 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 2, 18), PickupLocation1Percentage = 63, PickupLocation2Percentage = 26, PickupLocation3Percentage = 15 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 19), PickupLocation1Percentage = 35, PickupLocation2Percentage = 21, PickupLocation3Percentage = 39 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 20), PickupLocation1Percentage = 19, PickupLocation2Percentage = 63, PickupLocation3Percentage = 14 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 21), PickupLocation1Percentage = 47, PickupLocation2Percentage = 19, PickupLocation3Percentage = 35 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 22), PickupLocation1Percentage = 18, PickupLocation2Percentage = 25, PickupLocation3Percentage = 64 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 23), PickupLocation1Percentage = 15, PickupLocation2Percentage = 18, PickupLocation3Percentage = 28 },

                new PickupSchedule() { Date = new DateTime(2019, 2, 24), PickupLocation1Percentage = 26, PickupLocation2Percentage = 34, PickupLocation3Percentage = 14 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 2, 25), PickupLocation1Percentage = 59, PickupLocation2Percentage = 22, PickupLocation3Percentage = 15 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 26), PickupLocation1Percentage = 33, PickupLocation2Percentage = 20, PickupLocation3Percentage = 44 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 27), PickupLocation1Percentage = 23, PickupLocation2Percentage = 54, PickupLocation3Percentage = 19 },
                new PickupSchedule() { Date = new DateTime(2019, 2, 28), PickupLocation1Percentage = 40, PickupLocation2Percentage = 19, PickupLocation3Percentage = 38 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 1), PickupLocation1Percentage = 15, PickupLocation2Percentage = 21, PickupLocation3Percentage = 60 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 2), PickupLocation1Percentage = 13, PickupLocation2Percentage = 20, PickupLocation3Percentage = 35 },

                new PickupSchedule() { Date = new DateTime(2019, 3, 3), PickupLocation1Percentage = 30, PickupLocation2Percentage = 30, PickupLocation3Percentage = 12 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 3, 4), PickupLocation1Percentage = 65, PickupLocation2Percentage = 32, PickupLocation3Percentage = 17 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 5), PickupLocation1Percentage = 43, PickupLocation2Percentage = 25, PickupLocation3Percentage = 50 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 6), PickupLocation1Percentage = 26, PickupLocation2Percentage = 70, PickupLocation3Percentage = 16 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 7), PickupLocation1Percentage = 52, PickupLocation2Percentage = 28, PickupLocation3Percentage = 39 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 8), PickupLocation1Percentage = 22, PickupLocation2Percentage = 33, PickupLocation3Percentage = 77 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 9), PickupLocation1Percentage = 18, PickupLocation2Percentage = 26, PickupLocation3Percentage = 39 },

                new PickupSchedule() { Date = new DateTime(2019, 3, 10), PickupLocation1Percentage = 20, PickupLocation2Percentage = 32, PickupLocation3Percentage = 12 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 3, 11), PickupLocation1Percentage = 53, PickupLocation2Percentage = 25, PickupLocation3Percentage = 13 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 12), PickupLocation1Percentage = 36, PickupLocation2Percentage = 19, PickupLocation3Percentage = 46 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 13), PickupLocation1Percentage = 23, PickupLocation2Percentage = 60, PickupLocation3Percentage = 16 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 14), PickupLocation1Percentage = 44, PickupLocation2Percentage = 20, PickupLocation3Percentage = 30 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 15), PickupLocation1Percentage = 15, PickupLocation2Percentage = 25, PickupLocation3Percentage = 63 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 16), PickupLocation1Percentage = 15, PickupLocation2Percentage = 16, PickupLocation3Percentage = 36 },

                new PickupSchedule() { Date = new DateTime(2019, 3, 17), PickupLocation1Percentage = 24, PickupLocation2Percentage = 37, PickupLocation3Percentage = 17 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 3, 18), PickupLocation1Percentage = 58, PickupLocation2Percentage = 27, PickupLocation3Percentage = 15 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 19), PickupLocation1Percentage = 40, PickupLocation2Percentage = 22, PickupLocation3Percentage = 49 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 20), PickupLocation1Percentage = 22, PickupLocation2Percentage = 58, PickupLocation3Percentage = 14 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 21), PickupLocation1Percentage = 48, PickupLocation2Percentage = 20, PickupLocation3Percentage = 30 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 22), PickupLocation1Percentage = 17, PickupLocation2Percentage = 25, PickupLocation3Percentage = 53 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 23), PickupLocation1Percentage = 9, PickupLocation2Percentage = 17, PickupLocation3Percentage = 30 },

                new PickupSchedule() { Date = new DateTime(2019, 3, 24), PickupLocation1Percentage = 23, PickupLocation2Percentage = 37, PickupLocation3Percentage = 13 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 3, 25), PickupLocation1Percentage = 58, PickupLocation2Percentage = 31, PickupLocation3Percentage = 12 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 26), PickupLocation1Percentage = 35, PickupLocation2Percentage = 17, PickupLocation3Percentage = 53 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 27), PickupLocation1Percentage = 22, PickupLocation2Percentage = 60, PickupLocation3Percentage = 19 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 28), PickupLocation1Percentage = 46, PickupLocation2Percentage = 19, PickupLocation3Percentage = 37 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 29), PickupLocation1Percentage = 18, PickupLocation2Percentage = 30, PickupLocation3Percentage = 42 },
                new PickupSchedule() { Date = new DateTime(2019, 3, 30), PickupLocation1Percentage = 18, PickupLocation2Percentage = 19, PickupLocation3Percentage = 27 },

                new PickupSchedule() { Date = new DateTime(2019, 3, 31), PickupLocation1Percentage = 22, PickupLocation2Percentage = 30, PickupLocation3Percentage = 20 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 4, 1), PickupLocation1Percentage = 62, PickupLocation2Percentage = 25, PickupLocation3Percentage = 15 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 2), PickupLocation1Percentage = 39, PickupLocation2Percentage = 24, PickupLocation3Percentage = 49 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 3), PickupLocation1Percentage = 23, PickupLocation2Percentage = 66, PickupLocation3Percentage = 19 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 4), PickupLocation1Percentage = 50, PickupLocation2Percentage = 26, PickupLocation3Percentage = 40 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 5), PickupLocation1Percentage = 20, PickupLocation2Percentage = 31, PickupLocation3Percentage = 69 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 6), PickupLocation1Percentage = 18, PickupLocation2Percentage = 25, PickupLocation3Percentage = 38 },

                new PickupSchedule() { Date = new DateTime(2019, 4, 7), PickupLocation1Percentage = 29, PickupLocation2Percentage = 41, PickupLocation3Percentage = 16 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 4, 8), PickupLocation1Percentage = 54, PickupLocation2Percentage = 24, PickupLocation3Percentage = 14 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 9), PickupLocation1Percentage = 34, PickupLocation2Percentage = 14, PickupLocation3Percentage = 40 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 10), PickupLocation1Percentage = 23, PickupLocation2Percentage = 61, PickupLocation3Percentage = 14 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 11), PickupLocation1Percentage = 47, PickupLocation2Percentage = 30, PickupLocation3Percentage = 30 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 12), PickupLocation1Percentage = 18, PickupLocation2Percentage = 25, PickupLocation3Percentage = 62 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 13), PickupLocation1Percentage = 10, PickupLocation2Percentage = 17, PickupLocation3Percentage = 30 },

                new PickupSchedule() { Date = new DateTime(2019, 4, 14), PickupLocation1Percentage = 25, PickupLocation2Percentage = 35, PickupLocation3Percentage = 15 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 4, 15), PickupLocation1Percentage = 56, PickupLocation2Percentage = 33, PickupLocation3Percentage = 16 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 16), PickupLocation1Percentage = 35, PickupLocation2Percentage = 15, PickupLocation3Percentage = 44 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 17), PickupLocation1Percentage = 29, PickupLocation2Percentage = 59, PickupLocation3Percentage = 22 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 18), PickupLocation1Percentage = 46, PickupLocation2Percentage = 21, PickupLocation3Percentage = 32 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 19), PickupLocation1Percentage = 17, PickupLocation2Percentage = 28, PickupLocation3Percentage = 64 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 20), PickupLocation1Percentage = 13, PickupLocation2Percentage = 19, PickupLocation3Percentage = 31 },

                new PickupSchedule() { Date = new DateTime(2019, 4, 21), PickupLocation1Percentage = 20, PickupLocation2Percentage = 30, PickupLocation3Percentage = 19 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 4, 22), PickupLocation1Percentage = 59, PickupLocation2Percentage = 31, PickupLocation3Percentage = 12 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 23), PickupLocation1Percentage = 35, PickupLocation2Percentage = 16, PickupLocation3Percentage = 40 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 24), PickupLocation1Percentage = 20, PickupLocation2Percentage = 64, PickupLocation3Percentage = 15 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 25), PickupLocation1Percentage = 44, PickupLocation2Percentage = 19, PickupLocation3Percentage = 31 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 26), PickupLocation1Percentage = 12, PickupLocation2Percentage = 31, PickupLocation3Percentage = 53 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 27), PickupLocation1Percentage = 18, PickupLocation2Percentage = 18, PickupLocation3Percentage = 35 },

                new PickupSchedule() { Date = new DateTime(2019, 4, 28), PickupLocation1Percentage = 19, PickupLocation2Percentage = 34, PickupLocation3Percentage = 12 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 4, 29), PickupLocation1Percentage = 55, PickupLocation2Percentage = 26, PickupLocation3Percentage = 25 },
                new PickupSchedule() { Date = new DateTime(2019, 4, 30), PickupLocation1Percentage = 35, PickupLocation2Percentage = 22, PickupLocation3Percentage = 38 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 1), PickupLocation1Percentage = 21, PickupLocation2Percentage = 61, PickupLocation3Percentage = 12 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 2), PickupLocation1Percentage = 49, PickupLocation2Percentage = 24, PickupLocation3Percentage = 34 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 3), PickupLocation1Percentage = 15, PickupLocation2Percentage = 24, PickupLocation3Percentage = 60 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 4), PickupLocation1Percentage = 16, PickupLocation2Percentage = 24, PickupLocation3Percentage = 37 },

                new PickupSchedule() { Date = new DateTime(2019, 5, 5), PickupLocation1Percentage = 27, PickupLocation2Percentage = 37, PickupLocation3Percentage = 18 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 5, 6), PickupLocation1Percentage = 66, PickupLocation2Percentage = 35, PickupLocation3Percentage = 20 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 7), PickupLocation1Percentage = 42, PickupLocation2Percentage = 24, PickupLocation3Percentage = 49 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 8), PickupLocation1Percentage = 25, PickupLocation2Percentage = 69, PickupLocation3Percentage = 19 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 9), PickupLocation1Percentage = 40, PickupLocation2Percentage = 26, PickupLocation3Percentage = 39 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 10), PickupLocation1Percentage = 15, PickupLocation2Percentage = 26, PickupLocation3Percentage = 57 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 11), PickupLocation1Percentage = 10, PickupLocation2Percentage = 17, PickupLocation3Percentage = 26 },

                new PickupSchedule() { Date = new DateTime(2019, 5, 12), PickupLocation1Percentage = 27, PickupLocation2Percentage = 36, PickupLocation3Percentage = 14 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 5, 13), PickupLocation1Percentage = 61, PickupLocation2Percentage = 25, PickupLocation3Percentage = 12 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 14), PickupLocation1Percentage = 35, PickupLocation2Percentage = 22, PickupLocation3Percentage = 46 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 15), PickupLocation1Percentage = 23, PickupLocation2Percentage = 61, PickupLocation3Percentage = 12 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 16), PickupLocation1Percentage = 47, PickupLocation2Percentage = 18, PickupLocation3Percentage = 30 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 17), PickupLocation1Percentage = 22, PickupLocation2Percentage = 22, PickupLocation3Percentage = 59 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 18), PickupLocation1Percentage = 14, PickupLocation2Percentage = 19, PickupLocation3Percentage = 30 },

                new PickupSchedule() { Date = new DateTime(2019, 5, 19), PickupLocation1Percentage = 25, PickupLocation2Percentage = 35, PickupLocation3Percentage = 15 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 5, 20), PickupLocation1Percentage = 58, PickupLocation2Percentage = 28, PickupLocation3Percentage = 11 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 21), PickupLocation1Percentage = 37, PickupLocation2Percentage = 18, PickupLocation3Percentage = 42 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 22), PickupLocation1Percentage = 18, PickupLocation2Percentage = 62, PickupLocation3Percentage = 12 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 23), PickupLocation1Percentage = 42, PickupLocation2Percentage = 25, PickupLocation3Percentage = 31 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 24), PickupLocation1Percentage = 15, PickupLocation2Percentage = 35, PickupLocation3Percentage = 59 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 25), PickupLocation1Percentage = 11, PickupLocation2Percentage = 17, PickupLocation3Percentage = 31 },

                new PickupSchedule() { Date = new DateTime(2019, 5, 26), PickupLocation1Percentage = 28, PickupLocation2Percentage = 32, PickupLocation3Percentage = 16 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 5, 27), PickupLocation1Percentage = 61, PickupLocation2Percentage = 23, PickupLocation3Percentage = 15 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 28), PickupLocation1Percentage = 34, PickupLocation2Percentage = 19, PickupLocation3Percentage = 45 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 29), PickupLocation1Percentage = 24, PickupLocation2Percentage = 52, PickupLocation3Percentage = 20 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 30), PickupLocation1Percentage = 41, PickupLocation2Percentage = 18, PickupLocation3Percentage = 37 },
                new PickupSchedule() { Date = new DateTime(2019, 5, 31), PickupLocation1Percentage = 16, PickupLocation2Percentage = 24, PickupLocation3Percentage = 58 },
                new PickupSchedule() { Date = new DateTime(2019, 6, 1), PickupLocation1Percentage = 12, PickupLocation2Percentage = 19, PickupLocation3Percentage = 36 },

                new PickupSchedule() { Date = new DateTime(2019, 6, 2), PickupLocation1Percentage = 32, PickupLocation2Percentage = 29, PickupLocation3Percentage = 11 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 6, 3), PickupLocation1Percentage = 68, PickupLocation2Percentage = 29, PickupLocation3Percentage = 21 },
                new PickupSchedule() { Date = new DateTime(2019, 6, 4), PickupLocation1Percentage = 44, PickupLocation2Percentage = 26, PickupLocation3Percentage = 51 },
                new PickupSchedule() { Date = new DateTime(2019, 6, 5), PickupLocation1Percentage = 28, PickupLocation2Percentage = 69, PickupLocation3Percentage = 17 },
                new PickupSchedule() { Date = new DateTime(2019, 6, 6), PickupLocation1Percentage = 54, PickupLocation2Percentage = 30, PickupLocation3Percentage = 38 },
                new PickupSchedule() { Date = new DateTime(2019, 6, 7), PickupLocation1Percentage = 24, PickupLocation2Percentage = 30, PickupLocation3Percentage = 79 },
                new PickupSchedule() { Date = new DateTime(2019, 6, 8), PickupLocation1Percentage = 20, PickupLocation2Percentage = 25, PickupLocation3Percentage = 38 },

                new PickupSchedule() { Date = new DateTime(2019, 6, 9), PickupLocation1Percentage = 20, PickupLocation2Percentage = 28, PickupLocation3Percentage = 17 }, // Sunday
                new PickupSchedule() { Date = new DateTime(2019, 6, 10), PickupLocation1Percentage = 55, PickupLocation2Percentage = 28, PickupLocation3Percentage = 15 }
                //new PickupSchedule() { Date = new DateTime(2019, 6, 11), PickupLocation1Percentage = 34, PickupLocation2Percentage = 20, PickupLocation3Percentage = 45 }
            };
            #endregion

            var trips = new List<TripItem>();
            foreach (var day in schedule)
            {
                // Loop through each of the three locations (PickupLocationXPercentage / averageRecordsPerDay) to create records for those pickup points for that day.
                // Randomly set the time value.
                var numLocation1Trips = Math.Round((day.PickupLocation1Percentage / 100) * averageRecordsPerDay, 0);
                var numLocation2Trips = Math.Round((day.PickupLocation2Percentage / 100) * averageRecordsPerDay, 0);
                var numLocation3Trips = Math.Round((day.PickupLocation3Percentage / 100) * averageRecordsPerDay, 0);

                for (var i = 0; i < numLocation1Trips; i++)
                {
                    trips.Add(GenerateTripItem(day.Date, pickupLocations[0]));
                }
                for (var i = 0; i < numLocation2Trips; i++)
                {
                    trips.Add(GenerateTripItem(day.Date, pickupLocations[1]));
                }
                for (var i = 0; i < numLocation3Trips; i++)
                {
                    trips.Add(GenerateTripItem(day.Date, pickupLocations[2]));
                }
            }

            // Log list of trips.
            //Console.WriteLine(JsonConvert.SerializeObject(trips, Formatting.Indented));

            int count = 1;
            var serializer = new JsonSerializer();
            foreach (var trip in trips)
            {
                using (var file = File.CreateText($@"{count}.json"))
                {
                    //serialize object directly into file stream
                    serializer.Serialize(file, trip);
                }

                count++;
            }

            #region Internal methods
            TripLocation GetRandomDestination()
            {
                return destinations[random.Next(0, destinations.Count-1)];
            }

            DriverItem GetRandomDriver()
            {
                return drivers[random.Next(0, drivers.Count-1)];
            }

            List<DriverItem> GetRandomDrivers()
            {
                var selectedDrivers = new List<DriverItem>();
                var numDrivers = random.Next(3, 12);

                for (var i = 0; i < numDrivers; i++)
                {
                    var filtered = drivers.Where(d => !selectedDrivers.Contains(d)).ToList();
                    selectedDrivers.Add(filtered[random.Next(0, filtered.Count-1)]);
                }

                return selectedDrivers;
            }

            PassengerItem GetRandomPassenger()
            {
                return passengers[random.Next(0, drivers.Count-1)];
            }

            TripItem GenerateTripItem(DateTime day, PickupLocation source)
            {
                // How long the trip lasted.
                var tripDuration = random.Next(3, 80);
                // How long it took for a driver to accept the trip.
                var driverAcceptedTripDelay = random.Next(1, 3);
                // The time of day trips should begin.
                var tripWindowStart = TimeSpan.FromHours(4);
                // The time of day trips should end.
                var tripWindowEnd = TimeSpan.FromHours(23);
                var maxMinutes = (int)((tripWindowEnd - tripWindowStart).TotalMinutes);
                // Trip start time indicates when the request for a new trip was created.
                var tripStartTime = tripWindowStart.Add(TimeSpan.FromMinutes(random.Next(0, maxMinutes)));
                var tripStartDateTime = day.AddMinutes(tripStartTime.TotalMinutes);
                var driverAcceptedDateTime = tripStartDateTime.AddMinutes(driverAcceptedTripDelay);
                var tripEndDateTime = driverAcceptedDateTime.AddMinutes(tripDuration);

                return new TripItem
                {
                    Code = Utilities.GenerateRandomAlphaNumeric(8),
                    AcceptDate = driverAcceptedDateTime,
                    AvailableDrivers = GetRandomDrivers(),
                    Destination = GetRandomDestination(),
                    Driver = GetRandomDriver(),
                    Duration = tripDuration,
                    Passenger = GetRandomPassenger(),
                    Source = new TripLocation { Latitude = source.Latitude, Longitude = source.Longitude },
                    StartDate = tripStartDateTime,
                    EndDate = tripEndDateTime
                };
            }
            #endregion

            Console.WriteLine("Seed completed......press any key!");
            Console.ReadLine();
        }

        class PickupLocation
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        class PickupSchedule
        {
            public DateTime Date { get; set; }
            public double PickupLocation1Percentage { get; set; }
            public double PickupLocation2Percentage { get; set; }
            public double PickupLocation3Percentage { get; set; }
        }
    }
}
