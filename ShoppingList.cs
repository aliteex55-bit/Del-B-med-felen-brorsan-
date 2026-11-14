// Holds the items and takes care of loading and saving them.
class ShoppingList
{
    private List<Item> items = new List<Item>();
    private string path;

    // Asked for: utbyggnad U2-1. The list has a budget: the most the whole list may cost, in whole kronor.
    // Asked for: utbyggnad U4-1. private set, so Load can take the budget saved in the file. Code
    // outside ShoppingList can still only read it.
    public int Budget { get; private set; }

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

        // Asked for: utbyggnad U4-1, save the budget in the file together with the items. It goes on the
        // first line, as "budget;1000", so Load can read it before any item is added.
        lines.Add($"budget;{Budget}");

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

        // Asked for: utbyggnad U4-1. If the first line is the saved budget, "budget;1000", use it and
        // start the items on the next line. A file saved before U4 has no such line, and then the
        // budget from the constructor (1000 kr) stays.
        int firstItemLine = 0;

        if (lines.Length > 0 && lines[0].StartsWith("budget;"))
        {
            // Asked for: utbyggnad U4-2. A budget line that cannot be read, or a negative budget, must not
            // be ignored without a word or leave a budget nothing fits in. ParseBudget throws our own
            // InvalidLineException; say what was wrong and keep the budget from the constructor.
            try
            {
                Budget = ParseBudget(lines[0]);
            }
            catch (InvalidLineException e)
            {
                Console.WriteLine($"Rad {e.LineNumber} i {path} kunde inte läsas, {e.Message}: {lines[0]}. Budgettaket är {Budget} kr.");
            }

            firstItemLine = 1;
        }

        // Asked for: utbyggnad U3-1. A for loop instead of foreach, so the line number (i + 1) is known
        // and the message can say which line of the file was wrong.
        for (int i = firstItemLine; i < lines.Length; i++)
        {
            string line = lines[i];

            // Asked for: fix the crash at start, layer 1. An empty line holds no item and has no parts[1], so skip it.
            // Since layer 2 the file's last line break no longer gives an empty line, but a blank line inside the file still does.
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // Asked for: utbyggnad U1-1. A line with an empty name, like "15;", is refused by Item.
            // Skip that line with a message instead of crashing at start.
            try
            {
                // Asked for: utbyggnad U2-3. Load must respect the budget too, so it goes through Add
                // instead of putting the item straight into the list. A line whose item does not fit is
                // skipped with a message, like the other lines that cannot be used.
                if (!Add(ParseLine(line, i + 1)))
                {
                    Console.WriteLine($"En rad i {path} ryms inte i budgeten på {Budget} kr och hoppades över: {line}");
                }
            }
            // Asked for: utbyggnad U3-1. ParseLine throws our own InvalidLineException for a line that
            // cannot be read. Catch it, say which line it was and what was wrong, and skip the line.
            // Asked for: utbyggnad U3-2. This is now the only catch: ParseLine also turns Item's refusal
            // of an empty name (U1-1) or a negative price (U1-2) into an InvalidLineException.
            catch (InvalidLineException e)
            {
                Console.WriteLine($"Rad {e.LineNumber} i {path} kunde inte läsas och hoppades över, {e.Message}: {line}");
            }
        }
    }

    // Asked for: utbyggnad U4-2. Reads the budget from the first line, "budget;1000". A value that is not
    // a whole number, or is negative, throws InvalidLineException for line 1.
    private int ParseBudget(string line)
    {
        if (!int.TryParse(line.Substring("budget;".Length), out int budget))
        {
            throw new InvalidLineException(1, "budgettaket är inte ett heltal");
        }

        if (budget < 0)
        {
            throw new InvalidLineException(1, "budgettaket får inte vara negativt");
        }

        return budget;
    }

    // Asked for: utbyggnad U3-1. Turns one line of the file into an item. A line that cannot be read
    // throws our own InvalidLineException, with the line number and what was wrong.
    private Item ParseLine(string line, int lineNumber)
    {
        // Asked for: meet the requirement that the list looks the same after a restart. A name may itself
        // contain ";" (for example "Kaffe;bryggt"). Split at the first ";" only, into at most 2 parts,
        // so everything after it stays the name.
        string[] parts = line.Split(';', 2);

        // Asked for: fix fel 1, layer 3. A broken line in the file must not crash the program.
        // It needs a ";" (two parts) and a whole-number price.
        if (parts.Length < 2)
        {
            throw new InvalidLineException(lineNumber, "det saknas ett ; mellan pris och namn");
        }

        if (!int.TryParse(parts[0], out int price))
        {
            throw new InvalidLineException(lineNumber, "priset är inte ett heltal");
        }

        // Asked for: utbyggnad U3-2. Item refuses an empty name or a negative price with its own
        // exceptions (U1). Turn them into an InvalidLineException with the line number, and keep
        // Item's exception as the inner exception. The more specific ArgumentOutOfRangeException
        // must be caught before ArgumentException, as in U1-2.
        try
        {
            return new Item(parts[1], price);
        }
        catch (ArgumentOutOfRangeException e)
        {
            throw new InvalidLineException(lineNumber, "priset är negativt", e);
        }
        catch (ArgumentException e)
        {
            throw new InvalidLineException(lineNumber, "namnet är tomt", e);
        }
    }
}
