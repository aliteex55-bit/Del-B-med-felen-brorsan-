// Holds the items and takes care of loading and saving them.
class ShoppingList
{
    private List<Item> items = new List<Item>();
    private string path;

    // Asked for: utbyggnad U2-1. The list has a budget: the most the whole list may cost, in whole kronor.
    public int Budget { get; }

    public ShoppingList(string path, int budget)
    {
        this.path = path;
        Budget = budget;
    }

    // Asked for: utbyggnad U2-1. An item that would make the total go over the budget is not added.
    // The design choice is to say no by returning false (like RemoveAt), not by throwing an exception:
    // a full budget is an ordinary situation the caller checks with an if.
    // The sum is computed as long, so a huge price cannot overflow int and slip under the budget.
    public bool Add(Item item)
    {
        if ((long)Total() + item.Price > Budget)
        {
            return false;
        }

        items.Add(item);
        return true;
    }

    // Removes the item the user sees as number 1, 2, 3 ...
    // Asked for: fix fel 4, layer 1. A number that is not in the list must not crash the program.
    // Returns false and removes nothing when the number is outside 1 .. the number of items,
    // so the caller can tell the user.
    public bool RemoveAt(int number)
    {
        if (number < 1 || number > items.Count)
        {
            return false;
        }

        items.RemoveAt(number - 1);
        return true;
    }

    // Adds up the price of every item on the list.
    public int Total()
    {
        int sum = 0;

        // Asked for: fix fel 5. The first item has index 0, so the loop must start at 0;
        // starting at 1 left the first item out of the total.
        for (int i = 0; i < items.Count; i++)
        {
            sum += items[i].Price;
        }

        return sum;
    }

    // Looks up an item by its name. Returns null if there is no such item.
    public Item Find(string name)
    {
        foreach (Item item in items)
        {
            if (item.Name == name)
            {
                return item;
            }
        }

        return null;
    }

    public void Print()
    {
        for (int i = 0; i < items.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {items[i]}");
        }

        Console.WriteLine($"Totalt: {Total()} kr");
    }

    // Writes one item per line, as "price;name".
    public void Save()
    {
        List<string> lines = new List<string>();

        foreach (Item item in items)
        {
            lines.Add($"{item.Price};{item.Name}");
        }

        try
        {
            File.WriteAllText(path, string.Join("\r\n", lines) + "\r\n");

            // Asked for: fix fel 6, layer 2. Say the list is saved only when the writing actually worked:
            // this line is reached only if WriteAllText did not throw.
            Console.WriteLine("Listan är sparad.");
        }
        // Asked for: fix fel 6, layer 1. The catch was empty and caught everything, so a failed save
        // was hidden. Catch only the errors writing a file can give, and tell the user what happened.
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Programmet får inte skriva till {path}. Listan sparades inte.");
        }
        catch (IOException)
        {
            Console.WriteLine($"Filen {path} gick inte att skriva till, till exempel för att ett annat program använder den. Listan sparades inte.");
        }
    }

    // Reads the file back into the list.
    public void Load()
    {
        // Asked for: fix fel 2, layer 1. If the file is missing the program must still start:
        // tell the user and begin with an empty list instead of crashing.
        if (!File.Exists(path))
        {
            Console.WriteLine($"Filen {path} finns inte än. Listan börjar tom.");
            return;
        }

        // Asked for: fix fel 2, layer 2. The file can exist and still fail to read, for example when
        // another program has it locked or the user has no permission. Catch exactly those errors,
        // tell the user in Swedish and begin with an empty list instead of crashing.
        string[] lines;

        try
        {
            // Asked for: fix fel 1, layer 2. ReadAllLines splits at every kind of line break and removes it,
            // so no "\r" is left at the end of the names and searching for an item finds it.
            lines = File.ReadAllLines(path);
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Programmet får inte läsa {path}. Listan börjar tom, och sparar du skrivs filen över.");
            return;
        }
        catch (IOException)
        {
            Console.WriteLine($"Filen {path} gick inte att läsa, till exempel för att ett annat program använder den. Listan börjar tom, och sparar du skrivs filen över.");
            return;
        }

        foreach (string line in lines)
        {
            // Asked for: fix the crash at start, layer 1. An empty line holds no item and has no parts[1], so skip it.
            // Since layer 2 the file's last line break no longer gives an empty line, but a blank line inside the file still does.
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // Asked for: meet the requirement that the list looks the same after a restart. A name may itself
            // contain ";" (for example "Kaffe;bryggt"). Split at the first ";" only, into at most 2 parts,
            // so everything after it stays the name.
            string[] parts = line.Split(';', 2);

            // Asked for: fix fel 1, layer 3. A broken line in the file must not crash the program.
            // It needs a ";" (two parts) and a whole-number price; otherwise tell the user and skip the line.
            if (parts.Length < 2 || !int.TryParse(parts[0], out int price))
            {
                Console.WriteLine($"En rad i {path} kunde inte läsas och hoppades över: {line}");
                continue;
            }

            // Asked for: utbyggnad U1-1. A line with an empty name, like "15;", is refused by Item.
            // Skip that line with a message instead of crashing at start.
            try
            {
                items.Add(new Item(parts[1], price));
            }
            // Asked for: utbyggnad U1-2. A line with a negative price, like "-5;Ost", is refused by Item.
            // The more specific ArgumentOutOfRangeException must be caught before ArgumentException.
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine($"En rad i {path} har ett negativt pris och hoppades över: {line}");
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"En rad i {path} har ett tomt namn och hoppades över: {line}");
            }
        }
    }
}
