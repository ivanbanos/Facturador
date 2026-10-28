# Herramienta de esquema: aplica deploy/db/schema.dacpac a la base del cliente (sqlpackage Publish).
# También se usa para extraer el esquema de una base existente (scripts/extraer-esquema.sh).
# Build:  docker build -t facturador-dbmigrate deploy/db

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS tools
RUN dotnet tool install --tool-path /tools microsoft.sqlpackage --version 162.3.566

FROM mcr.microsoft.com/dotnet/runtime:8.0
COPY --from=tools /tools /tools
COPY migrar.sh /usr/local/bin/migrar.sh
RUN sed -i 's/\r$//' /usr/local/bin/migrar.sh && chmod +x /usr/local/bin/migrar.sh
ENV PATH="/tools:${PATH}"
USER app
ENTRYPOINT ["/usr/local/bin/migrar.sh"]
