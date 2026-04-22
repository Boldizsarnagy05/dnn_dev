# NaturaCo Recept Szerkesztő – WinForms kliens

Ez a dokumentum leírja a `WinFormsApp` projekt felépítését és azt, hogy melyik összetevő miért felelős. A kliens célja, hogy a NaturaCo szerkesztőinek asztali felületet biztosítson receptek készítéséhez, mentéséhez, publikálásához és visszavonásához – miközben a HotCakes webshop termékkatalógusából válogatnak összetevőket.

## Architektúra áttekintés

A rendszer háromrétegű:

1. **WinForms kliens** (ez a projekt) – szerkesztői felület, csak HTTP-n keresztül kommunikál.
2. **HotCakes REST API** (`https://naturaco.hu` alá telepítve) – termék- és kategórialekérdezésekhez.
3. **Saját DNN Web API** (`/api/RecipeModule/*`) – recept CRUD, publikálás, visszavonás, későbbiekben bundle kezelés.

A kliens **soha nem ér hozzá közvetlenül az adatbázishoz**. Minden írás és olvasás API-n keresztül történik, mert:
- az SQL Server 1433-as portja nyilvános interneten nem elérhető,
- a JWT-alapú jogosultságkezelés DNN oldalon van megvalósítva,
- így a kliens telepítés nélkül bárhonnan használható, ahol internet van.

```
WinFormsApp ── HTTP ──▶ HotCakes REST API  ─┐
            ── HTTP ──▶ DNN Web API         ├─▶ SQL Server (naturaco)
                        (RecipeModule)      ┘
```

## Projektstruktúra

```
WinFormsApp/
├── Program.cs                 ← alkalmazás belépési pont
├── Models/
│   └── RecipeModels.cs        ← DTO-k: Recipe, RecipeIngredient, HccProduct, HccCategory
├── Services/
│   ├── HotCakesService.cs     ← HotCakes REST API wrapper (olvasás)
│   └── RecipeApiService.cs    ← saját DNN Web API wrapper (írás + auth)
└── Forms/
    ├── LoginForm.cs           ← DNN JWT bejelentkezés
    └── MainForm.cs            ← recept szerkesztő fő ablak
```

## Összetevők felelőssége

### `Program.cs` – alkalmazás belépési pont
- Beállítja a WinForms futtatási környezetet (`EnableVisualStyles`, `SetCompatibleTextRenderingDefault`).
- Tárolja a konfigurációs konstansokat: `StoreUrl`, `ApiKey` (HotCakes), `DnnApiUrl`.
- Megnyitja a `LoginForm`-ot; ha sikeres a bejelentkezés, példányosítja a két service-t, és elindítja a `MainForm`-ot.
- **Miért külön lépésben?** A `RecipeApiService`-t csak akkor hozzuk létre, ha van érvényes JWT token – így a `MainForm` már hitelesített állapotban kapja meg.

### `Models/RecipeModels.cs` – adatátviteli objektumok
Négy osztályt tartalmaz:

| Osztály | Felelősség | Forrás/cél |
|---|---|---|
| `Recipe` | Recept fejadatok + összetevők listája. | DNN Web API (`RecipeRecipes` tábla). |
| `RecipeIngredient` | Egy összetevő sor (név, mennyiség, mértékegység, opcionális HotCakes termékhivatkozás). | DNN Web API (`RecipeIngredients` tábla). |
| `HccProduct` | HotCakes termék leképezett formája (`bvin`, név, ár, slug, kép). | HotCakes REST API. |
| `HccCategory` | HotCakes kategória (`bvin`, név). | HotCakes REST API. |

