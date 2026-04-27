# NaturaCo Recipe Editor – kliens app context

## 1. Mire valo ez a projekt

Ez a WinForms alkalmazas a NaturaCo (DNN + Hotcakes) webshop **kulso szerkesztoi
eszkoze**. Egy szerkeszto / katalogus-felelos a sajat gepen szerkesztheti meg a
recepteket, hozzakothet Hotcakes-termekeket mint hozzavalokat, majd egyetlen
gombnyomassal feltoltheti, publikalhatja, vagy visszavonhatja a webshopbol.

Szerveroldalon a recept egy **Hotcakes category**-kent jelenik meg, a
hozzavalok pedig a category-product kapcsolatokon keresztul kerulnek a
recepthez. Ez a leforditasi reteg a NaturaCo sajat DNN moduljaban
(`/DesktopModules/NaturaCo/API/RecipeSync/`) tortenik – a kliens csak egy
egyszeru JSON contractot lat.

A reszletes uzleti es szerveroldali kontextus forrasa:
`/Users/dorituran/Downloads/CLIENT_APP_CONTEXT.md`. Az itteni implementacio
pontosan azt a contractot koveti.

## 2. Architektura egy abran

```
+-----------------------+     HTTP POST JSON     +---------------------------+
|   WinForms kliens     | ---------------------> | DNN modul (NaturaCo)      |
|   (ez a projekt)      |    /RecipeSync/Save    |  - validacio              |
|                       |    /RecipeSync/Publish |  - Hotcakes store context |
|                       |    /RecipeSync/Revoke  |  - category create/update |
+-----------+-----------+                        |  - ingredient kapcsolatok |
            |                                    |  - bundle (opcionalis)    |
            |  Hotcakes REST                     +-------------+-------------+
            |  (csak olvasas:                                  |
            |   kategoriak, termekek)                          v
            v                                          +---------------+
   +-------------------+                               | Hotcakes      |
   | Hotcakes store    | <---------------------------- | Commerce DB   |
   +-------------------+                               +---------------+
```

Ket kulonbozo szerveroldali API-t hivunk:

- **Hotcakes REST** (`/DesktopModules/Hotcakes/Core/REST/v1`) – csak olvasas a
  termekkatalogus bongeszesehez. API key + storeUrl.
- **NaturaCo RecipeSync** (`/DesktopModules/NaturaCo/API/RecipeSync`) – minden
  receptmuvelet (Save / Publish / Revoke). Jelenleg `AllowAnonymous`, nincs
  auth.

## 3. Mappastruktura

```
WinFormsApp/
  App.config                 - StoreUrl, ApiKey, NaturaCoApiUrl
  Program.cs                 - belepesi pont, DI-szeru bekotes
  Forms/
    MainForm.cs              - editor logika, validacio, API hivasok
    MainForm.Designer.cs     - layout (NEM modositando)
    MainForm.resx
  Models/
    RecipeModels.cs          - editor modell + RecipeSync DTO-k
  Services/
    HotCakesService.cs       - Hotcakes REST wrapper (kategoriak, termekek)
    RecipeApiService.cs      - NaturaCo RecipeSync HTTP kliens
  ../lib/Hotcakes.CommerceDTO.dll   - Hotcakes telepitobol bemasolva
```

## 4. Komponensek felelossege

### 4.1 `Program.cs`
- Beolvassa az App.config kulcsait.
- Letrehozza a ket service-t (`HotCakesService`, `RecipeApiService`).
- Atadja oket a MainForm konstruktoranak (sima konstruktor injection).
- **Korai kilepes**, ha a `NaturaCoApiUrl` nincs konfiguralva – nem akarunk
  null reference-t a Save kozben.

### 4.2 `App.config`
Harom kulcs:
- `StoreUrl` – a Hotcakes katalogus host-ja (pl. `https://naturaco.hu`).
- `ApiKey` – Hotcakes Admin -> Configuration -> API Key.
- `NaturaCoApiUrl` – a NaturaCo DNN modul host-ja **path nelkul**
  (pl. `http://localhost`, `http://20.107.242.91`, `https://naturaco.hu`).
  A `/DesktopModules/NaturaCo/API/RecipeSync/...` szakaszt a kliens fuzi
  hozza – local es remote kornyezetben ugyanaz a kod fut, csak a host
  cserelodik (CLIENT_APP_CONTEXT.md – "Hogyan lehet remoteban hasznalni").

