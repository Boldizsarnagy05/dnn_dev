from __future__ import annotations

import html
import json
import re
import sys
import time
import uuid
import argparse
from pathlib import Path
from typing import Any
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen

# ---------------------------------------------------------------------------
# Kategória → DB RewriteUrl mapping
# ---------------------------------------------------------------------------

CATEGORY_MAP: dict[str, str] = {
    "italok_product_urls":                  "italok",
    "izesitok_product_urls":                "c3adzesc3adtc591k",
    "feherjeforras_product_urls":           "fehc3a9rjeforrc3a1s",
    "szenhidratforras_product_urls":        "szc3a9nhidrc3a1tforrc3a1s",
    "rost_es_vitamin_gyumolcs_product_urls":"rost-c3a9s-vitamin",
    "rost_es_vitamin_zoldseg_product_urls": "rost-c3a9s-vitamin",
    "zsirok_product_urls":                  "egc3a9szsc3a9ges-zsc3adrok",
    "keszetelek_product_urls":              "kc3a9sz-c3a9telek",
}

# ---------------------------------------------------------------------------
# HTTP helpers
# ---------------------------------------------------------------------------

def fetch_json(url: str, retries: int = 3, delay: float = 1.5) -> dict:
    last_err: Exception | None = None
    for attempt in range(1, retries + 1):
        try:
            req = Request(url, headers={"User-Agent": "Mozilla/5.0"})
            with urlopen(req, timeout=30) as r:
                return json.loads(r.read().decode("utf-8"))
        except (HTTPError, URLError, TimeoutError) as exc:
            last_err = exc
            if attempt < retries:
                time.sleep(delay * attempt)
    raise RuntimeError(f"Letöltési hiba: {url} ({last_err})")

# ---------------------------------------------------------------------------
# Shopify product parser
# ---------------------------------------------------------------------------

def shopify_url_to_api(url: str) -> str:
    """https://bio-barat.hu/products/[handle]  →  .../products/[handle].json"""
    url = url.rstrip("/")
    if url.endswith(".json"):
        return url
    return url + ".json"

def clean_html(raw: str) -> str:
    if not raw:
        return ""
    text = re.sub(r"<br\s*/?>", "\n", raw, flags=re.I)
    text = re.sub(r"</p\s*>", "\n\n", text, flags=re.I)
    text = re.sub(r"<li[^>]*>", "- ", text, flags=re.I)
    text = re.sub(r"</li\s*>", "\n", text, flags=re.I)
    text = re.sub(r"<[^>]+>", "", text)
    text = html.unescape(text)
    text = text.replace("\xa0", " ")
    text = re.sub(r"\r", "", text)
    text = re.sub(r"\n{3,}", "\n\n", text)
    text = re.sub(r"[ \t]+", " ", text)
    return text.strip()

def text_to_html(text: str) -> str:
    if not text:
        return ""
    escaped = text.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")
    parts = []
    for para in re.split(r"\n\n+", escaped):
        para = para.strip()
        if para:
            parts.append(f"<p>{para.replace(chr(10), '<br />')}</p>")
    return "".join(parts)

def parse_product(url: str, category_rewrite: str) -> dict[str, Any] | None:
    api_url = shopify_url_to_api(url)
    try:
        data = fetch_json(api_url)
    except RuntimeError as e:
        print(f"  HIBA: {e}", file=sys.stderr)
        return None

    p = data.get("product", {})
    if not p:
        return None

    variants = p.get("variants", [{}])
    v = variants[0] if variants else {}
    images = p.get("images", [])
    img_src = images[0]["src"] if images else ""

    # Ár: SitePrice és ListPrice
    site_price = v.get("price", "0") or "0"
    list_price = v.get("compare_at_price") or site_price

    tags = p.get("tags", [])
    keywords = ", ".join(tags) if isinstance(tags, list) else str(tags)

    description_html = text_to_html(clean_html(p.get("body_html", "")))

    return {
        "bvin":             str(uuid.uuid4()),
        "slug":             p.get("handle", ""),
        "sku":              str(v.get("sku", "") or p.get("handle", "")),
        "name":             p.get("title", ""),
        "site_price":       site_price,
        "list_price":       list_price,
        "image_url":        img_src,
        "description_html": description_html,
        "keywords":         keywords,
        "vendor":           p.get("vendor", ""),
        "category_rewrite": category_rewrite,
    }

# ---------------------------------------------------------------------------
# SQL helpers
# ---------------------------------------------------------------------------

def _esc(s: str) -> str:
    return s.replace("\u2018", "'").replace("\u2019", "'").replace("'", "''")

def sql_str(v: Any) -> str:
    return "N''" if not v else f"N'{_esc(str(v))}'"

def sql_str_max(v: Any, chunk: int = 4000) -> str:
    if not v:
        return "N''"
    s = str(v)
    if len(s) <= chunk:
        return f"N'{_esc(s)}'"
    pieces = [_esc(s[i:i+chunk]) for i in range(0, len(s), chunk)]
    parts = [f"CAST(N'{pieces[0]}' AS NVARCHAR(MAX))"] + [f"N'{p}'" for p in pieces[1:]]
    return "(\n        " + "\n        + ".join(parts) + "\n    )"

