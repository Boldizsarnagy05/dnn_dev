from __future__ import annotations

import argparse
import html
import json
import re
import sys
import time
import unicodedata
import uuid
from pathlib import Path
from typing import Any
from urllib.error import HTTPError, URLError
from urllib.parse import urlparse
from urllib.request import Request, urlopen

DEFAULT_URLS = [
    "https://www.gal.hu/GAL-A-vitamin",
    "https://www.gal.hu/GAL-B-komplex",
    "https://www.gal.hu/GAL-Babavaro",
    "https://www.gal.hu/GAL-Babavaro-C-vitamin-negyedeves-kiszereles",
    "https://www.gal.hu/GAL-Babavaro-Hidrofil-negyedeves-kiszereles",
    "https://www.gal.hu/GAL-Babavaro-Lipofil-negyedeves-kiszereles",
    "https://www.gal.hu/GAL-Babavaro-negyedeves-kiszereles",
    "https://www.gal.hu/GAL-Borsmentaolaj",
    "https://www.gal.hu/GAL-C-komplex",
    "https://www.gal.hu/GAL-Csirkeporc-komplex",
    "https://www.gal.hu/GAL-Curcugreen",
    "https://www.gal.hu/GAL-Curcugreen-Forte",
    "https://www.gal.hu/GAL-D3-vitamin",
    "https://www.gal.hu/GAL-E-vitamin",
    "https://www.gal.hu/GAL-Fenu-C",
    "https://www.gal.hu/GAL-Flora-rost",
    "https://www.gal.hu/GAL-Glicin-250g",
    "https://www.gal.hu/GAL-Glicin-500g",
    "https://www.gal.hu/GAL-Halkollagen-peptidek",
    "https://www.gal.hu/GAL-Jod-komplex",
    "https://www.gal.hu/GAL-K-komplex-vitamin",
    "https://www.gal.hu/GAL-K-komplex-D3-vitamin",
    "https://www.gal.hu/GAL-K-komplex-D3-Forte-vitamin",
    "https://www.gal.hu/GAL-K-komplex-Forte-vitamin",
    "https://www.gal.hu/GAL-K1-vitamin",
    "https://www.gal.hu/GAL-Kolin-so-cseppek",
    "https://www.gal.hu/GAL-Kurkuma-komplex",
    "https://www.gal.hu/GAL-Laktoferrin-plus",
    "https://www.gal.hu/GAL-Magnezium-biszglicinat",
    "https://www.gal.hu/GAL-Magnezium-biszglicinat-180-db",
    "https://www.gal.hu/GAL-Magnezium-L-laktat",
    "https://www.gal.hu/GAL-Marhakollagen-peptidek",
    "https://www.gal.hu/GAL-Marhakollagen-Peptidek-Klasszik",
    "https://www.gal.hu/GAL-Multivitamin",
    "https://www.gal.hu/GAL-Multivitamin-C-vitamin-csaladi-kiszereles",
    "https://www.gal.hu/GAL-Multivitamin-csaladi-kiszereles",
    "https://www.gal.hu/GAL-Multivitamin-Hidrofil-csaladi-kiszereles",
    "https://www.gal.hu/GAL-Multivitamin-Lipofil-csaladi-kiszereles",
    "https://www.gal.hu/GAL-Omega-3-Eco",
    "https://www.gal.hu/GAL-PrimaVie-Shilajit-60",
    "https://www.gal.hu/GAL-Q10-MCT",
    "https://www.gal.hu/GAL-Q10-Lazac",
    "https://www.gal.hu/GAL-Relax",
    "https://www.gal.hu/GAL-Sensoril-Ashwagandha",
    "https://www.gal.hu/GAL-Serteskollagen-peptidek",
    "https://www.gal.hu/GAL-Shoden-Ashwagandha",
    "https://www.gal.hu/GAL-Taurin-por",
    "https://www.gal.hu/GAL-UC-II-Porc-komplex",
    "https://www.gal.hu/GAL-Halolaj",
    "https://www.gal.hu/GAL-plusz-Babavaro",
    "https://www.gal.hu/GAL-plusz-Multivitamin",
    "https://www.gal.hu/TITOK-Halkollagen-MSM",
    "https://www.gal.hu/TITOK-Multivitamin",
    "https://www.gal.hu/TITOK-Multivitamin-Halkollagen-MSM",
]


