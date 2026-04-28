# NaturaCo DNN / Hotcakes projekt context

## Dokumentum celja

Ez a fajl atadasi es fejlesztoi context a NaturaCo DNN / Hotcakes projekthez.

Celja, hogy ha egy uj fejleszto vagy AI agent csak ezt olvassa el, akkor ertse:

- mi a projekt celja;
- milyen kornyezetben fut;
- hogyan kapcsolodik a DNN, Hotcakes, API es recept MVC modul;
- milyen dontesek szulettek a chatben;
- milyen endpointok es modellek vannak;
- hogyan kell tesztelni es telepiteni;
- mire kell vigyazni branch, packaging es Hotcakes integracio kozben.

Ez a dokumentum a beszelgetesben osszegyult informaciokat, a repo aktualis szerkezetet es a korabbi fejlesztesi donteseket egyesiti.

## Projekt roviden

A projekt egy DNN 9.13.10 alatt futo, Hotcakes Commerce-t hasznalo webapphoz keszul.

A webapp neve / marka: **NaturaCo**.

A NaturaCo egy premium, high-end elelmiszereket es kiegeszitoket arusito ceg weboldala. A weboldal hangulata:

- letisztult;
- premium;
- feher / sotet / bordo / sarga elemekkel dolgozik;
- a jelenlegi site design sok ures teret, kozepre rendezett navigaciot es finom kartyas elemeket hasznal;
- a webshop reszt Hotcakes kezeli.

A projekt egyetemi beadando, ezert a megoldasnak:

- elegge komplexnek kell lennie a jo ertekeleshez;
- de nem szabad tulbonyolitott, senior-enterprise szintu architekturanak lennie;
- egyszeru DNN MVC + Razor + kis JavaScript + Hotcakes integracio a cel.

## Fontos business context

A NaturaCo tortenete szerint a ceg 2026-ban alakult, negy fiatal vallalkozoval. Celjuk, hogy kulonleges, magas minosegu, elethez nelkulozhetetlen termekeket aruljanak, premium vasarloknak.

Ertekek:

- minoseg;
- premium termekek;
- szakertokkel valogatott termekpaletta;
- etikus es fenntarthato partnerek;
- luxus kompromisszum nelkul.

Ez a recept modul ezert nem egy olcso / gyors kajas oldal, hanem premium etel- es receptajanlo modul, amely a Hotcakes termekekhez kapcsolodik.

## Technologiai kornyezet

Ismert kornyezet:

- DNN: `9.13.10`
- Hotcakes Commerce: hasznalva a webshophoz
- .NET Framework: `net48` / DNN modul fejlesztesi kornyezet
- Fejlesztesi repo root: `c:\DNN\Dev\Website\DesktopModules\Teszko`
- Server URL: `http://20.107.242.91/`

A repo tartalmaz egy `dnn` mappat, amely 1/1 ugyanaz, mint a productionben levo DNN fajlrendszer megfelelo resze.

Fontos referencia dokumentumok a rootban:

- `DNN_MVC.pdf`
- `Hotcakes_Api_reference.pdf`
- `MODULE_CONTEXT.md`
- `Modulfejlesztés - Gyakorlati tanácsok.pdf`
- `megvalosithatosagi_tanulmanyok/Megvalosithatosagi_Tanulmany_Recept_Modul.docx`

Fejlesztesnel a `DNN_MVC.pdf` es a `Modulfejlesztés - Gyakorlati tanácsok.pdf` alapelvei a fontosak.

## Repo fo reszei

### `Recept_modul`

Ez a DNN MVC storefront modul.

Feladata:

- kulon DNN oldalon megjelenni;
- receptlistat mutatni;
- filterezni etkezestipus szerint;
- recept reszletezo oldalt mutatni;
- hozzavalokat kezelni;
- kosar elonezetet mutatni;
- kivalasztott hozzavalokat Hotcakes kosarba kuldeni.

Aktualis modul verzio:

- `Recept_modul/Recept_modul.dnn`: `00.00.02`
- aktualis install ZIP: `Recept_modul/install/Recept_modul_00.00.02_Install.zip`

Az SQL provider tovabbra is:

- `Recept_modul/Providers/DataProviders/SqlDataProvider/00.00.01.SqlDataProvider`

Ez szandekos. Nem volt DB schema valtozas, ezert nem kell `00.00.02.SqlDataProvider`.

### `NaturaCo_Boundle_api`

Ez a DNN API modul.

Feladata:

- HTTP API endpointokat adni a kulso kliensalkalmazasnak;
- receptet menteni Hotcakes category-kent;
- hozzavalokat Hotcakes product kapcsolatokkal kezelni;
- opcionálisan bundle termeket kezelni;
- publish / revoke muveleteket adni;
- metadata-t menteni a Hotcakes category description mezobe.

Megjegyzes: a mappanevben a `Boundle` eliras maradt, de ez csak elnevezesi sajatossag.

Aktualis API package:

- `NaturaCo_Boundle_api/Package/NaturaCo-api.zip`

### `Teszko_Dnn_HelloWorld_Items`

Korabbi / demo DNN MVC modul. Nem ez a NaturaCo recept modul fo kodja.

### `design_mockups`

Design / Figma jellegu segedanyagok lehetnek itt.

### `megvalosithatosagi_tanulmanyok`

Beadandohoz es tervezeshez kapcsolodo dokumentumok. A recept modulrol a `Megvalosithatosagi_Tanulmany_Recept_Modul.docx` adott reszletesebb leirast.

## Branch-ekkel kapcsolatos fontos szabaly

Nagyon fontos: ne keverjuk az API es recept modul valtoztatasokat rossz branchen.

Volt egy eset, amikor a `feature/recept_modul` branchen elkezdodott API endpoint modositas. Ezt vissza kellett vonni.

Aktualis elv:

- `feature/recept_modul`: csak `Recept_modul` valtoztatasok.
- API endpoint / service / gateway modositas: csak API branch-en, peldaul `feature/api`.

Ha a user azt mondja, hogy "most api branch-en vagyok", akkor ellenorizni kell:

```powershell
git status --short --branch
```

Ne feltetelezzuk, hogy valoban azon a branchen vagyunk.

## NaturaCo recept adatmodell koncepcio

Hotcakesben nincs nativ "recipe" domain endpoint.

A `Hotcakes_Api_reference.pdf` alapjan letezik:

- category API;
- product API;
- category-product association API;
- order/cart API;
- de nincs `RecipeSync`, `Recipe/List`, `Recipe/Load` jellegu nativ recept API.

Ezert a NaturaCo recept sajat konvencio:

```text
recept = Hotcakes category + NATURACO_RECIPE_METADATA
hozzavalo = Hotcakes product
recept-hozzavalo kapcsolat = category-product association
```

A recept extra adatai a Hotcakes category `Description` mezobe kerulnek JSON metadata-kent, HTML comment markerrel:

```text
<!-- NATURACO_RECIPE_METADATA:{...}:NATURACO_RECIPE_METADATA -->
```

Ez azert kell, mert Hotcakes category onmagaban nem tudja a NaturaCo recepthez szukseges mezoket:

- meal type;
- adag;
- ido;
- kcal;
- hozzavalok mennyisege;
- hozzavalo kaloria;
- ar;
- kiszereles;
- sorrend;
- leiras / steps / tags.

## API modul reszletesen

### API route

A route mapper:

```text
/DesktopModules/NaturaCo/API/RecipeSync/{action}
```

Pelda:

```text
http://20.107.242.91/DesktopModules/NaturaCo/API/RecipeSync/Save
```

### API fo fajlok

- `NaturaCo_Boundle_api/Controllers/RecipeSyncController.cs`
- `NaturaCo_Boundle_api/Services/RecipeSyncService.cs`
- `NaturaCo_Boundle_api/Services/HotcakesRecipeGateway.cs`
- `NaturaCo_Boundle_api/Services/RecipeMetadataFormatter.cs`
- `NaturaCo_Boundle_api/Models/SaveRecipeRequest.cs`
- `NaturaCo_Boundle_api/Models/RecipeIngredientDto.cs`
- `NaturaCo_Boundle_api/Models/RecipeMetadata.cs`
- `NaturaCo_Boundle_api/Models/RecipeSyncResult.cs`