def sql_money(v: Any) -> str:
    try:
        return str(float(str(v).replace(",", ".")))
    except (ValueError, TypeError):
        return "0"

# ---------------------------------------------------------------------------
# SQL generation
# ---------------------------------------------------------------------------

CUSTOM_PROPS_XML = (
    "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
    "<ArrayOfCustomProperty xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" "
    "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
    "<CustomProperty><DeveloperId>hcc</DeveloperId>"
    "<Key>swatchpath</Key><Value /></CustomProperty>"
    "</ArrayOfCustomProperty>"
)

def generate_sql(products: list[dict], store_id: int, culture: str) -> str:
    lines: list[str] = [
        "-- ==============================================================",
        "-- bio-barat.hu termék import – HotCakes Commerce 3.9.1",
        f"-- StoreId = {store_id}  |  Culture = {culture}",
        f"-- Termékek száma: {len(products)}",
        "-- ==============================================================",
        "",
        "SET NOCOUNT ON;",
        "SET XACT_ABORT ON;",
        "BEGIN TRANSACTION;",
        "",
        "DECLARE @Bvin UNIQUEIDENTIFIER;",
        "DECLARE @ExistingBvin UNIQUEIDENTIFIER;",
        "DECLARE @CatBvin UNIQUEIDENTIFIER;",
        "",
    ]

    # Kategória bvin-ek előre betöltve
    cat_rewrites = list({p["category_rewrite"] for p in products})
    for rw in cat_rewrites:
        lines += [
            f"-- Kategória: {rw}",
            f"DECLARE @Cat_{rw.replace('-','_').replace('c3','').replace('a9','').replace('ad','').replace('91','')[:20]} UNIQUEIDENTIFIER;",
        ]
        # Use a simpler approach: set at query time
    lines.append("")

    for idx, prod in enumerate(products, 1):
        slug      = prod["slug"]
        name      = prod["name"]
        sku       = prod["sku"]
        sp        = sql_money(prod["site_price"])
        lp        = sql_money(prod["list_price"])
        bvin      = prod["bvin"]
        img       = prod["image_url"]
        desc      = prod["description_html"]
        kw        = prod["keywords"]
        cat_rw    = prod["category_rewrite"]

        lines += [
            f"-- [{idx}/{len(products)}] {name[:80]}",
            f"SELECT @CatBvin = bvin FROM hcc_Category WHERE RewriteUrl = N'{cat_rw}' AND StoreId = {store_id};",
            f"SELECT @ExistingBvin = bvin FROM hcc_Product WHERE RewriteUrl = {sql_str(slug)} AND StoreId = {store_id};",
            "IF @ExistingBvin IS NOT NULL",
            "BEGIN",
            f"    DELETE FROM hcc_ProductPropertyValue WHERE ProductBvin = @ExistingBvin AND StoreId = {store_id};",
            f"    DELETE FROM hcc_ProductInventory WHERE ProductBvin = @ExistingBvin AND StoreId = {store_id};",
            f"    DELETE FROM hcc_ProductXCategory WHERE ProductId = @ExistingBvin AND StoreId = {store_id};",
            f"    DELETE FROM hcc_ProductXOption WHERE ProductBvin = @ExistingBvin AND StoreId = {store_id};",
            "    DELETE FROM hcc_ProductTranslations WHERE ProductId = @ExistingBvin;",
            f"    DELETE FROM hcc_Product WHERE bvin = @ExistingBvin AND StoreId = {store_id};",
            "END",
            "",
            f"SET @Bvin = CONVERT(UNIQUEIDENTIFIER, N'{bvin}');",
            "",
            "INSERT INTO hcc_Product (",
            "    bvin, StoreId, LastUpdated, CreationDate, Status, ProductTypeId,",
            "    ListPrice, SitePrice, SiteCost,",
            "    SKU, IsSearchable, IsAvailableForSale, AllowReviews, MinimumQty,",
            "    ShippingWeight, ShippingLength, ShippingWidth, ShippingHeight, ExtraShipFee,",
            "    ShipSeparately, NonShipping, ShippingMode,",
            "    GiftWrapAllowed, GiftWrapPrice, TaxClass, TaxExempt,",
            "    ManufacturerID, VendorID,",
            "    ImageFileSmall, ImageFileMedium,",
            "    Featured, RewriteUrl,",
            "    OutOfStockMode, IsBundle, IsGiftCard,",
            "    IsUserPrice, HideQty, IsRecurring, ShippingCharge,",
            "    AllowUpcharge, UpchargeAmount,",
            "    TemplateName, PreContentColumnId, PostContentColumnId, CustomProperties",
            ") VALUES (",
            f"    @Bvin, {store_id}, GETDATE(), GETDATE(), 1, NULL,",
            f"    {lp}, {sp}, 0,",
            f"    {sql_str(sku)}, 1, 1, 1, 1,",
            "    0, 0, 0, 0, 0,",
            "    0, 0, 1,",
            "    0, 0, N'-1', 0,",
            "    NULL, NULL,",
            f"    {sql_str(img)}, {sql_str(img)},",
            f"    0, {sql_str(slug)},",
            "    100, 0, 0,",
            "    0, 0, 0, 1,",
            "    0, 0,",
            f"    N'', N'', N'', {sql_str(CUSTOM_PROPS_XML)}",
            ");",
            "",
            "INSERT INTO hcc_ProductTranslations (",
            "    ProductId, Culture, ProductName, LongDescription,",
            "    MetaTitle, MetaDescription, MetaKeywords, Keywords, DescriptionTabs,",
            "    SitePriceOverrideText, ShortDescription, SmallImageAlternateText,",
            "    MediumImageAlternateText, HiddenSearchKeywords, UserPriceLabel",
            ") VALUES (",
            f"    @Bvin, N'{culture}',",
            f"    {sql_str(name)},",
            f"    {sql_str_max(desc)},",
            f"    {sql_str(name)},",
            f"    N'',",
            f"    N'', {sql_str_max(kw)},",
            f"    N'',",
            "    N'', N'', N'', N'', N'', N''",
            ");",
            "",
            "INSERT INTO hcc_ProductXCategory (ProductId, CategoryId, SortOrder, StoreId)",
            f"VALUES (@Bvin, @CatBvin, {idx}, {store_id});",
            "",
            "INSERT INTO hcc_ProductInventory",
            "    (bvin, ProductBvin, VariantId, QuantityOnHand, QuantityReserved,",
            "     LowStockPoint, LastUpdated, StoreId, OutOfStockPoint)",
            f"VALUES (CONVERT(VARCHAR(36), NEWID()), @Bvin, N'', 0, 0, 0, GETDATE(), {store_id}, 0);",
            "",
        ]

    lines += [
        "COMMIT TRANSACTION;",
        f"PRINT 'Import kész: {len(products)} termék.';",
    ]
    return "\n".join(lines)