# ---------------------------------------------------------------------------
# HTTP / HTML helpers
# ---------------------------------------------------------------------------

def fetch_html(url: str, retries: int = 3, delay: float = 1.0) -> str:
    last_error: Exception | None = None
    for attempt in range(1, retries + 1):
        try:
            request = Request(url, headers={"User-Agent": "Mozilla/5.0"})
            with urlopen(request, timeout=30) as response:
                return response.read().decode("utf-8", errors="replace")
        except (HTTPError, URLError, TimeoutError) as exc:
            last_error = exc
            if attempt == retries:
                break
            time.sleep(delay * attempt)
    raise RuntimeError(f"Nem sikerult letolteni: {url} ({last_error})")


def find_first(pattern: str, text: str, flags: int = re.S | re.I) -> str:
    match = re.search(pattern, text, flags)
    return match.group(1).strip() if match else ""


def clean_html_fragment(fragment: str) -> str:
    if not fragment:
        return ""
    text = fragment
    text = re.sub(r"<br\s*/?>", "\n", text, flags=re.I)
    text = re.sub(r"</p\s*>", "\n\n", text, flags=re.I)
    text = re.sub(r"<li[^>]*>", "- ", text, flags=re.I)
    text = re.sub(r"</li\s*>", "\n", text, flags=re.I)
    text = re.sub(r"</tr\s*>", "\n", text, flags=re.I)
    text = re.sub(r"</td\s*>", "\t", text, flags=re.I)
    text = re.sub(r"<[^>]+>", "", text)
    text = html.unescape(text)
    text = text.replace("\xa0", " ")
    text = re.sub(r"\r", "", text)
    text = re.sub(r"\n{3,}", "\n\n", text)
    text = re.sub(r"[ \t]+", " ", text)
    text = re.sub(r" *\n *", "\n", text)
    return text.strip()


def text_to_html(text: str) -> str:
    """Plain text → egyszerű HTML konverzió (info tab tartalomhoz)."""
    if not text:
        return ""
    escaped = (
        text.replace("&", "&amp;")
            .replace("<", "&lt;")
            .replace(">", "&gt;")
    )
    paragraphs = re.split(r"\n\n+", escaped)
    parts = []
    for para in paragraphs:
        para = para.strip()
        if para:
            para = para.replace("\n", "<br />")
            parts.append(f"<p>{para}</p>")
    return "".join(parts)


def slugify(value: str) -> str:
    normalized = unicodedata.normalize("NFKD", value)
    ascii_value = normalized.encode("ascii", "ignore").decode("ascii")
    ascii_value = ascii_value.lower()
    ascii_value = re.sub(r"[^a-z0-9]+", "-", ascii_value)
    return ascii_value.strip("-")


def extract_json_ld_product(html_text: str) -> dict[str, Any]:
    matches = re.findall(r'<script type="application/ld\+json">(.*?)</script>', html_text, re.S | re.I)
    for raw_json in matches:
        try:
            data = json.loads(html.unescape(raw_json))
        except json.JSONDecodeError:
            continue
        candidates: list[Any] = data if isinstance(data, list) else [data]
        for candidate in candidates:
            if isinstance(candidate, dict) and candidate.get("@type") == "Product":
                return candidate
    return {}


def extract_section(html_text: str, section_id: str) -> str:
    pattern = rf'<section id="{re.escape(section_id)}"[^>]*>(.*?)</section>'
    return find_first(pattern, html_text)


def extract_div_by_class(html_text: str, class_name: str) -> str:
    pattern = rf'<div class="{re.escape(class_name)}"[^>]*>(.*?)</div>'
    return find_first(pattern, html_text)


def extract_list_items(fragment: str) -> list[str]:
    if not fragment:
        return []
    items = re.findall(r"<li[^>]*>(.*?)</li>", fragment, re.S | re.I)
    return [clean_html_fragment(item) for item in items if clean_html_fragment(item)]


def sanitize_price(raw_price: Any) -> str:
    if raw_price is None:
        return ""
    price = str(raw_price).strip()
    price = price.replace(" ", "").replace(",", ".")
    return price


