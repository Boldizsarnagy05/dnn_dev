# NaturaCo Recept Szerkesztő – Kliens adatmodell

---

## Recept (Recipe)

| Mező | UI elem | Típus |
|---|---|---|
| RecipeName | Recept neve (szövegdoboz) | string |
| Category | Kategória (legördülő: Reggeli / Ebéd / Vacsora / Nassolnivaló) | string |
| ShortDescription | — (nincs külön UI mező, Description első sora) | string |
| Description | Leírás (többsoros szöveg) | string |
| Steps | Elkészítési lépések (többsoros szöveg) | string |
| AuthorName | — (alapértelmezett, nem látható UI) | string |
| PreviewImageUrl | — | string |
| Tags | — | string |
| Servings | Adag (szám) | int |
| PrepTimeMinutes | Elkészítés (perc) | int |
| CookTimeMinutes | Sütés/főzés (perc) | int |
| TotalCalories | Össz. kalória (számított, csak megjelenítés) | int? |
| EstimatedCost | Becsült költség (számított, csak megjelenítés) | decimal? |
| Status | Draft / Published / Revoked | string |
| CategoryBvin | Szerver által visszaadott HotCakes azonosító | string |
| BundleBvin | Szerver által visszaadott HotCakes bundle azonosító | string |

---

## Hozzávaló (RecipeIngredient)

Egy recept több hozzávalót tartalmaz. A gridben jelenik meg.

| Mező | Forrás | Típus |
|---|---|---|
| IngredientName | Termék neve (HotCakes) vagy egyedi dialógus | string |
| Amount | Felhasználó állítja / dupla kattintáskor alapért. | decimal |
| Unit | g / dkg / kg / ml / dl / l / db / ek / tk / csipet / ízlés szerint | string |
| ProductBvin | HotCakes termék azonosítója (egyedi hozzávalónál üres) | string |
| SortOrder | Hozzáadás sorrendje | int |
| LinkedProductPrice | HotCakes SitePrice (Ft/csomag) – csak számításhoz | decimal |
| PricePerGram | HotCakes opciókból számított Ft/g – csak számításhoz | decimal |
| CaloriesPer100g | HotCakes CustomProperties – csak számításhoz | decimal |
| PackageQuantity | Csomag mérete (pl. 500) | decimal |
| PackageUnit | Csomag egysége (pl. g, db) | string |

---

