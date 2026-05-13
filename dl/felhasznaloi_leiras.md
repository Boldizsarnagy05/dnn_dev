# NaturaCo Recipe Editor – Felhasználói leírás

## Rendszerkövetelmény

- Windows operációs rendszer
- .NET Framework 4.8

## Telepítés

1. Töltsd le az `app.zip` fájlt a weboldalról
2. Csomagold ki egy tetszőleges mappába
3. Futtasd a `NaturaCo.RecipeEditor.exe` fájlt – telepítés nem szükséges

---

## Az alkalmazás felépítése

Az alkalmazás három fő részre osztható:

**Bal panel – Termékkatalógus**
A webshop termékeit jeleníti meg kategóriánként. Innen lehet hozzávalókat hozzáadni a recepthez.

**Középső panel – Receptszerkesztő**
Itt szerkesztheted a recept adatait és hozzávalóit.

**Jobb panel – Receptlista**
Az összes meglévő recept listája névvel és állapottal.

---

## Új recept létrehozása

1. Kattints az **Új recept** gombra
2. Töltsd ki a mezőket:

| Mező | Leírás |
|------|--------|
| Recept neve | Kötelező |
| Leírás | Rövid bemutató szöveg |
| Étkezés típusa | Reggeli, Ebéd, Vacsora stb. |
| Elkészítési lépések | A recept menete |
| Adagok száma | Hány személyre szól |
| Előkészítési idő | Percben megadva |
| Főzési idő | Percben megadva |

---

## Hozzávalók kezelése

### Hozzávaló hozzáadása a katalógusból

1. A bal panelen válassz **kategóriát** a legördülő menüből
2. Keresd meg a terméket a keresőmezőben (opcionális)
3. **Dupla kattintás** a termékre – automatikusan bekerül a receptbe

### Egyedi hozzávaló hozzáadása

Ha egy hozzávaló nincs a katalógusban:

1. Kattints az **Egyedi hozzávaló** gombra
2. Add meg a **nevét**, **mennyiségét**, **mértékegységét** és a **kalóriatartalmat** (100 g-ra vonatkoztatva)
3. Kattints **OK**

### Hozzávaló törlése

Jelöld ki a sort a táblázatban, majd nyomd meg a **Delete** billentyűt, vagy kattints az **Eltávolítás** gombra.

### Mennyiség és mértékegység szerkesztése

A hozzávalók táblázatában a **Mennyiség**, **Mértékegység** és **Sorrend** oszlopok közvetlenül szerkeszthetők – kattints a cellára.

---

## Költség- és kalóriaszámítás

A szerkesztő alatt automatikusan frissülő összesítők láthatók:

- **Becsült költség** (Ft) – az összes hozzávaló ára
- **Adagonkénti költség** (Ft)
- **Össz. kalória** (kcal)
- **Adagonkénti kalória** (kcal)

---

## Recept mentése és publikálása

| Gomb | Mit csinál |
|------|-----------|
| **Mentés (tervezet)** | Elmenti a receptet, de nem jelenik meg a weboldalon |
| **Publikálás** | Megerősítés után megjelenik a webshopban |
| **Visszavonás** | Eltünteti a webshopból (az adatok megmaradnak) |

> Publikálás előtt a receptet el kell menteni tervezetként.

---

## Recept állapotok

| Állapot | Jelentés |
|---------|---------|
| **Tervezet** | Mentve, de nem látható a weboldalon |
| **Publikált** | Látható a webshopban |
| **Visszavont** | Korábban publikált, de most nem látható |

---

## Meglévő recept megnyitása

A jobb oldali listában kattints a recept nevére – az adatok automatikusan betöltődnek a szerkesztőbe.
