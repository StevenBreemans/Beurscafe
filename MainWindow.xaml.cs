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
        // List to store the order in which drinks are clicked
        private List<Drinks> orderedDrinks = new List<Drinks>();


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

        public MainWindow()
        {
            InitializeComponent();

            // Initialize drinks
            beer = new Drinks("Beer", 1.5, 5.0, 2.0);
            wine = new Drinks("Wine", 2.0, 6.0, 3.0);
            beer1 = new Drinks("Beer1", 1.5, 5.0, 2.0);
            wine1 = new Drinks("Wine1", 2.0, 6.0, 3.0);
            beer2 = new Drinks("Beer2", 1.5, 5.0, 2.0);
            wine2 = new Drinks("Wine2", 2.0, 6.0, 3.0);
            beer3 = new Drinks("Beer3", 1.5, 5.0, 2.0);
            wine3 = new Drinks("Wine3", 2.0, 6.0, 3.0);
            beer4 = new Drinks("Beer4", 1.5, 5.0, 2.0);
            wine4 = new Drinks("Wine4", 2.0, 6.0, 3.0);
            beer5 = new Drinks("Beer5", 1.5, 5.0, 2.0);
            wine5 = new Drinks("Wine5", 2.0, 6.0, 3.0);
            beer6 = new Drinks("Beer6", 1.5, 5.0, 2.0);
            wine6 = new Drinks("Wine6", 2.0, 6.0, 3.0);
            beer7 = new Drinks("Beer7", 1.5, 5.0, 2.0);
            wine7 = new Drinks("Wine7", 2.0, 6.0, 3.0);
            beer8 = new Drinks("Beer8", 1.5, 5.0, 2.0);
            wine8 = new Drinks("Wine8", 2.0, 6.0, 3.0);
            beer9 = new Drinks("Beer9", 1.5, 5.0, 2.0);
            wine9 = new Drinks("Wine9", 2.0, 6.0, 3.0);

            // Add drinks to the list
            drinksList.Add(beer);
            drinksList.Add(beer1);
            drinksList.Add(beer2);
            drinksList.Add(beer3);
            drinksList.Add(beer4);
            drinksList.Add(beer5);
            drinksList.Add(beer6);
            drinksList.Add(beer7);
            drinksList.Add(beer8);
            drinksList.Add(beer9);

            drinksList.Add(wine);
            drinksList.Add(wine1);
            drinksList.Add(wine2);
            drinksList.Add(wine3);
            drinksList.Add(wine4);
            drinksList.Add(wine5);
            drinksList.Add(wine6);
            drinksList.Add(wine7);
            drinksList.Add(wine8);
            drinksList.Add(wine9);

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
                // Adjust prices when timer reaches zero
                AdjustPrices();
                PopulateOrderDrinksTab();

                // Reset the timer to either the custom or default value
                if (customTimerSet)
                {
                    timeRemaining = originalTimeRemaining;  // Reset to custom time
                }
                else
                {
                    timeRemaining = defaultTimeRemaining; // Reset to default 5 minutes
                }

                // Reset the orders and update the order count display
                ResetOrders();
                UpdateOrderCountDisplay();

                // Update the TimerTabItem header with reset time
                UpdateTimerDisplay();
            }
        }

        // Reset orders for all drinks
        private void ResetOrders()
        {
            foreach (var drink in drinksList)
            {
                drink.Orders = 0;  // Reset the order count for each drink
            }
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
            if (MainTabControl.SelectedItem == TimerTabItem)
            {
                // Timer tab is clicked, show the dialog
                string input = Microsoft.VisualBasic.Interaction.InputBox("Enter new time in minutes:", "Adjust Timer", "5");

                if (int.TryParse(input, out int newMinutes))
                {
                    // Set the new remaining time based on user input
                    timeRemaining = TimeSpan.FromSeconds(newMinutes);
                    UpdateTimerDisplay();  // Update the header with the new time
                    // Save the original custom time
                    originalTimeRemaining = timeRemaining;                      
                    // Mark that a custom timer is set
                    customTimerSet = true;
                }
                else
                {
                    MessageBox.Show("Invalid input. Please enter a valid number.");
                }

                // Switch back to the previous tab after the dialog
                MainTabControl.SelectedIndex = lastSelectedIndex;
            }
            else
            {
                // Handle the other tabs normally and keep track of the last selected index
                if (MainTabControl.SelectedIndex == 1)  // View Edit Drinks tab (Index 1)
                {
                    PopulateEditDrinksTab();
                }
                else if (MainTabControl.SelectedIndex == 0)  // Order Drinks tab (Index 0)
                {
                    PopulateOrderDrinksTab();  // Refresh the Order Drinks tab
                }

                // Save the currently selected tab index
                lastSelectedIndex = MainTabControl.SelectedIndex;
            }
        }

        private void AdjustPrices()
        {
            foreach (var drink in drinksList)
            {
                drink.AdjustPrice();
                drink.Orders = 0;  // Reset orders to 0 after adjusting prices
            }

            // You could also update the buttons' content to reflect new prices if necessary
        }

        // Universal event handler for ordering drinks
        private void OrderDrink_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if (clickedButton != null)
            {
                // Get the drink name from the Tag property
                string drinkName = clickedButton.Tag.ToString();

                // Find the drink in the list based on the name
                Drinks clickedDrink = drinksList.Find(drink => drink.Name == drinkName);

                if (clickedDrink != null)
                {
                    // Increment the orders for the clicked drink
                    clickedDrink.Orders++;

                    // Check if the clicked drink is already the last one in the orderedDrinks list
                    if (orderedDrinks.Count == 0 || orderedDrinks[^1] != clickedDrink)
                    {
                        // Add the drink to the orderedDrinks list only if it's not already the last item
                        orderedDrinks.Add(clickedDrink);
                    }

                    // Refresh the right panel with updated counts
                    UpdateOrderCountDisplay();
                }
            }
        }


        private void UpdateOrderCountDisplay()
        {
            // Clear the previous order count display
            OrderCountPanel.Children.Clear();
            double totalSum = 0;
            //test
            // Iterate over the orderedDrinks list (ordered by click sequence)
            foreach (var drink in orderedDrinks)
            {
                if (drink.Orders > 0)
                {
                    // Use the null-coalescing operator ?? to handle null values
                    double drinkPrice = drink.CurrentPrice ?? 0; // Default to 0 if null
                    double drinkTotal = drink.Orders * drinkPrice;

                    // Create a Grid to structure the layout
                    Grid drinkGrid = new Grid
                    {
                        Margin = new Thickness(0, 15, 0, 0),  // Add space between rows
                        HorizontalAlignment = HorizontalAlignment.Stretch // Ensure full-width usage
                    };

                    // Define three columns, each taking 1/3 of the available width
                    drinkGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    drinkGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    drinkGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    // TextBlock for drink name
                    TextBlock drinkTextBlock = new TextBlock
                    {
                        Text = $"{drink.Name}:",
                        FontSize = 40,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center, // Left-align drink name
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
                        Tag = drink // Store the drink object in the Tag to access it in the click event
                    };
                    minusButton.Click += MinusButton_Click;
                    buttonStack.Children.Add(minusButton);

                    // TextBlock for drink orders count
                    TextBlock ordersTextBlock = new TextBlock
                    {
                        Text = $"{drink.Orders}",
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
                        Tag = drink // Store the drink object in the Tag to access it in the click event
                    };
                    plusButton.Click += PlusButton_Click;
                    buttonStack.Children.Add(plusButton);

                    Grid.SetColumn(buttonStack, 1); // Second column for buttons and count
                    drinkGrid.Children.Add(buttonStack);

                    // TextBlock for showing the total price for the drink
                    TextBlock drinkTotalTextBlock = new TextBlock
                    {
                        Text = $"= {drinkTotal:F2} EUR",
                        FontSize = 40,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center, // Right-align the total price
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


        private void PopulateEditDrinksTab()
        {
            // Clear the existing content in the edit panel
            DrinksEditPanel.Children.Clear();

            // Create a grid with 6 columns: Drink name, Min Price, Max Price, Current Price, Save Button, and Add New Drink button
            Grid grid = new Grid();
            grid.Margin = new Thickness(0, 10, 0, 10);

            // Define the columns
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });  // Drink name
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });  // Min Price
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });  // Max Price
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });  // Current Price
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });   // Save button
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });  // Takes up remaining space

            // Add header row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddHeaderToGrid(grid);

            // Create a row for each existing drink in the list
            int row = 1;
            foreach (var drink in drinksList)
            {
                AddDrinkRow(grid, drink, ref row, isNew: false); // isNew is false for existing drinks
            }

            // Add the button to add a new drink in the first row, right aligned
            Button addNewDrinkButton = new Button
            {
                Content = "Add New Drink",
                Width = 150,
                Height = 30,
                Margin = new Thickness(10, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            addNewDrinkButton.Click += (s, e) =>
            {
                // Add a new empty row for the new drink, without adding it to the list yet
                Drinks tempDrink = new Drinks("", null, null, null);  // Placeholder for unsaved new drink
                AddDrinkRow(grid, tempDrink, ref row, isNew: true); // isNew is true for unsaved drinks
            };

            // Set the "Add New Drink" button in the first row and last column
            Grid.SetRow(addNewDrinkButton, 0);
            Grid.SetColumn(addNewDrinkButton, 5);  // Placed in the last column
            grid.Children.Add(addNewDrinkButton);

            // Add the grid to the DrinksEditPanel
            DrinksEditPanel.Children.Add(grid);
        }

        // Helper method to add headers to the grid
        private void AddHeaderToGrid(Grid grid)
        {
            // Drink header
            TextBlock drinkHeader = new TextBlock { Text = "Drink", FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(drinkHeader, 0);
            Grid.SetColumn(drinkHeader, 0);
            grid.Children.Add(drinkHeader);

            // Min Price header
            TextBlock minPriceHeader = new TextBlock { Text = "Min Price", FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(minPriceHeader, 0);
            Grid.SetColumn(minPriceHeader, 1);
            grid.Children.Add(minPriceHeader);

            // Max Price header
            TextBlock maxPriceHeader = new TextBlock { Text = "Max Price", FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(maxPriceHeader, 0);
            Grid.SetColumn(maxPriceHeader, 2);
            grid.Children.Add(maxPriceHeader);

            // Current Price header
            TextBlock currentPriceHeader = new TextBlock { Text = "Current Price", FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
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
        private void AddDrinkRow(Grid grid, Drinks drink, ref int row, bool isNew)
        {
            // Add a row for the drink
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Drink Name
            TextBox nameTextBox = new TextBox { Text = drink.Name, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 10, 0) };
            Grid.SetRow(nameTextBox, row);
            Grid.SetColumn(nameTextBox, 0);
            grid.Children.Add(nameTextBox);

            // Min Price
            TextBox minPriceTextBox = new TextBox { Text = drink.MinPrice.ToString(), HorizontalContentAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 10, 0) };
            Grid.SetRow(minPriceTextBox, row);
            Grid.SetColumn(minPriceTextBox, 1);
            grid.Children.Add(minPriceTextBox);

            // Max Price
            TextBox maxPriceTextBox = new TextBox { Text = drink.MaxPrice.ToString(), HorizontalContentAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 10, 0) };
            Grid.SetRow(maxPriceTextBox, row);
            Grid.SetColumn(maxPriceTextBox, 2);
            grid.Children.Add(maxPriceTextBox);

            // Current Price
            TextBox currentPriceTextBox = new TextBox { Text = drink.CurrentPrice.ToString(), HorizontalContentAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 10, 0) };
            Grid.SetRow(currentPriceTextBox, row);
            Grid.SetColumn(currentPriceTextBox, 3);
            grid.Children.Add(currentPriceTextBox);

            // Save Button
            Button saveButton = new Button { Content = "Save", Width = 50, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
            Grid.SetRow(saveButton, row);
            Grid.SetColumn(saveButton, 4);
            grid.Children.Add(saveButton);

            // Create a new row below for the message label with initial height 10
            RowDefinition messageRow = new RowDefinition { Height = new GridLength(10) };
            grid.RowDefinitions.Add(messageRow);

            // Message Label (Placed below the row for the current drink)
            Label messageLabel = new Label { Foreground = Brushes.Red, Visibility = Visibility.Hidden, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(10, 0, 0, 0) };
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