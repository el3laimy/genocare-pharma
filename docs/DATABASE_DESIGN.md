# تصميم قاعدة البيانات (تصور أولي)

هذا تصور قبل التنفيذ؛ لا توجد قاعدة أو migrations في هذه المهمة. أسماء الجداول/الأعمدة قابلة للمراجعة.

## الجداول المقترحة

- `products(id, internal_code, name, unit, current_retail_price, current_purchase_price, is_active)`؛ السعر الحالي ليس مصدر تكلفة مبيعات تاريخية.
- `batches(id, product_id, batch_number, expiry_date, available_quantity, quarantine_quantity, non_sellable_quantity, actual_unit_cost)`
- `stock_movements(id, product_id, batch_id, quantity, movement_type, document_id, occurred_at)`
- `customers(id, name, phone, opening_balance, credit_limit, payment_term_days)` و`suppliers(...)`
- `sales_orders` و`sales_order_lines`
- `sales_invoices(id, customer_id, order_id, status, invoice_date, total, discount_total)` و`sales_invoice_lines(invoice_id, product_id, quantity, bonus_quantity, retail_price_snapshot, discount_percent, net_unit_price, line_total)`؛ بلا `batch_id` مباشر.
- `batch_allocations(invoice_line_id, batch_id, quantity, unit_cost)` لتوزيع السطر على عدة تشغيلات وتثبيت تكلفة المبيعات الفعلية.
- `purchases(id, supplier_id, supplier_invoice_id, total, payable_status)` و`purchase_receipts` و`purchase_receipt_lines`; الالتزام المالي على Purchase لا على كل Receipt.
- `payments(id, party_type, party_id, amount, payment_date, method, reference)` و`payment_allocations`
- `returns` و`return_lines`
- `stocktakes` و`stocktake_lines` و`adjustments` و`stock_overrides(id, reason, authorized_by, approved_at)`
- `representatives` و`deliveries`
- `audit_entries(id, actor, action, entity_type, entity_id, before_json, after_json, occurred_at)`

## قواعد تصميم

المبالغ `decimal` في المجال (تمثيل مناسب في SQLite)، والكميات decimal/whole حسب القرار. كل FK وحالة وتاريخ لها فهارس مناسبة؛ `internal_code` فريد؛ لا حذف متسلسل للمستندات المرحّلة. حفظ snapshot السعر والخصم في سطر الفاتورة إلزامي. يضاف `created_at/updated_at` حيث يلزم، و`row_version` عند دعم تعدد الأجهزة.

## ERD مبدئي

```mermaid
erDiagram
  PRODUCTS ||--o{ BATCHES : has
  PRODUCTS ||--o{ STOCK_MOVEMENTS : moves
  BATCHES ||--o{ STOCK_MOVEMENTS : tracks
  CUSTOMERS ||--o{ SALES_ORDERS : places
  SALES_ORDERS ||--o{ SALES_ORDER_LINES : contains
  SALES_ORDERS o|--o| SALES_INVOICES : converts_to
  CUSTOMERS ||--o{ SALES_INVOICES : billed
  SALES_INVOICES ||--|{ SALES_INVOICE_LINES : contains
  SALES_INVOICE_LINES ||--|{ BATCH_ALLOCATIONS : split_into
  BATCHES ||--o{ BATCH_ALLOCATIONS : allocated
  SUPPLIERS ||--o{ PURCHASES : receives_from
  PURCHASES ||--o{ PURCHASE_RECEIPTS : received
  PURCHASE_RECEIPTS ||--|{ PURCHASE_RECEIPT_LINES : contains
  BATCHES ||--o{ PURCHASE_RECEIPT_LINES : creates
  CUSTOMERS ||--o{ PAYMENTS : pays
  PAYMENTS ||--o{ PAYMENT_ALLOCATIONS : applies
  SALES_INVOICES ||--o{ PAYMENT_ALLOCATIONS : settled
  SALES_INVOICES ||--o{ RETURNS : source
  RETURNS ||--|{ RETURN_LINES : contains
  PRODUCTS ||--o{ RETURN_LINES : returned
```

## أسئلة تقنية لاحقة

اختيار GUID/INTEGER، ترقيم المستندات، تمثيل JSON، التشفير، WAL، وفهرسة تقارير الرصيد؛ تحسم قبل أول migration.