### Jelenlegi alap endpointok

Az alap API-ban ezek vannak:

- `POST Save`
- `POST Publish`
- `POST Revoke`

A controller:

```csharp
public IHttpActionResult Save(SaveRecipeRequest request)
public IHttpActionResult Publish(PublishRecipeRequest request)
public IHttpActionResult Revoke(RevokeRecipeRequest request)
```

### List / Load endpoint context

Volt egy kliensalkalmazas-fejlesztoi keres:

- legyen `GET List`
- legyen `GET Load(int id)`
- a server NaturaCo module controller fajljaba kell betenni a Save/Publish/Revoke melle.

A chatben tisztazva lett:

- Hotcakesben nincs ilyen kesz recept endpoint;
- a NaturaCo API-nak kell sajat wrapper;
- ha a kliens fejleszto oldalan mar van pontos `RecipeSyncController.cs` implementacio, akkor azt kell atvenni;
- ha nincs, akkor a Hotcakes category + metadata alapjan kell megirni.

Fontos: ezen dokumentum irasakor a `feature/recept_modul` branchen az API nincs modositva List/Load-ra. Ha List/Load kell, azt az API branchen kell implementalni vagy ellenorizni.

### SaveRecipeRequest

Aktualis fontos mezok:

```csharp
int? RecipeId
string RecipeName
string ShortDescription
string Description
string Steps
string Tags
int Servings
int PrepTimeMinutes
int CookTimeMinutes
int? TotalCalories
decimal? EstimatedCost
string AuthorName
string PreviewImageUrl
string MealType
string Status
string CategoryBvin
string BundleBvin
bool CreateOrUpdateBundle
bool PublishAfterSave
List<RecipeIngredientDto> Ingredients
```

### RecipeIngredientDto

Fontos mezok:

```csharp
string ProductBvin
string ProductName
decimal Quantity
string Unit
int? Calories
decimal? Price
decimal? PackageQuantity
string PackageUnit
int SortOrder
```

### Makrok es kcal

Korabbi Figma tervben volt feherje / szenhidrat / zsir makro panel.

Frissitett Figma alapjan:

- makrok kikerultek;
- kcal maradt;
- kcal a recepthez es hozzavalokhoz kell;
- a modul jelenlegi UI-ja kcal alapu.

### API validacio

`RecipeSyncService.ValidateSave` ellenorzi:

- request body letezik;
- `RecipeName` kotelezo;
- `Servings > 0`;
- legalabb egy ingredient kell;
- minden ingredienthez `ProductBvin` kell;
- minden ingredient `Quantity > 0`.

### Save endpoint hasznalata tesztre

Ha tesztre receptet kell letrehozni, a `Save` endpoint eleg. Nem kell kulon test endpoint, ha a kliens app vagy PowerShell teljes recept payloadot kuld.

Pelda PowerShell skeleton:

```powershell
$baseUrl = "http://20.107.242.91"
$productBvin = "VALOS_HOTCAKES_PRODUCT_BVIN"

$body = @{
    RecipeId = 101
    RecipeName = "Teszt reggeli recept"
    ShortDescription = "Teszt recept a recept modul kiprobalasahoz."
    Description = "<p>Ez egy teszt recept.</p>"
    Steps = "1. Elokeszites`n2. Talalas"
    Tags = "teszt"
    Servings = 2
    PrepTimeMinutes = 10
    CookTimeMinutes = 0
    TotalCalories = 320
    EstimatedCost = 1000
    MealType = "reggeli"
    Status = "Published"
    PublishAfterSave = $true
    CreateOrUpdateBundle = $false
    Ingredients = @(
        @{
            ProductBvin = $productBvin
            ProductName = "Teszt hozzavalo"
            Quantity = 1
            Unit = "db"
            Calories = 320
            Price = 1000
            PackageQuantity = 1
            PackageUnit = "db"
            SortOrder = 1
        }
    )
} | ConvertTo-Json -Depth 10

Invoke-RestMethod `
    -Uri "$baseUrl/DesktopModules/NaturaCo/API/RecipeSync/Save" `
    -Method Post `
    -ContentType "application/json; charset=utf-8" `
    -Body $body
