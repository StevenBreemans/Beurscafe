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


        private Drinks beer;
        private Drinks wine;
        private Drinks beer1;
        private Drinks wine1;
        private Drinks beer2;
        private Drinks wine2;
        private Drinks beer3;
        private Drinks wine3;
        private Drinks beer4;
        private Drinks wine4;
        private Drinks beer5;
        private Drinks wine5;
        private Drinks beer6;
        private Drinks wine6;
        private Drinks beer7;
        private Drinks wine7;
        private Drinks beer8;
        private Drinks wine8;
        private Drinks beer9;
        private Drinks wine9;
        private int lastSelectedIndex = 0;
        private TimeSpan defaultTimeRemaining = TimeSpan.FromMinutes(5);  // Default 5-minute timer
        private TimeSpan originalTimeRemaining;  // Store the original value to reset the timer
        private bool customTimerSet = false;     // Track if the custom timer is set
        private Drinks tempNewDrink = null;  // To track the temporary new drink

        public MainWindow()
        {
            InitializeComponent();

            // Initialize drinks
            beer1 = new Drinks("Beer1", 1.5, 5.0, 2.0);
            wine1 = new Drinks("Wine1", 2.0, 6.0, 3.0);
            beer2 = new Drinks("Beer2", 1.5, 5.0, 2.0);
            wine2 = new Drinks("Wine2", 2.0, 6.0, 3.0);
            beer3 = new Drinks("Beer3", 1.5, 5.0, 2.0);
            wine3 = new Drinks("Wine3", 2.0, 6.0, 3.0);
            beer4 = new Drinks("Beer4", 1.5, 5.0, 2.0);
            wine4 = new Drinks("Wine4", 2.0, 6.0, 3.0);


            // Add drinks to the list
            drinksList.Add(beer1);
            drinksList.Add(beer2);
            drinksList.Add(beer3);
            drinksList.Add(beer4);

            drinksList.Add(wine1);
            drinksList.Add(wine2);
            drinksList.Add(wine3);
            drinksList.Add(wine4);


            // Set the timer to 5 minutes initially
            timeRemaining = defaultTimeRemaining;
            UpdateTimerDisplay();  // Update the TabItem header with the initial time
            StartTimer();
            PopulateOrderDrinksTab();
            UpdateOrderCountDisplay();  // Initialize the order count display
        }
        private void StartTimer()
        {
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
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



        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (timeRemaining > TimeSpan.Zero)
            {
                // Update remaining time every second
                timeRemaining = timeRemaining.Add(TimeSpan.FromSeconds(-1));
                UpdateTimerDisplay();  // Update the TabItem header
            }
            else
            {
                // Play sound when the timer hits 0
                PlaySound();

                // Adjust prices when the timer reaches zero
                AdjustPrices();

                // Reset the timer to either the custom or default value
                timeRemaining = customTimerSet ? originalTimeRemaining : defaultTimeRemaining;

                // Clear orders and update the right-side panel
                ResetOrders();
                UpdateOrderCountDisplay();

                // Update the TimerTabItem header with reset time
                UpdateTimerDisplay();
            }
        }


        // Reset orders for all drinks in the orderedDrinks list
        private void ResetOrders()
        {
            orderedDrinks.Clear();  // Clear the ordered drinks list
            foreach (var drink in drinksList)
            {
                drink.Orders = 0;  // Reset the order count for each drink in the drinks list
            }
            UpdateOrderCountDisplay();  // Update the right-side display after resetting
        }


        // Method to update the TimerTabItem's header dynamically
        private void UpdateTimerDisplay()
        {
            // Assuming the timer TabItem is the last item in the TabControl
            TabItem timerTabItem = (TabItem)MainTabControl.Items[2]; // Adjust the index if necessary
            timerTabItem.Header = $"Resterende tijd: {timeRemaining:mm\\:ss}";
        }

        // Event handler for TabControl SelectionChanged
        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                        // Set the new remaining time based on user input
                        timeRemaining = TimeSpan.FromSeconds(newMinutes);  // Use minutes here
                        UpdateTimerDisplay();  // Update the header with the new time

                        // Save the original custom time
                        originalTimeRemaining = timeRemaining;
                        customTimerSet = true;  // Mark that a custom timer is set
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


        private void AdjustPrices()
        {
            foreach (var drink in drinksList)
            {
                drink.AdjustPrice();
                drink.Orders = 0;  // Reset orders to 0 after adjusting prices
            }

            // Update the left side of the screen with new prices
            PopulateOrderDrinksTab();
        }


        // Universal event handler for ordering drinks
        private void OrderDrink_Click(object sender, RoutedEventArgs e)
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
                    // Check if the last item in the ordered list is the same drink
                    if (orderedDrinks.Count > 0 && orderedDrinks.Last().Drink == clickedDrink)
                    {
                        // Increment the orders for the last item
                        orderedDrinks.Last().Orders++;
                    }
                    else
                    {
                        // Add a new entry for this drink to the ordered list
                        orderedDrinks.Add(new OrderedDrink(clickedDrink));
                    }

                    // Increment the order count for the drink itself
                    clickedDrink.Orders++;

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

            foreach (var orderedDrink in orderedDrinks)
            {
                if (orderedDrink.Orders > 0)
                {
                    double drinkPrice = orderedDrink.Drink.CurrentPrice ?? 0; // Default to 0 if null
                    double drinkTotal = orderedDrink.Orders * drinkPrice;

                    // Create a Grid to structure the layout (similar to your existing code)
                    Grid drinkGrid = new Grid
                    {
                        Margin = new Thickness(0, 15, 0, 0),  // Add space between rows
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
                        FontSize = 40,
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
                        Width = 30,
                        Height = 30,
                        Margin = new Thickness(5),
                        Tag = orderedDrink // Store the orderedDrink object in the Tag
                    };
                    minusButton.Click += MinusButton_Click;
                    buttonStack.Children.Add(minusButton);

                    // TextBlock for drink orders count
                    TextBlock ordersTextBlock = new TextBlock
                    {
                        Text = $"{orderedDrink.Orders}",
                        FontSize = 40,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10, 0, 10, 0)
                    };
                    buttonStack.Children.Add(ordersTextBlock);

                    // + Button
                    Button plusButton = new Button
                    {
                        Content = "+",
                        Width = 30,
                        Height = 30,
                        Margin = new Thickness(5),
                        Tag = orderedDrink // Store the orderedDrink object in the Tag
                    };
                    plusButton.Click += PlusButton_Click;
                    buttonStack.Children.Add(plusButton);

                    Grid.SetColumn(buttonStack, 1); // Second column
                    drinkGrid.Children.Add(buttonStack);

                    // TextBlock for showing the total price for the drink
                    TextBlock drinkTotalTextBlock = new TextBlock
                    {
                        Text = $"= {drinkTotal:F2} EUR",
                        FontSize = 40,
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

            // Update the total sum TextBlock (this one is already defined in XAML)
            TotalSumTextBlock.Text = $"Total: {totalSum:F2} EUR";
        }






        // Increment the order count
        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            Button plusButton = sender as Button;
            Drinks drink = plusButton.Tag as Drinks;

            if (drink != null)
            {
                drink.Orders++;  // Increment order count
                UpdateOrderCountDisplay();  // Refresh the display
            }
        }

        // Decrement the order count
        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            Button minusButton = sender as Button;
            Drinks drink = minusButton.Tag as Drinks;

            if (drink != null && drink.Orders > 0)
            {
                drink.Orders--;  // Decrement order count
                UpdateOrderCountDisplay();  // Refresh the display
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

            // Define five columns (Drink Name, Min Price, Max Price, Current Price, Save Button)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Drink name
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Min price
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Max price
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Current price
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Save button

            // Add header row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddHeaderToGrid(grid);

            // Create a row for each existing drink in the list
            int currentRow = 1;
            foreach (var drink in drinksList)
            {
                AddDrinkRow(grid, drink, ref currentRow, isNew: false);  // Add rows for existing drinks
            }

            // If there's a new drink to add, append it to the end or at a specified position
            if (newDrink != null && isNew)
            {
                AddDrinkRow(grid, newDrink, ref currentRow, isNew: true);  // Add the new empty row
            }
            Button addNewDrinkButton = new Button
            {
                Content = "Add New Drink",
                Width = 350,
                FontSize = 40,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            addNewDrinkButton.Click += AddNewDrinkButton_Click;  // Attach event handler here
            Grid.SetRow(addNewDrinkButton, 0);
            Grid.SetColumn(addNewDrinkButton, 4);  // Place in the last column
            grid.Children.Add(addNewDrinkButton);
            DrinksEditPanel.Children.Add(grid);
        }


        // Helper method to add headers to the grid
        private void AddHeaderToGrid(Grid grid)
        {
            // Drink header
            TextBlock drinkHeader = new TextBlock { Text = "Drink", FontWeight = FontWeights.Bold, FontSize = 45, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(drinkHeader, 0);
            Grid.SetColumn(drinkHeader, 0);
            grid.Children.Add(drinkHeader);

            // Min Price header
            TextBlock minPriceHeader = new TextBlock { Text = "Min Price", FontWeight = FontWeights.Bold, FontSize = 45, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(minPriceHeader, 0);
            Grid.SetColumn(minPriceHeader, 1);
            grid.Children.Add(minPriceHeader);

            // Max Price header
            TextBlock maxPriceHeader = new TextBlock { Text = "Max Price", FontWeight = FontWeights.Bold, FontSize = 45, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(maxPriceHeader, 0);
            Grid.SetColumn(maxPriceHeader, 2);
            grid.Children.Add(maxPriceHeader);

            // Current Price header
            TextBlock currentPriceHeader = new TextBlock { Text = "Current Price", FontWeight = FontWeights.Bold, FontSize = 45, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(currentPriceHeader, 0);
            Grid.SetColumn(currentPriceHeader, 3);
            grid.Children.Add(currentPriceHeader);

            // Save button header
            TextBlock saveHeader = new TextBlock { Text = "", Width = 50 };
            Grid.SetRow(saveHeader, 0);
            Grid.SetColumn(saveHeader, 4);
            grid.Children.Add(saveHeader);

            // Empty header for Add New Drink button
            TextBlock addDrinkHeader = new TextBlock { Text = "", Width = 150 };
            Grid.SetRow(addDrinkHeader, 0);
            Grid.SetColumn(addDrinkHeader, 5);
            grid.Children.Add(addDrinkHeader);
        }

        // Helper method to add a row for a drink
        // Helper method to add a row for a drink
        private void AddDrinkRow(Grid grid, Drinks drink, ref int row, bool isNew)
        {
            // Define commonly used values
            Thickness rowTopMargin = new Thickness(0, 40, 0, 0);  // 40px top margin
            double textBoxWidth = 200;
            double priceBoxWidth = 100;
            double buttonWidth = 100;
            double fontSize = 40;
            HorizontalAlignment textAlignment = HorizontalAlignment.Center;
            VerticalAlignment verticalAlignment = VerticalAlignment.Center;
            Thickness priceMargin = new Thickness(10, 0, 10, 0);  // Used for price TextBoxes and buttons
            double messageFontSize = 30;

            // Add a row for the drink
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Drink Name
            TextBox nameTextBox = new TextBox
            {
                Text = drink.Name,
                Width = textBoxWidth,
                FontSize = fontSize,
                HorizontalContentAlignment = textAlignment,
                VerticalAlignment = verticalAlignment,
                Margin = rowTopMargin
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
                HorizontalContentAlignment = textAlignment,
                VerticalAlignment = verticalAlignment,
                Margin = rowTopMargin
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
                HorizontalContentAlignment = textAlignment,
                VerticalAlignment = verticalAlignment,
                Margin = priceMargin
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
                HorizontalContentAlignment = textAlignment,
                VerticalAlignment = verticalAlignment,
                Margin = priceMargin
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
                VerticalAlignment = verticalAlignment,
                Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetRow(saveButton, row);
            Grid.SetColumn(saveButton, 4);
            grid.Children.Add(saveButton);

            // Create a new row below for the message label with initial height 10
            RowDefinition messageRow = new RowDefinition { Height = new GridLength(10) };
            grid.RowDefinitions.Add(messageRow);

            // Message Label (Placed below the row for the current drink)
            Label messageLabel = new Label
            {
                Foreground = Brushes.Red,
                Visibility = Visibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Center, // Centered horizontally
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = messageFontSize,  // Font size 30
                Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetRow(messageLabel, row + 1);  // Row under the drink's inputs
            Grid.SetColumn(messageLabel, 0);  // Align the message under the drink name column
            Grid.SetColumnSpan(messageLabel, 5);  // Span across all columns to prevent text overflow
            grid.Children.Add(messageLabel);

            // Save button click event
            saveButton.Click += (s, args) =>
            {
                string newName = nameTextBox.Text;

                // Regex for validating that the name contains only letters
                if (string.IsNullOrWhiteSpace(newName) || !Regex.IsMatch(newName, @"^[a-zA-Z]+$"))
                {
                    messageLabel.Content = "Drink name must contain only letters.";
                    messageLabel.Foreground = Brushes.Red;
                    messageLabel.Visibility = Visibility.Visible;
                    messageRow.Height = GridLength.Auto;  // Show the message row
                    return;  // Stop execution if the name is invalid
                }

                // Replace ',' with '.' to ensure parsing works for both decimal formats
                string minPriceText = minPriceTextBox.Text.Replace(',', '.');
                string maxPriceText = maxPriceTextBox.Text.Replace(',', '.');
                string currentPriceText = currentPriceTextBox.Text.Replace(',', '.');

                // Check if the prices are valid numbers
                if (!double.TryParse(minPriceText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double newMinPrice))
                {
                    messageLabel.Content = "Min price must be a valid number.";
                    messageLabel.Foreground = Brushes.Red;
                    messageLabel.Visibility = Visibility.Visible;
                    messageRow.Height = GridLength.Auto;  // Show the message row
                    return;  // Stop execution if min price is invalid
                }

                if (!double.TryParse(maxPriceText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double newMaxPrice))
                {
                    messageLabel.Content = "Max price must be a valid number.";
                    messageLabel.Foreground = Brushes.Red;
                    messageLabel.Visibility = Visibility.Visible;
                    messageRow.Height = GridLength.Auto;  // Show the message row
                    return;  // Stop execution if max price is invalid
                }

                if (!double.TryParse(currentPriceText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double newCurrentPrice))
                {
                    messageLabel.Content = "Current price must be a valid number.";
                    messageLabel.Foreground = Brushes.Red;
                    messageLabel.Visibility = Visibility.Visible;
                    messageRow.Height = GridLength.Auto;  // Show the message row
                    return;  // Stop execution if current price is invalid
                }

                // Further validation for price values
                if (newMinPrice >= newMaxPrice)
                {
                    messageLabel.Content = "Min price must be less than max price.";
                    messageLabel.Foreground = Brushes.Red;
                    messageLabel.Visibility = Visibility.Visible;
                    messageRow.Height = GridLength.Auto;  // Show the message row
                }
                else if (newCurrentPrice < newMinPrice || newCurrentPrice > newMaxPrice)
                {
                    messageLabel.Content = $"Current price must be between {newMinPrice:F2} and {newMaxPrice:F2}.";
                    messageLabel.Foreground = Brushes.Red;
                    messageLabel.Visibility = Visibility.Visible;
                    messageRow.Height = GridLength.Auto;  // Show the message row
                }
                else
                {
                    // If this is a new drink (it hasn't been saved yet), add it to the list
                    if (isNew)
                    {
                        // Check if a drink with the same name already exists to prevent adding duplicates
                        var existingDrink = drinksList.Find(d => d.Name.Equals(newName, StringComparison.OrdinalIgnoreCase));
                        if (existingDrink != null)
                        {
                            return;
                        }

                        // Add the new drink to the list
                        Drinks newDrink = new Drinks(newName, newMinPrice, newMaxPrice, newCurrentPrice);
                        drinksList.Add(newDrink);
                    }
                    else
                    {
                        // Update the existing drink in the list
                        Drinks existingDrink = drinksList.Find(d => d.Name == drink.Name);
                        if (existingDrink != null)
                        {
                            existingDrink.Name = newName;
                            existingDrink.MinPrice = newMinPrice;
                            existingDrink.MaxPrice = newMaxPrice;
                            existingDrink.CurrentPrice = newCurrentPrice;
                        }
                    }

                    // Clear the error message
                    messageLabel.Content = "";
                    messageLabel.Visibility = Visibility.Hidden;
                    messageRow.Height = new GridLength(10);  // Hide the message row if no error
                }
            };

            row += 2;  // Move to the next row (2 rows for each drink: one for inputs, one for message)
        }

        private void PopulateOrderDrinksTab()
        {
            // Clear the existing content in the order drinks panel
            DrinksPanel.Children.Clear();

            // Add a button for each drink in the list
            foreach (var drink in drinksList)
            {
                Button drinkButton = new Button
                {
                    Content = $"{drink.Name} - {drink.CurrentPrice:F2} EUR",
                    Width = 150,
                    Height = 50,
                    Margin = new Thickness(0, 10, 0, 0),
                    Tag = drink.Name  // Use Tag to identify the drink by name
                };

                // Attach the universal event handler for ordering drinks
                drinkButton.Click += OrderDrink_Click;

                // Add the button to the DrinksPanel (the panel in the "Order Drinks" tab)
                DrinksPanel.Children.Add(drinkButton);
            }
        }


    }
}