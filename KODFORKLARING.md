<!-- Asked for: put the farm explanation of the code in the repo as a code explanation, in flowing text that maps every part of the farm to the code. -->
# Koden som en bondgård

Här förklaras hela programmet som en bondgård. Varje del av gården motsvarar en del av koden, så man kan följa vad koden gör utan att först kunna C#.

## Gården i korthet

Programmet består av fyra filer, och varje fil är en egen del av gården:

- **Program.cs** är bonden.
- **ShoppingList.cs** är ladan.
- **Item.cs** är djuren.
- **InvalidLineException.cs** är en röd larmlapp som används när något i loggboken är fel.

Filen **items.txt**, där programmet sparar listan, är gårdens loggbok.

## Bonden: Program.cs

Bonden bestämmer i vilken ordning saker händer.

Först bygger bonden ladan med `new ShoppingList("items.txt", 1000)`. Det betyder: bygg en lada som använder loggboken items.txt och har ett tak på 1000 kr. Sedan läser bonden loggboken med `list.Load()`.

Efter det börjar bondens arbetsdag. Det är loopen `while (true)`, och den fortsätter tills bonden går och lägger sig. Varje varv ropar bonden först upp alla djuren med `list.Print()`. Sedan visas en meny med fem sysslor:

1. Lägga till ett djur.
2. Ta bort ett djur.
3. Spara i loggboken.
4. Söka efter ett djur.
5. Avsluta.

Väljer bonden att avsluta bryter `break` loopen, och programmet tar slut. Det händer också om det inte finns något mer att läsa, till exempel om någon trycker Ctrl+Z.

När bonden frågar efter en siffra kan någon svara med bokstäver. Då använder bonden `int.TryParse`. Det är som att bonden först tittar på svaret och, om det inte är en siffra, bara säger "skriv en siffra" i stället för att krascha.

## Ladan: ShoppingList.cs

Ladan håller ordning på djuren och har alla verktyg.

Inne i ladan finns **båsen**, `items`. Det är en lista där varje plats är ett djur. Listan är privat, så bara ladan själv får flytta djuren direkt.

Ladan vet också var loggboken står, och det sparas i `path`.

Ladan har ett **tak**, `Budget`. Taket bestämmer hur mycket alla djur får kosta tillsammans. Alla kan läsa hur högt taket är, men bara ladan själv får ändra det. Det är vad `private set` betyder.

Ladans verktyg är dess metoder:

**Grinden, `Add`**
När ett nytt djur ska in kollar grinden om priset ryms under taket tillsammans med alla andra djur.
- Om det ryms släpps djuret in, och grinden svarar `true`.
- Om det inte ryms svarar grinden `false`. Det är ett lugnt nej: ett för dyrt djur är inget fel, så grinden kastar inget undantag. Bonden ser nejet och säger hur mycket som finns kvar under taket.

**Utgången, `RemoveAt`**
Här tas ett djur bort genom att bonden säger numret på båset. Utgången kollar först att båset finns. Om det inte finns svarar den `false`, och bonden säger att numret inte finns. Bonden räknar båsen från 1 men listan räknar från 0, så utgången drar bort ett från numret.

**Vågen, `Total`**
Vågen lägger ihop priset på alla djur och börjar på bås 0. I den ursprungliga koden började den på bås 1, så det första djuret glömdes bort. Det var fel 5.

**Vallhunden, `Find`**
Hunden går igenom båsen ett och ett och letar efter ett djur med ett visst namn. Hittar den inget svarar den `null`, som betyder ingenting, och bonden säger att varan inte finns.

**Uppropet, `Print`**
Uppropet skriver ut alla djur med nummer och pris, till exempel "1. Mjölk - 15 kr", och sist vad allt kostar tillsammans.

## Loggboken: items.txt

Loggboken är filen där gården sparas mellan dagarna.

Första raden är taket, till exempel `budget;1000`. Efter det kommer ett djur per rad: priset, ett semikolon och namnet, till exempel `15;Mjölk`.

Två av ladans verktyg arbetar med loggboken, och de gör motsatta saker.

**Pennan, `Save`**
Pennan skriver i boken. Först skriver den taket och sedan alla djuren.

Pennan används inuti `using`. Det betyder att pennan alltid läggs tillbaka och boken alltid stängs, även om något går fel mitt i skrivandet.

Om boken är låst, eller om programmet inte får skriva i den, fångas det med `catch`. Då säger programmet ärligt att listan inte sparades. "Listan är sparad" skrivs bara när det verkligen gick.

**Morgonläsningen, `Load`**
Morgonläsningen gör tvärtom och läser boken. Det sker steg för steg:

1. Finns ingen bok börjar gården tom.
2. Står det `budget;` på första raden läser ladan taket med `ParseBudget` och bygger om taket. Är den raden trasig, eller taket negativt, kommer en larmlapp och taket stannar på 1000 kr.
3. Varje rad efter det läses med `ParseLine`, som gör om texten till ett djur. Tomma rader hoppas över.
4. Djuret går sedan in genom samma grind, `Add`. Därför gäller taket även när boken läses, och ett djur som inte ryms hoppas över med ett meddelande.

## Djuren: Item.cs

Varje djur har två saker: ett namn, `Name`, och ett pris, `Price`.

När ett djur skapas med `new Item` kontrollerar det sig självt direkt, ungefär som en veterinär vid födseln:
- Om namnet är tomt kastas ett `ArgumentException`.
- Om priset är negativt kastas ett `ArgumentOutOfRangeException`.

Ett djur som inte följer reglerna kan alltså aldrig finnas på gården. Ett djur som kostar 0 kr är tillåtet.

`ToString` är hur djuret presenterar sig, till exempel "Mjölk - 15 kr".

## Larmlappen: InvalidLineException.cs

Larmlappen är gårdens egen sorts larm för en rad i loggboken som inte går att läsa.

Den ärver från `Exception`, så den är en sorts vanligt larm. Den har en extra sak, radnumret `LineNumber`, så att man ser exakt vilken rad som var fel.

Ibland ligger veterinärens ursprungliga larm inuti lappen. Det kallas `InnerException`. Ett exempel: ett negativt pris i boken får `Item` att larma, och `ParseLine` gör om det larmet till en larmlapp med radnumret. Veterinärens larm följer med inuti lappen.

När `Load` får en larmlapp skriver programmet ut vilken rad som var fel och varför. Sedan hoppar det över raden och läser vidare.

## Stängslen och näten: try och catch

Runt allt som kan gå fel finns stängsel och nät:
- `try` är att försöka göra något.
- `catch` är nätet som fångar larmet om det går fel.

Med dem kan programmet förklara felet i stället för att krascha.

## Så hänger allt ihop

| Del av gården | Del av koden | Vad den gör |
|---|---|---|
| Bonden | `Program.cs` | Bestämmer vad som händer |
| Ladan | `ShoppingList` | Sköter djuren med sina verktyg |
| Djuren | `Item` | Ser till att de själva är rätt |
| Loggboken | `items.txt` | Sparar allt mellan dagarna |
| Larmlappen | `InvalidLineException` | Berättar exakt vilken rad som var fel |
