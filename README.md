# Kunskapskontroll 2: Robust inköpslista

Startkoden är en inköpslista i C# som sparar varorna i `items.txt` mellan körningarna. Den byggde utan fel men fungerade inte. Här rättas felen ett i taget, i små steg.

## Kör programmet

Du behöver .NET 10 SDK. Öppna en terminal i mappen och kör:

```console
dotnet run
```

## Flödesschema

[Klickbart flödesschema över startkoden](https://aliteex55-bit.github.io/Del-B-med-felen-brorsan-/flodesschema.html). Röda siffror visar var felen fanns, och en grön bock visar lösningen med koden efter lagningen.

## Felrapport

### Fel 1: programmet kraschade direkt vid start

**Vad hände:** programmet kraschade med `IndexOutOfRangeException` så fort det startade. `Save` skriver alltid en radbrytning sist, så även en fil som programmet själv hade sparat gav samma krasch vid nästa start.

**Varför:** `items.txt` har Windows-radbrytningar, `\r\n`, och slutar med en radbrytning. `Load` delade texten med `Split('\n')`. Det gav en tom sista rad, och en tom rad har ingen `parts[1]`. Samma delning lämnade `\r` kvar sist i varje namn, så sökningen hittade inga varor och `Save` lade till ett `\r` till vid varje sparning. En trasig rad i filen, till exempel `abc;Ost`, kraschade också.

**Så löstes det, i tre steg:**

1. Tomma rader hoppas över med `string.IsNullOrWhiteSpace` och `continue` ([f322a88](https://github.com/aliteex55-bit/Del-B-med-felen-brorsan-/commit/f322a88)).
2. Filen läses med `File.ReadAllLines`, som tar bort hela radbrytningen. Namnen blir rätt och sökningen fungerar ([0d9247f](https://github.com/aliteex55-bit/Del-B-med-felen-brorsan-/commit/0d9247f)).
3. En rad utan `;`, eller med ett pris som inte är ett heltal, hoppas över med ett meddelande. Priset läses med `int.TryParse` i stället för `int.Parse` ([e3dd039](https://github.com/aliteex55-bit/Del-B-med-felen-brorsan-/commit/e3dd039)).

**Provat:** programmet startar med lärarens `items.txt`, sökningen hittar varorna som syns i listan, listan ser likadan ut efter sparning och omstart, och tomma eller trasiga rader kraschar inte.

Fel 2–6, designvalet och klassdiagrammet läggs till här efter hand.
