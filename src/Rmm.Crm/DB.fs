namespace Rmm

open Microsoft.Data.SqlClient
open SqlHydra.Query

type DB(connectionString: string) =
    let getConnection = new SqlConnection(connectionString)

    let openConnection =
        let conn = getConnection 
        conn.Open()
        conn

    let compiler =
        SqlKata.Compilers.SqlServerCompiler()

    let getContext =
        let conn = openConnection
        new QueryContext(conn, compiler)

    member this.toSql(query: SqlKata.Query) =
        // let compiler = SqlKata.Compilers.SqlServerCompiler()
        compiler.Compile(query).Sql

    member this.ConnectionString = connectionString
    member this.GetContext() = getContext



// module DB =
// let connStr input = input |> fun (DB str) -> str

// let connectionString =
//     // "Server=KBNDVCRM80\\DEV01; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"
//     // "Server=KBNDVSQL110\\DEV01; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"
//     "Server=KBNTSSQL38\\TEST02; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"

// let getConnection (db:DB) = new SqlConnection(db.ConnectionString)
//
// let openConnection db =
//     let conn = getConnection db
//     conn.Open()
//     conn
//
// let compiler =
//     SqlKata.Compilers.SqlServerCompiler()
//
// let getContext db =
//     let conn = openConnection db
//     new QueryContext(conn, compiler)
//
// let toSql (query: SqlKata.Query) =
//     // let compiler = SqlKata.Compilers.SqlServerCompiler()
//     compiler.Compile(query).Sql
