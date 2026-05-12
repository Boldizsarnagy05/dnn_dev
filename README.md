# NaturaCo DNN fejlesztés

Ez a repo a NaturaCo webshop egyetemi beadandójához készült.

A projekt egy DNN 9.13.10 alapú weboldalt bővít, amely Hotcakes Commerce-t használ. A cél egy egyszerű, de jól használható receptes funkció: a vásárló recepteket böngészhet, megnézheti a hozzávalókat, majd a webshopban létező termékeket kosárba teheti.

## Fő részek

- `Recept_modul/`  
  A DNN MVC receptmodul. Ez jeleníti meg a recepteket, a részletező oldalt, az adagolást és a kosár-előnézetet.

- `NaturaCo_Boundle_api/`  
  API modul a receptek mentéséhez, listázásához, betöltéséhez, publikálásához és visszavonásához.

## Fontos működés

- A recept Hotcakes kategóriaként van tárolva.
- A recept adatai a kategória leírásában, NaturaCo metadata formában vannak.
- A recept kategóriák Hotcakes oldalon rejtettek, ezért nem jelennek meg az ÉTLAP oldalon.
- A Recept modul a metadata `Status = Published` alapján jeleníti meg őket.
- Üres `ProductBvin` esetén a hozzávaló nem webshop termék: látszik a receptben, de nem tehető kosárba.

