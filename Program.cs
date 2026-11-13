ShoppingList list = new ShoppingList("items.txt");
list.Load();

while (true)
{
    Console.WriteLine();
    list.Print();
    Console.WriteLine();
    Console.WriteLine("1. Lägg till vara");
    Console.WriteLine("2. Ta bort vara");
    Console.WriteLine("3. Spara");
    Console.WriteLine("4. Sök vara");
    Console.WriteLine("5. Avsluta");
    Console.Write("Välj: ");

    // Asked for: fix fel 3, layer 1. Letters or an empty line in the menu must not crash the program.
    string choiceText = Console.ReadLine();

    // When the input has ended (Ctrl+Z, or the end of piped input) there is nothing more to read,
    // so quit like choice 5 instead of asking again forever.
    if (choiceText == null)
    {
        break;
    }

    // TryParse gives false instead of throwing, so the user gets a message and the menu is shown again.
    if (!int.TryParse(choiceText, out int choice))
    {
        Console.WriteLine("Skriv en siffra mellan 1 och 5.");
        continue;
    }

    if (choice == 1)
    {
        Console.Write("Namn: ");
        string name = Console.ReadLine();
        Console.Write("Pris: ");

        // Asked for: fix fel 3, layer 2. Letters in the price must not crash the program:
        // TryParse gives false, the user is told, no item is added and the menu is shown again.
        if (!int.TryParse(Console.ReadLine(), out int price))
        {
            Console.WriteLine("Priset måste vara ett heltal. Varan lades inte till.");
            continue;
        }

        // Asked for: utbyggnad U1-1. Item refuses an empty name by throwing ArgumentException.
        // Catch exactly that, tell the user and go back to the menu without adding anything.
        Item item;

        try
        {
            item = new Item(name, price);
        }
        // Asked for: utbyggnad U1-2. Item refuses a negative price with ArgumentOutOfRangeException.
        // It must come before catch (ArgumentException): ArgumentOutOfRangeException is a kind of
        // ArgumentException, so the more specific catch has to be first or it would never be reached.
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("Priset får inte vara negativt. Varan lades inte till.");
            continue;
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Namnet får inte vara tomt. Varan lades inte till.");
            continue;
        }

        list.Add(item);
    }
    else if (choice == 2)
    {
        Console.Write("Nummer: ");

        // Asked for: fix fel 3, layer 3. Letters in the number must not crash the program:
        // TryParse gives false, the user is told, nothing is removed and the menu is shown again.
        if (!int.TryParse(Console.ReadLine(), out int number))
        {
            Console.WriteLine("Numret måste vara ett heltal. Ingen vara togs bort.");
            continue;
        }

        // Asked for: fix fel 4, layer 2. RemoveAt answers false when the number is not in the list,
        // so tell the user instead of letting nothing happen without a word.
        if (!list.RemoveAt(number))
        {
            Console.WriteLine($"Nummer {number} finns inte i listan. Ingen vara togs bort.");
        }
    }
    else if (choice == 3)
    {
        list.Save();
    }
    else if (choice == 4)
    {
        Console.Write("Namn att söka efter: ");
        string wanted = Console.ReadLine();
        Item found = list.Find(wanted);

        if (found == null)
        {
            Console.WriteLine("Varan finns inte i listan.");
        }
        else
        {
            Console.WriteLine($"Hittade: {found}");
        }
    }
    else if (choice == 5)
    {
        break;
    }
}