### 4.3 `Models/RecipeModels.cs`
Ket fajta tipus el itt egymas mellett:

**(a) Editor / UI modell** – csak a kliens szamara letezik:
- `Recipe` – a szerkesztoi munkakopia. Tartalmazza az osszes form-mezo-t es
  ket szerveroldali "horgonyt": `CategoryBvin`, `BundleBvin`. Ezeket Save utan
  toltjuk fel a szerver valaszabol.
- `RecipeIngredient` – a `dgvIngredients` GridView modellje. `ProductBvin`-t
  is tarol, hogy a Save-kor a ProductBvin-en alapulo DTO-ra tudjuk leforditani.
- `HccProduct`, `HccCategory` – csak a katalogusbongeszeshez.

**(b) NaturaCo RecipeSync DTO-k** – pontosan a szerveroldali contract
megfeleloi (CLIENT_APP_CONTEXT.md – "Save vegpont reszletesen"):
- `SaveRecipeRequest`
- `RecipeIngredientDto`
- `PublishRecipeRequest`
- `RevokeRecipeRequest`
- `RecipeSyncResult`

A ket reteg szandekosan kulon van:
- a UI / editor sajat allapotat tudja szabadon valtoztatni anelkul, hogy a
  szerveroldali contract elromlana,
- es forditva: a contract bovulhet anelkul, hogy az editor logika serulne.

### 4.4 `Services/HotCakesService.cs`
Vekony wrapper a Hotcakes `Hotcakes.CommerceDTO.v1.Client.Api` osztalya korul:
- `GetCategories()` – kategoriak listaja a `cmbCategory`-be.
  **Hidden=true** kategoriak (a NaturaCo modul ilyenkent hozza letre a
  recepteket) ki vannak szurve – csak valodi termekkategoriakat latunk.
- `GetProductsByCategory(bvin)` – egy kategoria termekei a `lstProducts`-ba.
- `FindProduct(bvin)` – egy termek lekerdezese (ar / nev visszaolvasas).
- `GetPricePerGramOrZero(product)` – a Hotcakes
  `/productoptions?byproduct={bvin}` vegpontjabol kiolvassa a "Csomag:"
  opcio default item-jet (`IsDefault=true`), abbol grammot parsol
  (`500 g`, `1 kg`), es Ft/g-ot szamol. Ha nincs ertelmezheto adat,
  0-t ad vissza es a szamolas darab-logikara esik vissza.
- `CalculateEstimatedCost(...)` – tartalek aggregator, ha nincs cache-elt
  ar (az editor egyebkent a `LinkedProductPrice` / `PricePerGram` cache-bol szamol).

Ez a service kizarolag **olvas**, ide nem irunk receptet.

### 4.5 `Services/RecipeApiService.cs`
A NaturaCo modul HTTP kliense:
- `SaveAsync(SaveRecipeRequest)` -> `RecipeSyncResult`
- `PublishAsync(PublishRecipeRequest)` -> `RecipeSyncResult`
- `RevokeAsync(RevokeRecipeRequest)` -> `RecipeSyncResult`

Egysegesen kezeli a sikert, a 400-as validacios hibakat es a halozati
hibakat: minden esetben `RecipeSyncResult`-et ad vissza, igy a UI mindig
ugyanazt a `Message` + `Errors` parost tudja mutatni
(CLIENT_APP_CONTEXT.md – "Hibas valaszok").

### 4.6 `Forms/MainForm.cs`
A teljes szerkesztoi workflow itt el. Felelos:
- a Hotcakes katalogus betolteseert es szuregyteseert,
- a hozzavalok hozzaadasaert (lstProducts dupla kattintas -> AddIngredient),
- a `dgvIngredients` 4-oszlopos beallitasaert (Hozzavalo / Mennyiseg /
  Mertekegyseg / Sorrend) – az egyeb mezok (ProductBvin, LinkedProductPrice,
  PricePerGram) modellben tarolodnak, de nem latszanak,