# ---------------------------------------------------------------------------
# URL lista betöltése product_lists.py-ból
# ---------------------------------------------------------------------------

def load_product_lists(py_file: Path) -> dict[str, list[str]]:
    content = py_file.read_text(encoding="utf-16")
    result: dict[str, list[str]] = {}
    for var_name in CATEGORY_MAP:
        pattern = rf"{var_name}\s*=\s*\[(.*?)\]"
        m = re.search(pattern, content, re.DOTALL)
        if m:
            urls = re.findall(r"'(https?://[^']+)'", m.group(1))
            result[var_name] = urls
        else:
            result[var_name] = []
    return result

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main() -> int:
    parser = argparse.ArgumentParser(description="bio-barat.hu Shopify scraper – HotCakes SQL import")
    parser.add_argument("--lists",    default="product_lists.py", help="product_lists.py elérési útja")
    parser.add_argument("--output",   default="sql_import_biobarat.sql", help="Kimeneti SQL fájl")
    parser.add_argument("--category", help="Csak ezt a kategóriát dolgozza fel (pl. italok_product_urls)")
    parser.add_argument("--limit",    type=int, help="Csak az első N terméket")
    parser.add_argument("--delay",    type=float, default=0.5, help="Késleltetés kérések között (mp)")
    parser.add_argument("--store-id", type=int, default=1)
    parser.add_argument("--culture",  default="en-US")
    args = parser.parse_args()

    lists_file = Path(args.lists)
    if not lists_file.exists():
        print(f"Nem található: {lists_file}", file=sys.stderr)
        return 1

    all_lists = load_product_lists(lists_file)

    # Melyik kategóriák kellenek?
    categories = [args.category] if args.category else list(CATEGORY_MAP.keys())

    all_products: list[dict] = []

    for cat_var in categories:
        if cat_var not in CATEGORY_MAP:
            print(f"Ismeretlen kategória: {cat_var}", file=sys.stderr)
            continue

        urls = all_lists.get(cat_var, [])
        if args.limit:
            urls = urls[:args.limit]

        cat_rw = CATEGORY_MAP[cat_var]
        print(f"\n[{cat_var}] → {cat_rw} | {len(urls)} termék")

        for i, url in enumerate(urls, 1):
            print(f"  [{i}/{len(urls)}] {url.split('/')[-1][:60]}")
            prod = parse_product(url, cat_rw)
            if prod:
                all_products.append(prod)
            if args.delay > 0:
                time.sleep(args.delay)

    if not all_products:
        print("Nem sikerült egyetlen terméket sem letölteni.", file=sys.stderr)
        return 1

    sql = generate_sql(all_products, args.store_id, args.culture)
    out = Path(args.output)
    out.write_text(sql, encoding="utf-8-sig")
    print(f"\nKész: {out} ({out.stat().st_size // 1024} KB)")
    print(f"Feldolgozott termékek: {len(all_products)}")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