def image_filename(image_url: str) -> str:
    if not image_url:
        return ""
    parsed = urlparse(image_url)
    return Path(parsed.path).name


def download_image(image_url: str, images_dir: Path) -> str:
    """Letölti a képet az images_dir mappába, visszaadja az abszolút elérési utat."""
    if not image_url:
        return ""
    filename = image_filename(image_url)
    if not filename:
        return ""
    dest = images_dir / filename
    if not dest.exists():
        try:
            request = Request(image_url, headers={"User-Agent": "Mozilla/5.0"})
            with urlopen(request, timeout=30) as response:
                dest.write_bytes(response.read())
        except Exception as exc:
            print(f"Kép letöltési hiba ({image_url}): {exc}", file=sys.stderr)
            return ""
    return str(dest.resolve())


# ---------------------------------------------------------------------------
# Product scraping
# ---------------------------------------------------------------------------

def parse_product(url: str, images_dir: Path) -> dict[str, Any]:
    html_text = fetch_html(url)
    product_ld = extract_json_ld_product(html_text)

    path_slug = url.rstrip("/").rsplit("/", 1)[-1]
    slug = slugify(path_slug)

    short_description_html = extract_div_by_class(html_text, "artdet__short-descripton-content")
    ingredients_section = extract_section(html_text, "artdet__osszetevok_napi_adag")
    properties_section = extract_section(html_text, "artdet__osszetevok")
    science_section = extract_section(html_text, "artdet__tudomanyos_hatter")

    ingredients_text = clean_html_fragment(ingredients_section)
    properties_items = extract_list_items(
        find_first(r'<div class="osszetevok-content[^\"]*"[^>]*>(.*?)</div>', properties_section)
    )
    contraindication_items = extract_list_items(
        find_first(r'<div class="ellenjavallatok-content"[^>]*>(.*?)</div>', properties_section)
    )

    additional_properties: list[dict[str, str]] = []
    for prop in product_ld.get("additionalProperty", []) if isinstance(product_ld, dict) else []:
        if isinstance(prop, dict):
            name = clean_html_fragment(str(prop.get("name", "")))
            value = clean_html_fragment(str(prop.get("value", "")))
            if name and value:
                additional_properties.append({"Property Name": name, "Value": value})

    meta_title = find_first(r'<title>(.*?)</title>', html_text)
    meta_description = find_first(r'<meta name="description" content="([^"]+)"', html_text)
    og_image = find_first(r'<meta property="og:image" content="([^"]+)"', html_text)
    image_path = download_image(og_image, images_dir)  # helyi biztonsági másolat
    # Az SQL-ben az eredeti HTTPS URL-t használjuk – nincs szükség szerverre feltöltésre
    image_url_for_sql = og_image

    description_parts = [
        clean_html_fragment(short_description_html),
        ingredients_text,
        clean_html_fragment(science_section),
    ]
    description = "\n\n".join(part for part in description_parts if part)

    info_tabs: list[dict[str, str]] = []
    if ingredients_text:
        info_tabs.append({"Tab Name": "Osszetevok", "Tab Description": ingredients_text})
    if properties_items:
        info_tabs.append({
            "Tab Name": "Termektulajdonsagok",
            "Tab Description": "\n".join(f"- {item}" for item in properties_items),
        })
    if contraindication_items:
        info_tabs.append({
            "Tab Name": "Ellenjavallatok",
            "Tab Description": "\n".join(f"- {item}" for item in contraindication_items),
        })
    science_text = clean_html_fragment(science_section)
    if science_text:
        info_tabs.append({"Tab Name": "Tudomanyos hatter", "Tab Description": science_text})

    sku = str(product_ld.get("sku") or find_first(r'UNAS\.shop\["sku"\]="([^"]+)"', html_text))

    manufacturer = ""
    if isinstance(product_ld.get("manufacturer"), dict):
        manufacturer = str(product_ld["manufacturer"].get("name", ""))
    elif product_ld.get("manufacturer"):
        manufacturer = str(product_ld.get("manufacturer", ""))

    return {
        "bvin": str(uuid.uuid4()),          # előre generált, fixált Bvin az SQL-hez
        "slug": slug,
        "sku": sku,
        "name": str(product_ld.get("name") or clean_html_fragment(find_first(r"<h1[^>]*>(.*?)</h1>", html_text))),
        "price": sanitize_price(
            product_ld.get("offers", {}).get("price") if isinstance(product_ld.get("offers"), dict) else ""
        ),
        "manufacturer": manufacturer,
        "image_local": image_path,          # helyi másolat elérési útja
        "image_url": image_url_for_sql,     # eredeti HTTPS URL → ez kerül az SQL-be
        "description": description,
        "search_keywords": ", ".join(p["Value"] for p in additional_properties),
        "meta_title": meta_title,
        "meta_description": meta_description,
        "info_tabs": info_tabs,
        "additional_properties": additional_properties,
    }


