using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Google.Cloud.Firestore;
using System.IO;
using FirestoreDocumentReference = Google.Cloud.Firestore.DocumentReference;


namespace Beurscafe
{
    public class FirebaseManager
    {
        private FirestoreDb _firestoreDb;

        public FirebaseManager()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "beurscafe-4d2ec-firebase-adminsdk-bpzxc-fe2fdd0547.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            _firestoreDb = FirestoreDb.Create("beurscafe-4d2ec");
        }

        // Example method to add a new drink to Firestore
        public async Task AddDrinkToFirestore(Drinks drink)
        {
            // Use the full namespace here to avoid ambiguity
            Google.Cloud.Firestore.DocumentReference docRef = _firestoreDb.Collection("Drinks").Document(drink.Name);

            Dictionary<string, object> drinkData = new Dictionary<string, object>
    {
        { "Name", drink.Name },
        { "MinPrice", drink.MinPrice },
        { "MaxPrice", drink.MaxPrice },
        { "CurrentPrice", drink.CurrentPrice },
        { "Orders", drink.Orders }
    };

            await docRef.SetAsync(drinkData);
        }


        // Method to fetch drinks from Firestore
        public async Task<List<Drinks>> GetDrinksFromFirestore()
        {
            List<Drinks> drinksList = new List<Drinks>();

            QuerySnapshot snapshot = await _firestoreDb.Collection("Drinks").GetSnapshotAsync();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                Dictionary<string, object> drinkData = document.ToDictionary();

                string name = drinkData["Name"].ToString();
                double minPrice = Convert.ToDouble(drinkData["MinPrice"]);
                double maxPrice = Convert.ToDouble(drinkData["MaxPrice"]);
                double currentPrice = Convert.ToDouble(drinkData["CurrentPrice"]);

                Drinks drink = new Drinks(name, minPrice, maxPrice, currentPrice);
                drinksList.Add(drink);
            }

            return drinksList;
        }

        public async Task DeleteDrinkFromFirestore(string drinkName)
        {
            Google.Cloud.Firestore.DocumentReference docRef = _firestoreDb.Collection("Drinks").Document(drinkName);
            await docRef.DeleteAsync();
        }


    }
}
