namespace Rmm.Management.SqlServer

open System
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

    static member dateTimeZero = DateTime(1900,01,01,00,00,00,000) 

    static member toSql(query: SqlKata.Query) =
        let compiler =
            SqlKata.Compilers.SqlServerCompiler()

        compiler.Compile(query).Sql

    member this.ConnectionString = connectionString
    member this.OpenConnection = openConnection
    member this.OpenContext() = openContext ()

module DB =
    let builder (dataSource: string) (database: string) = 
        let bldr = SqlConnectionStringBuilder()
        bldr.IntegratedSecurity <- true
        bldr.TrustServerCertificate <- true
        bldr.ConnectTimeout <- 1000
        bldr.ConnectRetryCount <- 3
        bldr.ConnectRetryInterval <- 30
        bldr.ApplicationName <- "Rmm.Management"
        bldr.DataSource <- dataSource
        bldr.InitialCatalog <- database
        bldr

    let connectionString (builder: SqlConnectionStringBuilder) = builder.ConnectionString
    let bldrDv80 = "KBNDVCRM80\\DEV01" |> builder
    let bldrDevTest = "KBNDVSQL110\\DEV01" |> builder
    let bldrTest = "KBNTSSQL38\\TEST02" |> builder

    let dv80Rmm = "RMM_MSCRM" |> bldrDv80 |> connectionString |> DB
    let dv80Config = "MSCRM_CONFIG" |> bldrDv80 |> connectionString |> DB
    let devTestRmm = "RMM_MSCRM" |> bldrDevTest |> connectionString |> DB
    let testRmm = "RMM_MSCRM" |> bldrTest |> connectionString |> DB
