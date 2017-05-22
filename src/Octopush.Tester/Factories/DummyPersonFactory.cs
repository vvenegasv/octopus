using Octopus.Tester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octopus.Tester.Factories
{
    internal class DummyPersonFactory
    {
        private static volatile DummyPersonFactory _instance;
        private static object _syncRoot = new Object();
        private static Random _random = new Random(DateTime.Now.Millisecond);
        private static readonly IReadOnlyList<string> _names = new List<string>() { "Viki Vong", "Brinda Bookout", "Janean Jonas", "Von Vankeuren", "Katelyn Kulik", "Verona Valdez", "Arletha Ashbaugh", "Bong Bellew", "Ilona Irving", "Refugia Radney", "Lili Lechner", "Carl Cowens", "Shanae Sleeper", "Enedina Etter", "Luke Lemmer", "Leota Legree", "Liliana Locicero", "Clementina Cline", "Reed Rivenburg", "Solange Schartz", "Myrtle Martin", "Meghan Mikkelsen", "Shari Santana", "Stacie Selman", "Linsey Lofgren", "Dudley Donley", "Oleta Oleson", "Zandra Zick", "Dedra Durfee", "Zulema Zubia", "Maribel Mclane", "Breana Brannigan", "Mitchel Millikin", "Gregoria Gladstone", "Cristen Currey", "Yajaira Yamada", "Eufemia Easterday", "Randall Red", "Salley Saiz", "Hisako Heber", "Hunter Halbert", "Shawnta Selig", "Tracey Tannenbaum", "Alyce Apel", "Mariam Mccullen", "Elenore Etherton", "Merri Mood", "Sherrill Sakamoto", "Zonia Zwick", "Lauran Laforge", "Hilario Hartin", "Monserrate Marton", "Karole Koprowski", "Otha Ocasio", "Shaneka Sweeney", "Rowena Rinaldo", "Carylon Chamlee", "Corinna Coan", "Eun Eastin", "Carissa Christopherso", "Jeanine Juckett", "Niesha Nevius", "Goldie Graig", "Yessenia Yoshioka", "Berenice Baney", "Caroline Casper", "Trent Takahashi", "Enrique Esposito", "Senaida Shumway", "Nola Neill", "Seema Schuyler", "Sol Smithers", "Lynsey Lary", "Felipe Furman", "Drew Dison", "Ying Yearby", "Arianne Attaway", "Sabina Sidle", "Terrilyn Tello", "Sherrie Simonetti", "Leontine Layton", "Manuel Marmolejo", "Zofia Zajicek", "Mable Mayle", "Catina Chavous", "Monica Minjares", "Anika Alford", "Vanna Vaccaro", "Yolonda Yamauchi", "Jenine Jacinto", "Minta Matarazzo", "Lesli Lucus", "Lezlie Lehner", "Cris Coverdale", "Marquis Marquardt", "Jasmine Jeanbaptiste", "Howard Hemstreet", "Helga Hardnett", "Erik Edmonson", "Jenice Josephson", "Cathryn Crosby", "Lidia Loughlin", "Amira Amburn", "Shay Sisto", "Augustine Atha", "Terresa Tarantino", "Sharonda Schebler", "Ciara Clifton", "Ines Izzo", "Janella Jacinto", "Alexa Ammann", "Janina Janis", "Brinda Bernstein", "Vicente Volker", "Toby Tucker", "Delmer Dunsmore", "Tijuana Traywick", "Elmo Ellingson", "Karisa Koogler", "Shannan Schmucker", "Gustavo Gillan", "Erna Evelyn", "Nickolas Neuendorf", "Jackeline Jenny", "Ok Ohlinger", "Jacquelyn Jandreau", "Casandra Correll", "Deandrea Doke", "Chia Crochet", "Marylin Maynard", "Roland Ratcliff", "Carina Cuffee", "Everett Exley", "Leesa Lora", "Kendra Ketter", "Jenny Jetter", "Lenard Lester", "Mayola Mitton", "Johna Jeske", "Jason Janco", "Floy Freeland", "Domenica Duggan", "Alena Ackerman", "Lynelle Larrimore", "Teena Taillon", "Billye Bowes", "Kati Kantner", "Tonie Taub", "August Addison", "Elizabet Eckhoff", };
        

        private DummyPersonFactory() { }

        public static DummyPersonFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new DummyPersonFactory();
                    }
                }
                return _instance;
            }
        }

        public DummyPerson Make()
        {
            return new DummyPerson
            {
                BirthDate = new DateTime(_random.Next(1950, DateTime.Now.Year), _random.Next(1, 12), _random.Next(1, 28)),
                Name = _names[_random.Next(0, _names.Count() - 1)]
            };
        }

        public DummyPerson Clone(DummyPerson source)
        {
            return new DummyPerson
            {
                BirthDate = source.BirthDate,
                Name = source.Name,
                PersonId = source.PersonId
            };
        }
    }
}
