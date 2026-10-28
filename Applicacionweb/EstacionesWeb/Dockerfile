# Frontend EstacionesWeb (React). Una sola imagen para todos los clientes:
# config.js se genera al arrancar el contenedor a partir de variables de entorno (ver docker/40-config-js.sh).
# Build:  docker build -t facturador-web .   (contexto: Applicacionweb/EstacionesWeb)

FROM node:20-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
# --ignore-scripts: ninguna dependencia del navegador necesita postinstall (evita descargas de @ironsoftware/ironpdf)
RUN npm ci --ignore-scripts --no-audit --no-fund
COPY . .
ENV GENERATE_SOURCEMAP=false
RUN npm run build

FROM nginx:1.27-alpine AS runtime
COPY docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY docker/40-config-js.sh /docker-entrypoint.d/40-config-js.sh
COPY --from=build /app/build /usr/share/nginx/html
# El config.js de desarrollo no se publica: siempre se genera desde variables
RUN rm -f /usr/share/nginx/html/config.js \
    && sed -i 's/\r$//' /docker-entrypoint.d/40-config-js.sh \
    && chmod +x /docker-entrypoint.d/40-config-js.sh \
    && touch /usr/share/nginx/html/config.js \
    && chown nginx:nginx /usr/share/nginx/html/config.js

ENV SERVER_URL="" \
    FEATURE_FLAGS="{}"
EXPOSE 80
HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD wget -q -O /dev/null http://127.0.0.1/ || exit 1
