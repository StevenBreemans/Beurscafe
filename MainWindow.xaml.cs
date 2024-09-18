using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using System.Media;  // Add this for SoundPlayer

namespace Beurscafe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer = new DispatcherTimer();
        private TimeSpan timeRemaining;

        // List to store all drinks
        private List<Drinks> drinksList = new List<Drinks>();
        // List to store the ordered drinks with their separate order counts
        private List<OrderedDrink> orderedDrinks = new List<OrderedDrink>();
        private bool isTimerPopupOpen = false;


        private int lastSelectedIndex = 0;
        private TimeSpan defaultTimeRemaining = TimeSpan.FromMinutes(5);  // Default 5-minute timer
        private TimeSpan originalTimeRemaining;  // Store the original value to reset the timer
        private bool customTimerSet = false;     // Track if the custom timer is set
        private Drinks tempNewDrink = null;  // To track the temporary new drink

        private FirebaseManager firebaseManager = new FirebaseManager();  // Initialize FirebaseManager

        // Add flag to prevent simultaneous updates
        private bool syncingWithFirestore = false;


        public MainWindow()
        {
            InitializeComponent();

            // Set the timer to 5 minutes initially
            timeRemaining = defaultTimeRemaining;
            UpdateTimerDisplay();  // Update the TabItem header with the initial time
            StartTimer();
            PopulateOrderDrinksTab();
            UpdateOrderCountDisplay();  // Initialize the order count display

            // Asynchronously fetch drinks from Firestore and update the UI
            LoadDrinksFromFirestore();

            // Listen for Firestore updates
            firebaseManager.ListenToTimerChanges(OnFirestoreTimerUpdate);
        }

        // Method to load drinks from Firestore and populate the UI
        private async void LoadDrinksFromFirestore()
        {
            try
            {
                // Fetch the drinks from Firestore
                var fetchedDrinks = await firebaseManager.GetDrinksFromFirestore();

                // If there are no drinks, show a message (optional)
                if (fetchedDrinks.Count == 0)
                {
                    MessageBox.Show("No drinks found in Firestore.");
                    return;
                }

                // Clear the current drinks list
                drinksList.Clear();

                // Add fetched drinks to the local drinks list
                drinksList.AddRange(fetchedDrinks);

                // Once drinks are fetched, update the UI
                PopulateOrderDrinksTab();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching drinks from Firestore: " + ex.Message);
            }
        }

        private async void LoadTimerFromFirestore()
        {
            var (firestoreTimeRemaining, firestoreResetTime) = await firebaseManager.GetTimerFromFirestore();

            // Calculate the elapsed time since the reset
            TimeSpan elapsedTime = DateTime.UtcNow - firestoreResetTime;
            timeRemaining = TimeSpan.FromSeconds(Math.Max(firestoreTimeRemaining - elapsedTime.TotalSeconds, 0));

            // Update the timer display
            UpdateTimerDisplay();

            // Start the timer
            StartTimer();
        }

        // Firestore Listener Update handler
        private void OnFirestoreTimerUpdate(int newTimeRemaining, DateTime resetTime)
        {
            // Prevent local updates while syncing with Firestore
            syncingWithFirestore = true;

            TimeSpan elapsedTime = DateTime.UtcNow - resetTime;
            TimeSpan updatedTime = TimeSpan.FromSeconds(Math.Max(newTimeRemaining - elapsedTime.TotalSeconds, 0));

            // Only update the timer if there's a significant difference (2 seconds)
            if (Math.Abs((updatedTime - timeRemaining).TotalSeconds) > 2)
            {
                timeRemaining = updatedTime;
                UpdateTimerDisplay();
            }

            // Sync complete, allow local updates again
            syncingWithFirestore = false;
        }
        // Start the timer, ensuring single event attachment
        private void StartTimer()
        {
            timer.Tick -= Timer_Tick; // Ensure the event handler is only attached once
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick; // Attach the event handler
            timer.Start();
        }


        // Method to play the sound when the timer reaches 0
        private void PlaySound()
        {
            try
            {
                // Assuming the .wav file is in the Resources folder
                string soundFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "WallStreetOpeningBell.wav");

                // Create a new SoundPlayer and play the sound
                SoundPlayer player = new SoundPlayer(soundFilePath);
                player.Play();  // Play the sound asynchronously
            }
            catch (Exception ex)
            {
                // Handle any issues like missing file
                MessageBox.Show("Error playing sound: " + ex.Message);
            }
        }


        // Timer Tick event handler (local timer)
        // Timer Tick event handler (local timer)
        private async void Timer_Tick(object? sender, EventArgs e)
        {
            if (syncingWithFirestore) return; // Skip the local update if syncing with Firestore

            if (timeRemaining > TimeSpan.Zero)
            {
                timeRemaining = timeRemaining.Add(TimeSpan.FromSeconds(-1));
                UpdateTimerDisplay(); // Local timer update

                // Update Firestore with the new time every second
                await firebaseManager.UpdateTimerInFirestore((int)timeRemaining.TotalSeconds);
            }
            else
            {
                // Timer reaches zero: play sound, adjust prices, reset orders, reset timer
                PlaySound();
                AdjustPrices();
                ResetOrders();
                timeRemaining = customTimerSet ? originalTimeRemaining : defaultTimeRemaining;

                PopulateEditDrinksTab(); // Refresh the Edit Drinks tab to reflect price changes

                // Sync the reset timer with Firestore
                await firebaseManager.UpdateTimerInFirestore((int)timeRemaining.TotalSeconds);
                UpdateTimerDisplay();
            }
        }


        // Reset orders for all drinks in the orderedDrinks list
        private async void ResetOrders()
        {
            orderedDrinks.Clear();  // Clear the ordered drinks list

            foreach (var drink in drinksList)
            {
                drink.Orders = 0;  // Reset the order count for each drink in the drinks list

                // Update Firestore to reset the orders for each drink
                await firebaseManager.UpdateDrinkOrdersInFirestore(drink.Name, drink.Orders);
            }

            UpdateOrderCountDisplay();  // Update the right-side display after resetting
        }




        // Method to update the TimerTabItem's header dynamically (UI thread-safe)
        private void UpdateTimerDisplay()
        {
            if (Dispatcher.CheckAccess())
            {
                // Update the timer display directly if on UI thread
                TabItem timerTabItem = (TabItem)MainTabControl.Items[2]; // Adjust the index if necessary
                timerTabItem.Header = $"Resterende tijd: {timeRemaining:mm\\:ss}";
            }
            else
            {
                // Safely update the UI from a non-UI thread
                Dispatcher.Invoke(() =>
                {
                    TabItem timerTabItem = (TabItem)MainTabControl.Items[2]; // Adjust the index if necessary
                    timerTabItem.Header = $"Resterende tijd: {timeRemaining:mm\\:ss}";
                });
            }
        }


        // Event handler for TabControl SelectionChanged
        private async void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If the selected tab is the TimerTabItem and the popup is not open
            if (MainTabControl.SelectedItem == TimerTabItem && !isTimerPopupOpen)
            {
                // Set the flag to indicate that the popup is currently open
                isTimerPopupOpen = true;

                // Immediately switch back to the last selected tab
                MainTabControl.SelectedIndex = lastSelectedIndex;

                // Show the dialog for adjusting the timer
                string input = Microsoft.VisualBasic.Interaction.InputBox("Enter new time in minutes:", "Adjust Timer", "5");

                // Handle the case where the user clicks "Cancel" or provides no input
                if (!string.IsNullOrEmpty(input))
                {
                    // Try parsing the input
                    if (int.TryParse(input, out int newMinutes))
                    {
                        // Set the new remaining time based on user input (Use TimeSpan.FromMinutes instead of FromSeconds)
                        timeRemaining = TimeSpan.FromSeconds(newMinutes);
                        UpdateTimerDisplay();  // Update the header with the new time

                        // Save the original custom time
                        originalTimeRemaining = timeRemaining;
                        customTimerSet = true;  // Mark that a custom timer is set

                        // Update Firestore with the new timer value
                        await firebaseManager.UpdateTimerInFirestore((int)timeRemaining.TotalSeconds);
                    }
                    else
                    {
                        // Show the message only if the input was invalid (not empty or cancel)
                        MessageBox.Show("Invalid input. Please enter a valid number.");
                    }
                }

                // After the dialog interaction, reset the flag to allow future popups
                isTimerPopupOpen = false;
            }
            else
            {
                // Save the currently selected tab index when any tab other than the TimerTabItem is selected
                lastSelectedIndex = MainTabControl.SelectedIndex;

                // Handle other tab actions normally
                if (MainTabControl.SelectedIndex == 1)  // View Edit Drinks tab (Index 1)
                {
                    PopulateEditDrinksTab();
                }
                else if (MainTabControl.SelectedIndex == 0)  // Order Drinks tab (Index 0)
                {
                    PopulateOrderDrinksTab();  // Refresh the Order Drinks tab
                }

                // Reset the flag if switching to any other tab
                isTimerPopupOpen = false;
            }

            // Automatically discard any unsaved drink when switching away from the View/Edit Drinks tab
            if (tempNewDrink != null && MainTabControl.SelectedIndex != 1)
            {
                // Discard the new drink automatically without confirmation
                tempNewDrink = null;
                PopulateEditDrinksTab();  // Refresh the Edit Drinks tab to remove the unsaved row
            }
        }
        private int roundsWithoutOrders = 0;  // Variable to track consecutive rounds without orders

        private void HandleNoOrders(int roundsWithoutOrders, Random random, StringBuilder priceChangesMessage)
        {
            var notOrderedDrinks = drinksList.Where(d => d.Orders == 0).ToList();

            if (roundsWithoutOrders < 3)
            {
                HandleFirstTwoRoundsWithoutOrders(random, priceChangesMessage, notOrderedDrinks);
            }
            else if (roundsWithoutOrders == 3)
            {
                HandleThirdRoundWithoutOrders(random, priceChangesMessage);
            }
            else if (roundsWithoutOrders >= 4)
            {
                HandleFourOrMoreRoundsWithoutOrders(random, priceChangesMessage);
            }
        }
        private void HandleFirstTwoRoundsWithoutOrders(Random random, StringBuilder priceChangesMessage, List<Drinks> notOrderedDrinks)
        {
            var eligibleDrinksForMinPrice = notOrderedDrinks.Where(d => d.CurrentPrice <= d.MaxPrice - 0.5).ToList();
            Drinks selectedDrink = null;

            // 40% chance to reduce one drink to its minimum price
            if (eligibleDrinksForMinPrice.Count > 0 && random.NextDouble() <= 0.40)
            {
                selectedDrink = eligibleDrinksForMinPrice[random.Next(eligibleDrinksForMinPrice.Count)];
                priceChangesMessage.AppendLine($"{selectedDrink.Name}: heeft een kans van 40% en is verlaagd naar de minimumprijs.");
                selectedDrink.CurrentPrice = selectedDrink.MinPrice;
            }

            // 55% chance to perform price adjustments for the rest of the drinks
            if (random.NextDouble() <= 0.85)
            {
                foreach (var drink in notOrderedDrinks.Where(d => d != selectedDrink))
                {
                    double decrease;

                    // 15% chance to decrease between 1.0 and 2.0
                    if (random.NextDouble() <= 0.15)
                    {
                        decrease = GetRandomNumber(1.0, 2.0, random);
                        priceChangesMessage.AppendLine($"{drink.Name}: kans van 15% en is gedaald tussen 1.0 en 2.0.");
                    }
                    // 85% chance to decrease between 0.1 and 1.2
                    else
                    {
                        decrease = GetRandomNumber(0.2, 1.2, random);
                        priceChangesMessage.AppendLine($"{drink.Name}: kans van 85% en is gedaald tussen 0.1 en 1.5.");
                    }

                    double oldPrice = drink.CurrentPrice.Value;
                    drink.CurrentPrice = Math.Max(RoundDown(drink.CurrentPrice.Value - decrease), drink.MinPrice.Value);
                    priceChangesMessage.AppendLine($"{drink.Name}: gedaald met {oldPrice - drink.CurrentPrice.Value:F2} EUR.");
                }
            }
            else
            {
                // No adjustments for this round
                priceChangesMessage.AppendLine("Geen prijsaanpassingen voor niet-bestelde drankjes deze ronde.");
            }
        }


        private void HandleThirdRoundWithoutOrders(Random random, StringBuilder priceChangesMessage)
        {
            priceChangesMessage.AppendLine("Er zijn 3 rondes geen bestellingen geplaatst. Eén willekeurig drankje wordt verlaagd naar de minimumprijs, en andere geschikte drankjes worden aangepast naar hun gemiddelde prijs of tot 0,5 onder de gemiddelde prijs.");

            // Find all drinks that haven't been ordered
            var notOrderedDrinksForThreeRounds = drinksList.Where(d => d.Orders == 0).ToList();

            // Find eligible drinks for adjustments (those above average)
            var eligibleDrinksForAdjustment = notOrderedDrinksForThreeRounds
                .Where(d => d.CurrentPrice > d.MinPrice && d.CurrentPrice > (d.MinPrice + d.MaxPrice) / 2)
                .ToList();

            // Check if there are eligible drinks
            if (notOrderedDrinksForThreeRounds.Count > 0)
            {
                // Initialize a variable to hold the selected drink
                Drinks selectedDrink = null;

                // Try to find a drink that is not at the minimum price
                for (int i = 0; i < notOrderedDrinksForThreeRounds.Count; i++)
                {
                    selectedDrink = notOrderedDrinksForThreeRounds[random.Next(notOrderedDrinksForThreeRounds.Count)];
                    if (selectedDrink.CurrentPrice > selectedDrink.MinPrice)
                    {
                        break; // Found a valid drink to set to min price
                    }
                }

                // If a valid drink was found, set it to the minimum price
                if (selectedDrink != null && selectedDrink.CurrentPrice > selectedDrink.MinPrice)
                {
                    selectedDrink.CurrentPrice = selectedDrink.MinPrice;
                    priceChangesMessage.AppendLine($"{selectedDrink.Name}: verlaagd naar de minimumprijs.");
                }
                else
                {
                    priceChangesMessage.AppendLine("Geen geschikt drankje gevonden om naar de minimumprijs te verlagen.");
                }

                // Adjust the other drinks that are eligible for adjustment, excluding the selected drink
                foreach (var drink in eligibleDrinksForAdjustment.Where(d => d != selectedDrink))
                {
                    double averagePrice = (drink.MinPrice.Value + drink.MaxPrice.Value) / 2;

                    // Set the price to either the average or a random value between 0.5 below the average and the average
                    double adjustment = GetRandomNumber(averagePrice - 0.7, averagePrice, random);
                    drink.CurrentPrice = RoundDown(adjustment); // Use rounding down logic

                    priceChangesMessage.AppendLine($"{drink.Name}: aangepast naar {drink.CurrentPrice:F2} EUR (tussen 0,7 onder en de gemiddelde prijs).");
                }
            }
            else
            {
                priceChangesMessage.AppendLine("Er zijn geen geschikte drankjes om naar de minimumprijs of gemiddelde prijs te verlagen.");
            }
        }




        private void HandleFourOrMoreRoundsWithoutOrders(Random random, StringBuilder priceChangesMessage)
        {
            priceChangesMessage.AppendLine("Er zijn 4 of meer rondes geen bestellingen geplaatst. Alle geschikte drankjes worden ingesteld op hun gemiddelde prijs of tot 0,5 onder de gemiddelde prijs.");

            // Find all eligible drinks
            var eligibleDrinks = drinksList.Where(d => d.Orders == 0 && d.CurrentPrice > d.MinPrice && d.CurrentPrice > (d.MinPrice + d.MaxPrice) / 2).ToList();

            // Check if there are any eligible drinks
            if (eligibleDrinks.Count > 0)
            {
                // Adjust each eligible drink to either the average price or a random value 0.5 below the average
                foreach (var drink in eligibleDrinks)
                {
                    double averagePrice = (drink.MinPrice.Value + drink.MaxPrice.Value) / 2;

                    // Set the price to either the average or a random value between 0.5 below the average and the average
                    double adjustment = GetRandomNumber(averagePrice - 0.5, averagePrice, random);
                    drink.CurrentPrice = RoundDown(adjustment); // Use rounding down logic

                    priceChangesMessage.AppendLine($"{drink.Name}: aangepast naar {drink.CurrentPrice:F2} EUR (tussen 0,5 onder en de gemiddelde prijs).");
                }
            }
            else
            {
                priceChangesMessage.AppendLine("Er zijn geen geschikte drankjes om aan te passen.");
            }
        }


        private void HandleOrderedDrinksPriceAdjustment(List<Drinks> sortedDrinks, StringBuilder priceChangesMessage, Random random)
        {
            if (sortedDrinks.Count > 0 && sortedDrinks[0].Orders >= 1)
            {
                AdjustPriceForMostOrderedDrink(sortedDrinks[0], priceChangesMessage, random);
            }

            if (sortedDrinks.Count > 1 && sortedDrinks[1].Orders >= 1)
            {
                AdjustPriceForSecondMostOrderedDrink(sortedDrinks[1], priceChangesMessage, random);
            }

            if (sortedDrinks.Count > 2 && sortedDrinks[2].Orders >= 1)
            {
                AdjustPriceForThirdMostOrderedDrink(sortedDrinks[2], priceChangesMessage, random);
            }

            AdjustRemainingDrinks(sortedDrinks.Skip(3).ToList(), priceChangesMessage, random);
        }


        private void AdjustPriceForMostOrderedDrink(Drinks mostOrderedDrink, StringBuilder priceChangesMessage, Random random)
        {
            double increase = GetRandomNumber(0.8, mostOrderedDrink.MaxPrice.Value - mostOrderedDrink.CurrentPrice.Value, random);
            double oldPrice = mostOrderedDrink.CurrentPrice.Value;
            mostOrderedDrink.CurrentPrice = Math.Min(RoundUp(mostOrderedDrink.CurrentPrice.Value + increase), mostOrderedDrink.MaxPrice.Value);
            priceChangesMessage.AppendLine($"{mostOrderedDrink.Name}: gestegen met {mostOrderedDrink.CurrentPrice.Value - oldPrice:F2} EUR.");
        }

        private void AdjustPriceForSecondMostOrderedDrink(Drinks secondMostOrderedDrink, StringBuilder priceChangesMessage, Random random)
        {
            double increase = GetRandomNumber(0.4, secondMostOrderedDrink.MaxPrice.Value - 0.5 - secondMostOrderedDrink.CurrentPrice.Value, random);
            double oldPrice = secondMostOrderedDrink.CurrentPrice.Value;
            secondMostOrderedDrink.CurrentPrice = Math.Min(RoundUp(secondMostOrderedDrink.CurrentPrice.Value + increase), secondMostOrderedDrink.MaxPrice.Value - 0.5);
            priceChangesMessage.AppendLine($"{secondMostOrderedDrink.Name}: gestegen met {secondMostOrderedDrink.CurrentPrice.Value - oldPrice:F2} EUR.");
        }
        private void AdjustPriceForThirdMostOrderedDrink(Drinks thirdMostOrderedDrink, StringBuilder priceChangesMessage, Random random)
        {
            double increase = GetRandomNumber(0.1, 1.0, random);
            double oldPrice = thirdMostOrderedDrink.CurrentPrice.Value;
            thirdMostOrderedDrink.CurrentPrice = Math.Min(RoundUp(thirdMostOrderedDrink.CurrentPrice.Value + increase), thirdMostOrderedDrink.MaxPrice.Value);
            priceChangesMessage.AppendLine($"{thirdMostOrderedDrink.Name}: gestegen met {thirdMostOrderedDrink.CurrentPrice.Value - oldPrice:F2} EUR.");
        }


        private void AdjustRemainingDrinks(List<Drinks> remainingDrinks, StringBuilder priceChangesMessage, Random random)
        {
            foreach (var drink in remainingDrinks.Where(d => d.Orders >= 1))
            {
                    double adjustment = GetRandomNumber(-0.8, 0.8, random);
                    double oldPrice = drink.CurrentPrice.Value;

                    if (adjustment > 0)
                    {
                        drink.CurrentPrice = Math.Min(RoundUp(drink.CurrentPrice.Value + adjustment), drink.MaxPrice.Value);
                        priceChangesMessage.AppendLine($"{drink.Name}: gestegen met {drink.CurrentPrice.Value - oldPrice:F2} EUR.");
                    }
                    else
                    {
                        drink.CurrentPrice = Math.Max(RoundDown(drink.CurrentPrice.Value + adjustment), drink.MinPrice.Value);
                        priceChangesMessage.AppendLine($"{drink.Name}: gedaald met {oldPrice - drink.CurrentPrice.Value:F2} EUR.");
                    }
            }
        }

        private async void AdjustPrices()
        {
            Random random = new Random();
            StringBuilder priceChangesMessage = new StringBuilder();
            priceChangesMessage.AppendLine("Prijswijzigingen:");

            var sortedDrinks = drinksList.OrderByDescending(d => d.Orders).ToList();
            bool noOrdersPlaced = sortedDrinks.All(d => d.Orders == 0);

            if (noOrdersPlaced) roundsWithoutOrders++;
            else roundsWithoutOrders = 0;

            HandleNoOrders(roundsWithoutOrders, random, priceChangesMessage);

            if (!noOrdersPlaced)
            {
                HandleOrderedDrinksPriceAdjustment(sortedDrinks, priceChangesMessage, random);
            }

            MessageBox.Show(priceChangesMessage.ToString(), "Prijswijzigingen", MessageBoxButton.OK, MessageBoxImage.Information);

            foreach (var drink in drinksList)
            {
                await firebaseManager.AddDrinkToFirestore(drink);
            }

            PopulateOrderDrinksTab();
        }


        // Helper method to generate a random number between a specified range using a passed Random instance
        private double GetRandomNumber(double minValue, double maxValue, Random random)
        {
            return random.NextDouble() * (maxValue - minValue) + minValue;
        }

        // Helper method to round up to 1 decimal place
        private double RoundUp(double value)
        {
            return Math.Ceiling(value * 10) / 10;
        }

        // Helper method to round down to 1 decimal place
        private double RoundDown(double value)
        {
            return Math.Floor(value * 10) / 10;
        }

        // Universal event handler for ordering drinks
        private async void OrderDrink_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if (clickedButton != null)
            {
                // Get the drink name from the Tag property
                string drinkName = clickedButton.Tag.ToString();

                // Find the drink in the drinksList based on the name
                Drinks clickedDrink = drinksList.Find(drink => drink.Name == drinkName);

                if (clickedDrink != null)
                {
                    // Check if the last item in the orderedDrinks list is the same drink
                    if (orderedDrinks.Count > 0 && orderedDrinks.Last().Drink == clickedDrink)
                    {
                        // If it is the last ordered drink, increment the Orders count for that entry
                        orderedDrinks.Last().Orders++;
                    }
                    else
                    {
                        // Otherwise, create a new entry for the drink in the orderedDrinks list (allowing duplicates)
                        orderedDrinks.Add(new OrderedDrink(clickedDrink));
                    }

                    // Update the total orders count for the drink in the Drinks class
                    clickedDrink.Orders++;

                    // Update Firestore with the new total orders for the drink
                    await firebaseManager.UpdateDrinkOrdersInFirestore(clickedDrink.Name, clickedDrink.Orders);


                    // Update the display with the new order counts
                    UpdateOrderCountDisplay();
                }
            }
        }

        private void UpdateOrderCountDisplay()
        {
            // Clear the previous order count display
            OrderCountPanel.Children.Clear();
            double totalSum = 0;

            // Get screen size or window size
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double screenWidth = SystemParameters.PrimaryScreenWidth;

            // Adjust size for the plus and minus buttons dynamically
            double buttonSize = screenHeight * 0.05;  // 5% of the screen height
            double fontSize = screenHeight * 0.03;    // Adjust font size 
            double dynamicTextBlockWidth = screenWidth * 0.02;  // 5% of the screen width

            foreach (var orderedDrink in orderedDrinks)
            {
                if (orderedDrink.Orders > 0)
                {
                    double drinkPrice = orderedDrink.Drink.CurrentPrice ?? 0; // Default to 0 if null
                    double drinkTotal = orderedDrink.Orders * drinkPrice;

                    // Create a Grid to structure the layout
                    Grid drinkGrid = new Grid
                    {
                        Margin = new Thickness(0, 15, 0, 20),  // Add space between rows
                        HorizontalAlignment = HorizontalAlignment.Stretch // Ensure full-width usage
                    };

                    // Define three columns
                    drinkGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    drinkGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    drinkGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    // TextBlock for drink name
                    TextBlock drinkTextBlock = new TextBlock
                    {
                        Text = $"{orderedDrink.Drink.Name}:",
                        FontSize = fontSize,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(20, 0, 10, 0)
                    };
                    Grid.SetColumn(drinkTextBlock, 0); // First column
                    drinkGrid.Children.Add(drinkTextBlock);

                    // Create a StackPanel for the buttons and the order count
                    StackPanel buttonStack = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center // Center-align buttons and count
                    };

                    // - Button
                    Button minusButton = new Button
                    {
                        Content = "-",
                        Width = buttonSize,
                        Height = buttonSize,
                        FontSize = fontSize,
                        Margin = new Thickness(5),
                        Tag = orderedDrink,  // Store the orderedDrink object in the Tag
                        Style = (Style)FindResource("MinusButtonStyle") // Apply the MinusButtonStyle
                    };

                    minusButton.Click += MinusButton_Click;
                    buttonStack.Children.Add(minusButton);

                    // TextBlock for drink orders count
                    TextBlock ordersTextBlock = new TextBlock
                    {
                        Text = $"{orderedDrink.Orders}",
                        FontSize = fontSize,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = dynamicTextBlockWidth,  // Set dynamic width based on screen width
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(10, 0, 10, 0)
                    };
                    buttonStack.Children.Add(ordersTextBlock);

                    // + Button
                    Button plusButton = new Button
                    {
                        Content = "+",
                        Width = buttonSize,
                        Height = buttonSize,
                        FontSize = fontSize,
                        Margin = new Thickness(5),
                        Tag = orderedDrink,  // Store the orderedDrink object in the Tag
                        Style = (Style)FindResource("PlusButtonStyle")  // Apply the PlusButtonStyle
                    };

                    plusButton.Click += PlusButton_Click;
                    buttonStack.Children.Add(plusButton);


                    Grid.SetColumn(buttonStack, 1); // Second column
                    drinkGrid.Children.Add(buttonStack);

                    // TextBlock for showing the total price for the drink
                    TextBlock drinkTotalTextBlock = new TextBlock
                    {
                        Text = $"= {drinkTotal:F2} EUR",
                        FontSize = fontSize,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(10, 0, 20, 0)
                    };
                    Grid.SetColumn(drinkTotalTextBlock, 2); // Third column
                    drinkGrid.Children.Add(drinkTotalTextBlock);

                    // Add this grid to the main panel
                    OrderCountPanel.Children.Add(drinkGrid);

                    // Add to the total sum
                    totalSum += drinkTotal;
                }
            }

            // Update the total sum TextBlock
            TotalSumTextBlock.Text = $"Total: {totalSum:F2} EUR";
        }



        // Increment the order count
        private async void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            Button plusButton = sender as Button;
            if (plusButton != null && plusButton.Tag is OrderedDrink orderedDrink)
            {
                // Increment the order count for the clicked drink
                orderedDrink.Orders++;
                orderedDrink.Drink.Orders++;  // Also update the total orders count in the Drinks object

                // Update the orders in Firestore
                await firebaseManager.UpdateDrinkOrdersInFirestore(orderedDrink.Drink.Name, orderedDrink.Drink.Orders);

                // Refresh the display to reflect the updated order count
                UpdateOrderCountDisplay();
            }
        }

        // Decrement the order count
        private async void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            Button minusButton = sender as Button;
            if (minusButton != null && minusButton.Tag is OrderedDrink orderedDrink)
            {
                // Decrement the order count if it's greater than 0
                if (orderedDrink.Orders > 0)
                {
                    orderedDrink.Orders--;
                    orderedDrink.Drink.Orders--;  // Also update the total orders count in the Drinks object

                    // Update the orders in Firestore
                    await firebaseManager.UpdateDrinkOrdersInFirestore(orderedDrink.Drink.Name, orderedDrink.Drink.Orders);
                }

                // If no orders left, remove the drink from the orderedDrinks list
                if (orderedDrink.Orders == 0)
                {
                    orderedDrinks.Remove(orderedDrink);
                }

                // Refresh the display to reflect the updated order count
                UpdateOrderCountDisplay();
            }
        }


        private void AddNewDrinkButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if there's already an unsaved new drink, if so, skip adding another
            if (tempNewDrink != null)
            {
                MessageBox.Show("Please save or discard the current new drink before adding another.");
                return;
            }

            // Create a new temporary drink with empty fields
            tempNewDrink = new Drinks("", null, null, null);

            // Add a new row with empty fields
            int newRow = drinksList.Count + 1;  // New row after the last drink
            PopulateEditDrinksTab(newRow, tempNewDrink, isNew: true);  // Add the new empty row
        }
        private void PopulateEditDrinksTab(int newRow = -1, Drinks newDrink = null, bool isNew = false)
        {
            DrinksEditPanel.Children.Clear();

            Grid grid = new Grid();
            grid.Margin = new Thickness(10);

            // Get screen size
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            // Calculate dynamic sizes
            double textBoxWidth = screenWidth * 0.15;  // 20% of screen width for text boxes
            double priceBoxWidth = screenWidth * 0.05; // 10% of screen width for price boxes
            double buttonWidth = screenWidth * 0.10;  // 15% of screen width for buttons
            double fontSize = screenHeight * 0.03;    // Font size based on screen height
            Thickness rowTopMargin = new Thickness(0, screenHeight * 0.03, 0, 0); // Dynamic top margin

            // Define six columns (Drink Name, Min Price, Max Price, Current Price, Save Button, Delete Button)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Drink name
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Min price
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Max price
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Current price
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Save button
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Delete button

            // Add header row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddHeaderToGrid(grid, fontSize, buttonWidth);

            // Create a row for each existing drink in the list
            int currentRow = 1;
            foreach (var drink in drinksList)
            {
                AddDrinkRow(grid, drink, ref currentRow, textBoxWidth, priceBoxWidth, buttonWidth, fontSize, rowTopMargin, isNew: false);  // Add rows for existing drinks
            }

            // If there's a new drink to add, append it to the end or at a specified position
            if (newDrink != null && isNew)
            {
                AddDrinkRow(grid, newDrink, ref currentRow, textBoxWidth, priceBoxWidth, buttonWidth, fontSize, rowTopMargin, isNew: true);  // Add the new empty row
            }

            // Add the grid to the panel
            DrinksEditPanel.Children.Add(grid);
        }

        // Helper method to add headers to the grid with dynamic font size
        private void AddHeaderToGrid(Grid grid, double fontSize, double buttonWidth)
        {
            // Drink header
            TextBlock drinkHeader = new TextBlock
            {
                Text = "Drink",
                FontWeight = FontWeights.Bold,
                FontSize = fontSize,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(drinkHeader, 0);
            Grid.SetColumn(drinkHeader, 0);
            grid.Children.Add(drinkHeader);

            // Min Price header
            TextBlock minPriceHeader = new TextBlock
            {
                Text = "Min Price",
                FontWeight = FontWeights.Bold,
                FontSize = fontSize,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(minPriceHeader, 0);
            Grid.SetColumn(minPriceHeader, 1);
            grid.Children.Add(minPriceHeader);

            // Max Price header
            TextBlock maxPriceHeader = new TextBlock
            {
                Text = "Max Price",
                FontWeight = FontWeights.Bold,
                FontSize = fontSize,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(maxPriceHeader, 0);
            Grid.SetColumn(maxPriceHeader, 2);
            grid.Children.Add(maxPriceHeader);

            // Current Price header
            TextBlock currentPriceHeader = new TextBlock
            {
                Text = "Current Price",
                FontWeight = FontWeights.Bold,
                FontSize = fontSize,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(currentPriceHeader, 0);
            Grid.SetColumn(currentPriceHeader, 3);
            grid.Children.Add(currentPriceHeader);

            // Add New Drink Button in the header, spanning Save and Delete columns
            Button addNewDrinkButton = new Button
            {
                Content = "Add New Drink",
                Width = buttonWidth * 2,   // Spanning two columns (Save and Delete)
                FontSize = fontSize,       // Dynamic font size
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = (Style)FindResource("AddNewDrinkButtonStyle")  // Apply the rounded blue style
            };
            addNewDrinkButton.Click += AddNewDrinkButton_Click;  // Attach event handler
            Grid.SetRow(addNewDrinkButton, 0);
            Grid.SetColumn(addNewDrinkButton, 4);  // Place in the Save button column
            Grid.SetColumnSpan(addNewDrinkButton, 2);  // Span across 2 columns (Save and Delete)
            grid.Children.Add(addNewDrinkButton);
        }

        // Helper method to add a row for a drink
        private void AddDrinkRow(Grid grid, Drinks drink, ref int row, double textBoxWidth, double priceBoxWidth, double buttonWidth, double fontSize, Thickness rowTopMargin, bool isNew)
        {
            // Add a row for the drink input
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Drink Name
            TextBox nameTextBox = new TextBox
            {
                Text = drink.Name,
                Width = textBoxWidth,
                FontSize = fontSize,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = rowTopMargin,
                Style = (Style)FindResource("RoundedTextBoxStyle") // Apply the rounded textbox style
            };
            Grid.SetRow(nameTextBox, row);
            Grid.SetColumn(nameTextBox, 0);
            grid.Children.Add(nameTextBox);

            // Min Price
            TextBox minPriceTextBox = new TextBox
            {
                Text = drink.MinPrice.ToString(),
                Width = priceBoxWidth,
                FontSize = fontSize,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = rowTopMargin,
                Style = (Style)FindResource("RoundedTextBoxStyle") // Apply the rounded textbox style
            };
            Grid.SetRow(minPriceTextBox, row);
            Grid.SetColumn(minPriceTextBox, 1);
            grid.Children.Add(minPriceTextBox);

            // Max Price
            TextBox maxPriceTextBox = new TextBox
            {
                Text = drink.MaxPrice.ToString(),
                Width = priceBoxWidth,
                FontSize = fontSize,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = rowTopMargin,
                Style = (Style)FindResource("RoundedTextBoxStyle") // Apply the rounded textbox style
            };
            Grid.SetRow(maxPriceTextBox, row);
            Grid.SetColumn(maxPriceTextBox, 2);
            grid.Children.Add(maxPriceTextBox);

            // Current Price
            TextBox currentPriceTextBox = new TextBox
            {
                Text = drink.CurrentPrice.ToString(),
                Width = priceBoxWidth,
                FontSize = fontSize,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = rowTopMargin,
                Style = (Style)FindResource("RoundedTextBoxStyle") // Apply the rounded textbox style
            };
            Grid.SetRow(currentPriceTextBox, row);
            Grid.SetColumn(currentPriceTextBox, 3);
            grid.Children.Add(currentPriceTextBox);

            // Save Button
            Button saveButton = new Button
            {
                Content = "Save",
                Width = buttonWidth,
                FontSize = fontSize,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = rowTopMargin,
                Style = (Style)FindResource("SaveButtonStyle") // Apply the green rounded button style
            };
            Grid.SetRow(saveButton, row);
            Grid.SetColumn(saveButton, 4);
            grid.Children.Add(saveButton);

            // Delete Button
            Button deleteButton = new Button
            {
                Content = "Delete",
                Width = buttonWidth,
                FontSize = fontSize,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = rowTopMargin,
                Style = (Style)FindResource("DeleteButtonStyle") // Apply the red rounded button style
            };
            Grid.SetRow(deleteButton, row);
            Grid.SetColumn(deleteButton, 5);
            grid.Children.Add(deleteButton);

            // Add row for the error message below the drink's inputs
            RowDefinition messageRow = new RowDefinition { Height = new GridLength(10) };
            grid.RowDefinitions.Add(messageRow);

            Label messageLabel = new Label
            {
                Foreground = Brushes.Red,
                Visibility = Visibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = fontSize * 0.8,  // Slightly smaller than the input font size
                Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetRow(messageLabel, row + 1);
            Grid.SetColumn(messageLabel, 0);
            Grid.SetColumnSpan(messageLabel, 6);  // Span across all columns to prevent text overflow
            grid.Children.Add(messageLabel);

            // Increment by 2 to account for the row and the error message
            row += 2;

            // Save button click event
            saveButton.Click += async (s, args) =>
            {
                string newName = nameTextBox.Text;
                string originalName = drink.Name; // Store the original name for reference

                // Validate the drink name
                if (!ValidateDrinkName(newName, messageLabel, messageRow)) return;

                // Replace ',' with '.' to ensure parsing works for both decimal formats
                string minPriceText = minPriceTextBox.Text.Replace(',', '.');
                string maxPriceText = maxPriceTextBox.Text.Replace(',', '.');
                string currentPriceText = currentPriceTextBox.Text.Replace(',', '.');

                // Validate the prices
                if (!ValidatePrices(minPriceText, maxPriceText, currentPriceText, out double newMinPrice, out double newMaxPrice, out double newCurrentPrice, messageLabel, messageRow)) return;

                // Further validation for price values
                if (newMinPrice >= newMaxPrice)
                {
                    DisplayError("Min price must be less than max price.", messageLabel, messageRow);
                    return;
                }
                else if (newCurrentPrice < newMinPrice || newCurrentPrice > newMaxPrice)
                {
                    DisplayError($"Current price must be between {newMinPrice:F2} and {newMaxPrice:F2}.", messageLabel, messageRow);
                    return;
                }

                // Only delete the old document if it's an update and the name has changed
                if (!isNew && originalName != newName)
                {
                    await firebaseManager.DeleteDrinkFromFirestore(originalName);  // Delete the old drink by original name
                }

                // Update the drink properties
                drink.Name = newName;
                drink.MinPrice = newMinPrice;
                drink.MaxPrice = newMaxPrice;
                drink.CurrentPrice = newCurrentPrice;

                // Save the updated or new drink to Firestore
                await firebaseManager.AddDrinkToFirestore(drink);  // Save or update the drink

                // If the drink is new, add it to the local list to ensure it shows in the UI
                if (isNew)
                {
                    drinksList.Add(drink);
                }

                // Clear the error message and hide it
                messageLabel.Content = "";
                messageLabel.Visibility = Visibility.Hidden;
                messageRow.Height = new GridLength(10);  // Hide the message row if no error

                // Refresh the Edit Drinks Tab and Order Drinks Tab to show the updated drink
                PopulateEditDrinksTab();
                PopulateOrderDrinksTab();  // Update the Order Drinks tab as well

                // If the drink is new, clear the temporary new drink variable after saving
                if (isNew)
                {
                    tempNewDrink = null;
                }
            };

            // Delete button click event
            deleteButton.Click += async (s, args) =>
            {
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete {drink.Name}?", "Confirm Delete", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    // Remove the drink from Firestore
                    await firebaseManager.DeleteDrinkFromFirestore(drink.Name);

                    // Remove the drink from the local list
                    drinksList.Remove(drink);

                    // Refresh the Edit Drinks Tab to show the updated list
                    PopulateEditDrinksTab();
                }
            };
        }


        private bool ValidateDrinkName(string name, Label messageLabel, RowDefinition messageRow)
        {
            if (string.IsNullOrWhiteSpace(name) || !Regex.IsMatch(name, @"^[a-zA-Z]+$"))
            {
                DisplayError("Drink name must contain only letters and cannot be empty.", messageLabel, messageRow);
                return false;
            }
            return true;
        }
        private bool ValidatePrices(string minPriceText, string maxPriceText, string currentPriceText, out double newMinPrice, out double newMaxPrice, out double newCurrentPrice, Label messageLabel, RowDefinition messageRow)
        {
            newMinPrice = newMaxPrice = newCurrentPrice = 0;

            if (!double.TryParse(minPriceText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out newMinPrice))
            {
                DisplayError("Min price must be a valid number.", messageLabel, messageRow);
                return false;
            }

            if (!double.TryParse(maxPriceText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out newMaxPrice))
            {
                DisplayError("Max price must be a valid number.", messageLabel, messageRow);
                return false;
            }

            if (!double.TryParse(currentPriceText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out newCurrentPrice))
            {
                DisplayError("Current price must be a valid number.", messageLabel, messageRow);
                return false;
            }

            return true;
        }
        private void DisplayError(string errorMessage, Label messageLabel, RowDefinition messageRow)
        {
            messageLabel.Content = errorMessage;
            messageLabel.Foreground = Brushes.Red;
            messageLabel.Visibility = Visibility.Visible;
            messageRow.Height = GridLength.Auto;  // Show the message row
        }

        private UIElement GetGridElement(int row, int column)
        {
            foreach (UIElement element in DrinksEditPanel.Children)
            {
                if (Grid.GetRow(element) == row && Grid.GetColumn(element) == column)
                {
                    return element;
                }
            }
            return null;
        }

        private void PopulateOrderDrinksTab()
        {
            // Clear the existing content in the order drinks panel
            DrinksPanel.Children.Clear();

            // Get screen size or window size
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            // Calculate dynamic button size based on screen size (adjust scaling factors as needed)
            double buttonWidth = screenWidth * 0.12;  // 15% of screen width
            double buttonHeight = screenHeight * 0.075;  // 10% of screen height
            double fontSize = screenHeight * 0.02;    // 3% of screen height for font size
            double marginSize = screenHeight * 0.05;  // 2% of screen height for margin

            // Add a button for each drink in the list
            foreach (var drink in drinksList)
            {
                Button drinkButton = new Button
                {
                    Content = $"{drink.Name} - {drink.CurrentPrice:F2} EUR",
                    Width = buttonWidth,    // Dynamic width based on screen size
                    Height = buttonHeight,  // Dynamic height based on screen size
                    FontSize = fontSize,    // Dynamic font size based on screen size
                    Margin = new Thickness(marginSize, marginSize, marginSize, marginSize),  // Dynamic margin
                    Tag = drink.Name        // Use Tag to identify the drink by name
                };

                // Apply the style for rounded corners (if defined in resources)
                drinkButton.Style = (Style)FindResource("RoundedButtonStyle");

                // Set the background color using the PriceToColorConverter
                Binding colorBinding = new Binding
                {
                    Source = drink,
                    Converter = (IValueConverter)FindResource("PriceToColorConverter") // Use the converter
                };
                drinkButton.SetBinding(Button.BackgroundProperty, colorBinding);

                // Attach the universal event handler for ordering drinks
                drinkButton.Click += OrderDrink_Click;

                // Add the button to the DrinksPanel (the panel in the "Order Drinks" tab)
                DrinksPanel.Children.Add(drinkButton);
            }
        }


    }
}