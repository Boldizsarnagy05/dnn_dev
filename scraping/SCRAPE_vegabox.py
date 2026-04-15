from __future__ import annotations

import html as html_module
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
# Kategória rewrite URL (gyümölcs és zöldség ugyanaz a kategória)
# ---------------------------------------------------------------------------

CATEGORY_REWRITE = "rost-c3a9s-vitamin"
VAR_NAMES = [
    "rost_es_vitamin_gyumolcs_product_urls",
    "rost_es_vitamin_zoldseg_product_urls",
]

# ---------------------------------------------------------------------------
# HTTP helpers
# ---------------------------------------------------------------------------

def fetch_html(url: str, retries: int = 3, delay: float = 1.5) -> str:
    import gzip
    last_err: Exception | None = None
    for attempt in range(1, retries + 1):
        try:
            req = Request(url, headers={
                "User-Agent": "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                "Accept-Language": "hu-HU,hu;q=0.9,en-US;q=0.8",
                "Accept-Encoding": "gzip, deflate",
            })
            with urlopen(req, timeout=30) as r:
                raw = r.read()
            try:
                return gzip.decompress(raw).decode("utf-8")
            except Exception:
                return raw.decode("utf-8", errors="replace")
        except (HTTPError, URLError, TimeoutError) as exc:
            last_err = exc
            if attempt < retries:
                time.sleep(delay * attempt)
    raise RuntimeError(f"Letöltési hiba: {url} ({last_err})")

# ---------------------------------------------------------------------------
# Parsers
# ---------------------------------------------------------------------------

def extract_json_ld_product(html_str: str) -> dict | None:
    scripts = re.findall(
        r'<script[^>]+type=["\']application/ld\+json["\'][^>]*>(.*?)</script>',
        html_str, re.DOTALL | re.I
    )
    for script in scripts:
        try:
            data = json.loads(script.strip())
            if isinstance(data, list):
                for item in data:
                    if isinstance(item, dict) and item.get("@type") == "Product":
                        return item
            elif isinstance(data, dict):
                if data.get("@type") == "Product":
                    return data
                for item in data.get("@graph", []):
                    if isinstance(item, dict) and item.get("@type") == "Product":
                        return item
        except (json.JSONDecodeError, ValueError):
            continue
    return None

def clean_html(raw: str) -> str:
    if not raw:
        return ""
    text = re.sub(r"<br\s*/?>", "\n", raw, flags=re.I)
    text = re.sub(r"</p\s*>", "\n\n", text, flags=re.I)
    text = re.sub(r"<li[^>]*>", "- ", text, flags=re.I)
    text = re.sub(r"</li\s*>", "\n", text, flags=re.I)
    text = re.sub(r"<[^>]+>", "", text)
    text = html_module.unescape(text)
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

def parse_product(url: str) -> dict[str, Any] | None:
    try:
        html_str = fetch_html(url)
    except RuntimeError as e:
        print(f"  HIBA: {e}", file=sys.stderr)
        return None

    product_data = extract_json_ld_product(html_str)
    if not product_data:
        print(f"  HIBA: Nincs JSON-LD termék adat: {url}", file=sys.stderr)
        return None

    name = product_data.get("name", "")

    slug = url.rstrip("/").split("/")[-1]
    if not slug:
        slug = url.rstrip("/").split("/")[-2]

    sku_raw = product_data.get("sku", "")
    sku = str(sku_raw) if sku_raw else slug

    offers = product_data.get("offers", {})
    if isinstance(offers, list):
        offers = offers[0] if offers else {}
    price = str(offers.get("price", "0") or "0")

    image = product_data.get("image", "")
    if isinstance(image, list):
        image = image[0] if image else ""
    if isinstance(image, dict):
        image = image.get("url", "")

    desc_raw = product_data.get("description", "")
    if desc_raw:
        description_html = text_to_html(clean_html(desc_raw))
    else:
        m = re.search(
            r'class="[^"]*woocommerce-product-details__short-description[^"]*">(.*?)</div',
            html_str, re.DOTALL | re.I
        )
        if not m:
            m = re.search(r'id="tab-description"[^>]*>(.*?)</div', html_str, re.DOTALL | re.I)
        description_html = text_to_html(clean_html(m.group(1))) if m else ""

    brand = product_data.get("brand", "")
    vendor = brand.get("name", "") if isinstance(brand, dict) else str(brand)

    return {
        "bvin":             str(uuid.uuid4()),
        "slug":             slug,
        "sku":              sku,
        "name":             name,
        "site_price":       price,
        "list_price":       price,
        "image_url":        image,
        "description_html": description_html,
        "keywords":         "",
        "vendor":           vendor,
        "category_rewrite": CATEGORY_REWRITE,
    }

