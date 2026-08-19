# Multi-stage Dockerfile for BCSMS React Frontend
# Stage 1: Build production bundle
FROM node:20-alpine AS build
WORKDIR /app

# Install dependencies using package-lock for reproducible builds
COPY frontend/web/package.json frontend/web/package-lock.json ./
RUN npm ci

# Copy source code and build production assets
COPY frontend/web/ ./
RUN npm run build

# Stage 2: Serve static bundle via Nginx
FROM nginx:1.27-alpine AS runtime

# Copy built static assets to Nginx html directory
COPY --from=build /app/dist /usr/share/nginx/html

# Copy custom Nginx configuration with SPA fallback and API reverse proxy
COPY docker/frontend/nginx.conf /etc/nginx/conf.d/default.conf

# Expose HTTP port
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
