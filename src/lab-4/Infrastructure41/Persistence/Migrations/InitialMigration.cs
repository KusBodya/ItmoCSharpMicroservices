using FluentMigrator;

namespace Infrastructure41.Persistence.Migrations;

[Migration(202511010001)]
public class InitialMigration : Migration
{
    public override void Up()
    {
        Execute.Sql("""
            CREATE TABLE IF NOT EXISTS products
            (
                product_id    BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                product_name  TEXT  NOT NULL,
                product_price MONEY NOT NULL
            );
            """);

        Execute.Sql("""
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_type WHERE typname = 'order_state'
                ) THEN
                    CREATE TYPE order_state AS ENUM ('created', 'processing', 'completed', 'cancelled');
                END IF;
            END
            $$ LANGUAGE plpgsql;
            """);

        Execute.Sql("""
            CREATE TABLE IF NOT EXISTS orders
            (
                order_id         BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                order_state      order_state              NOT NULL,
                order_created_at TIMESTAMP WITH TIME ZONE NOT NULL,
                order_created_by TEXT                     NOT NULL
            );
            """);

        Execute.Sql("""
            CREATE TABLE IF NOT EXISTS order_items
            (
                order_item_id       BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                order_id            BIGINT  NOT NULL REFERENCES orders (order_id),
                product_id          BIGINT  NOT NULL REFERENCES products (product_id),
                order_item_quantity INT     NOT NULL,
                order_item_deleted  BOOLEAN NOT NULL
            );
            """);

        Execute.Sql("""
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_type WHERE typname = 'order_history_item_kind'
                ) THEN
                    CREATE TYPE order_history_item_kind AS ENUM ('created', 'item_added', 'item_removed', 'state_changed');
                END IF;
            END
            $$ LANGUAGE plpgsql;
            """);

        Execute.Sql("""
            CREATE TABLE IF NOT EXISTS order_history
            (
                order_history_item_id         BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                order_id                      BIGINT                   NOT NULL REFERENCES orders (order_id),
                order_history_item_created_at TIMESTAMP WITH TIME ZONE NOT NULL,
                order_history_item_kind       order_history_item_kind  NOT NULL,
                order_history_item_payload    JSONB                    NOT NULL
            );
            """);
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE IF EXISTS order_history;");
        Execute.Sql("DROP TYPE IF EXISTS order_history_item_kind;");
        Execute.Sql("DROP TABLE IF EXISTS order_items;");
        Execute.Sql("DROP TABLE IF EXISTS orders;");
        Execute.Sql("DROP TYPE IF EXISTS order_state;");
        Execute.Sql("DROP TABLE IF EXISTS products;");
    }
}