# ---------------------------------------------------------------------------
# SQL helpers
# ---------------------------------------------------------------------------

CUSTOM_PROPS_XML = (
    "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
    "<ArrayOfCustomProperty xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" "
    "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
    "<CustomProperty><DeveloperId>hcc</DeveloperId>"
    "<Key>swatchpath</Key><Value /></CustomProperty>"
    "</ArrayOfCustomProperty>"
)

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
# SQL generálás
# ---------------------------------------------------------------------------

def generate_sql(products: list[dict], store_id: int, culture: str) -> str:
    lines: list[str] = [
        "-- ==============================================================",
        "-- vegabox.hu – rost és vitamin – HotCakes Commerce 3.9.1",
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
        f"SELECT @CatBvin = bvin FROM hcc_Category WHERE RewriteUrl = N'{CATEGORY_REWRITE}' AND StoreId = {store_id};",
        "",
    ]

    for idx, prod in enumerate(products, 1):
        slug  = prod["slug"]
        name  = prod["name"]
        sku   = prod["sku"]
        sp    = sql_money(prod["site_price"])
        lp    = sql_money(prod["list_price"])
        bvin  = prod["bvin"]
        img   = prod["image_url"]
        desc  = prod["description_html"]
        kw    = prod["keywords"]

        lines += [
            f"-- [{idx}/{len(products)}] {name[:80]}",
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
            "    N'',",
            f"    N'', {sql_str_max(kw)},",
            "    N'',",
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
        f"PRINT 'Import kész: {len(products)} termék (vegabox.hu – rost és vitamin).';",
    ]
    return "\n".join(lines)

# ---------------------------------------------------------------------------
# URL lista betöltése (mindkét változó)
# ---------------------------------------------------------------------------

def load_urls(py_file: Path) -> list[str]:
    content = py_file.read_text(encoding="utf-16")
    all_urls: list[str] = []
    for var in VAR_NAMES:
        m = re.search(rf"{var}\s*=\s*\[(.*?)\]", content, re.DOTALL)
        if m:
            all_urls.extend(re.findall(r"'(https?://[^']+)'", m.group(1)))
    return all_urls

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main() -> int:
    parser = argparse.ArgumentParser(description="vegabox.hu scraper – HotCakes SQL import")
    parser.add_argument("--lists",    default="product_lists.py")
    parser.add_argument("--output",   default="sql_categories/06_rost_vitamin.sql")
    parser.add_argument("--limit",    type=int)
    parser.add_argument("--delay",    type=float, default=0.5)
    parser.add_argument("--store-id", type=int, default=1)
    parser.add_argument("--culture",  default="en-US")
    args = parser.parse_args()

    lists_file = Path(args.lists)
    if not lists_file.exists():
        print(f"Nem található: {lists_file}", file=sys.stderr)
        return 1

    urls = load_urls(lists_file)
    if args.limit:
        urls = urls[:args.limit]

    print(f"[rost_es_vitamin] → {CATEGORY_REWRITE} | {len(urls)} termék (gyümölcs + zöldség)")

    products: list[dict] = []
    for i, url in enumerate(urls, 1):
        print(f"  [{i}/{len(urls)}] {url.rstrip('/').split('/')[-1][:60]}")
        prod = parse_product(url)
        if prod:
            products.append(prod)
        if args.delay > 0:
            time.sleep(args.delay)

    if not products:
        print("Nem sikerült egyetlen terméket sem letölteni.", file=sys.stderr)
        return 1

    Path(args.output).parent.mkdir(parents=True, exist_ok=True)
    sql = generate_sql(products, args.store_id, args.culture)
    out = Path(args.output)
    out.write_text(sql, encoding="utf-8-sig")
    print(f"\nKész: {out} ({out.stat().st_size // 1024} KB)")
    print(f"Feldolgozott termékek: {len(products)}")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
