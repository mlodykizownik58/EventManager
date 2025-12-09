using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagement.Migrations
{
    public partial class FixEventSignupPrimaryKey : Migration
    {
        // Pomocnicza metoda do tworzenia tabeli, jeśli nie istnieje
        private static void CreateTableIfNotExists(MigrationBuilder migrationBuilder, string tableName, string createTableSql)
        {
            var checkTableExistsSql = $@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}')
                BEGIN
                    {createTableSql}
                END";

            migrationBuilder.Sql(checkTableExistsSql);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sprawdzamy, czy tabela AspNetRoles już istnieje
            CreateTableIfNotExists(migrationBuilder, "AspNetRoles", @"
                CREATE TABLE AspNetRoles (
                    Id NVARCHAR(450) PRIMARY KEY,
                    Name NVARCHAR(256) NULL,
                    NormalizedName NVARCHAR(256) NULL,
                    ConcurrencyStamp NVARCHAR(256) NULL
                )
            ");

            // Sprawdzamy, czy tabela AspNetUsers już istnieje
            CreateTableIfNotExists(migrationBuilder, "AspNetUsers", @"
                CREATE TABLE AspNetUsers (
                    Id NVARCHAR(450) PRIMARY KEY,
                    UserName NVARCHAR(256) NULL,
                    NormalizedUserName NVARCHAR(256) NULL,
                    Email NVARCHAR(256) NULL,
                    NormalizedEmail NVARCHAR(256) NULL,
                    EmailConfirmed BIT NOT NULL,
                    PasswordHash NVARCHAR(256) NULL,
                    SecurityStamp NVARCHAR(256) NULL,
                    ConcurrencyStamp NVARCHAR(256) NULL,
                    PhoneNumber NVARCHAR(15) NULL,  -- Zmiana z MAX na 15
                    PhoneNumberConfirmed BIT NOT NULL,
                    TwoFactorEnabled BIT NOT NULL,
                    LockoutEnd DATETIMEOFFSET NULL,
                    LockoutEnabled BIT NOT NULL,
                    AccessFailedCount INT NOT NULL
                )
            ");

            // Tworzymy tabelę AspNetRoleClaims, jeśli nie istnieje
            CreateTableIfNotExists(migrationBuilder, "AspNetRoleClaims", @"
                CREATE TABLE AspNetRoleClaims (
                    Id INT NOT NULL IDENTITY(1,1),
                    RoleId NVARCHAR(450) NOT NULL,
                    ClaimType NVARCHAR(256) NULL,   -- Zmiana na NVARCHAR(256)
                    ClaimValue NVARCHAR(256) NULL,  -- Zmiana na NVARCHAR(256)
                    PRIMARY KEY (Id),
                    CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId FOREIGN KEY (RoleId) 
                        REFERENCES AspNetRoles (Id) ON DELETE CASCADE
                )
            ");

            // Tworzymy tabelę AspNetUserClaims, jeśli nie istnieje
            CreateTableIfNotExists(migrationBuilder, "AspNetUserClaims", @"
                CREATE TABLE AspNetUserClaims (
                    Id INT NOT NULL IDENTITY(1,1),
                    UserId NVARCHAR(450) NOT NULL,
                    ClaimType NVARCHAR(256) NULL,   -- Zmiana na NVARCHAR(256)
                    ClaimValue NVARCHAR(256) NULL,  -- Zmiana na NVARCHAR(256)
                    PRIMARY KEY (Id),
                    CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId FOREIGN KEY (UserId) 
                        REFERENCES AspNetUsers (Id) ON DELETE CASCADE
                )
            ");

            // Tworzymy tabelę AspNetUserLogins, jeśli nie istnieje
            CreateTableIfNotExists(migrationBuilder, "AspNetUserLogins", @"
                CREATE TABLE AspNetUserLogins (
                    LoginProvider NVARCHAR(128) NOT NULL,
                    ProviderKey NVARCHAR(128) NOT NULL,
                    ProviderDisplayName NVARCHAR(256) NULL,  -- Zmiana na NVARCHAR(256)
                    UserId NVARCHAR(450) NOT NULL,
                    PRIMARY KEY (LoginProvider, ProviderKey),
                    CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId FOREIGN KEY (UserId) 
                        REFERENCES AspNetUsers (Id) ON DELETE CASCADE
                )
            ");

            // Tworzymy tabelę AspNetUserRoles, jeśli nie istnieje
            CreateTableIfNotExists(migrationBuilder, "AspNetUserRoles", @"
                CREATE TABLE AspNetUserRoles (
                    UserId NVARCHAR(450) NOT NULL,
                    RoleId NVARCHAR(450) NOT NULL,
                    PRIMARY KEY (UserId, RoleId),
                    CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId FOREIGN KEY (RoleId) 
                        REFERENCES AspNetRoles (Id) ON DELETE CASCADE,
                    CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId FOREIGN KEY (UserId) 
                        REFERENCES AspNetUsers (Id) ON DELETE CASCADE
                )
            ");

            // Tworzymy tabelę AspNetUserTokens, jeśli nie istnieje
            CreateTableIfNotExists(migrationBuilder, "AspNetUserTokens", @"
                CREATE TABLE AspNetUserTokens (
                    UserId NVARCHAR(450) NOT NULL,
                    LoginProvider NVARCHAR(128) NOT NULL,
                    Name NVARCHAR(128) NOT NULL,
                    Value NVARCHAR(256) NULL,  -- Zmiana na NVARCHAR(256)
                    PRIMARY KEY (UserId, LoginProvider, Name),
                    CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId FOREIGN KEY (UserId) 
                        REFERENCES AspNetUsers (Id) ON DELETE CASCADE
                )
            ");

            // Tworzymy tabelę Events, jeśli nie istnieje
            CreateTableIfNotExists(migrationBuilder, "Events", @"
                CREATE TABLE Events (
                    Id INT NOT NULL IDENTITY(1,1),
                    Name NVARCHAR(256) NOT NULL,  -- Zmiana na NVARCHAR(256)
                    Description NVARCHAR(1000) NOT NULL,  -- Zmiana na NVARCHAR(1000)
                    Date DATETIME2 NOT NULL,
                    Location NVARCHAR(500) NOT NULL,  -- Zmiana na NVARCHAR(500)
                    OrganizerId NVARCHAR(450) NOT NULL,
                    PRIMARY KEY (Id),
                    CONSTRAINT FK_Events_AspNetUsers_OrganizerId FOREIGN KEY (OrganizerId) 
                        REFERENCES AspNetUsers (Id) ON DELETE CASCADE
                )
            ");

            // Tworzymy tabelę EventSignups, jeśli nie istnieje
            CreateTableIfNotExists(migrationBuilder, "EventSignups", @"
                CREATE TABLE EventSignups (
                    EventId INT NOT NULL,
                    UserId NVARCHAR(450) NOT NULL,
                    PRIMARY KEY (EventId, UserId),
                    CONSTRAINT FK_EventSignups_AspNetUsers_UserId FOREIGN KEY (UserId) 
                        REFERENCES AspNetUsers (Id) ON DELETE CASCADE,
                    CONSTRAINT FK_EventSignups_Events_EventId FOREIGN KEY (EventId) 
                        REFERENCES Events (Id) ON DELETE NO ACTION -- zmiana z CASCADE na NO ACTION
                )
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AspNetRoleClaims");
            migrationBuilder.DropTable(name: "AspNetUserClaims");
            migrationBuilder.DropTable(name: "AspNetUserLogins");
            migrationBuilder.DropTable(name: "AspNetUserRoles");
            migrationBuilder.DropTable(name: "AspNetUserTokens");
            migrationBuilder.DropTable(name: "EventSignups");
            migrationBuilder.DropTable(name: "AspNetRoles");
            migrationBuilder.DropTable(name: "Events");
            migrationBuilder.DropTable(name: "AspNetUsers");
        }
    }
}