Fontos mezők:
- `Recipe.Status` – `"Draft" | "Published" | "Revoked"` háromállapotú életciklus.
- `Recipe.BundleBvin` – ha a publikáláskor bundle is létrejön, ide kerül a hivatkozás (egyelőre null).
- `Recipe.EstimatedCost` – a kliens számolja újra minden összetevő-változáskor.
- `RecipeIngredient.ProductID` – nullable `Guid`, mert nem minden összetevő köthető webshop termékhez (pl. „só ízlés szerint").
- `RecipeIngredient.LinkedProductName / LinkedProductPrice` – csak megjelenítésre, **nem kerül adatbázisba**.

### `Services/HotCakesService.cs` – HotCakes REST API wrapper
A `Hotcakes.CommerceDTO.v1.Client.Api` osztályra épülő vékony absztrakciós réteg, kizárólag **olvasásra**.

Publikus metódusok:
- `GetCategories()` – összes kategória lekérése névsorba rendezve.
- `GetProductsByCategory(bvin, page, pageSize)` – kategóriához tartozó termékek lapozva.
- `GetAllProducts(page, pageSize)` – teljes termékkatalógus lapozva.
- `FindProduct(bvin)` / `FindProductBySlug(slug)` – egy termék pontos lekérése.
- `CalculateEstimatedCost(ingredients)` – ár-összegzés szerveroldali adatok alapján (jelenleg a kliens cache-ből dolgozik, ezért opcionális).

**Bundle-kezelés szándékosan kikommentálva**: a HotCakes REST API ProductDTO-ja nem tartalmaz `IsBundle` mezőt, és nincs `BundledProductsCreate()` metódus az `Api` osztályban. A `hcc_BundledProducts` tábla csak belső SDK-n vagy közvetlen SQL-en keresztül tölthető – ezt a DNN Web API fogja lefedni, ha az oldal elkészül. A kód eleje részletesen dokumentálja a hivatkozást a `Hotcakes.CommerceDTO.dll`-re.

### `Services/RecipeApiService.cs` – saját DNN Web API wrapper
`HttpClient`-re épülő szolgáltatás, minden írási műveletért és autentikációért felelős.

Publikus metódusok:
- `LoginAsync(user, pass)` – DNN JWT token megszerzése (`/api/JWT/Login`), majd minden további kérésbe automatikusan beállítja a `Bearer` fejlécet.
- `GetRecipesAsync()` / `GetRecipeAsync(id)` – recept lista és egy recept lekérése.
- `SaveRecipeAsync(recipe)` – új recept esetén `POST`, meglévőnél `PUT`. A szerver által visszaadott példányt adja vissza (így az új `RecipeID` is bekerül a kliens állapotba).
- `DeleteRecipeAsync(id)` – fizikai törlés (ritkán használt, jellemzően csak tervezetre).
- `PublishRecipeAsync(id)` – státuszváltás `Draft → Published`. A kommentelt kódban látszik, hogy ide fog majd bekerülni a bundle-létrehozás hívása.
- `RevokeRecipeAsync(id)` – státuszváltás `Published → Revoked`.

**Miért külön service-ben van az auth?** Mert a JWT token a `HttpClient` default header-ében él, így példányonként kell kezelni. A `LoginForm` hozza létre a példányt, és átadja a `MainForm`-nak.

### `Forms/LoginForm.cs` – bejelentkező ablak
- Bekéri a felhasználónevet/jelszót, meghívja a `RecipeApiService.LoginAsync`-et.
- Sikeres belépés után beállítja az `AuthenticatedRecipeService` property-t, és `DialogResult.OK`-val zár.
- Hiba esetén állapotsort és MessageBox-ot mutat, újraengedélyezi a Belépés gombot.
- **Miért `DialogResult.OK` mintázat?** A `Program.cs` így tud könnyen különbséget tenni „sikeres belépés" és „mégsem" között, anélkül hogy globális állapotot kezelnénk.

### `Forms/MainForm.cs` – fő szerkesztőablak
Ez az alkalmazás szíve. Felelősségei:

1. **Kategória- és termékböngésző** (bal oldali panel):
   - `MainForm_Load` → `LoadCategoriesAsync` betölti a kategóriákat ComboBoxba.
   - `cmbCategory_SelectedIndexChanged` → `LoadProductsForCategory` frissíti a terméklistát.
2. **Összetevők szerkesztése** (középső panel):
   - `lstProducts_DoubleClick` → `AddIngredient` hozzáadja a receptet a kiválasztott termékkel.
   - `RefreshIngredientGrid` rajzolja újra a DataGridView-t `SortOrder` szerint.
   - `RecalculateTotals` frissíti a `Becsült költség` és `Adagonként` címkéket.
3. **Recept metaadatok** (jobb oldali panel):
   - `ReadFormToRecipe` átmásolja a TextBox/NumericUpDown értékeket a `_currentRecipe` modellbe.
4. **Életciklus műveletek** (alsó gombsor):
   - `btnSaveDraft_Click` – mentés tervezetként (`Status = "Draft"`).
   - `btnPublish_Click` – megerősítéssel publikálás; a kommentelt részben látszik, hogy a bundle-visszajelzés itt kapna helyet.
   - `btnRevoke_Click` – megerősítéssel visszavonás.
5. **Állapotkezelés**: a `_currentRecipe` mező az aktuálisan szerkesztett receptet tartja, a `_allProducts` az utoljára betöltött kategóriát cache-eli.

**Miért `async void` az eseménykezelőkben?** Ez a WinForms-ban elfogadott minta: a felhasználói események `void` visszatérésűek, és a `try/catch` blokk minden műveletnél biztosítja, hogy a kivételek MessageBoxban jelenjenek meg, ne szakítsák meg a UI szálat.

## Konfiguráció és függőségek

### NuGet csomagok
- `Newtonsoft.Json` – JSON szerializáció a `RecipeApiService`-ben.
- `System.Net.Http` – beépített, de a .NET Framework verziótól függően explicit kellhet.

### Lokális referencia
- `Hotcakes.CommerceDTO.dll` – nem NuGet-en, hanem a HotCakes telepítésből másolandó:
  `\DesktopModules\Hotcakes\Core\bin\Hotcakes.CommerceDTO.dll`

### Konfigurációs pontok (`Program.cs`)
- `StoreUrl` – HotCakes bolt URL-je (ugyanaz a domain, mint a DNN).
- `ApiKey` – HotCakes Admin → Configuration → API oldalon generált kulcs.
- `DnnApiUrl` – a DNN portál alap URL-je (ugyanaz, mint a `StoreUrl`, csak szemantikailag külön).

Éles telepítéshez ezeket érdemes `App.config`-ba kiszervezni (jelenleg konstansok – fejlesztési fázisban így egyszerűbb).

## Nyitott pontok / jövőbeli bővítés

| Funkció | Állapot | Teendő |
|---|---|---|
| Bundle létrehozás publikáláskor | Kommentelve, nem aktív. | DNN Web API oldalán saját végpont (`POST /Recipe/{id}/bundle`) és `hcc_BundledProducts` írás. |
| Bundle törlés visszavonáskor | Kommentelve, nem aktív. | DNN Web API oldalán `DELETE /Recipe/{id}/bundle`. |
| Tápértékek (fehérje, szénhidrát, zsír) | Csak `TotalCalories` van. | HotCakes termék custom property mezőkből összegző számítás. |
| Előnézeti kép feltöltés | `PreviewImageURL` mező létezik, de uploader nincs. | Külön fájl-upload végpont vagy DNN File API integráció. |
| Recept lista / keresés | `GetRecipesAsync` megvan, UI nincs. | `MainForm`-ra recept-lista panel (open/new). |
| App.config konfiguráció | Konstansok a `Program.cs`-ben. | `appSettings` szekció + `ConfigurationManager`. |
| Designer (`.Designer.cs`) fájlok | Nincsenek a repóban, csak a kód mögöttes részek. | Visual Studio generálja újra első megnyitáskor, vagy kézzel pótolni. |
