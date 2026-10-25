# GenoCare

GenoCare نظام مكتبي محلي لإدارة مخزن وتوزيع الأدوية على الصيدليات والمخازن والجهات الطبية. يبدأ المشروع بحجم أقل من 500 صنف ومتوسط 31–100 فاتورة بيع يوميًا، مع تصميم يسمح بالتوسع لاحقًا.

## الهدف والنطاق

يوفر النظام دورة متصلة للأصناف والتشغيلات والمشتريات والاستلام والمبيعات والطلبات والتحصيل والمرتجعات والجرد، مع دقة مالية وسجل تدقيق ونسخ احتياطي واستيراد من Excel. التطبيق **offline-first** ويعمل حاليًا لمستخدم واحد على Windows.

## التقنية المعتمدة

C#/.NET 10، WinUI 3 وXAML وWindows App SDK، Fluent Design، MVVM عبر CommunityToolkit.Mvvm، SQLite مع Entity Framework Core، وClean Architecture داخل Modular Monolith.

## مبادئ المشروع

- لا تُفقد الفواتير القديمة عند تغيير الأسعار؛ تحفظ كل حركة قيمها وقت الترحيل.
- التشغيلات وتواريخ الصلاحية جزء من المخزون، مع اقتراح FEFO قابل للتعديل.
- كل تعديل مؤثر قابل للتتبع في Audit Log.
- المبالغ تستخدم دقة عشرية وسياسة تقريب معلنة قبل التنفيذ.
- المتطلبات المؤكدة منفصلة عن التوصيات والقرارات المعلقة؛ راجع [OPEN_DECISIONS](docs/OPEN_DECISIONS.md).

## الوثائق

- [المواصفات SRS](docs/SRS.md) · [المعمارية](docs/ARCHITECTURE.md) · [نموذج المجال](docs/DOMAIN_MODEL.md)
- [دورات العمل](docs/WORKFLOWS.md) · [تصميم قاعدة البيانات](docs/DATABASE_DESIGN.md) · [خريطة الشاشات](docs/SCREEN_MAP.md)
- [خارطة الطريق](docs/ROADMAP.md) · [القرارات المفتوحة](docs/OPEN_DECISIONS.md)

يحتوي Task 004 على طبقة SQLite وmigration أولية واختبارات تكامل حقيقية؛ لا يحتوي المشروع بعد على شاشات أعمال أو مبيعات أو مشتريات.

## بنية الحل الحالية

يحتوي الفرع `feature/002-solution-bootstrap` على `GenoCare.slnx` ومشاريع `Domain` و`Application` و`Infrastructure` و`Desktop`، واختبارات xUnit مستقلة عن Windows. مشروع Desktop هو WinUI 3 حقيقي بواجهة أولية RTL فقط؛ لا يحتوي وظائف أعمال.

### المتطلبات

يتطلب البناء .NET SDK 10.0.100 أو أحدث ضمن نفس feature band. بناء واختبار Domain وApplication وInfrastructure واختبارات البداية تم التحقق منها على Linux باستخدام SDK 10.0.100. بناء WinUI 3 يتطلب Windows وWindows App SDK، ويستخدم CI منصة x64؛ لا يُدّعى نجاحه على Linux.

### أوامر التطوير

```bash
dotnet restore src/GenoCare.Domain/GenoCare.Domain.csproj
dotnet build src/GenoCare.Domain/GenoCare.Domain.csproj
dotnet test tests/GenoCare.Domain.Tests/GenoCare.Domain.Tests.csproj
dotnet test tests/GenoCare.Application.Tests/GenoCare.Application.Tests.csproj
dotnet test tests/GenoCare.Infrastructure.Tests/GenoCare.Infrastructure.Tests.csproj
```

بناء `src/GenoCare.Desktop/GenoCare.Desktop.csproj` واختبار الحل الكامل موكلان إلى Windows CI في `.github/workflows/ci.yml`.

يجب أن يمرر المضيف مسار قاعدة البيانات إلى `AddGenoCareInfrastructure` داخل Windows Application Data، ثم يستدعي `DatabaseInitializer` لتطبيق migrations. لا تُخزن قاعدة SQLite بجوار binaries. SQLite يعمل offline، وتحتاج الكتابات المتزامنة إلى معاملات قصيرة؛ لا توجد مزامنة أجهزة في هذه المرحلة.

المتطلبات التفصيلية ومصدر إجابات العميل في [CLIENT_REQUIREMENTS_SOURCE](docs/CLIENT_REQUIREMENTS_SOURCE.md)، والقرارات التي لم تُحسم في [OPEN_DECISIONS](docs/OPEN_DECISIONS.md).

## واجهة Task 005

تحتوي `GenoCare.Desktop` الآن على Shell عربية RTL عبر `NavigationView` وصفحتي الأصناف والتشغيلات. الصفحات تستخدم Application use cases وDI ولا تتعامل مع EF مباشرة. تهيئة SQLite تتم عند بدء التطبيق في Local Application Data؛ عند فشلها تظهر رسالة عربية بدل فتح شاشة غير صالحة. اختبار GUI الفعلي يحتاج Windows ولا يثبت بمجرد compilation.
