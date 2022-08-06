namespace Rmm

open Microsoft.Data.SqlClient

module DB =

    let connectionString =
        // "Server=KBNDVCRM80\\DEV01; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"
        // "Server=KBNDVSQL110\\DEV01; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"
        "Server=KBNTSSQL38\\TEST02; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"

    let getConnection() =
        new SqlConnection(connectionString)

    let openConnection() =
        let conn = getConnection()
        conn.Open()
        conn

    let toSql (query: SqlKata.Query) =
        let compiler = SqlKata.Compilers.SqlServerCompiler()
        compiler.Compile(query).Sql