def load_urls(urls_file: Path | None, limit: int | None) -> list[str]:
    urls = DEFAULT_URLS
    if urls_file:
        urls = [line.strip() for line in urls_file.read_text(encoding="utf-8").splitlines() if line.strip()]
    if limit is not None:
        urls = urls[:limit]
    return urls


# ---------------------------------------------------------------------------
# SQL generation helpers
# ---------------------------------------------------------------------------

def _sql_escape(s: str) -> str:
    """U+2018/U+2019 kurva-aposztróf → ASCII aposztróf, majd SQL-escape ('' duplikálás)."""
    s = s.replace('\u2018', "'").replace('\u2019', "'")
    return s.replace("'", "''")


def sql_str(value: Any) -> str:
    """Értéket N'...' SQL literállá alakít; None/üres → N''."""
    if value is None:
        return "N''"
    return "N'" + _sql_escape(str(value)) + "'"


def sql_str_max(value: Any, chunk: int = 4000) -> str:
    """Hosszú szövegeket NVARCHAR(MAX) SQL literállá alakít (4000 kar. darabok + összefűzés).
    Az eredeti stringet daraboljuk ESCAPE ELŐTT, hogy a '' pár soha ne kerüljön két darab közé."""
    if value is None:
        return "N''"
    s = str(value)
    if len(s) <= chunk:
        return "N'" + _sql_escape(s) + "'"
    pieces = [_sql_escape(s[i:i + chunk]) for i in range(0, len(s), chunk)]
    sql_pieces = [f"CAST(N'{pieces[0]}' AS NVARCHAR(MAX))"] + [f"N'{p}'" for p in pieces[1:]]
    return "(\n        " + "\n        + ".join(sql_pieces) + "\n    )"


def sql_money(value: Any) -> str:
    """Árat decimális SQL literállá alakít; nem szám → 0."""
    if not value:
        return "0"
    try:
        return str(float(str(value).replace(",", ".")))
    except (ValueError, TypeError):
        return "0"


