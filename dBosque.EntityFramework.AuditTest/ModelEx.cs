using dBosque.EntityFramework.Audit;
using dBosque.EntityFramework.Audit.Attributes;

namespace AuditTest
{
    public partial class AuditLog : IAuditLog
    {

    }

    [Auditable]
    public partial class company
    { }
}