- a **kategoria-fuggo Unit es ar-logikaert**: a `GramCategoryBvins` halmazba
  rakott Hotcakes kategoriak (jelenleg Feherjeforras es Rost es vitamin)
  termekei automatikusan `Unit="g"` es `100` alapertekkel kerulnek a gridbe;
  ezekre `_hccService.GetPricePerGramOrZero` szamol Ft/g-ot, igy a koltseg
  `Amount × Ft/g`. Egyeb kategorianal `Unit="db"` es darab-logika fut
  (`Amount × Ft/db`),
- a grid CellValueChanged eventjevel az ar mindig automatikusan ujraszamol,
  amikor a felhasznalo a Mennyiseget vagy Mertekegyseget atirja,
- a `Recipe` editor-modell osszerakasaert a form mezoibol (`ReadFormToRecipe`),
- a **kliens-oldali validacioert** (`ValidateForSave`), amely ugyanazokat a
  szabalyokat ervenyesiti, mint a szerver:
  - `RecipeName` kotelezo,
  - `Servings > 0`,
  - legalabb 1 hozzavalo,
  - minden hozzavalo: `ProductBvin` ki van toltve, `Quantity > 0`,
- a `SaveRecipeRequest` osszerakasaert (`BuildSaveRequest`),
- a szerver-valasz feldolgozasaert (`ApplyServerResult`):
  - sikeres mentes -> `CategoryBvin`, `BundleBvin`, `Status` visszairodik az
    editor-modellbe,
  - hiba -> `Message` + `Errors` MessageBox-ban,
- a Publish / Revoke gomb gating-eert: csak ha mar van `CategoryBvin`,
- a state vizualis jelzeseert (`SetStatus`, `lblStatus`).

### 4.7 `Forms/MainForm.Designer.cs`
A vizualis layoutot tartalmazza. **Ezt a fajlt nem szabad kezzel atszerkeszteni**
– a felhasznalo egyszeru, lapos, panel nelkuli elrendezest kert. A logika
kizarolag a `MainForm.cs`-ben modosul, igy a designer gond nelkul ujrageneralhato
a Visual Studio designerrel is.

## 5. API contract – ahogy a kliens hasznalja

Minden vegpont:
```
POST {NaturaCoApiUrl}/DesktopModules/NaturaCo/API/RecipeSync/{Save|Publish|Revoke}
Content-Type: application/json
```

### 5.1 Save
Pelda body (a kliens ezt allitja elo `BuildSaveRequest`-bol):
```json
{
  "RecipeId": null,
  "RecipeName": "Teszt recept",
  "ShortDescription": "...",
  "Description": "...",
  "Steps": "...",
  "Tags": "teszt",
  "Servings": 2,
  "PrepTimeMinutes": 10,
  "CookTimeMinutes": 5,
  "TotalCalories": 250,
  "EstimatedCost": 1200,
  "AuthorName": "Tesztelo",
  "PreviewImageUrl": "",
  "Status": "Draft",
  "CategoryBvin": "",
  "BundleBvin": "",
  "CreateOrUpdateBundle": false,
  "PublishAfterSave": false,
  "Ingredients": [
    {
      "ProductBvin": "TEST-PRODUCT-1",
      "ProductName": "Teszt termek",
      "Quantity": 1,
      "Unit": "db",
      "SortOrder": 1
    }
  ]
}
```

Sikeres valasz:
```json
{
  "Success": true,
  "RecipeId": null,
  "CategoryBvin": "....",
  "BundleBvin": "",
  "Status": "Draft",
  "Message": "A recept mentese sikeres volt.",
  "Errors": []
}
```

A kliens **kotelezoen** elteszi a `CategoryBvin`-t (es ha jott `BundleBvin`-t).
Nelkuluk a kesobbi Publish / Revoke nem hivhato.

### 5.2 Publish / Revoke
```json
{
  "RecipeId": null,
  "CategoryBvin": "IDE_A_CATEGORY_BVIN",
  "BundleBvin": ""
}
```

A kliens megakadalyozza, hogy `CategoryBvin` nelkul ezek meghivasra
keruljenek (`btnPublish_Click`, `btnRevoke_Click` elejen).

## 6. Tipikus szerkesztoi workflow

1. App indul – kategoriak betoltodnek a `cmbCategory`-be.
2. Felhasznalo kivalaszt egy kategoriat -> `lstProducts` feltoltodik.
3. Dupla kattintassal hozzaad termekeket a `dgvIngredients`-be.
4. Kitolti a recept mezoit (nev, leiras, adag, idok, kaloria, stb.).
5. **Mentes tervezetkent** -> `Save` (Status=Draft, PublishAfterSave=false)
   -> `CategoryBvin` visszairodik.