def build_description_tabs_xml(info_tabs: list[dict[str, str]]) -> str:
    """Info tabokat HotCakes DescriptionTabs XML formátumra alakítja (ntext mező)."""
    if not info_tabs:
        return ""
    parts = [
        '<ArrayOfDescriptionTab xmlns:xsd="http://www.w3.org/2001/XMLSchema"'
        ' xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">'
    ]
    for i, tab in enumerate(info_tabs):
        tab_id = str(uuid.uuid4())
        title = (tab["Tab Name"]
                 .replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;"))
        html_content = text_to_html(tab["Tab Description"])
        html_esc = (html_content
                    .replace("&", "&amp;").replace("<", "&lt;")
                    .replace(">", "&gt;").replace('"', "&quot;"))
        parts += [
            "  <DescriptionTab>",
            f"    <TabId>{tab_id}</TabId>",
            f"    <Title>{title}</Title>",
            f"    <HtmlData>{html_esc}</HtmlData>",
            f"    <SortOrder>{i}</SortOrder>",
            "  </DescriptionTab>",
        ]
    parts.append("</ArrayOfDescriptionTab>")
    xml = "".join(parts)  # egy sorban – a sortörők SQL string literált törnének el
    # Non-ASCII karaktereket XML numerikus entitásokra cseréljük (&#xXXXX;),
    # hogy az SQL string literál 100% ASCII legyen – elkerüli az SSMS/cp1250 kódolási gondot.
    return "".join(c if ord(c) < 128 else f"&#{ord(c)};" for c in xml)


# ---------------------------------------------------------------------------
# SQL script generation
# ---------------------------------------------------------------------------

def generate_sql(products: list[dict[str, Any]], store_id: int = 1, culture: str = "hu-HU") -> str:
    lines: list[str] = []
    EMPTY_GUID = "00000000-0000-0000-0000-000000000000"

    # ---- fejléc ----
    lines += [
        "-- ==============================================================",
        "-- HotCakes Commerce 3.9.1 – termék import",
        "-- DNN Platform v09.13.10  |  .NET Framework 4.8.1",
        f"-- StoreId = {store_id}  |  Culture = {culture}",
        f"-- Termékek száma: {len(products)}",
        "--",
        "-- A képek az eredeti gal.hu HTTPS URL-jükkel vannak hivatkozva.",
        "-- ==============================================================",
        "",
        "SET NOCOUNT ON;",
        "SET XACT_ABORT ON;",
        "",
        "BEGIN TRANSACTION;",
        "",
    ]

    # ---- 1. StoreId egyéni terméktulajdonság ----
    lines += [
        "-- --------------------------------------------------------------",
        "-- 1. 'StoreId' egyéni terméktulajdonság létrehozása",
        "-- --------------------------------------------------------------",
        "DECLARE @PropId BIGINT;",
        "",
        f"IF NOT EXISTS (SELECT 1 FROM hcc_ProductProperty WHERE PropertyName = N'StoreId' AND StoreId = {store_id})",
        "BEGIN",
        "    INSERT INTO hcc_ProductProperty",
        "        (StoreId, PropertyName, TypeCode, DefaultValue, IsLocalizable, CultureCode,",
        "         DisplayOnSite, DisplayToDropShipper, LastUpdated, DisplayOnSearch)",
        f"    VALUES ({store_id}, N'StoreId', 1, N'1', 0, N'{culture}',",
        "             1, 0, GETDATE(), 0);",
        "",
        "    SET @PropId = SCOPE_IDENTITY();",
        "",
        "    INSERT INTO hcc_ProductPropertyTranslations",
        "        (ProductPropertyId, Culture, DisplayName, DefaultLocalizableValue)",
        f"    VALUES (@PropId, N'{culture}', N'Store ID', N'');",
        "END",
        "ELSE",
        "BEGIN",
        f"    SELECT @PropId = Id FROM hcc_ProductProperty WHERE PropertyName = N'StoreId' AND StoreId = {store_id};",
        "END",
        "",
    ]

    # ---- 2. Kategória ----
    lines += [
        "-- --------------------------------------------------------------",
        "-- 2. Kategória létrehozása (ha még nem létezik)",
        "-- --------------------------------------------------------------",
        "DECLARE @CatBvin UNIQUEIDENTIFIER;",
        "",
        f"IF NOT EXISTS (SELECT 1 FROM hcc_Category WHERE RewriteUrl = N'etrendkiegeszito' AND StoreId = {store_id})",
        "BEGIN",
        "    DECLARE @NewCatBvin UNIQUEIDENTIFIER = NEWID();",
        "",
        "    INSERT INTO hcc_Category (",
        "        bvin, StoreId, ParentID, LastUpdated, CreationDate,",
        "        RewriteUrl, TemplateName, SortOrder, DisplaySortOrder, SourceType,",
        "        ImageURL, BannerImageURL, CustomPageURL, CustomPageNewWindow,",
        "        ShowInTopMenu, Hidden, ShowTitle,",
        "        PreContentColumnId, PostContentColumnId",
        "    ) VALUES (",
        f"        @NewCatBvin, {store_id}, NULL, GETDATE(), GETDATE(),",
        "        N'etrendkiegeszito', N'', 0, 0, 0,",
        "        N'', N'', N'', 0,",
        "        0, 0, 1,",
        "        N'', N''",
        "    );",
        "",
        "    INSERT INTO hcc_CategoryTranslations",
        "        (CategoryId, Culture, Name, Description, MetaTitle, MetaDescription, MetaKeywords, Keywords)",
        f"    VALUES (@NewCatBvin, N'{culture}', N'Étrendkiegészítők', N'', N'', N'', N'', N'');",
        "END",
        "",
        "SELECT @CatBvin = bvin FROM hcc_Category",
        f"WHERE RewriteUrl = N'etrendkiegeszito' AND StoreId = {store_id};",
        "",
    ]

    # ---- 3. Termékek ----
    lines += [
        "-- --------------------------------------------------------------",
        f"-- 3. Termékek ({len(products)} db)",
        "-- --------------------------------------------------------------",
        "DECLARE @Bvin UNIQUEIDENTIFIER;",
        "DECLARE @ExistingBvin UNIQUEIDENTIFIER;",
        "",
    ]

    for idx, product in enumerate(products, start=1):
        slug = product["slug"]
        name = product["name"]
        sku = product["sku"]
        price = sql_money(product["price"])
        bvin = product["bvin"]
        img_url = product["image_url"]
        description_html = text_to_html(product["description"])
        tabs_xml = build_description_tabs_xml(product["info_tabs"])

        lines += [
            f"-- [{idx}/{len(products)}] {name}",
            f"-- Bvin: {bvin}",
        ]

        lines += [
            f"SELECT @ExistingBvin = bvin FROM hcc_Product WHERE RewriteUrl = {sql_str(slug)} AND StoreId = {store_id};",
            "IF @ExistingBvin IS NOT NULL",
            "BEGIN",
            f"    PRINT 'Meglévő termék törlése: {slug}';",
            f"    DELETE FROM hcc_ProductPropertyValue WHERE ProductBvin = @ExistingBvin AND StoreId = {store_id};",
            f"    DELETE FROM hcc_ProductInventory WHERE ProductBvin = @ExistingBvin AND StoreId = {store_id};",
            f"    DELETE FROM hcc_ProductXCategory WHERE ProductId = @ExistingBvin AND StoreId = {store_id};",
            f"    DELETE FROM hcc_ProductXOption WHERE ProductBvin = @ExistingBvin AND StoreId = {store_id};",
            "    DELETE FROM hcc_ProductTranslations WHERE ProductId = @ExistingBvin;",
            f"    DELETE FROM hcc_Product WHERE bvin = @ExistingBvin AND StoreId = {store_id};",
            "END",
            "",
            "BEGIN",
            f"    SET @Bvin = CONVERT(UNIQUEIDENTIFIER, N'{bvin}');",
            "",
            "    INSERT INTO hcc_Product (",
            "        bvin, StoreId, LastUpdated, CreationDate, Status, ProductTypeId,",
            "        ListPrice, SitePrice, SiteCost,",
            "        SKU, IsSearchable, IsAvailableForSale, AllowReviews, MinimumQty,",
            "        ShippingWeight, ShippingLength, ShippingWidth, ShippingHeight, ExtraShipFee,",
            "        ShipSeparately, NonShipping, ShippingMode,",
            "        GiftWrapAllowed, GiftWrapPrice, TaxClass, TaxExempt,",
            "        ManufacturerID, VendorID,",
            "        ImageFileSmall, ImageFileMedium,",
            "        Featured, RewriteUrl,",
            "        OutOfStockMode, IsBundle, IsGiftCard,",
            "        IsUserPrice, HideQty, IsRecurring, ShippingCharge,",
            "        AllowUpcharge, UpchargeAmount,",
            "        TemplateName, PreContentColumnId, PostContentColumnId, CustomProperties",
            "    ) VALUES (",
            f"        @Bvin, {store_id}, GETDATE(), GETDATE(), 1, NULL,",
            f"        {price}, {price}, 0,",
            f"        {sql_str(sku)}, 1, 1, 1, 1,",
            "        0, 0, 0, 0, 0,",
            "        0, 0, 1,",
            "        0, 0, N'-1', 0,",
            "        NULL, NULL,",
            f"        {sql_str(img_url)}, {sql_str(img_url)},",
            f"        0, {sql_str(slug)},",
            "        0, 0, 0,",
            "        0, 0, 0, 0,",
            "        0, 0,",
            "        N'', N'', N'', N''",
            "    );",
            "",
            "    INSERT INTO hcc_ProductTranslations (",
            "        ProductId, Culture, ProductName, LongDescription,",
            "        MetaTitle, MetaDescription, MetaKeywords, Keywords, DescriptionTabs,",
            "        SitePriceOverrideText, ShortDescription, SmallImageAlternateText,",
            "        MediumImageAlternateText, HiddenSearchKeywords, UserPriceLabel",
            "    ) VALUES (",
            f"        @Bvin, N'{culture}',",
            f"        {sql_str(name)},",
            f"        {sql_str_max(description_html)},",
            f"        {sql_str(product['meta_title'])},",
            f"        {sql_str_max(product['meta_description'])},",
            f"        N'', {sql_str_max(product['search_keywords'])},",
            f"        {sql_str_max(tabs_xml)},",
            "        N'', N'', N'', N'', N'', N''",
            "    );",
            "",
            "    INSERT INTO hcc_ProductXCategory (ProductId, CategoryId, SortOrder, StoreId)",
            f"    VALUES (@Bvin, @CatBvin, {idx - 1}, {store_id});",
            "",
            "    INSERT INTO hcc_ProductInventory",
            "        (bvin, ProductBvin, VariantId, QuantityOnHand, QuantityReserved,",
            "         LowStockPoint, LastUpdated, StoreId, OutOfStockPoint)",
            f"    VALUES (CONVERT(VARCHAR(36), NEWID()), @Bvin, N'', 0, 0, 0, GETDATE(), {store_id}, 0);",
            "",
            "    INSERT INTO hcc_ProductPropertyValue (ProductBvin, PropertyId, PropertyValue, StoreId)",
            f"    VALUES (@Bvin, @PropId, N'1', {store_id});",
        ]

        if product["additional_properties"]:
            lines.append("")
            lines.append("    -- Egyéb terméktulajdonságok (additionalProperty a JSON-LD-ből)")
            for prop in product["additional_properties"]:
                prop_name = prop["Property Name"].replace("'", "''")
                prop_value = prop["Value"].replace("'", "''")
                lines.append(f"    -- '{prop_name}' = '{prop_value}'")

        lines += [
            "",
            "END  -- BEGIN",
            "",
        ]

    # ---- lezárás ----
    lines += [
        "COMMIT TRANSACTION;",
        f"PRINT 'Import kész: {len(products)} termék feldolgozva.';",
    ]

    return "\n".join(lines)


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main() -> int:
    parser = argparse.ArgumentParser(description="GAL termék scraper – SQL import generátor")
    parser.add_argument("--output", default="sql_import.sql", help="A generált SQL fájl neve")
    parser.add_argument("--urls-file", help="Külön URL lista fájl, soronként egy URL")
    parser.add_argument("--limit", type=int, help="Csak az első N terméket dolgozza fel")
    parser.add_argument("--images-dir", default="gal_images", help="A letöltött képek mappája")
    parser.add_argument("--store-id", type=int, default=1, help="HotCakes StoreId (alapértelmezett: 1)")
    parser.add_argument("--culture", default="hu-HU", help="DNN kultúrakód (alapértelmezett: hu-HU)")
    args = parser.parse_args()

    images_dir = Path(args.images_dir).resolve()
    images_dir.mkdir(parents=True, exist_ok=True)
    print(f"Képek mentési helye: {images_dir}")

    urls_file = Path(args.urls_file) if args.urls_file else None
    urls = load_urls(urls_file, args.limit)
    if not urls:
        print("Nincs feldolgozható URL.", file=sys.stderr)
        return 1

    products = []
    for index, url in enumerate(urls, start=1):
        print(f"[{index}/{len(urls)}] Feldolgozás: {url}")
        try:
            products.append(parse_product(url, images_dir))
        except Exception as exc:
            print(f"Hiba ennél az URL-nél: {url} -> {exc}", file=sys.stderr)

    if not products:
        print("Nem sikerült egyetlen terméket sem feldolgozni.", file=sys.stderr)
        return 1

    sql = generate_sql(products, store_id=args.store_id, culture=args.culture)
    output_path = Path(args.output)
    output_path.write_text(sql, encoding="utf-8-sig")  # BOM: SSMS felismeri UTF-8-ként
    print(f"Elkészült: {output_path.resolve()}")
    print(f"Sikeresen feldolgozott termékek: {len(products)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
