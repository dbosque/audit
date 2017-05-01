using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace dBosque.EntityFramework.Audit
{
    /// <summary>
    /// A DbContext instance represents a combination of the Unit Of Work and Repository
    /// patterns such that it can be used to query from a database and group together
    /// changes that will then be written back to the store as a unit. DbContext is conceptually
    /// similar to ObjectContext.
    /// </summary>
    /// <typeparam name="T">The type of the table to store the audit log in.</typeparam>
    public class DbContextAuditor<T> : DbContext, IDisposable where T : IAuditLog
    {
        /// <summary>
        ///  Constructs a new context instance using the given string as the name or connection
        ///  string for the database to which a connection will be made. See the class remarks
        ///  for how this is used to create a connection.
        /// </summary>
        /// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
        public DbContextAuditor(string nameOrConnectionString)
            : base(nameOrConnectionString)
        { }

        /// <summary>
        ///  Constructs a new context instance using the given string as the name or connection
        ///  string for the database to which a connection will be made, and initializes it
        ///  from the given model. See the class remarks for how this is used to create a
        ///  connection.
        /// </summary>
        /// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
        /// <param name="model">The model that will back this context.</param>
        public DbContextAuditor(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model)
        { }

        /// <summary>
        /// Constructs a new context instance using the existing connection to connect to
        /// a database. The connection will not be disposed when the context is disposed
        /// if contextOwnsConnection is false.
        /// </summary>
        /// <param name="existingConnection">An existing connection to use for the new context.</param>
        /// <param name="contextOwnsConnection">If set to true the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.</param>
        public DbContextAuditor(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        { }

        /// <summary>
        /// Constructs a new context instance around an existing ObjectContext.
        /// </summary>
        /// <param name="objectContext">An existing ObjectContext to wrap with the new context.</param>
        /// <param name="dbContextOwnsObjectContext">If set to true the ObjectContext is disposed when the DbContext is disposed, otherwise the caller must dispose the connection.</param>
        public DbContextAuditor(ObjectContext objectContext, bool dbContextOwnsObjectContext)
            : base(objectContext, dbContextOwnsObjectContext)
        { }

        /// <summary>
        /// Constructs a new context instance using the existing connection to connect to
        /// a database, and initializes it from the given model. The connection will not
        /// be disposed when the context is disposed if contextOwnsConnection is false.
        /// </summary>
        /// <param name="existingConnection">An existing connection to use for the new context.</param>
        /// <param name="model">The model that will back this context.</param>
        /// <param name="contextOwnsConnection">If set to true the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.</param>
        public DbContextAuditor(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection)
            : base(existingConnection, model, contextOwnsConnection)
        { }

        /// <summary>
        /// The <seealso cref="AuditSettings"/> to use.
        /// </summary>
        public AuditSettings Settings { get; } = new AuditSettings();      

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are
        /// created for many-to-many relationships and relationships where there is no foreign
        /// key property included in the entity class (often referred to as independent associations).
        /// </returns>
        public override int SaveChanges()
        {
            if (!Settings.IsEnabled)
                return base.SaveChanges();

            return new DbContextAuditorCache<T>(this, Settings)
                .SaveChanges(base.SaveChanges);
        }

        /// <summary>
        ///  Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task result contains
        /// the number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are
        /// created for many-to-many relationships and relationships where there is no foreign
        /// key property included in the entity class (often referred to as independent associations).
        /// </returns>
        public override Task<int> SaveChangesAsync()
        {
            if (!Settings.IsEnabled)
                return base.SaveChangesAsync();

            return new DbContextAuditorCache<T>(this, Settings)
                .SaveChangesAsync(CancellationToken.None, base.SaveChangesAsync);
        }

        /// <summary>
        ///  Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task result contains
        /// the number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are
        /// created for many-to-many relationships and relationships where there is no foreign
        /// key property included in the entity class (often referred to as independent associations).
        /// </returns>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            if (!Settings.IsEnabled)
                return base.SaveChangesAsync(cancellationToken);

            return new DbContextAuditorCache<T>(this, Settings)
                .SaveChangesAsync(cancellationToken, base.SaveChangesAsync);
        }
    }
}