6. (opcionalis) Publikalas -> `Publish`.
7. (opcionalis kesobb) Visszavonas -> `Revoke`.

## 7. Validacio

Ket retegben validalunk:

- **Kliens** (`ValidateForSave` / btn elejen): azonnali visszajelzes a
  felhasznalonak, mielott halozatot terhelnenk.
- **Szerver**: ugyanezeket meg egyszer ellenorzi, hibaknal HTTP 400-zal es
  `RecipeSyncResult`-tel valaszol.

A ket reteg ugyanazt a halmazt nezi (CLIENT_APP_CONTEXT.md – "Save validacio"):
- `RecipeName` nem lehet ures,
- `Servings > 0`,
- legalabb 1 ingredient,
- minden ingredienthez kell `ProductBvin` es `Quantity > 0`.

## 8. Hibakezeles

`RecipeApiService` minden valaszt `RecipeSyncResult`-be csomagol, beleertve a
halozati / parsolasi hibakat is. A UI ennek megfeleloen csak ket agon kezel:
- `Success == true` -> `Message` (vagy default uzenet) info dialog.
- `Success == false` -> `Message + Errors` warning dialog.

A `try/catch` az event handlerekben mar csak nem-varhato kivetelekre (pl.
service maga is dobott valamit) szol, nem a normal hiba-utra.

## 9. Konfiguracio – local vs. remote

Csak a **host** valtozik:

| Kornyezet | NaturaCoApiUrl                       |
|-----------|--------------------------------------|
| Local DNN | `http://localhost`                   |
| Remote    | `http://20.107.242.91` vagy hasonlo  |
| Production| `https://naturaco.hu`                |

A path-t soha ne kodold kemenyen, a service maga rakja ossze
(`BasePath = "/DesktopModules/NaturaCo/API/RecipeSync"`).

A Hotcakes katalogusbongeszes a `StoreUrl` + `ApiKey` parosa alapjan mukodik –
ez egy fuggetlen csatorna, akkor is hasznalhato local testkor, ha a
RecipeSync local DNN-en fut.

## 10. Auth – jelenlegi helyzet

A controller `AllowAnonymous`. A kliens ezert:
- nem kuld DNN cookie-t,
- nem kuld `RequestVerificationToken`-t,
- nem hasznal API kulcsot a NaturaCo modulnak.

Production iranyban erre kesobb auth-ot lehet rakni; az `RecipeApiService`
egy helyrol kuld minden kerest, igy a token / API key hozzaadasa egy ponton
beszurando.

## 11. Tervezett bovitesek (ha szuksegesse valnak)

- `CreateOrUpdateBundle = true` workflow felulet (ma a kliens fixen `false`-t
  kuld – CLIENT_APP_CONTEXT.md "Bundle logika").
- `PublishAfterSave = true` egylepeses publikalas a UI-bol.
- Recept-lista / megnyitas (`Get` vegpont jelenleg nincs az API-ban; ha
  bekerul, ide jon a "betoltes ID-rol" gomb).
- Auth reteg a controller production-osodese utan.

## 12. Mit ne tegyunk

- Ne kodoljuk be fixen a host-ot (CLIENT_APP_CONTEXT.md – "Kliensoldali
  implementacios tanacs").
- Ne hivjunk Publish / Revoke-ot `CategoryBvin` nelkul.
- Ne kezeljuk a `RecipeId`-t fo szerveroldali horgonykent – a szerver
  szempontjabol a `CategoryBvin` az igazi azonosito.
- Ne dolgozzuk at a designert kezzel ugy, hogy minden mezo elnevezeset
  felforgatjuk – a `MainForm.cs` a Designer-beli kontroll-neveket varja.

## 13. Forrasok

- `/Users/dorituran/Downloads/CLIENT_APP_CONTEXT.md` – a vezerlo specifikacio.
- Hotcakes Commerce DTO referencia: `..\lib\Hotcakes.CommerceDTO.dll`
  (a Hotcakes telepitobol: `\DesktopModules\Hotcakes\Core\bin\`).
