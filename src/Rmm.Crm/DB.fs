namespace Rmm

open Microsoft.Data.SqlClient
open SqlHydra.Query

type DB = DB of string

module DB =
    let connStr input = input |> fun (DB str) -> str

    // let connectionString =
    //     // "Server=KBNDVCRM80\\DEV01; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"
    //     // "Server=KBNDVSQL110\\DEV01; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"
    //     "Server=KBNTSSQL38\\TEST02; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"

    let getConnection db = new SqlConnection(connStr db)

    let openConnection db =
        let conn = getConnection db
        conn.Open()
        conn

    let compiler =
        SqlKata.Compilers.SqlServerCompiler()

    let getContext db =
        let conn = openConnection db
        new QueryContext(conn, compiler)

    let toSql (query: SqlKata.Query) =
        // let compiler = SqlKata.Compilers.SqlServerCompiler()
        compiler.Compile(query).Sql
