# Kunskapskontroll 2: Robust inköpslista

Startkoden är en inköpslista i C# som sparar varorna i `items.txt` mellan körningarna. Den byggde utan fel men fungerade inte. Här rättas felen ett i taget, i små steg.

## Kör programmet

Du behöver .NET 10 SDK. Öppna en terminal i mappen och kör:

```console
dotnet run
```

## Flödesschema

[Klickbart flödesschema över startkoden](https://aliteex55-bit.github.io/Del1-2brooor/flodesschema.html). Röda etiketter som 1-1 visar var felen fanns, fel och lager. I läget Lösning blir de gröna och visar koden efter lagningen.

## Felrapport

### Fel 1: programmet kraschade direkt vid start

**Vad hände:** programmet kraschade med `IndexOutOfRangeException` så fort det startade. `Save` skriver alltid en radbrytning sist, så även en fil som programmet själv hade sparat gav samma krasch vid nästa start.

**Varför:** `items.txt` har Windows-radbrytningar, `\r\n`, och slutar med en radbrytning. `Load` delade texten med `Split('\n')`. Det gav en tom sista rad, och en tom rad har ingen `parts[1]`. Samma delning lämnade `\r` kvar sist i varje namn, så sökningen hittade inga varor och `Save` lade till ett `\r` till vid varje sparning. En trasig rad i filen, till exempel `abc;Ost`, kraschade också.

**Så löstes det, i tre steg:**

1. Tomma rader hoppas över med `string.IsNullOrWhiteSpace` och `continue` ([f322a88](https://github.com/aliteex55-bit/Del1-2brooor/commit/f322a88)).
2. Filen läses med `File.ReadAllLines`, som tar bort hela radbrytningen. Namnen blir rätt och sökningen fungerar ([0d9247f](https://github.com/aliteex55-bit/Del1-2brooor/commit/0d9247f)).
3. En rad utan `;`, eller med ett pris som inte är ett heltal, hoppas över med ett meddelande. Priset läses med `int.TryParse` i stället för `int.Parse` ([e3dd039](https://github.com/aliteex55-bit/Del1-2brooor/commit/e3dd039)).

**Provat:** programmet startar med lärarens `items.txt`, sökningen hittar varorna som syns i listan, listan ser likadan ut efter sparning och omstart, och tomma eller trasiga rader kraschar inte.

### Fel 2: programmet kraschade när items.txt saknades

**Vad hände:** fanns inte `items.txt`, till exempel efter att man döpt om den, kraschade programmet direkt med `FileNotFoundException`. En fil som fanns men inte gick att läsa kraschade också: en fil som ett annat program hade låst gav `IOException`, och en fil som programmet saknade läsrätt till gav `UnauthorizedAccessException`.

**Varför:** `Load` läste filen utan att kontrollera att den fanns, och utan att hantera de fel som kan uppstå när en fil läses.

**Så löstes det, i två steg:**

1. Finns inte filen säger programmet till och börjar med en tom lista. Filen skapas första gången man sparar ([e81c5ac](https://github.com/aliteex55-bit/Del1-2brooor/commit/e81c5ac)).
2. Läsningen ligger i ett `try`, med en `catch` för `UnauthorizedAccessException` och en för `IOException`. Båda skriver ett svenskt meddelande, varnar för att en sparning skriver över filen och börjar med en tom lista ([d9a76a5](https://github.com/aliteex55-bit/Del1-2brooor/commit/d9a76a5)).

**Provat:** programmet startar utan `items.txt`, och efter sparning och omstart finns varorna kvar. En låst fil och en fil utan läsrätt ger ett meddelande i stället för en krasch.

### Fel 3: bokstäver i stället för tal kraschade programmet

**Vad hände:** menyvalet, priset och numret lästes med `int.Parse`. Bokstäver, en tom rad eller ett decimaltal gav `FormatException`, ett för stort tal gav `OverflowException`, och när inmatningen tog slut gav `int.Parse(null)` `ArgumentNullException`.

**Varför:** `int.Parse` kastar ett undantag när texten inte är ett heltal, och inmatning från en användare kan alltid bli fel.

**Så löstes det, i tre steg:**

1. Menyvalet läses med `int.TryParse`. Är det inte en siffra skrivs "Skriv en siffra mellan 1 och 5." och menyn visas igen. Tar inmatningen slut avslutas programmet i stället för att fråga om och om igen ([b8b8f7b](https://github.com/aliteex55-bit/Del1-2brooor/commit/b8b8f7b)).
2. Priset läses med `int.TryParse`. Är det inte ett heltal säger programmet till och lägger inte till någon vara ([ac710d1](https://github.com/aliteex55-bit/Del1-2brooor/commit/ac710d1)).
3. Numret läses med `int.TryParse`. Är det inte ett heltal säger programmet till och tar inte bort något ([871d175](https://github.com/aliteex55-bit/Del1-2brooor/commit/871d175)).

**Provat:** bokstäver, en tom rad, decimaltal, för stora tal och slut på inmatningen ger ett meddelande i stället för en krasch, både i menyn, i priset och i numret. Ett nummer som är ett heltal men inte finns i listan, till exempel 9, hör till fel 4.

### Fel 4: ett nummer som inte fanns i listan kraschade programmet

**Vad hände:** tog man bort ett nummer som inte fanns kraschade programmet med `ArgumentOutOfRangeException`. Det gällde till exempel `9` när listan hade två varor, `0`, `-1` och `1` i en tom lista.

**Varför:** `RemoveAt` tog bort på platsen `number - 1` utan att kontrollera att den platsen fanns. En plats i listan måste ligga mellan 0 och antalet varor minus 1.

**Så löstes det, i två steg:**

1. `RemoveAt` i `ShoppingList` kontrollerar att numret ligger mellan 1 och antalet varor. Annars tas ingenting bort, och metoden svarar `false` ([3e66885](https://github.com/aliteex55-bit/Del1-2brooor/commit/3e66885)).
2. `Program.cs` använder svaret och skriver att numret inte finns när det blev `false` ([b40382c](https://github.com/aliteex55-bit/Del1-2brooor/commit/b40382c)).

**Varför `false` och inte ett undantag:** ett nummer som inte finns händer ofta när man skriver fel, och programmet behöver bara säga till. Därför räcker ett enkelt `false`.

**Provat:** `9`, ett nummer för mycket, `0`, `-1` och `1` i en tom lista ger ett meddelande i stället för en krasch. Numren 1 och 2 tar bort rätt vara.

### Fel 5: totalsumman hoppade över första varan

**Vad hände:** totalsumman blev för låg. Med lärarens tre varor, 15 + 32 + 89 kr, visade programmet 121 kr i stället för 136 kr. Med en enda vara blev summan 0 kr. Programmet kraschade inte, så felet märktes bara om man räknade själv.

**Varför:** en lista räknar platser från 0, så första varan ligger på `items[0]`. Loopen i `Total` började på `i = 1`, så `items[0]` räknades aldrig med.

**Så löstes det:** loopen börjar på `i = 0` i stället för `i = 1` ([031bc55](https://github.com/aliteex55-bit/Del1-2brooor/commit/031bc55)).

**Provat:** lärarens `items.txt` ger 136 kr, en vara för 20 kr ger 20 kr, och en tom lista ger 0 kr. Summan stämmer också efter att en vara lagts till eller tagits bort.

### Fel 6: programmet sa att listan var sparad fast det misslyckades

**Vad hände:** gick det inte att spara skrev programmet ändå "Listan är sparad.". Med en skrivskyddad `items.txt`, och med en fil som ett annat program höll öppen, fick man bekräftelsen, men den nya varan kom aldrig in i filen. Felet syns inte när man kör programmet som vanligt, bara när sparandet misslyckas.

**Varför:** `catch`-blocket i `Save` var tomt och fångade alla sorters fel, så felet försvann utan ett ord. Raden med "Listan är sparad." låg efter `try` och `catch` och kördes därför alltid.

**Så löstes det, i två steg:**

1. Den tomma `catch` blev två specifika `catch`, en för `UnauthorizedAccessException` och en för `IOException`. Båda säger vad som gick fel och att listan inte sparades ([810efa5](https://github.com/aliteex55-bit/Del1-2brooor/commit/810efa5)).
2. "Listan är sparad." flyttades in i `try`, direkt efter `File.WriteAllText`. Kastar skrivningen ett undantag körs raden aldrig, så bekräftelsen syns bara när det gick ([f560715](https://github.com/aliteex55-bit/Del1-2brooor/commit/f560715)).

**Provat:** en vanlig fil ger "Listan är sparad.", och varan finns i filen. En skrivskyddad fil och en fil som ett annat program har öppen ger bara meddelandet att listan inte sparades, och filen är oförändrad.

## Utbyggnad

### U1: Item vägrar ogiltiga värden

**Vad krävdes:** konstruktorn i `Item` ska vägra ett tomt namn med `ArgumentException` och ett negativt pris med `ArgumentOutOfRangeException`, i stället för att tyst skapa ett trasigt objekt. Programmet ska inte krascha, och användaren ska få veta vad som hände.

**Före:** ett tomt namn lades till som en vara utan namn, och priset -5 lades till som "Te - -5 kr". Rader som `15;` och `-5;Ost` i `items.txt` blev också sådana varor.

**Så gjordes det, i två steg:**

1. Tomt namn: `Item` kastar `ArgumentException`. `Program.cs` fångar det och säger att namnet inte får vara tomt, och `Load` hoppar över raden med ett meddelande ([9359127](https://github.com/aliteex55-bit/Del1-2brooor/commit/9359127)).
2. Negativt pris: `Item` kastar `ArgumentOutOfRangeException`. `Program.cs` och `Load` fångar det i en egen `catch` före `catch (ArgumentException)`. Ordningen behövs eftersom `ArgumentOutOfRangeException` är en sorts `ArgumentException`, och programmet tar den första `catch` som passar ([039d36c](https://github.com/aliteex55-bit/Del1-2brooor/commit/039d36c)).

**Provat:** ett tomt namn, ett namn med bara mellanslag och priserna -5 och -1 ger ett meddelande, och ingen vara läggs till. 0 kr är tillåtet. I filen hoppas `15;` och `-5;Ost` över med meddelanden, och resten av listan läses in.

U2 med budgettaket, designvalet och klassdiagrammet läggs till här efter hand.
