# خارطة التنفيذ

## المرحلة 0 — حسم الأساس

اعتماد القرارات في [OPEN_DECISIONS](OPEN_DECISIONS.md)، إعداد Windows build، هيكل solution، coding standards، وبيانات اختبار صغيرة.

## المرحلة 1 — النواة والبيانات

Domain primitives، Product/Batch، ledger والحركات، EF Core SQLite، migrations، Audit، backup/restore. معيار الخروج: اختبارات معاملات وحسابات تمر.

## المرحلة 2 — الأطراف والمشتريات

Customers/Suppliers، opening balances، Purchases وpartial receipts، شاشة الأصناف والموردين. معيار الخروج: استلامان ينعكسان بدقة في المخزون والحساب.

## المرحلة 3 — الطلب والمبيعات

Sales orders/invoices، الأسعار والخصم وBonus، FEFO قابل للتعديل، الائتمان، الطباعة. معيار الخروج: دورة بيع كاملة مع snapshot وAudit.

## المرحلة 4 — التحصيل والمرتجعات والتسليم

Payments/allocation، delivery/representatives، linked/exceptional returns، كشوف الحساب.

## المرحلة 5 — الجرد والتقارير والاستيراد

Stocktake/adjustments، تنبيهات الصلاحية والنقص، تقارير A4، Excel import/export مع dry run.

## المرحلة 6 — الصقل والإطلاق

اختبارات Windows UI، أداء وحالات فشل، استعادة نسخة، RTL/accessibility، حزمة تثبيت وتعليمات تشغيل.

الاعتماديات تسير من 0 إلى 1 ثم 2؛ المبيعات تعتمد النواة والأطراف والمخزون، والتقارير/الاستيراد تعتمد نماذجها المرحّلة. لا تنفذ مزامنة متعددة الأجهزة في هذه الخارطة الأولى.
