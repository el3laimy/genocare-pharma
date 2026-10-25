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

هذه المهمة توثق الأساس فقط؛ لا تحتوي على كود تطبيق أو قاعدة بيانات فعلية أو migrations.

## بنية الحل الحالية

يحتوي الفرع `feature/002-solution-bootstrap` على `GenoCare.slnx` ومشاريع `Domain` و`Application` و`Infrastructure` و`Desktop`، واختبارات xUnit مستقلة عن Windows. مشروع Desktop هو WinUI 3 حقيقي بواجهة أولية RTL فقط؛ لا يحتوي وظائف أعمال.

### المتطلبات

يتطلب البناء .NET SDK 10.0.100 أو أحدث ضمن نفس feature band. بناء واختبار Domain وApplication وInfrastructure ممكنان على Linux. بناء WinUI 3 يتطلب Windows وWindows App SDK، ولا يُدّعى نجاحه على Linux.

### أوامر التطوير

```bash
dotnet restore src/GenoCare.Domain/GenoCare.Domain.csproj
dotnet build src/GenoCare.Domain/GenoCare.Domain.csproj
dotnet test tests/GenoCare.Domain.Tests/GenoCare.Domain.Tests.csproj
dotnet test tests/GenoCare.Application.Tests/GenoCare.Application.Tests.csproj
```

بناء `src/GenoCare.Desktop/GenoCare.Desktop.csproj` واختبار الحل الكامل موكلان إلى Windows CI في `.github/workflows/ci.yml`.

المتطلبات التفصيلية ومصدر إجابات العميل في [CLIENT_REQUIREMENTS_SOURCE](docs/CLIENT_REQUIREMENTS_SOURCE.md)، والقرارات التي لم تُحسم في [OPEN_DECISIONS](docs/OPEN_DECISIONS.md).
