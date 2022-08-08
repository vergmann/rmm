namespace Rmm.SqlServer

open Microsoft.Data.SqlClient
open SqlHydra.Query

type DB(connectionString: string) =

    let getConnection () = new SqlConnection(connectionString)

    let openConnection () =
        let conn = getConnection ()
        conn.Open()
        conn

    let openContext () =
        let conn = openConnection ()

        let compiler =
            SqlKata.Compilers.SqlServerCompiler()

        new QueryContext(conn, compiler)

    static member toSql(query: SqlKata.Query) =
        let compiler =
            SqlKata.Compilers.SqlServerCompiler()

        compiler.Compile(query).Sql

    member this.ConnectionString = connectionString
    member this.OpenContext() = openContext ()
