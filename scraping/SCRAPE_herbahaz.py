from __future__ import annotations

import html as html_module
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
# Kategória rewrite URL
# ---------------------------------------------------------------------------

CATEGORY_REWRITE = "egc3a9szsc3a9ges-zsc3adrok"
VAR_NAME = "zsirok_product_urls"
BASE_URL = "https://www.herbahaz.hu"

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
                "Accept-Language": "hu-HU,hu;q=0.9",
                "Accept-Encoding": "gzip, deflate",
            })
            with urlopen(req, timeout=30) as r:
                raw = r.read()
            try:
                return gzip.decompress(raw).decode("utf-8")
            except Exception:
                try:
                    return raw.decode("utf-8")
                except UnicodeDecodeError:
                    return raw.decode("iso-8859-2", errors="replace")
        except (HTTPError, URLError, TimeoutError) as exc:
            last_err = exc
            if attempt < retries:
                time.sleep(delay * attempt)
    raise RuntimeError(f"Letöltési hiba: {url} ({last_err})")

# ---------------------------------------------------------------------------
# Parser – herbahaz.hu egyedi CMS
# ---------------------------------------------------------------------------

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

def parse_price_hu(raw: str) -> str:
    """Magyar formátum: '4.619 Ft' → '4619.0'"""
    # &nbsp; és egyéb nem törhető szóköz normalizálása
    raw = raw.replace("&nbsp;", " ").replace("&#160;", " ").replace("&#xA0;", " ").replace("\xa0", " ")
    m = re.search(r"([\d.,\s]+)\s*Ft", raw)
    if not m:
        return "0"
    s = m.group(1).strip()
    # Magyar: pont = ezres elválasztó, vessző = tizedes
    s = s.replace(" ", "").replace(".", "").replace(",", ".")
    try:
        return str(float(s))
    except ValueError:
        return "0"

def parse_product(url: str) -> dict[str, Any] | None:
    try:
        html_str = fetch_html(url)
    except RuntimeError as e:
        print(f"  HIBA: {e}", file=sys.stderr)
        return None

    # Termék neve – elsőként H1, majd class-alapú fallback
    name = ""
    # H1 (legmegbízhatóbb herbahaz.hu-n)
    m = re.search(r'<h1[^>]*>(.*?)</h1>', html_str, re.I | re.DOTALL)
    if m:
        candidate = clean_html(m.group(1)).strip()
        if len(candidate) > 3:  # ne legyen üres vagy &nbsp;
            name = candidate
    if not name:
        # og:title fallback
        m2 = re.search(r'property="og:title"\s+content="([^"]+)"', html_str, re.I)
        if m2:
            name = m2.group(1).strip()

    # Ár – keressük a "bruttó" / "ár" / "price" elemeket
    price = "0"
    # Próba 1: class="price-value" (pl. <span class="price-value">4.619 <span class="currency">Ft</span>)
    mp = re.search(r'class="price-value"[^>]*>([\d\s.,]+)', html_str, re.I)
    if mp:
        price = parse_price_hu(mp.group(1) + " Ft")
    if price == "0":
        # Próba 2: class="bruttó" vagy class="brutto"
        mp = re.search(r'class="[^"]*brut[^"]*"[^>]*>(.*?)</', html_str, re.I | re.DOTALL)
        if mp:
            price = parse_price_hu(mp.group(0))
    if price == "0":
        # Próba 3: "Ár:" label melletti .value
        mp = re.search(
            r'(?:Ár|Ar).*?<span[^>]*class="[^"]*value[^"]*"[^>]*>(.*?)</span>',
            html_str, re.I | re.DOTALL
        )
        if mp:
            price = parse_price_hu(mp.group(0))
    if price == "0":
        # Próba 4: bármilyen "Ft" szám az oldalon (&nbsp; és beágyazott tag is)
        mp = re.search(r'([\d.,]+)\s*(?:<[^>]+>)?\s*Ft', html_str)
        if mp:
            price = parse_price_hu(mp.group(1) + " Ft")

    # SKU / Termékkód
    sku = ""
    ms = re.search(
        r'[Tt]erm[eé]kkód[^<]*(?:</[^>]+>)?\s*:?\s*(?:<[^>]+>)?\s*([\w\-]+)',
        html_str, re.DOTALL
    )
    if not ms:
        ms = re.search(r'[Ss][Kk][Uu][^<]*(?:</[^>]+>)?\s*:?\s*(?:<[^>]+>)?\s*([\w\-]+)', html_str, re.DOTALL)
    if ms:
        sku = ms.group(1).strip()

    # URL slug
    path = url.split("?")[0].rstrip("/")
    slug = path.split("/")[-1]
    if not sku:
        sku = slug

    # Kép – /images/big/[filename]
    img_url = ""
    mi = re.search(r'/images/big/([^"\'>\s]+)', html_str)
    if mi:
        img_url = BASE_URL + "/images/big/" + mi.group(1)
    else:
        # Fallback: og:image
        mi2 = re.search(r'property="og:image"\s+content="([^"]+)"', html_str, re.I)
        if mi2:
            img_url = mi2.group(1)

    # Leírás – "Részletes termékinformáció" szekció
    desc = ""
    md = re.search(
        r'[Rr][eé]szletes\s+term[eé]kinform[aá]ci[oó].*?</(?:h\d|div)[^>]*>(.*?)</(?:div|section)',
        html_str, re.DOTALL | re.I
    )
    if not md:
        # Fallback: product_description div
        md = re.search(r'id="[^"]*description[^"]*"[^>]*>(.*?)</div', html_str, re.DOTALL | re.I)
    if md:
        desc = text_to_html(clean_html(md.group(1)))

    if not name:
        print(f"  HIBA: Nem sikerült kinyerni a terméknevet: {url}", file=sys.stderr)
        return None

    return {
        "bvin":             str(uuid.uuid4()),
        "slug":             slug,
        "sku":              sku,
        "name":             name,
        "site_price":       price,
        "list_price":       price,
        "image_url":        img_url,
        "description_html": desc,
        "keywords":         "",
        "vendor":           "",
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
        "-- herbahaz.hu – egészséges zsírok – HotCakes Commerce 3.9.1",
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
        f"PRINT 'Import kész: {len(products)} termék (herbahaz.hu – egészséges zsírok).';",
    ]
    return "\n".join(lines)

# ---------------------------------------------------------------------------
# URL lista betöltése
# ---------------------------------------------------------------------------

def load_urls(py_file: Path) -> list[str]:
    content = py_file.read_text(encoding="utf-16")
    m = re.search(rf"{VAR_NAME}\s*=\s*\[(.*?)\]", content, re.DOTALL)
    if not m:
        return []
    return re.findall(r"'(https?://[^']+)'", m.group(1))

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main() -> int:
    parser = argparse.ArgumentParser(description="herbahaz.hu scraper – HotCakes SQL import")
    parser.add_argument("--lists",    default="product_lists.py")
    parser.add_argument("--output",   default="sql_categories/05_zsirok.sql")
    parser.add_argument("--limit",    type=int)
    parser.add_argument("--delay",    type=float, default=0.8)
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

    print(f"[zsirok] → {CATEGORY_REWRITE} | {len(urls)} termék")

    products: list[dict] = []
    for i, url in enumerate(urls, 1):
        slug = url.split("?")[0].rstrip("/").split("/")[-1]
        print(f"  [{i}/{len(urls)}] {slug[:60]}")
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
