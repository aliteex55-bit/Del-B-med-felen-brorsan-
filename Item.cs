// One item on the shopping list.
class Item
{
    public string Name { get; set; }
    public int Price { get; set; }

    public Item(string name, int price)
    {
        // Asked for: utbyggnad U1-1. The item protects itself: an empty name is refused with
        // ArgumentException instead of quietly creating a broken object.
        // IsNullOrWhiteSpace is also true for null and for a name of only spaces.
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Namnet får inte vara tomt.", nameof(name));
        }

        Name = name;
        Price = price;
    }

    public override string ToString()
    {
        return $"{Name} - {Price} kr";
    }
}