```

### Product BVIN kikeresese

Hotcakes adminban:

1. SuperUser login.
2. Hotcakes / Store Admin.
3. Catalog -> Products.
4. Termek szerkesztese.
5. Browser URL-ben keresd az `id` / `bvin` / `productbvin` parameter erteket.

Ez kell `ProductBvin`-nek.

## Recept MVC modul reszletesen

### Modul celja

A `Recept_modul` egy kulon DNN oldalon futo, teljes oldalas recept storefront.

Elvart oldal:

- DNN adminban kulon oldal: `Receptek` vagy `Recept`;
- csak ez a modul legyen az oldalon;
- a site navigationben a kosar ikon elott jelenjen meg;
- a modul ne rontsa el a DNN skin / Hotcakes style-jait.

### Modul fo fajlok

- `Recept_modul/Controllers/ItemController.cs`
- `Recept_modul/Components/RecipeStorefrontService.cs`
- `Recept_modul/Components/RecipeMetadataFormatter.cs`
- `Recept_modul/Models/RecipeViewModels.cs`
- `Recept_modul/Views/Item/Index.cshtml`
- `Recept_modul/Views/Item/Details.cshtml`
- `Recept_modul/module.css`
- `Recept_modul/Recept_modul.dnn`

### Controller

`ItemController`:

- `Index(string mealType = "all")`
- `Details(string id)`
- `AddProductsToCart(AddRecipeCartRequest request)`

### Lista oldal

`Views/Item/Index.cshtml`

UI:

- cim: `Receptek`
- filterek:
  - `MINDEN`
  - `REGGELI`
  - `EBÉD`
  - `VACSORA`
  - `SNACKEK`
- desktopon 4 oszlopos kartya grid;
- mobilon reszponziv;
- kartyan:
  - receptnev;
  - kcal badge;
  - ido;
  - adag;
  - `Recept megtekintése`.

Frissitett Figma miatt nincs makro panel a kartyakon.

### Reszletezo oldal

`Views/Item/Details.cshtml`

UI:

- bal oldalon recepttartalom;
- jobb oldalon sotet kosar / kcal panel;
- vissza gomb;
- receptnev;
- kcal badge;
- ido es adag;
- HTML-kent renderelt leiras;
- adagvalaszto;
- `+ Mindent a kosárba` gomb;
- hozzavalo tablazat:
  - osszetevo;
  - mennyiseg;
  - kaloria;
  - kosar gomb;
- jobb oldali kosar elonezet:
  - ures allapot;
  - soronkent termek;
  - plusz/minusz;
  - vegosszeg;
  - `TERMÉKEK KOSÁRHOZ ADÁSA`.

### Fontos UI fixek, amelyek mar megtortentek

1. Basic Hotcakes termekkategoriak ne jelenjenek meg receptkent.

   Megoldas:

   - csak olyan category jelenik meg, amelynek `Description` mezojeben benne van a `NATURACO_RECIPE_METADATA` marker.
   - emiatt az `Egészséges zsírok`, `Italok`, `Fehérjeforrás`, stb. sima termekkategoriak nem latszodnak.

2. Linkek ne legyenek kekek kattintas utan.

   Megoldas:

   - `module.css` scoped visited/hover/focus szabalyok:
     - `.nc-filter`
     - `.nc-card-link`
     - `.nc-back`

3. Description ne literal HTML-kent jelenjen meg.

   Megoldas:

   - `@Html.Raw(Model.Description)` a detail view-ban.

4. Ha nincs termek/recept, a modul nyuljon le az oldal aljaig.

   Megoldas:

   - `module.css` rooton:

   ```css
   --nc-page-offset: 150px;
   display: flex;
   flex-direction: column;
   min-height: calc(100vh - var(--nc-page-offset));
   ```

   - `.nc-recipe-list`:

   ```css
   flex: 1 0 auto;
   ```

5. Korabbi full-width breakout CSS vissza lett vonva.

   Volt egy probalkozas `width: 100vw` / `margin-left: calc(50% - 50vw)` jellegu CSS-re, de az a DNN oldalon lebegesi / layout problemakat okozott. Ezt vissza kellett venni.

### Modul Hotcakes integracio

`RecipeStorefrontService` reflection alapon dolgozik Hotcakes belso osztalyokkal.

Fobb lepesek:

- `TryGetHotcakesApp`;
- DNN PortalSettings / Hotcakes request context inicializalas;
- store feloldas;
- category listazas;
- metadata marker szures;
- category -> recipe viewmodel map;
- category-product kapcsolatok betoltese;
- product adatokkal kiegeszites;
- Hotcakes cartba kuldes.

### Kosarba helyezes

Frontend:

- a detail oldalon JS preview objektumot epit;
- egy hozzavalo vagy minden hozzavalo hozzaadhato preview-ba;
- mennyisegek plusz/minusz allithatok;
- a vegso gomb POST-ol `AddProductsToCart` actionre.

Backend:

- `AddProductsToCart` -> `RecipeStorefrontService.AddToCart`;
- `TryAddLineToCart` Hotcakes LineItem-et hoz letre;
- Hotcakes order/cart API-n keresztul probalja menteni.

Volt bug:

- UI azt irta, hogy kesz, de Hotcakes kosar oldal ures maradt.

Javitas:

- a kod mar `AddToOrderWithCalculateAndSave` / `AddToOrderAndSave` / `CalculateOrderAndSave` jellegu mentest is probal, nem csak memoriaban adja a cart objecthez.

Tesztelesnel fontos:

- valos `ProductBvin` kell;
- ha demo/fallback ProductBvin van, a preview mukodhet, de a valodi Hotcakes cart nem fog frissulni.

## UI / Figma context

### Eredeti elkepzeles

Figma terv alapjan:

- kozepre rendezett lista;
- filter gombok;
- kartyas receptek;
- reszletezo oldal jobb oldali sotet panellel;
- sarga CTA gombok;
- bordo kcal/kosar panelek.

### Friss Figma valtozasok

Frissitesek:

- makrok kikerultek;
- csak kcal maradt;
- jobb oldali kosar design valtozott;
- `+ Mindent a kosárba` gomb bekerult a receptcsomag adag sor kozepere;
- a kartyakon nincs makro panel, csak ido + adag + kcal.

### Site designhez igazodas

A modulnak nem kell pixel-perfect Figma-only appnak lennie, hanem illeszkednie kell a korabban bekuldott NaturaCo weboldalhoz:

- feher ter;
- fekete/sotet text;
- bordo highlight;
- sarga kosar CTA;
- minimal, premium kartya design;
- ne legyen tul harsany SPA-s.

## Telepites es teszteles

### Recept modul telepites

Telepitheto ZIP:

```text
Recept_modul/install/Recept_modul_00.00.02_Install.zip
```

DNN-ben:

1. SuperUser login.
2. Extensions / Install Extension Wizard.
3. ZIP feltoltese.
4. Telepites.
5. Uj oldal letrehozasa: `Receptek` vagy `Recept`.
6. Csak a `Recept_modul` keruljon erre az oldalra.
7. Menu sorrendben a kosar ikon elott legyen.

### API telepites

Telepitheto ZIP:

```text
NaturaCo_Boundle_api/Package/NaturaCo-api.zip
```

Ha csak DLL-t kell frissiteni:

- build utan `NaturaCo.RecipeSyncApi.dll`;
- production DNN `/bin` ala kell kerulnie;
- DNN app pool restart / recycle szukseges lehet.

### Build parancsok

API:

```powershell
dotnet build NaturaCo_Boundle_api\NaturaCo.RecipeSyncApi.csproj
```

Recept modul Debug build:

```powershell
dotnet msbuild Recept_modul\Recept_modul.csproj /p:Configuration=Debug /p:VSToolsPath="" /p:VisualStudioVersion=17.0 /p:OutputPath=bin\Debug\
```

Megjegyzes:

- Release packaging a regi MSBuild Community Tasks fuggosegek miatt akadhat;
- a csomagolas korabban kezzel lett megoldva `Compress-Archive`-val;
- a ZIP belsejeben a manifest verziojat mindig ellenorizni kell.

### Ismert build warningok

Elnezheto / ismert warningok:

- `AssemblyInfo.cs` version format: `00.00.01.*`
- DNN `PortalAliasInfo.HTTPAlias` obsolete warning.

Ezek nem build-blockerek.

## Fontos teszt esetek

### Receptlista

Ellenorizni:

- `Receptek` oldal betolt;
- csak metadata-val jelolt receptek latszanak;
- basic Hotcakes termekkategoriak nem latszanak;
- filterek mukodnek;
- visited linkek nem kekulnek el;
- ures lista eseten az oldal magassaga normalis.

### Recept reszletezo

Ellenorizni:

- kartyarol reszletezo nyilik;
- leiras HTML-kent jelenik meg, nem literal `<p>...</p>`;
- adag noveles/csokkentes frissiti:
  - adag szoveget;
  - mennyisegeket;
  - kcal ertekeket;
  - preview-t.

### Kosar

Ellenorizni:

- egy hozzavalo preview-ba teheto;
- minden hozzavalo preview-ba teheto;
- preview plusz/minusz mukodik;
- vegso kosar gomb utan a Hotcakes kosar oldal valoban tartalmazza a termeket.

Ha nem:

- ellenorizd, hogy valos Hotcakes `ProductBvin` van-e;
- ne demo BVIN legyen;
- ellenorizd, hogy a termek elerheto es kosarba teheto Hotcakesben.

## Fontos dontesek es buktatok

### Save endpoint eleg teszt recept letrehozasara

Felmerult kulon endpoint:

- minden category-hoz teszt termek hozzaadasa;
- majd leszedese.

Vegul tisztazva:

- ha teljes recept payload kuldheto, a `Save` endpoint eleg;
- nem kell uj API endpoint csak emiatt;
- foleg ne a `feature/recept_modul` branchen.

### API branch es modul branch ne keveredjen

Ha API-t kell fejleszteni:

- eloszor branch ellenorzes;
- csak API branchen nyulj `NaturaCo_Boundle_api` ala.

Ha recept modult kell fejleszteni:

- csak `Recept_modul` ala nyulj;
- API-t ne modosits.

### Verzioszam

Mivel a szerveren mar fent lehetett `Recept_modul` `00.00.01`, a modul manifest verzioja `00.00.02` lett.

DB scriptet nem kellett emelni, mert nem volt schema valtozas.

### Encoding

A forrasban latszodhatnak mojibake jellegu karakterek PowerShell outputban, peldaul:

- `fÅ‘re`
- `kosÃ¡r`

Ez sokszor konzol encoding megjelenitesi gond. Ugyanakkor ha a browserben is hibas az ekezet, akkor fajl encodingot kell javitani UTF-8-ra.

Korabbi user feedback:

- `megtekintse` helyett `megtekintése`;
- `fore` helyett `főre`;
- link ne legyen kek.

Ezekre figyelni kell.

## Mit kell tudnia egy uj fejlesztonek

1. Ez DNN + Hotcakes integracio, nem standalone ASP.NET app.
2. A recept domain nem letezik Hotcakesben nativan.
3. A recept category + metadata konvencioval mukodik.
4. A recept MVC modul csak metadata-val jelolt category-kat jelenit meg.
5. A sima Hotcakes termekkategoriakat nem szabad receptkent mutatni.
6. A kosarba rakashoz valos Hotcakes ProductBvin kell.
7. A `Save` endpoint teljes receptet ment; teszt receptek letrehozasara hasznalhato.
8. A `List/Load` endpoint igeny valid, de API branch-en kell kezelni.
9. A `feature/recept_modul` branchen ne modosits API-t.
10. A telepitheto recept modul jelenlegi csomagja `00.00.02`.

## Gyors aktualis allapot

E dokumentum keszitesekor ismert allapot:

- aktiv branch a terminal szerint: `feature/recept_modul`;
- `Recept_modul.dnn` verzio: `00.00.02`;
- regi `Recept_modul_00.00.01_Install.zip` torolve / lecserelve;
- uj `Recept_modul_00.00.02_Install.zip` letrehozva;
- API alatt nem maradhat recept_modul branchhez tartozo diff.

Ellenorzeshez:

```powershell
git status --short --branch
```

Ha `NaturaCo_Boundle_api` alatt diff latszik a recept modul branch-en, azt tisztazni kell, mielott tovabb dolgozol.

